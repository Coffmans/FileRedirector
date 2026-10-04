using FileRedirector.Models;

namespace FileRedirector.UI;

public partial class JobEditorForm : Form
{
    private readonly RedirectJob _job;

    public RedirectJob Result => _job;

    public JobEditorForm(RedirectJob? job = null)
    {
        // Edit a copy so Cancel (or a failed validation) leaves the caller's job untouched
        _job = job?.Clone() ?? new RedirectJob { CreatedAt = DateTime.UtcNow.ToString("o") };

        InitializeComponent();

        Text = job is null ? "New Redirect Job" : "Edit Redirect Job";

        // Populate combo with enum values
        cmbAction.Items.AddRange(Enum.GetNames<SourceFileAction>());

        // Populate source-grid PathType combos. HTTP/HTTPS have no directory listing, so they're
        // destination-only — unless an existing job already uses one (kept so the grid can display it).
        var sourceTypes = Enum.GetValues<PathType>()
            .Where(t => !IsHttp(t) || _job.Sources.Any(s => s.PathType == t))
            .Select(t => t.ToString())
            .ToArray();
        ((DataGridViewComboBoxColumn)dgvSources.Columns["colSrcType"]!)
            .Items.AddRange(sourceTypes);
        ((DataGridViewComboBoxColumn)dgvDests.Columns["colDestType"]!)
            .Items.AddRange(Enum.GetNames<PathType>());

        PopulateFields();

        // Show passwords as dots, both when displayed and while being edited
        dgvSources.CellFormatting        += MaskPasswordCell;
        dgvDests.CellFormatting          += MaskPasswordCell;
        dgvSources.EditingControlShowing += MaskPasswordEditor;
        dgvDests.EditingControlShowing   += MaskPasswordEditor;

        cmbAction.SelectedIndexChanged += (_, _) => UpdateActionVisibility();
        UpdateActionVisibility();
    }

    // ─── Populate ─────────────────────────────────────────────────────────────

    private void PopulateFields()
    {
        txtName.Text = _job.Name;
        chkEnabled.Checked = _job.IsEnabled;
        nudPoll.Value = Math.Clamp(_job.PollIntervalSeconds, 5, 86400);
        cmbAction.Text = _job.SourceAction.ToString();
        txtMovePath.Text = _job.MoveToPath ?? string.Empty;
        txtSuffix.Text = _job.ProcessedSuffix ?? ".done";
        txtNotes.Text = _job.Notes ?? string.Empty;

        foreach (var src in _job.Sources)
            dgvSources.Rows.Add(src.Path, src.FilePattern, src.PathType.ToString(),
                                src.Username, src.Password, src.IsPassive, src.AcceptAnyCertificate);

        foreach (var dest in _job.Destinations)
            dgvDests.Rows.Add(dest.Path, dest.FileNameTemplate, dest.PathType.ToString(),
                              dest.Username, dest.Password, dest.IsPassive, dest.AcceptAnyCertificate);
    }

    // ─── Collect ──────────────────────────────────────────────────────────────

    private void CollectFields()
    {
        _job.Name = txtName.Text.Trim();
        _job.IsEnabled = chkEnabled.Checked;
        _job.PollIntervalSeconds = (int)nudPoll.Value;
        _job.SourceAction = Enum.Parse<SourceFileAction>(cmbAction.Text);
        _job.MoveToPath = txtMovePath.Text.NullIfEmpty();
        _job.ProcessedSuffix = txtSuffix.Text.NullIfEmpty() ?? ".done";
        _job.Notes = txtNotes.Text.NullIfEmpty();

        _job.Sources.Clear();
        foreach (DataGridViewRow r in dgvSources.Rows)
        {
            if (r.IsNewRow) continue;
            var path = r.Cells["colSrcPath"].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(path)) continue;
            _job.Sources.Add(new JobSource
            {
                Path = path,
                FilePattern = r.Cells["colSrcPattern"].Value?.ToString()?.Trim().NullIfEmpty() ?? "*.*",
                PathType = Enum.TryParse<PathType>(r.Cells["colSrcType"].Value?.ToString(), out var spt)
                                  ? spt : PathType.LocalOrUNC,
                Username = r.Cells["colSrcUser"].Value?.ToString().NullIfEmpty(),
                Password = r.Cells["colSrcPass"].Value?.ToString().NullIfEmpty(),
                IsPassive = r.Cells["colSrcPassive"].Value is true,
                AcceptAnyCertificate = r.Cells["colSrcTrustCert"].Value is true
            });
        }

        _job.Destinations.Clear();
        int ord = 0;
        foreach (DataGridViewRow r in dgvDests.Rows)
        {
            if (r.IsNewRow) continue;
            var path = r.Cells["colDestPath"].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(path)) continue;
            _job.Destinations.Add(new JobDestination
            {
                Path = path,
                FileNameTemplate = r.Cells["colDestTemplate"].Value?.ToString().NullIfEmpty(),
                PathType = Enum.TryParse<PathType>(r.Cells["colDestType"].Value?.ToString(), out var dpt)
                                       ? dpt : PathType.LocalOrUNC,
                Username = r.Cells["colDestUser"].Value?.ToString().NullIfEmpty(),
                Password = r.Cells["colDestPass"].Value?.ToString().NullIfEmpty(),
                SortOrder = ord++,
                IsPassive = r.Cells["colDestPassive"].Value is true,
                AcceptAnyCertificate = r.Cells["colDestTrustCert"].Value is true
            });
        }
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private void BtnOk_Click(object sender, EventArgs e)
    {
        CollectFields();
        if (string.IsNullOrWhiteSpace(_job.Name))
        {
            MessageBox.Show("Job name is required.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_job.Sources.Count == 0)
        {
            MessageBox.Show("Add at least one source path.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_job.Sources.Any(s => IsHttp(s.PathType)))
        {
            MessageBox.Show("HTTP/HTTPS can't be used as a source because it has no directory listing. " +
                            "Use Local/UNC or FTP/FTPS for sources.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_job.Destinations.Count == 0)
        {
            MessageBox.Show("Add at least one destination path.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_job.SourceAction == SourceFileAction.Move && string.IsNullOrWhiteSpace(_job.MoveToPath))
        {
            MessageBox.Show("Enter a 'Move to' path, or choose a different source action.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void BtnAddSource_Click(object sender, EventArgs e) => dgvSources.Rows.Add();
    private void BtnRemoveSource_Click(object sender, EventArgs e)
    {
        foreach (DataGridViewRow r in dgvSources.SelectedRows)
            if (!r.IsNewRow) dgvSources.Rows.Remove(r);
    }

    private void BtnAddDest_Click(object sender, EventArgs e) => dgvDests.Rows.Add();
    private void BtnRemoveDest_Click(object sender, EventArgs e)
    {
        foreach (DataGridViewRow r in dgvDests.SelectedRows)
            if (!r.IsNewRow) dgvDests.Rows.Remove(r);
    }

    private void BtnWildcards_Click(object sender, EventArgs e)
    {
        using var frm = new WildcardReferenceForm();
        frm.ShowDialog(this);
    }

    private static readonly HashSet<string> PasswordColumns = ["colSrcPass", "colDestPass"];

    private void MaskPasswordCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        var grid = (DataGridView)sender!;
        if (e.ColumnIndex >= 0 && PasswordColumns.Contains(grid.Columns[e.ColumnIndex].Name)
            && e.Value is string { Length: > 0 })
        {
            e.Value = "••••••••";   // fixed length, so the real length isn't revealed
            e.FormattingApplied = true;
        }
    }

    private void MaskPasswordEditor(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        // The editing TextBox is reused across columns, so set the flag every time
        var grid = (DataGridView)sender!;
        if (e.Control is TextBox tb)
            tb.UseSystemPasswordChar = PasswordColumns.Contains(grid.CurrentCell.OwningColumn.Name);
    }

    private static bool IsHttp(PathType t) => t is PathType.HTTP or PathType.HTTPS;

    private void UpdateActionVisibility()
    {
        bool isMark = cmbAction.Text == nameof(SourceFileAction.MarkProcessed);
        bool isMove = cmbAction.Text == nameof(SourceFileAction.Move);
        lblSuffix.Visible = txtSuffix.Visible = isMark;
        lblMovePath.Visible = txtMovePath.Visible = isMove;
    }

}

// ─── String extension (shared) ────────────────────────────────────────────────
internal static class StringExt
{
    public static string? NullIfEmpty(this string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s;
}
