using FileRedirector.Models;

namespace FileRedirector.Services;

/// <summary>
/// Receives activity from the actor system (log lines, copy results) so the UI can display it.
/// Called from actor threads — implementations must marshal to their own thread and must not block.
/// </summary>
public interface IActivitySink
{
    void Log(string message);
    void CopyCompleted(FileCopyResult result);
}
