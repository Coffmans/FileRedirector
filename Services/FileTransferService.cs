using FileRedirector.Models;
using FileRedirector.Wildcards;
using FluentFTP;
using System.Net;

namespace FileRedirector.Services;

/// <summary>
/// Handles reading file lists from sources and writing files to destinations
/// regardless of protocol (local/UNC, HTTP/S, FTP/S).
/// </summary>
public static class FileTransferService
{
    // Shared per credential key — prevents socket exhaustion from per-call HttpClient creation.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, HttpClient> _httpClients = new();

    private const int CopyBufferSize = 81920;

    private static HttpClient GetHttpClient(string? username, string? password)
    {
        var key = $"{username ?? ""}:{password ?? ""}";
        return _httpClients.GetOrAdd(key, _ =>
        {
            var handler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(username))
                handler.Credentials = new NetworkCredential(username, password);
            // Default 100s is too short for large uploads now that bodies are streamed
            return new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(30) };
        });
    }

    // ─── Source: list files ──────────────────────────────────────────────────

    public static async Task<List<DiscoveredFile>> ListSourceFilesAsync(JobSource source)
    {
        var resolvedPath = WildcardEngine.Resolve(source.Path);
        var pattern      = WildcardEngine.BuildPatternRegex(source.FilePattern);

        return source.PathType switch
        {
            PathType.LocalOrUNC => ListLocalFiles(resolvedPath, pattern),
            PathType.FTP or PathType.FTPS => await ListFtpFilesAsync(source, resolvedPath, pattern),
            _ => throw new NotSupportedException(
                     $"{source.PathType} can't be used as a source (no directory listing). Use it as a destination instead.")
        };
    }

    private static List<DiscoveredFile> ListLocalFiles(string path, System.Text.RegularExpressions.Regex pattern)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Source folder not found: {path}");

        return [.. Directory.EnumerateFiles(path)
            .Where(f => pattern.IsMatch(Path.GetFileName(f)))
            .Select(f =>
            {
                var info = new FileInfo(f);
                return new DiscoveredFile
                {
                    FullPath     = f,
                    FileName     = info.Name,
                    SizeBytes    = info.Length,
                    LastModified = info.LastWriteTimeUtc
                };
            })];
    }

    private static async Task<List<DiscoveredFile>> ListFtpFilesAsync(
        JobSource source, string path, System.Text.RegularExpressions.Regex pattern)
    {
        var uri = ToFtpUri(path);
        await using var client = BuildFtpClient(uri, source);
        await client.Connect();

        var listing = await client.GetListing(RemotePath(uri));
        await client.Disconnect();

        return [.. listing
            .Where(i => i.Type == FtpObjectType.File && pattern.IsMatch(i.Name))
            .Select(i => new DiscoveredFile
            {
                // Build from the server-reported full path; stored escaped so it round-trips via RemotePath()
                FullPath     = new UriBuilder(uri) { Path = i.FullName }.Uri.AbsoluteUri,
                FileName     = i.Name,
                SizeBytes    = i.Size,
                LastModified = i.Modified
            })];
    }

    // ─── Source: stage file for copying ──────────────────────────────────────

    /// <summary>
    /// A readable copy of a source file. Local/UNC sources are read in place; remote sources are
    /// downloaded once to a temp file so every destination can stream from it without holding
    /// the whole file in memory.
    /// </summary>
    public sealed class StagedFile(string localPath, bool isTemp) : IAsyncDisposable
    {
        public string LocalPath { get; } = localPath;

        public Stream OpenRead() => new FileStream(LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                                   CopyBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

        public ValueTask DisposeAsync()
        {
            if (isTemp) TryDeleteLocal(LocalPath);
            return ValueTask.CompletedTask;
        }
    }

    public static async Task<StagedFile> StageSourceAsync(DiscoveredFile file, JobSource source)
    {
        if (source.PathType == PathType.LocalOrUNC)
            return new StagedFile(file.FullPath, isTemp: false);

        var tempDir = Path.Combine(Path.GetTempPath(), "FileRedirector");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, $"{Guid.NewGuid():N}.tmp");
        try
        {
            switch (source.PathType)
            {
                case PathType.FTP:
                case PathType.FTPS:
                    await DownloadFtpFileAsync(file, source, tempPath);
                    break;
                case PathType.HTTP:
                case PathType.HTTPS:
                    await DownloadHttpFileAsync(file, source, tempPath);
                    break;
                default:
                    throw new NotSupportedException($"PathType {source.PathType} not supported for read.");
            }
            return new StagedFile(tempPath, isTemp: true);
        }
        catch
        {
            TryDeleteLocal(tempPath);
            throw;
        }
    }

    private static async Task DownloadFtpFileAsync(DiscoveredFile file, JobSource source, string localPath)
    {
        var uri = new Uri(file.FullPath);
        await using var client = BuildFtpClient(uri, source);
        await client.Connect();
        var status = await client.DownloadFile(localPath, RemotePath(uri), FtpLocalExists.Overwrite);
        await client.Disconnect();
        if (status != FtpStatus.Success)
            throw new IOException($"FTP download of '{RemotePath(uri)}' failed ({status}).");
    }

    private static async Task DownloadHttpFileAsync(DiscoveredFile file, JobSource source, string localPath)
    {
        var http = GetHttpClient(source.Username, source.Password);
        using var response = await http.GetAsync(file.FullPath, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var input  = await response.Content.ReadAsStreamAsync();
        await using var output = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None,
                                                CopyBufferSize, FileOptions.Asynchronous);
        await input.CopyToAsync(output);
    }

    // ─── Destination: write file ─────────────────────────────────────────────

    public static async Task WriteToDestinationAsync(
        StagedFile source, string originalFileName, JobDestination dest, DateTime referenceTime
    )
    {
        // Resolve destination path and optional filename template
        var resolvedDir      = WildcardEngine.Resolve(dest.Path, referenceTime, originalFileName);
        var outputFileName   = string.IsNullOrWhiteSpace(dest.FileNameTemplate)
                                ? originalFileName
                                : WildcardEngine.Resolve(dest.FileNameTemplate, referenceTime, originalFileName);

        switch (dest.PathType)
        {
            case PathType.LocalOrUNC:
                await WriteLocalFileAsync(source, resolvedDir, outputFileName);
                break;
            case PathType.FTP:
            case PathType.FTPS:
                await WriteFtpFileAsync(source, dest, resolvedDir, outputFileName);
                break;
            case PathType.HTTP:
            case PathType.HTTPS:
                await WriteHttpFileAsync(source, dest, resolvedDir, outputFileName);
                break;
        }
    }

    // Local and FTP writes go to a temporary ".partial" name first and are renamed into place
    // once complete, so anything watching the destination never sees a half-written file.
    private static string PartialName(string fileName) => $"{fileName}.{Guid.NewGuid().ToString("N")[..8]}.partial";

    private static async Task WriteLocalFileAsync(StagedFile source, string dir, string fileName)
    {
        Directory.CreateDirectory(dir);
        var finalPath = Path.Combine(dir, fileName);
        var tempPath  = Path.Combine(dir, PartialName(fileName));
        try
        {
            await using (var input  = source.OpenRead())
            await using (var output = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                                                     CopyBufferSize, FileOptions.Asynchronous))
            {
                await input.CopyToAsync(output);
            }
            File.Move(tempPath, finalPath, overwrite: true);
        }
        catch
        {
            TryDeleteLocal(tempPath);
            throw;
        }
    }

    private static async Task WriteFtpFileAsync(
        StagedFile source, JobDestination dest, string resolvedPath, string fileName)
    {
        var uri = ToFtpUri(resolvedPath);
        await using var client = BuildFtpClientDest(uri, dest);
        await client.Connect();

        var remoteDir = RemotePath(uri).TrimEnd('/');
        var finalPath = $"{remoteDir}/{fileName}";
        var tempPath  = $"{remoteDir}/{PartialName(fileName)}";

        if (remoteDir.Length > 0)
            await client.CreateDirectory(remoteDir, true);

        await using (var input = source.OpenRead())
        {
            var status = await client.UploadStream(input, tempPath, FtpRemoteExists.Overwrite, true);
            if (status != FtpStatus.Success)
                throw new IOException($"FTP upload to '{tempPath}' failed ({status}).");
        }

        try
        {
            await MoveFtpFileOrThrow(client, tempPath, finalPath);
        }
        catch
        {
            try { await client.DeleteFile(tempPath); } catch { /* best effort */ }
            throw;
        }
        await client.Disconnect();
    }

    private static async Task WriteHttpFileAsync(StagedFile source, JobDestination dest, string url, string fileName)
    {
        var http = GetHttpClient(dest.Username, dest.Password);

        var baseUri = url.EndsWith(Path.AltDirectorySeparatorChar)
            ? new Uri(url)
            : new Uri(url + Path.AltDirectorySeparatorChar);

        // Escape so names containing '#', '?' or '%' aren't misread as URL syntax
        var target = new Uri(baseUri, Uri.EscapeDataString(fileName));

        await using var input = source.OpenRead();
        using var content  = new StreamContent(input, CopyBufferSize);
        using var response = await http.PutAsync(target, content);
        response.EnsureSuccessStatusCode();
    }

    // ─── Source: post-copy actions ────────────────────────────────────────────

    public static async Task ApplySourceActionAsync(
        DiscoveredFile file, JobSource source,
        SourceFileAction action, string? movePath, string? processedSuffix)
    {
        switch (action)
        {
            case SourceFileAction.Delete:
                await DeleteSourceFileAsync(file, source);
                break;

            case SourceFileAction.Move:
                if (string.IsNullOrWhiteSpace(movePath))
                    throw new InvalidOperationException("Source action is Move but no 'Move to' path is configured.");
                var resolvedMove = WildcardEngine.Resolve(movePath, DateTime.Now, file.FileName);
                await MoveSourceFileAsync(file, source, resolvedMove);
                break;

            case SourceFileAction.MarkProcessed:
                var suffix = processedSuffix ?? ".done";
                await RenameSourceFileAsync(file, source, file.FileName + suffix);
                break;

            case SourceFileAction.Leave:
            default:
                break; // nothing — DB tracking prevents reprocessing
        }
    }

    private static async Task DeleteSourceFileAsync(DiscoveredFile file, JobSource source)
    {
        switch (source.PathType)
        {
            case PathType.LocalOrUNC:
                File.Delete(file.FullPath);
                break;
            case PathType.FTP:
            case PathType.FTPS:
                await WithFtpSourceAsync(file, source, (client, remote) => client.DeleteFile(remote));
                break;
            default:
                throw new NotSupportedException($"Delete is not supported for {source.PathType} sources.");
        }
    }

    // Move and MarkProcessed never overwrite: if the target name is taken (e.g. an earlier
    // file with the same name was already archived), a " (1)", " (2)"… suffix is added instead.

    /// <param name="destDir">Local/UNC folder for local sources; for FTP sources, a folder on the same
    /// server (either a remote path like "/archive/@YYYY" or a full ftp:// URL).</param>
    private static async Task MoveSourceFileAsync(DiscoveredFile file, JobSource source, string destDir)
    {
        switch (source.PathType)
        {
            case PathType.LocalOrUNC:
                Directory.CreateDirectory(destDir);
                File.Move(file.FullPath, UniqueLocalPath(destDir, file.FileName));
                break;
            case PathType.FTP:
            case PathType.FTPS:
                var remoteDir = destDir.Contains("://") ? RemotePath(new Uri(destDir)) : destDir.Replace('\\', '/');
                remoteDir = remoteDir.TrimEnd('/');
                await WithFtpSourceAsync(file, source, async (client, remote) =>
                {
                    if (remoteDir.Length > 0)
                        await client.CreateDirectory(remoteDir, true);
                    await MoveFtpFileOrThrow(client, remote, await UniqueFtpPath(client, remoteDir, file.FileName));
                });
                break;
            default:
                throw new NotSupportedException($"Move is not supported for {source.PathType} sources.");
        }
    }

    private static async Task RenameSourceFileAsync(DiscoveredFile file, JobSource source, string newName)
    {
        switch (source.PathType)
        {
            case PathType.LocalOrUNC:
                var dir = Path.GetDirectoryName(file.FullPath)!;
                File.Move(file.FullPath, UniqueLocalPath(dir, newName));
                break;
            case PathType.FTP:
            case PathType.FTPS:
                await WithFtpSourceAsync(file, source, async (client, remote) =>
                {
                    var remoteDir = remote[..remote.LastIndexOf('/')];
                    await MoveFtpFileOrThrow(client, remote, await UniqueFtpPath(client, remoteDir, newName));
                });
                break;
            default:
                throw new NotSupportedException($"Rename is not supported for {source.PathType} sources.");
        }
    }

    private static async Task WithFtpSourceAsync(DiscoveredFile file, JobSource source, Func<AsyncFtpClient, string, Task> action)
    {
        var uri = new Uri(file.FullPath);
        await using var client = BuildFtpClient(uri, source);
        await client.Connect();
        await action(client, RemotePath(uri));
        await client.Disconnect();
    }

    private static async Task MoveFtpFileOrThrow(AsyncFtpClient client, string from, string to)
    {
        if (!await client.MoveFile(from, to, FtpRemoteExists.Overwrite))
            throw new IOException($"FTP server refused to move '{from}' to '{to}'.");
    }

    /// <summary>"name.ext" for attempt 0, then "name (1).ext", "name (2).ext", …</summary>
    private static string NumberedName(string fileName, int attempt)
        => attempt == 0
            ? fileName
            : $"{Path.GetFileNameWithoutExtension(fileName)} ({attempt}){Path.GetExtension(fileName)}";

    private static string UniqueLocalPath(string dir, string fileName)
    {
        for (int i = 0; ; i++)
        {
            var candidate = Path.Combine(dir, NumberedName(fileName, i));
            if (!File.Exists(candidate)) return candidate;
        }
    }

    private static async Task<string> UniqueFtpPath(AsyncFtpClient client, string remoteDir, string fileName)
    {
        for (int i = 0; ; i++)
        {
            var candidate = $"{remoteDir}/{NumberedName(fileName, i)}";
            if (!await client.FileExists(candidate)) return candidate;
        }
    }

    private static void TryDeleteLocal(string path)
    {
        try { File.Delete(path); } catch { /* best effort */ }
    }

    /// <summary>
    /// True if a local file is currently open for writing by another process
    /// (i.e. it can't be opened for reading while denying other writers).
    /// </summary>
    public static bool IsLocalFileLocked(string path)
    {
        try
        {
            using var _ = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false; // not a lock — let the copy attempt surface the real error
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Accepts "ftp://host/dir", "ftps://host/dir" or a bare "host/dir".</summary>
    private static Uri ToFtpUri(string path)
        => new(path.Contains("://") ? path : $"ftp://{path}");

    /// <summary>Unescaped server-side path of an ftp:// URI (e.g. "%20" → " ").</summary>
    private static string RemotePath(Uri uri) => Uri.UnescapeDataString(uri.AbsolutePath);

    private static AsyncFtpClient BuildFtpClient(Uri uri, JobSource source)
        => BuildFtpClient(uri, source.PathType, source.Username, source.Password,
                          source.IsPassive, source.AcceptAnyCertificate);

    private static AsyncFtpClient BuildFtpClientDest(Uri uri, JobDestination dest)
        => BuildFtpClient(uri, dest.PathType, dest.Username, dest.Password,
                          dest.IsPassive, dest.AcceptAnyCertificate);

    private static AsyncFtpClient BuildFtpClient(
        Uri uri, PathType type, string? username, string? password, bool passive, bool acceptAnyCertificate)
    {
        var client = new AsyncFtpClient(uri.Host, username ?? "", password ?? "", uri.Port > 0 ? uri.Port : 21);
        client.Config.EncryptionMode     = type == PathType.FTPS ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
        client.Config.DataConnectionType = passive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;

        // FTPS: the server certificate must be valid and trusted by Windows, unless this
        // source/destination explicitly opts out (e.g. a self-signed certificate on a trusted network).
        client.Config.ValidateAnyCertificate = false;
        client.ValidateCertificate += (_, e) =>
            e.Accept = acceptAnyCertificate || e.PolicyErrors == System.Net.Security.SslPolicyErrors.None;
        return client;
    }
}
