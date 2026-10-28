using System;
using System.Windows;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class CourseEditorWindow : Window
{
    private readonly AppSession _session;
    private readonly Course? _course;

    public CourseEditorWindow() : this(new AppSession(), null)
    {
    }

    public CourseEditorWindow(AppSession session, Course? course = null)
    {
        _session = session;
        _course = course;
        InitializeComponent();

        if (_course is not null)
        {
            Title = "Edit Course - " + _course.CourseCode;
            LblHeader.Text = "Edit Course";
            TxtCourseCode.Text = _course.CourseCode;
            TxtCourseTitle.Text = _course.CourseTitle;
            TxtDepartment.Text = _course.Department;
            TxtSemester.Text = _course.Semester;
        }
        else
        {
            Title = "New Course";
            LblHeader.Text = "Create New Course";
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        string code = TxtCourseCode.Text.Trim();
        string title = TxtCourseTitle.Text.Trim();
        string dept = TxtDepartment.Text.Trim();
        string sem = TxtSemester.Text.Trim();

        if (code.Length == 0 || title.Length == 0 || dept.Length == 0 || sem.Length == 0)
        {
            ShowError("Course code, title, department, and semester are all required.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            ShowError("You must be signed in.");
            return;
        }

        BtnSave.IsEnabled = false;
        try
        {
            if (_course is null)
            {
                var (ok, message, _) = await _session.Courses.CreateCourseAsync(actor, code, title, dept, sem);
                if (!ok)
                {
                    ShowError(message);
                    return;
                }
            }
            else
            {
                var (ok, message) = await _session.Courses.UpdateCourseAsync(
                    actor, _course.CourseId, code, title, dept, sem);
                if (!ok)
                {
                    ShowError(message);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError("Could not save course: " + ex.Message);
        }
        finally
        {
            BtnSave.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        LblError.Text = message;
        ErrorBanner.Visibility = Visibility.Visible;
    }
}
