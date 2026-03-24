using Akka.Actor;
using Akka.Routing;
using FileRedirector.Data;
using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.Actors;

/// <summary>
/// Top-level supervisor.
/// • Owns one DirectoryMonitorActor per job.
/// • Owns a round-robin pool of FileCopyActors.
/// • Receives FilesDiscovered, deduplicates, and dispatches CopyFile to the pool.
/// • Raises UI-friendly events so the WinForms layer can update without tight coupling.
/// </summary>
public class CoordinatorActor : ReceiveActor
{
    public static event Action<string>?         LogMessage;
    public static event Action<FileCopyResult>? CopyCompleted;

    private readonly DatabaseService     _db;
    private readonly FileTransferService _transfer;

    // jobId → monitor actor
    private readonly Dictionary<int, IActorRef> _monitors = new();

    // Round-robin pool of copy workers
    private IActorRef _copyPool = ActorRefs.Nobody;

    public CoordinatorActor(DatabaseService db, FileTransferService transfer)
    {
        _db       = db;
        _transfer = transfer;

        Receive<StartMonitoring>(msg => OnStartMonitoring(msg));
        Receive<StopMonitoring>(msg  => OnStopMonitoring(msg));
        Receive<FilesDiscovered>(msg => OnFilesDiscovered(msg));
        Receive<FileCopyResult>(msg  => OnFileCopyResult(msg));

        // Refresh job config in its monitor
        Receive<RedirectJob>(job =>
        {
            if (_monitors.TryGetValue(job.Id, out var mon))
                mon.Tell(job);
        });
    }

    protected override void PreStart()
    {
        base.PreStart();

        // Create the copy pool (size can be tuned; 4 concurrent copies)
        var poolProps = FileCopyActor
            .CreateProps(_db, _transfer)
            .WithRouter(new RoundRobinPool(4));

        _copyPool = Context.ActorOf(poolProps, "copy-pool");

        // Start a monitor for every enabled job
        foreach (var job in _db.GetAllJobs().Where(j => j.IsEnabled))
            SpawnMonitor(job);
    }

    // ─── Message handlers ─────────────────────────────────────────────────────

    private void OnStartMonitoring(StartMonitoring msg)
    {
        if (_monitors.ContainsKey(msg.Job.Id))
        {
            _monitors[msg.Job.Id].Tell(msg);
        }
        else
        {
            SpawnMonitor(msg.Job);
        }
        Log($"Job '{msg.Job.Name}' started.");
    }

    private void OnStopMonitoring(StopMonitoring msg)
    {
        if (_monitors.TryGetValue(msg.JobId, out var mon))
        {
            mon.Tell(msg);
            Log($"Job id={msg.JobId} stopped.");
        }
    }

    private void OnFilesDiscovered(FilesDiscovered msg)
    {
        var job = _db.GetJob(msg.JobId);
        if (job is null) return;

        var newFiles = new List<DiscoveredFile>();
        foreach (var file in msg.Files)
        {
            // Skip if already processed (Leave mode) or already has processed suffix
            if (job.SourceAction == SourceFileAction.Leave &&
                _db.WasAlreadyProcessed(msg.JobId, msg.Source.Path, file.FileName))
                continue;

            // Skip files that already carry the processed suffix
            if (job.SourceAction == SourceFileAction.MarkProcessed &&
                !string.IsNullOrEmpty(job.ProcessedSuffix) &&
                file.FileName.EndsWith(job.ProcessedSuffix, StringComparison.OrdinalIgnoreCase))
                continue;

            newFiles.Add(file);
        }

        if (newFiles.Count == 0) return;

        Log($"Job '{job.Name}': {newFiles.Count} new file(s) discovered from {msg.Source.Path}");

        foreach (var file in newFiles)
        {
            _copyPool.Tell(new CopyFile(
                JobId:          msg.JobId,
                File:           file,
                Source:         msg.Source,
                Destinations:   job.Destinations,
                SourceAction:   job.SourceAction,
                MoveToPath:     job.MoveToPath,
                ProcessedSuffix:job.ProcessedSuffix),
                Self); // route replies back here
        }
    }

    private void OnFileCopyResult(FileCopyResult msg)
    {
        var status = msg.Success ? "OK" : $"FAILED – {msg.Error}";
        Log($"Copy {status}: {msg.File.FileName}");
        CopyCompleted?.Invoke(msg);
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private void SpawnMonitor(RedirectJob job)
    {
        var props  = DirectoryMonitorActor.CreateProps(job, _db, _transfer, Self);
        var actor  = Context.ActorOf(props, $"monitor-{job.Id}");
        _monitors[job.Id] = actor;
    }

    private static void Log(string msg)
    {
        Console.WriteLine($"[Coordinator] {msg}");
        LogMessage?.Invoke(msg);
    }

    public static Props CreateProps(DatabaseService db, FileTransferService transfer)
        => Props.Create(() => new CoordinatorActor(db, transfer));
}
