using Akka.Actor;
using FileRedirector.Data;
using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.Actors;

/// <summary>
/// One actor per RedirectJob.
/// Polls the job's source(s) on a configurable interval and hands off discovered
/// files to the FileCopyCoordinator (which routes them to the copy-worker pool).
/// </summary>
public class DirectoryMonitorActor : ReceiveActor, IWithTimers
{
    public ITimerScheduler Timers { get; set; } = null!;

    private readonly DatabaseService     _db;
    private readonly IActorRef           _coordinator;

    private RedirectJob _job;
    private bool        _polling = false;

    // Timer key – one timer per actor instance
    private const string PollTimerKey = "poll";

    public DirectoryMonitorActor(
        RedirectJob job,
        DatabaseService db,
        IActorRef coordinator)
    {
        _job         = job;
        _db          = db;
        _coordinator = coordinator;

        // StartMonitoring also carries the latest job definition (sent again after edits)
        Receive<StartMonitoring>(msg => { _job = msg.Job; OnStartMonitoring(); });
        Receive<StopMonitoring>(_ => OnStopMonitoring());
        Receive<PollNow>(_ => OnPoll());
    }

    protected override void PreStart()
    {
        base.PreStart();
        if (_job.IsEnabled)
            Self.Tell(new StartMonitoring(_job));
    }

    private void OnStartMonitoring()
    {
        _polling = true;
        RestartTimer();
        // Immediate first poll
        Self.Tell(new PollNow(_job.Id));
        Log($"Monitoring started (interval {_job.PollIntervalSeconds}s).");
    }

    private void OnStopMonitoring()
    {
        _polling = false;
        Timers.Cancel(PollTimerKey);
        Log("Monitoring stopped.");
    }

    private void RestartTimer()
    {
        Timers.Cancel(PollTimerKey);
        if (_polling)
        {
            Timers.StartPeriodicTimer(
                PollTimerKey,
                new PollNow(_job.Id),
                TimeSpan.FromSeconds(_job.PollIntervalSeconds));
        }
    }

    private void OnPoll()
    {
        if (!_polling) return;

        // Reload from DB so any configuration changes take effect without restarting
        var latest = _db.GetJob(_job.Id);
        if (latest is null || !latest.IsEnabled)
        {
            OnStopMonitoring();
            return;
        }
        _job = latest;
        _db.UpdateJobLastRun(_job.Id);

        // Fire scan tasks; results reported back via coordinator message
        foreach (var source in _job.Sources)
        {
            var capturedSource = source;
            var capturedJob    = _job;

            // Run the async scan on a background thread but pipe result to Self/coordinator
            var scanTask = FileTransferService.ListSourceFilesAsync(capturedSource);

            // Pipe completed task as a message
            scanTask.PipeTo(
                recipient: _coordinator,
                sender:    Self,
                success:   files => new FilesDiscovered(capturedJob.Id, capturedSource, files),
                // Runs off the actor thread, so only captured values are used here
                failure:   ex => new ScanFailed(capturedJob.Id, capturedJob.Name, capturedSource,
                                                ex.GetBaseException().Message));
        }
    }

    private void Log(string msg)
        => AppLog.Info($"[Monitor:{_job.Name}] {msg}");

    // ─── Props factory ────────────────────────────────────────────────────────

    public static Props CreateProps(
        RedirectJob job,
        DatabaseService db,
        IActorRef coordinator)
        => Props.Create(() => new DirectoryMonitorActor(job, db, coordinator));
}
