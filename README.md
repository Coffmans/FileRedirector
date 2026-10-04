# FileRedirector

A Windows WinForms application that monitors source locations and copies files to multiple destinations using **Akka.NET** for concurrent actor-based processing.

---

## Features

- **Sources**: Local paths, UNC (`\\server\share`), FTP/FTPS
- **Destinations**: Local paths, UNC, FTP/FTPS, HTTP/HTTPS (`PUT`)
- **Multiple sources and destinations** per job — all stored in a local SQLite database
- **Concurrent copying** via an Akka.NET round-robin actor pool (default: 4 parallel workers)
- **Streaming transfers** — files are never loaded fully into memory, so large files are fine
- **Safe delivery** — local and FTP destinations are written to a temporary `.partial` name and renamed into place when complete
- **Flexible post-copy source handling** (local and FTP sources):
  - `Leave` — file stays; the DB tracks what was delivered to avoid re-processing
  - `Delete` — remove source file after successful copy
  - `Move` — relocate source file to an archive folder
  - `MarkProcessed` — rename with a configurable suffix (e.g. `.done`)
- **Rich `@`-wildcard system** for paths, file patterns, and filename templates
- **Per-job polling interval** (default: 30 seconds)
- **Live activity log** and file history tab in the UI, plus daily log files on disk
- **Encrypted credentials** — passwords are stored with Windows DPAPI

> HTTP/HTTPS can't be used as a *source*, because HTTP has no standard directory listing.

---

## Wildcard Tokens

Use these tokens in **source paths**, **destination paths**, **file patterns**, and **filename templates**.
Tokens are case-insensitive; the longest matching token always wins (so `@MIN` is never read as `@M` + `IN`).

| Token      | Description                          | Example        |
|------------|--------------------------------------|----------------|
| `@YYYY`    | 4-digit year                         | `2025`         |
| `@YY`      | 2-digit year                         | `25`           |
| `@MM`      | Zero-padded month                    | `04`           |
| `@M`       | Month (no padding)                   | `4`            |
| `@MNAME`   | Full month name                      | `April`        |
| `@MABB`    | 3-letter month abbreviation          | `Apr`          |
| `@DD`      | Zero-padded day                      | `07`           |
| `@D`       | Day (no padding)                     | `7`            |
| `@HH`      | Zero-padded hour (24h)               | `09`           |
| `@H`       | Hour (no padding)                    | `9`            |
| `@MIN`     | Zero-padded minute                   | `05`           |
| `@SS`      | Zero-padded second                   | `42`           |
| `@DOW`     | Day of week number (0=Sun … 6=Sat)   | `3`            |
| `@DOWS`    | Full day name                        | `Wednesday`    |
| `@DOWA`    | 3-letter day abbreviation            | `Wed`          |
| `@WOY`     | ISO week of year                     | `14`           |
| `@QTR`     | Quarter                              | `2`            |
| `@TICK`    | DateTime.UtcNow.Ticks (unique)       | `638765432100` |
| `@GUID`    | Short 8-character GUID hex           | `3f2a1c8b`     |
| `@FILE`    | Original filename without extension  | `Report`       |
| `@EXT`     | Original file extension (no dot)     | `pdf`          |
| `@ORIGNAME`| Full original filename with extension| `Report.pdf`   |

### Examples

| Use Case                          | Template                                    |
|-----------------------------------|---------------------------------------------|
| Daily archive folder              | `D:\Archive\@YYYY\@MM\@DD`                  |
| Month-named destination           | `\\server\reports\@MNAME @YYYY`             |
| Rename with timestamp             | `@FILE_@YYYY@MM@DD_@HH@MIN.@EXT`            |
| FTP path by quarter               | `ftp://host/data/Q@QTR_@YYYY/`              |
| File pattern for current month    | `Report_@MM@DD*.csv`                        |

---

## Architecture

```
MainForm (WinForms UI, implements IActivitySink)
    │
    └── ActorSystemManager
            │
            └── CoordinatorActor  (Akka top-level supervisor)
                    │
                    ├── DirectoryMonitorActor  × N  (one per running job)
                    │       Polls source(s) on configurable interval
                    │       → sends FilesDiscovered / ScanFailed to Coordinator
                    │
                    └── FileCopyActor pool  (RoundRobinPool, 4 workers)
                            Streams from source to all destinations
                            Applies source-file action
                            Records result in SQLite
```

The actors report log lines and copy results to the UI through `IActivitySink`; the UI marshals them onto its own thread without blocking the actors.

### Actor Messages

| Message            | Direction                          | Purpose                                         |
|--------------------|------------------------------------|-------------------------------------------------|
| `StartMonitoring`  | Coordinator → Monitor              | Start (or restart with new settings) polling    |
| `StopMonitoring`   | Monitor → self                     | Halt polling when the job is disabled in the DB |
| `PollNow`          | Timer → Monitor (self)             | Trigger one scan cycle                          |
| `FilesDiscovered`  | Monitor → Coordinator              | Scan results; dedup & dispatch                  |
| `ScanFailed`       | Monitor → Coordinator              | Scan error, shown in the activity log           |
| `CopyFile`         | Coordinator → Pool worker          | Do one file copy operation                      |
| `FileCopyResult`   | Pool worker → Coordinator          | Outcome; triggers UI update                     |

Stopping a job stops its monitor actor and saves the job as disabled, so it stays stopped across restarts.

---

## Avoiding Duplicate Processing

Before a discovered file is copied, the coordinator checks, in order:

1. **Already in progress** — a file that is still being copied is never dispatched again, even if a new scan finds it.
2. **Already delivered** — the `ProcessedFiles` table records each delivery with the file's name, size and modified time. The same unchanged file is never delivered twice, even if the source action (delete/move/rename) failed. In `Leave` mode, a file that is replaced or updated in place *is* copied again.
3. **Stable** — the file must have the same size and modified time on two consecutive scans, and (for local/UNC sources) must not be open for writing by another program. This avoids copying half-written files; it means a new file is picked up about one poll interval after it appears.

After a successful copy, `Delete` / `Move` / `MarkProcessed` remove the file from the scan. `Move` and `MarkProcessed` never overwrite an existing file — a ` (1)`, ` (2)`… suffix is added instead.

If every destination succeeds but the source action fails, the copy is recorded as delivered **with a warning** (⚠ in the log and history).

---

## Credentials & Security

- **Passwords** are encrypted with Windows DPAPI (current-user scope) before being stored. Only the Windows account that saved them can decrypt them; if the app is run under another account, re-enter the passwords in the job editor.
- **FTPS certificates** must be valid and trusted by Windows. For a server with a self-signed certificate on a trusted network, tick **Trust Any Cert (FTPS)** for that source/destination. (Jobs created by earlier versions have this ticked for existing FTPS entries, to keep them working — untick it where the server has a proper certificate.)

---

## Source Actions for FTP

For FTP/FTPS sources, the **Move to** path is a folder on the *same* FTP server — either a remote path such as `/archive/@YYYY` or a full `ftp://host/archive` URL.

---

## Data & Logs

| What              | Where                                              |
|-------------------|----------------------------------------------------|
| Jobs & history    | `%APPDATA%\FileRedirector\jobs.db` (SQLite, WAL)   |
| Log files         | `%APPDATA%\FileRedirector\logs\FileRedirector-yyyyMMdd.log` (kept 30 days) |
| Download staging  | `%TEMP%\FileRedirector` (FTP/HTTP sources, deleted after each copy) |

Deleting a job also deletes its copy history.

---

## Prerequisites

- **Windows 10/11** (WinForms requires Windows)
- **.NET 8.0 Desktop Runtime** or **SDK** — [download](https://dotnet.microsoft.com/download/dotnet/8)

---

## Building & Testing

```powershell
git clone <repo>
cd FileRedirector
dotnet build -c Release
dotnet test
dotnet publish FileRedirector.csproj -c Release -r win-x64 --self-contained false
```

Or open `FileRedirector.slnx` in **Visual Studio 2026+** and press **F5**.

---

## NuGet Dependencies

| Package                                 | Purpose                              |
|-----------------------------------------|--------------------------------------|
| `Akka` 1.5.27                           | Actor system core                    |
| `Akka.DependencyInjection` 1.5.27       | DI integration for actors            |
| `Microsoft.Data.Sqlite` 9.x             | SQLite ADO.NET driver                |
| `Dapper` 2.x                            | Micro-ORM for SQLite queries         |
| `FluentFTP` 54.x                        | Async FTP/FTPS client                |
| `Microsoft.Extensions.DependencyInjection` | Service registration              |

---

## Extending

### Adding a new wildcard token

Edit `Wildcards/WildcardEngine.cs` and add a tuple to the `Tokens` array:
```csharp
("@MYTOKEN", (dt, fn) => /* your logic */),
```
Order in the array doesn't matter — tokens are always matched longest-first.

### Increasing copy parallelism

In `Actors/CoordinatorActor.cs`, change the pool size:
```csharp
.WithRouter(new RoundRobinPool(8))  // 8 concurrent copy workers
```

### Supporting a new protocol

Implement the list/stage/write methods in `Services/FileTransferService.cs` and add the new value to the `PathType` enum in `Models/Models.cs`.
