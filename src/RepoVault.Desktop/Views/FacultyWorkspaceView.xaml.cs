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

public partial class FacultyWorkspaceView : UserControl
{
    private readonly AppSession _session;
    private readonly Action<string>? _setStatus;
    private readonly Action? _toggleSidebar;
    private readonly Action<bool>? _setSidebarCollapsed;
    private readonly Action<int>? _updateNavHighlight;
    private readonly Action<int>? _updatePendingTicketsBadge;

    private readonly List<Course> _courses = new();
    private readonly List<RepositoryRecord> _submissions = new();
    private readonly List<CodingTask> _tasks = new();
    private readonly List<ContributorViewDto> _tickets = new();
    private readonly List<TicketGridRow> _ticketRows = new();
    private readonly List<ContributorViewDto> _inspectorTeam = new();
    private readonly List<RepositoryRecord> _trashed = new();
    private readonly List<DiscussionCommentViewDto> _comments = new();
    private readonly Dictionary<int, string> _userLookup = new();

    private Course? _selectedCourse;
    private RepositoryRecord? _selectedRepo;
    private bool _isLoading;
    private int _activePageIndex;
    private int _activeInspPanelTabIndex;

    public FacultyWorkspaceView() : this(new AppSession(), null, null, null, null, null)
    {
    }

    public FacultyWorkspaceView(
        AppSession session,
        Action<string>? setStatus = null,
        Action? toggleSidebar = null,
        Action<bool>? setSidebarCollapsed = null,
        Action<int>? updateNavHighlight = null,
        Action<int>? updatePendingTicketsBadge = null)
    {
        _session = session;
        _setStatus = setStatus;
        _toggleSidebar = toggleSidebar;
        _setSidebarCollapsed = setSidebarCollapsed;
        _updateNavHighlight = updateNavHighlight;
        _updatePendingTicketsBadge = updatePendingTicketsBadge;
        InitializeComponent();
        SwitchBottomPanelTab(0);
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        await LoadCoursesAsync();
    }

    public void SetSidebarOpenState(bool isSidebarOpen)
    {
        Visibility toggleVis = isSidebarOpen ? Visibility.Collapsed : Visibility.Visible;
        BtnPageSidebarToggle.Visibility = toggleVis;
        BtnWorkspaceSidebarToggle.Visibility = toggleVis;
    }

    public void NavigateToPage(int pageIndex)
    {
        _activePageIndex = Math.Clamp(pageIndex, 0, 3);

        DedicatedInspectorGrid.Visibility = Visibility.Collapsed;
        TopLevelPagesRoot.Visibility = Visibility.Visible;
        _setSidebarCollapsed?.Invoke(false);

        PageCoursesTasksGrid.Visibility = _activePageIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        PageSubmissionsBorder.Visibility = _activePageIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        PageTicketsBorder.Visibility = _activePageIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        PageTrashBorder.Visibility = _activePageIndex == 3 ? Visibility.Visible : Visibility.Collapsed;

        LblActivePageTitle.Text = _activePageIndex switch
        {
            0 => "Courses & Tasks",
            1 => "Student Submissions",
            2 => "Groupmate Tickets",
            3 => "Repo Bin",
            _ => "Courses & Tasks",
        };

        _updateNavHighlight?.Invoke(_activePageIndex);
    }

    public void ToggleBottomPanel()
    {
        if (DedicatedInspectorGrid.Visibility != Visibility.Visible)
        {
            return;
        }

        BottomPanelBorder.Visibility = BottomPanelBorder.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public async Task LoadCoursesAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            int prevCourseId = _selectedCourse?.CourseId ?? 0;

            List<UserSummaryDto> users = await _session.Users.ListUserSummariesAsync();
            _userLookup.Clear();
            foreach (UserSummaryDto u in users)
            {
                _userLookup[u.UserId] = $"{u.FullName} ({u.Username})";
            }

            List<Course> list = await _session.Courses.ListCoursesAsync();
            _courses.Clear();
            _courses.AddRange(list);

            GridCourses.ItemsSource = null;
            GridCourses.ItemsSource = _courses;

            CboActiveCourse.Items.Clear();
            foreach (Course c in _courses)
            {
                CboActiveCourse.Items.Add($"{c.CourseCode} - {c.CourseTitle}");
            }

            if (_courses.Count > 0)
            {
                int idx = _courses.FindIndex(c => c.CourseId == prevCourseId);
                if (idx < 0)
                {
                    idx = 0;
                }

                GridCourses.SelectedIndex = idx;
                CboActiveCourse.SelectedIndex = idx;
                _selectedCourse = _courses[idx];
            }
            else
            {
                _selectedCourse = null;
            }
        }
        catch (Exception ex)
        {
            ReportStatus("Could not load courses: " + ex.Message);
        }
        finally
        {
            _isLoading = false;
        }

        await LoadTrashedReposAsync();

        if (_selectedCourse is not null)
        {
            await LoadSelectedCourseAsync(_selectedCourse);
        }
    }

    public async Task RefreshDataAsync()
    {
        await LoadTrashedReposAsync();
        if (_selectedCourse is not null)
        {
            await LoadSelectedCourseAsync(_selectedCourse);
        }
        else
        {
            await LoadCoursesAsync();
        }
    }

    private async Task LoadTrashedReposAsync()
    {
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        try
        {
            var (ok, _, repos) = await _session.Repos.ListTrashedInRepoBinAsync(actor);
            if (!ok)
            {
                return;
            }

            _trashed.Clear();
            _trashed.AddRange(repos);
            GridTrashedRepos.ItemsSource = null;
            GridTrashedRepos.ItemsSource = _trashed;
            if (_trashed.Count > 0 && GridTrashedRepos.SelectedIndex < 0)
            {
                GridTrashedRepos.SelectedIndex = 0;
            }
        }
        catch
        {
        }
    }

    private async Task LoadSelectedCourseAsync(Course course)
    {
        if (_isLoading)
        {
            return;
        }

        _selectedCourse = course;
        _isLoading = true;
        try
        {
            int prevRepoId = _selectedRepo?.RepoId ?? 0;

            LblSelectedCourseHeader.Text = $"{course.CourseCode} - {course.CourseTitle}";
            LblSelectedCourseSub.Text = $"{course.Department}  |  {course.Semester}";

            _submissions.Clear();
            List<RepositoryRecord> repos = await _session.Repos.ListActiveByCourseAsync(course.CourseId);
            _submissions.AddRange(repos);

            var repoById = _submissions.ToDictionary(r => r.RepoId, r => r);
            var subRows = new List<SubmissionGridRow>();
            int selectRepoIdx = -1;

            for (int i = 0; i < _submissions.Count; i++)
            {
                RepositoryRecord r = _submissions[i];
                string ownerText = r.OwnerStudentId.HasValue && _userLookup.TryGetValue(r.OwnerStudentId.Value, out string? name)
                    ? name
                    : (r.OwnerStudentId.HasValue ? "Student #" + r.OwnerStudentId.Value : "Course");

                string submittedText = r.IsSubmitted
                    ? (r.SubmittedAt.HasValue ? r.SubmittedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "Submitted")
                    : "Draft";

                string deadlineStatus = ComputeDeadlineStatus(r);

                EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(r.RepoId);
                string scoreText = eval is not null ? $"{eval.TotalScore} / 100" : "-- / 100";

                subRows.Add(new SubmissionGridRow(
                    r.RepoId,
                    r.RepoSlug,
                    r.Title,
                    ownerText,
                    submittedText,
                    deadlineStatus,
                    scoreText,
                    r.IsLocked ? "Locked" : "Open"));

                if (r.RepoId == prevRepoId)
                {
                    selectRepoIdx = i;
                }
            }

            GridSubmissions.ItemsSource = subRows;
            if (_submissions.Count > 0)
            {
                int target = selectRepoIdx >= 0 ? selectRepoIdx : 0;
                GridSubmissions.SelectedIndex = target;
                _selectedRepo = _submissions[target];
            }
            else
            {
                _selectedRepo = null;
            }

            _tasks.Clear();
            _tasks.AddRange(await _session.Courses.ListTasksByCourseAsync(course.CourseId));
            GridCodingTasks.ItemsSource = _tasks.Select(t => new TaskGridRow(
                t.TaskId,
                t.Title,
                t.StarterTemplate,
                t.DeadlineUtc.HasValue ? t.DeadlineUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "Open",
                t.AllowGroupWork ? $"Group (Max {t.MaxGroupSize})" : "Solo Task")).ToList();

            _tickets.Clear();
            _tickets.AddRange(await _session.Contributors.ListTicketsByCourseAsync(course.CourseId));
            _ticketRows.Clear();

            foreach (ContributorViewDto tk in _tickets)
            {
                repoById.TryGetValue(tk.RepoId, out RepositoryRecord? r);
                string slug = r?.RepoSlug ?? ("#" + tk.RepoId);
                string rTitle = r?.Title ?? slug;
                string ownerDisplay = r is not null && r.OwnerStudentId.HasValue &&
                                      _userLookup.TryGetValue(r.OwnerStudentId.Value, out string? ow)
                    ? ow
                    : (r?.OwnerStudentId.HasValue == true ? "Student #" + r.OwnerStudentId.Value : "Course Faculty");

                _ticketRows.Add(new TicketGridRow(
                    tk.RepoId,
                    tk.StudentUserId,
                    slug,
                    rTitle,
                    course.CourseCode,
                    ownerDisplay,
                    $"{tk.FullName} ({tk.Username})",
                    tk.TeamRole,
                    tk.AccessLevel.ToString(),
                    tk.ApprovalStatus.ToString(),
                    tk.AssignedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")));
            }

            GridTickets.ItemsSource = null;
            GridTickets.ItemsSource = _ticketRows;

            if (_ticketRows.Count > 0 && GridTickets.SelectedIndex < 0)
            {
                GridTickets.SelectedIndex = 0;
            }

            UpdateTicketActionButtonsState();
            UpdateFacultySelectedTicketPreview();

            int pendingCount = _tickets.Count(tk =>
                tk.AccessLevel == AccessLevel.GroupCollaborator &&
                tk.ApprovalStatus == TicketApprovalStatus.Pending);

            _updatePendingTicketsBadge?.Invoke(pendingCount);
            LblPendingTicketsAlert.Text = pendingCount > 0
                ? $"{pendingCount} Pending GroupCollaborator ticket(s) in {course.CourseCode} require faculty approval."
                : $"Groupmate ticket queue for {course.CourseCode} (0 pending).";

            if (DedicatedInspectorGrid.Visibility == Visibility.Visible && prevRepoId > 0)
            {
                RepositoryRecord? fresh = await _session.Repos.GetByIdAsync(prevRepoId);
                if (fresh is not null)
                {
                    await InspectSubmissionAsync(fresh);
                }
            }
        }
        catch (Exception ex)
        {
            ReportStatus("Could not load course data: " + ex.Message);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void UpdateTicketActionButtonsState()
    {
        bool isGroupTicket = GridTickets.SelectedIndex >= 0 &&
                             GridTickets.SelectedIndex < _tickets.Count &&
                             _tickets[GridTickets.SelectedIndex].AccessLevel == AccessLevel.GroupCollaborator;

        BtnApproveTicket.IsEnabled = isGroupTicket;
        BtnRejectTicket.IsEnabled = isGroupTicket;
    }

    private void UpdateFacultySelectedTicketPreview()
    {
        if (GridTickets.SelectedIndex < 0 || GridTickets.SelectedIndex >= _ticketRows.Count)
        {
            LblFacultyTicketDetailHeader.Text = "Select a row above to inspect ticket ownership, target student, and access permissions.";
            LblFacultyTicketDetailParties.Text = string.Empty;
            LblFacultyTicketDetailAccess.Text = string.Empty;
            return;
        }

        TicketGridRow row = _ticketRows[GridTickets.SelectedIndex];
        LblFacultyTicketDetailHeader.Text = $"Repository: {row.RepoTitle} ({row.RepoSlug})  |  Course: {row.CourseCode}";
        LblFacultyTicketDetailParties.Text =
            $"Repository Owner (Requested / Granted By): {row.OwnerDisplay}  |  Invited User: {row.StudentDisplay}  |  Team Role: {row.TeamRole}";
        LblFacultyTicketDetailAccess.Text =
            $"Role & Access Type: {DescribeAccessLevel(row.AccessLevel)}  |  Approval Status: {DescribeApprovalStatus(row.ApprovalStatus)}";
    }

    private static string DescribeAccessLevel(string accessLevel) => accessLevel switch
    {
        "GroupCollaborator" => "GroupCollaborator - Full Code & Push Access upon Approval",
        "PeerViewer" => "PeerViewer - Metadata & README Only",
        "Owner" => "Owner - Repository Creator (Full Code & Push Access)",
        "FacultyReviewer" => "FacultyReviewer - Course Instructor Read & Grading Access",
        _ => accessLevel,
    };

    private static string DescribeApprovalStatus(string status) => status switch
    {
        "Pending" => "Pending Faculty Review",
        "Approved" => "Approved",
        "Rejected" => "Rejected",
        _ => status,
    };

    private void UpdateInspectorTeamButtonsState()
    {
        bool isGroupTicket = GridInspectorTeam.SelectedIndex >= 0 &&
                             GridInspectorTeam.SelectedIndex < _inspectorTeam.Count &&
                             _inspectorTeam[GridInspectorTeam.SelectedIndex].AccessLevel == AccessLevel.GroupCollaborator;

        BtnInspApproveTicket.IsEnabled = isGroupTicket;
        BtnInspRejectTicket.IsEnabled = isGroupTicket;
    }

    private static string ComputeDeadlineStatus(RepositoryRecord r)
    {
        if (!r.DeadlineUtc.HasValue)
        {
            return "ON TIME";
        }

        if (r.IsSubmitted && r.SubmittedAt.HasValue)
        {
            return r.SubmittedAt.Value <= r.DeadlineUtc.Value ? "ON TIME" : "LATE";
        }

        return DateTime.UtcNow > r.DeadlineUtc.Value ? "LATE" : "ON TIME";
    }

    public async Task OpenInspectionViewAsync(RepositoryRecord repo)
    {
        TopLevelPagesRoot.Visibility = Visibility.Collapsed;
        DedicatedInspectorGrid.Visibility = Visibility.Visible;
        _setSidebarCollapsed?.Invoke(true);
        BottomPanelBorder.Visibility = Visibility.Visible;
        SwitchBottomPanelTab(0);
        await InspectSubmissionAsync(repo);
    }

    private void SwitchBottomPanelTab(int tabIdx)
    {
        _activeInspPanelTabIndex = Math.Clamp(tabIdx, 0, 4);
        InspPanelTaskSpecGrid.Visibility = _activeInspPanelTabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        GridInspectorUnifiedCommits.Visibility = _activeInspPanelTabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        InspPanelDiscussionsGrid.Visibility = _activeInspPanelTabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        InspPanelTeamGrid.Visibility = _activeInspPanelTabIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        InspPanelRubricGrid.Visibility = _activeInspPanelTabIndex == 4 ? Visibility.Visible : Visibility.Collapsed;

        Button[] tabs = [BtnInspPanelTabSpec, BtnInspPanelTabCommits, BtnInspPanelTabDiscussions, BtnInspPanelTabTeam, BtnInspPanelTabRubric];
        for (int i = 0; i < tabs.Length; i++)
        {
            bool active = i == _activeInspPanelTabIndex;
            tabs[i].BorderBrush = active
                ? (Brush)FindResource("AccentPrimaryBrush")
                : Brushes.Transparent;
            tabs[i].Foreground = active
                ? (Brush)FindResource("TextPrimaryBrush")
                : (Brush)FindResource("TextMutedBrush");
            tabs[i].FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal;
        }
    }

    private async Task InspectSubmissionAsync(RepositoryRecord repo)
    {
        _selectedRepo = repo;
        LblInspectorSlugTitle.Text = repo.RepoSlug;
        HeaderLockIcon.Visibility = repo.IsLocked ? Visibility.Visible : Visibility.Collapsed;

        string iconKey = repo.IsLocked ? "IconUnlock" : "IconLock";
        if (TryFindResource(iconKey) is Geometry lockGeo)
        {
            LockToggleIconPath.Data = lockGeo;
        }

        BtnLockUnlockRepo.ToolTip = repo.IsLocked ? "Unlock Repository" : "Lock Repository";

        double ratio = repo.SourceLines > 0 ? (double)repo.CommentLines / repo.SourceLines * 100.0 : 0.0;
        LblInspMetricSummaryLine.Text =
            $"{repo.TotalFiles} Files  |  {repo.SourceLines:N0} Source LOC  |  {repo.CommentLines:N0} Comments ({ratio:F1}%)  |  {repo.BlankLines:N0} Blanks";

        await LoadInspectorTaskSpecAsync(repo);
        await LoadInspectorFilesAsync(repo);
        await LoadInspectorUnifiedCommitsAsync(repo);
        await LoadInspectorTeamAndDiscussionsAsync(repo);
        await LoadInspectorRubricAndLanguagesAsync(repo);
    }

    private async Task LoadInspectorTaskSpecAsync(RepositoryRecord repo)
    {
        string courseCode = _selectedCourse?.CourseCode
                            ?? _courses.FirstOrDefault(c => c.CourseId == repo.CourseId)?.CourseCode
                            ?? ("#" + repo.CourseId);
        CodingTask? task = null;
        if (repo.TaskId.HasValue)
        {
            task = _tasks.FirstOrDefault(t => t.TaskId == repo.TaskId.Value)
                   ?? await _session.Courses.GetTaskByIdAsync(repo.TaskId.Value);
        }

        if (task is not null)
        {
            LblInspSpecTitle.Text = task.Title;
            LblInspSpecCourse.Text = $"Course: {courseCode}";
            LblInspSpecTemplate.Text = $"Template: {task.StarterTemplate}";
            LblInspSpecMode.Text = task.AllowGroupWork
                ? $"Mode: Group Task - Max {task.MaxGroupSize}"
                : "Mode: Solo Task";
            LblInspSpecDeadline.Text = task.DeadlineUtc.HasValue
                ? $"Deadline: {task.DeadlineUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm}"
                : "Deadline: Open";
            TxtInspSpecInstructions.Text = string.IsNullOrWhiteSpace(task.Instructions)
                ? "No detailed problem instructions provided for this task."
                : task.Instructions;
        }
        else
        {
            LblInspSpecTitle.Text = repo.Title;
            LblInspSpecCourse.Text = $"Course: {courseCode}";
            LblInspSpecTemplate.Text = $"Language: {repo.PrimaryLanguage ?? "Git"}";
            LblInspSpecMode.Text = "Mode: Classroom Repository";
            LblInspSpecDeadline.Text = repo.DeadlineUtc.HasValue
                ? $"Deadline: {repo.DeadlineUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm}"
                : "Deadline: Open";
            TxtInspSpecInstructions.Text = string.IsNullOrWhiteSpace(repo.Description)
                ? "Classroom project repository."
                : repo.Description;
        }
    }

    private async Task LoadInspectorFilesAsync(RepositoryRecord repo)
    {
        TvInspectorFiles.Items.Clear();
        TxtInspectorCode.Text = string.Empty;
        LblInspectorFilePath.Text = "No file selected";

        try
        {
            List<RepoFileEntry> files = await _session.Browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
            var root = new TreeViewItem
            {
                Header = App.CreateExplorerNodeHeader(repo.RepoSlug, repo.RepoSlug, true),
                Tag = repo.RepoSlug,
                IsExpanded = true,
            };

            TreeViewItem? firstLeaf = null;
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
                        if (isLeaf && firstLeaf is null)
                        {
                            firstLeaf = existing;
                        }
                    }

                    current = existing;
                }
            }

            TvInspectorFiles.Items.Add(root);
            if (firstLeaf is not null)
            {
                firstLeaf.IsSelected = true;
            }
        }
        catch (Exception ex)
        {
            TxtInspectorCode.Text = "Could not load repository files: " + ex.Message;
        }
    }

    private async void TvInspectorFiles_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem item && item.Tag is RepoFileEntry file && _selectedRepo is not null)
        {
            User? actor = _session.GetCurrentUserEntity();
            if (actor is null)
            {
                return;
            }

            LblInspectorFileIcon.Text = App.GetFileIconGlyph(file.Path, false);
            LblInspectorFilePath.Text = file.Path;
            try
            {
                RepoBlobContent blob = await _session.Browser.ReadFileBlobAuthorizedAsync(
                    _session.Rbac, actor, _selectedRepo.RepoId, _selectedRepo.BareRepoDiskPath, file.Path);
                TxtInspectorCode.Text = blob.TextContent;
            }
            catch (Exception ex)
            {
                TxtInspectorCode.Text = "Could not read file blob: " + ex.Message;
            }
        }
    }

    private async Task LoadInspectorUnifiedCommitsAsync(RepositoryRecord repo)
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

        GridInspectorUnifiedCommits.ItemsSource = rows;
    }

    private async Task LoadInspectorTeamAndDiscussionsAsync(RepositoryRecord repo)
    {
        _inspectorTeam.Clear();
        _inspectorTeam.AddRange(await _session.Contributors.ListContributorViewsByRepoAsync(repo.RepoId));
        GridInspectorTeam.ItemsSource = null;
        GridInspectorTeam.ItemsSource = _inspectorTeam;
        if (_inspectorTeam.Count > 0 && GridInspectorTeam.SelectedIndex < 0)
        {
            GridInspectorTeam.SelectedIndex = 0;
        }

        UpdateInspectorTeamButtonsState();

        _comments.Clear();
        _comments.AddRange(await _session.Discussions.ListCommentViewsByRepoAsync(repo.RepoId, true));
        GridInspectorDiscussions.ItemsSource = _comments.Select(cm => new InspectorCommentRow(
            cm.CommentId,
            cm.AuthorFullName,
            cm.AuthorRole.ToString(),
            cm.CommentBody,
            cm.IsModeratedHidden ? "Hidden" : "Visible",
            cm.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))).ToList();

        if (_comments.Count > 0 && GridInspectorDiscussions.SelectedIndex < 0)
        {
            GridInspectorDiscussions.SelectedIndex = 0;
        }
    }

    private async Task LoadInspectorRubricAndLanguagesAsync(RepositoryRecord repo)
    {
        EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(repo.RepoId);
        if (eval is not null)
        {
            LblInspEvalTotalScore.Text = $"Total: {eval.TotalScore} / 100";
            LblInspScoreArch.Text = $"Arch: {eval.ArchitectureScore}/25";
            LblInspScoreFunc.Text = $"Func: {eval.FunctionalityScore}/25";
            LblInspScoreQual.Text = $"Qual: {eval.CodeQualityScore}/25";
            LblInspScoreDoc.Text = $"Doc: {eval.DocumentationScore}/25";
            TxtInspEvalNotes.Text = string.IsNullOrWhiteSpace(eval.CritiqueNotes)
                ? "No faculty critique notes provided."
                : eval.CritiqueNotes;
        }
        else
        {
            LblInspEvalTotalScore.Text = "Total: -- / 100";
            LblInspScoreArch.Text = "Arch: 0/25";
            LblInspScoreFunc.Text = "Func: 0/25";
            LblInspScoreQual.Text = "Qual: 0/25";
            LblInspScoreDoc.Text = "Doc: 0/25";
            TxtInspEvalNotes.Text = "Not graded yet. Click 'Grade Rubric' in the header to evaluate.";
        }

        List<RepositoryLanguage> langs = await _session.Repos.ListLanguagesAsync(repo.RepoId);
        GridInspectorLanguages.ItemsSource = langs.Select(l => new InspectorLangRow(
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
        InspectorExplorerBorder.Visibility = InspectorExplorerBorder.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void BtnToggleBottomPanel_Click(object sender, RoutedEventArgs e)
    {
        ToggleBottomPanel();
    }

    private void BtnInspPanelTabSpec_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(0);

    private void BtnInspPanelTabCommits_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(1);

    private void BtnInspPanelTabDiscussions_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(2);

    private void BtnInspPanelTabTeam_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(3);

    private void BtnInspPanelTabRubric_Click(object sender, RoutedEventArgs e) => SwitchBottomPanelTab(4);

    private async void BtnBackToSubmissions_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(1);
        await RefreshDataAsync();
    }

    private async void BtnInspectSelectedSubmission_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubmissions.SelectedIndex >= 0 && GridSubmissions.SelectedIndex < _submissions.Count)
        {
            await OpenInspectionViewAsync(_submissions[GridSubmissions.SelectedIndex]);
        }
    }

    private async void GridSubmissions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (GridSubmissions.SelectedIndex >= 0 && GridSubmissions.SelectedIndex < _submissions.Count)
        {
            await OpenInspectionViewAsync(_submissions[GridSubmissions.SelectedIndex]);
        }
    }

    private async void MenuInspectSubmission_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubmissions.SelectedIndex >= 0 && GridSubmissions.SelectedIndex < _submissions.Count)
        {
            await OpenInspectionViewAsync(_submissions[GridSubmissions.SelectedIndex]);
        }
    }

    private async void MenuGradeSubmission_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubmissions.SelectedIndex >= 0 && GridSubmissions.SelectedIndex < _submissions.Count)
        {
            await OpenGradeRubricForRepoAsync(_submissions[GridSubmissions.SelectedIndex]);
        }
    }

    private async void MenuLockUnlockSubmission_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubmissions.SelectedIndex < 0 || GridSubmissions.SelectedIndex >= _submissions.Count)
        {
            return;
        }

        RepositoryRecord target = _submissions[GridSubmissions.SelectedIndex];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        bool nextLock = !target.IsLocked;
        var (ok, message) = await _session.Repos.SetLockStateAsync(actor, target.RepoId, nextLock);
        if (!ok)
        {
            MessageBox.Show(message, "Lock / Unlock Repository", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await RefreshDataAsync();
        ReportStatus(nextLock
            ? $"Locked repository '{target.RepoSlug}'."
            : $"Unlocked repository '{target.RepoSlug}'.");
    }

    private async void MenuCopySubmissionClone_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubmissions.SelectedIndex < 0 || GridSubmissions.SelectedIndex >= _submissions.Count)
        {
            return;
        }

        RepositoryRecord target = _submissions[GridSubmissions.SelectedIndex];
        await CopyCloneCommandForRepoAsync(target.RepoSlug, false);
    }

    private void MenuMoveSubmissionToBin_Click(object sender, RoutedEventArgs e)
    {
        BtnMoveToRepoBin_Click(sender, e);
    }

    private async void GridCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || GridCourses.SelectedIndex < 0 || GridCourses.SelectedIndex >= _courses.Count)
        {
            return;
        }

        int idx = GridCourses.SelectedIndex;
        if (CboActiveCourse.SelectedIndex != idx)
        {
            CboActiveCourse.SelectedIndex = idx;
        }

        await LoadSelectedCourseAsync(_courses[idx]);
    }

    private async void CboActiveCourse_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || CboActiveCourse.SelectedIndex < 0 || CboActiveCourse.SelectedIndex >= _courses.Count)
        {
            return;
        }

        int idx = CboActiveCourse.SelectedIndex;
        if (GridCourses.SelectedIndex != idx)
        {
            GridCourses.SelectedIndex = idx;
        }

        await LoadSelectedCourseAsync(_courses[idx]);
    }

    private void GridTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateTicketActionButtonsState();
        UpdateFacultySelectedTicketPreview();
    }

    private void GridInspectorTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateInspectorTeamButtonsState();
    }

    private async void BtnRefreshCourse_Click(object sender, RoutedEventArgs e)
    {
        await RefreshDataAsync();
        ReportStatus("Faculty classroom view refreshed.");
    }

    private async void BtnNewCourse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new CourseEditorWindow(_session)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await LoadCoursesAsync();
            ReportStatus("Course created.");
        }
    }

    private async void BtnEditCourse_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCourse is null)
        {
            return;
        }

        var dlg = new CourseEditorWindow(_session, _selectedCourse)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await LoadCoursesAsync();
            ReportStatus("Course updated.");
        }
    }

    private async void BtnDeleteCourse_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCourse is null)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        MessageBoxResult confirm = MessageBox.Show(
            $"Delete course '{_selectedCourse.CourseCode} - {_selectedCourse.CourseTitle}'?\n\nCourses with existing repositories cannot be deleted.",
            "Delete Course",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var (ok, message) = await _session.Courses.DeleteCourseAsync(actor, _selectedCourse.CourseId);
        if (!ok)
        {
            MessageBox.Show(message, "Delete Course", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await LoadCoursesAsync();
        ReportStatus("Course deleted.");
    }

    private async void BtnRestoreTrashed_Click(object sender, RoutedEventArgs e)
    {
        if (GridTrashedRepos.SelectedIndex < 0 || GridTrashedRepos.SelectedIndex >= _trashed.Count)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        RepositoryRecord target = _trashed[GridTrashedRepos.SelectedIndex];
        var (ok, message) = await _session.Repos.RestoreFromRepoBinAsync(actor, target.RepoId);
        if (!ok)
        {
            MessageBox.Show(message, "Restore Repository", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await RefreshDataAsync();
        ReportStatus($"Restored '{target.RepoSlug}' from Repo Bin.");
    }

    private async void BtnPurgeTrashed_Click(object sender, RoutedEventArgs e)
    {
        if (GridTrashedRepos.SelectedIndex < 0 || GridTrashedRepos.SelectedIndex >= _trashed.Count)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        RepositoryRecord target = _trashed[GridTrashedRepos.SelectedIndex];
        MessageBoxResult confirm = MessageBox.Show(
            $"Permanently purge '{target.Title}' ({target.RepoSlug})?\n\nThis deletes the bare repository from disk and cannot be undone.",
            "Confirm Permanent Purge",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var (ok, message) = await _session.Repos.PurgeFromRepoBinAsync(actor, target.RepoId);
        if (!ok)
        {
            MessageBox.Show(message, "Purge Repository", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await RefreshDataAsync();
        ReportStatus($"Purged '{target.RepoSlug}' permanently.");
    }

    private async void BtnGradeRubric_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        await OpenGradeRubricForRepoAsync(_selectedRepo);
    }

    private async Task OpenGradeRubricForRepoAsync(RepositoryRecord repo)
    {
        try
        {
            string ownerText = repo.OwnerStudentId.HasValue &&
                               _userLookup.TryGetValue(repo.OwnerStudentId.Value, out string? name)
                ? name
                : "Student";

            string courseCode = _selectedCourse?.CourseCode
                                ?? _courses.FirstOrDefault(c => c.CourseId == repo.CourseId)?.CourseCode
                                ?? string.Empty;

            var dlg = new GradeRubricWindow(_session, repo, courseCode, ownerText)
            {
                Owner = Window.GetWindow(this),
            };

            if (dlg.ShowDialog() == true)
            {
                await RefreshDataAsync();
                ReportStatus($"Saved rubric evaluation for '{repo.RepoSlug}'.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not open Grade Rubric dialog: " + ex.Message,
                "Grade Rubric", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnLockUnlockRepo_Click(object sender, RoutedEventArgs e)
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

        bool nextLock = !_selectedRepo.IsLocked;
        var (ok, message) = await _session.Repos.SetLockStateAsync(actor, _selectedRepo.RepoId, nextLock);
        if (!ok)
        {
            MessageBox.Show(message, "Lock / Unlock Repo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        RepositoryRecord? fresh = await _session.Repos.GetByIdAsync(_selectedRepo.RepoId);
        if (fresh is not null)
        {
            await InspectSubmissionAsync(fresh);
        }

        ReportStatus(nextLock
            ? $"Locked repository '{_selectedRepo.RepoSlug}'."
            : $"Unlocked repository '{_selectedRepo.RepoSlug}'.");
    }

    private async void BtnEditRepo_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        var dlg = new RepositoryEditorWindow(_session, _selectedRepo)
        {
            Owner = Window.GetWindow(this),
        };

        dlg.ShowDialog();
        await RefreshDataAsync();
        ReportStatus($"Refreshed repository settings for '{_selectedRepo.RepoSlug}'.");
    }

    private async void BtnMoveToRepoBin_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubmissions.SelectedIndex < 0 || GridSubmissions.SelectedIndex >= _submissions.Count)
        {
            return;
        }

        RepositoryRecord target = _submissions[GridSubmissions.SelectedIndex];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        MessageBoxResult confirm = MessageBox.Show(
            $"Move '{target.Title}' ({target.RepoSlug}) to the Repo Bin?",
            "Move to Repo Bin",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var (ok, message) = await _session.Repos.MoveToRepoBinAsync(actor, target.RepoId);
        if (!ok)
        {
            MessageBox.Show(message, "Move to Repo Bin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ReportStatus($"Moved '{target.RepoSlug}' to Repo Bin.");
        await RefreshDataAsync();
    }

    private async void BtnCopyCloneUrl_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        await CopyCloneCommandForRepoAsync(_selectedRepo.RepoSlug, true);
    }

    private async Task CopyCloneCommandForRepoAsync(string repoSlug, bool animateHeaderButton)
    {
        if (_session.Server is not { IsRunning: true })
        {
            await _session.EnsureServerRunningAsync();
        }

        string url = _session.BuildCloneUrl(repoSlug);
        string cmd = $"git clone {url}";

        try
        {
            Clipboard.SetText(cmd);
            ReportStatus("Copied: " + cmd);

            if (animateHeaderButton)
            {
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
        }
        catch (Exception ex)
        {
            ReportStatus("Clipboard error: " + ex.Message);
        }
    }

    private async void BtnPostNewCodingTask_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCourse is null)
        {
            return;
        }

        var dlg = new CodingTaskWindow(_session, _selectedCourse.CourseId, _selectedCourse.CourseCode)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await RefreshDataAsync();
            ReportStatus($"Posted new coding task in {_selectedCourse.CourseCode}.");
        }
    }

    private async void BtnApproveTicket_Click(object sender, RoutedEventArgs e)
    {
        await ReviewSelectedTicketAsync(true);
    }

    private async void BtnRejectTicket_Click(object sender, RoutedEventArgs e)
    {
        await ReviewSelectedTicketAsync(false);
    }

    private async void BtnInspApproveTicket_Click(object sender, RoutedEventArgs e)
    {
        await ReviewInspectorSelectedTicketAsync(true);
    }

    private async void BtnInspRejectTicket_Click(object sender, RoutedEventArgs e)
    {
        await ReviewInspectorSelectedTicketAsync(false);
    }

    private async Task ReviewSelectedTicketAsync(bool approve)
    {
        if (GridTickets.SelectedIndex < 0 || GridTickets.SelectedIndex >= _tickets.Count)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        ContributorViewDto ticket = _tickets[GridTickets.SelectedIndex];
        var (ok, message) = await _session.Contributors.ReviewGroupmateTicketAsync(
            actor, ticket.RepoId, ticket.StudentUserId, approve);

        if (!ok)
        {
            MessageBox.Show(message, "Groupmate Tickets", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ReportStatus(approve
            ? $"Approved groupmate ticket for {ticket.FullName}."
            : $"Rejected groupmate ticket for {ticket.FullName}.");
        await RefreshDataAsync();
    }

    private async Task ReviewInspectorSelectedTicketAsync(bool approve)
    {
        if (GridInspectorTeam.SelectedIndex < 0 || GridInspectorTeam.SelectedIndex >= _inspectorTeam.Count)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        ContributorViewDto ticket = _inspectorTeam[GridInspectorTeam.SelectedIndex];
        var (ok, message) = await _session.Contributors.ReviewGroupmateTicketAsync(
            actor, ticket.RepoId, ticket.StudentUserId, approve);

        if (!ok)
        {
            MessageBox.Show(message, "Team & Access", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await RefreshDataAsync();
        ReportStatus(approve
            ? $"Approved groupmate ticket for {ticket.FullName}."
            : $"Rejected groupmate ticket for {ticket.FullName}.");
    }

    private void BtnReplyFacultyComment_Click(object sender, RoutedEventArgs e)
    {
        if (GridInspectorDiscussions.SelectedIndex >= 0 && GridInspectorDiscussions.SelectedIndex < _comments.Count)
        {
            DiscussionCommentViewDto target = _comments[GridInspectorDiscussions.SelectedIndex];
            string prefix = $"@{target.AuthorUsername}: ";
            if (!TxtFacultyComment.Text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                TxtFacultyComment.Text = prefix + TxtFacultyComment.Text;
            }
        }
        else if (!TxtFacultyComment.Text.StartsWith("@", StringComparison.Ordinal))
        {
            TxtFacultyComment.Text = "@student: " + TxtFacultyComment.Text;
        }

        TxtFacultyComment.Focus();
        TxtFacultyComment.CaretIndex = TxtFacultyComment.Text.Length;
    }

    private async void BtnPostFacultyComment_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        string body = TxtFacultyComment.Text.Trim();
        if (body.Length == 0)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        BtnPostFacultyComment.IsEnabled = false;
        try
        {
            var (ok, message, _) = await _session.Discussions.AddCommentAsync(actor, _selectedRepo.RepoId, body);
            if (!ok)
            {
                MessageBox.Show(message, "Discussion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TxtFacultyComment.Clear();
            await LoadInspectorTeamAndDiscussionsAsync(_selectedRepo);
            ReportStatus("Posted faculty feedback comment.");
        }
        finally
        {
            BtnPostFacultyComment.IsEnabled = true;
        }
    }

    private async void BtnToggleHideComment_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null || GridInspectorDiscussions.SelectedIndex < 0 ||
            GridInspectorDiscussions.SelectedIndex >= _comments.Count)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        DiscussionCommentViewDto target = _comments[GridInspectorDiscussions.SelectedIndex];
        bool nextHidden = !target.IsModeratedHidden;
        var (ok, message) = await _session.Discussions.SetModerationHiddenAsync(actor, target.CommentId, nextHidden);
        if (!ok)
        {
            MessageBox.Show(message, "Moderate Comment", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await LoadInspectorTeamAndDiscussionsAsync(_selectedRepo);
        ReportStatus(nextHidden ? "Comment hidden by moderation." : "Comment restored to visible.");
    }

    private void ReportStatus(string text)
    {
        _setStatus?.Invoke(text);
    }

    private sealed record SubmissionGridRow(
        int RepoId, string RepoSlug, string Title, string OwnerText, string SubmittedText,
        string DeadlineStatus, string TotalScoreText, string LockedText);

    private sealed record TaskGridRow(
        int TaskId, string Title, string StarterTemplate, string DeadlineText, string ModeText);

    private sealed record TicketGridRow(
        int RepoId, int StudentUserId, string RepoSlug, string RepoTitle, string CourseCode,
        string OwnerDisplay, string StudentDisplay, string TeamRole,
        string AccessLevel, string ApprovalStatus, string AssignedAtText);

    private sealed record UnifiedCommitRow(
        string ShortSha, string CommitMessage, string AuthorName, string TimestampText, string TimelinessText);

    private sealed record InspectorLangRow(
        string LanguageName, int FileCount, int SourceLines, int CommentLines, string ShareText);

    private sealed record InspectorCommentRow(
        int CommentId, string AuthorFullName, string AuthorRole, string CommentBody, string VisibilityText, string CreatedAtText);
}
