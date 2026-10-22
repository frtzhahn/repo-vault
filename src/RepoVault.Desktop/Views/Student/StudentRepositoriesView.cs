using System.Runtime.InteropServices;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

public sealed class StudentRepositoriesView : StudentPageBase
{
    private readonly DataGridView _grid;
    private readonly TextBox _search;
    private readonly Label _selectedTitle;
    private readonly Label _selectedMeta;
    private readonly TextBox _cloneUrl;
    private readonly Label _metrics;
    private readonly Label _team;
    private readonly Button _openButton;
    private List<StudentRepoRow> _allRows = new();
    private RepositoryRecord? _selectedRepo;
    private int _detailVersion;

    public StudentRepositoriesView(AppSession session) : base(session)
    {
        _grid = UiTheme.Grid();
        _grid.Columns.Add(UiTheme.Column("Repository", nameof(StudentRepoRow.Repository), 110));
        _grid.Columns.Add(UiTheme.Column("Course", nameof(StudentRepoRow.Course), 70));
        _grid.Columns.Add(UiTheme.Column("Language", nameof(StudentRepoRow.Language), 90));
        _grid.Columns.Add(UiTheme.Column("LOC", nameof(StudentRepoRow.SourceLines), 70, "N0"));
        _grid.Columns.Add(UiTheme.Column("Deadline", nameof(StudentRepoRow.Deadline), 100));
        _grid.Columns.Add(UiTheme.Column("Status", nameof(StudentRepoRow.Status), 80));

        _search = new TextBox
        {
            Width = 230,
            Height = 30,
            Font = UiTheme.Body,
            PlaceholderText = "Search repositories...",
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0),
        };
        _search.TextChanged += (_, _) => ApplyFilter();
        var refresh = UiTheme.Button("Refresh", 84);
        refresh.Click += async (_, _) => await SafeLoadAsync();

        var header = Header(
            "My Repositories",
            "Repositories where you are listed as a contributor.",
            refresh, _search);

        _selectedTitle = UiTheme.Label("Select a repository", UiTheme.Section);
        _selectedMeta = UiTheme.Label(
            "Choose a row to see the clone URL, code metrics, and team.",
            UiTheme.Small, UiTheme.Muted);
        _cloneUrl = new TextBox
        {
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Font = UiTheme.Body,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 4, 8, 0),
        };
        var copy = UiTheme.Button("Copy", 64);
        copy.Click += (_, _) => CopyCloneUrl();
        _openButton = UiTheme.Button("Open Repository", 132, true);
        _openButton.Enabled = false;
        _openButton.Click += (_, _) => OpenSelected();

        _metrics = UiTheme.Label("", UiTheme.Body);
        _team = UiTheme.Label("", UiTheme.Body);

        var details = BuildDetails(copy);
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 218f));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(_grid, 0, 1);
        root.Controls.Add(details, 0, 2);
        Controls.Add(root);

        _grid.SelectionChanged += async (_, _) => await UpdateDetailsAsync();
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                OpenSelected();
            }
        };
    }

    public event Action<RepositoryRecord>? OpenRepositoryRequested;

    public override async Task LoadAsync()
    {
        _selectedTitle.Text = "Loading repositories...";
        _selectedMeta.Text = "Reading your team assignments from the database.";
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

            _allRows = repos
                .OrderBy(r => r.DeadlineUtc ?? DateTime.MaxValue)
                .ThenBy(r => r.RepoSlug)
                .Select(r => new StudentRepoRow(
                    r, courseCodes.TryGetValue(r.CourseId, out string? code) ? code : "Course"))
                .ToList();

            ApplyFilter();

            if (_allRows.Count == 0)
            {
                _selectedTitle.Text = "No repositories yet";
                _selectedMeta.Text = "Use Tools > Seed / Re-index Workspaces to load the demo data.";
            }
        }
        catch (Exception ex)
        {
            _allRows.Clear();
            ApplyFilter();
            _selectedTitle.Text = "Could not load repositories";
            _selectedMeta.Text = FriendlyError(ex);
        }
    }

    private Control BuildDetails(Button copy)
    {
        var card = UiTheme.Card();
        card.Margin = new Padding(0, 12, 0, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var titleRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0),
        };
        titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var titleStack = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
        };
        titleStack.Controls.Add(_selectedTitle, 0, 0);
        titleStack.Controls.Add(_selectedMeta, 0, 1);
        titleRow.Controls.Add(titleStack, 0, 0);
        titleRow.Controls.Add(_openButton, 1, 0);

        var cloneRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0, 10, 0, 10),
        };
        cloneRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cloneRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        cloneRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cloneRow.Controls.Add(UiTheme.Label("Clone URL", UiTheme.Small, UiTheme.Muted), 0, 0);
        cloneRow.Controls.Add(_cloneUrl, 1, 0);
        cloneRow.Controls.Add(copy, 2, 0);

        layout.Controls.Add(titleRow, 0, 0);
        layout.SetColumnSpan(titleRow, 2);
        layout.Controls.Add(cloneRow, 0, 1);
        layout.SetColumnSpan(cloneRow, 2);
        layout.Controls.Add(DetailCard("Code Metrics", _metrics), 0, 2);
        layout.Controls.Add(DetailCard("Team", _team), 1, 2);

        card.Controls.Add(layout);
        return card;
    }

    private static CardPanel DetailCard(string title, Label body)
    {
        var card = UiTheme.Card();
        card.Padding = new Padding(10);
        card.Margin = new Padding(0, 0, 8, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(UiTheme.Label(title, UiTheme.SemiBold), 0, 0);
        body.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        layout.Controls.Add(body, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private void ApplyFilter()
    {
        string term = _search.Text.Trim();
        List<StudentRepoRow> rows = term.Length == 0
            ? _allRows
            : _allRows.Where(r =>
                r.Repository.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Course.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.Language.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();

        _grid.DataSource = null;
        _grid.DataSource = rows;

        if (rows.Count > 0)
        {
            _grid.ClearSelection();
            _grid.Rows[0].Selected = true;
            _grid.CurrentCell = _grid.Rows[0].Cells[0];
        }
        else
        {
            _selectedRepo = null;
            _openButton.Enabled = false;
            if (_allRows.Count > 0)
            {
                _selectedTitle.Text = "No matches";
                _selectedMeta.Text = "Try a different repository, course, or language.";
                _cloneUrl.Clear();
                _metrics.Text = "";
                _team.Text = "";
            }
        }
    }

    private async Task UpdateDetailsAsync()
    {
        int version = ++_detailVersion;
        var row = _grid.CurrentRow?.DataBoundItem as StudentRepoRow;
        _selectedRepo = row?.Repo;

        if (row is null)
        {
            _openButton.Enabled = false;
            return;
        }

        _openButton.Enabled = true;
        _selectedTitle.Text = row.Repo.RepoSlug;
        _selectedMeta.Text =
            $"{row.CourseCode} · {row.Repo.Title} · Deadline {UiTheme.FormatUtc(row.Repo.DeadlineUtc)} · {row.Status}";
        _cloneUrl.Text = LanHelper.BuildCloneUrl(row.Repo.RepoSlug, Session.ServerPort);
        _metrics.Text =
            $"Source lines      {row.Repo.SourceLines:N0}\n" +
            $"Comment lines     {row.Repo.CommentLines:N0}\n" +
            $"Blank lines       {row.Repo.BlankLines:N0}\n" +
            $"Files             {row.Repo.TotalFiles:N0}";
        _team.Text = "loading...";

        try
        {
            List<ContributorViewDto> team =
                await Session.Contributors.ListContributorViewsByRepoAsync(row.Repo.RepoId);
            if (version != _detailVersion || IsDisposed)
            {
                return;
            }

            _team.Text = team.Count == 0
                ? "no contributors assigned"
                : string.Join("\n", team.Select(c => $"{c.FullName} — {c.TeamRole}"));
        }
        catch (Exception ex)
        {
            if (version == _detailVersion && !IsDisposed)
            {
                _team.Text = FriendlyError(ex);
            }
        }
    }

    private void OpenSelected()
    {
        if (_selectedRepo is not null)
        {
            OpenRepositoryRequested?.Invoke(_selectedRepo);
        }
    }

    private void CopyCloneUrl()
    {
        if (_cloneUrl.Text.Length == 0)
        {
            return;
        }

        try
        {
            Clipboard.SetText(_cloneUrl.Text);
            _selectedMeta.Text = "clone url copied to clipboard.";
        }
        catch (ExternalException)
        {
            _selectedMeta.Text = "could not access the clipboard.";
        }
    }
}
