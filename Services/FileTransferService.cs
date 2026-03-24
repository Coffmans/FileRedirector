using FileRedirector.Models;
using FileRedirector.Wildcards;
using FluentFTP;
using System.Net;

namespace FileRedirector.Services;

/// <summary>
/// Handles reading file lists from sources and writing files to destinations
/// regardless of protocol (local/UNC, HTTP/S, FTP/S).
/// </summary>
public class FileTransferService
{
    // ─── Source: list files ──────────────────────────────────────────────────

    public async Task<List<DiscoveredFile>> ListSourceFilesAsync(JobSource source)
    {
        var resolvedPath = WildcardEngine.Resolve(source.Path);
        var pattern      = WildcardEngine.BuildPatternRegex(source.FilePattern);

        return source.PathType switch
        {
            PathType.LocalOrUNC => ListLocalFiles(resolvedPath, pattern),
            PathType.FTP or PathType.FTPS => await ListFtpFilesAsync(source, resolvedPath, pattern),
            PathType.HTTP or PathType.HTTPS => [], // HTTP sources listed via manifest – see notes
            _ => []
        };
    }

    private static List<DiscoveredFile> ListLocalFiles(string path, System.Text.RegularExpressions.Regex pattern)
    {
        if (!Directory.Exists(path)) return [];

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

    private async Task<List<DiscoveredFile>> ListFtpFilesAsync(
        JobSource source, string path, System.Text.RegularExpressions.Regex pattern)
    {
        var uri    = new Uri(path);
        var client = BuildFtpClient(uri, source);
        await client.Connect();

        var listing = await client.GetListing(uri.AbsolutePath);
        await client.Disconnect();

        return [.. listing
            .Where(i => i.Type == FtpObjectType.File && pattern.IsMatch(i.Name))
            .Select(i => new DiscoveredFile
            {
                FullPath     = path.TrimEnd('/') + "/" + i.Name,
                FileName     = i.Name,
                SizeBytes    = i.Size,
                LastModified = i.Modified
            })];
    }

    // ─── Source: read file bytes ─────────────────────────────────────────────

    public async Task<byte[]> ReadSourceFileAsync(DiscoveredFile file, JobSource source)
    {
        return source.PathType switch
        {
            PathType.LocalOrUNC => await File.ReadAllBytesAsync(file.FullPath),
            PathType.FTP or PathType.FTPS => await ReadFtpFileAsync(file, source),
            PathType.HTTP or PathType.HTTPS => await ReadHttpFileAsync(file, source),
            _ => throw new NotSupportedException($"PathType {source.PathType} not supported for read.")
        };
    }

    private async Task<byte[]> ReadFtpFileAsync(DiscoveredFile file, JobSource source)
    {
        var uri    = new Uri(file.FullPath);
        var client = BuildFtpClient(uri, source);
        await client.Connect();
        using var ms = new MemoryStream();
        await client.DownloadStream(ms, uri.AbsolutePath);
        await client.Disconnect();
        return ms.ToArray();
    }

    private async Task<byte[]> ReadHttpFileAsync(DiscoveredFile file, JobSource source)
    {
        using var handler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(source.Username))
            handler.Credentials = new NetworkCredential(source.Username, source.Password);

        using var http = new HttpClient(handler);
        return await http.GetByteArrayAsync(file.FullPath);
    }

    // ─── Destination: write file ─────────────────────────────────────────────

    public async Task WriteToDestinationAsync(
        byte[] data, string originalFileName, JobDestination dest, DateTime referenceTime)
    {
        // Resolve destination path and optional filename template
        var resolvedDir      = WildcardEngine.Resolve(dest.Path, referenceTime, originalFileName);
        var outputFileName   = string.IsNullOrWhiteSpace(dest.FileNameTemplate)
                                ? originalFileName
                                : WildcardEngine.Resolve(dest.FileNameTemplate, referenceTime, originalFileName);

        switch (dest.PathType)
        {
            case PathType.LocalOrUNC:
                await WriteLocalFileAsync(data, resolvedDir, outputFileName);
                break;
            case PathType.FTP:
            case PathType.FTPS:
                await WriteFtpFileAsync(data, dest, resolvedDir, outputFileName, referenceTime);
                break;
            case PathType.HTTP:
            case PathType.HTTPS:
                await WriteHttpFileAsync(data, dest, resolvedDir, outputFileName);
                break;
        }
    }

    private async Task WriteLocalFileAsync(byte[] data, string dir, string fileName)
    {
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, fileName);
        await File.WriteAllBytesAsync(fullPath, data);
    }

    private async Task WriteFtpFileAsync(
        byte[] data, JobDestination dest, string resolvedPath, string fileName, DateTime refTime)
    {
        var uri    = new Uri(resolvedPath.StartsWith("ftp") ? resolvedPath : $"ftp://{resolvedPath}");
        var client = BuildFtpClientDest(uri, dest);
        await client.Connect();

        var remotePath = uri.AbsolutePath.TrimEnd('/') + "/" + fileName;
        // Ensure remote directory exists
        await client.CreateDirectory(uri.AbsolutePath, true);
        using var ms = new MemoryStream(data);
        await client.UploadStream(ms, remotePath, FtpRemoteExists.Overwrite, true);
        await client.Disconnect();
    }

    private async Task WriteHttpFileAsync(byte[] data, JobDestination dest, string url, string fileName)
    {
        using var handler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(dest.Username))
            handler.Credentials = new NetworkCredential(dest.Username, dest.Password);

        using var http    = new HttpClient(handler);
        using var content = new ByteArrayContent(data);
        var target        = url.TrimEnd('/') + "/" + fileName;
        var response      = await http.PutAsync(target, content);
        response.EnsureSuccessStatusCode();
    }

    // ─── Source: post-copy actions ────────────────────────────────────────────

    public async Task ApplySourceActionAsync(
        DiscoveredFile file, JobSource source,
        SourceFileAction action, string? movePath, string? processedSuffix)
    {
        switch (action)
        {
            case SourceFileAction.Delete:
                await DeleteSourceFileAsync(file, source);
                break;

            case SourceFileAction.Move:
                if (!string.IsNullOrEmpty(movePath))
                {
                    var resolvedMove = WildcardEngine.Resolve(movePath, DateTime.Now, file.FileName);
                    await MoveSourceFileAsync(file, source, resolvedMove);
                }
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

    private Task DeleteSourceFileAsync(DiscoveredFile file, JobSource source)
    {
        if (source.PathType == PathType.LocalOrUNC)
            File.Delete(file.FullPath);
        // FTP/HTTP deletion left as extension point
        return Task.CompletedTask;
    }

    private Task MoveSourceFileAsync(DiscoveredFile file, JobSource source, string destDir)
    {
        if (source.PathType == PathType.LocalOrUNC)
        {
            Directory.CreateDirectory(destDir);
            File.Move(file.FullPath, Path.Combine(destDir, file.FileName), overwrite: true);
        }
        return Task.CompletedTask;
    }

    private Task RenameSourceFileAsync(DiscoveredFile file, JobSource source, string newName)
    {
        if (source.PathType == PathType.LocalOrUNC)
        {
            var dir = Path.GetDirectoryName(file.FullPath)!;
            File.Move(file.FullPath, Path.Combine(dir, newName), overwrite: true);
        }
        return Task.CompletedTask;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static AsyncFtpClient BuildFtpClient(Uri uri, JobSource source)
    {
        var client = new AsyncFtpClient(uri.Host, source.Username ?? "", source.Password ?? "", uri.Port > 0 ? uri.Port : 21);
        client.Config.EncryptionMode   = source.PathType == PathType.FTPS ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
        client.Config.DataConnectionType = source.IsPassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
        client.Config.ValidateAnyCertificate = true; // TODO: make configurable
        return client;
    }

    private static AsyncFtpClient BuildFtpClientDest(Uri uri, JobDestination dest)
    {
        var client = new AsyncFtpClient(uri.Host, dest.Username ?? "", dest.Password ?? "", uri.Port > 0 ? uri.Port : 21);
        client.Config.EncryptionMode     = dest.PathType == PathType.FTPS ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
        client.Config.DataConnectionType = dest.IsPassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
        client.Config.ValidateAnyCertificate = true;
        return client;
    }
}
