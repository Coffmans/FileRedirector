using Dapper;
using FileRedirector.Data;
using FileRedirector.Models;
using Microsoft.Data.Sqlite;

namespace FileRedirector.Tests;

public sealed class DatabaseServiceTests : IDisposable
{
    private readonly TempFolder _temp = new();
    private string DbPath => Path.Combine(_temp.Path, "jobs.db");

    public void Dispose() => _temp.Dispose();

    private T Raw<T>(string sql)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        return conn.ExecuteScalar<T>(sql)!;
    }

    private static RedirectJob SampleJob() => new()
    {
        Name = "job",
        Sources      = [new JobSource { Path = @"C:\in", Username = "u", Password = "src-secret" }],
        Destinations = [new JobDestination { Path = @"C:\out", Password = "dest-secret", PathType = PathType.FTPS,
                                             AcceptAnyCertificate = true }]
    };

    [Fact]
    public void Saves_and_loads_a_job_with_encrypted_passwords()
    {
        var db = new DatabaseService(DbPath);
        var id = db.SaveJob(SampleJob());

        var loaded = db.GetJob(id)!;
        Assert.Equal("src-secret", loaded.Sources.Single().Password);
        Assert.Equal("dest-secret", loaded.Destinations.Single().Password);
        Assert.True(loaded.Destinations.Single().AcceptAnyCertificate);

        Assert.StartsWith(SecretProtector.Prefix, Raw<string>("SELECT Password FROM JobSources"));
        Assert.StartsWith(SecretProtector.Prefix, Raw<string>("SELECT Password FROM JobDestinations"));
    }

    [Fact]
    public void Uses_wal_journal_mode()
    {
        _ = new DatabaseService(DbPath);
        Assert.Equal("wal", Raw<string>("PRAGMA journal_mode"));
    }

    [Fact]
    public void SetJobEnabled_persists()
    {
        var db = new DatabaseService(DbPath);
        var id = db.SaveJob(SampleJob());
        db.SetJobEnabled(id, false);
        Assert.False(db.GetJob(id)!.IsEnabled);
    }

    [Fact]
    public void Saving_an_edited_job_keeps_its_last_run_time()
    {
        var db = new DatabaseService(DbPath);
        var id = db.SaveJob(SampleJob());
        var stale = db.GetJob(id)!;      // LastRunAt is null in this copy
        db.UpdateJobLastRun(id);
        db.SaveJob(stale);
        Assert.NotNull(db.GetJob(id)!.LastRunAt);
    }

    [Fact]
    public void DeleteJob_removes_children_and_history()
    {
        var db = new DatabaseService(DbPath);
        var id = db.SaveJob(SampleJob());
        db.RecordProcessedFile(new ProcessedFile { JobId = id, SourcePath = "p", FileName = "f", Success = true });

        db.DeleteJob(id);

        Assert.Null(db.GetJob(id));
        Assert.Equal(0, Raw<int>("SELECT COUNT(*) FROM JobSources"));
        Assert.Equal(0, Raw<int>("SELECT COUNT(*) FROM JobDestinations"));
        Assert.Equal(0, Raw<int>("SELECT COUNT(*) FROM ProcessedFiles"));
    }

    [Fact]
    public void WasAlreadyProcessed_matches_the_exact_file_version()
    {
        var db = new DatabaseService(DbPath);
        db.RecordProcessedFile(new ProcessedFile
        {
            JobId = 1, SourcePath = "src", FileName = "a.csv", FileSizeBytes = 10, SourceModifiedTicks = 100, Success = true
        });

        Assert.True(db.WasAlreadyProcessed(1, "src", "a.csv", 10, 100));
        Assert.False(db.WasAlreadyProcessed(1, "src", "a.csv", 10, 200));   // modified since
        Assert.False(db.WasAlreadyProcessed(1, "src", "a.csv", 11, 100));   // size changed
        Assert.False(db.WasAlreadyProcessed(2, "src", "a.csv", 10, 100));   // other job
    }

    [Fact]
    public void Failed_copies_do_not_count_as_processed()
    {
        var db = new DatabaseService(DbPath);
        db.RecordProcessedFile(new ProcessedFile
        {
            JobId = 1, SourcePath = "src", FileName = "a.csv", FileSizeBytes = 10, SourceModifiedTicks = 100, Success = false
        });
        Assert.False(db.WasAlreadyProcessed(1, "src", "a.csv", 10, 100));
    }

    [Fact]
    public void Legacy_history_rows_only_match_when_requested()
    {
        var db = new DatabaseService(DbPath);
        db.RecordProcessedFile(new ProcessedFile
        {
            JobId = 1, SourcePath = "src", FileName = "a.csv", FileSizeBytes = 10, SourceModifiedTicks = null, Success = true
        });

        Assert.False(db.WasAlreadyProcessed(1, "src", "a.csv", 10, 100));
        Assert.True(db.WasAlreadyProcessed(1, "src", "a.csv", 10, 100, matchLegacyRows: true));
        Assert.False(db.WasAlreadyProcessed(1, "src", "a.csv", 99, 100, matchLegacyRows: true));
    }

    [Fact]
    public void Migrates_a_database_from_the_previous_version()
    {
        using (var conn = new SqliteConnection($"Data Source={DbPath}"))
        {
            conn.Execute("""
                CREATE TABLE RedirectJobs (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, IsEnabled INTEGER NOT NULL DEFAULT 1,
                  PollIntervalSeconds INTEGER NOT NULL DEFAULT 30, SourceAction TEXT NOT NULL DEFAULT 'MarkProcessed', MoveToPath TEXT,
                  ProcessedSuffix TEXT DEFAULT '.done', CreatedAt TEXT NOT NULL, LastRunAt TEXT, Notes TEXT);
                CREATE TABLE JobSources (Id INTEGER PRIMARY KEY AUTOINCREMENT, JobId INTEGER NOT NULL, Path TEXT NOT NULL,
                  FilePattern TEXT NOT NULL DEFAULT '*.*', PathType TEXT NOT NULL DEFAULT 'LocalOrUNC',
                  Username TEXT, Password TEXT, IsPassive INTEGER NOT NULL DEFAULT 1);
                CREATE TABLE JobDestinations (Id INTEGER PRIMARY KEY AUTOINCREMENT, JobId INTEGER NOT NULL, Path TEXT NOT NULL,
                  FileNameTemplate TEXT, PathType TEXT NOT NULL DEFAULT 'LocalOrUNC', SortOrder INTEGER NOT NULL DEFAULT 0,
                  Username TEXT, Password TEXT, IsPassive INTEGER NOT NULL DEFAULT 1);
                CREATE TABLE ProcessedFiles (Id INTEGER PRIMARY KEY AUTOINCREMENT, JobId INTEGER NOT NULL, SourcePath TEXT NOT NULL,
                  FileName TEXT NOT NULL, FileSizeBytes INTEGER NOT NULL DEFAULT 0, ProcessedAt TEXT NOT NULL,
                  Success INTEGER NOT NULL DEFAULT 1, ErrorMessage TEXT);
                INSERT INTO RedirectJobs (Name, CreatedAt) VALUES ('legacy', '2026-01-01');
                INSERT INTO JobSources (JobId, Path, PathType, Password) VALUES (1, 'ftp.example.com/in', '4', 'secret1');
                INSERT INTO JobDestinations (JobId, Path, PathType, Password) VALUES (1, 'D:/out', '0', 'secret2');
                """);
        }
        SqliteConnection.ClearAllPools();

        var job = new DatabaseService(DbPath).GetJob(1)!;

        Assert.Equal("secret1", job.Sources.Single().Password);
        Assert.Equal("secret2", job.Destinations.Single().Password);
        Assert.StartsWith(SecretProtector.Prefix, Raw<string>("SELECT Password FROM JobSources"));
        Assert.True(job.Sources.Single().AcceptAnyCertificate);        // existing FTPS entry keeps working
        Assert.False(job.Destinations.Single().AcceptAnyCertificate);  // non-FTPS entry is unaffected
        Assert.Equal(1, Raw<int>("SELECT COUNT(*) FROM pragma_table_info('ProcessedFiles') WHERE name='SourceModifiedTicks'"));
    }

    [Fact]
    public void Clone_is_independent_of_the_original()
    {
        var job = SampleJob();
        var copy = job.Clone();
        copy.Name = "changed";
        copy.Sources[0].Path = "changed";
        copy.Destinations.Clear();

        Assert.Equal("job", job.Name);
        Assert.Equal(@"C:\in", job.Sources[0].Path);
        Assert.Single(job.Destinations);
    }
}
