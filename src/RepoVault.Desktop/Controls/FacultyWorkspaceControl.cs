using System.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;
using RepoVault.Desktop.Forms;

namespace RepoVault.Desktop.Controls;

public partial class FacultyWorkspaceControl : UserControl
{
    private readonly AppSession _session;
    private readonly List<Course> _courses = new();
    private readonly List<RepositoryRecord> _submissions = new();
    private readonly List<CodingTask> _tasks = new();
    private readonly List<ContributorViewDto> _tickets = new();
    private readonly List<DiscussionCommentViewDto> _comments = new();
    private Course? _selectedCourse;
    private RepositoryRecord? _selectedRepo;
    private bool _isLoading;

    public FacultyWorkspaceControl(AppSession session)
    {
        _session = session;
        InitializeComponent();
        ConfigureGrids();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadCoursesAsync();
    }

    private void ConfigureGrids()
    {
        // courses grid
        dgvCourses.Columns.Clear();
        dgvCourses.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCourseId", Visible = false });
        dgvCourses.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCourseCode", HeaderText = "Course", Width = 75 });
        dgvCourses.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCourseTitle", HeaderText = "Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvCourses.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSemester", HeaderText = "Semester", Width = 110 });

        // submissions grid
        dgvSubmissions.Columns.Clear();
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRepoId", Visible = false });
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSlug", HeaderText = "Slug", Width = 120 });
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Repository Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOwner", HeaderText = "Owner", Width = 110 });
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSubmitted", HeaderText = "Turned In", Width = 80 });
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDeadline", HeaderText = "Deadline", Width = 95 });
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colScore", HeaderText = "Grade", Width = 75 });
        dgvSubmissions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLocked", HeaderText = "Locked", Width = 55 });

        // tasks grid
        dgvTasks.Columns.Clear();
        dgvTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskId", Visible = false });
        dgvTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskTitle", HeaderText = "Task Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskTemplate", HeaderText = "Template", Width = 75 });
        dgvTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskDeadline", HeaderText = "Deadline", Width = 120 });
        dgvTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskCollab", HeaderText = "Type", Width = 80 });
        dgvTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskMax", HeaderText = "Max Group", Width = 75 });

        // tickets grid
        dgvTickets.Columns.Clear();
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketRepoId", Visible = false });
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketUserId", Visible = false });
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketStudent", HeaderText = "Student", Width = 120 });
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketUsername", HeaderText = "Username", Width = 95 });
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketRole", HeaderText = "Team Role", Width = 90 });
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketLevel", HeaderText = "Access Level", Width = 110 });
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketStatus", HeaderText = "Status", Width = 85 });
        dgvTickets.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTicketDate", HeaderText = "Assigned At", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        // commits grid
        dgvCommits.Columns.Clear();
        dgvCommits.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCommitSha",
            HeaderText = "Commit",
            Width = 80,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 9F) }
        });
        dgvCommits.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommitMsg", HeaderText = "Message", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvCommits.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommitAuthor", HeaderText = "Author", Width = 110 });
        dgvCommits.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommitDate", HeaderText = "Date", Width = 120 });

        // push ledger grid
        dgvPushes.Columns.Clear();
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushId", HeaderText = "Push #", Width = 50 });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushBranch", HeaderText = "Branch", Width = 75 });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPushOldSha",
            HeaderText = "Old SHA",
            Width = 75,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 9F) }
        });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPushNewSha",
            HeaderText = "New SHA",
            Width = 75,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 9F) }
        });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushUser", HeaderText = "Pushed By", Width = 110 });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushDate", HeaderText = "Timestamp", Width = 120 });

        // discussions grid
        dgvDiscussions.Columns.Clear();
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommentId", Visible = false });
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAuthor", HeaderText = "Author", Width = 120 });
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRole", HeaderText = "Role", Width = 75 });
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBody", HeaderText = "Comment", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colModerated", HeaderText = "Hidden", Width = 65 });
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDate", HeaderText = "Posted At", Width = 130 });
    }

    public async Task LoadCoursesAsync()
    {
        try
        {
            List<Course> list = await _session.Courses.ListCoursesAsync();
            _courses.Clear();
            _courses.AddRange(list);

            dgvCourses.Rows.Clear();
            foreach (Course c in _courses)
            {
                dgvCourses.Rows.Add(c.CourseId, c.CourseCode, c.CourseTitle, c.Semester);
            }

            if (dgvCourses.Rows.Count > 0)
            {
                dgvCourses.Rows[0].Selected = true;
                await OnCourseSelectedAsync(_courses[0]);
            }
            else
            {
                _selectedCourse = null;
                ClearCourseUi();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load courses: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public async Task RefreshDataAsync()
    {
        if (_selectedCourse is not null)
        {
            await OnCourseSelectedAsync(_selectedCourse);
        }
        else
        {
            await LoadCoursesAsync();
        }
    }

    private void ClearCourseUi()
    {
        dgvSubmissions.Rows.Clear();
        dgvTasks.Rows.Clear();
        dgvTickets.Rows.Clear();
        ClearInspectUi();
    }

    private void ClearInspectUi()
    {
        tvFiles.Nodes.Clear();
        rtbCode.Clear();
        lblFilePath.Text = "No file selected";
        lblFileSize.Text = string.Empty;
        dgvCommits.Rows.Clear();
        dgvPushes.Rows.Clear();
        dgvDiscussions.Rows.Clear();
    }

    private async Task OnCourseSelectedAsync(Course course)
    {
        _selectedCourse = course;
        _isLoading = true;

        try
        {
            // load submissions for course
            _submissions.Clear();
            List<RepositoryRecord> list = await _session.Repos.ListActiveByCourseAsync(course.CourseId);
            _submissions.AddRange(list);

            dgvSubmissions.Rows.Clear();
            foreach (RepositoryRecord r in _submissions)
            {
                string turnedIn = r.IsSubmitted
                    ? (r.SubmittedAt.HasValue ? r.SubmittedAt.Value.ToLocalTime().ToString("MM/dd HH:mm") : "Yes")
                    : "Draft";

                string deadlineStatus = "No Deadline";
                if (r.DeadlineUtc.HasValue)
                {
                    if (r.IsSubmitted && r.SubmittedAt.HasValue)
                    {
                        deadlineStatus = r.SubmittedAt.Value <= r.DeadlineUtc.Value ? "ON TIME" : "LATE";
                    }
                    else
                    {
                        deadlineStatus = DateTime.UtcNow > r.DeadlineUtc.Value ? "OVERDUE" : "PENDING";
                    }
                }

                // fetch evaluation score if present
                string scoreText = "-- / 100";
                EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(r.RepoId);
                if (eval is not null)
                {
                    scoreText = $"{eval.TotalScore} / 100";
                }

                string ownerText = r.OwnerStudentId.HasValue ? $"Student #{r.OwnerStudentId.Value}" : "General";
                string lockedText = r.IsLocked ? "Yes" : "No";

                dgvSubmissions.Rows.Add(
                    r.RepoId, r.RepoSlug, r.Title, ownerText, turnedIn,
                    deadlineStatus, scoreText, lockedText);
            }

            // load coding tasks for course
            _tasks.Clear();
            List<CodingTask> tasks = await _session.Courses.ListTasksByCourseAsync(course.CourseId);
            _tasks.AddRange(tasks);

            dgvTasks.Rows.Clear();
            foreach (CodingTask t in _tasks)
            {
                string deadline = t.DeadlineUtc.HasValue
                    ? t.DeadlineUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    : "None";
                string collab = t.AllowGroupWork ? "Group" : "Solo";
                dgvTasks.Rows.Add(t.TaskId, t.Title, t.StarterTemplate, deadline, collab, t.MaxGroupSize);
            }

            // load tickets for course
            _tickets.Clear();
            List<ContributorViewDto> tickets = await _session.Contributors.ListTicketsByCourseAsync(course.CourseId);
            _tickets.AddRange(tickets);

            dgvTickets.Rows.Clear();
            foreach (ContributorViewDto tk in _tickets)
            {
                dgvTickets.Rows.Add(
                    tk.RepoId, tk.StudentUserId, tk.FullName, tk.Username,
                    tk.TeamRole, tk.AccessLevel.ToString(), tk.ApprovalStatus.ToString(),
                    tk.AssignedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            }

            if (dgvSubmissions.Rows.Count > 0)
            {
                dgvSubmissions.Rows[0].Selected = true;
                await OnSubmissionSelectedAsync(_submissions[0]);
            }
            else
            {
                _selectedRepo = null;
                ClearInspectUi();
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OnSubmissionSelectedAsync(RepositoryRecord repo)
    {
        _selectedRepo = repo;
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        await Task.WhenAll(
            LoadSourceFilesAsync(repo, actor),
            LoadCommitsAndPushesAsync(repo),
            LoadDiscussionsAsync(repo));
    }

    private async Task LoadSourceFilesAsync(RepositoryRecord repo, User actor)
    {
        tvFiles.Nodes.Clear();
        rtbCode.Clear();
        lblFilePath.Text = "No file selected";
        lblFileSize.Text = string.Empty;

        try
        {
            List<RepoFileEntry> files = await _session.Browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
            TreeNode root = new TreeNode(repo.RepoSlug) { Tag = null };

            foreach (RepoFileEntry f in files)
            {
                string[] parts = f.Path.Split('/');
                TreeNode current = root;
                for (int i = 0; i < parts.Length; i++)
                {
                    bool isFile = (i == parts.Length - 1);
                    string part = parts[i];
                    TreeNode? existing = current.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == part);
                    if (existing is null)
                    {
                        existing = new TreeNode(part) { Tag = isFile ? f : null };
                        current.Nodes.Add(existing);
                    }
                    current = existing;
                }
            }

            tvFiles.Nodes.Add(root);
            root.Expand();

            TreeNode? firstFile = FindFirstFileNode(root);
            if (firstFile is not null)
            {
                tvFiles.SelectedNode = firstFile;
            }
        }
        catch (Exception ex)
        {
            rtbCode.Text = "could not inspect repository files: " + ex.Message;
        }
    }

    private static TreeNode? FindFirstFileNode(TreeNode parent)
    {
        foreach (TreeNode node in parent.Nodes)
        {
            if (node.Tag is RepoFileEntry)
            {
                return node;
            }

            TreeNode? deeper = FindFirstFileNode(node);
            if (deeper is not null)
            {
                return deeper;
            }
        }

        return null;
    }

    private async void tvFiles_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is RepoFileEntry file && _selectedRepo is not null)
        {
            User? actor = _session.GetCurrentUserEntity();
            if (actor is null)
            {
                return;
            }

            lblFilePath.Text = file.Path;
            lblFileSize.Text = $"{file.SizeBytes:N0} bytes";
            try
            {
                RepoBlobContent blob = await _session.Browser.ReadFileBlobAuthorizedAsync(
                    _session.Rbac, actor, _selectedRepo.RepoId, _selectedRepo.BareRepoDiskPath, file.Path);
                rtbCode.Text = blob.TextContent;
            }
            catch (Exception ex)
            {
                rtbCode.Text = "could not read blob: " + ex.Message;
            }
        }
        else
        {
            lblFilePath.Text = e.Node?.Text ?? string.Empty;
            lblFileSize.Text = string.Empty;
            rtbCode.Text = string.Empty;
        }
    }

    private async Task LoadCommitsAndPushesAsync(RepositoryRecord repo)
    {
        dgvCommits.Rows.Clear();
        dgvPushes.Rows.Clear();

        try
        {
            List<CommitLogEntry> commits = await _session.Repos.ListCommitLogsAsync(repo.RepoId);
            foreach (CommitLogEntry c in commits)
            {
                string sha = c.CommitHash.Length >= 8 ? c.CommitHash[..8] : c.CommitHash;
                dgvCommits.Rows.Add(
                    sha, c.CommitMessage, c.AuthorName,
                    c.CommittedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            }

            List<PushLedgerViewDto> pushes = await _session.Repos.ListPushLedgerViewAsync(repo.RepoId);
            foreach (PushLedgerViewDto p in pushes)
            {
                string oldSha = p.OldCommitHash.Length >= 8 ? p.OldCommitHash[..8] : p.OldCommitHash;
                string newSha = p.NewCommitHash.Length >= 8 ? p.NewCommitHash[..8] : p.NewCommitHash;
                dgvPushes.Rows.Add(
                    p.PushId, p.BranchRef, oldSha, newSha, p.PusherFullName,
                    p.PushedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load commit history: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task LoadDiscussionsAsync(RepositoryRecord repo)
    {
        dgvDiscussions.Rows.Clear();
        try
        {
            List<DiscussionCommentViewDto> comments = await _session.Discussions.ListCommentViewsByRepoAsync(
                repo.RepoId, true);
            _comments.Clear();
            _comments.AddRange(comments);

            foreach (DiscussionCommentViewDto cm in comments)
            {
                string hiddenText = cm.IsModeratedHidden ? "Hidden" : "Visible";
                dgvDiscussions.Rows.Add(
                    cm.CommentId, cm.AuthorFullName, cm.AuthorRole.ToString(),
                    cm.CommentBody, hiddenText,
                    cm.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load discussions: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void dgvCourses_SelectionChanged(object? sender, EventArgs e)
    {
        if (_isLoading || dgvCourses.SelectedRows.Count == 0)
        {
            return;
        }

        int index = dgvCourses.SelectedRows[0].Index;
        if (index >= 0 && index < _courses.Count)
        {
            await OnCourseSelectedAsync(_courses[index]);
        }
    }

    private async void dgvSubmissions_SelectionChanged(object? sender, EventArgs e)
    {
        if (_isLoading || dgvSubmissions.SelectedRows.Count == 0)
        {
            return;
        }

        int index = dgvSubmissions.SelectedRows[0].Index;
        if (index >= 0 && index < _submissions.Count)
        {
            await OnSubmissionSelectedAsync(_submissions[index]);
        }
    }

    private async void btnNewCourse_Click(object? sender, EventArgs e)
    {
        using var dlg = new CourseEditorDialog(_session);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await LoadCoursesAsync();
        }
    }

    private async void btnEditCourse_Click(object? sender, EventArgs e)
    {
        if (_selectedCourse is null)
        {
            MessageBox.Show(this, "please select a course to edit.", "Course Catalog",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new CourseEditorDialog(_session, _selectedCourse);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await LoadCoursesAsync();
        }
    }

    private async void btnDeleteCourse_Click(object? sender, EventArgs e)
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

        DialogResult confirm = MessageBox.Show(this,
            $"are you sure you want to delete '{_selectedCourse.CourseCode} - {_selectedCourse.CourseTitle}'?\n\nNote: Courses with active repositories cannot be deleted.",
            "Delete Course", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        btnDeleteCourse.Enabled = false;
        try
        {
            var (ok, message) = await _session.Courses.DeleteCourseAsync(actor, _selectedCourse.CourseId);
            if (!ok)
            {
                MessageBox.Show(this, message, "Delete Course", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, "course deleted successfully.", "Delete Course",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadCoursesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not delete course: " + ex.Message, "Delete Course",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnDeleteCourse.Enabled = true;
        }
    }

    private async void btnRepoBin_Click(object? sender, EventArgs e)
    {
        using var dlg = new RepoBinDialog(_session);
        dlg.ShowDialog(this);
        await RefreshDataAsync();
    }

    private async void btnGradeRubric_Click(object? sender, EventArgs e)
    {
        if (_selectedRepo is null)
        {
            MessageBox.Show(this, "please select a student repository to grade.", "Rubric Grading",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string ownerText = _selectedRepo.OwnerStudentId.HasValue ? $"Student #{_selectedRepo.OwnerStudentId.Value}" : "General";
        string courseCode = _selectedCourse?.CourseCode ?? string.Empty;

        using var dlg = new GradeRubricDialog(
            _session, _selectedRepo.RepoId, _selectedRepo.Title, _selectedRepo.RepoSlug,
            courseCode, ownerText, _selectedRepo.SourceLines, _selectedRepo.CommentLines, _selectedRepo.TotalFiles);

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await RefreshDataAsync();
        }
    }

    private async void btnToggleLock_Click(object? sender, EventArgs e)
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
        string actionText = nextLock ? "lock" : "unlock";

        DialogResult confirm = MessageBox.Show(this,
            $"are you sure you want to {actionText} repository '{_selectedRepo.RepoSlug}'?",
            "Lock / Unlock Repository", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        btnToggleLock.Enabled = false;
        try
        {
            var (ok, message) = await _session.Repos.SetLockStateAsync(actor, _selectedRepo.RepoId, nextLock);
            if (!ok)
            {
                MessageBox.Show(this, message, "Lock Repository", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, $"repository {actionText}ed successfully.", "Lock Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not update lock state: " + ex.Message, "Lock Repository",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnToggleLock.Enabled = true;
        }
    }

    private async void btnEditRepo_Click(object? sender, EventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        using var dlg = new RepositoryEditorDialog(_session, _selectedRepo);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await RefreshDataAsync();
        }
    }

    private async void btnTrashRepo_Click(object? sender, EventArgs e)
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

        DialogResult confirm = MessageBox.Show(this,
            $"are you sure you want to move '{_selectedRepo.Title}' ({_selectedRepo.RepoSlug}) to the Repository Bin?",
            "Move to Repo Bin", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        btnTrashRepo.Enabled = false;
        try
        {
            var (ok, message) = await _session.Repos.MoveToRepoBinAsync(actor, _selectedRepo.RepoId);
            if (!ok)
            {
                MessageBox.Show(this, message, "Repo Bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, "repository moved to bin.", "Repo Bin",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not trash repository: " + ex.Message, "Repo Bin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnTrashRepo.Enabled = true;
        }
    }

    private void btnCopyClone_Click(object? sender, EventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        try
        {
            string ip = LanHelper.GetPreferredLanIpv4Address();
            string url = LanHelper.BuildCloneUrl(_selectedRepo.RepoSlug, _session.ServerPort, ip);
            Clipboard.SetText(url);
            MessageBox.Show(this, "git clone URL copied to clipboard (Read-Only access for faculty).",
                "Repo Vault", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not copy clone url: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnNewTask_Click(object? sender, EventArgs e)
    {
        if (_selectedCourse is null)
        {
            MessageBox.Show(this, "please select a course first.", "Coding Task",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new CodingTaskDialog(_session, _selectedCourse.CourseId, _selectedCourse.CourseCode);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await RefreshDataAsync();
        }
    }

    private async void btnApproveTicket_Click(object? sender, EventArgs e)
    {
        if (dgvTickets.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "please select a ticket to approve.", "Groupmate Ticket",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int index = dgvTickets.SelectedRows[0].Index;
        if (index < 0 || index >= _tickets.Count)
        {
            return;
        }

        ContributorViewDto ticket = _tickets[index];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        btnApproveTicket.Enabled = false;
        try
        {
            var (ok, message) = await _session.Contributors.ReviewGroupmateTicketAsync(
                actor, ticket.RepoId, ticket.StudentUserId, true);

            if (!ok)
            {
                MessageBox.Show(this, message, "Approve Ticket", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, $"ticket approved for {ticket.FullName}.", "Approve Ticket",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not approve ticket: " + ex.Message, "Approve Ticket",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnApproveTicket.Enabled = true;
        }
    }

    private async void btnRejectTicket_Click(object? sender, EventArgs e)
    {
        if (dgvTickets.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "please select a ticket to reject.", "Groupmate Ticket",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int index = dgvTickets.SelectedRows[0].Index;
        if (index < 0 || index >= _tickets.Count)
        {
            return;
        }

        ContributorViewDto ticket = _tickets[index];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        btnRejectTicket.Enabled = false;
        try
        {
            var (ok, message) = await _session.Contributors.ReviewGroupmateTicketAsync(
                actor, ticket.RepoId, ticket.StudentUserId, false);

            if (!ok)
            {
                MessageBox.Show(this, message, "Reject Ticket", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, $"ticket rejected for {ticket.FullName}.", "Reject Ticket",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not reject ticket: " + ex.Message, "Reject Ticket",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnRejectTicket.Enabled = true;
        }
    }

    private async void btnToggleModeration_Click(object? sender, EventArgs e)
    {
        if (dgvDiscussions.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "please select a comment to moderate.", "Moderate Comment",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int index = dgvDiscussions.SelectedRows[0].Index;
        if (index < 0 || index >= _comments.Count)
        {
            return;
        }

        DiscussionCommentViewDto comment = _comments[index];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        bool nextHidden = !comment.IsModeratedHidden;
        btnToggleModeration.Enabled = false;
        try
        {
            var (ok, message) = await _session.Discussions.SetModerationHiddenAsync(
                actor, comment.CommentId, nextHidden);

            if (!ok)
            {
                MessageBox.Show(this, message, "Moderate Comment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await LoadDiscussionsAsync(_selectedRepo!);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not moderate comment: " + ex.Message, "Moderate Comment",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnToggleModeration.Enabled = true;
        }
    }

    private async void btnPostComment_Click(object? sender, EventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        string text = txtComment.Text.Trim();
        if (text.Length == 0)
        {
            return;
        }

        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        btnPostComment.Enabled = false;
        try
        {
            var (ok, message, _) = await _session.Discussions.AddCommentAsync(
                actor, _selectedRepo.RepoId, text);

            if (!ok)
            {
                MessageBox.Show(this, message, "Post Comment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtComment.Clear();
            await LoadDiscussionsAsync(_selectedRepo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not post comment: " + ex.Message, "Post Comment",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnPostComment.Enabled = true;
        }
    }

    public void HandlePushNotification(int pushedRepoId)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HandlePushNotification(pushedRepoId)));
            return;
        }

        _ = RefreshDataAsync();
    }
}
