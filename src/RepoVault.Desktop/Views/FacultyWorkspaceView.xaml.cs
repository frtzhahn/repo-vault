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
    private readonly List<RepositoryRecord> _trashed = new();
    private readonly List<DiscussionCommentViewDto> _comments = new();
    private readonly Dictionary<int, string> _userLookup = new();

    private Course? _selectedCourse;
    private RepositoryRecord? _selectedRepo;
    private bool _isLoading;
    private int _activePageIndex;
    private int _activeInspSubViewIndex;

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
        HighlightInspSubViewButton(0);
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        await LoadCoursesAsync();
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
            0 => "1. Courses & Tasks",
            1 => "2. Student Submissions",
            2 => "3. Groupmate Tickets",
            3 => "4. Repo Bin (Trash)",
            _ => "1. Courses & Tasks",
        };

        _updateNavHighlight?.Invoke(_activePageIndex);
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

            var repoSlugById = _submissions.ToDictionary(r => r.RepoId, r => r.RepoSlug);
            var subRows = new List<SubmissionGridRow>();
            int selectRepoIdx = -1;

            for (int i = 0; i < _submissions.Count; i++)
            {
                RepositoryRecord r = _submissions[i];
                string ownerText = r.OwnerStudentId.HasValue && _userLookup.TryGetValue(r.OwnerStudentId.Value, out string? name)
                    ? name
                    : (r.OwnerStudentId.HasValue ? "Student #" + r.OwnerStudentId.Value : "Course");

                string submittedText = r.IsSubmitted
                    ? (r.SubmittedAt.HasValue ? r.SubmittedAt.Value.ToLocalTime().ToString("MM/dd HH:mm") : "Yes")
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
                t.DeadlineUtc.HasValue ? t.DeadlineUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "No Deadline",
                t.AllowGroupWork ? "Group" : "Solo",
                t.MaxGroupSize)).ToList();

            _tickets.Clear();
            _tickets.AddRange(await _session.Contributors.ListTicketsByCourseAsync(course.CourseId));
            GridTickets.ItemsSource = _tickets.Select(tk => new TicketGridRow(
                tk.RepoId,
                tk.StudentUserId,
                repoSlugById.TryGetValue(tk.RepoId, out string? slug) ? slug : "#" + tk.RepoId,
                $"{tk.FullName} ({tk.Username})",
                tk.TeamRole,
                tk.AccessLevel.ToString(),
                tk.ApprovalStatus.ToString())).ToList();

            if (_tickets.Count > 0 && GridTickets.SelectedIndex < 0)
            {
                GridTickets.SelectedIndex = 0;
            }

            UpdateTicketActionButtonsState();

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
        SwitchInspSubView(0);
        await InspectSubmissionAsync(repo);
    }

    private void SwitchInspSubView(int subViewIdx)
    {
        _activeInspSubViewIndex = Math.Clamp(subViewIdx, 0, 2);
        InspSubCodeGrid.Visibility = _activeInspSubViewIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        InspSubCommitsGrid.Visibility = _activeInspSubViewIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        InspSubDiscussionsBorder.Visibility = _activeInspSubViewIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        HighlightInspSubViewButton(_activeInspSubViewIndex);
    }

    private void HighlightInspSubViewButton(int activeIdx)
    {
        Button[] buttons = [BtnInspSubCode, BtnInspSubCommits, BtnInspSubDiscussions];
        for (int i = 0; i < buttons.Length; i++)
        {
            bool isSelected = i == activeIdx;
            buttons[i].BorderBrush = isSelected
                ? (Brush)FindResource("AccentPrimaryBrush")
                : (Brush)FindResource("BorderSubtleBrush");
            buttons[i].FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Normal;
        }
    }

    private async Task InspectSubmissionAsync(RepositoryRecord repo)
    {
        _selectedRepo = repo;
        string ownerText = repo.OwnerStudentId.HasValue && _userLookup.TryGetValue(repo.OwnerStudentId.Value, out string? name)
            ? name
            : "Student";

        LblInspectorHeader.Text = $"Inspecting: {repo.RepoSlug} by {ownerText}";
        LblInspLockBadge.Text = repo.IsLocked ? "LOCKED" : "OPEN";
        InspLockBadgeBorder.BorderBrush = repo.IsLocked
            ? (Brush)FindResource("AccentWarningBrush")
            : (Brush)FindResource("AccentSuccessBrush");
        BtnLockUnlockRepo.Content = repo.IsLocked ? "Unlock Repo" : "Lock Repo";

        string ip = LanHelper.GetPreferredLanIpv4Address();
        string cloneUrl = LanHelper.BuildCloneUrl(repo.RepoSlug, _session.ServerPort, ip);
        TxtInspectorCloneUrl.Text = cloneUrl;

        LblInspFiles.Text = $"Total Files: {repo.TotalFiles}";
        LblInspSloc.Text = $"Source LOC: {repo.SourceLines:N0}";
        LblInspComments.Text = $"Comment LOC: {repo.CommentLines:N0}";
        LblInspBlanks.Text = $"Blank LOC: {repo.BlankLines:N0}";
        LblInspPrimaryLang.Text = $"Primary Language: {repo.PrimaryLanguage ?? "None"}";

        await LoadInspectorFilesAsync(repo);
        await LoadInspectorCommitsAndPushesAsync(repo);
        await LoadInspectorLanguagesAsync(repo);
        await LoadInspectorDiscussionsAsync(repo);
    }

    private async Task LoadInspectorFilesAsync(RepositoryRecord repo)
    {
        TvInspectorFiles.Items.Clear();
        TxtInspectorCode.Text = string.Empty;
        LblInspectorFilePath.Text = "No file selected";
        LblInspectorFileSize.Text = string.Empty;

        try
        {
            List<RepoFileEntry> files = await _session.Browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
            var root = new TreeViewItem
            {
                Header = repo.RepoSlug,
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

            LblInspectorFilePath.Text = file.Path;
            LblInspectorFileSize.Text = $"{file.SizeBytes:N0} bytes";
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

    private async Task LoadInspectorCommitsAndPushesAsync(RepositoryRecord repo)
    {
        List<CommitLogEntry> commits = await _session.Repos.ListCommitLogsAsync(repo.RepoId);
        GridInspectorCommits.ItemsSource = commits.Select(c => new InspectorCommitRow(
            c.CommitHash.Length >= 8 ? c.CommitHash[..8] : c.CommitHash,
            c.CommitMessage,
            c.AuthorName,
            c.CommittedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))).ToList();

        List<PushLedgerViewDto> pushes = await _session.Repos.ListPushLedgerViewAsync(repo.RepoId);
        GridInspectorPushes.ItemsSource = pushes.Select(p => new InspectorPushRow(
            p.PushId,
            p.BranchRef,
            p.NewCommitHash.Length >= 8 ? p.NewCommitHash[..8] : p.NewCommitHash,
            p.PusherFullName,
            p.PushedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))).ToList();
    }

    private async Task LoadInspectorLanguagesAsync(RepositoryRecord repo)
    {
        List<RepositoryLanguage> langs = await _session.Repos.ListLanguagesAsync(repo.RepoId);
        GridInspectorLanguages.ItemsSource = langs.Select(l => new InspectorLangRow(
            l.LanguageName,
            l.FileCount,
            l.SourceLines,
            l.CommentLines,
            l.BlankLines,
            $"{l.PercentageShare:F1}%")).ToList();
    }

    private async Task LoadInspectorDiscussionsAsync(RepositoryRecord repo)
    {
        _comments.Clear();
        _comments.AddRange(await _session.Discussions.ListCommentViewsByRepoAsync(repo.RepoId, true));
        GridInspectorDiscussions.ItemsSource = _comments.Select(cm => new InspectorCommentRow(
            cm.CommentId,
            cm.AuthorFullName,
            cm.AuthorRole.ToString(),
            cm.CommentBody,
            cm.IsModeratedHidden ? "Hidden" : "Visible",
            cm.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))).ToList();
    }

    private void BtnToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _toggleSidebar?.Invoke();
    }

    private async void BtnBackToSubmissions_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(1);
        await RefreshDataAsync();
    }

    private void BtnInspSubCode_Click(object sender, RoutedEventArgs e) => SwitchInspSubView(0);

    private void BtnInspSubCommits_Click(object sender, RoutedEventArgs e) => SwitchInspSubView(1);

    private void BtnInspSubDiscussions_Click(object sender, RoutedEventArgs e) => SwitchInspSubView(2);

    private async void BtnInspectSubmissionRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int repoId })
        {
            RepositoryRecord? target = _submissions.FirstOrDefault(r => r.RepoId == repoId);
            if (target is not null)
            {
                await OpenInspectionViewAsync(target);
            }
        }
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
            MessageBox.Show("Select a student submission to grade.", "Grade Rubric",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string ownerText = _selectedRepo.OwnerStudentId.HasValue &&
                           _userLookup.TryGetValue(_selectedRepo.OwnerStudentId.Value, out string? name)
            ? name
            : "Student";

        var dlg = new GradeRubricWindow(
            _session, _selectedRepo, _selectedCourse?.CourseCode ?? "", ownerText)
        {
            Owner = Window.GetWindow(this),
        };

        if (dlg.ShowDialog() == true)
        {
            await RefreshDataAsync();
            ReportStatus($"Saved rubric evaluation for '{_selectedRepo.RepoSlug}'.");
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

    private void BtnCopyCloneUrl_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        string ip = LanHelper.GetPreferredLanIpv4Address();
        string url = LanHelper.BuildCloneUrl(_selectedRepo.RepoSlug, _session.ServerPort, ip);
        try
        {
            Clipboard.SetText(url);
            ReportStatus("Copied read-only clone URL to clipboard: " + url);
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

    private async Task ReviewSelectedTicketAsync(bool approve)
    {
        if (GridTickets.SelectedIndex < 0 || GridTickets.SelectedIndex >= _tickets.Count)
        {
            MessageBox.Show("Select a groupmate ticket from the table first.",
                "Groupmate Tickets", MessageBoxButton.OK, MessageBoxImage.Information);
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
            await LoadInspectorDiscussionsAsync(_selectedRepo);
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

        await LoadInspectorDiscussionsAsync(_selectedRepo);
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
        int TaskId, string Title, string StarterTemplate, string DeadlineText, string ModeText, int MaxGroupSize);

    private sealed record TicketGridRow(
        int RepoId, int StudentUserId, string RepoSlug, string StudentDisplay,
        string TeamRole, string AccessLevel, string ApprovalStatus);

    private sealed record InspectorCommitRow(
        string ShortSha, string CommitMessage, string AuthorName, string CommittedAtText);

    private sealed record InspectorPushRow(
        int PushId, string BranchRef, string NewShortSha, string PusherFullName, string PushedAtText);

    private sealed record InspectorLangRow(
        string LanguageName, int FileCount, int SourceLines, int CommentLines, int BlankLines, string ShareText);

    private sealed record InspectorCommentRow(
        int CommentId, string AuthorFullName, string AuthorRole, string CommentBody, string VisibilityText, string CreatedAtText);
}
