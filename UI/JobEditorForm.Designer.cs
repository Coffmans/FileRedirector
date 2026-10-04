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
        tabControl = new TabControl();
        tabGeneral = new TabPage();
        tblGeneral = new TableLayoutPanel();
        lblName = new Label();
        txtName = new TextBox();
        lblEnabled = new Label();
        chkEnabled = new CheckBox();
        lblPoll = new Label();
        nudPoll = new NumericUpDown();
        lblAction = new Label();
        cmbAction = new ComboBox();
        lblMovePath = new Label();
        txtMovePath = new TextBox();
        lblSuffix = new Label();
        txtSuffix = new TextBox();
        lblNotes = new Label();
        txtNotes = new TextBox();
        lblWildcards = new Label();
        btnWildcards = new Button();
        tabSources = new TabPage();
        pnlSources = new Panel();
        dgvSources = new DataGridView();
        pnlSourceToolbar = new Panel();
        btnRemoveSource = new Button();
        btnAddSource = new Button();
        tabDests = new TabPage();
        pnlDests = new Panel();
        dgvDests = new DataGridView();
        pnlDestToolbar = new Panel();
        btnRemoveDest = new Button();
        btnAddDest = new Button();
        pnlButtons = new Panel();
        btnOk = new Button();
        btnCancel = new Button();
        colSrcPath = new DataGridViewTextBoxColumn();
        colSrcPattern = new DataGridViewTextBoxColumn();
        colSrcType = new DataGridViewComboBoxColumn();
        colSrcUser = new DataGridViewTextBoxColumn();
        colSrcPass = new DataGridViewTextBoxColumn();
        colSrcPassive = new DataGridViewCheckBoxColumn();
        colSrcTrustCert = new DataGridViewCheckBoxColumn();
        colDestPath = new DataGridViewTextBoxColumn();
        colDestTemplate = new DataGridViewTextBoxColumn();
        colDestType = new DataGridViewComboBoxColumn();
        colDestUser = new DataGridViewTextBoxColumn();
        colDestPass = new DataGridViewTextBoxColumn();
        colDestPassive = new DataGridViewCheckBoxColumn();
        colDestTrustCert = new DataGridViewCheckBoxColumn();
        tabControl.SuspendLayout();
        tabGeneral.SuspendLayout();
        tblGeneral.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudPoll).BeginInit();
        tabSources.SuspendLayout();
        pnlSources.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvSources).BeginInit();
        pnlSourceToolbar.SuspendLayout();
        tabDests.SuspendLayout();
        pnlDests.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvDests).BeginInit();
        pnlDestToolbar.SuspendLayout();
        pnlButtons.SuspendLayout();
        SuspendLayout();
        // 
        // tabControl
        // 
        tabControl.Controls.Add(tabGeneral);
        tabControl.Controls.Add(tabSources);
        tabControl.Controls.Add(tabDests);
        tabControl.Dock = DockStyle.Fill;
        tabControl.Location = new Point(0, 0);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(1085, 652);
        tabControl.TabIndex = 0;
        // 
        // tabGeneral
        // 
        tabGeneral.Controls.Add(tblGeneral);
        tabGeneral.Location = new Point(4, 24);
        tabGeneral.Name = "tabGeneral";
        tabGeneral.Size = new Size(1077, 624);
        tabGeneral.TabIndex = 0;
        tabGeneral.Text = "General";
        // 
        // tblGeneral
        // 
        tblGeneral.ColumnCount = 2;
        tblGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        tblGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tblGeneral.Controls.Add(lblName, 0, 0);
        tblGeneral.Controls.Add(txtName, 1, 0);
        tblGeneral.Controls.Add(lblEnabled, 0, 1);
        tblGeneral.Controls.Add(chkEnabled, 1, 1);
        tblGeneral.Controls.Add(lblPoll, 0, 2);
        tblGeneral.Controls.Add(nudPoll, 1, 2);
        tblGeneral.Controls.Add(lblAction, 0, 3);
        tblGeneral.Controls.Add(cmbAction, 1, 3);
        tblGeneral.Controls.Add(lblMovePath, 0, 4);
        tblGeneral.Controls.Add(txtMovePath, 1, 4);
        tblGeneral.Controls.Add(lblSuffix, 0, 5);
        tblGeneral.Controls.Add(txtSuffix, 1, 5);
        tblGeneral.Controls.Add(lblNotes, 0, 6);
        tblGeneral.Controls.Add(txtNotes, 1, 6);
        tblGeneral.Controls.Add(lblWildcards, 0, 7);
        tblGeneral.Controls.Add(btnWildcards, 1, 7);
        tblGeneral.Dock = DockStyle.Fill;
        tblGeneral.Location = new Point(0, 0);
        tblGeneral.Name = "tblGeneral";
        tblGeneral.Padding = new Padding(16);
        tblGeneral.RowCount = 9;
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.RowStyles.Add(new RowStyle());
        tblGeneral.Size = new Size(1077, 624);
        tblGeneral.TabIndex = 0;
        // 
        // lblName
        // 
        lblName.Dock = DockStyle.Fill;
        lblName.ForeColor = SystemColors.GrayText;
        lblName.Location = new Point(19, 16);
        lblName.Name = "lblName";
        lblName.Padding = new Padding(0, 0, 8, 0);
        lblName.Size = new Size(154, 29);
        lblName.TabIndex = 0;
        lblName.Text = "Job Name";
        lblName.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtName
        // 
        txtName.Dock = DockStyle.Fill;
        txtName.Location = new Point(179, 19);
        txtName.Name = "txtName";
        txtName.Size = new Size(879, 23);
        txtName.TabIndex = 1;
        // 
        // lblEnabled
        // 
        lblEnabled.Dock = DockStyle.Fill;
        lblEnabled.Location = new Point(19, 45);
        lblEnabled.Name = "lblEnabled";
        lblEnabled.Size = new Size(154, 27);
        lblEnabled.TabIndex = 2;
        // 
        // chkEnabled
        // 
        chkEnabled.AutoSize = true;
        chkEnabled.Checked = true;
        chkEnabled.CheckState = CheckState.Checked;
        chkEnabled.Location = new Point(176, 49);
        chkEnabled.Margin = new Padding(0, 4, 0, 4);
        chkEnabled.Name = "chkEnabled";
        chkEnabled.Size = new Size(68, 19);
        chkEnabled.TabIndex = 3;
        chkEnabled.Text = "Enabled";
        // 
        // lblPoll
        // 
        lblPoll.Dock = DockStyle.Fill;
        lblPoll.ForeColor = SystemColors.GrayText;
        lblPoll.Location = new Point(19, 72);
        lblPoll.Name = "lblPoll";
        lblPoll.Padding = new Padding(0, 0, 8, 0);
        lblPoll.Size = new Size(154, 29);
        lblPoll.TabIndex = 4;
        lblPoll.Text = "Poll Interval (sec)";
        lblPoll.TextAlign = ContentAlignment.MiddleRight;
        // 
        // nudPoll
        // 
        nudPoll.Dock = DockStyle.Fill;
        nudPoll.Location = new Point(179, 75);
        nudPoll.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
        nudPoll.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
        nudPoll.Name = "nudPoll";
        nudPoll.Size = new Size(879, 23);
        nudPoll.TabIndex = 5;
        nudPoll.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // lblAction
        // 
        lblAction.Dock = DockStyle.Fill;
        lblAction.ForeColor = SystemColors.GrayText;
        lblAction.Location = new Point(19, 101);
        lblAction.Name = "lblAction";
        lblAction.Padding = new Padding(0, 0, 8, 0);
        lblAction.Size = new Size(154, 29);
        lblAction.TabIndex = 6;
        lblAction.Text = "After Copy Action";
        lblAction.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cmbAction
        // 
        cmbAction.Dock = DockStyle.Fill;
        cmbAction.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbAction.Location = new Point(179, 104);
        cmbAction.Name = "cmbAction";
        cmbAction.Size = new Size(879, 23);
        cmbAction.TabIndex = 7;
        // 
        // lblMovePath
        // 
        lblMovePath.Dock = DockStyle.Fill;
        lblMovePath.ForeColor = SystemColors.GrayText;
        lblMovePath.Location = new Point(19, 130);
        lblMovePath.Name = "lblMovePath";
        lblMovePath.Padding = new Padding(0, 0, 8, 0);
        lblMovePath.Size = new Size(154, 29);
        lblMovePath.TabIndex = 8;
        lblMovePath.Text = "Move-To Path";
        lblMovePath.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtMovePath
        // 
        txtMovePath.Dock = DockStyle.Fill;
        txtMovePath.Location = new Point(179, 133);
        txtMovePath.Name = "txtMovePath";
        txtMovePath.Size = new Size(879, 23);
        txtMovePath.TabIndex = 9;
        // 
        // lblSuffix
        // 
        lblSuffix.Dock = DockStyle.Fill;
        lblSuffix.ForeColor = SystemColors.GrayText;
        lblSuffix.Location = new Point(19, 159);
        lblSuffix.Name = "lblSuffix";
        lblSuffix.Padding = new Padding(0, 0, 8, 0);
        lblSuffix.Size = new Size(154, 29);
        lblSuffix.TabIndex = 10;
        lblSuffix.Text = "Processed Suffix";
        lblSuffix.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtSuffix
        // 
        txtSuffix.Dock = DockStyle.Fill;
        txtSuffix.Location = new Point(179, 162);
        txtSuffix.Name = "txtSuffix";
        txtSuffix.Size = new Size(879, 23);
        txtSuffix.TabIndex = 11;
        txtSuffix.Text = ".done";
        // 
        // lblNotes
        // 
        lblNotes.Dock = DockStyle.Fill;
        lblNotes.ForeColor = SystemColors.GrayText;
        lblNotes.Location = new Point(19, 188);
        lblNotes.Name = "lblNotes";
        lblNotes.Padding = new Padding(0, 0, 8, 0);
        lblNotes.Size = new Size(154, 66);
        lblNotes.TabIndex = 12;
        lblNotes.Text = "Notes";
        lblNotes.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtNotes
        // 
        txtNotes.Dock = DockStyle.Fill;
        txtNotes.Location = new Point(179, 191);
        txtNotes.Multiline = true;
        txtNotes.Name = "txtNotes";
        txtNotes.Size = new Size(879, 60);
        txtNotes.TabIndex = 13;
        // 
        // lblWildcards
        // 
        lblWildcards.Dock = DockStyle.Fill;
        lblWildcards.Location = new Point(19, 254);
        lblWildcards.Name = "lblWildcards";
        lblWildcards.Size = new Size(154, 32);
        lblWildcards.TabIndex = 14;
        // 
        // btnWildcards
        // 
        btnWildcards.AutoSize = true;
        btnWildcards.FlatStyle = FlatStyle.System;
        btnWildcards.Location = new Point(176, 258);
        btnWildcards.Margin = new Padding(0, 4, 0, 4);
        btnWildcards.Name = "btnWildcards";
        btnWildcards.Size = new Size(132, 24);
        btnWildcards.TabIndex = 15;
        btnWildcards.Text = "Wildcard Reference…";
        btnWildcards.Click += BtnWildcards_Click;
        // 
        // tabSources
        // 
        tabSources.Controls.Add(pnlSources);
        tabSources.Location = new Point(4, 24);
        tabSources.Name = "tabSources";
        tabSources.Size = new Size(1077, 624);
        tabSources.TabIndex = 1;
        tabSources.Text = "Sources";
        // 
        // pnlSources
        // 
        pnlSources.Controls.Add(dgvSources);
        pnlSources.Controls.Add(pnlSourceToolbar);
        pnlSources.Dock = DockStyle.Fill;
        pnlSources.Location = new Point(0, 0);
        pnlSources.Name = "pnlSources";
        pnlSources.Padding = new Padding(8);
        pnlSources.Size = new Size(1077, 624);
        pnlSources.TabIndex = 0;
        // 
        // dgvSources
        // 
        dgvSources.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvSources.ColumnHeadersHeight = 32;
        dgvSources.Columns.AddRange(new DataGridViewColumn[] { colSrcPath, colSrcPattern, colSrcType, colSrcUser, colSrcPass, colSrcPassive, colSrcTrustCert });
        dgvSources.Dock = DockStyle.Fill;
        dgvSources.Location = new Point(8, 46);
        dgvSources.Name = "dgvSources";
        dgvSources.RowHeadersVisible = false;
        dgvSources.RowTemplate.Height = 28;
        dgvSources.Size = new Size(1061, 570);
        dgvSources.TabIndex = 0;
        // 
        // pnlSourceToolbar
        // 
        pnlSourceToolbar.Controls.Add(btnRemoveSource);
        pnlSourceToolbar.Controls.Add(btnAddSource);
        pnlSourceToolbar.Dock = DockStyle.Top;
        pnlSourceToolbar.Location = new Point(8, 8);
        pnlSourceToolbar.Name = "pnlSourceToolbar";
        pnlSourceToolbar.Padding = new Padding(0, 4, 0, 4);
        pnlSourceToolbar.Size = new Size(1061, 38);
        pnlSourceToolbar.TabIndex = 1;
        // 
        // btnRemoveSource
        // 
        btnRemoveSource.FlatStyle = FlatStyle.System;
        btnRemoveSource.Location = new Point(76, 0);
        btnRemoveSource.Name = "btnRemoveSource";
        btnRemoveSource.Size = new Size(70, 26);
        btnRemoveSource.TabIndex = 0;
        btnRemoveSource.Text = "Remove";
        btnRemoveSource.Click += BtnRemoveSource_Click;
        // 
        // btnAddSource
        // 
        btnAddSource.FlatStyle = FlatStyle.System;
        btnAddSource.Location = new Point(0, 0);
        btnAddSource.Name = "btnAddSource";
        btnAddSource.Size = new Size(70, 26);
        btnAddSource.TabIndex = 1;
        btnAddSource.Text = "+ Add";
        btnAddSource.Click += BtnAddSource_Click;
        // 
        // tabDests
        // 
        tabDests.Controls.Add(pnlDests);
        tabDests.Location = new Point(4, 24);
        tabDests.Name = "tabDests";
        tabDests.Size = new Size(1077, 624);
        tabDests.TabIndex = 2;
        tabDests.Text = "Destinations";
        // 
        // pnlDests
        // 
        pnlDests.Controls.Add(dgvDests);
        pnlDests.Controls.Add(pnlDestToolbar);
        pnlDests.Dock = DockStyle.Fill;
        pnlDests.Location = new Point(0, 0);
        pnlDests.Name = "pnlDests";
        pnlDests.Padding = new Padding(8);
        pnlDests.Size = new Size(1077, 624);
        pnlDests.TabIndex = 0;
        // 
        // dgvDests
        // 
        dgvDests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDests.ColumnHeadersHeight = 32;
        dgvDests.Columns.AddRange(new DataGridViewColumn[] { colDestPath, colDestTemplate, colDestType, colDestUser, colDestPass, colDestPassive, colDestTrustCert });
        dgvDests.Dock = DockStyle.Fill;
        dgvDests.Location = new Point(8, 46);
        dgvDests.Name = "dgvDests";
        dgvDests.RowHeadersVisible = false;
        dgvDests.RowTemplate.Height = 28;
        dgvDests.Size = new Size(1061, 570);
        dgvDests.TabIndex = 0;
        // 
        // pnlDestToolbar
        // 
        pnlDestToolbar.Controls.Add(btnRemoveDest);
        pnlDestToolbar.Controls.Add(btnAddDest);
        pnlDestToolbar.Dock = DockStyle.Top;
        pnlDestToolbar.Location = new Point(8, 8);
        pnlDestToolbar.Name = "pnlDestToolbar";
        pnlDestToolbar.Padding = new Padding(0, 4, 0, 4);
        pnlDestToolbar.Size = new Size(1061, 38);
        pnlDestToolbar.TabIndex = 1;
        // 
        // btnRemoveDest
        // 
        btnRemoveDest.FlatStyle = FlatStyle.System;
        btnRemoveDest.Location = new Point(76, 0);
        btnRemoveDest.Name = "btnRemoveDest";
        btnRemoveDest.Size = new Size(70, 26);
        btnRemoveDest.TabIndex = 0;
        btnRemoveDest.Text = "Remove";
        btnRemoveDest.Click += BtnRemoveDest_Click;
        // 
        // btnAddDest
        // 
        btnAddDest.FlatStyle = FlatStyle.System;
        btnAddDest.Location = new Point(0, 0);
        btnAddDest.Name = "btnAddDest";
        btnAddDest.Size = new Size(70, 26);
        btnAddDest.TabIndex = 1;
        btnAddDest.Text = "+ Add";
        btnAddDest.Click += BtnAddDest_Click;
        // 
        // pnlButtons
        // 
        pnlButtons.Controls.Add(btnOk);
        pnlButtons.Controls.Add(btnCancel);
        pnlButtons.Dock = DockStyle.Bottom;
        pnlButtons.Location = new Point(0, 652);
        pnlButtons.Name = "pnlButtons";
        pnlButtons.Padding = new Padding(8);
        pnlButtons.Size = new Size(1085, 48);
        pnlButtons.TabIndex = 1;
        // 
        // btnOk
        // 
        btnOk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOk.FlatStyle = FlatStyle.System;
        btnOk.Location = new Point(885, 0);
        btnOk.Name = "btnOk";
        btnOk.Size = new Size(90, 30);
        btnOk.TabIndex = 0;
        btnOk.Text = "Save";
        btnOk.Click += BtnOk_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.FlatStyle = FlatStyle.System;
        btnCancel.Location = new Point(885, 0);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(90, 30);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Click += BtnCancel_Click;
        // 
        // colSrcPath
        // 
        colSrcPath.FillWeight = 34.51059F;
        colSrcPath.HeaderText = "Path  (@wildcards ok)";
        colSrcPath.Name = "colSrcPath";
        // 
        // colSrcPattern
        // 
        colSrcPattern.FillWeight = 19.9798164F;
        colSrcPattern.HeaderText = "File Pattern";
        colSrcPattern.Name = "colSrcPattern";
        // 
        // colSrcType
        // 
        colSrcType.FillWeight = 12.7144279F;
        colSrcType.FlatStyle = FlatStyle.Flat;
        colSrcType.HeaderText = "Type";
        colSrcType.Name = "colSrcType";
        // 
        // colSrcUser
        // 
        colSrcUser.FillWeight = 9.081734F;
        colSrcUser.HeaderText = "Username";
        colSrcUser.Name = "colSrcUser";
        // 
        // colSrcPass
        // 
        colSrcPass.FillWeight = 9.081734F;
        colSrcPass.HeaderText = "Password";
        colSrcPass.Name = "colSrcPass";
        // 
        // colSrcPassive
        // 
        colSrcPassive.FillWeight = 11.1675138F;
        colSrcPassive.HeaderText = "Passive FTP";
        colSrcPassive.Name = "colSrcPassive";
        // 
        // colSrcTrustCert
        // 
        colSrcTrustCert.FillWeight = 13.4641981F;
        colSrcTrustCert.HeaderText = "Trust Any Cert (FTPS)";
        colSrcTrustCert.Name = "colSrcTrustCert";
        colSrcTrustCert.ToolTipText = "Skip FTPS certificate validation. Only use for self-signed certificates on trusted networks.";
        // 
        // colDestPath
        // 
        colDestPath.FillWeight = 34.51059F;
        colDestPath.HeaderText = "Path  (@wildcards ok)";
        colDestPath.Name = "colDestPath";
        // 
        // colDestTemplate
        // 
        colDestTemplate.FillWeight = 19.9798145F;
        colDestTemplate.HeaderText = "Filename Template (Optional)";
        colDestTemplate.Name = "colDestTemplate";
        // 
        // colDestType
        // 
        colDestType.FillWeight = 12.714427F;
        colDestType.FlatStyle = FlatStyle.Flat;
        colDestType.HeaderText = "Type";
        colDestType.Name = "colDestType";
        // 
        // colDestUser
        // 
        colDestUser.FillWeight = 9.081734F;
        colDestUser.HeaderText = "Username";
        colDestUser.Name = "colDestUser";
        // 
        // colDestPass
        // 
        colDestPass.FillWeight = 9.081734F;
        colDestPass.HeaderText = "Password";
        colDestPass.Name = "colDestPass";
        // 
        // colDestPassive
        // 
        colDestPassive.FillWeight = 11.1675138F;
        colDestPassive.HeaderText = "Passive FTP";
        colDestPassive.Name = "colDestPassive";
        // 
        // colDestTrustCert
        // 
        colDestTrustCert.FillWeight = 13.4641981F;
        colDestTrustCert.HeaderText = "Trust Any Cert (FTPS)";
        colDestTrustCert.MinimumWidth = 10;
        colDestTrustCert.Name = "colDestTrustCert";
        colDestTrustCert.ToolTipText = "Skip FTPS certificate validation. Only use for self-signed certificates on trusted networks.";
        // 
        // JobEditorForm
        // 
        AcceptButton = btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(1085, 700);
        Controls.Add(tabControl);
        Controls.Add(pnlButtons);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(800, 600);
        Name = "JobEditorForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Redirect Job";
        tabControl.ResumeLayout(false);
        tabGeneral.ResumeLayout(false);
        tblGeneral.ResumeLayout(false);
        tblGeneral.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudPoll).EndInit();
        tabSources.ResumeLayout(false);
        pnlSources.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvSources).EndInit();
        pnlSourceToolbar.ResumeLayout(false);
        tabDests.ResumeLayout(false);
        pnlDests.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvDests).EndInit();
        pnlDestToolbar.ResumeLayout(false);
        pnlButtons.ResumeLayout(false);
        ResumeLayout(false);
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
    private TabPage                     tabDests;
    private Panel                       pnlDests;
    private Panel                       pnlDestToolbar;
    private Button                      btnAddDest;
    private Button                      btnRemoveDest;
    private DataGridView                dgvDests;
    private Panel                       pnlButtons;
    private Button                      btnOk;
    private Button                      btnCancel;
    private DataGridViewTextBoxColumn colSrcPath;
    private DataGridViewTextBoxColumn colSrcPattern;
    private DataGridViewComboBoxColumn colSrcType;
    private DataGridViewTextBoxColumn colSrcUser;
    private DataGridViewTextBoxColumn colSrcPass;
    private DataGridViewCheckBoxColumn colSrcPassive;
    private DataGridViewCheckBoxColumn colSrcTrustCert;
    private DataGridViewTextBoxColumn colDestPath;
    private DataGridViewTextBoxColumn colDestTemplate;
    private DataGridViewComboBoxColumn colDestType;
    private DataGridViewTextBoxColumn colDestUser;
    private DataGridViewTextBoxColumn colDestPass;
    private DataGridViewCheckBoxColumn colDestPassive;
    private DataGridViewCheckBoxColumn colDestTrustCert;
}
