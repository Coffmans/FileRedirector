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
    private readonly FileTransferService _transfer;

    public FileCopyActor(DatabaseService db, FileTransferService transfer)
    {
        _db       = db;
        _transfer = transfer;

        ReceiveAsync<CopyFile>(HandleCopyFileAsync);
    }

    private async Task HandleCopyFileAsync(CopyFile msg)
    {
        var refTime = DateTime.Now;
        var result  = new FileCopyResult(msg.JobId, msg.File, Success: false, Error: null);

        try
        {
            // ── 1. Read source ────────────────────────────────────────────────
            var data = await _transfer.ReadSourceFileAsync(msg.File, msg.Source);

            // ── 2. Write to each destination ──────────────────────────────────
            var errors = new List<string>();
            foreach (var dest in msg.Destinations)
            {
                try
                {
                    await _transfer.WriteToDestinationAsync(data, msg.File.FileName, dest, refTime);
                }
                catch (Exception ex)
                {
                    errors.Add($"Dest '{dest.Path}': {ex.Message}");
                }
            }

            bool allOk = errors.Count == 0;

            // ── 3. Apply source action ONLY if all destinations succeeded ─────
            if (allOk)
            {
                await _transfer.ApplySourceActionAsync(
                    msg.File, msg.Source,
                    msg.SourceAction, msg.MoveToPath, msg.ProcessedSuffix);
            }

            // ── 4. Record in DB ───────────────────────────────────────────────
            _db.RecordProcessedFile(new ProcessedFile
            {
                JobId        = msg.JobId,
                SourcePath   = msg.Source.Path,
                FileName     = msg.File.FileName,
                FileSizeBytes= msg.File.SizeBytes,
                ProcessedAt  = DateTime.UtcNow.ToString("o"),
                Success      = allOk,
                ErrorMessage = allOk ? null : string.Join("; ", errors)
            });

            result = result with { Success = allOk, Error = allOk ? null : string.Join("; ", errors) };
        }
        catch (Exception ex)
        {
            _db.RecordProcessedFile(new ProcessedFile
            {
                JobId        = msg.JobId,
                SourcePath   = msg.Source.Path,
                FileName     = msg.File.FileName,
                FileSizeBytes= msg.File.SizeBytes,
                ProcessedAt  = DateTime.UtcNow.ToString("o"),
                Success      = false,
                ErrorMessage = ex.Message
            });

            result = result with { Error = ex.Message };
        }

        // Reply to coordinator
        Sender.Tell(result);
    }

    public static Props CreateProps(DatabaseService db, FileTransferService transfer)
        => Props.Create(() => new FileCopyActor(db, transfer));
}
