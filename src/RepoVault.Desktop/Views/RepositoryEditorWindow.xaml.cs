using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class RepositoryEditorWindow : Window
{
    private readonly AppSession _session;
    private readonly RepositoryRecord _repo;
    private readonly List<User> _students = new();
    private readonly List<ContributorViewDto> _contributors = new();

    public RepositoryEditorWindow() : this(new AppSession(), new RepositoryRecord())
    {
    }

    public RepositoryEditorWindow(AppSession session, RepositoryRecord repo)
    {
        _session = session;
        _repo = repo;
        InitializeComponent();

        Title = "Repository Settings - " + _repo.RepoSlug;
        LblHeader.Text = $"Settings for {_repo.RepoSlug}";
        TxtTitle.Text = _repo.Title;
        TxtDescription.Text = _repo.Description ?? string.Empty;
        ChkIsLocked.IsChecked = _repo.IsLocked;

        if (_repo.DeadlineUtc.HasValue)
        {
            ChkHasDeadline.IsChecked = true;
            DpDeadline.IsEnabled = true;
            DpDeadline.SelectedDate = _repo.DeadlineUtc.Value.ToLocalTime().Date;
        }
        else
        {
            ChkHasDeadline.IsChecked = false;
            DpDeadline.IsEnabled = false;
            DpDeadline.SelectedDate = DateTime.Today.AddDays(7);
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || _repo.RepoId == 0)
        {
            return;
        }

        await LoadContributorsAndStudentsAsync();
    }

    private async Task LoadContributorsAndStudentsAsync()
    {
        try
        {
            _contributors.Clear();
            _contributors.AddRange(await _session.Contributors.ListContributorViewsByRepoAsync(_repo.RepoId));
            GridContributors.ItemsSource = null;
            GridContributors.ItemsSource = _contributors;

            _students.Clear();
            CboAddStudent.Items.Clear();
            List<User> all = await _session.Users.ListUsersAsync(UserRole.Student, false);
            foreach (User s in all)
            {
                _students.Add(s);
                CboAddStudent.Items.Add($"{s.FullName} ({s.Username})");
            }

            if (CboAddStudent.Items.Count > 0)
            {
                CboAddStudent.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not load contributors: " + ex.Message);
        }
    }

    private void ChkHasDeadline_Changed(object sender, RoutedEventArgs e)
    {
        if (DpDeadline is not null)
        {
            DpDeadline.IsEnabled = ChkHasDeadline.IsChecked == true;
        }
    }

    private async void BtnAssignStudent_Click(object sender, RoutedEventArgs e)
    {
        if (CboAddStudent.SelectedIndex < 0 || CboAddStudent.SelectedIndex >= _students.Count)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        User student = _students[CboAddStudent.SelectedIndex];
        var (ok, msg) = await _session.Contributors.AssignContributorAsync(actor, _repo.RepoId, student.UserId, "Developer");
        if (!ok)
        {
            ShowError(msg);
            return;
        }

        ErrorBanner.Visibility = Visibility.Collapsed;
        await LoadContributorsAndStudentsAsync();
    }

    private async void BtnRemoveSelected_Click(object sender, RoutedEventArgs e)
    {
        if (GridContributors.SelectedIndex < 0 || GridContributors.SelectedIndex >= _contributors.Count)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        ContributorViewDto selected = _contributors[GridContributors.SelectedIndex];
        var (ok, msg) = await _session.Contributors.RemoveContributorAsync(actor, _repo.RepoId, selected.StudentUserId);
        if (!ok)
        {
            ShowError(msg);
            return;
        }

        ErrorBanner.Visibility = Visibility.Collapsed;
        await LoadContributorsAndStudentsAsync();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        string title = TxtTitle.Text.Trim();
        string desc = TxtDescription.Text.Trim();
        DateTime? deadline = (ChkHasDeadline.IsChecked == true && DpDeadline.SelectedDate.HasValue)
            ? DpDeadline.SelectedDate.Value.AddHours(23).AddMinutes(59).ToUniversalTime()
            : null;
        bool locked = ChkIsLocked.IsChecked == true;

        if (title.Length == 0)
        {
            ShowError("Repository title is required.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        BtnSave.IsEnabled = false;
        try
        {
            var (ok, message) = await _session.Repos.UpdateRepositoryAsync(
                actor, _repo.RepoId, title, desc, deadline, locked);
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
            ShowError("Could not save settings: " + ex.Message);
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
