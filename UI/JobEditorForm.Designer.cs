namespace FileRedirector.UI;

partial class JobEditorForm
{
    private System.ComponentModel.IContainer components = null;

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

        tabControl       = new TabControl();
        tabGeneral       = new TabPage();
        tblGeneral       = new TableLayoutPanel();
        lblName          = new Label();
        txtName          = new TextBox();
        lblEnabled       = new Label();
        chkEnabled       = new CheckBox();
        lblPoll          = new Label();
        nudPoll          = new NumericUpDown();
        lblAction        = new Label();
        cmbAction        = new ComboBox();
        lblMovePath      = new Label();
        txtMovePath      = new TextBox();
        lblSuffix        = new Label();
        txtSuffix        = new TextBox();
        lblNotes         = new Label();
        txtNotes         = new TextBox();
        lblWildcards     = new Label();
        btnWildcards     = new Button();
        tabSources       = new TabPage();
        pnlSources       = new Panel();
        pnlSourceToolbar = new Panel();
        btnAddSource     = new Button();
        btnRemoveSource  = new Button();
        dgvSources       = new DataGridView();
        colSrcPath       = new DataGridViewTextBoxColumn();
        colSrcPattern    = new DataGridViewTextBoxColumn();
        colSrcType       = new DataGridViewComboBoxColumn();
        colSrcUser       = new DataGridViewTextBoxColumn();
        colSrcPass       = new DataGridViewTextBoxColumn();
        colSrcPassive    = new DataGridViewCheckBoxColumn();
        tabDests         = new TabPage();
        pnlDests         = new Panel();
        pnlDestToolbar   = new Panel();
        btnAddDest       = new Button();
        btnRemoveDest    = new Button();
        dgvDests         = new DataGridView();
        colDestPath      = new DataGridViewTextBoxColumn();
        colDestTemplate  = new DataGridViewTextBoxColumn();
        colDestType      = new DataGridViewComboBoxColumn();
        colDestUser      = new DataGridViewTextBoxColumn();
        colDestPass      = new DataGridViewTextBoxColumn();
        colDestPassive   = new DataGridViewCheckBoxColumn();
        pnlButtons       = new Panel();
        btnOk            = new Button();
        btnCancel        = new Button();

        tabControl.SuspendLayout();
        tabGeneral.SuspendLayout();
        tblGeneral.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudPoll).BeginInit();
        tabSources.SuspendLayout();
        pnlSources.SuspendLayout();
        pnlSourceToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvSources).BeginInit();
        tabDests.SuspendLayout();
        pnlDests.SuspendLayout();
        pnlDestToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvDests).BeginInit();
        pnlButtons.SuspendLayout();
        SuspendLayout();

        // ── TabControl ────────────────────────────────────────────────────────
        tabControl.Dock = DockStyle.Fill;
        tabControl.TabPages.AddRange(new TabPage[] { tabGeneral, tabSources, tabDests });

        // ── Tab: General ──────────────────────────────────────────────────────
        tabGeneral.Text = "General";
        tabGeneral.Controls.Add(tblGeneral);

        tblGeneral.Dock        = DockStyle.Fill;
        tblGeneral.ColumnCount = 2;
        tblGeneral.RowCount    = 9;
        tblGeneral.Padding     = new Padding(16);
        tblGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        tblGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblGeneral.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Row 0 – Job Name
        lblName.Text      = "Job Name";
        lblName.ForeColor = SystemColors.GrayText;
        lblName.Dock      = DockStyle.Fill;
        lblName.TextAlign = ContentAlignment.MiddleRight;
        lblName.Padding   = new Padding(0, 0, 8, 0);
        txtName.Dock = DockStyle.Fill;
        tblGeneral.Controls.Add(lblName, 0, 0);
        tblGeneral.Controls.Add(txtName, 1, 0);

        // Row 1 – Enabled
        lblEnabled.Text = "";
        lblEnabled.Dock = DockStyle.Fill;
        chkEnabled.Text    = "Enabled";
        chkEnabled.Checked = true;
        chkEnabled.AutoSize= true;
        chkEnabled.Margin  = new Padding(0, 4, 0, 4);
        tblGeneral.Controls.Add(lblEnabled, 0, 1);
        tblGeneral.Controls.Add(chkEnabled, 1, 1);

        // Row 2 – Poll Interval
        lblPoll.Text      = "Poll Interval (sec)";
        lblPoll.ForeColor = SystemColors.GrayText;
        lblPoll.Dock      = DockStyle.Fill;
        lblPoll.TextAlign = ContentAlignment.MiddleRight;
        lblPoll.Padding   = new Padding(0, 0, 8, 0);
        nudPoll.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
        nudPoll.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
        nudPoll.Value   = new decimal(new int[] { 30, 0, 0, 0 });
        nudPoll.Dock    = DockStyle.Fill;
        tblGeneral.Controls.Add(lblPoll, 0, 2);
        tblGeneral.Controls.Add(nudPoll, 1, 2);

        // Row 3 – After Copy Action
        lblAction.Text      = "After Copy Action";
        lblAction.ForeColor = SystemColors.GrayText;
        lblAction.Dock      = DockStyle.Fill;
        lblAction.TextAlign = ContentAlignment.MiddleRight;
        lblAction.Padding   = new Padding(0, 0, 8, 0);
        cmbAction.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbAction.Dock          = DockStyle.Fill;
        tblGeneral.Controls.Add(lblAction, 0, 3);
        tblGeneral.Controls.Add(cmbAction, 1, 3);

        // Row 4 – Move-To Path
        lblMovePath.Text      = "Move-To Path";
        lblMovePath.ForeColor = SystemColors.GrayText;
        lblMovePath.Dock      = DockStyle.Fill;
        lblMovePath.TextAlign = ContentAlignment.MiddleRight;
        lblMovePath.Padding   = new Padding(0, 0, 8, 0);
        txtMovePath.Dock = DockStyle.Fill;
        tblGeneral.Controls.Add(lblMovePath, 0, 4);
        tblGeneral.Controls.Add(txtMovePath, 1, 4);

        // Row 5 – Processed Suffix
        lblSuffix.Text      = "Processed Suffix";
        lblSuffix.ForeColor = SystemColors.GrayText;
        lblSuffix.Dock      = DockStyle.Fill;
        lblSuffix.TextAlign = ContentAlignment.MiddleRight;
        lblSuffix.Padding   = new Padding(0, 0, 8, 0);
        txtSuffix.Dock = DockStyle.Fill;
        txtSuffix.Text = ".done";
        tblGeneral.Controls.Add(lblSuffix, 0, 5);
        tblGeneral.Controls.Add(txtSuffix, 1, 5);

        // Row 6 – Notes
        lblNotes.Text      = "Notes";
        lblNotes.ForeColor = SystemColors.GrayText;
        lblNotes.Dock      = DockStyle.Fill;
        lblNotes.TextAlign = ContentAlignment.MiddleRight;
        lblNotes.Padding   = new Padding(0, 0, 8, 0);
        txtNotes.Dock      = DockStyle.Fill;
        txtNotes.Multiline = true;
        txtNotes.Height    = 60;
        tblGeneral.Controls.Add(lblNotes, 0, 6);
        tblGeneral.Controls.Add(txtNotes, 1, 6);

        // Row 7 – Wildcard reference
        lblWildcards.Text = "";
        lblWildcards.Dock = DockStyle.Fill;
        btnWildcards.Text      = "Wildcard Reference\u2026";
        btnWildcards.FlatStyle = FlatStyle.System;
        btnWildcards.AutoSize  = true;
        btnWildcards.Margin    = new Padding(0, 4, 0, 4);
        btnWildcards.Click    += btnWildcards_Click;
        tblGeneral.Controls.Add(lblWildcards, 0, 7);
        tblGeneral.Controls.Add(btnWildcards, 1, 7);

        // ── Tab: Sources ──────────────────────────────────────────────────────
        tabSources.Text = "Sources";
        tabSources.Controls.Add(pnlSources);

        pnlSources.Dock    = DockStyle.Fill;
        pnlSources.Padding = new Padding(8);
        pnlSources.Controls.Add(dgvSources);
        pnlSources.Controls.Add(pnlSourceToolbar);

        pnlSourceToolbar.Dock    = DockStyle.Top;
        pnlSourceToolbar.Height  = 38;
        pnlSourceToolbar.Padding = new Padding(0, 4, 0, 4);
        pnlSourceToolbar.Controls.Add(btnRemoveSource);
        pnlSourceToolbar.Controls.Add(btnAddSource);

        btnAddSource.Text      = "+ Add";
        btnAddSource.FlatStyle = FlatStyle.System;
        btnAddSource.Size      = new Size(70, 26);
        btnAddSource.Location  = new Point(0, 0);
        btnAddSource.Click    += btnAddSource_Click;

        btnRemoveSource.Text      = "Remove";
        btnRemoveSource.FlatStyle = FlatStyle.System;
        btnRemoveSource.Size      = new Size(70, 26);
        btnRemoveSource.Location  = new Point(76, 0);
        btnRemoveSource.Click    += btnRemoveSource_Click;

        // Sources columns — all names set BEFORE AddRange
        colSrcPath.Name       = "Path";
        colSrcPath.HeaderText = "Path  (@wildcards ok)";
        colSrcPath.FillWeight = 38;
        colSrcPattern.Name       = "Pattern";
        colSrcPattern.HeaderText = "File Pattern";
        colSrcPattern.FillWeight = 22;
        colSrcType.Name       = "PathType";
        colSrcType.HeaderText = "Type";
        colSrcType.FillWeight = 14;
        colSrcType.FlatStyle  = FlatStyle.Flat;
        colSrcUser.Name       = "Username";
        colSrcUser.HeaderText = "Username";
        colSrcUser.FillWeight = 10;
        colSrcPass.Name       = "Password";
        colSrcPass.HeaderText = "Password";
        colSrcPass.FillWeight = 10;
        colSrcPassive.Name       = "Passive";
        colSrcPassive.HeaderText = "Passive FTP";
        colSrcPassive.FillWeight = 8;

        dgvSources.Dock                  = DockStyle.Fill;
        dgvSources.RowHeadersVisible     = false;
        dgvSources.AllowUserToAddRows    = true;
        dgvSources.AllowUserToDeleteRows = true;
        dgvSources.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
        dgvSources.ColumnHeadersHeight   = 32;
        dgvSources.RowTemplate.Height    = 28;
        dgvSources.Columns.AddRange(new DataGridViewColumn[] { colSrcPath, colSrcPattern, colSrcType, colSrcUser, colSrcPass, colSrcPassive });

        // ── Tab: Destinations ─────────────────────────────────────────────────
        tabDests.Text = "Destinations";
        tabDests.Controls.Add(pnlDests);

        pnlDests.Dock    = DockStyle.Fill;
        pnlDests.Padding = new Padding(8);
        pnlDests.Controls.Add(dgvDests);
        pnlDests.Controls.Add(pnlDestToolbar);

        pnlDestToolbar.Dock    = DockStyle.Top;
        pnlDestToolbar.Height  = 38;
        pnlDestToolbar.Padding = new Padding(0, 4, 0, 4);
        pnlDestToolbar.Controls.Add(btnRemoveDest);
        pnlDestToolbar.Controls.Add(btnAddDest);

        btnAddDest.Text      = "+ Add";
        btnAddDest.FlatStyle = FlatStyle.System;
        btnAddDest.Size      = new Size(70, 26);
        btnAddDest.Location  = new Point(0, 0);
        btnAddDest.Click    += btnAddDest_Click;

        btnRemoveDest.Text      = "Remove";
        btnRemoveDest.FlatStyle = FlatStyle.System;
        btnRemoveDest.Size      = new Size(70, 26);
        btnRemoveDest.Location  = new Point(76, 0);
        btnRemoveDest.Click    += btnRemoveDest_Click;

        // Destinations columns — all names set BEFORE AddRange
        colDestPath.Name       = "Path";
        colDestPath.HeaderText = "Path  (@wildcards ok)";
        colDestPath.FillWeight = 38;
        colDestTemplate.Name       = "Pattern";
        colDestTemplate.HeaderText = "Filename Template";
        colDestTemplate.FillWeight = 22;
        colDestType.Name       = "PathType";
        colDestType.HeaderText = "Type";
        colDestType.FillWeight = 14;
        colDestType.FlatStyle  = FlatStyle.Flat;
        colDestUser.Name       = "Username";
        colDestUser.HeaderText = "Username";
        colDestUser.FillWeight = 10;
        colDestPass.Name       = "Password";
        colDestPass.HeaderText = "Password";
        colDestPass.FillWeight = 10;
        colDestPassive.Name       = "Passive";
        colDestPassive.HeaderText = "Passive FTP";
        colDestPassive.FillWeight = 8;

        dgvDests.Dock                  = DockStyle.Fill;
        dgvDests.RowHeadersVisible     = false;
        dgvDests.AllowUserToAddRows    = true;
        dgvDests.AllowUserToDeleteRows = true;
        dgvDests.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDests.ColumnHeadersHeight   = 32;
        dgvDests.RowTemplate.Height    = 28;
        dgvDests.Columns.AddRange(new DataGridViewColumn[] { colDestPath, colDestTemplate, colDestType, colDestUser, colDestPass, colDestPassive });

        // ── Button panel ──────────────────────────────────────────────────────
        pnlButtons.Dock   = DockStyle.Bottom;
        pnlButtons.Height = 48;
        pnlButtons.Padding= new Padding(8);
        pnlButtons.Controls.Add(btnOk);
        pnlButtons.Controls.Add(btnCancel);

        btnOk.Text      = "Save";
        btnOk.FlatStyle = FlatStyle.System;
        btnOk.Size      = new Size(90, 30);
        btnOk.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
        btnOk.Click    += btnOk_Click;

        btnCancel.Text      = "Cancel";
        btnCancel.FlatStyle = FlatStyle.System;
        btnCancel.Size      = new Size(90, 30);
        btnCancel.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.Click    += btnCancel_Click;

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        // ── Form ─────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(960, 700);
        MinimumSize         = new Size(800, 600);
        Font                = new Font("Segoe UI", 9F);
        FormBorderStyle     = FormBorderStyle.Sizable;
        StartPosition       = FormStartPosition.CenterParent;
        Text                = "Redirect Job";
        Controls.Add(tabControl);
        Controls.Add(pnlButtons);

        tblGeneral.ResumeLayout(false);
        tblGeneral.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudPoll).EndInit();
        tabGeneral.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvSources).EndInit();
        pnlSourceToolbar.ResumeLayout(false);
        pnlSources.ResumeLayout(false);
        tabSources.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvDests).EndInit();
        pnlDestToolbar.ResumeLayout(false);
        pnlDests.ResumeLayout(false);
        tabDests.ResumeLayout(false);
        tabControl.ResumeLayout(false);
        pnlButtons.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TabControl                  tabControl;
    private TabPage                     tabGeneral;
    private TableLayoutPanel            tblGeneral;
    private Label                       lblName;
    private TextBox                     txtName;
    private Label                       lblEnabled;
    private CheckBox                    chkEnabled;
    private Label                       lblPoll;
    private NumericUpDown               nudPoll;
    private Label                       lblAction;
    private ComboBox                    cmbAction;
    private Label                       lblMovePath;
    private TextBox                     txtMovePath;
    private Label                       lblSuffix;
    private TextBox                     txtSuffix;
    private Label                       lblNotes;
    private TextBox                     txtNotes;
    private Label                       lblWildcards;
    private Button                      btnWildcards;
    private TabPage                     tabSources;
    private Panel                       pnlSources;
    private Panel                       pnlSourceToolbar;
    private Button                      btnAddSource;
    private Button                      btnRemoveSource;
    private DataGridView                dgvSources;
    private DataGridViewTextBoxColumn   colSrcPath;
    private DataGridViewTextBoxColumn   colSrcPattern;
    private DataGridViewComboBoxColumn  colSrcType;
    private DataGridViewTextBoxColumn   colSrcUser;
    private DataGridViewTextBoxColumn   colSrcPass;
    private DataGridViewCheckBoxColumn  colSrcPassive;
    private TabPage                     tabDests;
    private Panel                       pnlDests;
    private Panel                       pnlDestToolbar;
    private Button                      btnAddDest;
    private Button                      btnRemoveDest;
    private DataGridView                dgvDests;
    private DataGridViewTextBoxColumn   colDestPath;
    private DataGridViewTextBoxColumn   colDestTemplate;
    private DataGridViewComboBoxColumn  colDestType;
    private DataGridViewTextBoxColumn   colDestUser;
    private DataGridViewTextBoxColumn   colDestPass;
    private DataGridViewCheckBoxColumn  colDestPassive;
    private Panel                       pnlButtons;
    private Button                      btnOk;
    private Button                      btnCancel;
}
