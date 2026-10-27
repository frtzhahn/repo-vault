using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class CourseEditorDialog : Form
{
    private readonly AppSession _session;
    private readonly Course? _course;

    public CourseEditorDialog(AppSession session, Course? course = null)
    {
        _session = session;
        _course = course;
        InitializeComponent();

        if (_course is not null)
        {
            lblHeader.Text = "Edit Course";
            Text = "Edit Course - " + _course.CourseCode;
            txtCourseCode.Text = _course.CourseCode;
            txtCourseTitle.Text = _course.CourseTitle;
            txtDepartment.Text = _course.Department;
            txtSemester.Text = _course.Semester;
        }
        else
        {
            lblHeader.Text = "New Course";
            Text = "New Course";
        }
    }

    private async void btnSave_Click(object? sender, EventArgs e)
    {
        errorProvider.Clear();

        string code = txtCourseCode.Text.Trim();
        string title = txtCourseTitle.Text.Trim();
        string dept = txtDepartment.Text.Trim();
        string sem = txtSemester.Text.Trim();

        if (code.Length == 0)
        {
            errorProvider.SetError(txtCourseCode, "course code is required.");
            return;
        }

        if (title.Length == 0)
        {
            errorProvider.SetError(txtCourseTitle, "course title is required.");
            return;
        }

        if (dept.Length == 0)
        {
            errorProvider.SetError(txtDepartment, "department is required.");
            return;
        }

        if (sem.Length == 0)
        {
            errorProvider.SetError(txtSemester, "semester is required.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            MessageBox.Show(this, "you must be logged in to manage courses.", "Course Editor",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnSave.Enabled = false;
        try
        {
            if (_course is null)
            {
                var (ok, message, _) = await _session.Courses.CreateCourseAsync(
                    actor, code, title, dept, sem);
                if (!ok)
                {
                    MessageBox.Show(this, message, "Course Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                var (ok, message) = await _session.Courses.UpdateCourseAsync(
                    actor, _course.CourseId, code, title, dept, sem);
                if (!ok)
                {
                    MessageBox.Show(this, message, "Course Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            MessageBox.Show(this, "course saved successfully.", "Course Editor",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not save course: " + ex.Message, "Course Editor",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }
}
