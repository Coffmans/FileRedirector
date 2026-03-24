using FileRedirector.Actors;
using FileRedirector.Data;
using FileRedirector.Models;
using FileRedirector.Services;

namespace FileRedirector.UI;

public partial class MainForm : Form
{
    // ─── Services ─────────────────────────────────────────────────────────────
    private readonly DatabaseService    _db;
    private readonly ActorSystemManager _actorMgr;

    // ─── Runtime row colors for list highlighting ─────────────────────────────
    private static readonly Color C_GREEN = Color.FromArgb(0, 128, 0);
    private static readonly Color C_RED   = Color.FromArgb(180, 0, 0);
    private static readonly Color C_TEXT  = SystemColors.WindowText;
    private static readonly Color C_MUTE  = SystemColors.GrayText;

    public MainForm()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileRedirector");
        Directory.CreateDirectory(appData);

        var dbPath   = Path.Combine(appData, "jobs.db");
        _db          = new DatabaseService(dbPath);
        var transfer = new FileTransferService();
        _actorMgr    = new ActorSystemManager(_db, transfer);

        InitializeComponent();

        lvJobs.DoubleClick             += (_, _) => EditJob();
        tabsRight.SelectedIndexChanged += TabsRight_SelectedIndexChanged;

        CoordinatorActor.LogMessage    += msg => SafeAppendLog(msg);
        CoordinatorActor.CopyCompleted += res => SafeOnCopyCompleted(res);
    }

    // ─── Form events ──────────────────────────────────────────────────────────

    private void MainForm_Load(object sender, EventArgs e)
    {
        _actorMgr.Start();
        RefreshJobList();
        SetStatus("Actor system started.");
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        => _actorMgr.Dispose();

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
        _actorMgr.RefreshJob(frm.Result);
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
        _db.SaveJob(job);
        _actorMgr.StartJob(job);
        RefreshJobList();
        SetStatus($"Job '{job.Name}' started.");
    }

    private void StopJob()
    {
        var job = SelectedJob();
        if (job is null) return;
        _actorMgr.StopJob(job.Id);
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
        var records = _db.GetAllHistory(500);
        lvHistory.BeginUpdate();
        lvHistory.Items.Clear();
        foreach (var r in records)
        {
            var time = DateTime.Parse(r.ProcessedAt).ToLocalTime().ToString("g");
            var job  = _db.GetJob(r.JobId);
            var item = new ListViewItem(time);
            item.SubItems.Add(job?.Name ?? r.JobId.ToString());
            item.SubItems.Add(r.FileName);
            item.SubItems.Add(FormatBytes(r.FileSizeBytes));
            item.SubItems.Add(r.Success ? "✔ OK" : "✘ Fail");
            item.SubItems.Add(r.ErrorMessage ?? "");
            item.ForeColor = r.Success ? C_GREEN : C_RED;
            lvHistory.Items.Add(item);
        }
        lvHistory.EndUpdate();
    }

    private void SafeAppendLog(string msg)
    {
        if (InvokeRequired) { Invoke(() => SafeAppendLog(msg)); return; }
        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
        rtbLog.ScrollToCaret();
    }

    private void SafeOnCopyCompleted(FileCopyResult res)
    {
        if (InvokeRequired) { Invoke(() => SafeOnCopyCompleted(res)); return; }
        var color  = res.Success ? C_GREEN : C_RED;
        var symbol = res.Success ? "✔" : "✘";
        var msg    = $"{symbol} {res.File.FileName} — {(res.Success ? "copied OK" : res.Error)}";
        rtbLog.SelectionStart  = rtbLog.TextLength;
        rtbLog.SelectionLength = 0;
        rtbLog.SelectionColor  = color;
        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
        rtbLog.SelectionColor  = rtbLog.ForeColor;
        rtbLog.ScrollToCaret();
        RefreshJobList();
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
