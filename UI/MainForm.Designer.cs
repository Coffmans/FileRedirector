namespace FileRedirector.UI;

partial class MainForm
{
    /// <summary>Required designer variable.</summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>Clean up any resources being used.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        toolStrip    = new ToolStrip();
        btnNewJob    = new ToolStripButton();
        btnEdit      = new ToolStripButton();
        btnDelete    = new ToolStripButton();
        sep1         = new ToolStripSeparator();
        btnStart     = new ToolStripButton();
        btnStop      = new ToolStripButton();
        sep2         = new ToolStripSeparator();
        btnRefresh   = new ToolStripButton();
        pnlLeft      = new Panel();
        lblJobsHeader= new Label();
        lvJobs       = new ListView();
        colJobName   = new ColumnHeader();
        colStatus    = new ColumnHeader();
        colInterval  = new ColumnHeader();
        colLastRun   = new ColumnHeader();
        colSources   = new ColumnHeader();
        colDests     = new ColumnHeader();
        splitterMain = new Splitter();
        tabsRight    = new TabControl();
        tabLog       = new TabPage();
        rtbLog       = new RichTextBox();
        tabHistory   = new TabPage();
        lvHistory    = new ListView();
        colHTime     = new ColumnHeader();
        colHJob      = new ColumnHeader();
        colHFile     = new ColumnHeader();
        colHSize     = new ColumnHeader();
        colHResult   = new ColumnHeader();
        colHError    = new ColumnHeader();
        statusStrip  = new StatusStrip();
        lblStatus    = new ToolStripStatusLabel();

        toolStrip.SuspendLayout();
        pnlLeft.SuspendLayout();
        tabsRight.SuspendLayout();
        tabLog.SuspendLayout();
        tabHistory.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();

        // ── ToolStrip ─────────────────────────────────────────────────────────
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.Padding   = new Padding(4, 0, 0, 0);
        toolStrip.Dock      = DockStyle.Top;
        toolStrip.Items.AddRange(new ToolStripItem[]
        {
            btnNewJob, btnEdit, btnDelete, sep1, btnStart, btnStop, sep2, btnRefresh
        });

        btnNewJob.Text         = "＋ New Job";
        btnNewJob.BackColor    = Color.FromArgb(198, 230, 255);
        btnNewJob.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnNewJob.Margin       = new Padding(2);
        btnNewJob.Click       += BtnNewJob_Click;

        btnEdit.Text         = "✎ Edit";
        btnEdit.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnEdit.Margin       = new Padding(2);
        btnEdit.Click       += BtnEdit_Click;

        btnDelete.Text         = "✕ Delete";
        btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnDelete.Margin       = new Padding(2);
        btnDelete.Click       += BtnDelete_Click;

        btnStart.Text         = "▶ Start";
        btnStart.BackColor    = Color.FromArgb(198, 239, 206);
        btnStart.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnStart.Margin       = new Padding(2);
        btnStart.Click       += BtnStart_Click;

        btnStop.Text         = "⏹ Stop";
        btnStop.BackColor    = Color.FromArgb(255, 199, 206);
        btnStop.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnStop.Margin       = new Padding(2);
        btnStop.Click       += BtnStop_Click;

        btnRefresh.Text         = "⟳ Refresh";
        btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnRefresh.Margin       = new Padding(2);
        btnRefresh.Click       += BtnRefresh_Click;

        // ── Left panel ────────────────────────────────────────────────────────
        pnlLeft.Dock    = DockStyle.Left;
        pnlLeft.Padding = new Padding(0, 0, 4, 0);
        pnlLeft.Width   = 460;
        pnlLeft.Controls.Add(lvJobs);
        pnlLeft.Controls.Add(lblJobsHeader);

        lblJobsHeader.Text      = "REDIRECT JOBS";
        lblJobsHeader.ForeColor = SystemColors.GrayText;
        lblJobsHeader.Dock      = DockStyle.Top;
        lblJobsHeader.Height    = 24;
        lblJobsHeader.TextAlign = ContentAlignment.MiddleLeft;
        lblJobsHeader.Padding   = new Padding(8, 0, 0, 0);
        lblJobsHeader.Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold);

        lvJobs.Dock          = DockStyle.Fill;
        lvJobs.View          = View.Details;
        lvJobs.FullRowSelect = true;
        lvJobs.GridLines     = true;
        lvJobs.MultiSelect   = false;
        lvJobs.HideSelection = false;
        lvJobs.Columns.AddRange(new ColumnHeader[]
        {
            colJobName, colStatus, colInterval, colLastRun, colSources, colDests
        });

        colJobName.Text  = "Job Name";  colJobName.Width  = 180;
        colStatus.Text   = "Status";    colStatus.Width   = 80;
        colInterval.Text = "Interval";  colInterval.Width = 70;
        colLastRun.Text  = "Last Run";  colLastRun.Width  = 140;
        colSources.Text  = "Sources";   colSources.Width  = 50;
        colDests.Text    = "Dests";     colDests.Width    = 50;

        // ── Splitter ──────────────────────────────────────────────────────────
        splitterMain.Dock  = DockStyle.Left;
        splitterMain.Width = 4;

        // ── Tab control ───────────────────────────────────────────────────────
        tabsRight.Dock = DockStyle.Fill;
        tabsRight.TabPages.AddRange(new TabPage[] { tabLog, tabHistory });

        tabLog.Text = "Activity Log";
        tabLog.Controls.Add(rtbLog);

        rtbLog.Dock       = DockStyle.Fill;
        rtbLog.ReadOnly   = true;
        rtbLog.Font       = new Font("Consolas", 8.5f);
        rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;

        tabHistory.Text = "File History";
        tabHistory.Controls.Add(lvHistory);

        lvHistory.Dock          = DockStyle.Fill;
        lvHistory.View          = View.Details;
        lvHistory.FullRowSelect = true;
        lvHistory.GridLines     = true;
        lvHistory.HideSelection = false;
        lvHistory.Columns.AddRange(new ColumnHeader[]
        {
            colHTime, colHJob, colHFile, colHSize, colHResult, colHError
        });

        colHTime.Text   = "Time";    colHTime.Width   = 140;
        colHJob.Text    = "Job";     colHJob.Width    = 100;
        colHFile.Text   = "File";    colHFile.Width   = 200;
        colHSize.Text   = "Size";    colHSize.Width   = 70;
        colHResult.Text = "Result";  colHResult.Width = 70;
        colHError.Text  = "Error";   colHError.Width  = 260;

        // ── StatusStrip ───────────────────────────────────────────────────────
        statusStrip.Items.Add(lblStatus);
        lblStatus.Text = "Ready";

        // ── Form ─────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(1280, 800);
        MinimumSize         = new Size(900, 600);
        Font                = new Font("Segoe UI", 9F);
        StartPosition       = FormStartPosition.CenterScreen;
        Text                = "File Redirector";
        Load               += MainForm_Load;
        FormClosing        += MainForm_FormClosing;

        Controls.Add(tabsRight);
        Controls.Add(splitterMain);
        Controls.Add(pnlLeft);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);

        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        pnlLeft.ResumeLayout(false);
        tabsRight.ResumeLayout(false);
        tabLog.ResumeLayout(false);
        tabHistory.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private ToolStrip            toolStrip;
    private ToolStripButton      btnNewJob;
    private ToolStripButton      btnEdit;
    private ToolStripButton      btnDelete;
    private ToolStripSeparator   sep1;
    private ToolStripButton      btnStart;
    private ToolStripButton      btnStop;
    private ToolStripSeparator   sep2;
    private ToolStripButton      btnRefresh;
    private Panel                pnlLeft;
    private Label                lblJobsHeader;
    private ListView             lvJobs;
    private ColumnHeader         colJobName;
    private ColumnHeader         colStatus;
    private ColumnHeader         colInterval;
    private ColumnHeader         colLastRun;
    private ColumnHeader         colSources;
    private ColumnHeader         colDests;
    private Splitter             splitterMain;
    private TabControl           tabsRight;
    private TabPage              tabLog;
    private RichTextBox          rtbLog;
    private TabPage              tabHistory;
    private ListView             lvHistory;
    private ColumnHeader         colHTime;
    private ColumnHeader         colHJob;
    private ColumnHeader         colHFile;
    private ColumnHeader         colHSize;
    private ColumnHeader         colHResult;
    private ColumnHeader         colHError;
    private StatusStrip          statusStrip;
    private ToolStripStatusLabel lblStatus;
}
