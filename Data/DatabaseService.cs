using Dapper;
using Microsoft.Data.Sqlite;
using FileRedirector.Models;

namespace FileRedirector.Data;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource     = dbPath,
            ForeignKeys    = true,   // enforce ON DELETE CASCADE regardless of the SQLite build's default
            DefaultTimeout = 30      // seconds to wait on a locked database before failing
        }.ToString();
        InitializeDatabase();
    }

    private SqliteConnection GetConnection() => new(_connectionString);

    private void InitializeDatabase()
    {
        using var conn = GetConnection();
        conn.Open();

        // WAL lets the UI read while copy workers write; the setting is stored in the database file
        conn.Execute("PRAGMA journal_mode=WAL;");

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
                IsPassive INTEGER NOT NULL DEFAULT 1,
                AcceptAnyCertificate INTEGER NOT NULL DEFAULT 0
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
                IsPassive INTEGER NOT NULL DEFAULT 1,
                AcceptAnyCertificate INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS ProcessedFiles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                JobId INTEGER NOT NULL,
                SourcePath TEXT NOT NULL,
                FileName TEXT NOT NULL,
                FileSizeBytes INTEGER NOT NULL DEFAULT 0,
                SourceModifiedTicks INTEGER,
                ProcessedAt TEXT NOT NULL,
                Success INTEGER NOT NULL DEFAULT 1,
                ErrorMessage TEXT
            );

            CREATE INDEX IF NOT EXISTS IX_ProcessedFiles_JobId_FileName
                ON ProcessedFiles(JobId, FileName, SourcePath);

            CREATE INDEX IF NOT EXISTS IX_ProcessedFiles_ProcessedAt
                ON ProcessedFiles(ProcessedAt);
            """);

        // Migrations for databases created by earlier versions
        var processedCols = conn.Query<string>("SELECT name FROM pragma_table_info('ProcessedFiles')").ToList();
        if (!processedCols.Contains("SourceModifiedTicks"))
            conn.Execute("ALTER TABLE ProcessedFiles ADD COLUMN SourceModifiedTicks INTEGER");

        foreach (var table in new[] { "JobSources", "JobDestinations" })
        {
            var cols = conn.Query<string>($"SELECT name FROM pragma_table_info('{table}')").ToList();
            if (!cols.Contains("AcceptAnyCertificate"))
            {
                conn.Execute($"ALTER TABLE {table} ADD COLUMN AcceptAnyCertificate INTEGER NOT NULL DEFAULT 0");
                // Earlier versions accepted any FTPS certificate; keep existing FTPS entries working.
                // (PathType may be stored as the enum name or its number: FTPS = 4.)
                conn.Execute($"UPDATE {table} SET AcceptAnyCertificate=1 WHERE PathType IN ('FTPS','4')");
            }

            // Encrypt passwords saved in plain text by earlier versions
            var plain = conn.Query<(long Id, string Password)>(
                $"SELECT Id, Password FROM {table} WHERE Password IS NOT NULL AND Password <> '' " +
                $"AND Password NOT LIKE '{SecretProtector.Prefix}%'");
            foreach (var (id, password) in plain)
                conn.Execute($"UPDATE {table} SET Password=@p WHERE Id=@id",
                             new { p = SecretProtector.Protect(password), id });
        }
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
                    Notes=@Notes
                WHERE Id=@Id
                """, job, tx);

            conn.Execute("DELETE FROM JobSources WHERE JobId=@Id", new { job.Id }, tx);
            conn.Execute("DELETE FROM JobDestinations WHERE JobId=@Id", new { job.Id }, tx);
        }

        foreach (var src in job.Sources)
        {
            src.JobId = job.Id;
            conn.Execute("""
                INSERT INTO JobSources (JobId,Path,FilePattern,PathType,Username,Password,IsPassive,AcceptAnyCertificate)
                VALUES (@JobId,@Path,@FilePattern,@PathType,@Username,@Password,@IsPassive,@AcceptAnyCertificate)
                """, new
                {
                    src.JobId, src.Path, src.FilePattern, src.PathType, src.Username,
                    Password = SecretProtector.Protect(src.Password),
                    src.IsPassive, src.AcceptAnyCertificate
                }, tx);
        }

        int order = 0;
        foreach (var dest in job.Destinations)
        {
            dest.JobId     = job.Id;
            dest.SortOrder = order++;
            conn.Execute("""
                INSERT INTO JobDestinations (JobId,Path,FileNameTemplate,PathType,SortOrder,Username,Password,IsPassive,AcceptAnyCertificate)
                VALUES (@JobId,@Path,@FileNameTemplate,@PathType,@SortOrder,@Username,@Password,@IsPassive,@AcceptAnyCertificate)
                """, new
                {
                    dest.JobId, dest.Path, dest.FileNameTemplate, dest.PathType, dest.SortOrder, dest.Username,
                    Password = SecretProtector.Protect(dest.Password),
                    dest.IsPassive, dest.AcceptAnyCertificate
                }, tx);
        }

        tx.Commit();
        return job.Id;
    }

    /// <summary>Deletes the job, its sources/destinations and its copy history.</summary>
    public void DeleteJob(int id)
    {
        using var conn = GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        conn.Execute("DELETE FROM ProcessedFiles  WHERE JobId=@id", new { id }, tx);
        conn.Execute("DELETE FROM JobSources      WHERE JobId=@id", new { id }, tx);
        conn.Execute("DELETE FROM JobDestinations WHERE JobId=@id", new { id }, tx);
        conn.Execute("DELETE FROM RedirectJobs    WHERE Id=@id",    new { id }, tx);
        tx.Commit();
    }

    public void SetJobEnabled(int jobId, bool enabled)
    {
        using var conn = GetConnection();
        conn.Execute("UPDATE RedirectJobs SET IsEnabled=@enabled WHERE Id=@jobId", new { enabled, jobId });
    }

    public void UpdateJobLastRun(int jobId)
    {
        using var conn = GetConnection();
        conn.Execute("UPDATE RedirectJobs SET LastRunAt=@now WHERE Id=@jobId",
            new { now = DateTime.UtcNow.ToString("o"), jobId });
    }

    // ─── Sources / Destinations ───────────────────────────────────────────────

    public List<JobSource> GetSources(int jobId)
    {
        using var conn = GetConnection();
        var sources = conn.Query<JobSource>("SELECT * FROM JobSources WHERE JobId=@jobId", new { jobId }).ToList();
        foreach (var s in sources)
            s.Password = SecretProtector.Unprotect(s.Password);
        return sources;
    }

    public List<JobDestination> GetDestinations(int jobId)
    {
        using var conn = GetConnection();
        var dests = conn.Query<JobDestination>(
            "SELECT * FROM JobDestinations WHERE JobId=@jobId ORDER BY SortOrder", new { jobId }).ToList();
        foreach (var d in dests)
            d.Password = SecretProtector.Unprotect(d.Password);
        return dests;
    }

    // ─── Processed files tracking ─────────────────────────────────────────────

    /// <summary>
    /// True if this exact file version (same name, size and modified time) was already delivered.
    /// </summary>
    /// <param name="matchLegacyRows">
    /// Also accept rows written before modified times were recorded, matching on name + size only.
    /// </param>
    public bool WasAlreadyProcessed(int jobId, string sourcePath, string fileName, long sizeBytes, long modifiedTicks,
                                    bool matchLegacyRows = false)
    {
        using var conn = GetConnection();
        return conn.ExecuteScalar<int>("""
            SELECT COUNT(1) FROM ProcessedFiles
            WHERE JobId=@jobId AND SourcePath=@sourcePath AND FileName=@fileName AND Success=1
              AND FileSizeBytes=@sizeBytes
              AND (SourceModifiedTicks=@modifiedTicks OR (@matchLegacyRows AND SourceModifiedTicks IS NULL))
            """,
            new { jobId, sourcePath, fileName, sizeBytes, modifiedTicks, matchLegacyRows }) > 0;
    }

    public void RecordProcessedFile(ProcessedFile record)
    {
        using var conn = GetConnection();
        conn.Execute("""
            INSERT INTO ProcessedFiles (JobId,SourcePath,FileName,FileSizeBytes,SourceModifiedTicks,ProcessedAt,Success,ErrorMessage)
            VALUES (@JobId,@SourcePath,@FileName,@FileSizeBytes,@SourceModifiedTicks,@ProcessedAt,@Success,@ErrorMessage)
            """, record);
    }

    public List<ProcessedFile> GetAllHistory(int limit = 500)
    {
        using var conn = GetConnection();
        return [.. conn.Query<ProcessedFile>(
            "SELECT * FROM ProcessedFiles ORDER BY ProcessedAt DESC LIMIT @limit",
            new { limit })];
    }
}
