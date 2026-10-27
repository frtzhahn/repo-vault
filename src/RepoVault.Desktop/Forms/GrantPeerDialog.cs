using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class GrantPeerDialog : Form
{
    private readonly AppSession _session;
    private readonly int _repoId;
    private readonly List<User> _students = new();

    public GrantPeerDialog(AppSession session, int repoId)
    {
        _session = session;
        _repoId = repoId;
        InitializeComponent();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadStudentsAsync();
    }

    private async Task LoadStudentsAsync()
    {
        try
        {
            List<User> all = await _session.Users.ListUsersAsync(roleFilter: UserRole.Student, includeInactive: false);
            _students.Clear();
            cboStudents.Items.Clear();

            int currentUserId = _session.CurrentUser?.UserId ?? 0;
            foreach (User student in all)
            {
                if (student.UserId == currentUserId || !student.IsActive)
                {
                    continue;
                }

                _students.Add(student);
                cboStudents.Items.Add($"{student.FullName} ({student.Username}) - {student.IdNumber}");
            }

            if (cboStudents.Items.Count > 0)
            {
                cboStudents.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load students: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnGrant_Click(object? sender, EventArgs e)
    {
        if (cboStudents.SelectedIndex < 0 || cboStudents.SelectedIndex >= _students.Count)
        {
            MessageBox.Show(this, "please select a classmate.", "Grant Access",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            MessageBox.Show(this, "you must be logged in to grant access.", "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        User target = _students[cboStudents.SelectedIndex];
        btnGrant.Enabled = false;
        try
        {
            var (ok, message) = await _session.Contributors.GrantPeerViewerAsync(
                actor, _repoId, target.UserId);

            if (!ok)
            {
                MessageBox.Show(this, message, "Grant Access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, "peer viewer access granted.", "Grant Access",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not grant peer access: " + ex.Message, "Grant Access",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnGrant.Enabled = true;
        }
    }
}
