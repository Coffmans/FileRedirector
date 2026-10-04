using FileRedirector.Data;
using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.UI;

public partial class MainForm : Form, IActivitySink
{
    // ─── Services ─────────────────────────────────────────────────────────────
    private readonly DatabaseService    _db;
    private readonly ActorSystemManager _actorMgr;

    // Debounce timer — coalesces rapid per-file RefreshJobList calls into one update.
    private readonly System.Windows.Forms.Timer _refreshDebounceTimer;

    // ─── Runtime row colors for list highlighting ─────────────────────────────
    private static readonly Color C_GREEN = Color.FromArgb(0, 128, 0);
    private static readonly Color C_RED   = Color.FromArgb(180, 0, 0);
    private static readonly Color C_WARN  = Color.FromArgb(200, 120, 0);
    private static readonly Color C_TEXT  = SystemColors.WindowText;
    private static readonly Color C_MUTE  = SystemColors.GrayText;

    public MainForm()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"FileRedirector");
        Directory.CreateDirectory(appData);

        var dbPath   = Path.Combine(appData, "jobs.db");
        _db          = new DatabaseService(dbPath);
        _actorMgr    = new ActorSystemManager(_db, this);

        InitializeComponent();

        components ??= new System.ComponentModel.Container();   
        _refreshDebounceTimer = new System.Windows.Forms.Timer(components) { Interval = 1500 };
        _refreshDebounceTimer.Tick += (_, _) => { _refreshDebounceTimer.Stop(); RefreshJobList(); };

        lvJobs.DoubleClick             += (_, _) => EditJob();
        tabsRight.SelectedIndexChanged += TabsRight_SelectedIndexChanged;
    }

    // ─── Form events ──────────────────────────────────────────────────────────

    private void MainForm_Load(object sender, EventArgs e)
    {
        _actorMgr.Start();
        RefreshJobList();
        SetStatus("Actor system started.");
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _closing = true;   // drop any further activity from actors while shutting down
        _actorMgr.Dispose();
    }

    private void TabsRight_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (tabsRight.SelectedTab == tabHistory)
            RefreshHistory();
    }

    // ─── Toolbar handlers ─────────────────────────────────────────────────────

    private void BtnNewJob_Click(object sender, EventArgs e) => NewJob();
    private void BtnEdit_Click(object sender, EventArgs e)    => EditJob();
    private void BtnDelete_Click(object sender, EventArgs e)  => DeleteJob();
    private void BtnStart_Click(object sender, EventArgs e)   => StartJob();
    private void BtnStop_Click(object sender, EventArgs e)    => StopJob();
    private void BtnRefresh_Click(object sender, EventArgs e) => RefreshJobList();
    // ─── Job actions ──────────────────────────────────────────────────────────

    private void NewJob()
    {
        using var frm = new JobEditorForm();
        if (frm.ShowDialog(this) != DialogResult.OK) return;
        _db.SaveJob(frm.Result);
        if (frm.Result.IsEnabled)
            _actorMgr.StartJob(frm.Result);
        RefreshJobList();
        SetStatus($"Job '{frm.Result.Name}' created.");
    }

    private void EditJob()
    {
        var job = SelectedJob();
        if (job is null) return;
        using var frm = new JobEditorForm(job);
        if (frm.ShowDialog(this) != DialogResult.OK) return;
        _db.SaveJob(frm.Result);
        // Start (or restart with the new settings) if enabled; stop if it was disabled in the editor
        if (frm.Result.IsEnabled)
            _actorMgr.StartJob(frm.Result);
        else
            _actorMgr.StopJob(frm.Result.Id);
        RefreshJobList();
        SetStatus($"Job '{frm.Result.Name}' updated.");
    }

    private void DeleteJob()
    {
        var job = SelectedJob();
        if (job is null) return;
        if (MessageBox.Show($"Delete job '{job.Name}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        _actorMgr.StopJob(job.Id);
        _db.DeleteJob(job.Id);
        RefreshJobList();
        SetStatus($"Job '{job.Name}' deleted.");
    }

    private void StartJob()
    {
        var job = SelectedJob();
        if (job is null) return;
        job.IsEnabled = true;
        _db.SetJobEnabled(job.Id, true);
        _actorMgr.StartJob(job);
        RefreshJobList();
        SetStatus($"Job '{job.Name}' started.");
    }

    private void StopJob()
    {
        var job = SelectedJob();
        if (job is null) return;
        // Persist so the job stays stopped across restarts and shows as Disabled
        job.IsEnabled = false;
        _db.SetJobEnabled(job.Id, false);
        _actorMgr.StopJob(job.Id);
        RefreshJobList();
        SetStatus($"Job '{job.Name}' stopped.");
    }

    // ─── UI refresh ───────────────────────────────────────────────────────────

    private void RefreshJobList()
    {
        lvJobs.BeginUpdate();
        lvJobs.Items.Clear();
        foreach (var job in _db.GetAllJobs())
        {
            var item = new ListViewItem(job.Name) { Tag = job };
            item.SubItems.Add(job.IsEnabled ? "Active" : "Disabled");
            item.SubItems.Add($"{job.PollIntervalSeconds}s");
            item.SubItems.Add(job.LastRunAt is null ? "Never"
                : DateTime.Parse(job.LastRunAt).ToLocalTime().ToString("g"));
            item.SubItems.Add(job.Sources.Count.ToString());
            item.SubItems.Add(job.Destinations.Count.ToString());
            item.ForeColor = job.IsEnabled ? C_TEXT : C_MUTE;
            lvJobs.Items.Add(item);
        }
        lvJobs.EndUpdate();
    }

    private void RefreshHistory()
    {
        var records  = _db.GetAllHistory(500);
        var jobNames = _db.GetAllJobs().ToDictionary(j => j.Id, j => j.Name);
        lvHistory.BeginUpdate();
        lvHistory.Items.Clear();
        foreach (var r in records)
        {
            var time = DateTime.Parse(r.ProcessedAt).ToLocalTime().ToString("g");
            var item = new ListViewItem(time);
            item.SubItems.Add(jobNames.TryGetValue(r.JobId, out var name) ? name : r.JobId.ToString());
            item.SubItems.Add(r.FileName);
            item.SubItems.Add(FormatBytes(r.FileSizeBytes));
            var warn = r.Success && r.ErrorMessage is not null;
            item.SubItems.Add(!r.Success ? "✘ Fail" : warn ? "⚠ OK" : "✔ OK");
            item.SubItems.Add(r.ErrorMessage ?? "");
            item.ForeColor = !r.Success ? C_RED : warn ? C_WARN : C_GREEN;
            lvHistory.Items.Add(item);
        }
        lvHistory.EndUpdate();
    }

    // ─── IActivitySink (called from actor threads) ────────────────────────────
    // BeginInvoke never blocks the calling actor, so it can't deadlock with the UI thread waiting
    // on actor-system shutdown; anything arriving during/after close is dropped.

    private volatile bool _closing;

    void IActivitySink.Log(string message)            => PostToUi(() => AppendLog(message));
    void IActivitySink.CopyCompleted(FileCopyResult r) => PostToUi(() => OnCopyCompleted(r));

    private void PostToUi(Action action)
    {
        if (_closing || IsDisposed || !IsHandleCreated) return;
        try
        {
            BeginInvoke(() => { if (!_closing && !IsDisposed) action(); });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // Handle destroyed between the check and the call — nothing to show it on
        }
    }

    private void AppendLog(string msg)
    {
        const int maxChars = 50_000;
        if (rtbLog.TextLength > maxChars)
            rtbLog.Text = rtbLog.Text[^(maxChars / 2)..];
        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
        rtbLog.ScrollToCaret();
    }

    private void OnCopyCompleted(FileCopyResult res)
    {
        var color  = !res.Success ? C_RED : res.Warning is null ? C_GREEN : C_WARN;
        var symbol = !res.Success ? "✘" : res.Warning is null ? "✔" : "⚠";
        var detail = !res.Success ? res.Error : res.Warning ?? "copied OK";
        var msg    = $"{symbol} {res.File.FileName} — {detail}";
        rtbLog.SelectionStart  = rtbLog.TextLength;
        rtbLog.SelectionLength = 0;
        rtbLog.SelectionColor  = color;
        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
        rtbLog.SelectionColor  = rtbLog.ForeColor;
        rtbLog.ScrollToCaret();
        // Debounce: restart the timer so rapid completions collapse into one list refresh.
        _refreshDebounceTimer.Stop();
        _refreshDebounceTimer.Start();
    }

    private RedirectJob? SelectedJob()
        => lvJobs.SelectedItems.Count == 0 ? null : lvJobs.SelectedItems[0].Tag as RedirectJob;

    private void SetStatus(string msg) => lblStatus.Text = msg;

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024)         return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / 1024.0 / 1024.0:F1} MB";
    }
}
