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
/// • A file is only dispatched once it is stable (same size and modified time on two
///   consecutive scans, and not locked for writing) and not already being copied.
/// • Raises UI-friendly events so the WinForms layer can update without tight coupling.
/// </summary>
public class CoordinatorActor : ReceiveActor
{
    private readonly DatabaseService     _db;
    private readonly IActivitySink       _sink;

    // jobId → monitor actor
    private readonly Dictionary<int, IActorRef> _monitors = [];

    // Round-robin pool of copy workers
    private IActorRef _copyPool = ActorRefs.Nobody;

    // Makes monitor names unique: a stopped monitor's name stays reserved until it has fully terminated.
    private int _monitorSeq;

    private readonly record struct FileKey(int JobId, string SourcePath, string FullPath);

    // Files currently dispatched to the copy pool and not yet reported back
    private readonly HashSet<FileKey> _inFlight = [];

    // Files seen on the last scan but not yet dispatched → their size/modified time at that scan
    private readonly Dictionary<FileKey, (long Size, DateTime Modified)> _pending = [];

    // (jobId, source path) → last scan error, so a failing source is logged once rather than every poll
    private readonly Dictionary<(int JobId, string SourcePath), string> _scanErrors = [];

    public CoordinatorActor(DatabaseService db, IActivitySink sink)
    {
        _db       = db;
        _sink     = sink;

        Receive<StartMonitoring>(msg => OnStartMonitoring(msg));
        Receive<StopMonitoring>(msg  => OnStopMonitoring(msg));
        Receive<FilesDiscovered>(msg => OnFilesDiscovered(msg));
        Receive<FileCopyResult>(msg  => OnFileCopyResult(msg));
        Receive<ScanFailed>(msg      => OnScanFailed(msg));
    }

    protected override void PreStart()
    {
        base.PreStart();

        // Create the copy pool (size can be tuned; 4 concurrent copies)
        var poolProps = FileCopyActor
            .CreateProps(_db, _sink)
            .WithRouter(new RoundRobinPool(4));

        _copyPool = Context.ActorOf(poolProps, "copy-pool");

        // Start a monitor for every enabled job
        foreach (var job in _db.GetAllJobs().Where(j => j.IsEnabled))
            SpawnMonitor(job);
    }

    // ─── Message handlers ─────────────────────────────────────────────────────

    private void OnStartMonitoring(StartMonitoring msg)
    {
        if (_monitors.TryGetValue(msg.Job.Id, out IActorRef? value))
        {
            value.Tell(msg);
        }
        else
        {
            SpawnMonitor(msg.Job);
        }
        Log($"Job '{msg.Job.Name}' started.");
    }

    private void OnStopMonitoring(StopMonitoring msg)
    {
        if (_monitors.Remove(msg.JobId, out var mon))
        {
            Context.Stop(mon);
            _pending.Keys.Where(k => k.JobId == msg.JobId).ToList()
                         .ForEach(k => _pending.Remove(k));
            _scanErrors.Keys.Where(k => k.JobId == msg.JobId).ToList()
                            .ForEach(k => _scanErrors.Remove(k));
            Log($"Job id={msg.JobId} stopped.");
        }
    }

    private void OnFilesDiscovered(FilesDiscovered msg)
    {
        // Ignore results from a monitor that has since been stopped/replaced
        if (!_monitors.TryGetValue(msg.JobId, out var mon) || !mon.Equals(Sender)) return;

        var job = _db.GetJob(msg.JobId);
        if (job is null) return;

        if (_scanErrors.Remove((msg.JobId, msg.Source.Path)))
            Log($"Job '{job.Name}': scanning {msg.Source.Path} is working again.");

        // Forget pending files from this source that have disappeared since the last scan
        var seen = msg.Files.Select(f => f.FullPath).ToHashSet();
        _pending.Keys
            .Where(k => k.JobId == msg.JobId && k.SourcePath == msg.Source.Path && !seen.Contains(k.FullPath))
            .ToList()
            .ForEach(k => _pending.Remove(k));

        var newFiles = new List<DiscoveredFile>();
        foreach (var file in msg.Files)
        {
            var key = new FileKey(msg.JobId, msg.Source.Path, file.FullPath);

            // Already being copied (scan overlapped a slow copy)
            if (_inFlight.Contains(key))
                continue;

            // Skip files that already carry the processed suffix
            if (job.SourceAction == SourceFileAction.MarkProcessed &&
                !string.IsNullOrEmpty(job.ProcessedSuffix) &&
                file.FileName.EndsWith(job.ProcessedSuffix, StringComparison.OrdinalIgnoreCase))
                continue;

            // Skip this exact file version (name + size + modified time) if it was already delivered.
            // Leave mode relies on this entirely, so a file replaced or updated in place is copied again.
            // Other modes normally remove/rename the file, so a hit means the source action failed.
            // Leave mode also honours history rows from before modified times were recorded.
            if (_db.WasAlreadyProcessed(msg.JobId, msg.Source.Path, file.FileName, file.SizeBytes,
                                        file.LastModified.Ticks,
                                        matchLegacyRows: job.SourceAction == SourceFileAction.Leave))
                continue;

            // Stability: require identical size + modified time on two consecutive scans,
            // so files that are still being written aren't picked up half-finished.
            var observed = (file.SizeBytes, file.LastModified);
            if (!_pending.TryGetValue(key, out var previous) || previous != observed)
            {
                _pending[key] = observed;
                continue;
            }

            // Still held open for writing by another process — try again next scan
            if (msg.Source.PathType == PathType.LocalOrUNC && FileTransferService.IsLocalFileLocked(file.FullPath))
                continue;

            _pending.Remove(key);
            newFiles.Add(file);
        }

        if (newFiles.Count == 0) return;

        Log($"Job '{job.Name}': {newFiles.Count} new file(s) discovered from {msg.Source.Path}");

        foreach (var file in newFiles)
        {
            _inFlight.Add(new FileKey(msg.JobId, msg.Source.Path, file.FullPath));
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

    private void OnScanFailed(ScanFailed msg)
    {
        if (!_monitors.TryGetValue(msg.JobId, out var mon) || !mon.Equals(Sender)) return;

        var key = (msg.JobId, msg.Source.Path);
        if (_scanErrors.TryGetValue(key, out var previous) && previous == msg.Error) return;

        _scanErrors[key] = msg.Error;
        Log($"Job '{msg.JobName}': scan FAILED for {msg.Source.Path} – {msg.Error}");
    }

    private void OnFileCopyResult(FileCopyResult msg)
    {
        _inFlight.RemoveWhere(k => k.JobId == msg.JobId && k.FullPath == msg.File.FullPath);

        var status = msg.Success
            ? (msg.Warning is null ? "OK" : $"OK (warning – {msg.Warning})")
            : $"FAILED – {msg.Error}";
        Log($"Copy {status}: {msg.File.FileName}");
        _sink.CopyCompleted(msg);
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private void SpawnMonitor(RedirectJob job)
    {
        var props  = DirectoryMonitorActor.CreateProps(job, _db, Self);
        var actor  = Context.ActorOf(props, $"monitor-{job.Id}-{++_monitorSeq}");
        _monitors[job.Id] = actor;
    }

    private void Log(string msg)
    {
        AppLog.Info($"[Coordinator] {msg}");
        _sink.Log(msg);
    }

    public static Props CreateProps(DatabaseService db, IActivitySink sink)
        => Props.Create(() => new CoordinatorActor(db, sink));
}
