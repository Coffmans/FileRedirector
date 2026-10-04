using Microsoft.Data.Sqlite;

namespace FileRedirector.Tests;

/// <summary>A unique scratch folder per test, deleted afterwards.</summary>
public sealed class TempFolder : IDisposable
{
    public string Path { get; } = Directory.CreateDirectory(
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FileRedirector.Tests", Guid.NewGuid().ToString("N"))).FullName;

    public string Sub(string name) => Directory.CreateDirectory(System.IO.Path.Combine(Path, name)).FullName;

    public string File(string relative, string content)
    {
        var full = System.IO.Path.Combine(Path, relative);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
        System.IO.File.WriteAllText(full, content);
        return full;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();   // pooled connections keep the .db file open
        try { Directory.Delete(Path, recursive: true); } catch { /* best effort */ }
    }
}
