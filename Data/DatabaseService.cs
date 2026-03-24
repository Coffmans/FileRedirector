using Dapper;
using Microsoft.Data.Sqlite;
using FileRedirector.Models;
using System.Text.Json;

namespace FileRedirector.Data;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string dbPath)
    {
        _connectionString = $"Data Source={dbPath};";
        InitializeDatabase();
    }

    private SqliteConnection GetConnection() => new(_connectionString);

    private void InitializeDatabase()
    {
        using var conn = GetConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS RedirectJobs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                IsEnabled INTEGER NOT NULL DEFAULT 1,
                PollIntervalSeconds INTEGER NOT NULL DEFAULT 30,
                SourceAction TEXT NOT NULL DEFAULT 'MarkProcessed',
                MoveToPath TEXT,
                ProcessedSuffix TEXT DEFAULT '.done',
                CreatedAt TEXT NOT NULL,
                LastRunAt TEXT,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS JobSources (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                JobId INTEGER NOT NULL REFERENCES RedirectJobs(Id) ON DELETE CASCADE,
                Path TEXT NOT NULL,
                FilePattern TEXT NOT NULL DEFAULT '*.*',
                PathType TEXT NOT NULL DEFAULT 'LocalOrUNC',
                Username TEXT,
                Password TEXT,
                IsPassive INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS JobDestinations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                JobId INTEGER NOT NULL REFERENCES RedirectJobs(Id) ON DELETE CASCADE,
                Path TEXT NOT NULL,
                FileNameTemplate TEXT,
                PathType TEXT NOT NULL DEFAULT 'LocalOrUNC',
                SortOrder INTEGER NOT NULL DEFAULT 0,
                Username TEXT,
                Password TEXT,
                IsPassive INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS ProcessedFiles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                JobId INTEGER NOT NULL,
                SourcePath TEXT NOT NULL,
                FileName TEXT NOT NULL,
                FileSizeBytes INTEGER NOT NULL DEFAULT 0,
                ProcessedAt TEXT NOT NULL,
                Success INTEGER NOT NULL DEFAULT 1,
                ErrorMessage TEXT
            );

            CREATE INDEX IF NOT EXISTS IX_ProcessedFiles_JobId_FileName
                ON ProcessedFiles(JobId, FileName, SourcePath);
            """);
    }

    // ─── Jobs ────────────────────────────────────────────────────────────────

    public List<RedirectJob> GetAllJobs()
    {
        using var conn = GetConnection();
        var jobs = conn.Query<RedirectJob>("SELECT * FROM RedirectJobs ORDER BY Name").ToList();
        foreach (var job in jobs)
        {
            job.Sources      = GetSources(job.Id);
            job.Destinations = GetDestinations(job.Id);
        }
        return jobs;
    }

    public RedirectJob? GetJob(int id)
    {
        using var conn = GetConnection();
        var job = conn.QuerySingleOrDefault<RedirectJob>("SELECT * FROM RedirectJobs WHERE Id=@id", new { id });
        if (job is null) return null;
        job.Sources      = GetSources(id);
        job.Destinations = GetDestinations(id);
        return job;
    }

    public int SaveJob(RedirectJob job)
    {
        using var conn = GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        if (job.Id == 0)
        {
            job.Id = conn.QuerySingle<int>("""
                INSERT INTO RedirectJobs (Name,IsEnabled,PollIntervalSeconds,SourceAction,MoveToPath,ProcessedSuffix,CreatedAt,LastRunAt,Notes)
                VALUES (@Name,@IsEnabled,@PollIntervalSeconds,@SourceAction,@MoveToPath,@ProcessedSuffix,@CreatedAt,@LastRunAt,@Notes);
                SELECT last_insert_rowid();
                """, job, tx);
        }
        else
        {
            conn.Execute("""
                UPDATE RedirectJobs SET
                    Name=@Name, IsEnabled=@IsEnabled, PollIntervalSeconds=@PollIntervalSeconds,
                    SourceAction=@SourceAction, MoveToPath=@MoveToPath, ProcessedSuffix=@ProcessedSuffix,
                    LastRunAt=@LastRunAt, Notes=@Notes
                WHERE Id=@Id
                """, job, tx);

            conn.Execute("DELETE FROM JobSources WHERE JobId=@Id", new { job.Id }, tx);
            conn.Execute("DELETE FROM JobDestinations WHERE JobId=@Id", new { job.Id }, tx);
        }

        foreach (var src in job.Sources)
        {
            src.JobId = job.Id;
            conn.Execute("""
                INSERT INTO JobSources (JobId,Path,FilePattern,PathType,Username,Password,IsPassive)
                VALUES (@JobId,@Path,@FilePattern,@PathType,@Username,@Password,@IsPassive)
                """, src, tx);
        }

        int order = 0;
        foreach (var dest in job.Destinations)
        {
            dest.JobId     = job.Id;
            dest.SortOrder = order++;
            conn.Execute("""
                INSERT INTO JobDestinations (JobId,Path,FileNameTemplate,PathType,SortOrder,Username,Password,IsPassive)
                VALUES (@JobId,@Path,@FileNameTemplate,@PathType,@SortOrder,@Username,@Password,@IsPassive)
                """, dest, tx);
        }

        tx.Commit();
        return job.Id;
    }

    public void DeleteJob(int id)
    {
        using var conn = GetConnection();
        conn.Execute("DELETE FROM RedirectJobs WHERE Id=@id", new { id });
    }

    public void UpdateJobLastRun(int jobId)
    {
        using var conn = GetConnection();
        conn.Execute("UPDATE RedirectJobs SET LastRunAt=@now WHERE Id=@jobId",
            new { now = DateTime.UtcNow.ToString("o"), jobId });
    }

    // ─── Sources / Destinations ───────────────────────────────────────────────

    public List<JobSource> GetSources(int jobId)
        => [.. GetConnection().Query<JobSource>("SELECT * FROM JobSources WHERE JobId=@jobId", new { jobId })];

    public List<JobDestination> GetDestinations(int jobId)
        => [.. GetConnection().Query<JobDestination>(
               "SELECT * FROM JobDestinations WHERE JobId=@jobId ORDER BY SortOrder", new { jobId })];

    // ─── Processed files tracking ─────────────────────────────────────────────

    public bool WasAlreadyProcessed(int jobId, string sourcePath, string fileName)
    {
        using var conn = GetConnection();
        return conn.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM ProcessedFiles WHERE JobId=@jobId AND SourcePath=@sourcePath AND FileName=@fileName AND Success=1",
            new { jobId, sourcePath, fileName }) > 0;
    }

    public void RecordProcessedFile(ProcessedFile record)
    {
        using var conn = GetConnection();
        conn.Execute("""
            INSERT INTO ProcessedFiles (JobId,SourcePath,FileName,FileSizeBytes,ProcessedAt,Success,ErrorMessage)
            VALUES (@JobId,@SourcePath,@FileName,@FileSizeBytes,@ProcessedAt,@Success,@ErrorMessage)
            """, record);
    }

    public List<ProcessedFile> GetRecentHistory(int jobId, int limit = 200)
    {
        using var conn = GetConnection();
        return [.. conn.Query<ProcessedFile>(
            "SELECT * FROM ProcessedFiles WHERE JobId=@jobId ORDER BY ProcessedAt DESC LIMIT @limit",
            new { jobId, limit })];
    }

    public List<ProcessedFile> GetAllHistory(int limit = 500)
    {
        using var conn = GetConnection();
        return [.. conn.Query<ProcessedFile>(
            "SELECT * FROM ProcessedFiles ORDER BY ProcessedAt DESC LIMIT @limit",
            new { limit })];
    }
}
