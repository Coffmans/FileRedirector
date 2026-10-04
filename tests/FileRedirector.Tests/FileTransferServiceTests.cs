using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.Tests;

public sealed class FileTransferServiceTests : IDisposable
{
    private readonly TempFolder _temp = new();
    public void Dispose() => _temp.Dispose();

    private static DiscoveredFile Discovered(string path)
    {
        var info = new FileInfo(path);
        return new DiscoveredFile
        {
            FullPath = path, FileName = info.Name, SizeBytes = info.Length, LastModified = info.LastWriteTimeUtc
        };
    }

    private static readonly JobSource LocalSource = new() { PathType = PathType.LocalOrUNC };

    [Fact]
    public async Task Lists_matching_local_files()
    {
        var src = _temp.Sub("src");
        _temp.File("src/a.csv", "1");
        _temp.File("src/b.txt", "2");
        _temp.File("src/c.csv.done", "3");

        var files = await FileTransferService.ListSourceFilesAsync(new JobSource { Path = src, FilePattern = "*.csv" });

        Assert.Equal(["a.csv"], files.Select(f => f.FileName));
        Assert.Equal(1, files[0].SizeBytes);
    }

    [Fact]
    public async Task Missing_source_folder_is_reported()
        => await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            FileTransferService.ListSourceFilesAsync(new JobSource { Path = Path.Combine(_temp.Path, "nope") }));

    [Theory]
    [InlineData(PathType.HTTP)]
    [InlineData(PathType.HTTPS)]
    public async Task Http_cannot_be_a_source(PathType type)
        => await Assert.ThrowsAsync<NotSupportedException>(() =>
            FileTransferService.ListSourceFilesAsync(new JobSource { Path = "http://example.com/", PathType = type }));

    [Fact]
    public async Task Writes_local_destination_with_template_and_no_partial_files_left()
    {
        var srcFile = _temp.File("src/Report.pdf", "content");
        var outDir  = Path.Combine(_temp.Path, "out", "@YYYY");   // created on demand
        var dest    = new JobDestination { Path = outDir, FileNameTemplate = "@FILE_@MM@DD.@EXT" };
        var refTime = new DateTime(2026, 4, 7);

        await using (var staged = await FileTransferService.StageSourceAsync(Discovered(srcFile), LocalSource))
            await FileTransferService.WriteToDestinationAsync(staged, "Report.pdf", dest, refTime);

        var written = Path.Combine(_temp.Path, "out", "2026", "Report_0407.pdf");
        Assert.Equal("content", File.ReadAllText(written));
        Assert.Empty(Directory.GetFiles(_temp.Path, "*.partial", SearchOption.AllDirectories));
        Assert.True(File.Exists(srcFile));   // local sources are read in place, never deleted by staging
    }

    [Fact]
    public async Task MarkProcessed_never_overwrites_an_existing_marker()
    {
        var first = _temp.File("src/a.txt", "first");
        await FileTransferService.ApplySourceActionAsync(Discovered(first), LocalSource,
                                                         SourceFileAction.MarkProcessed, null, ".done");
        var second = _temp.File("src/a.txt", "second");
        await FileTransferService.ApplySourceActionAsync(Discovered(second), LocalSource,
                                                         SourceFileAction.MarkProcessed, null, ".done");

        var src = Path.Combine(_temp.Path, "src");
        Assert.Equal("first",  File.ReadAllText(Path.Combine(src, "a.txt.done")));
        Assert.Equal("second", File.ReadAllText(Path.Combine(src, "a.txt (1).done")));
        Assert.False(File.Exists(Path.Combine(src, "a.txt")));
    }

    [Fact]
    public async Task Move_never_overwrites_an_archived_file()
    {
        var archive = _temp.Sub("archive");
        _temp.File("archive/a.txt", "old");
        var file = _temp.File("src/a.txt", "new");

        await FileTransferService.ApplySourceActionAsync(Discovered(file), LocalSource,
                                                         SourceFileAction.Move, archive, null);

        Assert.Equal("old", File.ReadAllText(Path.Combine(archive, "a.txt")));
        Assert.Equal("new", File.ReadAllText(Path.Combine(archive, "a (1).txt")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task Move_without_a_path_fails_clearly(string? movePath)
    {
        var file = _temp.File("src/a.txt", "x");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            FileTransferService.ApplySourceActionAsync(Discovered(file), LocalSource,
                                                       SourceFileAction.Move, movePath, null));
        Assert.True(File.Exists(file));
    }

    [Fact]
    public async Task Delete_and_Leave_actions()
    {
        var a = _temp.File("src/a.txt", "x");
        var b = _temp.File("src/b.txt", "x");
        await FileTransferService.ApplySourceActionAsync(Discovered(a), LocalSource, SourceFileAction.Delete, null, null);
        await FileTransferService.ApplySourceActionAsync(Discovered(b), LocalSource, SourceFileAction.Leave, null, null);
        Assert.False(File.Exists(a));
        Assert.True(File.Exists(b));
    }

    [Fact]
    public void Detects_files_open_for_writing()
    {
        var path = _temp.File("src/a.txt", "x");
        Assert.False(FileTransferService.IsLocalFileLocked(path));
        using (new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read))
            Assert.True(FileTransferService.IsLocalFileLocked(path));
        Assert.False(FileTransferService.IsLocalFileLocked(path));
    }
}
