using System.Collections.Concurrent;
using FileRedirector.Data;
using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.Tests;

/// <summary>
/// Runs the real actor system against local folders with a 1-second poll interval.
/// Slower than the unit tests (a few seconds each).
/// </summary>
[Trait("Category", "Integration")]
public sealed class PipelineIntegrationTests : IDisposable
{
    private readonly TempFolder _temp = new();
    private readonly DatabaseService _db;
    private readonly RecordingSink _sink = new();
    private readonly ActorSystemManager _mgr;

    public PipelineIntegrationTests()
    {
        _db  = new DatabaseService(Path.Combine(_temp.Path, "jobs.db"));
        _mgr = new ActorSystemManager(_db, _sink);
        _mgr.Start();
    }

    public void Dispose()
    {
        _mgr.Dispose();
        _temp.Dispose();
    }

    private RedirectJob StartJob(SourceFileAction action, string src, string dst, string pattern = "*.*")
    {
        var job = new RedirectJob
        {
            Name = action.ToString(), PollIntervalSeconds = 1, SourceAction = action,
            Sources      = [new JobSource { Path = src, FilePattern = pattern }],
            Destinations = [new JobDestination { Path = dst }]
        };
        _db.SaveJob(job);
        _mgr.StartJob(job);
        return job;
    }

    private static async Task<bool> WaitUntil(Func<bool> condition, int timeoutMs = 10_000)
    {
        var until = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < until)
        {
            if (condition()) return true;
            await Task.Delay(100);
        }
        return condition();
    }

    [Fact]
    public async Task Copies_then_marks_processed_exactly_once()
    {
        var src = _temp.Sub("src");
        var dst = Path.Combine(_temp.Path, "dst");
        StartJob(SourceFileAction.MarkProcessed, src, dst, "*.txt");

        _temp.File("src/a.txt", "hello");

        Assert.True(await WaitUntil(() => File.Exists(Path.Combine(src, "a.txt.done"))));
        Assert.Equal("hello", File.ReadAllText(Path.Combine(dst, "a.txt")));

        await Task.Delay(3000);   // several more polls
        Assert.Single(_sink.Results);
        Assert.True(_sink.Results.Single().Success);
    }

    [Fact]
    public async Task Does_not_copy_a_file_while_it_is_being_written()
    {
        var src = _temp.Sub("src");
        var dst = Path.Combine(_temp.Path, "dst");
        StartJob(SourceFileAction.Delete, src, dst);

        var writer = new FileStream(Path.Combine(src, "big.bin"), FileMode.Create, FileAccess.Write, FileShare.Read);
        writer.Write(new byte[1024]);
        writer.Flush();

        await Task.Delay(4000);
        Assert.False(File.Exists(Path.Combine(dst, "big.bin")));

        writer.Dispose();
        Assert.True(await WaitUntil(() => File.Exists(Path.Combine(dst, "big.bin"))));
        Assert.True(await WaitUntil(() => !File.Exists(Path.Combine(src, "big.bin"))));
    }

    [Fact]
    public async Task Leave_mode_recopies_only_when_the_file_changes()
    {
        var src = _temp.Sub("src");
        var dst = Path.Combine(_temp.Path, "dst");
        StartJob(SourceFileAction.Leave, src, dst);

        var srcFile = _temp.File("src/r.csv", "v1");
        var dstFile = Path.Combine(dst, "r.csv");
        Assert.True(await WaitUntil(() => File.Exists(dstFile)));

        File.Delete(dstFile);
        await Task.Delay(3000);
        Assert.False(File.Exists(dstFile));   // unchanged → not copied again

        File.WriteAllText(srcFile, "v2-longer");
        Assert.True(await WaitUntil(() => File.Exists(dstFile)));
        Assert.Equal("v2-longer", File.ReadAllText(dstFile));
    }

    [Fact]
    public async Task Stop_then_start_immediately_keeps_working()
    {
        var src = _temp.Sub("src");
        var dst = Path.Combine(_temp.Path, "dst");
        var job = StartJob(SourceFileAction.Delete, src, dst);

        _mgr.StopJob(job.Id);
        _mgr.StartJob(job);
        _mgr.StopJob(job.Id);
        _mgr.StartJob(job);

        _temp.File("src/b.txt", "b");
        Assert.True(await WaitUntil(() => File.Exists(Path.Combine(dst, "b.txt"))));
    }

    [Fact]
    public async Task Scan_errors_are_reported_once()
    {
        var dst = Path.Combine(_temp.Path, "dst");
        StartJob(SourceFileAction.Leave, Path.Combine(_temp.Path, "missing"), dst);

        Assert.True(await WaitUntil(() => _sink.Messages.Any(m => m.Contains("scan FAILED"))));
        await Task.Delay(3000);
        Assert.Single(_sink.Messages, m => m.Contains("scan FAILED"));
    }

    private sealed class RecordingSink : IActivitySink
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ConcurrentQueue<FileCopyResult> Results { get; } = new();
        public void Log(string message) => Messages.Enqueue(message);
        public void CopyCompleted(FileCopyResult result) => Results.Enqueue(result);
    }
}
