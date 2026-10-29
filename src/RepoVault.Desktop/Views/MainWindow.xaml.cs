using System;
using System.ComponentModel;
using System.Threading.Tasks;
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

        _session.ServerPushCompleted += Server_PushCompleted;
        _session.ServerStateChanged += Session_ServerStateChanged;

        var user = _session.CurrentUser;
        if (user is not null)
        {
            LblLoggedInUser.Text = $"{user.FullName} ({user.Username})";
            if (user.Role == UserRole.Faculty)
            {
                LblRoleBadge.Text = "FACULTY";
                RoleBadgeBorder.BorderBrush = (Brush)FindResource("AccentPrimaryBrush");
                Title = $"Repo Vault - Faculty Workstation ({user.FullName})";
                FacultyNavPanel.Visibility = Visibility.Visible;
                StudentNavPanel.Visibility = Visibility.Collapsed;

                var fw = new FacultyWorkspaceView(
                    _session, SetStatus, ToggleSidebar, SetSidebarCollapsed,
                    UpdateFacultyNavHighlight, UpdatePendingTicketsBadge);
                fw.SetSidebarOpenState(true);
                WorkspaceHost.Content = fw;
                HighlightNavButton(BtnFacultyNavCourses);
            }
            else
            {
                LblRoleBadge.Text = "STUDENT";
                RoleBadgeBorder.BorderBrush = (Brush)FindResource("AccentBadgeBrush");
                Title = $"Repo Vault - Student Workstation ({user.FullName})";
                StudentNavPanel.Visibility = Visibility.Visible;
                FacultyNavPanel.Visibility = Visibility.Collapsed;

                var sw = new StudentWorkspaceView(
                    _session, SetStatus, ToggleSidebar, SetSidebarCollapsed,
                    UpdateStudentNavHighlight);
                sw.SetSidebarOpenState(true);
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
        bool isSidebarOpen = !collapsed;

        if (WorkspaceHost.Content is StudentWorkspaceView sw)
        {
            sw.SetSidebarOpenState(isSidebarOpen);
        }
        else if (WorkspaceHost.Content is FacultyWorkspaceView fw)
        {
            fw.SetSidebarOpenState(isSidebarOpen);
        }
    }

    public void UpdatePendingTicketsBadge(int pendingCount)
    {
        LblFacultyNavTicketsText.Text = pendingCount > 0
            ? $"Groupmate Tickets ({pendingCount})"
            : "Groupmate Tickets";
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
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        // auto-start the in-process git http server immediately on login
        await StartServerAsync(silentOnError: true);

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
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (e.Key == Key.B)
            {
                ToggleSidebar();
                e.Handled = true;
            }
            else if (e.Key == Key.J)
            {
                if (WorkspaceHost.Content is StudentWorkspaceView sw)
                {
                    sw.ToggleBottomPanel();
                    e.Handled = true;
                }
                else if (WorkspaceHost.Content is FacultyWorkspaceView fw)
                {
                    fw.ToggleBottomPanel();
                    e.Handled = true;
                }
            }
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
        LblThemeToggleText.Text = App.IsDarkMode ? "Dark" : "Light";
    }

    private async void BtnServerToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_session.Server is { IsRunning: true })
        {
            await StopServerAsync();
        }
        else
        {
            await StartServerAsync(silentOnError: false);
        }
    }

    private async Task StartServerAsync(bool silentOnError)
    {
        var (ok, loopbackFallback, errorMsg) = await _session.EnsureServerRunningAsync();
        UpdateServerUi();

        if (!ok)
        {
            if (!silentOnError)
            {
                MessageBox.Show(this,
                    $"Could not start Git HTTP Server on port {_session.ServerPort}: {errorMsg}\n\n" +
                    "If the port is in use, close the other process. For LAN binding, run scripts/enable-lan-port.bat as Administrator.",
                    "Git Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                SetStatus("Git Server could not bind port " + _session.ServerPort + ".");
            }

            return;
        }

        string hostDisplay = _session.GetActiveServerHostDisplay();
        SetStatus(loopbackFallback
            ? $"Git Server running on http://{hostDisplay}/"
            : $"Git Server started on http://{hostDisplay}/");
    }

    private async Task StopServerAsync()
    {
        await _session.StopServerAsync();
        UpdateServerUi();
        SetStatus("Git HTTP Server stopped.");
    }

    private void Session_ServerStateChanged(object? sender, EventArgs e)
    {
        _ = Dispatcher.InvokeAsync(UpdateServerUi);
    }

    private void Server_PushCompleted(object? sender, PushCompletedEventArgs e)
    {
        _ = Dispatcher.InvokeAsync(async () =>
        {
            _pushCount++;
            LblPushCounter.Text = $"Pushes: {_pushCount}";
            SetStatus($"Push on repo #{e.RepoId} ({e.BranchRef}) at {e.PushedAt.ToLocalTime():HH:mm:ss}.");

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
        bool running = _session.Server is { IsRunning: true };
        string hostDisplay = _session.GetActiveServerHostDisplay();

        LblLanInfo.Text = hostDisplay;
        LblServerToggleText.Text = running ? "Stop Git Server" : "Start Git Server";
        LblServerStatus.Text = running
            ? $"Running ({hostDisplay})"
            : "Git Server: Stopped";
        ServerDot.Fill = running
            ? (Brush)FindResource("AccentSuccessBrush")
            : (Brush)FindResource("AccentDangerBrush");
    }

    private async void BtnSeedDemo_Click(object sender, RoutedEventArgs e)
    {
        BtnSeedDemo.IsEnabled = false;
        SetStatus("Seeding and indexing classroom demo repositories...");
        try
        {
            SeederResult res = await _session.Seeder.SeedAllAsync(_session.ReposDirectory);
            string summary = $"Seeded {res.ReposSeeded} repos, indexed {res.CommitsIndexed} commits.";
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
        _session.ServerPushCompleted -= Server_PushCompleted;
        _session.ServerStateChanged -= Session_ServerStateChanged;
        try
        {
            _session.StopServerAsync().GetAwaiter().GetResult();
        }
        catch
        {
        }
    }
}
