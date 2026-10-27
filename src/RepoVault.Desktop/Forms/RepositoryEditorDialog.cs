using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class RepositoryEditorDialog : Form
{
    private readonly AppSession _session;
    private readonly RepositoryRecord _repo;

    public RepositoryEditorDialog(AppSession session, RepositoryRecord repo)
    {
        _session = session;
        _repo = repo;
        InitializeComponent();

        Text = "Repository Settings - " + _repo.RepoSlug;
        lblHeader.Text = "Settings for " + _repo.RepoSlug;
        txtTitle.Text = _repo.Title;
        txtDesc.Text = _repo.Description ?? string.Empty;
        chkLock.Checked = _repo.IsLocked;

        if (_repo.DeadlineUtc.HasValue)
        {
            chkHasDeadline.Checked = true;
            dtpDeadline.Enabled = true;
            dtpDeadline.Value = _repo.DeadlineUtc.Value.ToLocalTime();
        }
        else
        {
            chkHasDeadline.Checked = false;
            dtpDeadline.Enabled = false;
            dtpDeadline.Value = DateTime.Now.AddDays(7);
        }
    }

    private void chkHasDeadline_CheckedChanged(object? sender, EventArgs e)
    {
        dtpDeadline.Enabled = chkHasDeadline.Checked;
    }

    private async void btnSave_Click(object? sender, EventArgs e)
    {
        errorProvider.Clear();

        string title = txtTitle.Text.Trim();
        string desc = txtDesc.Text.Trim();
        DateTime? deadline = chkHasDeadline.Checked ? dtpDeadline.Value.ToUniversalTime() : null;
        bool isLocked = chkLock.Checked;

        if (title.Length == 0)
        {
            errorProvider.SetError(txtTitle, "repository title is required.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            MessageBox.Show(this, "you must be logged in to modify repositories.", "Repository Settings",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnSave.Enabled = false;
        try
        {
            var (updateOk, updateMsg) = await _session.Repos.UpdateRepositoryAsync(
                actor, _repo.RepoId, title, desc, deadline, isLocked);

            if (!updateOk)
            {
                MessageBox.Show(this, updateMsg, "Repository Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, "repository settings saved successfully.", "Repository Settings",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not save settings: " + ex.Message, "Repository Settings",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }
}
