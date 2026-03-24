using FileRedirector.Models;
using FileRedirector.Wildcards;

namespace FileRedirector.UI;

public partial class JobEditorForm : Form
{
    private readonly RedirectJob _job;
    private readonly bool        _isNew;

    public RedirectJob Result => _job;

    public JobEditorForm(RedirectJob? job = null)
    {
        _isNew = job is null;
        _job   = job ?? new RedirectJob { CreatedAt = DateTime.UtcNow.ToString("o") };

        InitializeComponent();

        Text = _isNew ? "New Redirect Job" : "Edit Redirect Job";

        // Populate combo with enum values
        cmbAction.Items.AddRange(Enum.GetNames<SourceFileAction>());

        // Populate source-grid PathType combos
        ((DataGridViewComboBoxColumn)dgvSources.Columns["PathType"]!)
            .Items.AddRange(Enum.GetNames<PathType>());
        ((DataGridViewComboBoxColumn)dgvDests.Columns["PathType"]!)
            .Items.AddRange(Enum.GetNames<PathType>());

        PopulateFields();

        cmbAction.SelectedIndexChanged += (_, _) => UpdateActionVisibility();
        UpdateActionVisibility();
    }

    // ─── Populate ─────────────────────────────────────────────────────────────

    private void PopulateFields()
    {
        txtName.Text       = _job.Name;
        chkEnabled.Checked = _job.IsEnabled;
        nudPoll.Value      = Math.Clamp(_job.PollIntervalSeconds, 5, 86400);
        cmbAction.Text     = _job.SourceAction.ToString();
        txtMovePath.Text   = _job.MoveToPath ?? string.Empty;
        txtSuffix.Text     = _job.ProcessedSuffix ?? ".done";
        txtNotes.Text      = _job.Notes ?? string.Empty;

        foreach (var src in _job.Sources)
            dgvSources.Rows.Add(src.Path, src.FilePattern, src.PathType.ToString(),
                                src.Username, src.Password, src.IsPassive);

        foreach (var dest in _job.Destinations)
            dgvDests.Rows.Add(dest.Path, dest.FileNameTemplate, dest.PathType.ToString(),
                              dest.Username, dest.Password, dest.IsPassive);
    }

    // ─── Collect ──────────────────────────────────────────────────────────────

    private void CollectFields()
    {
        _job.Name                = txtName.Text.Trim();
        _job.IsEnabled           = chkEnabled.Checked;
        _job.PollIntervalSeconds = (int)nudPoll.Value;
        _job.SourceAction        = Enum.Parse<SourceFileAction>(cmbAction.Text);
        _job.MoveToPath          = txtMovePath.Text.NullIfEmpty();
        _job.ProcessedSuffix     = txtSuffix.Text.NullIfEmpty() ?? ".done";
        _job.Notes               = txtNotes.Text.NullIfEmpty();

        _job.Sources.Clear();
        foreach (DataGridViewRow r in dgvSources.Rows)
        {
            if (r.IsNewRow) continue;
            var path = r.Cells["Path"].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(path)) continue;
            _job.Sources.Add(new JobSource
            {
                Path        = path,
                FilePattern = r.Cells["Pattern"].Value?.ToString()?.Trim().NullIfEmpty() ?? "*.*",
                PathType    = Enum.TryParse<PathType>(r.Cells["PathType"].Value?.ToString(), out var spt)
                                  ? spt : PathType.LocalOrUNC,
                Username    = r.Cells["Username"].Value?.ToString().NullIfEmpty(),
                Password    = r.Cells["Password"].Value?.ToString().NullIfEmpty(),
                IsPassive   = r.Cells["Passive"].Value is true
            });
        }

        _job.Destinations.Clear();
        int ord = 0;
        foreach (DataGridViewRow r in dgvDests.Rows)
        {
            if (r.IsNewRow) continue;
            var path = r.Cells["Path"].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(path)) continue;
            bool passive = dgvDests.Columns.Contains("Passive") &&
                           r.Cells["Passive"].Value is true;
            _job.Destinations.Add(new JobDestination
            {
                Path             = path,
                FileNameTemplate = r.Cells["Pattern"].Value?.ToString().NullIfEmpty(),
                PathType         = Enum.TryParse<PathType>(r.Cells["PathType"].Value?.ToString(), out var dpt)
                                       ? dpt : PathType.LocalOrUNC,
                Username         = r.Cells["Username"].Value?.ToString().NullIfEmpty(),
                Password         = r.Cells["Password"].Value?.ToString().NullIfEmpty(),
                SortOrder        = ord++,
                IsPassive        = passive
            });
        }
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private void btnOk_Click(object sender, EventArgs e)
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
        if (_job.Destinations.Count == 0)
        {
            MessageBox.Show("Add at least one destination path.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnAddSource_Click(object sender, EventArgs e)      => dgvSources.Rows.Add();
    private void btnRemoveSource_Click(object sender, EventArgs e)
    {
        foreach (DataGridViewRow r in dgvSources.SelectedRows)
            if (!r.IsNewRow) dgvSources.Rows.Remove(r);
    }

    private void btnAddDest_Click(object sender, EventArgs e)        => dgvDests.Rows.Add();
    private void btnRemoveDest_Click(object sender, EventArgs e)
    {
        foreach (DataGridViewRow r in dgvDests.SelectedRows)
            if (!r.IsNewRow) dgvDests.Rows.Remove(r);
    }

    private void btnWildcards_Click(object sender, EventArgs e)
    {
        using var frm = new WildcardReferenceForm();
        frm.ShowDialog(this);
    }

    private void UpdateActionVisibility()
    {
        bool isMark = cmbAction.Text == nameof(SourceFileAction.MarkProcessed);
        bool isMove = cmbAction.Text == nameof(SourceFileAction.Move);
        lblSuffix.Visible   = txtSuffix.Visible   = isMark;
        lblMovePath.Visible = txtMovePath.Visible = isMove;
    }
}

// ─── String extension (shared) ────────────────────────────────────────────────
internal static class StringExt
{
    public static string? NullIfEmpty(this string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s;
}
