using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class RepoBinWindow : Window
{
    private readonly AppSession _session;
    private readonly List<RepositoryRecord> _trashed = new();

    public RepoBinWindow() : this(new AppSession())
    {
    }

    public RepoBinWindow(AppSession session)
    {
        _session = session;
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        await LoadTrashedAsync();
    }

    private async Task LoadTrashedAsync()
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
                MessageBox.Show(this, message, "Repo Bin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _trashed.Clear();
            _trashed.AddRange(repos);

            GridTrashedRepos.ItemsSource = _trashed.Select(r => new TrashedRepoRow(
                r.RepoId,
                r.RepoSlug,
                r.Title,
                r.PrimaryLanguage ?? "None",
                r.TotalFiles,
                r.SourceLines,
                r.TrashedAt.HasValue ? r.TrashedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "Unknown")).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not load Repo Bin: " + ex.Message,
                "Repo Bin", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnRestoreSelected_Click(object sender, RoutedEventArgs e)
    {
        if (GridTrashedRepos.SelectedIndex < 0 || GridTrashedRepos.SelectedIndex >= _trashed.Count)
        {
            MessageBox.Show(this, "Select a trashed repository to restore.",
                "Repo Bin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        RepositoryRecord repo = _trashed[GridTrashedRepos.SelectedIndex];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        BtnRestoreSelected.IsEnabled = false;
        try
        {
            var (ok, message) = await _session.Repos.RestoreFromRepoBinAsync(actor, repo.RepoId);
            if (!ok)
            {
                MessageBox.Show(this, message, "Restore Repository", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await LoadTrashedAsync();
        }
        finally
        {
            BtnRestoreSelected.IsEnabled = true;
        }
    }

    private async void BtnPurgePermanently_Click(object sender, RoutedEventArgs e)
    {
        if (GridTrashedRepos.SelectedIndex < 0 || GridTrashedRepos.SelectedIndex >= _trashed.Count)
        {
            MessageBox.Show(this, "Select a trashed repository to purge.",
                "Repo Bin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        RepositoryRecord repo = _trashed[GridTrashedRepos.SelectedIndex];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        MessageBoxResult confirm = MessageBox.Show(this,
            $"Permanently purge '{repo.Title}' ({repo.RepoSlug})?\n\nThis deletes the bare Git repository from disk and removes all MariaDB records. This cannot be undone.",
            "Confirm Permanent Purge",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        BtnPurgePermanently.IsEnabled = false;
        try
        {
            var (ok, message) = await _session.Repos.PurgeFromRepoBinAsync(actor, repo.RepoId);
            if (!ok)
            {
                MessageBox.Show(this, message, "Purge Repository", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await LoadTrashedAsync();
        }
        finally
        {
            BtnPurgePermanently.IsEnabled = true;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private sealed record TrashedRepoRow(
        int RepoId, string RepoSlug, string Title, string PrimaryLang, int TotalFiles, int SourceLines, string TrashedAtText);
}
