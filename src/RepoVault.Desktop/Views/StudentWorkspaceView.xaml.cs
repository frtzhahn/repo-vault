using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class StudentWorkspaceView : UserControl
{
    private readonly AppSession _session;
    private readonly Action<string>? _setStatus;
    private readonly Action? _toggleSidebar;
    private readonly Action<bool>? _setSidebarCollapsed;
    private readonly Action<int>? _updateNavHighlight;

    private readonly List<Course> _courses = new();
    private readonly List<RepositoryRecord> _repos = new();
    private readonly List<RepositoryRecord> _ownedRepos = new();
    private readonly List<CodingTask> _tasks = new();

    private RepositoryRecord? _selectedRepo;
    private string? _currentEditingFilePath;
    private string _loadedFileContent = string.Empty;
    private bool _suppressEditorTextChanged;
    private bool _canPushCurrentRepo;
    private bool _isLoading;
    private int _activePageIndex;
    private int _activePanelTabIndex;

    public StudentWorkspaceView() : this(new AppSession(), null, null, null, null)
    {
    }

    public StudentWorkspaceView(
        AppSession session,
        Action<string>? setStatus = null,
        Action? toggleSidebar = null,
        Action<bool>? setSidebarCollapsed = null,
        Action<int>? updateNavHighlight = null)
    {
        _session = session;
        _setStatus = setStatus;
        _toggleSidebar = toggleSidebar;
        _setSidebarCollapsed = setSidebarCollapsed;
        _updateNavHighlight = updateNavHighlight;
        InitializeComponent();
        SwitchBottomPanelTab(0);
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        await LoadCoursesAndDataAsync();
    }

    public void NavigateToPage(int pageIndex)
    {
        _activePageIndex = Math.Clamp(pageIndex, 0, 2);

        DedicatedRepoWorkspaceGrid.Visibility = Visibility.Collapsed;
        TopLevelPagesRoot.Visibility = Visibility.Visible;
        _setSidebarCollapsed?.Invoke(false);

        PageWorkspacesBorder.Visibility = _activePageIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        PageTasksBorder.Visibility = _activePageIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        PageTicketsBorder.Visibility = _activePageIndex == 2 ? Visibility.Visible : Visibility.Collapsed;

        LblActivePageTitle.Text = _activePageIndex switch
        {
            0 => "1. My Workspaces",
            1 => "2. Assigned Tasks",
            2 => "3. Team & Tickets",
            _ => "1. My Workspaces",
        };

        _updateNavHighlight?.Invoke(_activePageIndex);
    }

    public void ToggleBottomPanel()
    {
        if (DedicatedRepoWorkspaceGrid.Visibility != Visibility.Visible)
        {
            return;
        }

        BottomPanelBorder.Visibility = BottomPanelBorder.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public async Task LoadCoursesAndDataAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            List<Course> list = await _session.Courses.ListCoursesAsync();
            _courses.Clear();
            _courses.AddRange(list);

            CboCourseFilter.Items.Clear();
            CboCourseFilter.Items.Add("All Courses");
            foreach (Course c in _courses)
            {
                CboCourseFilter.Items.Add($"{c.CourseCode} - {c.CourseTitle}");
            }

            CboCourseFilter.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ReportStatus("Could not load courses: " + ex.Message);
        }
        finally
        {
            _isLoading = false;
        }

        await RefreshDataAsync();
    }

    public async Task RefreshDataAsync()
    {
        if (_isLoading)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        _isLoading = true;
        try
        {
            int? courseFilter = null;
            if (CboCourseFilter.SelectedIndex > 0 && CboCourseFilter.SelectedIndex - 1 < _courses.Count)
            {
                courseFilter = _courses[CboCourseFilter.SelectedIndex - 1].CourseId;
            }

            int prevRepoId = _selectedRepo?.RepoId ?? 0;

            _repos.Clear();
            List<RepositoryRecord> visible = await _session.Repos.ListVisibleRepositoriesForUserAsync(actor, courseFilter);
            _repos.AddRange(visible);

            var courseLookup = _courses.ToDictionary(c => c.CourseId, c => c.CourseCode);
            var repoRows = new List<StudentRepoRow>();
            int selectIdx = -1;

            for (int i = 0; i < _repos.Count; i++)
            {
                RepositoryRecord r = _repos[i];
                string code = courseLookup.TryGetValue(r.CourseId, out string? cc) ? cc : "#" + r.CourseId;
                string status = r.IsLocked ? "LOCKED" : (r.IsSubmitted ? "SUBMITTED" : "DRAFT");
                string deadline = ComputeDeadlineBadge(r);
                bool canPush = await _session.Rbac.CanPushToRepositoryAsync(actor, r.RepoId);
                string role = r.OwnerStudentId == actor.UserId
                    ? "OWNER"
                    : (canPush ? "GROUPMATE" : "VIEWER");

                repoRows.Add(new StudentRepoRow(
                    r.RepoId, r.RepoSlug, r.Title, code, role, status, deadline, r.SourceLines.ToString("N0")));
                if (r.RepoId == prevRepoId)
                {
                    selectIdx = i;
                }
            }

            GridMyRepos.ItemsSource = repoRows;
            if (_repos.Count > 0)
            {
                GridMyRepos.SelectedIndex = selectIdx >= 0 ? selectIdx : 0;
            }

            // load assigned tasks
            _tasks.Clear();
            if (courseFilter.HasValue)
            {
                _tasks.AddRange(await _session.Courses.ListTasksByCourseAsync(courseFilter.Value));
            }
            else
            {
                foreach (Course c in _courses)
                {
                    _tasks.AddRange(await _session.Courses.ListTasksByCourseAsync(c.CourseId));
                }
            }

            var taskRows = new List<StudentTaskRow>();
            foreach (CodingTask t in _tasks)
            {
                string cCode = courseLookup.TryGetValue(t.CourseId, out string? tc) ? tc : "#" + t.CourseId;
                string typeBadge = t.AllowGroupWork ? $"GROUP (max {t.MaxGroupSize})" : "SOLO";
                string dl = t.DeadlineUtc.HasValue
                    ? t.DeadlineUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    : "Open";
                taskRows.Add(new StudentTaskRow(t.TaskId, t.Title, cCode, t.StarterTemplate, typeBadge, dl));
            }

            GridAssignedTasks.ItemsSource = taskRows;
            if (_tasks.Count > 0 && GridAssignedTasks.SelectedIndex < 0)
            {
                GridAssignedTasks.SelectedIndex = 0;
            }

            // populate owned repositories selector on Page 3 (Team & Tickets)
            _ownedRepos.Clear();
            _ownedRepos.AddRange(_repos.Where(r => r.OwnerStudentId == actor.UserId && !r.IsLocked));

            int prevTicketIdx = CboTicketRepoSelector.SelectedIndex;
            CboTicketRepoSelector.Items.Clear();
            foreach (RepositoryRecord owned in _ownedRepos)
            {
                CboTicketRepoSelector.Items.Add($"{owned.RepoSlug} - {owned.Title}");
            }

            if (_ownedRepos.Count > 0)
            {
                CboTicketRepoSelector.SelectedIndex = prevTicketIdx >= 0 && prevTicketIdx < _ownedRepos.Count
                    ? prevTicketIdx
                    : 0;
            }

            bool hasTargetOwnedRepo = _ownedRepos.Count > 0;
            BtnRequestGroupmateTicket.IsEnabled = hasTargetOwnedRepo;
            BtnGrantPeerAccess.IsEnabled = hasTargetOwnedRepo;

            await RefreshAllTicketsTableAsync();

            if (DedicatedRepoWorkspaceGrid.Visibility == Visibility.Visible && prevRepoId > 0)
            {
                RepositoryRecord? fresh = await _session.Repos.GetByIdAsync(prevRepoId);
                if (fresh is not null)
                {
                    await LoadSelectedRepoAsync(fresh, _currentEditingFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            ReportStatus("Refresh failed: " + ex.Message);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task RefreshAllTicketsTableAsync()
    {
        var allContributorRows = new List<TicketTableRow>();
        foreach (RepositoryRecord r in _repos)
        {
            List<ContributorViewDto> list = await _session.Contributors.ListContributorViewsByRepoAsync(r.RepoId);
            foreach (ContributorViewDto item in list)
            {
                allContributorRows.Add(new TicketTableRow(
                    r.RepoSlug,
                    item.FullName,
                    item.Username,
                    item.TeamRole,
                    item.AccessLevel.ToString(),
                    item.ApprovalStatus.ToString(),
                    item.AssignedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")));
            }
        }

        GridContributors.ItemsSource = allContributorRows;
    }

    private static string ComputeDeadlineBadge(RepositoryRecord r)
    {
        if (!r.DeadlineUtc.HasValue)
        {
            return "OPEN";
        }

        if (r.IsSubmitted && r.SubmittedAt.HasValue)
        {
            return r.SubmittedAt.Value <= r.DeadlineUtc.Value ? "ON TIME" : "LATE";
        }

        return DateTime.UtcNow > r.DeadlineUtc.Value ? "LATE" : "ON TIME";
    }

    public async Task OpenRepoWorkspaceAsync(RepositoryRecord repo)
    {
        TopLevelPagesRoot.Visibility = Visibility.Collapsed;
        DedicatedRepoWorkspaceGrid.Visibility = Visibility.Visible;
        _setSidebarCollapsed?.Invoke(true);
        NewFileBoxBorder.Visibility = Visibility.Collapsed;
        await LoadSelectedRepoAsync(repo, null);
    }

    private void SwitchBottomPanelTab(int tabIdx)
    {
        _activePanelTabIndex = Math.Clamp(tabIdx, 0, 3);
        GridUnifiedCommits.Visibility = _activePanelTabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        PanelViewDiscussionsGrid.Visibility = _activePanelTabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        PanelViewTeamGrid.Visibility = _activePanelTabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        PanelViewRubricGrid.Visibility = _activePanelTabIndex == 3 ? Visibility.Visible : Visibility.Collapsed;

        Button[] tabs = [BtnPanelTabCommits, BtnPanelTabDiscussions, BtnPanelTabTeam, BtnPanelTabRubric];
        for (int i = 0; i < tabs.Length; i++)
        {
            bool active = i == _activePanelTabIndex;
            tabs[i].BorderBrush = active
                ? (Brush)FindResource("AccentPrimaryBrush")
                : Brushes.Transparent;
            tabs[i].Foreground = active
                ? (Brush)FindResource("TextPrimaryBrush")
                : (Brush)FindResource("TextMutedBrush");
            tabs[i].FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal;
        }
    }

    private async Task LoadSelectedRepoAsync(RepositoryRecord repo, string? preferredFilePath)
    {
        _selectedRepo = repo;
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        LblRepoSlugTitle.Text = repo.RepoSlug;
        _canPushCurrentRepo = await _session.Rbac.CanPushToRepositoryAsync(actor, repo.RepoId);

        bool canSubmit = _canPushCurrentRepo && !repo.IsLocked &&
                         (!repo.DeadlineUtc.HasValue || repo.DeadlineUtc.Value >= DateTime.UtcNow);
        BtnTurnInToggle.IsEnabled = canSubmit;
        BtnTurnInToggle.Content = repo.IsSubmitted ? "Unsubmit" : "Turn In";

        bool isOwner = repo.OwnerStudentId == actor.UserId && !repo.IsLocked;
        BtnRepoRequestTicket.IsEnabled = isOwner;
        BtnRepoGrantPeer.IsEnabled = isOwner;

        TxtCodeViewer.IsReadOnly = !_canPushCurrentRepo;
        BtnToggleNewFileBox.Visibility = _canPushCurrentRepo ? Visibility.Visible : Visibility.Collapsed;
        HeaderLockIcon.Visibility = !_canPushCurrentRepo ? Visibility.Visible : Visibility.Collapsed;
        EditorLockIcon.Visibility = !_canPushCurrentRepo ? Visibility.Visible : Visibility.Collapsed;

        string lockReason = repo.IsLocked
            ? "Read-Only Mode: Repository is locked by faculty"
            : "Read-Only Mode";
        HeaderLockIcon.ToolTip = lockReason;
        EditorLockIcon.ToolTip = lockReason;

        double ratio = repo.SourceLines > 0 ? (double)repo.CommentLines / repo.SourceLines * 100.0 : 0.0;
        LblMetricSummaryLine.Text =
            $"{repo.TotalFiles} Files  |  {repo.SourceLines:N0} Source LOC  |  {repo.CommentLines:N0} Comments ({ratio:F1}%)  |  {repo.BlankLines:N0} Blanks";

        await LoadFileTreeAsync(repo, preferredFilePath);
        await LoadUnifiedCommitsAndPushesAsync(repo);
        await LoadTeamAndCommentsAsync(repo);
        await LoadEvaluationAndLanguagesAsync(repo);
    }

    private async Task LoadFileTreeAsync(RepositoryRecord repo, string? preferredFilePath)
    {
        TvFiles.Items.Clear();
        _suppressEditorTextChanged = true;
        TxtCodeViewer.Text = string.Empty;
        _suppressEditorTextChanged = false;
        _loadedFileContent = string.Empty;
        _currentEditingFilePath = null;
        LblSelectedFilePath.Text = "No file selected";
        InlineCommitGroup.Visibility = Visibility.Collapsed;

        try
        {
            List<RepoFileEntry> files = await _session.Browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
            var root = new TreeViewItem
            {
                Header = App.CreateExplorerNodeHeader(repo.RepoSlug, repo.RepoSlug, true),
                Tag = repo.RepoSlug,
                IsExpanded = true,
            };

            TreeViewItem? firstFileNode = null;
            TreeViewItem? preferredNode = null;

            foreach (RepoFileEntry f in files)
            {
                string[] parts = f.Path.Split('/');
                ItemsControl current = root;
                for (int i = 0; i < parts.Length; i++)
                {
                    bool isLeaf = i == parts.Length - 1;
                    string segment = parts[i];
                    TreeViewItem? existing = current.Items.OfType<TreeViewItem>()
                        .FirstOrDefault(item =>
                            item.Tag is string folderSeg && string.Equals(folderSeg, segment, StringComparison.Ordinal));

                    if (existing is null)
                    {
                        existing = new TreeViewItem
                        {
                            Header = App.CreateExplorerNodeHeader(segment, f.Path, !isLeaf),
                            Tag = isLeaf ? (object)f : segment,
                            IsExpanded = true,
                        };
                        current.Items.Add(existing);
                        if (isLeaf)
                        {
                            firstFileNode ??= existing;
                            if (!string.IsNullOrWhiteSpace(preferredFilePath) &&
                                string.Equals(f.Path, preferredFilePath, StringComparison.OrdinalIgnoreCase))
                            {
                                preferredNode = existing;
                            }
                        }
                    }

                    current = existing;
                }
            }

            TvFiles.Items.Add(root);
            TreeViewItem? targetNode = preferredNode ?? firstFileNode;
            if (targetNode is not null)
            {
                targetNode.IsSelected = true;
            }
        }
        catch (Exception ex)
        {
            TxtCodeViewer.Text = "Could not load repository file tree: " + ex.Message;
        }
    }

    private async void TvFiles_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem item && item.Tag is RepoFileEntry file && _selectedRepo is not null)
        {
            User? actor = _session.GetCurrentUserEntity();
            if (actor is null)
            {
                return;
            }

            _currentEditingFilePath = file.Path;
            LblActiveFileIcon.Text = App.GetFileIconGlyph(file.Path, false);
            LblSelectedFilePath.Text = file.Path;
            InlineCommitGroup.Visibility = _canPushCurrentRepo ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                RepoBlobContent blob = await _session.Browser.ReadFileBlobAuthorizedAsync(
                    _session.Rbac, actor, _selectedRepo.RepoId, _selectedRepo.BareRepoDiskPath, file.Path);
                _suppressEditorTextChanged = true;
                TxtCodeViewer.Text = blob.TextContent;
                _loadedFileContent = blob.TextContent;
                _suppressEditorTextChanged = false;
            }
            catch (Exception ex)
            {
                _suppressEditorTextChanged = true;
                TxtCodeViewer.Text = "Could not read file blob: " + ex.Message;
                _suppressEditorTextChanged = false;
            }
        }
    }

    private void TxtCodeViewer_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressEditorTextChanged || string.IsNullOrWhiteSpace(_currentEditingFilePath))
        {
            return;
        }

        bool dirty = !string.Equals(TxtCodeViewer.Text, _loadedFileContent, StringComparison.Ordinal);
        LblSelectedFilePath.Text = dirty
            ? $"{_currentEditingFilePath} *"
            : _currentEditingFilePath;
    }

    private void BtnToggleNewFileBox_Click(object sender, RoutedEventArgs e)
    {
        if (!_canPushCurrentRepo)
        {
            return;
        }

        if (NewFileBoxBorder.Visibility == Visibility.Visible)
        {
            NewFileBoxBorder.Visibility = Visibility.Collapsed;
        }
        else
        {
            NewFileBoxBorder.Visibility = Visibility.Visible;
            TxtNewFilePath.Focus();
            TxtNewFilePath.SelectAll();
        }
    }

    private void TxtNewFilePath_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            NewFileBoxBorder.Visibility = Visibility.Collapsed;
            TxtNewFilePath.Clear();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            CreateNewFileFromInput();
            e.Handled = true;
        }
    }

    private void CreateNewFileFromInput()
    {
        if (_selectedRepo is null || !_canPushCurrentRepo)
        {
            return;
        }

        string rawPath = TxtNewFilePath.Text.Replace('\\', '/').Trim().Trim('/');
        if (rawPath.Length == 0 || rawPath.Contains("..", StringComparison.Ordinal))
        {
            return;
        }

        NewFileBoxBorder.Visibility = Visibility.Collapsed;
        TxtNewFilePath.Clear();

        _currentEditingFilePath = rawPath;
        LblActiveFileIcon.Text = App.GetFileIconGlyph(rawPath, false);
        LblSelectedFilePath.Text = $"{rawPath} *";
        InlineCommitGroup.Visibility = Visibility.Visible;

        _suppressEditorTextChanged = true;
        _loadedFileContent = string.Empty;
        TxtCodeViewer.Text = $"// {rawPath}\n";
        _suppressEditorTextChanged = false;

        TxtCommitMessage.Text = $"add {rawPath}";
        TxtCodeViewer.Focus();
        TxtCodeViewer.CaretIndex = TxtCodeViewer.Text.Length;
    }

    private async void TxtCommitMessage_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await CommitCurrentFileAsync();
        }
    }

    private async void BtnCommitChanges_Click(object sender, RoutedEventArgs e)
    {
        await CommitCurrentFileAsync();
    }

    private async Task CommitCurrentFileAsync()
    {
        if (_selectedRepo is null || !_canPushCurrentRepo || string.IsNullOrWhiteSpace(_currentEditingFilePath))
        {
            return;
        }

        string commitMsg = TxtCommitMessage.Text.Trim();
        if (commitMsg.Length == 0)
        {
            commitMsg = $"update {_currentEditingFilePath}";
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        string targetPath = _currentEditingFilePath;
        string content = TxtCodeViewer.Text;

        BtnCommitChanges.IsEnabled = false;
        try
        {
            var (ok, message) = await _session.Repos.CommitFileInAppAsync(
                actor, _selectedRepo.RepoId, targetPath, content, commitMsg);
            if (!ok)
            {
                MessageBox.Show(message, "Commit", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TxtCommitMessage.Clear();
            RepositoryRecord? fresh = await _session.Repos.GetByIdAsync(_selectedRepo.RepoId);
            if (fresh is not null)
            {
                await LoadSelectedRepoAsync(fresh, targetPath);
            }

            ReportStatus($"Committed '{targetPath}' to '{_selectedRepo.RepoSlug}'.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Commit error: " + ex.Message,
                "Commit", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnCommitChanges.IsEnabled = true;
        }
    }

    private async Task LoadUnifiedCommitsAndPushesAsync(RepositoryRecord repo)
    {
        List<CommitLogEntry> commits = await _session.Repos.ListCommitLogsAsync(repo.RepoId);
        List<PushLedgerViewDto> pushes = await _session.Repos.ListPushLedgerViewAsync(repo.RepoId);

        var pushByNewSha = new Dictionary<string, PushLedgerViewDto>(StringComparer.OrdinalIgnoreCase);
        foreach (PushLedgerViewDto p in pushes)
        {
            pushByNewSha.TryAdd(p.NewCommitHash, p);
        }

        var rows = new List<UnifiedCommitRow>();
        foreach (CommitLogEntry c in commits)
        {
            string shortSha = c.CommitHash.Length >= 8 ? c.CommitHash[..8] : c.CommitHash;
            DateTime timestampUtc = pushByNewSha.TryGetValue(c.CommitHash, out PushLedgerViewDto? push)
                ? push.PushedAt
                : c.CommittedAt;

            string timeliness = !repo.DeadlineUtc.HasValue || timestampUtc <= repo.DeadlineUtc.Value
                ? "ON TIME"
                : "LATE";

            rows.Add(new UnifiedCommitRow(
                shortSha,
                c.CommitMessage,
                c.AuthorName,
                timestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                timeliness));
        }

        GridUnifiedCommits.ItemsSource = rows;
    }

    private async Task LoadTeamAndCommentsAsync(RepositoryRecord repo)
    {
        List<ContributorViewDto> contributors = await _session.Contributors.ListContributorViewsByRepoAsync(repo.RepoId);
        GridRepoTeam.ItemsSource = contributors;

        List<DiscussionCommentViewDto> comments = await _session.Discussions.ListCommentViewsByRepoAsync(repo.RepoId, false);
        GridComments.ItemsSource = comments.Select(cm => new CommentGridRow(
            cm.CommentId,
            $"{cm.AuthorFullName} ({cm.AuthorRole})",
            cm.IsModeratedHidden ? "[moderated by faculty]" : cm.CommentBody,
            cm.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))).ToList();
    }

    private async Task LoadEvaluationAndLanguagesAsync(RepositoryRecord repo)
    {
        EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(repo.RepoId);
        if (eval is not null)
        {
            LblEvalTotalScore.Text = $"Total: {eval.TotalScore} / 100";
            LblScoreArch.Text = $"Arch: {eval.ArchitectureScore}/25";
            LblScoreFunc.Text = $"Func: {eval.FunctionalityScore}/25";
            LblScoreQual.Text = $"Qual: {eval.CodeQualityScore}/25";
            LblScoreDoc.Text = $"Doc: {eval.DocumentationScore}/25";
            TxtEvalNotes.Text = string.IsNullOrWhiteSpace(eval.CritiqueNotes)
                ? "No faculty critique notes provided."
                : eval.CritiqueNotes;
        }
        else
        {
            LblEvalTotalScore.Text = "Total: -- / 100";
            LblScoreArch.Text = "Arch: 0/25";
            LblScoreFunc.Text = "Func: 0/25";
            LblScoreQual.Text = "Qual: 0/25";
            LblScoreDoc.Text = "Doc: 0/25";
            TxtEvalNotes.Text = "Pending faculty rubric evaluation.";
        }

        List<RepositoryLanguage> langs = await _session.Repos.ListLanguagesAsync(repo.RepoId);
        GridRepoLanguages.ItemsSource = langs.Select(l => new LangGridRow(
            l.LanguageName,
            l.FileCount,
            l.SourceLines,
            l.CommentLines,
            $"{l.PercentageShare:F1}%")).ToList();
    }

    private void BtnToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _toggleSidebar?.Invoke();
    }

    private void BtnToggleExplorer_Click(object sender, RoutedEventArgs e)
    {
        ExplorerPaneBorder.Visibility = ExplorerPaneBorder.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void BtnToggleBottomPanel_Click(object sender, RoutedEventArgs e)
    {
        ToggleBottomPanel();
    }

    private void BtnPanelTabCommits_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(0);

    private void BtnPanelTabDiscussions_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(1);

    private void BtnPanelTabTeam_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(2);

    private void BtnPanelTabRubric_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(3);

    private async void BtnBackToWorkspaces_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(0);
        await RefreshDataAsync();
    }

    private async void BtnOpenWorkspaceRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int repoId })
        {
            RepositoryRecord? target = _repos.FirstOrDefault(r => r.RepoId == repoId);
            if (target is not null)
            {
                await OpenRepoWorkspaceAsync(target);
            }
        }
    }

    private async void BtnOpenSelectedWorkspace_Click(object sender, RoutedEventArgs e)
    {
        if (GridMyRepos.SelectedIndex >= 0 && GridMyRepos.SelectedIndex < _repos.Count)
        {
            await OpenRepoWorkspaceAsync(_repos[GridMyRepos.SelectedIndex]);
        }
    }

    private async void GridMyRepos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (GridMyRepos.SelectedIndex >= 0 && GridMyRepos.SelectedIndex < _repos.Count)
        {
            await OpenRepoWorkspaceAsync(_repos[GridMyRepos.SelectedIndex]);
        }
    }

    private async void CboCourseFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        await RefreshDataAsync();
    }

    private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        await RefreshDataAsync();
        ReportStatus("Student workspace refreshed.");
    }

    private void GridAssignedTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GridAssignedTasks.SelectedIndex >= 0 && GridAssignedTasks.SelectedIndex < _tasks.Count)
        {
            CodingTask task = _tasks[GridAssignedTasks.SelectedIndex];
            LblTaskInstructions.Text = string.IsNullOrWhiteSpace(task.Instructions)
                ? $"Template: {task.StarterTemplate}"
                : task.Instructions;
        }
    }

    private async void BtnStartCodingTask_Click(object sender, RoutedEventArgs e)
    {
        if (GridAssignedTasks.SelectedIndex < 0 || GridAssignedTasks.SelectedIndex >= _tasks.Count)
        {
            MessageBox.Show("Select a coding task from the Assigned Tasks table first.",
                "Start Coding Task", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        CodingTask task = _tasks[GridAssignedTasks.SelectedIndex];
        BtnStartCodingTask.IsEnabled = false;
        try
        {
            var (ok, message, repo) = await _session.Repos.StartCodingTaskAsync(actor, task.TaskId);
            if (!ok || repo is null)
            {
                MessageBox.Show("Could not start coding task: " + message,
                    "Start Coding Task", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await RefreshDataAsync();
            await OpenRepoWorkspaceAsync(repo);
            ReportStatus($"Opened coding task workspace '{repo.RepoSlug}'.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not start coding task: " + ex.Message,
                "Start Coding Task", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnStartCodingTask.IsEnabled = true;
        }
    }

    private async void BtnTurnInToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        bool nextState = !_selectedRepo.IsSubmitted;
        BtnTurnInToggle.IsEnabled = false;
        try
        {
            var (ok, message) = await _session.Repos.SetSubmissionStateAsync(actor, _selectedRepo.RepoId, nextState);
            if (!ok)
            {
                MessageBox.Show(message, "Turn In", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RepositoryRecord? fresh = await _session.Repos.GetByIdAsync(_selectedRepo.RepoId);
            if (fresh is not null)
            {
                await LoadSelectedRepoAsync(fresh, _currentEditingFilePath);
            }

            ReportStatus(nextState ? $"Turned in '{_selectedRepo.RepoSlug}'." : $"Reverted '{_selectedRepo.RepoSlug}' to draft.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not update submission state: " + ex.Message,
                "Turn In", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnTurnInToggle.IsEnabled = true;
        }
    }

    private async void BtnCopyCloneCommand_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        string ip = LanHelper.GetPreferredLanIpv4Address();
        string cloneUrl = LanHelper.BuildCloneUrl(_selectedRepo.RepoSlug, _session.ServerPort, ip);
        string cmd = $"git clone {cloneUrl}";

        try
        {
            Clipboard.SetText(cmd);
            ReportStatus("Copied: " + cmd);

            if (TryFindResource("IconCheck") is Geometry checkGeo)
            {
                CloneBtnIconPath.Data = checkGeo;
            }

            LblCloneBtnText.Text = "Copied";
            await Task.Delay(1500);

            if (TryFindResource("IconCopy") is Geometry copyGeo)
            {
                CloneBtnIconPath.Data = copyGeo;
            }

            LblCloneBtnText.Text = "Clone";
        }
        catch (Exception ex)
        {
            ReportStatus("Clipboard error: " + ex.Message);
        }
    }

    private RepositoryRecord? GetSelectedOwnedRepoForTickets()
    {
        int idx = CboTicketRepoSelector.SelectedIndex;
        if (idx >= 0 && idx < _ownedRepos.Count)
        {
            return _ownedRepos[idx];
        }

        return _selectedRepo;
    }

    private async void BtnRequestGroupmateTicket_Click(object sender, RoutedEventArgs e)
    {
        RepositoryRecord? target = GetSelectedOwnedRepoForTickets();
        if (target is null)
        {
            return;
        }

        await ShowRequestTicketDialogAsync(target);
    }

    private async void BtnRepoRequestTicket_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is not null)
        {
            await ShowRequestTicketDialogAsync(_selectedRepo);
        }
    }

    private async Task ShowRequestTicketDialogAsync(RepositoryRecord target)
    {
        var dlg = new RequestTicketWindow(_session, target.RepoId)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await RefreshAllTicketsTableAsync();
            if (_selectedRepo is not null && _selectedRepo.RepoId == target.RepoId)
            {
                await LoadTeamAndCommentsAsync(_selectedRepo);
            }

            ReportStatus($"Groupmate ticket submitted for '{target.RepoSlug}'.");
        }
    }

    private async void BtnGrantPeerAccess_Click(object sender, RoutedEventArgs e)
    {
        RepositoryRecord? target = GetSelectedOwnedRepoForTickets();
        if (target is null)
        {
            return;
        }

        await ShowGrantPeerDialogAsync(target);
    }

    private async void BtnRepoGrantPeer_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is not null)
        {
            await ShowGrantPeerDialogAsync(_selectedRepo);
        }
    }

    private async Task ShowGrantPeerDialogAsync(RepositoryRecord target)
    {
        var dlg = new GrantPeerWindow(_session, target.RepoId)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await RefreshAllTicketsTableAsync();
            if (_selectedRepo is not null && _selectedRepo.RepoId == target.RepoId)
            {
                await LoadTeamAndCommentsAsync(_selectedRepo);
            }

            ReportStatus($"Peer viewer access granted on '{target.RepoSlug}'.");
        }
    }

    private async void BtnPostComment_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        string body = TxtNewComment.Text.Trim();
        if (body.Length == 0)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        BtnPostComment.IsEnabled = false;
        try
        {
            var (ok, message, _) = await _session.Discussions.AddCommentAsync(actor, _selectedRepo.RepoId, body);
            if (!ok)
            {
                MessageBox.Show(message, "Post Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TxtNewComment.Clear();
            await LoadTeamAndCommentsAsync(_selectedRepo);
            ReportStatus("Discussion comment posted.");
        }
        finally
        {
            BtnPostComment.IsEnabled = true;
        }
    }

    private void ReportStatus(string text)
    {
        _setStatus?.Invoke(text);
    }

    private sealed record StudentRepoRow(
        int RepoId, string RepoSlug, string Title, string CourseCode,
        string AccessRole, string StatusBadge, string DeadlineBadge, string SourceLocText);

    private sealed record StudentTaskRow(
        int TaskId, string Title, string CourseCode, string StarterTemplate,
        string TypeBadge, string DeadlineText);

    private sealed record TicketTableRow(
        string RepoSlug, string FullName, string Username, string TeamRole,
        string AccessLevel, string ApprovalStatus, string AssignedAtText);

    private sealed record UnifiedCommitRow(
        string ShortSha, string CommitMessage, string AuthorName, string TimestampText, string TimelinessText);

    private sealed record CommentGridRow(
        int CommentId, string AuthorDisplay, string CommentBody, string CreatedAtText);

    private sealed record LangGridRow(
        string LanguageName, int FileCount, int SourceLines, int CommentLines, string ShareText);
}
