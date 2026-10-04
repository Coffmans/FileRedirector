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
        toolStrip = new ToolStrip();
        btnNewJob = new ToolStripButton();
        btnEdit = new ToolStripButton();
        btnDelete = new ToolStripButton();
        sep1 = new ToolStripSeparator();
        btnStart = new ToolStripButton();
        btnStop = new ToolStripButton();
        sep2 = new ToolStripSeparator();
        btnRefresh = new ToolStripButton();
        pnlLeft = new Panel();
        lvJobs = new ListView();
        colJobName = new ColumnHeader();
        colStatus = new ColumnHeader();
        colInterval = new ColumnHeader();
        colLastRun = new ColumnHeader();
        colSources = new ColumnHeader();
        colDests = new ColumnHeader();
        lblJobsHeader = new Label();
        splitterMain = new Splitter();
        tabsRight = new TabControl();
        tabLog = new TabPage();
        rtbLog = new RichTextBox();
        tabHistory = new TabPage();
        lvHistory = new ListView();
        colHTime = new ColumnHeader();
        colHJob = new ColumnHeader();
        colHFile = new ColumnHeader();
        colHSize = new ColumnHeader();
        colHResult = new ColumnHeader();
        colHError = new ColumnHeader();
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        toolStrip.SuspendLayout();
        pnlLeft.SuspendLayout();
        tabsRight.SuspendLayout();
        tabLog.SuspendLayout();
        tabHistory.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolStrip
        // 
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.Items.AddRange(new ToolStripItem[] { btnNewJob, btnEdit, btnDelete, sep1, btnStart, btnStop, sep2, btnRefresh });
        toolStrip.Location = new Point(0, 0);
        toolStrip.Name = "toolStrip";
        toolStrip.Padding = new Padding(4, 0, 0, 0);
        toolStrip.Size = new Size(1280, 25);
        toolStrip.TabIndex = 3;
        // 
        // btnNewJob
        // 
        btnNewJob.BackColor = Color.FromArgb(198, 230, 255);
        btnNewJob.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnNewJob.Margin = new Padding(2);
        btnNewJob.Name = "btnNewJob";
        btnNewJob.Size = new Size(71, 21);
        btnNewJob.Text = "＋ New Job";
        btnNewJob.Click += BtnNewJob_Click;
        // 
        // btnEdit
        // 
        btnEdit.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnEdit.Margin = new Padding(2);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new Size(46, 21);
        btnEdit.Text = "✎ Edit";
        btnEdit.Click += BtnEdit_Click;
        // 
        // btnDelete
        // 
        btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnDelete.Margin = new Padding(2);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(57, 21);
        btnDelete.Text = "✕ Delete";
        btnDelete.Click += BtnDelete_Click;
        // 
        // sep1
        // 
        sep1.Name = "sep1";
        sep1.Size = new Size(6, 25);
        // 
        // btnStart
        // 
        btnStart.BackColor = Color.FromArgb(198, 239, 206);
        btnStart.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnStart.Margin = new Padding(2);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(48, 21);
        btnStart.Text = "▶ Start";
        btnStart.Click += BtnStart_Click;
        // 
        // btnStop
        // 
        btnStop.BackColor = Color.FromArgb(255, 199, 206);
        btnStop.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnStop.Margin = new Padding(2);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(50, 21);
        btnStop.Text = "⏹ Stop";
        btnStop.Click += BtnStop_Click;
        // 
        // sep2
        // 
        sep2.Name = "sep2";
        sep2.Size = new Size(6, 25);
        // 
        // btnRefresh
        // 
        btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnRefresh.Margin = new Padding(2);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(63, 21);
        btnRefresh.Text = "⟳ Refresh";
        btnRefresh.Click += BtnRefresh_Click;
        // 
        // pnlLeft
        // 
        pnlLeft.Controls.Add(lvJobs);
        pnlLeft.Controls.Add(lblJobsHeader);
        pnlLeft.Dock = DockStyle.Left;
        pnlLeft.Location = new Point(0, 25);
        pnlLeft.Name = "pnlLeft";
        pnlLeft.Padding = new Padding(0, 0, 4, 0);
        pnlLeft.Size = new Size(460, 753);
        pnlLeft.TabIndex = 2;
        // 
        // lvJobs
        // 
        lvJobs.Columns.AddRange(new ColumnHeader[] { colJobName, colStatus, colInterval, colLastRun, colSources, colDests });
        lvJobs.Dock = DockStyle.Fill;
        lvJobs.FullRowSelect = true;
        lvJobs.GridLines = true;
        lvJobs.Location = new Point(0, 24);
        lvJobs.MultiSelect = false;
        lvJobs.Name = "lvJobs";
        lvJobs.Size = new Size(456, 729);
        lvJobs.TabIndex = 0;
        lvJobs.UseCompatibleStateImageBehavior = false;
        lvJobs.View = View.Details;
        // 
        // colJobName
        // 
        colJobName.Text = "Job Name";
        colJobName.Width = 180;
        // 
        // colStatus
        // 
        colStatus.Text = "Status";
        colStatus.Width = 80;
        // 
        // colInterval
        // 
        colInterval.Text = "Interval";
        colInterval.Width = 70;
        // 
        // colLastRun
        // 
        colLastRun.Text = "Last Run";
        colLastRun.Width = 140;
        // 
        // colSources
        // 
        colSources.Text = "Sources";
        colSources.Width = 50;
        // 
        // colDests
        // 
        colDests.Text = "Dests";
        colDests.Width = 50;
        // 
        // lblJobsHeader
        // 
        lblJobsHeader.Dock = DockStyle.Top;
        lblJobsHeader.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        lblJobsHeader.ForeColor = SystemColors.GrayText;
        lblJobsHeader.Location = new Point(0, 0);
        lblJobsHeader.Name = "lblJobsHeader";
        lblJobsHeader.Padding = new Padding(8, 0, 0, 0);
        lblJobsHeader.Size = new Size(456, 24);
        lblJobsHeader.TabIndex = 1;
        lblJobsHeader.Text = "REDIRECT JOBS";
        lblJobsHeader.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // splitterMain
        // 
        splitterMain.Location = new Point(460, 25);
        splitterMain.Name = "splitterMain";
        splitterMain.Size = new Size(4, 753);
        splitterMain.TabIndex = 1;
        splitterMain.TabStop = false;
        // 
        // tabsRight
        // 
        tabsRight.Controls.Add(tabLog);
        tabsRight.Controls.Add(tabHistory);
        tabsRight.Dock = DockStyle.Fill;
        tabsRight.Location = new Point(464, 25);
        tabsRight.Name = "tabsRight";
        tabsRight.SelectedIndex = 0;
        tabsRight.Size = new Size(816, 753);
        tabsRight.TabIndex = 0;
        // 
        // tabLog
        // 
        tabLog.Controls.Add(rtbLog);
        tabLog.Location = new Point(4, 24);
        tabLog.Name = "tabLog";
        tabLog.Size = new Size(808, 725);
        tabLog.TabIndex = 0;
        tabLog.Text = "Activity Log";
        // 
        // rtbLog
        // 
        rtbLog.Dock = DockStyle.Fill;
        rtbLog.Font = new Font("Consolas", 8.5F);
        rtbLog.Location = new Point(0, 0);
        rtbLog.Name = "rtbLog";
        rtbLog.ReadOnly = true;
        rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
        rtbLog.Size = new Size(808, 725);
        rtbLog.TabIndex = 0;
        rtbLog.Text = "";
        // 
        // tabHistory
        // 
        tabHistory.Controls.Add(lvHistory);
        tabHistory.Location = new Point(4, 24);
        tabHistory.Name = "tabHistory";
        tabHistory.Size = new Size(808, 725);
        tabHistory.TabIndex = 1;
        tabHistory.Text = "File History";
        // 
        // lvHistory
        // 
        lvHistory.Columns.AddRange(new ColumnHeader[] { colHTime, colHJob, colHFile, colHSize, colHResult, colHError });
        lvHistory.Dock = DockStyle.Fill;
        lvHistory.FullRowSelect = true;
        lvHistory.GridLines = true;
        lvHistory.Location = new Point(0, 0);
        lvHistory.Name = "lvHistory";
        lvHistory.Size = new Size(808, 725);
        lvHistory.TabIndex = 0;
        lvHistory.UseCompatibleStateImageBehavior = false;
        lvHistory.View = View.Details;
        // 
        // colHTime
        // 
        colHTime.Text = "Time";
        colHTime.Width = 140;
        // 
        // colHJob
        // 
        colHJob.Text = "Job";
        colHJob.Width = 100;
        // 
        // colHFile
        // 
        colHFile.Text = "File";
        colHFile.Width = 200;
        // 
        // colHSize
        // 
        colHSize.Text = "Size";
        colHSize.Width = 70;
        // 
        // colHResult
        // 
        colHResult.Text = "Result";
        colHResult.Width = 70;
        // 
        // colHError
        // 
        colHError.Text = "Error";
        colHError.Width = 260;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
        statusStrip.Location = new Point(0, 778);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1280, 22);
        statusStrip.TabIndex = 4;
        // 
        // lblStatus
        // 
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(39, 17);
        lblStatus.Text = "Ready";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        Controls.Add(tabsRight);
        Controls.Add(splitterMain);
        Controls.Add(pnlLeft);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(900, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "File Redirector";
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;
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
