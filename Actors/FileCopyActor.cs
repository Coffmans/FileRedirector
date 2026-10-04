using Akka.Actor;
using FileRedirector.Data;
using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.Actors;

/// <summary>
/// A single worker in the FileCopy router pool.
/// Reads one file from its source, writes it to every destination, then
/// applies the source-file action (leave/delete/move/mark).
/// </summary>
public class FileCopyActor : ReceiveActor
{
    private readonly DatabaseService     _db;
    private readonly IActivitySink       _sink;

    public FileCopyActor(DatabaseService db, IActivitySink sink)
    {
        _db       = db;
        _sink     = sink;

        ReceiveAsync<CopyFile>(HandleCopyFileAsync);
    }

    private async Task HandleCopyFileAsync(CopyFile msg)
    {
        var sender  = Sender;   // capture before any await
        var refTime = DateTime.Now;
        var result  = new FileCopyResult(msg.JobId, msg.File, Success: false, Error: null);

        try
        {
            // ── 1. Stage source (local: read in place; remote: download once to temp) ──
            var errors = new List<string>();
            await using (var staged = await FileTransferService.StageSourceAsync(msg.File, msg.Source))
            {
                // ── 2. Stream to each destination ─────────────────────────────
                foreach (var dest in msg.Destinations)
                {
                    try
                    {
                        await FileTransferService.WriteToDestinationAsync(staged, msg.File.FileName, dest, refTime);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Dest '{dest.Path}': {ex.Message}");
                    }
                }
            }

            bool allOk = errors.Count == 0;

            // ── 3. Apply source action ONLY if all destinations succeeded ─────
            // The file has been delivered at this point, so a failure here is only a warning:
            // it's still recorded as processed (with its size/modified time) so the coordinator
            // won't deliver the same unchanged file again.
            string? warning = null;
            if (allOk)
            {
                try
                {
                    await FileTransferService.ApplySourceActionAsync(
                        msg.File, msg.Source,
                        msg.SourceAction, msg.MoveToPath, msg.ProcessedSuffix);
                }
                catch (Exception ex)
                {
                    warning = $"Delivered, but source action '{msg.SourceAction}' failed: {ex.Message}";
                }
            }

            var error = allOk ? warning : string.Join("; ", errors);
            Record(msg, allOk, error);
            result = result with { Success = allOk, Error = allOk ? null : error, Warning = warning };
        }
        catch (Exception ex)
        {
            Record(msg, success: false, ex.Message);
            result = result with { Error = ex.Message };
        }
        finally
        {
            // Always reply — the coordinator uses this to clear the file's in-flight marker.
            sender.Tell(result);
        }
    }

    private void Record(CopyFile msg, bool success, string? error)
    {
        try
        {
            _db.RecordProcessedFile(new ProcessedFile
            {
                JobId               = msg.JobId,
                SourcePath          = msg.Source.Path,
                FileName            = msg.File.FileName,
                FileSizeBytes       = msg.File.SizeBytes,
                SourceModifiedTicks = msg.File.LastModified.Ticks,
                ProcessedAt         = DateTime.UtcNow.ToString("o"),
                Success             = success,
                ErrorMessage        = error
            });
        }
        catch (Exception ex)
        {
            AppLog.Error($"[FileCopy] Failed to record history for {msg.File.FileName}", ex);
            _sink.Log($"Failed to record history for {msg.File.FileName}: {ex.Message}");
        }
    }

    public static Props CreateProps(DatabaseService db, IActivitySink sink)
        => Props.Create(() => new FileCopyActor(db, sink));
}
