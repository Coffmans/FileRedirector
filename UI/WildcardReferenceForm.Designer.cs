namespace FileRedirector.UI;

partial class WildcardReferenceForm
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

        dgvTokens  = new DataGridView();
        colToken   = new DataGridViewTextBoxColumn();
        colExample = new DataGridViewTextBoxColumn();

        ((System.ComponentModel.ISupportInitialize)dgvTokens).BeginInit();
        SuspendLayout();

        // Columns — names set BEFORE AddRange
        colToken.Name       = "Token";
        colToken.HeaderText = "Token";
        colToken.Width      = 120;

        colExample.Name       = "Example";
        colExample.HeaderText = "Example  (as of now)";
        colExample.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        dgvTokens.Dock                  = DockStyle.Fill;
        dgvTokens.RowHeadersVisible     = false;
        dgvTokens.AllowUserToAddRows    = false;
        dgvTokens.AllowUserToDeleteRows = false;
        dgvTokens.ReadOnly              = true;
        dgvTokens.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
        dgvTokens.ColumnHeadersHeight   = 30;
        dgvTokens.RowTemplate.Height    = 24;
        dgvTokens.Columns.AddRange(new DataGridViewColumn[] { colToken, colExample });

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(480, 540);
        Font                = new Font("Consolas", 9F);
        FormBorderStyle     = FormBorderStyle.FixedDialog;
        MaximizeBox         = false;
        MinimizeBox         = false;
        StartPosition       = FormStartPosition.CenterParent;
        Text                = "Wildcard Token Reference";
        Controls.Add(dgvTokens);

        ((System.ComponentModel.ISupportInitialize)dgvTokens).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private DataGridView              dgvTokens;
    private DataGridViewTextBoxColumn colToken;
    private DataGridViewTextBoxColumn colExample;
}
