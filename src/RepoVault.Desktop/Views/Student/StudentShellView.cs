using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

public sealed class StudentShellView : UserControl
{
    private readonly AppSession _session;
    private readonly Panel _content;
    private readonly Dictionary<StudentRoute, Button> _navButtons = new();
    private StudentPageBase? _activeView;
    private bool _started;
    private int _navigationVersion;

    public StudentShellView(AppSession session)
    {
        _session = session;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        Font = UiTheme.Body;
        DoubleBuffered = true;

        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 210,
            BackColor = UiTheme.Sidebar,
            Padding = new Padding(16, 18, 16, 16),
        };
        sidebar.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawLine(pen, sidebar.Width - 1, 0, sidebar.Width - 1, sidebar.Height);
        };

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = UiTheme.Sidebar,
            Margin = new Padding(0),
        };
        var section = UiTheme.Label("STUDENT", UiTheme.Small, UiTheme.Muted);
        section.Font = new Font(UiTheme.Small, FontStyle.Bold);
        section.Margin = new Padding(8, 0, 0, 12);
        nav.Controls.Add(section);

        AddNav(nav, StudentRoute.Repositories, "My Repositories");
        AddNav(nav, StudentRoute.Courses, "Browse Courses");
        AddNav(nav, StudentRoute.Evaluations, "Evaluations");
        AddNav(nav, StudentRoute.Discussions, "Discussions");
        sidebar.Controls.Add(nav);

        _content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            Padding = new Padding(28, 24, 28, 24),
        };

        Controls.Add(_content);
        Controls.Add(sidebar);

        Load += async (_, _) =>
        {
            if (_started)
            {
                return;
            }

            _started = true;
            await NavigateAsync(StudentRoute.Repositories);
        };
    }

    private void AddNav(FlowLayoutPanel nav, StudentRoute route, string text)
    {
        Button button = UiTheme.NavButton(text);
        button.Click += async (_, _) => await NavigateAsync(route);
        _navButtons[route] = button;
        nav.Controls.Add(button);
    }

    private async Task NavigateAsync(
        StudentRoute route, RepositoryRecord? repo = null,
        RepositoryDetailTab tab = RepositoryDetailTab.Overview)
    {
        if (route == StudentRoute.RepositoryDetail && repo is null)
        {
            return;
        }

        int version = ++_navigationVersion;
        await Task.Yield();
        if (version != _navigationVersion || IsDisposed)
        {
            return;
        }

        SetActive(route == StudentRoute.RepositoryDetail ? StudentRoute.Repositories : route);

        StudentPageBase? previous = _activeView;
        if (previous is not null)
        {
            _content.Controls.Remove(previous);
            previous.Dispose();
        }

        StudentPageBase view = route switch
        {
            StudentRoute.Repositories => new StudentRepositoriesView(_session),
            StudentRoute.Courses => new StudentCoursesView(_session),
            StudentRoute.Evaluations => new StudentEvaluationsView(_session),
            StudentRoute.Discussions => new StudentDiscussionsView(_session),
            _ => new RepositoryDetailView(_session, repo!, tab),
        };

        switch (view)
        {
            case StudentRepositoriesView repositories:
                repositories.OpenRepositoryRequested += OpenRepository;
                break;
            case StudentCoursesView courses:
                courses.OpenRepositoryRequested += OpenRepository;
                break;
            case StudentEvaluationsView evaluations:
                evaluations.OpenRepositoryRequested += OpenRepositoryTab;
                break;
            case StudentDiscussionsView discussions:
                discussions.OpenRepositoryRequested += OpenRepositoryTab;
                break;
            case RepositoryDetailView detail:
                detail.BackRequested += BackToRepositories;
                break;
        }

        _activeView = view;
        _content.Controls.Add(view);
        view.BringToFront();

        try
        {
            await view.LoadAsync();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load this screen: " + ex.Message,
                "Repo Vault", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void OpenRepository(RepositoryRecord repo)
    {
        _ = NavigateAsync(StudentRoute.RepositoryDetail, repo);
    }

    private void OpenRepositoryTab(RepositoryRecord repo, RepositoryDetailTab tab)
    {
        _ = NavigateAsync(StudentRoute.RepositoryDetail, repo, tab);
    }

    private void BackToRepositories()
    {
        _ = NavigateAsync(StudentRoute.Repositories);
    }

    public async Task RefreshActiveAsync()
    {
        if (_activeView is not null && !_activeView.IsDisposed)
        {
            try
            {
                await _activeView.LoadAsync();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private void SetActive(StudentRoute route)
    {
        foreach (var pair in _navButtons)
        {
            bool active = pair.Key == route;
            pair.Value.BackColor = active ? UiTheme.Selected : UiTheme.Sidebar;
            pair.Value.Font = active ? UiTheme.SemiBold : UiTheme.Body;
        }
    }
}
