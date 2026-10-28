using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class GrantPeerWindow : Window
{
    private readonly AppSession _session;
    private readonly int _repoId;
    private readonly List<User> _students = new();

    public GrantPeerWindow() : this(new AppSession(), 0)
    {
    }

    public GrantPeerWindow(AppSession session, int repoId)
    {
        _session = session;
        _repoId = repoId;
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        try
        {
            List<User> all = await _session.Users.ListUsersAsync(UserRole.Student, false);
            _students.Clear();
            CboStudents.Items.Clear();

            int currentUserId = _session.CurrentUser?.UserId ?? 0;
            foreach (User student in all)
            {
                if (student.UserId == currentUserId || !student.IsActive)
                {
                    continue;
                }

                _students.Add(student);
                CboStudents.Items.Add($"{student.FullName} ({student.Username}) - {student.IdNumber}");
            }

            if (CboStudents.Items.Count > 0)
            {
                CboStudents.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not load student list: " + ex.Message);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void BtnGrant_Click(object sender, RoutedEventArgs e)
    {
        if (CboStudents.SelectedIndex < 0 || CboStudents.SelectedIndex >= _students.Count)
        {
            ShowError("Select a classmate.");
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            ShowError("You must be signed in.");
            return;
        }

        User target = _students[CboStudents.SelectedIndex];
        BtnGrant.IsEnabled = false;
        try
        {
            var (ok, message) = await _session.Contributors.GrantPeerViewerAsync(
                actor, _repoId, target.UserId);

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
            ShowError("Could not grant peer access: " + ex.Message);
        }
        finally
        {
            BtnGrant.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        LblError.Text = message;
        ErrorBanner.Visibility = Visibility.Visible;
    }
}
