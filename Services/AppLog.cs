namespace FileRedirector.Services;

/// <summary>
/// Minimal thread-safe file log: one file per day under %APPDATA%\FileRedirector\logs.
/// Logging never throws — a failure to write the log must not take the app down.
/// </summary>
public static class AppLog
{
    private static readonly object _lock = new();

    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileRedirector", "logs");

    public static void Info(string message)  => Write("INFO ", message);
    public static void Warn(string message)  => Write("WARN ", message);
    public static void Error(string message, Exception? ex = null)
        => Write("ERROR", ex is null ? message : $"{message}{Environment.NewLine}{ex}");

    /// <summary>Deletes log files older than the given number of days.</summary>
    public static void PruneOldLogs(int keepDays = 30)
    {
        try
        {
            if (!Directory.Exists(LogDirectory)) return;
            var cutoff = DateTime.Now.AddDays(-keepDays);
            foreach (var file in Directory.EnumerateFiles(LogDirectory, "FileRedirector-*.log"))
                if (File.GetLastWriteTime(file) < cutoff)
                    File.Delete(file);
        }
        catch { /* best effort */ }
    }

    private static void Write(string level, string message)
    {
        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(LogDirectory);
                File.AppendAllText(
                    Path.Combine(LogDirectory, $"FileRedirector-{DateTime.Now:yyyyMMdd}.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {level} {message}{Environment.NewLine}");
            }
        }
        catch { /* logging must never crash the app */ }
    }
}
