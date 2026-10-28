using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class StudentWorkspaceView : UserControl
{
    private readonly AppSession _session;
    private readonly Action<string>? _setStatus;
    private readonly List<Course> _courses = new();
    private readonly List<RepositoryRecord> _repos = new();
    private readonly List<CodingTask> _tasks = new();
    private RepositoryRecord? _selectedRepo;
    private bool _isLoading;

    public StudentWorkspaceView() : this(new AppSession(), null)
    {
    }

    public StudentWorkspaceView(AppSession session, Action<string>? setStatus = null)
    {
        _session = session;
        _setStatus = setStatus;
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        await LoadCoursesAndDataAsync();
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
                repoRows.Add(new StudentRepoRow(r.RepoId, r.RepoSlug, code, status, deadline));
                if (r.RepoId == prevRepoId)
                {
                    selectIdx = i;
                }
            }

            GridMyRepos.ItemsSource = repoRows;

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
                string typeBadge = t.AllowGroupWork ? $"GROUP ({t.MaxGroupSize})" : "SOLO";
                string dl = t.DeadlineUtc.HasValue
                    ? t.DeadlineUtc.Value.ToLocalTime().ToString("MM/dd HH:mm")
                    : "None";
                taskRows.Add(new StudentTaskRow(t.TaskId, t.Title, typeBadge, dl));
            }

            GridAssignedTasks.ItemsSource = taskRows;

            if (_repos.Count > 0)
            {
                int target = selectIdx >= 0 ? selectIdx : 0;
                GridMyRepos.SelectedIndex = target;
                await LoadSelectedRepoAsync(_repos[target]);
            }
            else
            {
                _selectedRepo = null;
                ClearRepoDetails();
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

    private void ClearRepoDetails()
    {
        LblRepoTitle.Text = "No Repository Selected";
        LblRepoSlugBadge.Text = "--";
        LblCourseBadge.Text = "--";
        LblDeadlineBadge.Text = "--";
        TxtCloneCommand.Text = string.Empty;
        BtnTurnInToggle.IsEnabled = false;
        BtnRequestGroupmateTicket.IsEnabled = false;
        BtnGrantPeerAccess.IsEnabled = false;

        TvFiles.Items.Clear();
        TxtCodeViewer.Text = string.Empty;
        LblSelectedFilePath.Text = "No file selected";
        LblSelectedFileSize.Text = string.Empty;

        GridCommits.ItemsSource = null;
        GridPushes.ItemsSource = null;
        GridContributors.ItemsSource = null;
        GridComments.ItemsSource = null;
    }

    private async Task LoadSelectedRepoAsync(RepositoryRecord repo)
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
        DeadlineBadgeBorder.Background = dlBadge == "LATE"
            ? (Brush)FindResource("AccentDangerBrush")
            : (Brush)FindResource("AccentSuccessBrush");

        string ip = LanHelper.GetPreferredLanIpv4Address();
        string cloneUrl = LanHelper.BuildCloneUrl(repo.RepoSlug, _session.ServerPort, ip);
        TxtCloneCommand.Text = $"git clone {cloneUrl}";

        bool canSubmit = !repo.IsLocked && (!repo.DeadlineUtc.HasValue || repo.DeadlineUtc.Value >= DateTime.UtcNow);
        BtnTurnInToggle.IsEnabled = canSubmit;
        BtnTurnInToggle.Content = repo.IsSubmitted ? "Unsubmit" : "Turn In Task";

        bool isOwner = repo.OwnerStudentId == actor.UserId;
        BtnRequestGroupmateTicket.IsEnabled = isOwner && !repo.IsLocked;
        BtnGrantPeerAccess.IsEnabled = isOwner && !repo.IsLocked;

        double ratio = repo.SourceLines > 0 ? (double)repo.CommentLines / repo.SourceLines * 100.0 : 0.0;
        LblMetricFiles.Text = $"Total Files: {repo.TotalFiles}";
        LblMetricSourceLoc.Text = $"Source LOC: {repo.SourceLines:N0}";
        LblMetricCommentLoc.Text = $"Comment LOC: {repo.CommentLines:N0}";
        LblMetricBlankLoc.Text = $"Blank LOC: {repo.BlankLines:N0}";
        LblMetricRatio.Text = repo.SourceLines > 0 ? $"Comment Ratio: {ratio:F1}%" : "Comment Ratio: N/A";

        await LoadFileTreeAsync(repo);
        await LoadCommitsAndPushesAsync(repo);
        await LoadTeamAndCommentsAsync(repo);
        await LoadEvaluationAsync(repo);
    }

    private async Task LoadFileTreeAsync(RepositoryRecord repo)
    {
        TvFiles.Items.Clear();
        TxtCodeViewer.Text = string.Empty;
        LblSelectedFilePath.Text = "No file selected";
        LblSelectedFileSize.Text = string.Empty;

        try
        {
            List<RepoFileEntry> files = await _session.Browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
            var root = new TreeViewItem
            {
                Header = repo.RepoSlug,
                IsExpanded = true
            };

            TreeViewItem? firstFileNode = null;
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
                            IsExpanded = true
                        };
                        current.Items.Add(existing);
                        if (isLeaf && firstFileNode is null)
                        {
                            firstFileNode = existing;
                        }
                    }

                    current = existing;
                }
            }

            TvFiles.Items.Add(root);
            if (firstFileNode is not null)
            {
                firstFileNode.IsSelected = true;
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

            LblSelectedFilePath.Text = file.Path;
            LblSelectedFileSize.Text = $"{file.SizeBytes:N0} bytes";
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

    private async Task LoadTeamAndCommentsAsync(RepositoryRecord repo)
    {
        List<ContributorViewDto> contributors = await _session.Contributors.ListContributorViewsByRepoAsync(repo.RepoId);
        GridContributors.ItemsSource = contributors;

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

    private async void GridMyRepos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || GridMyRepos.SelectedIndex < 0 || GridMyRepos.SelectedIndex >= _repos.Count)
        {
            return;
        }

        await LoadSelectedRepoAsync(_repos[GridMyRepos.SelectedIndex]);
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
            MessageBox.Show("Select a coding task from the Assigned Tasks list first.",
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

            _selectedRepo = repo;
            TabSidebar.SelectedItem = TabMyRepos;
            await RefreshDataAsync();
            ReportStatus($"Initialized coding task repository '{repo.RepoSlug}'.");
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

            ReportStatus(nextState ? $"Turned in '{_selectedRepo.RepoSlug}'." : $"Reverted '{_selectedRepo.RepoSlug}' to draft.");
            await RefreshDataAsync();
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

    private async void BtnRequestGroupmateTicket_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        var dlg = new RequestTicketWindow(_session, _selectedRepo.RepoId)
        {
            Owner = Window.GetWindow(this)
        };

        if (dlg.ShowDialog() == true)
        {
            await LoadTeamAndCommentsAsync(_selectedRepo);
            ReportStatus("Groupmate collaboration ticket submitted for faculty approval.");
        }
    }

    private async void BtnGrantPeerAccess_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        var dlg = new GrantPeerWindow(_session, _selectedRepo.RepoId)
        {
            Owner = Window.GetWindow(this)
        };

        if (dlg.ShowDialog() == true)
        {
            await LoadTeamAndCommentsAsync(_selectedRepo);
            ReportStatus("Peer viewer access granted.");
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
        int RepoId, string RepoSlug, string CourseCode, string StatusBadge, string DeadlineBadge);

    private sealed record StudentTaskRow(
        int TaskId, string Title, string TypeBadge, string DeadlineText);

    private sealed record CommitGridRow(
        string ShortSha, string CommitMessage, string AuthorName, string CommittedAtText);

    private sealed record PushGridRow(
        int PushId, string BranchRef, string OldShortSha, string NewShortSha, string PusherFullName, string PushedAtText);

    private sealed record CommentGridRow(
        int CommentId, string AuthorDisplay, string CommentBody, string CreatedAtText);
}
