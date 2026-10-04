namespace FileRedirector.Models;

public enum PathType
{
    LocalOrUNC,
    HTTP,
    HTTPS,
    FTP,
    FTPS
}

public enum SourceFileAction
{
    Leave,
    Delete,
    Move,
    MarkProcessed  // rename with a suffix like .done or track in DB
}

public class RedirectJob
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 30;
    public SourceFileAction SourceAction { get; set; } = SourceFileAction.MarkProcessed;
    public string? MoveToPath { get; set; }           // used when SourceAction == Move
    public string? ProcessedSuffix { get; set; } = ".done"; // used when SourceAction == MarkProcessed
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public string? LastRunAt { get; set; }
    public string? Notes { get; set; }

    // Navigation (not stored directly — loaded separately)
    public List<JobSource> Sources { get; set; } = [];
    public List<JobDestination> Destinations { get; set; } = [];

    /// <summary>Deep copy, so the UI and actors never share (and mutate) the same instance.</summary>
    public RedirectJob Clone()
    {
        var copy = (RedirectJob)MemberwiseClone();
        copy.Sources      = [.. Sources.Select(s => s.Clone())];
        copy.Destinations = [.. Destinations.Select(d => d.Clone())];
        return copy;
    }
}

public class JobSource
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string Path { get; set; } = string.Empty;       // may contain @-wildcards
    public string FilePattern { get; set; } = "*.*";        // e.g. *.csv, Report_@MM@DD*.pdf
    public PathType PathType { get; set; } = PathType.LocalOrUNC;

    // FTP / HTTP credentials
    public string? Username { get; set; }
    public string? Password { get; set; }   // plain text in memory; DPAPI-encrypted in the DB
    public bool IsPassive { get; set; } = true;
    public bool AcceptAnyCertificate { get; set; }   // FTPS only: skip certificate validation

    public JobSource Clone() => (JobSource)MemberwiseClone();
}

public class JobDestination
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string Path { get; set; } = string.Empty;    // may contain @-wildcards
    public string? FileNameTemplate { get; set; }        // optional rename template; null = keep original
    public PathType PathType { get; set; } = PathType.LocalOrUNC;
    public int SortOrder { get; set; }

    // FTP / HTTP credentials
    public string? Username { get; set; }
    public string? Password { get; set; }   // plain text in memory; DPAPI-encrypted in the DB
    public bool IsPassive { get; set; } = true;
    public bool AcceptAnyCertificate { get; set; }   // FTPS only: skip certificate validation

    public JobDestination Clone() => (JobDestination)MemberwiseClone();
}

public class ProcessedFile
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public long? SourceModifiedTicks { get; set; }   // source LastModified (UTC ticks) at copy time
    public string ProcessedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

// ─── Actor messages ──────────────────────────────────────────────────────────

public record StartMonitoring(RedirectJob Job);
public record StopMonitoring(int JobId);
public record PollNow(int JobId);
public record FilesDiscovered(int JobId, JobSource Source, List<DiscoveredFile> Files);
public record ScanFailed(int JobId, string JobName, JobSource Source, string Error);
public record CopyFile(int JobId, DiscoveredFile File, JobSource Source, List<JobDestination> Destinations, SourceFileAction SourceAction, string? MoveToPath, string? ProcessedSuffix);
// Warning: set when the file was delivered but something non-fatal failed (e.g. the source action).
public record FileCopyResult(int JobId, DiscoveredFile File, bool Success, string? Error, string? Warning = null);

// Passed between actors, so it's immutable once created
public class DiscoveredFile
{
    public string FullPath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public DateTime LastModified { get; init; }
}
