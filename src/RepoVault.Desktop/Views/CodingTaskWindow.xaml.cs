using System;
using System.Windows;
using System.Windows.Controls;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class CodingTaskWindow : Window
{
    private readonly AppSession _session;
    private readonly int _courseId;
    private readonly string _courseCode;

    public CodingTaskWindow() : this(new AppSession(), 0, "CS201")
    {
    }

    public CodingTaskWindow(AppSession session, int courseId, string courseCode)
    {
        _session = session;
        _courseId = courseId;
        _courseCode = courseCode;
        InitializeComponent();

        Title = $"Post New Coding Task - {_courseCode}";
        LblHeader.Text = $"Post Coding Task ({_courseCode})";
        DpDeadline.SelectedDate = DateTime.Today.AddDays(7);
    }

    private void ChkHasDeadline_Changed(object sender, RoutedEventArgs e)
    {
        if (DpDeadline is not null)
        {
            DpDeadline.IsEnabled = ChkHasDeadline.IsChecked == true;
        }
    }

    private void ChkAllowGroupWork_Changed(object sender, RoutedEventArgs e)
    {
        if (CboMaxGroupSize is null)
        {
            return;
        }

        bool allow = ChkAllowGroupWork.IsChecked == true;
        CboMaxGroupSize.IsEnabled = allow;
        CboMaxGroupSize.SelectedIndex = allow ? 2 : 0;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        string title = TxtTitle.Text.Trim();
        string instructions = TxtInstructions.Text.Trim();
        string template = (CboStarterTemplate.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "csharp";
        DateTime? deadline = (ChkHasDeadline.IsChecked == true && DpDeadline.SelectedDate.HasValue)
            ? DpDeadline.SelectedDate.Value.AddHours(23).AddMinutes(59).ToUniversalTime()
            : null;
        bool allowGroup = ChkAllowGroupWork.IsChecked == true;
        string maxRaw = (CboMaxGroupSize.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "1";
        int maxGroup = allowGroup && int.TryParse(maxRaw, out int parsed) ? parsed : 1;

        if (title.Length == 0)
        {
            ShowError("Task title is required.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            ShowError("You must be signed in.");
            return;
        }

        BtnCreate.IsEnabled = false;
        try
        {
            var (ok, message, _) = await _session.Courses.CreateCodingTaskAsync(
                actor, _courseId, title, instructions, template, deadline, allowGroup, maxGroup);

            if (!ok)
            {
                ShowError(message);
                return;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError("Could not post task: " + ex.Message);
        }
        finally
        {
            BtnCreate.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        LblError.Text = message;
        ErrorBanner.Visibility = Visibility.Visible;
    }
}
