using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

internal sealed class EvaluationRow
{
    public EvaluationRow(RepositoryRecord repo, string courseCode, EvaluationRecord? evaluation)
    {
        Repo = repo;
        CourseCode = courseCode;
        Evaluation = evaluation;
    }

    public RepositoryRecord Repo { get; }
    public string CourseCode { get; }
    public EvaluationRecord? Evaluation { get; }

    public string Repository => Repo.RepoSlug;
    public string Course => CourseCode;
    public string Score => Evaluation is null ? "Not graded" : Evaluation.TotalScore.ToString();
    public string Evaluated => Evaluation is null ? "-" : UiTheme.FormatUtcShort(Evaluation.EvaluatedAt);
}

public sealed class StudentEvaluationsView : StudentPageBase
{
    private readonly DataGridView _grid;
    private readonly Label _title;
    private readonly Label _meta;
    private readonly Label _total;
    private readonly Label _scores;
    private readonly TextBox _notes;
    private readonly Button _openButton;

    public StudentEvaluationsView(AppSession session) : base(session)
    {
        _grid = UiTheme.Grid();
        _grid.Columns.Add(UiTheme.Column("Repository", nameof(EvaluationRow.Repository), 120));
        _grid.Columns.Add(UiTheme.Column("Course", nameof(EvaluationRow.Course), 70));
        _grid.Columns.Add(UiTheme.Column("Score", nameof(EvaluationRow.Score), 80));
        _grid.Columns.Add(UiTheme.Column("Evaluated", nameof(EvaluationRow.Evaluated), 100));

        var refresh = UiTheme.Button("Refresh", 84);
        refresh.Click += async (_, _) => await SafeLoadAsync();
        var header = Header(
            "Evaluations",
            "Faculty rubric results for your repositories.",
            refresh);

        _title = UiTheme.Label("Select a repository", UiTheme.Section);
        _meta = UiTheme.Label("Choose a row to read the rubric and critique notes.", UiTheme.Small, UiTheme.Muted);
        _total = UiTheme.Label("-", new Font(UiTheme.Heading.FontFamily, 18f, FontStyle.Bold));
        _scores = UiTheme.Label("", UiTheme.Body);
        _notes = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = UiTheme.Surface,
            Font = UiTheme.Body,
            ScrollBars = ScrollBars.Vertical,
            Margin = new Padding(0, 4, 0, 0),
        };
        _openButton = UiTheme.Button("Open Repository", 132);
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
        var listLabel = UiTheme.Label("Your Repositories", UiTheme.SemiBold);
        listLabel.Margin = new Padding(0, 0, 0, 8);
        left.Controls.Add(listLabel, 0, 0);
        left.Controls.Add(_grid, 0, 1);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
            Padding = new Padding(18, 0, 0, 0),
        };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var titleRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = new Padding(0, 0, 0, 10),
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
        titleStack.Controls.Add(_title, 0, 0);
        titleStack.Controls.Add(_meta, 0, 1);
        titleRow.Controls.Add(titleStack, 0, 0);
        titleRow.Controls.Add(_openButton, 1, 0);

        var totalCard = DetailCard("Total Score", _total);
        totalCard.Height = 75;
        totalCard.Margin = new Padding(0, 0, 0, 10);
        var rubricCard = DetailCard("Rubric", _scores);
        rubricCard.Height = 130;
        rubricCard.Margin = new Padding(0, 0, 0, 10);
        var notesCard = DetailCard("Critique Notes", _notes);

        right.Controls.Add(titleRow, 0, 0);
        right.Controls.Add(totalCard, 0, 1);
        right.Controls.Add(rubricCard, 0, 2);
        right.Controls.Add(notesCard, 0, 3);

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
        split.SplitterDistance = 280;

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

        _grid.SelectionChanged += (_, _) => ShowSelected();
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                OpenSelected();
            }
        };
    }

    public event Action<RepositoryRecord, RepositoryDetailTab>? OpenRepositoryRequested;

    public override async Task LoadAsync()
    {
        _title.Text = "Loading evaluations...";
        _meta.Text = "Reading rubric records from the database.";
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

            var rows = new List<EvaluationRow>();
            foreach (RepositoryRecord repo in repos.OrderBy(r => r.RepoSlug))
            {
                EvaluationRecord? evaluation =
                    await Session.Evaluations.GetEvaluationByRepoAsync(repo.RepoId);
                string code = courseCodes.TryGetValue(repo.CourseId, out string? found) ? found : "Course";
                rows.Add(new EvaluationRow(repo, code, evaluation));
            }

            _grid.DataSource = null;
            _grid.DataSource = rows;

            if (rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                _grid.CurrentCell = _grid.Rows[0].Cells[0];
                ShowSelected();
            }
            else
            {
                _title.Text = "No repositories yet";
                _meta.Text = "Use Tools > Seed / Re-index Workspaces to load the demo data.";
            }
        }
        catch (Exception ex)
        {
            _grid.DataSource = null;
            _title.Text = "Could not load evaluations";
            _meta.Text = FriendlyError(ex);
            _openButton.Enabled = false;
        }
    }

    private void ShowSelected()
    {
        var row = _grid.CurrentRow?.DataBoundItem as EvaluationRow;
        if (row is null)
        {
            _title.Text = "Select a repository";
            _meta.Text = "Choose a row to read the rubric and critique notes.";
            _total.Text = "-";
            _scores.Text = "";
            _notes.Clear();
            _openButton.Enabled = false;
            return;
        }

        _openButton.Enabled = true;
        _title.Text = row.Repo.RepoSlug;
        _meta.Text = $"{row.CourseCode} · {row.Repo.Title}";

        if (row.Evaluation is null)
        {
            _total.Text = "Not graded";
            _scores.Text = "No faculty evaluation has been saved for this repository yet.";
            _notes.Clear();
            return;
        }

        EvaluationRecord evaluation = row.Evaluation;
        _total.Text = $"{evaluation.TotalScore} / 100";
        _scores.Text =
            $"Architecture      {evaluation.ArchitectureScore} / 25\n" +
            $"Functionality     {evaluation.FunctionalityScore} / 25\n" +
            $"Code Quality      {evaluation.CodeQualityScore} / 25\n" +
            $"Documentation     {evaluation.DocumentationScore} / 25\n" +
            $"Evaluated         {UiTheme.FormatUtc(evaluation.EvaluatedAt)}";
        _notes.Text = string.IsNullOrWhiteSpace(evaluation.CritiqueNotes)
            ? "No critique notes."
            : evaluation.CritiqueNotes;
    }

    private static CardPanel DetailCard(string title, Control body)
    {
        var card = UiTheme.Card();
        card.Padding = new Padding(10);
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
        body.Margin = new Padding(0, 4, 0, 0);
        if (body.Dock != DockStyle.Fill)
        {
            body.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }

        layout.Controls.Add(body, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private void OpenSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is EvaluationRow row)
        {
            OpenRepositoryRequested?.Invoke(row.Repo, RepositoryDetailTab.Evaluation);
        }
    }
}
