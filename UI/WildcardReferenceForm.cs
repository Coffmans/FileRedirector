using FileRedirector.Wildcards;

namespace FileRedirector.UI;

public partial class WildcardReferenceForm : Form
{
    public WildcardReferenceForm()
    {
        InitializeComponent();
        PopulateGrid();
    }

    private void PopulateGrid()
    {
        foreach (var (token, example) in WildcardEngine.GetTokenPreviews())
            dgvTokens.Rows.Add(token, example);
    }
}
