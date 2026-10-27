using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class CodingTaskDialog : Form
{
    private readonly AppSession _session;
    private readonly int _courseId;
    private readonly string _courseCode;

    public CodingTaskDialog(AppSession session, int courseId, string courseCode)
    {
        _session = session;
        _courseId = courseId;
        _courseCode = courseCode;
        InitializeComponent();

        Text = $"New Coding Task - {_courseCode}";
        lblHeader.Text = $"Create Coding Task ({_courseCode})";
        cboTemplate.SelectedIndex = 0;
        dtpDeadline.Value = DateTime.Now.AddDays(7);
    }

    private void chkHasDeadline_CheckedChanged(object? sender, EventArgs e)
    {
        dtpDeadline.Enabled = chkHasDeadline.Checked;
    }

    private void chkAllowGroupWork_CheckedChanged(object? sender, EventArgs e)
    {
        numMaxGroupSize.Enabled = chkAllowGroupWork.Checked;
        if (!chkAllowGroupWork.Checked)
        {
            numMaxGroupSize.Value = 1;
        }
        else if (numMaxGroupSize.Value == 1)
        {
            numMaxGroupSize.Value = 3;
        }
    }

    private async void btnCreate_Click(object? sender, EventArgs e)
    {
        errorProvider.Clear();

        string title = txtTitle.Text.Trim();
        string instructions = txtInstructions.Text.Trim();
        string template = cboTemplate.SelectedItem?.ToString() ?? "csharp";
        DateTime? deadline = chkHasDeadline.Checked ? dtpDeadline.Value.ToUniversalTime() : null;
        bool allowGroup = chkAllowGroupWork.Checked;
        int maxGroup = (int)numMaxGroupSize.Value;

        if (title.Length == 0)
        {
            errorProvider.SetError(txtTitle, "task title is required.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            MessageBox.Show(this, "you must be logged in to create tasks.", "Coding Task",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnCreate.Enabled = false;
        try
        {
            var (ok, message, _) = await _session.Courses.CreateCodingTaskAsync(
                actor, _courseId, title, instructions, template, deadline, allowGroup, maxGroup);

            if (!ok)
            {
                MessageBox.Show(this, message, "Coding Task", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, $"task '{title}' created successfully.", "Coding Task",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not create task: " + ex.Message, "Coding Task",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnCreate.Enabled = true;
        }
    }
}
