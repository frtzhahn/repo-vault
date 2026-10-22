using System.Runtime.InteropServices;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

internal sealed class CommitRow
{
    public CommitRow(CommitLogEntry entry)
    {
        Entry = entry;
    }

    public CommitLogEntry Entry { get; }
    public string Hash => Entry.CommitHash.Length > 10 ? Entry.CommitHash[..10] : Entry.CommitHash;
    public string Author => Entry.AuthorName;
    public string Message => Entry.CommitMessage.Split('\n')[0];
    public string Date => UiTheme.FormatUtc(Entry.CommittedAt);
}

public sealed class RepositoryDetailView : StudentPageBase
{
    private readonly Label _title;
    private readonly Label _meta;
    private readonly Label _statusBadge;
    private readonly Label _lockBadge;
    private readonly TextBox _cloneUrl;
    private readonly TabControl _tabs;
    private TextBox _description = null!;
    private Label _metrics = null!;
    private Label _team = null!;
    private Label _languages = null!;
    private Label _overviewEvaluation = null!;
    private TreeView _fileTree = null!;
    private RichTextBox _filePreview = null!;
    private Label _fileStatus = null!;
    private DataGridView _commitGrid = null!;
    private Label _commitStatus = null!;
    private DiscussionBoardView _discussion = null!;
    private Label _evaluationSummary = null!;
    private Label _evaluationDetails = null!;
    private TextBox _evaluationNotes = null!;
    private RepositoryRecord _currentRepo;

    public RepositoryDetailView(
        AppSession session, RepositoryRecord repo, RepositoryDetailTab initialTab) : base(session)
    {
        _currentRepo = repo;
        _title = UiTheme.Label(repo.RepoSlug, new Font(UiTheme.Heading.FontFamily, 18f, FontStyle.Bold));
        _meta = UiTheme.Label("", UiTheme.Small, UiTheme.Muted);
        _statusBadge = UiTheme.BadgeLabel("Active");
        _lockBadge = UiTheme.BadgeLabel("Unlocked");
        _cloneUrl = new TextBox
        {
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Font = UiTheme.Body,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 4, 8, 0),
        };

        var back = UiTheme.Button("< Back to My Repositories", 180);
        back.Click += (_, _) => BackRequested?.Invoke();
        var copy = UiTheme.Button("Copy", 64);
        copy.Click += (_, _) => CopyCloneUrl();

        _tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.Body,
            Padding = new Point(14, 6),
        };

        BuildOverviewTab();
        BuildFilesTab();
        BuildCommitsTab();
        BuildDiscussionTab();
        BuildEvaluationTab();
        _tabs.SelectedIndex = (int)initialTab;

        var titleRow = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            BackColor = UiTheme.Background,
            Margin = new Padding(0, 8, 0, 4),
        };
        titleRow.Controls.Add(_title);
        titleRow.Controls.Add(_statusBadge);
        titleRow.Controls.Add(_lockBadge);

        var cloneRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        cloneRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cloneRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        cloneRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cloneRow.Controls.Add(UiTheme.Label("Clone URL", UiTheme.Small, UiTheme.Muted), 0, 0);
        cloneRow.Controls.Add(_cloneUrl, 1, 0);
        cloneRow.Controls.Add(copy, 2, 0);

        var head = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.Background,
            Margin = new Padding(0, 0, 0, 12),
        };
        head.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        head.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        head.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        head.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        back.Margin = new Padding(0, 0, 0, 4);
        head.Controls.Add(back, 0, 0);
        head.Controls.Add(titleRow, 0, 1);
        head.Controls.Add(_meta, 0, 2);
        head.Controls.Add(cloneRow, 0, 3);

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
        root.Controls.Add(head, 0, 0);
        root.Controls.Add(_tabs, 0, 1);
        Controls.Add(root);

        _fileTree.AfterSelect += async (_, e) =>
        {
            if (e.Node?.Tag is RepoFileEntry entry)
            {
                await PreviewFileAsync(entry);
            }
        };
    }

    public event Action? BackRequested;

    public override async Task LoadAsync()
    {
        await RefreshRepositoryAsync();
        await LoadOverviewAsync();
        await LoadFilesAsync();
        await LoadCommitsAsync();
        await LoadEvaluationAsync();
        await _discussion.LoadRepositoryAsync(_currentRepo.RepoId);
    }

    private void BuildOverviewTab()
    {
        var page = new TabPage("Overview")
        {
            BackColor = UiTheme.Background,
            Padding = new Padding(12),
        };

        _description = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = UiTheme.Surface,
            Font = UiTheme.Body,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Height = 70,
        };
        _metrics = UiTheme.Label("", UiTheme.Body);
        _team = UiTheme.Label("", UiTheme.Body);
        _languages = UiTheme.Label("", UiTheme.Body);
        _overviewEvaluation = UiTheme.Label("", UiTheme.Body);

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
            Padding = new Padding(0, 0, 8, 0),
        };
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        var descriptionCard = DetailCard("Description", _description);
        descriptionCard.Height = 80;
        descriptionCard.Margin = new Padding(0, 0, 0, 10);
        var metricsCard = DetailCard("Code Metrics", _metrics);
        metricsCard.Height = 125;
        metricsCard.Margin = new Padding(0, 0, 0, 10);
        left.Controls.Add(descriptionCard, 0, 0);
        left.Controls.Add(metricsCard, 0, 1);
        left.Controls.Add(DetailCard("Languages", _languages), 0, 2);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
            Padding = new Padding(8, 0, 0, 0),
        };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        var teamCard = DetailCard("Team", _team);
        teamCard.Height = 125;
        teamCard.Margin = new Padding(0, 0, 0, 10);
        right.Controls.Add(teamCard, 0, 0);
        right.Controls.Add(DetailCard("Evaluation", _overviewEvaluation), 0, 1);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(right, 1, 0);
        page.Controls.Add(layout);
        _tabs.TabPages.Add(page);
    }

    private void BuildFilesTab()
    {
        var page = new TabPage("Files")
        {
            BackColor = UiTheme.Background,
            Padding = new Padding(12),
        };

        _fileStatus = UiTheme.Label("Select a file to preview.", UiTheme.Small, UiTheme.Muted);
        _fileStatus.Margin = new Padding(0, 0, 0, 6);

        _fileTree = new TreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = UiTheme.Body,
            BackColor = UiTheme.Background,
            HideSelection = false,
            FullRowSelect = true,
            ShowLines = false,
        };
        _filePreview = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = UiTheme.Mono,
            BackColor = UiTheme.Surface,
            ReadOnly = true,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both,
        };

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 6,
            FixedPanel = FixedPanel.Panel1,
            Size = new Size(900, 500),
            Panel1MinSize = 180,
            Panel2MinSize = 260,
            BackColor = UiTheme.Border,
            Margin = new Padding(0),
        };
        split.Panel1.BackColor = UiTheme.Background;
        split.Panel2.BackColor = UiTheme.Surface;
        split.Panel1.Controls.Add(_fileTree);
        split.Panel2.Controls.Add(_filePreview);
        split.SplitterDistance = 230;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(_fileStatus, 0, 0);
        layout.Controls.Add(split, 0, 1);
        page.Controls.Add(layout);
        _tabs.TabPages.Add(page);
    }

    private void BuildCommitsTab()
    {
        var page = new TabPage("Commits")
        {
            BackColor = UiTheme.Background,
            Padding = new Padding(12),
        };

        _commitStatus = UiTheme.Label("Loading commits...", UiTheme.Small, UiTheme.Muted);
        _commitStatus.Margin = new Padding(0, 0, 0, 6);

        _commitGrid = UiTheme.Grid();
        _commitGrid.Columns.Add(UiTheme.Column("Commit", nameof(CommitRow.Hash), 80));
        _commitGrid.Columns.Add(UiTheme.Column("Author", nameof(CommitRow.Author), 130));
        _commitGrid.Columns.Add(UiTheme.Column("Message", nameof(CommitRow.Message), 260));
        _commitGrid.Columns.Add(UiTheme.Column("Date", nameof(CommitRow.Date), 120));

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(_commitStatus, 0, 0);
        layout.Controls.Add(_commitGrid, 0, 1);
        page.Controls.Add(layout);
        _tabs.TabPages.Add(page);
    }

    private void BuildDiscussionTab()
    {
        var page = new TabPage("Discussion")
        {
            BackColor = UiTheme.Background,
            Padding = new Padding(12),
        };
        _discussion = new DiscussionBoardView(Session);
        page.Controls.Add(_discussion);
        _tabs.TabPages.Add(page);
    }

    private void BuildEvaluationTab()
    {
        var page = new TabPage("Evaluation")
        {
            BackColor = UiTheme.Background,
            Padding = new Padding(12),
        };

        _evaluationSummary = UiTheme.Label("Not graded yet.", new Font(UiTheme.Heading.FontFamily, 16f, FontStyle.Bold));
        _evaluationDetails = UiTheme.Label("", UiTheme.Body);
        _evaluationNotes = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = UiTheme.Surface,
            Font = UiTheme.Body,
            ScrollBars = ScrollBars.Vertical,
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(_evaluationSummary, 0, 0);
        _evaluationDetails.Margin = new Padding(0, 8, 0, 10);
        layout.Controls.Add(_evaluationDetails, 0, 1);
        layout.Controls.Add(DetailCard("Critique Notes", _evaluationNotes), 0, 2);
        page.Controls.Add(layout);
        _tabs.TabPages.Add(page);
    }

    private async Task RefreshRepositoryAsync()
    {
        try
        {
            RepositoryRecord? fresh = await Session.Repos.GetByIdAsync(_currentRepo.RepoId);
            if (fresh is not null)
            {
                _currentRepo = fresh;
            }
        }
        catch
        {
        }

        _title.Text = _currentRepo.RepoSlug;
        _meta.Text =
            $"{_currentRepo.Title} · Deadline {UiTheme.FormatUtc(_currentRepo.DeadlineUtc)} · Created {UiTheme.FormatUtcShort(_currentRepo.CreatedAt)}";
        _statusBadge.Text = _currentRepo.LifecycleStatus == LifecycleStatus.Active ? "Active" : "Trashed";
        _lockBadge.Text = _currentRepo.IsLocked ? "Locked" : "Unlocked";
        _cloneUrl.Text = LanHelper.BuildCloneUrl(_currentRepo.RepoSlug, Session.ServerPort);
        _description.Text = string.IsNullOrWhiteSpace(_currentRepo.Description)
            ? "No description."
            : _currentRepo.Description;
    }

    private async Task LoadOverviewAsync()
    {
        try
        {
            List<ContributorViewDto> team =
                await Session.Contributors.ListContributorViewsByRepoAsync(_currentRepo.RepoId);
            List<RepositoryLanguage> languages =
                await Session.Repos.ListLanguagesAsync(_currentRepo.RepoId);
            EvaluationRecord? evaluation =
                await Session.Evaluations.GetEvaluationByRepoAsync(_currentRepo.RepoId);

            _metrics.Text =
                $"Primary language    {_currentRepo.PrimaryLanguage ?? "-"}\n" +
                $"Source lines        {_currentRepo.SourceLines:N0}\n" +
                $"Comment lines       {_currentRepo.CommentLines:N0}\n" +
                $"Blank lines         {_currentRepo.BlankLines:N0}\n" +
                $"Files               {_currentRepo.TotalFiles:N0}";

            _team.Text = team.Count == 0
                ? "No contributors assigned."
                : string.Join("\n", team.Select(c => $"{c.FullName} — {c.TeamRole}"));

            _languages.Text = languages.Count == 0
                ? "No language metrics yet."
                : string.Join("\n", languages.Select(l =>
                    $"{l.LanguageName} · {l.SourceLines:N0} source · {l.PercentageShare:0.##}%"));

            _overviewEvaluation.Text = evaluation is null
                ? "Not graded yet."
                : $"Total {evaluation.TotalScore} / 100 · evaluated {UiTheme.FormatUtc(evaluation.EvaluatedAt)}";
        }
        catch (Exception ex)
        {
            _metrics.Text = FriendlyError(ex);
            _team.Text = "";
            _languages.Text = "";
            _overviewEvaluation.Text = "";
        }
    }

    private async Task LoadFilesAsync()
    {
        _fileTree.Nodes.Clear();
        _filePreview.Clear();

        if (!Directory.Exists(_currentRepo.BareRepoDiskPath))
        {
            _fileStatus.Text = "Bare repository not found. Run Tools > Seed / Re-index Workspaces.";
            return;
        }

        try
        {
            List<RepoFileEntry> files =
                await Session.Browser.ListFilesAtHeadAsync(_currentRepo.BareRepoDiskPath);
            if (files.Count == 0)
            {
                _fileStatus.Text = "No files at HEAD.";
                return;
            }

            var directories = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
            foreach (RepoFileEntry file in files)
            {
                TreeNodeCollection parent = _fileTree.Nodes;

                if (file.DirectoryPath.Length > 0)
                {
                    string current = "";
                    foreach (string part in file.DirectoryPath.Split('/'))
                    {
                        current = current.Length == 0 ? part : current + "/" + part;
                        if (!directories.TryGetValue(current, out TreeNode? directory))
                        {
                            directory = new TreeNode(part);
                            parent.Add(directory);
                            directories[current] = directory;
                        }

                        parent = directory.Nodes;
                    }
                }

                parent.Add(new TreeNode(file.FileName) { Tag = file });
            }

            _fileTree.ExpandAll();
            _fileStatus.Text = $"{files.Count} files at HEAD. Select a file to preview.";

            TreeNode? firstFile = FindFirstFile(_fileTree.Nodes);
            if (firstFile is not null)
            {
                _fileTree.SelectedNode = firstFile;
            }
        }
        catch (Exception ex)
        {
            _fileStatus.Text = "Could not read repository files: " + FriendlyError(ex);
        }
    }

    private static TreeNode? FindFirstFile(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is RepoFileEntry)
            {
                return node;
            }

            TreeNode? nested = FindFirstFile(node.Nodes);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private async Task PreviewFileAsync(RepoFileEntry entry)
    {
        _fileStatus.Text = $"Loading {entry.Path}...";
        try
        {
            RepoBlobContent blob = await Session.Browser.ReadFileBlobAsync(
                _currentRepo.BareRepoDiskPath, entry.Path);
            if (IsDisposed)
            {
                return;
            }

            _filePreview.Text = blob.TextContent;
            _filePreview.SelectionStart = 0;
            _fileStatus.Text =
                $"{entry.Path} · {entry.SizeBytes:N0} bytes" +
                (blob.IsBinary ? " · binary preview" : "") +
                (blob.IsTruncated ? " · truncated" : "");
        }
        catch (Exception ex)
        {
            if (IsDisposed)
            {
                return;
            }

            _filePreview.Clear();
            _fileStatus.Text = "Could not read file: " + FriendlyError(ex);
        }
    }

    private async Task LoadCommitsAsync()
    {
        try
        {
            List<CommitLogEntry> commits =
                await Session.Repos.ListCommitLogsAsync(_currentRepo.RepoId);
            _commitGrid.DataSource = null;
            _commitGrid.DataSource = commits.Select(c => new CommitRow(c)).ToList();
            _commitStatus.Text = commits.Count == 0
                ? "No commits indexed yet. Run Tools > Seed / Re-index Workspaces."
                : $"{commits.Count} commits indexed.";
        }
        catch (Exception ex)
        {
            _commitGrid.DataSource = null;
            _commitStatus.Text = "Could not load commits: " + FriendlyError(ex);
        }
    }

    private async Task LoadEvaluationAsync()
    {
        try
        {
            EvaluationRecord? evaluation =
                await Session.Evaluations.GetEvaluationByRepoAsync(_currentRepo.RepoId);
            if (evaluation is null)
            {
                _evaluationSummary.Text = "Not graded yet.";
                _evaluationDetails.Text = "No faculty evaluation has been saved for this repository.";
                _evaluationNotes.Text = "";
                return;
            }

            _evaluationSummary.Text = $"{evaluation.TotalScore} / 100";
            _evaluationDetails.Text =
                $"Architecture      {evaluation.ArchitectureScore} / 25\n" +
                $"Functionality     {evaluation.FunctionalityScore} / 25\n" +
                $"Code Quality      {evaluation.CodeQualityScore} / 25\n" +
                $"Documentation     {evaluation.DocumentationScore} / 25\n" +
                $"Evaluated         {UiTheme.FormatUtc(evaluation.EvaluatedAt)}";
            _evaluationNotes.Text = string.IsNullOrWhiteSpace(evaluation.CritiqueNotes)
                ? "No critique notes."
                : evaluation.CritiqueNotes;
        }
        catch (Exception ex)
        {
            _evaluationSummary.Text = "Could not load evaluation";
            _evaluationDetails.Text = FriendlyError(ex);
            _evaluationNotes.Text = "";
        }
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
        layout.Controls.Add(body, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private void CopyCloneUrl()
    {
        try
        {
            Clipboard.SetText(_cloneUrl.Text);
            _meta.Text = "clone url copied to clipboard.";
        }
        catch (ExternalException)
        {
            _meta.Text = "could not access the clipboard.";
        }
    }
}
