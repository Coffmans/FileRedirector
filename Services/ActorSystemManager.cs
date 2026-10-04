using Akka.Actor;
using FileRedirector.Actors;
using FileRedirector.Data;
using FileRedirector.Models;

namespace FileRedirector.Services;

/// <summary>
/// Manages the Akka.NET ActorSystem lifecycle and provides a simple façade
/// for the WinForms layer to start/stop/refresh jobs.
/// </summary>
public class ActorSystemManager(DatabaseService db, IActivitySink sink) : IDisposable
{
    private ActorSystem? _system;
    private IActorRef    _coordinator = ActorRefs.Nobody;

    private readonly DatabaseService _db = db;
    private readonly IActivitySink _sink = sink;

    public void Start()
    {
        _system     = ActorSystem.Create("FileRedirector");
        _coordinator = _system.ActorOf(
            CoordinatorActor.CreateProps(_db, _sink), "coordinator");
    }

    /// <summary>Starts the job's monitor, or restarts it with the updated definition if already running.</summary>
    // Actors get their own copy — the caller's instance stays owned by the UI thread
    public void StartJob(RedirectJob job)   => _coordinator.Tell(new StartMonitoring(job.Clone()));
    public void StopJob(int jobId)          => _coordinator.Tell(new StopMonitoring(jobId));

    public void Dispose()
    {
        _system?.Terminate().Wait(TimeSpan.FromSeconds(5));
        _system?.Dispose();
        GC.SuppressFinalize(this);
    }
}
