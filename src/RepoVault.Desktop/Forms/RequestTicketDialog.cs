using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class RequestTicketDialog : Form
{
    private readonly AppSession _session;
    private readonly int _repoId;
    private readonly List<User> _students = new();

    public RequestTicketDialog(AppSession session, int repoId)
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

    private async void btnSubmit_Click(object? sender, EventArgs e)
    {
        errorProvider.Clear();

        if (cboStudents.SelectedIndex < 0 || cboStudents.SelectedIndex >= _students.Count)
        {
            errorProvider.SetError(cboStudents, "select a classmate.");
            return;
        }

        string role = txtTeamRole.Text.Trim();
        if (role.Length == 0)
        {
            errorProvider.SetError(txtTeamRole, "team role is required.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            MessageBox.Show(this, "you must be logged in to request tickets.", "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        User target = _students[cboStudents.SelectedIndex];
        btnSubmit.Enabled = false;
        try
        {
            var (ok, message) = await _session.Contributors.RequestGroupmateTicketAsync(
                actor, _repoId, target.UserId, role);

            if (!ok)
            {
                MessageBox.Show(this, message, "Request Ticket", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, "groupmate ticket submitted. waiting for faculty review.",
                "Request Ticket", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not submit ticket: " + ex.Message, "Request Ticket",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnSubmit.Enabled = true;
        }
    }
}
