using System.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;
using RepoVault.Desktop.Forms;

namespace RepoVault.Desktop.Controls;

public partial class StudentWorkspaceControl : UserControl
{
    private readonly AppSession _session;
    private readonly List<Course> _courses = new();
    private readonly List<RepositoryRecord> _repos = new();
    private readonly List<CodingTask> _tasks = new();
    private RepositoryRecord? _selectedRepo;
    private bool _isLoading;

    public StudentWorkspaceControl(AppSession session)
    {
        _session = session;
        InitializeComponent();
        ConfigureGrids();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadCoursesAndDataAsync();
    }

    private void ConfigureGrids()
    {
        // my repos grid
        dgvMyRepos.Columns.Clear();
        dgvMyRepos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRepoId", Visible = false });
        dgvMyRepos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Repository", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvMyRepos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRole", HeaderText = "Role", Width = 75 });
        dgvMyRepos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSubmitted", HeaderText = "Turned In", Width = 80 });
        dgvMyRepos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLocked", HeaderText = "Locked", Width = 55 });

        // course tasks grid
        dgvCourseTasks.Columns.Clear();
        dgvCourseTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskId", Visible = false });
        dgvCourseTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTaskTitle", HeaderText = "Task Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvCourseTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTemplate", HeaderText = "Lang", Width = 65 });
        dgvCourseTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDeadline", HeaderText = "Deadline", Width = 110 });
        dgvCourseTasks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCollab", HeaderText = "Type", Width = 75 });

        // commits grid
        dgvCommits.Columns.Clear();
        var colCommitSha = new DataGridViewTextBoxColumn
        {
            Name = "colCommitSha",
            HeaderText = "Commit",
            Width = 85,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 9F) }
        };
        dgvCommits.Columns.Add(colCommitSha);
        dgvCommits.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommitMsg", HeaderText = "Message", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvCommits.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommitAuthor", HeaderText = "Author", Width = 120 });
        dgvCommits.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommitDate", HeaderText = "Committed", Width = 130 });

        // push ledger grid
        dgvPushes.Columns.Clear();
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushId", HeaderText = "Push #", Width = 55 });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushBranch", HeaderText = "Branch", Width = 80 });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPushOldSha",
            HeaderText = "Old SHA",
            Width = 80,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 9F) }
        });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPushNewSha",
            HeaderText = "New SHA",
            Width = 80,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Consolas", 9F) }
        });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushUser", HeaderText = "Pushed By", Width = 120 });
        dgvPushes.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPushDate", HeaderText = "Pushed At", Width = 130 });

        // contributors grid
        dgvContributors.Columns.Clear();
        dgvContributors.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContribName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvContributors.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContribUser", HeaderText = "Username", Width = 95 });
        dgvContributors.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContribRole", HeaderText = "Team Role", Width = 85 });
        dgvContributors.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContribAccess", HeaderText = "Access Level", Width = 110 });
        dgvContributors.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContribStatus", HeaderText = "Approval Status", Width = 110 });

        // discussions grid
        dgvDiscussions.Columns.Clear();
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommentAuthor", HeaderText = "Author", Width = 120 });
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommentBody", HeaderText = "Comment", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvDiscussions.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCommentDate", HeaderText = "Date", Width = 130 });
    }

    public async Task LoadCoursesAndDataAsync()
    {
        try
        {
            List<Course> list = await _session.Courses.ListCoursesAsync();
            _courses.Clear();
            _courses.AddRange(list);

            cboCourses.Items.Clear();
            cboCourses.Items.Add("All Courses");
            foreach (Course c in _courses)
            {
                cboCourses.Items.Add($"{c.CourseCode} - {c.CourseTitle}");
            }

            if (cboCourses.Items.Count > 0)
            {
                cboCourses.SelectedIndex = 0;
            }

            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load course data: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
            if (cboCourses.SelectedIndex > 0 && cboCourses.SelectedIndex - 1 < _courses.Count)
            {
                courseFilter = _courses[cboCourses.SelectedIndex - 1].CourseId;
            }

            // preserve selection if possible
            int prevSelectedRepoId = _selectedRepo?.RepoId ?? 0;

            // load repositories
            _repos.Clear();
            List<RepositoryRecord> repos = await _session.Repos.ListVisibleRepositoriesForUserAsync(actor, courseFilter);
            _repos.AddRange(repos);

            dgvMyRepos.Rows.Clear();
            int selectRowIndex = -1;
            for (int i = 0; i < _repos.Count; i++)
            {
                RepositoryRecord r = _repos[i];
                string role = r.OwnerStudentId == actor.UserId ? "Owner" : "Contributor";
                string turnedIn = r.IsSubmitted
                    ? (r.SubmittedAt.HasValue ? r.SubmittedAt.Value.ToLocalTime().ToString("MM/dd HH:mm") : "Yes")
                    : "No";
                string locked = r.IsLocked ? "Yes" : "No";

                dgvMyRepos.Rows.Add(r.RepoId, r.Title, role, turnedIn, locked);
                if (r.RepoId == prevSelectedRepoId)
                {
                    selectRowIndex = i;
                }
            }

            // load assigned tasks
            _tasks.Clear();
            if (courseFilter.HasValue)
            {
                List<CodingTask> tasks = await _session.Courses.ListTasksByCourseAsync(courseFilter.Value);
                _tasks.AddRange(tasks);
            }
            else
            {
                foreach (Course c in _courses)
                {
                    List<CodingTask> tasks = await _session.Courses.ListTasksByCourseAsync(c.CourseId);
                    _tasks.AddRange(tasks);
                }
            }

            dgvCourseTasks.Rows.Clear();
            foreach (CodingTask t in _tasks)
            {
                string deadline = t.DeadlineUtc.HasValue
                    ? t.DeadlineUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    : "None";
                string collab = t.AllowGroupWork ? $"Group ({t.MaxGroupSize})" : "Solo";
                dgvCourseTasks.Rows.Add(t.TaskId, t.Title, t.StarterTemplate, deadline, collab);
            }

            if (dgvMyRepos.Rows.Count > 0)
            {
                int targetIndex = selectRowIndex >= 0 ? selectRowIndex : 0;
                dgvMyRepos.Rows[targetIndex].Selected = true;
                await OnRepoSelectedAsync(_repos[targetIndex]);
            }
            else
            {
                _selectedRepo = null;
                ClearDetailsUi();
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ClearDetailsUi()
    {
        txtCloneUrl.Text = string.Empty;
        btnToggleSubmission.Enabled = false;
        btnRequestTicket.Enabled = false;
        btnGrantPeer.Enabled = false;

        tvFiles.Nodes.Clear();
        rtbCode.Clear();
        lblFilePath.Text = "No file selected";
        lblFileSize.Text = string.Empty;

        dgvCommits.Rows.Clear();
        dgvPushes.Rows.Clear();
        dgvContributors.Rows.Clear();
        dgvDiscussions.Rows.Clear();
        flpLanguages.Controls.Clear();

        lblMetricTotalFilesVal.Text = "0";
        lblMetricPrimaryLangVal.Text = "None";
        lblMetricSourceLocVal.Text = "0";
        lblMetricCommentLocVal.Text = "0";
        lblMetricBlankLocVal.Text = "0";
        lblMetricRatioVal.Text = "N/A";

        lblRubricTitle.Text = "Faculty Rubric Evaluation";
        lblRubricStatus.Text = "Status: No repository selected";
        lblRubricTotal.Text = "Total Score: -- / 100";
        lblArchScore.Text = "-- / 25";
        lblFuncScore.Text = "-- / 25";
        lblQualScore.Text = "-- / 25";
        lblDocScore.Text = "-- / 25";
        rtbFacultyFeedback.Text = string.Empty;
        lblLockedWarning.Visible = false;
    }

    private async Task OnRepoSelectedAsync(RepositoryRecord repo)
    {
        _selectedRepo = repo;
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        string ip = LanHelper.GetPreferredLanIpv4Address();
        txtCloneUrl.Text = LanHelper.BuildCloneUrl(repo.RepoSlug, _session.ServerPort, ip);

        bool canSubmit = !repo.IsLocked && (!repo.DeadlineUtc.HasValue || repo.DeadlineUtc.Value >= DateTime.UtcNow);
        btnToggleSubmission.Enabled = canSubmit;
        btnToggleSubmission.Text = repo.IsSubmitted ? "Unsubmit Work" : "Turn In Submission";

        bool isOwner = repo.OwnerStudentId == actor.UserId;
        btnRequestTicket.Enabled = isOwner && !repo.IsLocked;
        btnGrantPeer.Enabled = isOwner && !repo.IsLocked;

        await Task.WhenAll(
            LoadSourceFilesAsync(repo, actor),
            LoadCommitsAndPushesAsync(repo),
            LoadMetricsAsync(repo),
            LoadTeamAndDiscussionsAsync(repo),
            LoadRubricAsync(repo));
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
                    c.CommittedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }

            List<PushLedgerViewDto> pushes = await _session.Repos.ListPushLedgerViewAsync(repo.RepoId);
            foreach (PushLedgerViewDto p in pushes)
            {
                string oldSha = p.OldCommitHash.Length >= 8 ? p.OldCommitHash[..8] : p.OldCommitHash;
                string newSha = p.NewCommitHash.Length >= 8 ? p.NewCommitHash[..8] : p.NewCommitHash;
                dgvPushes.Rows.Add(
                    p.PushId, p.BranchRef, oldSha, newSha, p.PusherFullName,
                    p.PushedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load commit history: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task LoadMetricsAsync(RepositoryRecord repo)
    {
        lblMetricTotalFilesVal.Text = repo.TotalFiles.ToString();
        lblMetricPrimaryLangVal.Text = string.IsNullOrWhiteSpace(repo.PrimaryLanguage) ? "None" : repo.PrimaryLanguage;
        lblMetricSourceLocVal.Text = repo.SourceLines.ToString("N0");
        lblMetricCommentLocVal.Text = repo.CommentLines.ToString("N0");
        lblMetricBlankLocVal.Text = repo.BlankLines.ToString("N0");
        lblMetricRatioVal.Text = repo.SourceLines > 0 ? $"{(double)repo.CommentLines / repo.SourceLines:P1}" : "N/A";

        flpLanguages.Controls.Clear();
        try
        {
            List<RepositoryLanguage> langs = await _session.Repos.ListLanguagesAsync(repo.RepoId);
            foreach (RepositoryLanguage lang in langs)
            {
                var rowPanel = new Panel
                {
                    Width = Math.Max(280, flpLanguages.ClientSize.Width - 24),
                    Height = 44,
                    Margin = new Padding(0, 0, 0, 6)
                };

                var lblLang = new Label
                {
                    Text = $"{lang.LanguageName} - {lang.PercentageShare:F1}% ({lang.SourceLines:N0} lines)",
                    Dock = DockStyle.Top,
                    Height = 18,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
                };

                var pb = new ProgressBar
                {
                    Dock = DockStyle.Bottom,
                    Height = 18,
                    Minimum = 0,
                    Maximum = 100,
                    Value = Math.Clamp((int)Math.Round(lang.PercentageShare), 0, 100)
                };

                rowPanel.Controls.Add(lblLang);
                rowPanel.Controls.Add(pb);
                flpLanguages.Controls.Add(rowPanel);
            }
        }
        catch (Exception)
        {
            // non-critical if languages cannot be retrieved
        }
    }

    private async Task LoadTeamAndDiscussionsAsync(RepositoryRecord repo)
    {
        dgvContributors.Rows.Clear();
        dgvDiscussions.Rows.Clear();

        try
        {
            List<ContributorViewDto> contributors = await _session.Contributors.ListContributorViewsByRepoAsync(repo.RepoId);
            foreach (ContributorViewDto c in contributors)
            {
                dgvContributors.Rows.Add(
                    c.FullName, c.Username, c.TeamRole,
                    c.AccessLevel.ToString(), c.ApprovalStatus.ToString());
            }

            List<DiscussionCommentViewDto> comments = await _session.Discussions.ListCommentViewsByRepoAsync(repo.RepoId, false);
            foreach (DiscussionCommentViewDto cm in comments)
            {
                string body = cm.IsModeratedHidden ? "[moderated by faculty]" : cm.CommentBody;
                dgvDiscussions.Rows.Add(
                    cm.AuthorFullName, body,
                    cm.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not load team or discussions: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task LoadRubricAsync(RepositoryRecord repo)
    {
        lblLockedWarning.Visible = repo.IsLocked;

        try
        {
            EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(repo.RepoId);
            if (eval is not null)
            {
                lblRubricStatus.Text = $"Status: Evaluated on {eval.EvaluatedAt.ToLocalTime():yyyy-MM-dd HH:mm}";
                lblRubricTotal.Text = $"Total Score: {eval.TotalScore} / 100";
                lblArchScore.Text = $"{eval.ArchitectureScore} / 25";
                lblFuncScore.Text = $"{eval.FunctionalityScore} / 25";
                lblQualScore.Text = $"{eval.CodeQualityScore} / 25";
                lblDocScore.Text = $"{eval.DocumentationScore} / 25";
                rtbFacultyFeedback.Text = eval.CritiqueNotes ?? "No written critique notes provided.";
            }
            else
            {
                lblRubricStatus.Text = "Status: Not Evaluated Yet";
                lblRubricTotal.Text = "Total Score: -- / 100";
                lblArchScore.Text = "-- / 25";
                lblFuncScore.Text = "-- / 25";
                lblQualScore.Text = "-- / 25";
                lblDocScore.Text = "-- / 25";
                rtbFacultyFeedback.Text = "Faculty has not submitted a rubric evaluation for this repository yet.";
            }
        }
        catch (Exception)
        {
            lblRubricStatus.Text = "Status: Could not load evaluation";
        }
    }

    private async void dgvMyRepos_SelectionChanged(object? sender, EventArgs e)
    {
        if (_isLoading || dgvMyRepos.SelectedRows.Count == 0)
        {
            return;
        }

        int index = dgvMyRepos.SelectedRows[0].Index;
        if (index >= 0 && index < _repos.Count)
        {
            await OnRepoSelectedAsync(_repos[index]);
        }
    }

    private async void btnRefresh_Click(object? sender, EventArgs e)
    {
        await RefreshDataAsync();
    }

    private async void cboCourses_SelectedIndexChanged(object? sender, EventArgs e)
    {
        await RefreshDataAsync();
    }

    private void btnCopyClone_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCloneUrl.Text))
        {
            return;
        }

        try
        {
            Clipboard.SetText(txtCloneUrl.Text);
            MessageBox.Show(this, "git clone URL copied to clipboard.", "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not copy to clipboard: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnToggleSubmission_Click(object? sender, EventArgs e)
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
        string actionText = nextState ? "turn in your submission" : "unsubmit your work";
        DialogResult confirm = MessageBox.Show(this,
            $"are you sure you want to {actionText} for '{_selectedRepo.Title}'?",
            "Submission State", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        btnToggleSubmission.Enabled = false;
        try
        {
            var (ok, message) = await _session.Repos.SetSubmissionStateAsync(actor, _selectedRepo.RepoId, nextState);
            if (!ok)
            {
                MessageBox.Show(this, message, "Submission State", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, nextState ? "submission turned in successfully." : "submission reverted to draft.",
                "Submission State", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not update submission state: " + ex.Message, "Submission State",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnToggleSubmission.Enabled = true;
        }
    }

    private async void btnStartTask_Click(object? sender, EventArgs e)
    {
        if (dgvCourseTasks.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "please select a task from the list.", "Start Task",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int index = dgvCourseTasks.SelectedRows[0].Index;
        if (index < 0 || index >= _tasks.Count)
        {
            return;
        }

        CodingTask task = _tasks[index];
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        DialogResult confirm = MessageBox.Show(this,
            $"initialize workspace repository for '{task.Title}' with {task.StarterTemplate} starter template?",
            "Start Coding Task", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        btnStartTask.Enabled = false;
        try
        {
            var (ok, message, repo) = await _session.Repos.StartCodingTaskAsync(actor, task.TaskId);
            if (!ok || repo is null)
            {
                MessageBox.Show(this, "could not start task: " + message, "Start Task",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, $"task repository initialized at {repo.RepoSlug}.", "Start Task",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // switch back to submissions tab and select newly started repo
            tabSidebar.SelectedTab = tabSubmissions;
            _selectedRepo = repo;
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not start task: " + ex.Message, "Start Task",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnStartTask.Enabled = true;
        }
    }

    private async void btnRequestTicket_Click(object? sender, EventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        using var dlg = new RequestTicketDialog(_session, _selectedRepo.RepoId);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await LoadTeamAndDiscussionsAsync(_selectedRepo);
        }
    }

    private async void btnGrantPeer_Click(object? sender, EventArgs e)
    {
        if (_selectedRepo is null)
        {
            return;
        }

        using var dlg = new GrantPeerDialog(_session, _selectedRepo.RepoId);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await LoadTeamAndDiscussionsAsync(_selectedRepo);
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
            var (ok, message, _) = await _session.Discussions.AddCommentAsync(actor, _selectedRepo.RepoId, text);
            if (!ok)
            {
                MessageBox.Show(this, message, "Post Comment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtComment.Clear();
            await LoadTeamAndDiscussionsAsync(_selectedRepo);
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
