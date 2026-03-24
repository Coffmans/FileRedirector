# FileRedirector

A Windows WinForms application that monitors source locations and copies files to multiple destinations using **Akka.NET** for concurrent actor-based processing.

---

## Features

- **Any-protocol sources & destinations**: Local paths, UNC (`\\server\share`), HTTP/S, FTP/FTPS  
- **Multiple sources and destinations** per job — all stored in a local SQLite database  
- **Concurrent copying** via an Akka.NET round-robin actor pool (default: 4 parallel workers)  
- **Flexible post-copy source handling**:
  - `Leave` — file stays; DB tracks processed files to avoid re-processing  
  - `Delete` — remove source file after successful copy  
  - `Move` — relocate source file to an archive folder  
  - `MarkProcessed` — rename with a configurable suffix (e.g. `.done`)  
- **Rich `@`-wildcard system** for paths, file patterns, and filename templates  
- **Per-job polling interval** (default: 30 seconds)  
- **Dark-themed UI** with live activity log and file history tab  

---

## Wildcard Tokens

Use these tokens in **source paths**, **destination paths**, and **filename templates**:

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
| Rename with timestamp             | `@FILE_@YYYY@MM@DD_@HH@MIN.@EXT`           |
| FTP path by quarter               | `ftp://host/data/Q@QTR_@YYYY/`             |
| File pattern for current month    | `Report_@MM@DD*.csv`                        |

---

## Architecture

```
MainForm (WinForms UI)
    │
    └── ActorSystemManager
            │
            └── CoordinatorActor  (Akka top-level supervisor)
                    │
                    ├── DirectoryMonitorActor  × N  (one per job)
                    │       Polls source(s) on configurable interval
                    │       → sends FilesDiscovered to Coordinator
                    │
                    └── FileCopyActor pool  (RoundRobinPool, 4 workers)
                            Reads from source, writes to all destinations
                            Applies source-file action
                            Records result in SQLite
```

### Actor Messages

| Message            | Direction                          | Purpose                                |
|--------------------|------------------------------------|----------------------------------------|
| `StartMonitoring`  | Coordinator → Monitor              | Activate polling for a job             |
| `StopMonitoring`   | Coordinator → Monitor              | Halt polling                           |
| `PollNow`          | Timer → Monitor (self)             | Trigger one scan cycle                 |
| `FilesDiscovered`  | Monitor → Coordinator              | New files found, dedup & dispatch      |
| `CopyFile`         | Coordinator → Pool worker          | Do one file copy operation             |
| `FileCopyResult`   | Pool worker → Coordinator          | Outcome; triggers UI update            |

---

## Avoiding Duplicate Processing

The application uses a **layered deduplication** strategy:

1. **MarkProcessed** — rename source file with suffix (`.done`); scanner pattern won't match it again  
2. **Delete / Move** — source file is gone after successful copy  
3. **Leave** — a `ProcessedFiles` SQLite table records every successfully copied file; the coordinator filters already-seen entries before dispatching copy work  

All three modes guarantee a file is not processed more than once even across restarts.

---

## Prerequisites

- **Windows 10/11** (WinForms requires Windows)
- **.NET 8.0 Desktop Runtime** or **SDK** — [download](https://dotnet.microsoft.com/download/dotnet/8)

---

## Building

```powershell
git clone <repo>
cd FileRedirector
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained false
```

Or open `FileRedirector.csproj` in **Visual Studio 2026+** and press **F5**.

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
Tokens are matched **longest-first** to avoid partial collisions.

### Increasing copy parallelism

In `Actors/CoordinatorActor.cs`, change the pool size:
```csharp
.WithRouter(new RoundRobinPool(8))  // 8 concurrent copy workers
```

### Supporting a new protocol

Implement the read/write methods in `Services/FileTransferService.cs` and add the new value to the `PathType` enum in `Models/Models.cs`.
