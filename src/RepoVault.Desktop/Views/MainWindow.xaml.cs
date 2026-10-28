using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    private Button? _activeNavButton;

    public MainWindow() : this(new AppSession())
    {
    }

    public MainWindow(AppSession session)
    {
        _session = session;
        InitializeComponent();
        UpdateThemeButtonLabel();

        string lanIp = LanHelper.GetPreferredLanIpv4Address();
        LblLanInfo.Text = $"LAN: {lanIp}:{_session.ServerPort}";

        var user = _session.CurrentUser;
        if (user is not null)
        {
            LblLoggedInUser.Text = $"{user.FullName} ({user.Username})";
            if (user.Role == UserRole.Faculty)
            {
                LblRoleBadge.Text = "FACULTY WORKSTATION";
                RoleBadgeBorder.BorderBrush = (Brush)FindResource("AccentPrimaryBrush");
                Title = $"Repo Vault - Faculty Workstation ({user.FullName})";
                FacultyNavPanel.Visibility = Visibility.Visible;
                StudentNavPanel.Visibility = Visibility.Collapsed;

                var fw = new FacultyWorkspaceView(
                    _session, SetStatus, ToggleSidebar, SetSidebarCollapsed,
                    UpdateFacultyNavHighlight, UpdatePendingTicketsBadge);
                WorkspaceHost.Content = fw;
                HighlightNavButton(BtnFacultyNavCourses);
            }
            else
            {
                LblRoleBadge.Text = "STUDENT WORKSTATION";
                RoleBadgeBorder.BorderBrush = (Brush)FindResource("AccentBadgeBrush");
                Title = $"Repo Vault - Student Workstation ({user.FullName})";
                StudentNavPanel.Visibility = Visibility.Visible;
                FacultyNavPanel.Visibility = Visibility.Collapsed;

                var sw = new StudentWorkspaceView(
                    _session, SetStatus, ToggleSidebar, SetSidebarCollapsed,
                    UpdateStudentNavHighlight);
                WorkspaceHost.Content = sw;
                HighlightNavButton(BtnStudentNavWorkspaces);
            }
        }

        UpdateServerUi();
    }

    public bool LogoutRequested { get; private set; }

    public void SetStatus(string message)
    {
        LblStatusBar.Text = message;
    }

    public void ToggleSidebar()
    {
        bool currentlyVisible = LeftSidebarBorder.Visibility == Visibility.Visible;
        SetSidebarCollapsed(currentlyVisible);
    }

    public void SetSidebarCollapsed(bool collapsed)
    {
        LeftSidebarBorder.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
    }

    public void UpdatePendingTicketsBadge(int pendingCount)
    {
        BtnFacultyNavTickets.Content = pendingCount > 0
            ? $"3. Groupmate Tickets ({pendingCount})"
            : "3. Groupmate Tickets";
    }

    private void UpdateStudentNavHighlight(int pageIndex)
    {
        Button? target = pageIndex switch
        {
            0 => BtnStudentNavWorkspaces,
            1 => BtnStudentNavTasks,
            2 => BtnStudentNavTickets,
            _ => null,
        };
        HighlightNavButton(target);
    }

    private void UpdateFacultyNavHighlight(int pageIndex)
    {
        Button? target = pageIndex switch
        {
            0 => BtnFacultyNavCourses,
            1 => BtnFacultyNavSubmissions,
            2 => BtnFacultyNavTickets,
            3 => BtnFacultyNavTrash,
            _ => null,
        };
        HighlightNavButton(target);
    }

    private void HighlightNavButton(Button? active)
    {
        Button[] all =
        [
            BtnStudentNavWorkspaces,
            BtnStudentNavTasks,
            BtnStudentNavTickets,
            BtnFacultyNavCourses,
            BtnFacultyNavSubmissions,
            BtnFacultyNavTickets,
            BtnFacultyNavTrash,
        ];

        foreach (Button btn in all)
        {
            btn.Background = Brushes.Transparent;
            btn.BorderBrush = Brushes.Transparent;
            btn.FontWeight = FontWeights.Normal;
        }

        _activeNavButton = active;
        if (active is not null)
        {
            active.Background = (Brush)FindResource("BgElevatedBrush");
            active.BorderBrush = (Brush)FindResource("AccentPrimaryBrush");
            active.FontWeight = FontWeights.SemiBold;
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SeederResult seeded = await _session.Seeder.EnsureDemoSeededAsync(_session.ReposDirectory);
            if (seeded.ReposSeeded > 0)
            {
                SetStatus($"Auto-initialized {seeded.ReposSeeded} demo repositories ({seeded.CommitsIndexed} commits indexed).");
            }
        }
        catch (Exception ex)
        {
            SetStatus("Demo auto-check warning: " + ex.Message);
        }

        if (WorkspaceHost.Content is StudentWorkspaceView sw)
        {
            await sw.RefreshDataAsync();
        }
        else if (WorkspaceHost.Content is FacultyWorkspaceView fw)
        {
            await fw.RefreshDataAsync();
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.B && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            ToggleSidebar();
            e.Handled = true;
        }
    }

    private void BtnToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        ToggleSidebar();
    }

    private void BtnStudentNavWorkspaces_Click(object sender, RoutedEventArgs e)
    {
        if (WorkspaceHost.Content is StudentWorkspaceView sw)
        {
            sw.NavigateToPage(0);
        }
    }

    private void BtnStudentNavTasks_Click(object sender, RoutedEventArgs e)
    {
        if (WorkspaceHost.Content is StudentWorkspaceView sw)
        {
            sw.NavigateToPage(1);
        }
    }

    private void BtnStudentNavTickets_Click(object sender, RoutedEventArgs e)
    {
        if (WorkspaceHost.Content is StudentWorkspaceView sw)
        {
            sw.NavigateToPage(2);
        }
    }

    private void BtnFacultyNavCourses_Click(object sender, RoutedEventArgs e)
    {
        if (WorkspaceHost.Content is FacultyWorkspaceView fw)
        {
            fw.NavigateToPage(0);
        }
    }

    private void BtnFacultyNavSubmissions_Click(object sender, RoutedEventArgs e)
    {
        if (WorkspaceHost.Content is FacultyWorkspaceView fw)
        {
            fw.NavigateToPage(1);
        }
    }

    private void BtnFacultyNavTickets_Click(object sender, RoutedEventArgs e)
    {
        if (WorkspaceHost.Content is FacultyWorkspaceView fw)
        {
            fw.NavigateToPage(2);
        }
    }

    private void BtnFacultyNavTrash_Click(object sender, RoutedEventArgs e)
    {
        if (WorkspaceHost.Content is FacultyWorkspaceView fw)
        {
            fw.NavigateToPage(3);
        }
    }

    private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        App.ToggleTheme();
        UpdateThemeButtonLabel();
        UpdateServerUi();
        HighlightNavButton(_activeNavButton);
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
            LblPushCounter.Text = $"Pushes: {_pushCount}";
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
            ? $"Git Server: http://{host}:{_session.ServerPort}/"
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
