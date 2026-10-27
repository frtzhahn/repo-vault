using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class RepoBinDialog : Form
{
    private readonly AppSession _session;
    private readonly List<RepositoryRecord> _trashed = new();

    public RepoBinDialog(AppSession session)
    {
        _session = session;
        InitializeComponent();
        ConfigureGrid();
    }

    private void ConfigureGrid()
    {
        dgvRepoBin.Columns.Clear();
        dgvRepoBin.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRepoId", Visible = false });
        dgvRepoBin.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSlug", HeaderText = "Slug", Width = 140 });
        dgvRepoBin.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Repository Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvRepoBin.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTrashedAt", HeaderText = "Trashed At", Width = 130 });
        dgvRepoBin.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLang", HeaderText = "Lang", Width = 70 });
        dgvRepoBin.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFiles", HeaderText = "Files", Width = 60 });
        dgvRepoBin.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLines", HeaderText = "Lines", Width = 75 });
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadTrashedReposAsync();
    }

    private async Task LoadTrashedReposAsync()
    {
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        try
        {
            var (ok, message, repos) = await _session.Repos.ListTrashedInRepoBinAsync(actor);
            if (!ok)
            {
                MessageBox.Show(this, message, "Repo Bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _trashed.Clear();
            _trashed.AddRange(repos);

            dgvRepoBin.Rows.Clear();
            foreach (RepositoryRecord r in _trashed)
            {
                string trashedAt = r.TrashedAt.HasValue
                    ? r.TrashedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    : "Unknown";

                dgvRepoBin.Rows.Add(
                    r.RepoId, r.RepoSlug, r.Title, trashedAt,
                    r.PrimaryLanguage ?? "None", r.TotalFiles, r.SourceLines);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load repo bin: " + ex.Message, "Repo Bin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnRestore_Click(object? sender, EventArgs e)
    {
        if (dgvRepoBin.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "please select a repository to restore.", "Repo Bin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int index = dgvRepoBin.SelectedRows[0].Index;
        if (index < 0 || index >= _trashed.Count)
        {
            return;
        }

        RepositoryRecord repo = _trashed[index];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        btnRestore.Enabled = false;
        try
        {
            var (ok, message) = await _session.Repos.RestoreFromRepoBinAsync(actor, repo.RepoId);
            if (!ok)
            {
                MessageBox.Show(this, message, "Restore Repository", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, $"repository '{repo.Title}' restored to active status.", "Restore Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadTrashedReposAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not restore repository: " + ex.Message, "Restore Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnRestore.Enabled = true;
        }
    }

    private async void btnPurge_Click(object? sender, EventArgs e)
    {
        if (dgvRepoBin.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "please select a repository to purge.", "Repo Bin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int index = dgvRepoBin.SelectedRows[0].Index;
        if (index < 0 || index >= _trashed.Count)
        {
            return;
        }

        RepositoryRecord repo = _trashed[index];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        DialogResult confirm = MessageBox.Show(this,
            $"are you absolutely sure you want to PERMANENTLY PURGE '{repo.Title}' ({repo.RepoSlug})?\n\nThis will permanently delete the bare repository disk path and database records. This action CANNOT be undone.",
            "Confirm Permanent Purge", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        btnPurge.Enabled = false;
        try
        {
            var (ok, message) = await _session.Repos.PurgeFromRepoBinAsync(actor, repo.RepoId);
            if (!ok)
            {
                MessageBox.Show(this, message, "Purge Repository", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, $"repository '{repo.Title}' permanently purged.", "Purge Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadTrashedReposAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not purge repository: " + ex.Message, "Purge Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnPurge.Enabled = true;
        }
    }
}
