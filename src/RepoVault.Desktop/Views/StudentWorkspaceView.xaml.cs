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
    private bool _canPushCurrentRepo;
    private bool _isLoading;
    private int _activePageIndex;
    private int _activeSubViewIndex;

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
        HighlightSubViewButton(0);
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

            // if currently inside DedicatedRepoWorkspaceGrid, refresh the active repo
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
                    string.IsNullOrWhiteSpace(item.ReviewedByFullName) ? "--" : item.ReviewedByFullName));
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
        SwitchSubView(0);
        await LoadSelectedRepoAsync(repo, null);
    }

    private void SwitchSubView(int subViewIndex)
    {
        _activeSubViewIndex = Math.Clamp(subViewIndex, 0, 3);
        SubViewCodeEditorGrid.Visibility = _activeSubViewIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        SubViewCommitsGrid.Visibility = _activeSubViewIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        SubViewDiscussionBorder.Visibility = _activeSubViewIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        SubViewGradeBorder.Visibility = _activeSubViewIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        HighlightSubViewButton(_activeSubViewIndex);
    }

    private void HighlightSubViewButton(int activeIdx)
    {
        Button[] buttons = [BtnSubViewCode, BtnSubViewCommits, BtnSubViewDiscussion, BtnSubViewGrade];
        for (int i = 0; i < buttons.Length; i++)
        {
            bool isSelected = i == activeIdx;
            buttons[i].BorderBrush = isSelected
                ? (Brush)FindResource("AccentPrimaryBrush")
                : (Brush)FindResource("BorderSubtleBrush");
            buttons[i].FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Normal;
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

        string courseCode = _courses.FirstOrDefault(c => c.CourseId == repo.CourseId)?.CourseCode ?? $"COURSE #{repo.CourseId}";
        LblRepoTitle.Text = repo.Title;
        LblRepoSlugBadge.Text = repo.RepoSlug;
        LblCourseBadge.Text = courseCode;

        string dlBadge = ComputeDeadlineBadge(repo);
        LblDeadlineBadge.Text = dlBadge;
        DeadlineBadgeBorder.BorderBrush = dlBadge == "LATE"
            ? (Brush)FindResource("AccentDangerBrush")
            : (Brush)FindResource("AccentSuccessBrush");

        string ip = LanHelper.GetPreferredLanIpv4Address();
        string cloneUrl = LanHelper.BuildCloneUrl(repo.RepoSlug, _session.ServerPort, ip);
        TxtCloneCommand.Text = $"git clone {cloneUrl}";
        LblRepoDescription.Text = string.IsNullOrWhiteSpace(repo.Description)
            ? "No additional instructions provided for this repository."
            : repo.Description;

        _canPushCurrentRepo = await _session.Rbac.CanPushToRepositoryAsync(actor, repo.RepoId);

        bool canSubmit = _canPushCurrentRepo && !repo.IsLocked &&
                         (!repo.DeadlineUtc.HasValue || repo.DeadlineUtc.Value >= DateTime.UtcNow);
        BtnTurnInToggle.IsEnabled = canSubmit;
        BtnTurnInToggle.Content = repo.IsSubmitted ? "Unsubmit" : "Turn In Task";

        // configure in-app code editor write vs read-only state
        TxtCodeViewer.IsReadOnly = !_canPushCurrentRepo;
        NewFileBarGrid.Visibility = _canPushCurrentRepo ? Visibility.Visible : Visibility.Collapsed;
        CommitBarBorder.Visibility = _canPushCurrentRepo ? Visibility.Visible : Visibility.Collapsed;
        ReadOnlyBannerBorder.Visibility = _canPushCurrentRepo ? Visibility.Collapsed : Visibility.Visible;

        LblEditorModeBadge.Text = _canPushCurrentRepo ? "EDITABLE" : "READ-ONLY";
        EditorModeBadgeBorder.BorderBrush = _canPushCurrentRepo
            ? (Brush)FindResource("AccentSuccessBrush")
            : (Brush)FindResource("BorderSubtleBrush");

        if (!_canPushCurrentRepo)
        {
            if (repo.IsLocked)
            {
                LblReadOnlyReason.Text = "Read-Only Mode: This repository is locked by faculty.";
            }
            else if (repo.DeadlineUtc.HasValue && repo.DeadlineUtc.Value < DateTime.UtcNow)
            {
                LblReadOnlyReason.Text = "Read-Only Mode: The submission deadline for this repository has passed.";
            }
            else
            {
                LblReadOnlyReason.Text = "Read-Only Mode: Only the repository owner and approved groupmates can edit and commit.";
            }
        }

        double ratio = repo.SourceLines > 0 ? (double)repo.CommentLines / repo.SourceLines * 100.0 : 0.0;
        LblMetricFiles.Text = $"Total Files: {repo.TotalFiles}";
        LblMetricSourceLoc.Text = $"Source LOC: {repo.SourceLines:N0}";
        LblMetricCommentLoc.Text = $"Comment LOC: {repo.CommentLines:N0}";
        LblMetricBlankLoc.Text = $"Blank LOC: {repo.BlankLines:N0}";
        LblMetricRatio.Text = repo.SourceLines > 0 ? $"Comment Ratio: {ratio:F1}%" : "Comment Ratio: N/A";
        LblMetricLang.Text = $"Primary Language: {repo.PrimaryLanguage ?? "--"}";

        await LoadFileTreeAsync(repo, preferredFilePath);
        await LoadCommitsAndPushesAsync(repo);
        await LoadCommentsAsync(repo);
        await LoadEvaluationAsync(repo);
    }

    private async Task LoadFileTreeAsync(RepositoryRecord repo, string? preferredFilePath)
    {
        TvFiles.Items.Clear();
        TxtCodeViewer.Text = string.Empty;
        _currentEditingFilePath = null;
        LblSelectedFilePath.Text = "No file selected";
        LblSelectedFileSize.Text = string.Empty;

        try
        {
            List<RepoFileEntry> files = await _session.Browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
            var root = new TreeViewItem
            {
                Header = repo.RepoSlug,
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
                        .FirstOrDefault(item => string.Equals(item.Header?.ToString(), segment, StringComparison.Ordinal));

                    if (existing is null)
                    {
                        existing = new TreeViewItem
                        {
                            Header = segment,
                            Tag = isLeaf ? f : null,
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
            LblSelectedFilePath.Text = file.Path;
            LblSelectedFileSize.Text = $"{file.SizeBytes:N0} bytes";
            if (string.IsNullOrWhiteSpace(TxtCommitMessage.Text))
            {
                TxtCommitMessage.Text = $"update {file.Path}";
            }

            try
            {
                RepoBlobContent blob = await _session.Browser.ReadFileBlobAuthorizedAsync(
                    _session.Rbac, actor, _selectedRepo.RepoId, _selectedRepo.BareRepoDiskPath, file.Path);
                TxtCodeViewer.Text = blob.TextContent;
            }
            catch (Exception ex)
            {
                TxtCodeViewer.Text = "Could not read file blob: " + ex.Message;
            }
        }
    }

    private void BtnCreateNewFile_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null || !_canPushCurrentRepo)
        {
            return;
        }

        string rawPath = TxtNewFilePath.Text.Replace('\\', '/').Trim().Trim('/');
        if (rawPath.Length == 0 || rawPath.Contains("..", StringComparison.Ordinal))
        {
            MessageBox.Show("Enter a valid relative file path (for example: src/Helper.cs or Notes.md).",
                "New File", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _currentEditingFilePath = rawPath;
        LblSelectedFilePath.Text = $"{rawPath} (new file - uncommitted)";
        LblSelectedFileSize.Text = "0 bytes";
        TxtCodeViewer.Text = $"// {rawPath}\n";
        TxtCommitMessage.Text = $"add {rawPath}";
        TxtNewFilePath.Clear();
        TxtCodeViewer.Focus();
        TxtCodeViewer.CaretIndex = TxtCodeViewer.Text.Length;
        ReportStatus($"Prepared new file '{rawPath}'. Edit code and click Commit Changes.");
    }

    private async void BtnCommitChanges_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null || !_canPushCurrentRepo)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentEditingFilePath))
        {
            MessageBox.Show("Select a file from the left tree or create a new file with '+ File' first.",
                "Commit Changes", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string commitMsg = TxtCommitMessage.Text.Trim();
        if (commitMsg.Length == 0)
        {
            MessageBox.Show("Enter a commit message before committing changes.",
                "Commit Changes", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
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
                MessageBox.Show(message, "Commit Changes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TxtCommitMessage.Clear();
            RepositoryRecord? fresh = await _session.Repos.GetByIdAsync(_selectedRepo.RepoId);
            if (fresh is not null)
            {
                await LoadSelectedRepoAsync(fresh, targetPath);
            }

            ReportStatus($"Committed '{targetPath}' to '{_selectedRepo.RepoSlug}' and updated code metrics.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Commit error: " + ex.Message,
                "Commit Changes", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnCommitChanges.IsEnabled = true;
        }
    }

    private async Task LoadCommitsAndPushesAsync(RepositoryRecord repo)
    {
        List<CommitLogEntry> commits = await _session.Repos.ListCommitLogsAsync(repo.RepoId);
        GridCommits.ItemsSource = commits.Select(c => new CommitGridRow(
            c.CommitHash.Length >= 8 ? c.CommitHash[..8] : c.CommitHash,
            c.CommitMessage,
            c.AuthorName,
            c.CommittedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"))).ToList();

        List<PushLedgerViewDto> pushes = await _session.Repos.ListPushLedgerViewAsync(repo.RepoId);
        GridPushes.ItemsSource = pushes.Select(p => new PushGridRow(
            p.PushId,
            p.BranchRef,
            p.OldCommitHash.Length >= 8 ? p.OldCommitHash[..8] : p.OldCommitHash,
            p.NewCommitHash.Length >= 8 ? p.NewCommitHash[..8] : p.NewCommitHash,
            p.PusherFullName,
            p.PushedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"))).ToList();
    }

    private async Task LoadCommentsAsync(RepositoryRecord repo)
    {
        List<DiscussionCommentViewDto> comments = await _session.Discussions.ListCommentViewsByRepoAsync(repo.RepoId, false);
        GridComments.ItemsSource = comments.Select(cm => new CommentGridRow(
            cm.CommentId,
            $"{cm.AuthorFullName} ({cm.AuthorRole})",
            cm.IsModeratedHidden ? "[moderated by faculty]" : cm.CommentBody,
            cm.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))).ToList();
    }

    private async Task LoadEvaluationAsync(RepositoryRecord repo)
    {
        EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(repo.RepoId);
        if (eval is not null)
        {
            LblEvalStatus.Text = $"Status: Graded on {eval.EvaluatedAt.ToLocalTime():yyyy-MM-dd HH:mm}";
            LblEvalTotalScore.Text = $"{eval.TotalScore} / 100";
            LblScoreArch.Text = $"{eval.ArchitectureScore} / 25";
            LblScoreFunc.Text = $"{eval.FunctionalityScore} / 25";
            LblScoreQual.Text = $"{eval.CodeQualityScore} / 25";
            LblScoreDoc.Text = $"{eval.DocumentationScore} / 25";
            BarScoreArch.Value = eval.ArchitectureScore;
            BarScoreFunc.Value = eval.FunctionalityScore;
            BarScoreQual.Value = eval.CodeQualityScore;
            BarScoreDoc.Value = eval.DocumentationScore;
            TxtEvalNotes.Text = string.IsNullOrWhiteSpace(eval.CritiqueNotes)
                ? "No faculty evaluation notes provided."
                : eval.CritiqueNotes;
        }
        else
        {
            LblEvalStatus.Text = "Status: Pending Faculty Evaluation";
            LblEvalTotalScore.Text = "-- / 100";
            LblScoreArch.Text = "0 / 25";
            LblScoreFunc.Text = "0 / 25";
            LblScoreQual.Text = "0 / 25";
            LblScoreDoc.Text = "0 / 25";
            BarScoreArch.Value = 0;
            BarScoreFunc.Value = 0;
            BarScoreQual.Value = 0;
            BarScoreDoc.Value = 0;
            TxtEvalNotes.Text = "This repository has not been graded yet.";
        }
    }

    private void BtnToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _toggleSidebar?.Invoke();
    }

    private async void BtnBackToWorkspaces_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(0);
        await RefreshDataAsync();
    }

    private void BtnSubViewCode_Click(object sender, RoutedEventArgs e) => SwitchSubView(0);

    private void BtnSubViewCommits_Click(object sender, RoutedEventArgs e) => SwitchSubView(1);

    private void BtnSubViewDiscussion_Click(object sender, RoutedEventArgs e) => SwitchSubView(2);

    private void BtnSubViewGrade_Click(object sender, RoutedEventArgs e) => SwitchSubView(3);

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

    private void CboTicketRepoSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
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
                MessageBox.Show(message, "Turn In Task", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                "Turn In Task", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnTurnInToggle.IsEnabled = true;
        }
    }

    private void BtnCopyCloneCommand_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtCloneCommand.Text))
        {
            return;
        }

        try
        {
            Clipboard.SetText(TxtCloneCommand.Text);
            ReportStatus("Copied clone command to clipboard: " + TxtCloneCommand.Text);
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
            MessageBox.Show("Select one of your owned repositories first.",
                "Request Groupmate Ticket", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dlg = new RequestTicketWindow(_session, target.RepoId)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await RefreshAllTicketsTableAsync();
            ReportStatus($"Groupmate collaboration ticket submitted for '{target.RepoSlug}'.");
        }
    }

    private async void BtnGrantPeerAccess_Click(object sender, RoutedEventArgs e)
    {
        RepositoryRecord? target = GetSelectedOwnedRepoForTickets();
        if (target is null)
        {
            MessageBox.Show("Select one of your owned repositories first.",
                "Grant Peer Access", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dlg = new GrantPeerWindow(_session, target.RepoId)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await RefreshAllTicketsTableAsync();
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
            await LoadCommentsAsync(_selectedRepo);
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
        string AccessLevel, string ApprovalStatus, string ReviewedByFullName);

    private sealed record CommitGridRow(
        string ShortSha, string CommitMessage, string AuthorName, string CommittedAtText);

    private sealed record PushGridRow(
        int PushId, string BranchRef, string OldShortSha, string NewShortSha, string PusherFullName, string PushedAtText);

    private sealed record CommentGridRow(
        int CommentId, string AuthorDisplay, string CommentBody, string CreatedAtText);
}
