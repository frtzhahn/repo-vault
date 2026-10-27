namespace RepoVault.Desktop.Controls;

partial class StudentWorkspaceControl
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.splitMain = new System.Windows.Forms.SplitContainer();
        this.tabSidebar = new System.Windows.Forms.TabControl();
        this.tabSubmissions = new System.Windows.Forms.TabPage();
        this.dgvMyRepos = new System.Windows.Forms.DataGridView();
        this.pnlSubmissionsBottom = new System.Windows.Forms.Panel();
        this.btnToggleSubmission = new System.Windows.Forms.Button();
        this.tabAssignedTasks = new System.Windows.Forms.TabPage();
        this.dgvCourseTasks = new System.Windows.Forms.DataGridView();
        this.pnlTasksBottom = new System.Windows.Forms.Panel();
        this.btnStartTask = new System.Windows.Forms.Button();
        this.pnlCloneBar = new System.Windows.Forms.Panel();
        this.btnCopyClone = new System.Windows.Forms.Button();
        this.txtCloneUrl = new System.Windows.Forms.TextBox();
        this.lblCloneUrl = new System.Windows.Forms.Label();
        this.pnlCourseFilter = new System.Windows.Forms.Panel();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.cboCourses = new System.Windows.Forms.ComboBox();
        this.lblCourseFilter = new System.Windows.Forms.Label();
        this.tabDetails = new System.Windows.Forms.TabControl();
        this.tabCode = new System.Windows.Forms.TabPage();
        this.splitCode = new System.Windows.Forms.SplitContainer();
        this.tvFiles = new System.Windows.Forms.TreeView();
        this.rtbCode = new System.Windows.Forms.RichTextBox();
        this.pnlCodeHeader = new System.Windows.Forms.Panel();
        this.lblFileSize = new System.Windows.Forms.Label();
        this.lblFilePath = new System.Windows.Forms.Label();
        this.tabHistory = new System.Windows.Forms.TabPage();
        this.splitHistory = new System.Windows.Forms.SplitContainer();
        this.dgvCommits = new System.Windows.Forms.DataGridView();
        this.lblCommitsHeader = new System.Windows.Forms.Label();
        this.dgvPushes = new System.Windows.Forms.DataGridView();
        this.lblPushesHeader = new System.Windows.Forms.Label();
        this.tabMetrics = new System.Windows.Forms.TabPage();
        this.pnlMetricsContent = new System.Windows.Forms.Panel();
        this.flpLanguages = new System.Windows.Forms.FlowLayoutPanel();
        this.lblLanguagesHeader = new System.Windows.Forms.Label();
        this.tblMetricsSummary = new System.Windows.Forms.TableLayoutPanel();
        this.lblMetricTotalFilesTitle = new System.Windows.Forms.Label();
        this.lblMetricTotalFilesVal = new System.Windows.Forms.Label();
        this.lblMetricPrimaryLangTitle = new System.Windows.Forms.Label();
        this.lblMetricPrimaryLangVal = new System.Windows.Forms.Label();
        this.lblMetricSourceLocTitle = new System.Windows.Forms.Label();
        this.lblMetricSourceLocVal = new System.Windows.Forms.Label();
        this.lblMetricCommentLocTitle = new System.Windows.Forms.Label();
        this.lblMetricCommentLocVal = new System.Windows.Forms.Label();
        this.lblMetricBlankLocTitle = new System.Windows.Forms.Label();
        this.lblMetricBlankLocVal = new System.Windows.Forms.Label();
        this.lblMetricRatioTitle = new System.Windows.Forms.Label();
        this.lblMetricRatioVal = new System.Windows.Forms.Label();
        this.lblMetricsHeader = new System.Windows.Forms.Label();
        this.tabTeam = new System.Windows.Forms.TabPage();
        this.splitTeam = new System.Windows.Forms.SplitContainer();
        this.dgvContributors = new System.Windows.Forms.DataGridView();
        this.pnlTeamActions = new System.Windows.Forms.Panel();
        this.btnGrantPeer = new System.Windows.Forms.Button();
        this.btnRequestTicket = new System.Windows.Forms.Button();
        this.lblTeamHeader = new System.Windows.Forms.Label();
        this.dgvDiscussions = new System.Windows.Forms.DataGridView();
        this.pnlCommentInput = new System.Windows.Forms.Panel();
        this.btnPostComment = new System.Windows.Forms.Button();
        this.txtComment = new System.Windows.Forms.TextBox();
        this.lblDiscussionHeader = new System.Windows.Forms.Label();
        this.tabRubric = new System.Windows.Forms.TabPage();
        this.pnlRubricContent = new System.Windows.Forms.Panel();
        this.lblLockedWarning = new System.Windows.Forms.Label();
        this.rtbFacultyFeedback = new System.Windows.Forms.RichTextBox();
        this.lblCritiqueHeader = new System.Windows.Forms.Label();
        this.tblRubricScores = new System.Windows.Forms.TableLayoutPanel();
        this.lblArchTitle = new System.Windows.Forms.Label();
        this.lblArchScore = new System.Windows.Forms.Label();
        this.lblFuncTitle = new System.Windows.Forms.Label();
        this.lblFuncScore = new System.Windows.Forms.Label();
        this.lblQualTitle = new System.Windows.Forms.Label();
        this.lblQualScore = new System.Windows.Forms.Label();
        this.lblDocTitle = new System.Windows.Forms.Label();
        this.lblDocScore = new System.Windows.Forms.Label();
        this.lblRubricTotal = new System.Windows.Forms.Label();
        this.lblRubricStatus = new System.Windows.Forms.Label();
        this.lblRubricTitle = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
        this.splitMain.Panel1.SuspendLayout();
        this.splitMain.Panel2.SuspendLayout();
        this.splitMain.SuspendLayout();
        this.tabSidebar.SuspendLayout();
        this.tabSubmissions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvMyRepos)).BeginInit();
        this.pnlSubmissionsBottom.SuspendLayout();
        this.tabAssignedTasks.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCourseTasks)).BeginInit();
        this.pnlTasksBottom.SuspendLayout();
        this.pnlCloneBar.SuspendLayout();
        this.pnlCourseFilter.SuspendLayout();
        this.tabDetails.SuspendLayout();
        this.tabCode.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitCode)).BeginInit();
        this.splitCode.Panel1.SuspendLayout();
        this.splitCode.Panel2.SuspendLayout();
        this.splitCode.SuspendLayout();
        this.pnlCodeHeader.SuspendLayout();
        this.tabHistory.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitHistory)).BeginInit();
        this.splitHistory.Panel1.SuspendLayout();
        this.splitHistory.Panel2.SuspendLayout();
        this.splitHistory.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCommits)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPushes)).BeginInit();
        this.tabMetrics.SuspendLayout();
        this.pnlMetricsContent.SuspendLayout();
        this.tblMetricsSummary.SuspendLayout();
        this.tabTeam.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitTeam)).BeginInit();
        this.splitTeam.Panel1.SuspendLayout();
        this.splitTeam.Panel2.SuspendLayout();
        this.splitTeam.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvContributors)).BeginInit();
        this.pnlTeamActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDiscussions)).BeginInit();
        this.pnlCommentInput.SuspendLayout();
        this.tabRubric.SuspendLayout();
        this.pnlRubricContent.SuspendLayout();
        this.tblRubricScores.SuspendLayout();
        this.SuspendLayout();
        //
        // splitMain
        //
        this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
        this.splitMain.Location = new System.Drawing.Point(0, 0);
        this.splitMain.Name = "splitMain";
        //
        // splitMain.Panel1
        //
        this.splitMain.Panel1.Controls.Add(this.tabSidebar);
        this.splitMain.Panel1.Controls.Add(this.pnlCloneBar);
        this.splitMain.Panel1.Controls.Add(this.pnlCourseFilter);
        this.splitMain.Panel1MinSize = 280;
        //
        // splitMain.Panel2
        //
        this.splitMain.Panel2.Controls.Add(this.tabDetails);
        this.splitMain.Panel2MinSize = 400;
        this.splitMain.Size = new System.Drawing.Size(1024, 600);
        this.splitMain.SplitterDistance = 340;
        this.splitMain.TabIndex = 0;
        //
        // tabSidebar
        //
        this.tabSidebar.Controls.Add(this.tabSubmissions);
        this.tabSidebar.Controls.Add(this.tabAssignedTasks);
        this.tabSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabSidebar.Location = new System.Drawing.Point(0, 40);
        this.tabSidebar.Name = "tabSidebar";
        this.tabSidebar.SelectedIndex = 0;
        this.tabSidebar.Size = new System.Drawing.Size(340, 504);
        this.tabSidebar.TabIndex = 1;
        //
        // tabSubmissions
        //
        this.tabSubmissions.Controls.Add(this.dgvMyRepos);
        this.tabSubmissions.Controls.Add(this.pnlSubmissionsBottom);
        this.tabSubmissions.Location = new System.Drawing.Point(4, 24);
        this.tabSubmissions.Name = "tabSubmissions";
        this.tabSubmissions.Padding = new System.Windows.Forms.Padding(3);
        this.tabSubmissions.Size = new System.Drawing.Size(332, 476);
        this.tabSubmissions.TabIndex = 0;
        this.tabSubmissions.Text = "My Submissions";
        this.tabSubmissions.UseVisualStyleBackColor = true;
        //
        // dgvMyRepos
        //
        this.dgvMyRepos.AllowUserToAddRows = false;
        this.dgvMyRepos.AllowUserToDeleteRows = false;
        this.dgvMyRepos.AllowUserToResizeRows = false;
        this.dgvMyRepos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvMyRepos.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvMyRepos.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvMyRepos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvMyRepos.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvMyRepos.Location = new System.Drawing.Point(3, 3);
        this.dgvMyRepos.MultiSelect = false;
        this.dgvMyRepos.Name = "dgvMyRepos";
        this.dgvMyRepos.ReadOnly = true;
        this.dgvMyRepos.RowHeadersVisible = false;
        this.dgvMyRepos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvMyRepos.Size = new System.Drawing.Size(326, 434);
        this.dgvMyRepos.TabIndex = 0;
        this.dgvMyRepos.SelectionChanged += new System.EventHandler(this.dgvMyRepos_SelectionChanged);
        //
        // pnlSubmissionsBottom
        //
        this.pnlSubmissionsBottom.Controls.Add(this.btnToggleSubmission);
        this.pnlSubmissionsBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlSubmissionsBottom.Location = new System.Drawing.Point(3, 437);
        this.pnlSubmissionsBottom.Name = "pnlSubmissionsBottom";
        this.pnlSubmissionsBottom.Padding = new System.Windows.Forms.Padding(4);
        this.pnlSubmissionsBottom.Size = new System.Drawing.Size(326, 36);
        this.pnlSubmissionsBottom.TabIndex = 1;
        //
        // btnToggleSubmission
        //
        this.btnToggleSubmission.Dock = System.Windows.Forms.DockStyle.Fill;
        this.btnToggleSubmission.Location = new System.Drawing.Point(4, 4);
        this.btnToggleSubmission.Name = "btnToggleSubmission";
        this.btnToggleSubmission.Size = new System.Drawing.Size(318, 28);
        this.btnToggleSubmission.TabIndex = 0;
        this.btnToggleSubmission.Text = "Turn In Submission";
        this.btnToggleSubmission.UseVisualStyleBackColor = true;
        this.btnToggleSubmission.Click += new System.EventHandler(this.btnToggleSubmission_Click);
        //
        // tabAssignedTasks
        //
        this.tabAssignedTasks.Controls.Add(this.dgvCourseTasks);
        this.tabAssignedTasks.Controls.Add(this.pnlTasksBottom);
        this.tabAssignedTasks.Location = new System.Drawing.Point(4, 24);
        this.tabAssignedTasks.Name = "tabAssignedTasks";
        this.tabAssignedTasks.Padding = new System.Windows.Forms.Padding(3);
        this.tabAssignedTasks.Size = new System.Drawing.Size(332, 476);
        this.tabAssignedTasks.TabIndex = 1;
        this.tabAssignedTasks.Text = "Assigned Tasks";
        this.tabAssignedTasks.UseVisualStyleBackColor = true;
        //
        // dgvCourseTasks
        //
        this.dgvCourseTasks.AllowUserToAddRows = false;
        this.dgvCourseTasks.AllowUserToDeleteRows = false;
        this.dgvCourseTasks.AllowUserToResizeRows = false;
        this.dgvCourseTasks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvCourseTasks.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvCourseTasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvCourseTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvCourseTasks.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvCourseTasks.Location = new System.Drawing.Point(3, 3);
        this.dgvCourseTasks.MultiSelect = false;
        this.dgvCourseTasks.Name = "dgvCourseTasks";
        this.dgvCourseTasks.ReadOnly = true;
        this.dgvCourseTasks.RowHeadersVisible = false;
        this.dgvCourseTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvCourseTasks.Size = new System.Drawing.Size(326, 434);
        this.dgvCourseTasks.TabIndex = 0;
        //
        // pnlTasksBottom
        //
        this.pnlTasksBottom.Controls.Add(this.btnStartTask);
        this.pnlTasksBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlTasksBottom.Location = new System.Drawing.Point(3, 437);
        this.pnlTasksBottom.Name = "pnlTasksBottom";
        this.pnlTasksBottom.Padding = new System.Windows.Forms.Padding(4);
        this.pnlTasksBottom.Size = new System.Drawing.Size(326, 36);
        this.pnlTasksBottom.TabIndex = 1;
        //
        // btnStartTask
        //
        this.btnStartTask.Dock = System.Windows.Forms.DockStyle.Fill;
        this.btnStartTask.Location = new System.Drawing.Point(4, 4);
        this.btnStartTask.Name = "btnStartTask";
        this.btnStartTask.Size = new System.Drawing.Size(318, 28);
        this.btnStartTask.TabIndex = 0;
        this.btnStartTask.Text = "Start Coding Task";
        this.btnStartTask.UseVisualStyleBackColor = true;
        this.btnStartTask.Click += new System.EventHandler(this.btnStartTask_Click);
        //
        // pnlCloneBar
        //
        this.pnlCloneBar.Controls.Add(this.btnCopyClone);
        this.pnlCloneBar.Controls.Add(this.txtCloneUrl);
        this.pnlCloneBar.Controls.Add(this.lblCloneUrl);
        this.pnlCloneBar.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlCloneBar.Location = new System.Drawing.Point(0, 544);
        this.pnlCloneBar.Name = "pnlCloneBar";
        this.pnlCloneBar.Padding = new System.Windows.Forms.Padding(6, 4, 6, 6);
        this.pnlCloneBar.Size = new System.Drawing.Size(340, 56);
        this.pnlCloneBar.TabIndex = 2;
        //
        // btnCopyClone
        //
        this.btnCopyClone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnCopyClone.Location = new System.Drawing.Point(270, 22);
        this.btnCopyClone.Name = "btnCopyClone";
        this.btnCopyClone.Size = new System.Drawing.Size(64, 25);
        this.btnCopyClone.TabIndex = 2;
        this.btnCopyClone.Text = "Copy";
        this.btnCopyClone.UseVisualStyleBackColor = true;
        this.btnCopyClone.Click += new System.EventHandler(this.btnCopyClone_Click);
        //
        // txtCloneUrl
        //
        this.txtCloneUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.txtCloneUrl.Font = new System.Drawing.Font("Consolas", 8.25F);
        this.txtCloneUrl.Location = new System.Drawing.Point(6, 24);
        this.txtCloneUrl.Name = "txtCloneUrl";
        this.txtCloneUrl.ReadOnly = true;
        this.txtCloneUrl.Size = new System.Drawing.Size(258, 20);
        this.txtCloneUrl.TabIndex = 1;
        //
        // lblCloneUrl
        //
        this.lblCloneUrl.AutoSize = true;
        this.lblCloneUrl.Location = new System.Drawing.Point(6, 6);
        this.lblCloneUrl.Name = "lblCloneUrl";
        this.lblCloneUrl.Size = new System.Drawing.Size(63, 15);
        this.lblCloneUrl.TabIndex = 0;
        this.lblCloneUrl.Text = "Clone URL:";
        //
        // pnlCourseFilter
        //
        this.pnlCourseFilter.Controls.Add(this.btnRefresh);
        this.pnlCourseFilter.Controls.Add(this.cboCourses);
        this.pnlCourseFilter.Controls.Add(this.lblCourseFilter);
        this.pnlCourseFilter.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlCourseFilter.Location = new System.Drawing.Point(0, 0);
        this.pnlCourseFilter.Name = "pnlCourseFilter";
        this.pnlCourseFilter.Padding = new System.Windows.Forms.Padding(6);
        this.pnlCourseFilter.Size = new System.Drawing.Size(340, 40);
        this.pnlCourseFilter.TabIndex = 0;
        //
        // btnRefresh
        //
        this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnRefresh.Location = new System.Drawing.Point(264, 7);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(70, 26);
        this.btnRefresh.TabIndex = 2;
        this.btnRefresh.Text = "Refresh";
        this.btnRefresh.UseVisualStyleBackColor = true;
        this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
        //
        // cboCourses
        //
        this.cboCourses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.cboCourses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboCourses.FormattingEnabled = true;
        this.cboCourses.Location = new System.Drawing.Point(58, 9);
        this.cboCourses.Name = "cboCourses";
        this.cboCourses.Size = new System.Drawing.Size(200, 23);
        this.cboCourses.TabIndex = 1;
        this.cboCourses.SelectedIndexChanged += new System.EventHandler(this.cboCourses_SelectedIndexChanged);
        //
        // lblCourseFilter
        //
        this.lblCourseFilter.AutoSize = true;
        this.lblCourseFilter.Location = new System.Drawing.Point(6, 12);
        this.lblCourseFilter.Name = "lblCourseFilter";
        this.lblCourseFilter.Size = new System.Drawing.Size(47, 15);
        this.lblCourseFilter.TabIndex = 0;
        this.lblCourseFilter.Text = "Course:";
        //
        // tabDetails
        //
        this.tabDetails.Controls.Add(this.tabCode);
        this.tabDetails.Controls.Add(this.tabHistory);
        this.tabDetails.Controls.Add(this.tabMetrics);
        this.tabDetails.Controls.Add(this.tabTeam);
        this.tabDetails.Controls.Add(this.tabRubric);
        this.tabDetails.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabDetails.Location = new System.Drawing.Point(0, 0);
        this.tabDetails.Name = "tabDetails";
        this.tabDetails.SelectedIndex = 0;
        this.tabDetails.Size = new System.Drawing.Size(680, 600);
        this.tabDetails.TabIndex = 0;
        //
        // tabCode
        //
        this.tabCode.Controls.Add(this.splitCode);
        this.tabCode.Location = new System.Drawing.Point(4, 24);
        this.tabCode.Name = "tabCode";
        this.tabCode.Padding = new System.Windows.Forms.Padding(3);
        this.tabCode.Size = new System.Drawing.Size(672, 572);
        this.tabCode.TabIndex = 0;
        this.tabCode.Text = "Source Files";
        this.tabCode.UseVisualStyleBackColor = true;
        //
        // splitCode
        //
        this.splitCode.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitCode.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
        this.splitCode.Location = new System.Drawing.Point(3, 3);
        this.splitCode.Name = "splitCode";
        //
        // splitCode.Panel1
        //
        this.splitCode.Panel1.Controls.Add(this.tvFiles);
        this.splitCode.Panel1MinSize = 160;
        //
        // splitCode.Panel2
        //
        this.splitCode.Panel2.Controls.Add(this.rtbCode);
        this.splitCode.Panel2.Controls.Add(this.pnlCodeHeader);
        this.splitCode.Panel2MinSize = 240;
        this.splitCode.Size = new System.Drawing.Size(666, 566);
        this.splitCode.SplitterDistance = 220;
        this.splitCode.TabIndex = 0;
        //
        // tvFiles
        //
        this.tvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tvFiles.Location = new System.Drawing.Point(0, 0);
        this.tvFiles.Name = "tvFiles";
        this.tvFiles.Size = new System.Drawing.Size(220, 566);
        this.tvFiles.TabIndex = 0;
        this.tvFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFiles_AfterSelect);
        //
        // rtbCode
        //
        this.rtbCode.BackColor = System.Drawing.SystemColors.Window;
        this.rtbCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.rtbCode.Dock = System.Windows.Forms.DockStyle.Fill;
        this.rtbCode.Font = new System.Drawing.Font("Consolas", 9.75F);
        this.rtbCode.Location = new System.Drawing.Point(0, 30);
        this.rtbCode.Name = "rtbCode";
        this.rtbCode.ReadOnly = true;
        this.rtbCode.Size = new System.Drawing.Size(442, 536);
        this.rtbCode.TabIndex = 1;
        this.rtbCode.Text = "";
        this.rtbCode.WordWrap = false;
        //
        // pnlCodeHeader
        //
        this.pnlCodeHeader.Controls.Add(this.lblFileSize);
        this.pnlCodeHeader.Controls.Add(this.lblFilePath);
        this.pnlCodeHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlCodeHeader.Location = new System.Drawing.Point(0, 0);
        this.pnlCodeHeader.Name = "pnlCodeHeader";
        this.pnlCodeHeader.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
        this.pnlCodeHeader.Size = new System.Drawing.Size(442, 30);
        this.pnlCodeHeader.TabIndex = 0;
        //
        // lblFileSize
        //
        this.lblFileSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.lblFileSize.Location = new System.Drawing.Point(316, 8);
        this.lblFileSize.Name = "lblFileSize";
        this.lblFileSize.Size = new System.Drawing.Size(120, 16);
        this.lblFileSize.TabIndex = 1;
        this.lblFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // lblFilePath
        //
        this.lblFilePath.AutoSize = true;
        this.lblFilePath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblFilePath.Location = new System.Drawing.Point(6, 8);
        this.lblFilePath.Name = "lblFilePath";
        this.lblFilePath.Size = new System.Drawing.Size(91, 15);
        this.lblFilePath.TabIndex = 0;
        this.lblFilePath.Text = "No file selected";
        //
        // tabHistory
        //
        this.tabHistory.Controls.Add(this.splitHistory);
        this.tabHistory.Location = new System.Drawing.Point(4, 24);
        this.tabHistory.Name = "tabHistory";
        this.tabHistory.Padding = new System.Windows.Forms.Padding(3);
        this.tabHistory.Size = new System.Drawing.Size(672, 572);
        this.tabHistory.TabIndex = 1;
        this.tabHistory.Text = "Commits & Pushes";
        this.tabHistory.UseVisualStyleBackColor = true;
        //
        // splitHistory
        //
        this.splitHistory.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitHistory.Location = new System.Drawing.Point(3, 3);
        this.splitHistory.Name = "splitHistory";
        this.splitHistory.Orientation = System.Windows.Forms.Orientation.Horizontal;
        //
        // splitHistory.Panel1
        //
        this.splitHistory.Panel1.Controls.Add(this.dgvCommits);
        this.splitHistory.Panel1.Controls.Add(this.lblCommitsHeader);
        this.splitHistory.Panel1MinSize = 120;
        //
        // splitHistory.Panel2
        //
        this.splitHistory.Panel2.Controls.Add(this.dgvPushes);
        this.splitHistory.Panel2.Controls.Add(this.lblPushesHeader);
        this.splitHistory.Panel2MinSize = 120;
        this.splitHistory.Size = new System.Drawing.Size(666, 566);
        this.splitHistory.SplitterDistance = 260;
        this.splitHistory.TabIndex = 0;
        //
        // dgvCommits
        //
        this.dgvCommits.AllowUserToAddRows = false;
        this.dgvCommits.AllowUserToDeleteRows = false;
        this.dgvCommits.AllowUserToResizeRows = false;
        this.dgvCommits.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvCommits.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvCommits.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvCommits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvCommits.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvCommits.Location = new System.Drawing.Point(0, 24);
        this.dgvCommits.Name = "dgvCommits";
        this.dgvCommits.ReadOnly = true;
        this.dgvCommits.RowHeadersVisible = false;
        this.dgvCommits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvCommits.Size = new System.Drawing.Size(666, 236);
        this.dgvCommits.TabIndex = 1;
        //
        // lblCommitsHeader
        //
        this.lblCommitsHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblCommitsHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblCommitsHeader.Location = new System.Drawing.Point(0, 0);
        this.lblCommitsHeader.Name = "lblCommitsHeader";
        this.lblCommitsHeader.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        this.lblCommitsHeader.Size = new System.Drawing.Size(666, 24);
        this.lblCommitsHeader.TabIndex = 0;
        this.lblCommitsHeader.Text = "Commit History (Indexed):";
        //
        // dgvPushes
        //
        this.dgvPushes.AllowUserToAddRows = false;
        this.dgvPushes.AllowUserToDeleteRows = false;
        this.dgvPushes.AllowUserToResizeRows = false;
        this.dgvPushes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvPushes.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvPushes.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvPushes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvPushes.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvPushes.Location = new System.Drawing.Point(0, 24);
        this.dgvPushes.Name = "dgvPushes";
        this.dgvPushes.ReadOnly = true;
        this.dgvPushes.RowHeadersVisible = false;
        this.dgvPushes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvPushes.Size = new System.Drawing.Size(666, 278);
        this.dgvPushes.TabIndex = 1;
        //
        // lblPushesHeader
        //
        this.lblPushesHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblPushesHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblPushesHeader.Location = new System.Drawing.Point(0, 0);
        this.lblPushesHeader.Name = "lblPushesHeader";
        this.lblPushesHeader.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        this.lblPushesHeader.Size = new System.Drawing.Size(666, 24);
        this.lblPushesHeader.TabIndex = 0;
        this.lblPushesHeader.Text = "Push Ledger (Tamper-Proof Timestamps):";
        //
        // tabMetrics
        //
        this.tabMetrics.Controls.Add(this.pnlMetricsContent);
        this.tabMetrics.Location = new System.Drawing.Point(4, 24);
        this.tabMetrics.Name = "tabMetrics";
        this.tabMetrics.Padding = new System.Windows.Forms.Padding(3);
        this.tabMetrics.Size = new System.Drawing.Size(672, 572);
        this.tabMetrics.TabIndex = 2;
        this.tabMetrics.Text = "Code Metrics";
        this.tabMetrics.UseVisualStyleBackColor = true;
        //
        // pnlMetricsContent
        //
        this.pnlMetricsContent.AutoScroll = true;
        this.pnlMetricsContent.Controls.Add(this.flpLanguages);
        this.pnlMetricsContent.Controls.Add(this.lblLanguagesHeader);
        this.pnlMetricsContent.Controls.Add(this.tblMetricsSummary);
        this.pnlMetricsContent.Controls.Add(this.lblMetricsHeader);
        this.pnlMetricsContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlMetricsContent.Location = new System.Drawing.Point(3, 3);
        this.pnlMetricsContent.Name = "pnlMetricsContent";
        this.pnlMetricsContent.Padding = new System.Windows.Forms.Padding(16);
        this.pnlMetricsContent.Size = new System.Drawing.Size(666, 566);
        this.pnlMetricsContent.TabIndex = 0;
        //
        // flpLanguages
        //
        this.flpLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.flpLanguages.AutoScroll = true;
        this.flpLanguages.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.flpLanguages.Location = new System.Drawing.Point(16, 210);
        this.flpLanguages.Name = "flpLanguages";
        this.flpLanguages.Size = new System.Drawing.Size(634, 240);
        this.flpLanguages.TabIndex = 3;
        this.flpLanguages.WrapContents = false;
        //
        // lblLanguagesHeader
        //
        this.lblLanguagesHeader.AutoSize = true;
        this.lblLanguagesHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.lblLanguagesHeader.Location = new System.Drawing.Point(16, 184);
        this.lblLanguagesHeader.Name = "lblLanguagesHeader";
        this.lblLanguagesHeader.Size = new System.Drawing.Size(155, 19);
        this.lblLanguagesHeader.TabIndex = 2;
        this.lblLanguagesHeader.Text = "Language Breakdown";
        //
        // tblMetricsSummary
        //
        this.tblMetricsSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.tblMetricsSummary.ColumnCount = 4;
        this.tblMetricsSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblMetricsSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblMetricsSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblMetricsSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblMetricsSummary.Controls.Add(this.lblMetricTotalFilesTitle, 0, 0);
        this.tblMetricsSummary.Controls.Add(this.lblMetricTotalFilesVal, 0, 1);
        this.tblMetricsSummary.Controls.Add(this.lblMetricPrimaryLangTitle, 1, 0);
        this.tblMetricsSummary.Controls.Add(this.lblMetricPrimaryLangVal, 1, 1);
        this.tblMetricsSummary.Controls.Add(this.lblMetricSourceLocTitle, 2, 0);
        this.tblMetricsSummary.Controls.Add(this.lblMetricSourceLocVal, 2, 1);
        this.tblMetricsSummary.Controls.Add(this.lblMetricCommentLocTitle, 3, 0);
        this.tblMetricsSummary.Controls.Add(this.lblMetricCommentLocVal, 3, 1);
        this.tblMetricsSummary.Controls.Add(this.lblMetricBlankLocTitle, 0, 2);
        this.tblMetricsSummary.Controls.Add(this.lblMetricBlankLocVal, 0, 3);
        this.tblMetricsSummary.Controls.Add(this.lblMetricRatioTitle, 1, 2);
        this.tblMetricsSummary.Controls.Add(this.lblMetricRatioVal, 1, 3);
        this.tblMetricsSummary.Location = new System.Drawing.Point(16, 44);
        this.tblMetricsSummary.Name = "tblMetricsSummary";
        this.tblMetricsSummary.RowCount = 4;
        this.tblMetricsSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.tblMetricsSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this.tblMetricsSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.tblMetricsSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        this.tblMetricsSummary.Size = new System.Drawing.Size(634, 120);
        this.tblMetricsSummary.TabIndex = 1;
        //
        // lblMetricTotalFilesTitle
        //
        this.lblMetricTotalFilesTitle.AutoSize = true;
        this.lblMetricTotalFilesTitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetricTotalFilesTitle.Location = new System.Drawing.Point(3, 0);
        this.lblMetricTotalFilesTitle.Name = "lblMetricTotalFilesTitle";
        this.lblMetricTotalFilesTitle.Size = new System.Drawing.Size(59, 15);
        this.lblMetricTotalFilesTitle.TabIndex = 0;
        this.lblMetricTotalFilesTitle.Text = "Total Files";
        //
        // lblMetricTotalFilesVal
        //
        this.lblMetricTotalFilesVal.AutoSize = true;
        this.lblMetricTotalFilesVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblMetricTotalFilesVal.Location = new System.Drawing.Point(3, 20);
        this.lblMetricTotalFilesVal.Name = "lblMetricTotalFilesVal";
        this.lblMetricTotalFilesVal.Size = new System.Drawing.Size(19, 21);
        this.lblMetricTotalFilesVal.TabIndex = 1;
        this.lblMetricTotalFilesVal.Text = "0";
        //
        // lblMetricPrimaryLangTitle
        //
        this.lblMetricPrimaryLangTitle.AutoSize = true;
        this.lblMetricPrimaryLangTitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetricPrimaryLangTitle.Location = new System.Drawing.Point(161, 0);
        this.lblMetricPrimaryLangTitle.Name = "lblMetricPrimaryLangTitle";
        this.lblMetricPrimaryLangTitle.Size = new System.Drawing.Size(102, 15);
        this.lblMetricPrimaryLangTitle.TabIndex = 2;
        this.lblMetricPrimaryLangTitle.Text = "Primary Language";
        //
        // lblMetricPrimaryLangVal
        //
        this.lblMetricPrimaryLangVal.AutoSize = true;
        this.lblMetricPrimaryLangVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblMetricPrimaryLangVal.Location = new System.Drawing.Point(161, 20);
        this.lblMetricPrimaryLangVal.Name = "lblMetricPrimaryLangVal";
        this.lblMetricPrimaryLangVal.Size = new System.Drawing.Size(51, 21);
        this.lblMetricPrimaryLangVal.TabIndex = 3;
        this.lblMetricPrimaryLangVal.Text = "None";
        //
        // lblMetricSourceLocTitle
        //
        this.lblMetricSourceLocTitle.AutoSize = true;
        this.lblMetricSourceLocTitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetricSourceLocTitle.Location = new System.Drawing.Point(319, 0);
        this.lblMetricSourceLocTitle.Name = "lblMetricSourceLocTitle";
        this.lblMetricSourceLocTitle.Size = new System.Drawing.Size(65, 15);
        this.lblMetricSourceLocTitle.TabIndex = 4;
        this.lblMetricSourceLocTitle.Text = "Source LOC";
        //
        // lblMetricSourceLocVal
        //
        this.lblMetricSourceLocVal.AutoSize = true;
        this.lblMetricSourceLocVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblMetricSourceLocVal.Location = new System.Drawing.Point(319, 20);
        this.lblMetricSourceLocVal.Name = "lblMetricSourceLocVal";
        this.lblMetricSourceLocVal.Size = new System.Drawing.Size(19, 21);
        this.lblMetricSourceLocVal.TabIndex = 5;
        this.lblMetricSourceLocVal.Text = "0";
        //
        // lblMetricCommentLocTitle
        //
        this.lblMetricCommentLocTitle.AutoSize = true;
        this.lblMetricCommentLocTitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetricCommentLocTitle.Location = new System.Drawing.Point(477, 0);
        this.lblMetricCommentLocTitle.Name = "lblMetricCommentLocTitle";
        this.lblMetricCommentLocTitle.Size = new System.Drawing.Size(83, 15);
        this.lblMetricCommentLocTitle.TabIndex = 6;
        this.lblMetricCommentLocTitle.Text = "Comment LOC";
        //
        // lblMetricCommentLocVal
        //
        this.lblMetricCommentLocVal.AutoSize = true;
        this.lblMetricCommentLocVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblMetricCommentLocVal.Location = new System.Drawing.Point(477, 20);
        this.lblMetricCommentLocVal.Name = "lblMetricCommentLocVal";
        this.lblMetricCommentLocVal.Size = new System.Drawing.Size(19, 21);
        this.lblMetricCommentLocVal.TabIndex = 7;
        this.lblMetricCommentLocVal.Text = "0";
        //
        // lblMetricBlankLocTitle
        //
        this.lblMetricBlankLocTitle.AutoSize = true;
        this.lblMetricBlankLocTitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetricBlankLocTitle.Location = new System.Drawing.Point(3, 56);
        this.lblMetricBlankLocTitle.Name = "lblMetricBlankLocTitle";
        this.lblMetricBlankLocTitle.Size = new System.Drawing.Size(58, 15);
        this.lblMetricBlankLocTitle.TabIndex = 8;
        this.lblMetricBlankLocTitle.Text = "Blank LOC";
        //
        // lblMetricBlankLocVal
        //
        this.lblMetricBlankLocVal.AutoSize = true;
        this.lblMetricBlankLocVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblMetricBlankLocVal.Location = new System.Drawing.Point(3, 76);
        this.lblMetricBlankLocVal.Name = "lblMetricBlankLocVal";
        this.lblMetricBlankLocVal.Size = new System.Drawing.Size(19, 21);
        this.lblMetricBlankLocVal.TabIndex = 9;
        this.lblMetricBlankLocVal.Text = "0";
        //
        // lblMetricRatioTitle
        //
        this.lblMetricRatioTitle.AutoSize = true;
        this.lblMetricRatioTitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetricRatioTitle.Location = new System.Drawing.Point(161, 56);
        this.lblMetricRatioTitle.Name = "lblMetricRatioTitle";
        this.lblMetricRatioTitle.Size = new System.Drawing.Size(89, 15);
        this.lblMetricRatioTitle.TabIndex = 10;
        this.lblMetricRatioTitle.Text = "Comment Ratio";
        //
        // lblMetricRatioVal
        //
        this.lblMetricRatioVal.AutoSize = true;
        this.lblMetricRatioVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblMetricRatioVal.Location = new System.Drawing.Point(161, 76);
        this.lblMetricRatioVal.Name = "lblMetricRatioVal";
        this.lblMetricRatioVal.Size = new System.Drawing.Size(40, 21);
        this.lblMetricRatioVal.TabIndex = 11;
        this.lblMetricRatioVal.Text = "N/A";
        //
        // lblMetricsHeader
        //
        this.lblMetricsHeader.AutoSize = true;
        this.lblMetricsHeader.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        this.lblMetricsHeader.Location = new System.Drawing.Point(16, 16);
        this.lblMetricsHeader.Name = "lblMetricsHeader";
        this.lblMetricsHeader.Size = new System.Drawing.Size(227, 20);
        this.lblMetricsHeader.TabIndex = 0;
        this.lblMetricsHeader.Text = "Static Code Metrics Summary";
        //
        // tabTeam
        //
        this.tabTeam.Controls.Add(this.splitTeam);
        this.tabTeam.Location = new System.Drawing.Point(4, 24);
        this.tabTeam.Name = "tabTeam";
        this.tabTeam.Padding = new System.Windows.Forms.Padding(3);
        this.tabTeam.Size = new System.Drawing.Size(672, 572);
        this.tabTeam.TabIndex = 3;
        this.tabTeam.Text = "Team & Tickets";
        this.tabTeam.UseVisualStyleBackColor = true;
        //
        // splitTeam
        //
        this.splitTeam.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitTeam.Location = new System.Drawing.Point(3, 3);
        this.splitTeam.Name = "splitTeam";
        this.splitTeam.Orientation = System.Windows.Forms.Orientation.Horizontal;
        //
        // splitTeam.Panel1
        //
        this.splitTeam.Panel1.Controls.Add(this.dgvContributors);
        this.splitTeam.Panel1.Controls.Add(this.pnlTeamActions);
        this.splitTeam.Panel1MinSize = 120;
        //
        // splitTeam.Panel2
        //
        this.splitTeam.Panel2.Controls.Add(this.dgvDiscussions);
        this.splitTeam.Panel2.Controls.Add(this.pnlCommentInput);
        this.splitTeam.Panel2.Controls.Add(this.lblDiscussionHeader);
        this.splitTeam.Panel2MinSize = 120;
        this.splitTeam.Size = new System.Drawing.Size(666, 566);
        this.splitTeam.SplitterDistance = 260;
        this.splitTeam.TabIndex = 0;
        //
        // dgvContributors
        //
        this.dgvContributors.AllowUserToAddRows = false;
        this.dgvContributors.AllowUserToDeleteRows = false;
        this.dgvContributors.AllowUserToResizeRows = false;
        this.dgvContributors.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvContributors.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvContributors.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvContributors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvContributors.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvContributors.Location = new System.Drawing.Point(0, 36);
        this.dgvContributors.Name = "dgvContributors";
        this.dgvContributors.ReadOnly = true;
        this.dgvContributors.RowHeadersVisible = false;
        this.dgvContributors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvContributors.Size = new System.Drawing.Size(666, 224);
        this.dgvContributors.TabIndex = 1;
        //
        // pnlTeamActions
        //
        this.pnlTeamActions.Controls.Add(this.btnGrantPeer);
        this.pnlTeamActions.Controls.Add(this.btnRequestTicket);
        this.pnlTeamActions.Controls.Add(this.lblTeamHeader);
        this.pnlTeamActions.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlTeamActions.Location = new System.Drawing.Point(0, 0);
        this.pnlTeamActions.Name = "pnlTeamActions";
        this.pnlTeamActions.Padding = new System.Windows.Forms.Padding(4);
        this.pnlTeamActions.Size = new System.Drawing.Size(666, 36);
        this.pnlTeamActions.TabIndex = 0;
        //
        // btnGrantPeer
        //
        this.btnGrantPeer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnGrantPeer.Location = new System.Drawing.Point(522, 4);
        this.btnGrantPeer.Name = "btnGrantPeer";
        this.btnGrantPeer.Size = new System.Drawing.Size(140, 26);
        this.btnGrantPeer.TabIndex = 2;
        this.btnGrantPeer.Text = "Grant Peer Viewer...";
        this.btnGrantPeer.UseVisualStyleBackColor = true;
        this.btnGrantPeer.Click += new System.EventHandler(this.btnGrantPeer_Click);
        //
        // btnRequestTicket
        //
        this.btnRequestTicket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnRequestTicket.Location = new System.Drawing.Point(346, 4);
        this.btnRequestTicket.Name = "btnRequestTicket";
        this.btnRequestTicket.Size = new System.Drawing.Size(170, 26);
        this.btnRequestTicket.TabIndex = 1;
        this.btnRequestTicket.Text = "Request Groupmate Ticket...";
        this.btnRequestTicket.UseVisualStyleBackColor = true;
        this.btnRequestTicket.Click += new System.EventHandler(this.btnRequestTicket_Click);
        //
        // lblTeamHeader
        //
        this.lblTeamHeader.AutoSize = true;
        this.lblTeamHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblTeamHeader.Location = new System.Drawing.Point(4, 10);
        this.lblTeamHeader.Name = "lblTeamHeader";
        this.lblTeamHeader.Size = new System.Drawing.Size(183, 15);
        this.lblTeamHeader.TabIndex = 0;
        this.lblTeamHeader.Text = "Contributors & Access Tickets:";
        //
        // dgvDiscussions
        //
        this.dgvDiscussions.AllowUserToAddRows = false;
        this.dgvDiscussions.AllowUserToDeleteRows = false;
        this.dgvDiscussions.AllowUserToResizeRows = false;
        this.dgvDiscussions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvDiscussions.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvDiscussions.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvDiscussions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvDiscussions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvDiscussions.Location = new System.Drawing.Point(0, 24);
        this.dgvDiscussions.Name = "dgvDiscussions";
        this.dgvDiscussions.ReadOnly = true;
        this.dgvDiscussions.RowHeadersVisible = false;
        this.dgvDiscussions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvDiscussions.Size = new System.Drawing.Size(666, 218);
        this.dgvDiscussions.TabIndex = 1;
        //
        // pnlCommentInput
        //
        this.pnlCommentInput.Controls.Add(this.btnPostComment);
        this.pnlCommentInput.Controls.Add(this.txtComment);
        this.pnlCommentInput.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlCommentInput.Location = new System.Drawing.Point(0, 242);
        this.pnlCommentInput.Name = "pnlCommentInput";
        this.pnlCommentInput.Padding = new System.Windows.Forms.Padding(4);
        this.pnlCommentInput.Size = new System.Drawing.Size(666, 60);
        this.pnlCommentInput.TabIndex = 2;
        //
        // btnPostComment
        //
        this.btnPostComment.Dock = System.Windows.Forms.DockStyle.Right;
        this.btnPostComment.Location = new System.Drawing.Point(582, 4);
        this.btnPostComment.Name = "btnPostComment";
        this.btnPostComment.Size = new System.Drawing.Size(80, 52);
        this.btnPostComment.TabIndex = 1;
        this.btnPostComment.Text = "Post";
        this.btnPostComment.UseVisualStyleBackColor = true;
        this.btnPostComment.Click += new System.EventHandler(this.btnPostComment_Click);
        //
        // txtComment
        //
        this.txtComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.txtComment.Location = new System.Drawing.Point(4, 4);
        this.txtComment.Multiline = true;
        this.txtComment.Name = "txtComment";
        this.txtComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtComment.Size = new System.Drawing.Size(572, 52);
        this.txtComment.TabIndex = 0;
        //
        // lblDiscussionHeader
        //
        this.lblDiscussionHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblDiscussionHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblDiscussionHeader.Location = new System.Drawing.Point(0, 0);
        this.lblDiscussionHeader.Name = "lblDiscussionHeader";
        this.lblDiscussionHeader.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        this.lblDiscussionHeader.Size = new System.Drawing.Size(666, 24);
        this.lblDiscussionHeader.TabIndex = 0;
        this.lblDiscussionHeader.Text = "Peer Discussion:";
        //
        // tabRubric
        //
        this.tabRubric.Controls.Add(this.pnlRubricContent);
        this.tabRubric.Location = new System.Drawing.Point(4, 24);
        this.tabRubric.Name = "tabRubric";
        this.tabRubric.Padding = new System.Windows.Forms.Padding(3);
        this.tabRubric.Size = new System.Drawing.Size(672, 572);
        this.tabRubric.TabIndex = 4;
        this.tabRubric.Text = "Rubric Evaluation";
        this.tabRubric.UseVisualStyleBackColor = true;
        //
        // pnlRubricContent
        //
        this.pnlRubricContent.AutoScroll = true;
        this.pnlRubricContent.Controls.Add(this.lblLockedWarning);
        this.pnlRubricContent.Controls.Add(this.rtbFacultyFeedback);
        this.pnlRubricContent.Controls.Add(this.lblCritiqueHeader);
        this.pnlRubricContent.Controls.Add(this.tblRubricScores);
        this.pnlRubricContent.Controls.Add(this.lblRubricTotal);
        this.pnlRubricContent.Controls.Add(this.lblRubricStatus);
        this.pnlRubricContent.Controls.Add(this.lblRubricTitle);
        this.pnlRubricContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlRubricContent.Location = new System.Drawing.Point(3, 3);
        this.pnlRubricContent.Name = "pnlRubricContent";
        this.pnlRubricContent.Padding = new System.Windows.Forms.Padding(20);
        this.pnlRubricContent.Size = new System.Drawing.Size(666, 566);
        this.pnlRubricContent.TabIndex = 0;
        //
        // lblLockedWarning
        //
        this.lblLockedWarning.AutoSize = true;
        this.lblLockedWarning.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
        this.lblLockedWarning.ForeColor = System.Drawing.Color.Crimson;
        this.lblLockedWarning.Location = new System.Drawing.Point(20, 420);
        this.lblLockedWarning.Name = "lblLockedWarning";
        this.lblLockedWarning.Size = new System.Drawing.Size(434, 17);
        this.lblLockedWarning.TabIndex = 6;
        this.lblLockedWarning.Text = "Repository is locked by faculty. No further pushes can be accepted.";
        this.lblLockedWarning.Visible = false;
        //
        // rtbFacultyFeedback
        //
        this.rtbFacultyFeedback.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.rtbFacultyFeedback.BackColor = System.Drawing.SystemColors.Window;
        this.rtbFacultyFeedback.Location = new System.Drawing.Point(20, 260);
        this.rtbFacultyFeedback.Name = "rtbFacultyFeedback";
        this.rtbFacultyFeedback.ReadOnly = true;
        this.rtbFacultyFeedback.Size = new System.Drawing.Size(626, 140);
        this.rtbFacultyFeedback.TabIndex = 5;
        this.rtbFacultyFeedback.Text = "";
        //
        // lblCritiqueHeader
        //
        this.lblCritiqueHeader.AutoSize = true;
        this.lblCritiqueHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.lblCritiqueHeader.Location = new System.Drawing.Point(20, 234);
        this.lblCritiqueHeader.Name = "lblCritiqueHeader";
        this.lblCritiqueHeader.Size = new System.Drawing.Size(199, 19);
        this.lblCritiqueHeader.TabIndex = 4;
        this.lblCritiqueHeader.Text = "Faculty Critique & Feedback:";
        //
        // tblRubricScores
        //
        this.tblRubricScores.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.tblRubricScores.ColumnCount = 2;
        this.tblRubricScores.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
        this.tblRubricScores.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
        this.tblRubricScores.Controls.Add(this.lblArchTitle, 0, 0);
        this.tblRubricScores.Controls.Add(this.lblArchScore, 1, 0);
        this.tblRubricScores.Controls.Add(this.lblFuncTitle, 0, 1);
        this.tblRubricScores.Controls.Add(this.lblFuncScore, 1, 1);
        this.tblRubricScores.Controls.Add(this.lblQualTitle, 0, 2);
        this.tblRubricScores.Controls.Add(this.lblQualScore, 1, 2);
        this.tblRubricScores.Controls.Add(this.lblDocTitle, 0, 3);
        this.tblRubricScores.Controls.Add(this.lblDocScore, 1, 3);
        this.tblRubricScores.Location = new System.Drawing.Point(20, 110);
        this.tblRubricScores.Name = "tblRubricScores";
        this.tblRubricScores.RowCount = 4;
        this.tblRubricScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this.tblRubricScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this.tblRubricScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this.tblRubricScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
        this.tblRubricScores.Size = new System.Drawing.Size(626, 108);
        this.tblRubricScores.TabIndex = 3;
        //
        // lblArchTitle
        //
        this.lblArchTitle.AutoSize = true;
        this.lblArchTitle.Location = new System.Drawing.Point(3, 0);
        this.lblArchTitle.Name = "lblArchTitle";
        this.lblArchTitle.Size = new System.Drawing.Size(155, 15);
        this.lblArchTitle.TabIndex = 0;
        this.lblArchTitle.Text = "Architecture & Design (0-25)";
        //
        // lblArchScore
        //
        this.lblArchScore.AutoSize = true;
        this.lblArchScore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblArchScore.Location = new System.Drawing.Point(441, 0);
        this.lblArchScore.Name = "lblArchScore";
        this.lblArchScore.Size = new System.Drawing.Size(43, 15);
        this.lblArchScore.TabIndex = 1;
        this.lblArchScore.Text = "-- / 25";
        //
        // lblFuncTitle
        //
        this.lblFuncTitle.AutoSize = true;
        this.lblFuncTitle.Location = new System.Drawing.Point(3, 26);
        this.lblFuncTitle.Name = "lblFuncTitle";
        this.lblFuncTitle.Size = new System.Drawing.Size(180, 15);
        this.lblFuncTitle.TabIndex = 2;
        this.lblFuncTitle.Text = "Functionality & Correctness (0-25)";
        //
        // lblFuncScore
        //
        this.lblFuncScore.AutoSize = true;
        this.lblFuncScore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblFuncScore.Location = new System.Drawing.Point(441, 26);
        this.lblFuncScore.Name = "lblFuncScore";
        this.lblFuncScore.Size = new System.Drawing.Size(43, 15);
        this.lblFuncScore.TabIndex = 3;
        this.lblFuncScore.Text = "-- / 25";
        //
        // lblQualTitle
        //
        this.lblQualTitle.AutoSize = true;
        this.lblQualTitle.Location = new System.Drawing.Point(3, 52);
        this.lblQualTitle.Name = "lblQualTitle";
        this.lblQualTitle.Size = new System.Drawing.Size(161, 15);
        this.lblQualTitle.TabIndex = 4;
        this.lblQualTitle.Text = "Code Quality & Cleanliness (0-25)";
        //
        // lblQualScore
        //
        this.lblQualScore.AutoSize = true;
        this.lblQualScore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblQualScore.Location = new System.Drawing.Point(441, 52);
        this.lblQualScore.Name = "lblQualScore";
        this.lblQualScore.Size = new System.Drawing.Size(43, 15);
        this.lblQualScore.TabIndex = 5;
        this.lblQualScore.Text = "-- / 25";
        //
        // lblDocTitle
        //
        this.lblDocTitle.AutoSize = true;
        this.lblDocTitle.Location = new System.Drawing.Point(3, 78);
        this.lblDocTitle.Name = "lblDocTitle";
        this.lblDocTitle.Size = new System.Drawing.Size(169, 15);
        this.lblDocTitle.TabIndex = 6;
        this.lblDocTitle.Text = "Documentation & Readme (0-25)";
        //
        // lblDocScore
        //
        this.lblDocScore.AutoSize = true;
        this.lblDocScore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblDocScore.Location = new System.Drawing.Point(441, 78);
        this.lblDocScore.Name = "lblDocScore";
        this.lblDocScore.Size = new System.Drawing.Size(43, 15);
        this.lblDocScore.TabIndex = 7;
        this.lblDocScore.Text = "-- / 25";
        //
        // lblRubricTotal
        //
        this.lblRubricTotal.AutoSize = true;
        this.lblRubricTotal.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        this.lblRubricTotal.Location = new System.Drawing.Point(20, 72);
        this.lblRubricTotal.Name = "lblRubricTotal";
        this.lblRubricTotal.Size = new System.Drawing.Size(189, 25);
        this.lblRubricTotal.TabIndex = 2;
        this.lblRubricTotal.Text = "Total Score: -- / 100";
        //
        // lblRubricStatus
        //
        this.lblRubricStatus.AutoSize = true;
        this.lblRubricStatus.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblRubricStatus.Location = new System.Drawing.Point(20, 48);
        this.lblRubricStatus.Name = "lblRubricStatus";
        this.lblRubricStatus.Size = new System.Drawing.Size(139, 15);
        this.lblRubricStatus.TabIndex = 1;
        this.lblRubricStatus.Text = "Status: Not Evaluated Yet";
        //
        // lblRubricTitle
        //
        this.lblRubricTitle.AutoSize = true;
        this.lblRubricTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblRubricTitle.Location = new System.Drawing.Point(20, 20);
        this.lblRubricTitle.Name = "lblRubricTitle";
        this.lblRubricTitle.Size = new System.Drawing.Size(206, 21);
        this.lblRubricTitle.TabIndex = 0;
        this.lblRubricTitle.Text = "Faculty Rubric Evaluation";
        //
        // StudentWorkspaceControl
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this.splitMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "StudentWorkspaceControl";
        this.Size = new System.Drawing.Size(1024, 600);
        this.splitMain.Panel1.ResumeLayout(false);
        this.splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
        this.splitMain.ResumeLayout(false);
        this.tabSidebar.ResumeLayout(false);
        this.tabSubmissions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvMyRepos)).EndInit();
        this.pnlSubmissionsBottom.ResumeLayout(false);
        this.tabAssignedTasks.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvCourseTasks)).EndInit();
        this.pnlTasksBottom.ResumeLayout(false);
        this.pnlCloneBar.ResumeLayout(false);
        this.pnlCloneBar.PerformLayout();
        this.pnlCourseFilter.ResumeLayout(false);
        this.pnlCourseFilter.PerformLayout();
        this.tabDetails.ResumeLayout(false);
        this.tabCode.ResumeLayout(false);
        this.splitCode.Panel1.ResumeLayout(false);
        this.splitCode.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitCode)).EndInit();
        this.splitCode.ResumeLayout(false);
        this.pnlCodeHeader.ResumeLayout(false);
        this.pnlCodeHeader.PerformLayout();
        this.tabHistory.ResumeLayout(false);
        this.splitHistory.Panel1.ResumeLayout(false);
        this.splitHistory.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitHistory)).EndInit();
        this.splitHistory.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvCommits)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPushes)).EndInit();
        this.tabMetrics.ResumeLayout(false);
        this.pnlMetricsContent.ResumeLayout(false);
        this.pnlMetricsContent.PerformLayout();
        this.tblMetricsSummary.ResumeLayout(false);
        this.tblMetricsSummary.PerformLayout();
        this.tabTeam.ResumeLayout(false);
        this.splitTeam.Panel1.ResumeLayout(false);
        this.splitTeam.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitTeam)).EndInit();
        this.splitTeam.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvContributors)).EndInit();
        this.pnlTeamActions.ResumeLayout(false);
        this.pnlTeamActions.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDiscussions)).EndInit();
        this.pnlCommentInput.ResumeLayout(false);
        this.pnlCommentInput.PerformLayout();
        this.tabRubric.ResumeLayout(false);
        this.pnlRubricContent.ResumeLayout(false);
        this.pnlRubricContent.PerformLayout();
        this.tblRubricScores.ResumeLayout(false);
        this.tblRubricScores.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.SplitContainer splitMain;
    private System.Windows.Forms.Panel pnlCourseFilter;
    private System.Windows.Forms.Label lblCourseFilter;
    private System.Windows.Forms.ComboBox cboCourses;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.TabControl tabSidebar;
    private System.Windows.Forms.TabPage tabSubmissions;
    private System.Windows.Forms.DataGridView dgvMyRepos;
    private System.Windows.Forms.Panel pnlSubmissionsBottom;
    private System.Windows.Forms.Button btnToggleSubmission;
    private System.Windows.Forms.TabPage tabAssignedTasks;
    private System.Windows.Forms.DataGridView dgvCourseTasks;
    private System.Windows.Forms.Panel pnlTasksBottom;
    private System.Windows.Forms.Button btnStartTask;
    private System.Windows.Forms.Panel pnlCloneBar;
    private System.Windows.Forms.Label lblCloneUrl;
    private System.Windows.Forms.TextBox txtCloneUrl;
    private System.Windows.Forms.Button btnCopyClone;
    private System.Windows.Forms.TabControl tabDetails;
    private System.Windows.Forms.TabPage tabCode;
    private System.Windows.Forms.SplitContainer splitCode;
    private System.Windows.Forms.TreeView tvFiles;
    private System.Windows.Forms.Panel pnlCodeHeader;
    private System.Windows.Forms.Label lblFilePath;
    private System.Windows.Forms.Label lblFileSize;
    private System.Windows.Forms.RichTextBox rtbCode;
    private System.Windows.Forms.TabPage tabHistory;
    private System.Windows.Forms.SplitContainer splitHistory;
    private System.Windows.Forms.Label lblCommitsHeader;
    private System.Windows.Forms.DataGridView dgvCommits;
    private System.Windows.Forms.Label lblPushesHeader;
    private System.Windows.Forms.DataGridView dgvPushes;
    private System.Windows.Forms.TabPage tabMetrics;
    private System.Windows.Forms.Panel pnlMetricsContent;
    private System.Windows.Forms.Label lblMetricsHeader;
    private System.Windows.Forms.TableLayoutPanel tblMetricsSummary;
    private System.Windows.Forms.Label lblMetricTotalFilesTitle;
    private System.Windows.Forms.Label lblMetricTotalFilesVal;
    private System.Windows.Forms.Label lblMetricPrimaryLangTitle;
    private System.Windows.Forms.Label lblMetricPrimaryLangVal;
    private System.Windows.Forms.Label lblMetricSourceLocTitle;
    private System.Windows.Forms.Label lblMetricSourceLocVal;
    private System.Windows.Forms.Label lblMetricCommentLocTitle;
    private System.Windows.Forms.Label lblMetricCommentLocVal;
    private System.Windows.Forms.Label lblMetricBlankLocTitle;
    private System.Windows.Forms.Label lblMetricBlankLocVal;
    private System.Windows.Forms.Label lblMetricRatioTitle;
    private System.Windows.Forms.Label lblMetricRatioVal;
    private System.Windows.Forms.Label lblLanguagesHeader;
    private System.Windows.Forms.FlowLayoutPanel flpLanguages;
    private System.Windows.Forms.TabPage tabTeam;
    private System.Windows.Forms.SplitContainer splitTeam;
    private System.Windows.Forms.Panel pnlTeamActions;
    private System.Windows.Forms.Label lblTeamHeader;
    private System.Windows.Forms.Button btnRequestTicket;
    private System.Windows.Forms.Button btnGrantPeer;
    private System.Windows.Forms.DataGridView dgvContributors;
    private System.Windows.Forms.Label lblDiscussionHeader;
    private System.Windows.Forms.DataGridView dgvDiscussions;
    private System.Windows.Forms.Panel pnlCommentInput;
    private System.Windows.Forms.TextBox txtComment;
    private System.Windows.Forms.Button btnPostComment;
    private System.Windows.Forms.TabPage tabRubric;
    private System.Windows.Forms.Panel pnlRubricContent;
    private System.Windows.Forms.Label lblRubricTitle;
    private System.Windows.Forms.Label lblRubricStatus;
    private System.Windows.Forms.Label lblRubricTotal;
    private System.Windows.Forms.TableLayoutPanel tblRubricScores;
    private System.Windows.Forms.Label lblArchTitle;
    private System.Windows.Forms.Label lblArchScore;
    private System.Windows.Forms.Label lblFuncTitle;
    private System.Windows.Forms.Label lblFuncScore;
    private System.Windows.Forms.Label lblQualTitle;
    private System.Windows.Forms.Label lblQualScore;
    private System.Windows.Forms.Label lblDocTitle;
    private System.Windows.Forms.Label lblDocScore;
    private System.Windows.Forms.Label lblCritiqueHeader;
    private System.Windows.Forms.RichTextBox rtbFacultyFeedback;
    private System.Windows.Forms.Label lblLockedWarning;
}
