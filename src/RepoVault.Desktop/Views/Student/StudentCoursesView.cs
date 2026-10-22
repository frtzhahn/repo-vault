using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

internal sealed class StudentCourseRow
{
    public StudentCourseRow(Course course, int repoCount)
    {
        Course = course;
        RepoCount = repoCount;
    }

    public Course Course { get; }
    public int RepoCount { get; }
    public string Code => Course.CourseCode;
    public string Title => Course.CourseTitle;
    public string Semester => Course.Semester;
    public string Department => Course.Department;
}

public sealed class StudentCoursesView : StudentPageBase
{
    private readonly DataGridView _courseGrid;
    private readonly DataGridView _repoGrid;
    private readonly Label _repoTitle;
    private readonly Button _openButton;
    private readonly Dictionary<int, List<RepositoryRecord>> _reposByCourse = new();

    public StudentCoursesView(AppSession session) : base(session)
    {
        _courseGrid = UiTheme.Grid();
        _courseGrid.Columns.Add(UiTheme.Column("Code", nameof(StudentCourseRow.Code), 70));
        _courseGrid.Columns.Add(UiTheme.Column("Title", nameof(StudentCourseRow.Title), 180));
        _courseGrid.Columns.Add(UiTheme.Column("Semester", nameof(StudentCourseRow.Semester), 120));
        _courseGrid.Columns.Add(UiTheme.Column("Repositories", nameof(StudentCourseRow.RepoCount), 90));

        _repoGrid = UiTheme.Grid();
        _repoGrid.Columns.Add(UiTheme.Column("Repository", nameof(StudentRepoRow.Repository), 120));
        _repoGrid.Columns.Add(UiTheme.Column("Language", nameof(StudentRepoRow.Language), 90));
        _repoGrid.Columns.Add(UiTheme.Column("LOC", nameof(StudentRepoRow.SourceLines), 70, "N0"));
        _repoGrid.Columns.Add(UiTheme.Column("Deadline", nameof(StudentRepoRow.Deadline), 100));
        _repoGrid.Columns.Add(UiTheme.Column("Status", nameof(StudentRepoRow.Status), 80));

        var refresh = UiTheme.Button("Refresh", 84);
        refresh.Click += async (_, _) => await SafeLoadAsync();
        var header = Header(
            "Browse Courses",
            "Every course in the catalog and its active repositories.",
            refresh);

        _repoTitle = UiTheme.Label("Select a course", UiTheme.Section);
        _openButton = UiTheme.Button("Open Repository", 132, true);
        _openButton.Enabled = false;
        _openButton.Click += (_, _) => OpenSelected();

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
        var courseLabel = UiTheme.Label("Courses", UiTheme.SemiBold);
        courseLabel.Margin = new Padding(0, 0, 0, 8);
        left.Controls.Add(courseLabel, 0, 0);
        left.Controls.Add(_courseGrid, 0, 1);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
            Padding = new Padding(18, 0, 0, 0),
        };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _repoTitle.Margin = new Padding(0, 0, 0, 8);
        _openButton.Anchor = AnchorStyles.Right;
        _openButton.Margin = new Padding(0, 10, 0, 0);
        right.Controls.Add(_repoTitle, 0, 0);
        right.Controls.Add(_repoGrid, 0, 1);
        right.Controls.Add(_openButton, 0, 2);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 6,
            FixedPanel = FixedPanel.Panel1,
            Size = new Size(900, 500),
            Panel1MinSize = 220,
            Panel2MinSize = 280,
            BackColor = UiTheme.Border,
            Margin = new Padding(0),
        };
        split.Panel1.BackColor = UiTheme.Background;
        split.Panel2.BackColor = UiTheme.Background;
        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(right);
        split.SplitterDistance = 300;

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

        _courseGrid.SelectionChanged += (_, _) => ShowSelectedCourse();
        _repoGrid.SelectionChanged += (_, _) =>
            _openButton.Enabled = _repoGrid.CurrentRow?.DataBoundItem is StudentRepoRow;
        _repoGrid.CellDoubleClick += (_, e) =>
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
        _repoTitle.Text = "Loading courses...";
        _openButton.Enabled = false;

        try
        {
            List<Course> courses = await Session.Courses.ListCoursesAsync();
            _reposByCourse.Clear();

            foreach (Course course in courses)
            {
                _reposByCourse[course.CourseId] =
                    await Session.Repos.ListActiveByCourseAsync(course.CourseId);
            }

            List<StudentCourseRow> rows = courses
                .OrderBy(c => c.Semester)
                .ThenBy(c => c.CourseCode)
                .Select(c => new StudentCourseRow(
                    c, _reposByCourse.TryGetValue(c.CourseId, out var repos) ? repos.Count : 0))
                .ToList();

            _courseGrid.DataSource = null;
            _courseGrid.DataSource = rows;

            if (rows.Count > 0)
            {
                _courseGrid.ClearSelection();
                _courseGrid.Rows[0].Selected = true;
                _courseGrid.CurrentCell = _courseGrid.Rows[0].Cells[0];
                ShowSelectedCourse();
            }
            else
            {
                _repoTitle.Text = "No courses found";
                _repoGrid.DataSource = null;
            }
        }
        catch (Exception ex)
        {
            _courseGrid.DataSource = null;
            _repoGrid.DataSource = null;
            _repoTitle.Text = "Could not load courses: " + FriendlyError(ex);
            _openButton.Enabled = false;
        }
    }

    private void ShowSelectedCourse()
    {
        var row = _courseGrid.CurrentRow?.DataBoundItem as StudentCourseRow;
        if (row is null)
        {
            _repoTitle.Text = "Select a course";
            _repoGrid.DataSource = null;
            _openButton.Enabled = false;
            return;
        }

        List<RepositoryRecord> repos = _reposByCourse.TryGetValue(row.Course.CourseId, out var found)
            ? found
            : new List<RepositoryRecord>();

        var repoRows = repos
            .OrderBy(r => r.DeadlineUtc ?? DateTime.MaxValue)
            .ThenBy(r => r.RepoSlug)
            .Select(r => new StudentRepoRow(r, row.Code))
            .ToList();

        _repoTitle.Text = $"Repositories in {row.Code} · {row.Title}";
        _repoGrid.DataSource = null;
        _repoGrid.DataSource = repoRows;

        if (repoRows.Count > 0)
        {
            _repoGrid.ClearSelection();
            _repoGrid.Rows[0].Selected = true;
            _repoGrid.CurrentCell = _repoGrid.Rows[0].Cells[0];
        }

        _openButton.Enabled = repoRows.Count > 0;
    }

    private void OpenSelected()
    {
        if (_repoGrid.CurrentRow?.DataBoundItem is StudentRepoRow row)
        {
            OpenRepositoryRequested?.Invoke(row.Repo);
        }
    }
}
