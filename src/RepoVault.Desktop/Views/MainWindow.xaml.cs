using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Media;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly AppSession _session;
    private int _pushCount;

    public MainWindow() : this(new AppSession())
    {
    }

    public MainWindow(AppSession session)
    {
        _session = session;
        InitializeComponent();
        UpdateThemeButtonLabel();

        string lanIp = LanHelper.GetPreferredLanIpv4Address();
        LblLanInfo.Text = $"LAN IPv4: {lanIp}  |  Port: {_session.ServerPort}  |  Storage: {_session.ReposDirectory}";

        var user = _session.CurrentUser;
        if (user is not null)
        {
            LblLoggedInUser.Text = $"{user.FullName} ({user.Username})";
            if (user.Role == UserRole.Faculty)
            {
                LblRoleBadge.Text = "FACULTY WORKSTATION";
                RoleBadgeBorder.Background = (Brush)FindResource("AccentPrimaryBrush");
                Title = $"Repo Vault - Faculty Workstation ({user.FullName})";
                WorkspaceHost.Content = new FacultyWorkspaceView(_session, SetStatus);
            }
            else
            {
                LblRoleBadge.Text = "STUDENT WORKSTATION";
                RoleBadgeBorder.Background = (Brush)FindResource("AccentBadgeBrush");
                Title = $"Repo Vault - Student Workstation ({user.FullName})";
                WorkspaceHost.Content = new StudentWorkspaceView(_session, SetStatus);
            }
        }

        UpdateServerUi();
    }

    public bool LogoutRequested { get; private set; }

    public void SetStatus(string message)
    {
        LblStatusBar.Text = message;
    }

    private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        App.ToggleTheme();
        UpdateThemeButtonLabel();
        UpdateServerUi();
    }

    private void UpdateThemeButtonLabel()
    {
        BtnThemeToggle.Content = App.IsDarkMode ? "Theme: Dark" : "Theme: Light";
    }

    private async void BtnServerToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_session.RunningServer is { IsRunning: true })
        {
            await StopServerAsync();
        }
        else
        {
            await StartServerAsync();
        }
    }

    private async Task StartServerAsync()
    {
        if (_session.RunningServer is { IsRunning: true })
        {
            return;
        }

        int port = _session.ServerPort;
        string lanIp = LanHelper.GetPreferredLanIpv4Address();
        bool loopbackFallback = false;

        GitHttpServer server = CreateServer($"http://+:{port}/");
        try
        {
            await server.StartAsync();
        }
        catch (HttpListenerException)
        {
            server.Dispose();
            loopbackFallback = true;

            // wildcard binding requires urlacl rule; fall back to loopback
            server = CreateServer($"http://127.0.0.1:{port}/");
            try
            {
                await server.StartAsync();
            }
            catch (HttpListenerException ex)
            {
                server.Dispose();
                MessageBox.Show(this,
                    $"Could not start Git HTTP Server on port {port}: {ex.Message}\n\n" +
                    "If the port is in use, close the other process. For LAN binding, run scripts/enable-lan-port.bat as Administrator.",
                    "Git Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        server.PushCompleted += Server_PushCompleted;
        _session.RunningServer = server;
        UpdateServerUi();

        if (loopbackFallback)
        {
            SetStatus($"Git Server running on http://127.0.0.1:{port}/ (run scripts/enable-lan-port.bat as Admin for LAN wildcard binding).");
        }
        else
        {
            SetStatus($"Git Server started on http://{lanIp}:{port}/");
        }
    }

    private async Task StopServerAsync()
    {
        GitHttpServer? server = _session.RunningServer;
        if (server is null)
        {
            return;
        }

        server.PushCompleted -= Server_PushCompleted;
        _session.RunningServer = null;
        try
        {
            await server.StopAsync();
        }
        finally
        {
            server.Dispose();
        }

        UpdateServerUi();
        SetStatus("Git HTTP Server stopped.");
    }

    private GitHttpServer CreateServer(string prefix)
    {
        return new GitHttpServer(
            _session.Db, _session.Auth, _session.Rbac, _session.Repos,
            _session.ServerPort, prefix);
    }

    private void Server_PushCompleted(object? sender, PushCompletedEventArgs e)
    {
        _ = Dispatcher.InvokeAsync(async () =>
        {
            _pushCount++;
            LblPushCounter.Text = $"Pushes Received: {_pushCount}";
            SetStatus($"Push received on repo #{e.RepoId} ({e.BranchRef}) at {e.PushedAt.ToLocalTime():HH:mm:ss}.");

            if (WorkspaceHost.Content is StudentWorkspaceView sw)
            {
                await sw.RefreshDataAsync();
            }
            else if (WorkspaceHost.Content is FacultyWorkspaceView fw)
            {
                await fw.RefreshDataAsync();
            }
        });
    }

    private void UpdateServerUi()
    {
        bool running = _session.RunningServer is { IsRunning: true };
        string host = LanHelper.GetPreferredLanIpv4Address();

        BtnServerToggle.Content = running ? "Stop Git Server" : "Start Git Server";
        LblServerStatus.Text = running
            ? $"Running on http://{host}:{_session.ServerPort}/"
            : "Git Server: Stopped";
        ServerDot.Fill = running
            ? (Brush)FindResource("AccentSuccessBrush")
            : (Brush)FindResource("AccentDangerBrush");
    }

    private void BtnCopyCloneBase_Click(object sender, RoutedEventArgs e)
    {
        string url = $"http://{LanHelper.GetPreferredLanIpv4Address()}:{_session.ServerPort}/repos/";
        try
        {
            Clipboard.SetText(url);
            SetStatus($"Copied clone base URL to clipboard: {url}<slug>.git");
        }
        catch (Exception ex)
        {
            SetStatus("Clipboard error: " + ex.Message);
        }
    }

    private async void BtnSeedDemo_Click(object sender, RoutedEventArgs e)
    {
        BtnSeedDemo.IsEnabled = false;
        SetStatus("Seeding and indexing classroom demo repositories...");
        try
        {
            SeederResult res = await _session.Seeder.SeedAllAsync(_session.ReposDirectory);
            string summary = $"Seeded {res.ReposSeeded} repositories and indexed {res.CommitsIndexed} commits.";
            SetStatus(summary);

            if (WorkspaceHost.Content is StudentWorkspaceView sw)
            {
                await sw.RefreshDataAsync();
            }
            else if (WorkspaceHost.Content is FacultyWorkspaceView fw)
            {
                await fw.RefreshDataAsync();
            }
        }
        catch (Exception ex)
        {
            SetStatus("Seed error: " + ex.Message);
            MessageBox.Show(this, "Could not seed demo workspaces: " + ex.Message,
                "Seed Demo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnSeedDemo.IsEnabled = true;
        }
    }

    private async void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        await StopServerAsync();
        _session.Reset();
        LogoutRequested = true;
        Close();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        GitHttpServer? server = _session.RunningServer;
        if (server is not null)
        {
            server.PushCompleted -= Server_PushCompleted;
            _session.RunningServer = null;
            try
            {
                server.StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
            }

            server.Dispose();
        }
    }
}
