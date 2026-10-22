using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

internal sealed class DiscussionRepoItem
{
    public DiscussionRepoItem(RepositoryRecord repo, string courseCode)
    {
        Repo = repo;
        CourseCode = courseCode;
    }

    public RepositoryRecord Repo { get; }
    public string CourseCode { get; }
    public override string ToString() => $"{Repo.RepoSlug}  ·  {CourseCode}";
}

public sealed class StudentDiscussionsView : StudentPageBase
{
    private readonly ListBox _repoList;
    private readonly Label _repoTitle;
    private readonly Button _openButton;
    private readonly DiscussionBoardView _board;
    private bool _loading;

    public StudentDiscussionsView(AppSession session) : base(session)
    {
        _repoList = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = UiTheme.Body,
            BackColor = UiTheme.Background,
            ForeColor = UiTheme.Text,
            IntegralHeight = false,
        };
        _repoList.SelectedIndexChanged += async (_, _) => await ShowSelectedAsync();

        _repoTitle = UiTheme.Label("Select a repository", UiTheme.Section);
        _repoTitle.Margin = new Padding(0, 0, 0, 8);
        _openButton = UiTheme.Button("Open Repository", 132);
        _openButton.Enabled = false;
        _openButton.Click += (_, _) => OpenSelected();
        _board = new DiscussionBoardView(session);

        var refresh = UiTheme.Button("Refresh", 84);
        refresh.Click += async (_, _) => await SafeLoadAsync();
        var header = Header(
            "Discussions",
            "Peer and faculty feedback on your repositories.",
            refresh);

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        var listLabel = UiTheme.Label("Your Repositories", UiTheme.SemiBold);
        listLabel.Margin = new Padding(0, 0, 0, 8);
        left.Controls.Add(listLabel, 0, 0);
        left.Controls.Add(_repoList, 0, 1);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
            Padding = new Padding(18, 0, 0, 0),
        };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        var rightHeader = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        rightHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rightHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        rightHeader.Controls.Add(_repoTitle, 0, 0);
        rightHeader.Controls.Add(_openButton, 1, 0);
        right.Controls.Add(rightHeader, 0, 0);
        right.Controls.Add(_board, 0, 1);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 6,
            FixedPanel = FixedPanel.Panel1,
            Size = new Size(900, 500),
            Panel1MinSize = 200,
            Panel2MinSize = 300,
            BackColor = UiTheme.Border,
            Margin = new Padding(0),
        };
        split.Panel1.BackColor = UiTheme.Background;
        split.Panel2.BackColor = UiTheme.Background;
        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(right);
        split.SplitterDistance = 250;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(split, 0, 1);
        Controls.Add(root);
    }

    public event Action<RepositoryRecord, RepositoryDetailTab>? OpenRepositoryRequested;

    public override async Task LoadAsync()
    {
        _loading = true;
        _repoList.Items.Clear();
        _repoTitle.Text = "Loading repositories...";
        _openButton.Enabled = false;

        try
        {
            int userId = Session.CurrentUser?.UserId
                ?? throw new InvalidOperationException("not signed in.");

            List<RepositoryRecord> repos = await Session.Repos.ListByStudentContributorAsync(userId);
            List<Course> courses = await Session.Courses.ListCoursesAsync();
            var courseCodes = new Dictionary<int, string>();
            foreach (Course course in courses)
            {
                courseCodes[course.CourseId] = course.CourseCode;
            }

            foreach (RepositoryRecord repo in repos.OrderBy(r => r.RepoSlug))
            {
                string code = courseCodes.TryGetValue(repo.CourseId, out string? found) ? found : "Course";
                _repoList.Items.Add(new DiscussionRepoItem(repo, code));
            }

            if (_repoList.Items.Count > 0)
            {
                _repoList.SelectedIndex = 0;
            }
            else
            {
                _repoTitle.Text = "No repositories yet";
            }
        }
        catch (Exception ex)
        {
            _repoTitle.Text = "Could not load discussions: " + FriendlyError(ex);
        }
        finally
        {
            _loading = false;
        }

        if (IsDisposed)
        {
            return;
        }

        if (_repoList.SelectedItem is DiscussionRepoItem)
        {
            await ShowSelectedAsync();
        }
    }

    private async Task ShowSelectedAsync()
    {
        if (_loading)
        {
            return;
        }

        if (_repoList.SelectedItem is not DiscussionRepoItem item)
        {
            _repoTitle.Text = "Select a repository";
            _openButton.Enabled = false;
            return;
        }

        _repoTitle.Text = $"Discussion · {item.Repo.RepoSlug}";
        _openButton.Enabled = true;
        await _board.LoadRepositoryAsync(item.Repo.RepoId);
    }

    private void OpenSelected()
    {
        if (_repoList.SelectedItem is DiscussionRepoItem item)
        {
            OpenRepositoryRequested?.Invoke(item.Repo, RepositoryDetailTab.Discussion);
        }
    }
}
