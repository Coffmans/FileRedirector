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

public enum JobStatus
{
    Idle,
    Running,
    Paused,
    Error,
    Disabled
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
    public string? Password { get; set; }   // stored encrypted in DB
    public bool IsPassive { get; set; } = true;
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
    public string? Password { get; set; }
    public bool IsPassive { get; set; } = true;
}

public class ProcessedFile
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ProcessedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

// ─── Actor messages ──────────────────────────────────────────────────────────

public record StartMonitoring(RedirectJob Job);
public record StopMonitoring(int JobId);
public record PollNow(int JobId);
public record FilesDiscovered(int JobId, JobSource Source, List<DiscoveredFile> Files);
public record CopyFile(int JobId, DiscoveredFile File, JobSource Source, List<JobDestination> Destinations, SourceFileAction SourceAction, string? MoveToPath, string? ProcessedSuffix);
public record FileCopyResult(int JobId, DiscoveredFile File, bool Success, string? Error);

public class DiscoveredFile
{
    public string FullPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
}
