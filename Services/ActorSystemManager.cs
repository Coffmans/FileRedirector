using Akka.Actor;
using FileRedirector.Actors;
using FileRedirector.Data;
using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.Services;

/// <summary>
/// Manages the Akka.NET ActorSystem lifecycle and provides a simple façade
/// for the WinForms layer to start/stop/refresh jobs.
/// </summary>
public class ActorSystemManager : IDisposable
{
    private ActorSystem? _system;
    private IActorRef    _coordinator = ActorRefs.Nobody;

    private readonly DatabaseService     _db;
    private readonly FileTransferService _transfer;

    public ActorSystemManager(DatabaseService db, FileTransferService transfer)
    {
        _db       = db;
        _transfer = transfer;
    }

    public void Start()
    {
        _system     = ActorSystem.Create("FileRedirector");
        _coordinator = _system.ActorOf(
            CoordinatorActor.CreateProps(_db, _transfer), "coordinator");
    }

    public void StartJob(RedirectJob job)   => _coordinator.Tell(new StartMonitoring(job));
    public void StopJob(int jobId)          => _coordinator.Tell(new StopMonitoring(jobId));
    public void RefreshJob(RedirectJob job) => _coordinator.Tell(job); // pushes updated config

    public void Dispose()
    {
        _system?.Terminate().Wait(TimeSpan.FromSeconds(5));
        _system?.Dispose();
    }
}
