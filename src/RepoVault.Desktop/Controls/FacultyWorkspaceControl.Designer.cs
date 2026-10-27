namespace RepoVault.Desktop.Controls;

partial class FacultyWorkspaceControl
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
        this.dgvCourses = new System.Windows.Forms.DataGridView();
        this.pnlCoursesBottom = new System.Windows.Forms.FlowLayoutPanel();
        this.btnDeleteCourse = new System.Windows.Forms.Button();
        this.btnEditCourse = new System.Windows.Forms.Button();
        this.pnlCoursesTop = new System.Windows.Forms.Panel();
        this.btnRepoBin = new System.Windows.Forms.Button();
        this.btnNewCourse = new System.Windows.Forms.Button();
        this.lblCourseCatalog = new System.Windows.Forms.Label();
        this.tabFaculty = new System.Windows.Forms.TabControl();
        this.tabSubmissions = new System.Windows.Forms.TabPage();
        this.dgvSubmissions = new System.Windows.Forms.DataGridView();
        this.pnlSubmissionsActions = new System.Windows.Forms.FlowLayoutPanel();
        this.btnGradeRubric = new System.Windows.Forms.Button();
        this.btnToggleLock = new System.Windows.Forms.Button();
        this.btnEditRepo = new System.Windows.Forms.Button();
        this.btnTrashRepo = new System.Windows.Forms.Button();
        this.btnCopyClone = new System.Windows.Forms.Button();
        this.tabTasks = new System.Windows.Forms.TabPage();
        this.dgvTasks = new System.Windows.Forms.DataGridView();
        this.pnlTasksHeader = new System.Windows.Forms.Panel();
        this.btnNewTask = new System.Windows.Forms.Button();
        this.lblTasksHeader = new System.Windows.Forms.Label();
        this.tabTickets = new System.Windows.Forms.TabPage();
        this.dgvTickets = new System.Windows.Forms.DataGridView();
        this.pnlTicketsHeader = new System.Windows.Forms.Panel();
        this.btnRejectTicket = new System.Windows.Forms.Button();
        this.btnApproveTicket = new System.Windows.Forms.Button();
        this.lblTicketsHeader = new System.Windows.Forms.Label();
        this.tabInspect = new System.Windows.Forms.TabPage();
        this.splitInspect = new System.Windows.Forms.SplitContainer();
        this.tvFiles = new System.Windows.Forms.TreeView();
        this.splitInspectRight = new System.Windows.Forms.SplitContainer();
        this.rtbCode = new System.Windows.Forms.RichTextBox();
        this.pnlCodeHeader = new System.Windows.Forms.Panel();
        this.lblFileSize = new System.Windows.Forms.Label();
        this.lblFilePath = new System.Windows.Forms.Label();
        this.splitHistory = new System.Windows.Forms.SplitContainer();
        this.dgvCommits = new System.Windows.Forms.DataGridView();
        this.lblCommitsHeader = new System.Windows.Forms.Label();
        this.dgvPushes = new System.Windows.Forms.DataGridView();
        this.lblPushesHeader = new System.Windows.Forms.Label();
        this.tabDiscussions = new System.Windows.Forms.TabPage();
        this.dgvDiscussions = new System.Windows.Forms.DataGridView();
        this.pnlCommentInput = new System.Windows.Forms.Panel();
        this.btnPostComment = new System.Windows.Forms.Button();
        this.txtComment = new System.Windows.Forms.TextBox();
        this.pnlDiscussionActions = new System.Windows.Forms.Panel();
        this.btnToggleModeration = new System.Windows.Forms.Button();
        this.lblDiscussionHeader = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
        this.splitMain.Panel1.SuspendLayout();
        this.splitMain.Panel2.SuspendLayout();
        this.splitMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCourses)).BeginInit();
        this.pnlCoursesBottom.SuspendLayout();
        this.pnlCoursesTop.SuspendLayout();
        this.tabFaculty.SuspendLayout();
        this.tabSubmissions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvSubmissions)).BeginInit();
        this.pnlSubmissionsActions.SuspendLayout();
        this.tabTasks.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).BeginInit();
        this.pnlTasksHeader.SuspendLayout();
        this.tabTickets.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvTickets)).BeginInit();
        this.pnlTicketsHeader.SuspendLayout();
        this.tabInspect.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitInspect)).BeginInit();
        this.splitInspect.Panel1.SuspendLayout();
        this.splitInspect.Panel2.SuspendLayout();
        this.splitInspect.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitInspectRight)).BeginInit();
        this.splitInspectRight.Panel1.SuspendLayout();
        this.splitInspectRight.Panel2.SuspendLayout();
        this.splitInspectRight.SuspendLayout();
        this.pnlCodeHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitHistory)).BeginInit();
        this.splitHistory.Panel1.SuspendLayout();
        this.splitHistory.Panel2.SuspendLayout();
        this.splitHistory.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCommits)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPushes)).BeginInit();
        this.tabDiscussions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDiscussions)).BeginInit();
        this.pnlCommentInput.SuspendLayout();
        this.pnlDiscussionActions.SuspendLayout();
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
        this.splitMain.Panel1.Controls.Add(this.dgvCourses);
        this.splitMain.Panel1.Controls.Add(this.pnlCoursesBottom);
        this.splitMain.Panel1.Controls.Add(this.pnlCoursesTop);
        this.splitMain.Panel1MinSize = 280;
        //
        // splitMain.Panel2
        //
        this.splitMain.Panel2.Controls.Add(this.tabFaculty);
        this.splitMain.Panel2MinSize = 400;
        this.splitMain.Size = new System.Drawing.Size(1024, 600);
        this.splitMain.SplitterDistance = 320;
        this.splitMain.TabIndex = 0;
        //
        // dgvCourses
        //
        this.dgvCourses.AllowUserToAddRows = false;
        this.dgvCourses.AllowUserToDeleteRows = false;
        this.dgvCourses.AllowUserToResizeRows = false;
        this.dgvCourses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvCourses.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvCourses.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvCourses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvCourses.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvCourses.Location = new System.Drawing.Point(0, 48);
        this.dgvCourses.MultiSelect = false;
        this.dgvCourses.Name = "dgvCourses";
        this.dgvCourses.ReadOnly = true;
        this.dgvCourses.RowHeadersVisible = false;
        this.dgvCourses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvCourses.Size = new System.Drawing.Size(320, 516);
        this.dgvCourses.TabIndex = 1;
        this.dgvCourses.SelectionChanged += new System.EventHandler(this.dgvCourses_SelectionChanged);
        //
        // pnlCoursesBottom
        //
        this.pnlCoursesBottom.Controls.Add(this.btnDeleteCourse);
        this.pnlCoursesBottom.Controls.Add(this.btnEditCourse);
        this.pnlCoursesBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlCoursesBottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlCoursesBottom.Location = new System.Drawing.Point(0, 564);
        this.pnlCoursesBottom.Name = "pnlCoursesBottom";
        this.pnlCoursesBottom.Padding = new System.Windows.Forms.Padding(4);
        this.pnlCoursesBottom.Size = new System.Drawing.Size(320, 36);
        this.pnlCoursesBottom.TabIndex = 2;
        //
        // btnDeleteCourse
        //
        this.btnDeleteCourse.Location = new System.Drawing.Point(212, 4);
        this.btnDeleteCourse.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
        this.btnDeleteCourse.Name = "btnDeleteCourse";
        this.btnDeleteCourse.Size = new System.Drawing.Size(100, 26);
        this.btnDeleteCourse.TabIndex = 1;
        this.btnDeleteCourse.Text = "Delete Course";
        this.btnDeleteCourse.UseVisualStyleBackColor = true;
        this.btnDeleteCourse.Click += new System.EventHandler(this.btnDeleteCourse_Click);
        //
        // btnEditCourse
        //
        this.btnEditCourse.Location = new System.Drawing.Point(109, 4);
        this.btnEditCourse.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
        this.btnEditCourse.Name = "btnEditCourse";
        this.btnEditCourse.Size = new System.Drawing.Size(100, 26);
        this.btnEditCourse.TabIndex = 0;
        this.btnEditCourse.Text = "Edit Course...";
        this.btnEditCourse.UseVisualStyleBackColor = true;
        this.btnEditCourse.Click += new System.EventHandler(this.btnEditCourse_Click);
        //
        // pnlCoursesTop
        //
        this.pnlCoursesTop.Controls.Add(this.btnRepoBin);
        this.pnlCoursesTop.Controls.Add(this.btnNewCourse);
        this.pnlCoursesTop.Controls.Add(this.lblCourseCatalog);
        this.pnlCoursesTop.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlCoursesTop.Location = new System.Drawing.Point(0, 0);
        this.pnlCoursesTop.Name = "pnlCoursesTop";
        this.pnlCoursesTop.Padding = new System.Windows.Forms.Padding(8, 8, 8, 4);
        this.pnlCoursesTop.Size = new System.Drawing.Size(320, 48);
        this.pnlCoursesTop.TabIndex = 0;
        //
        // btnRepoBin
        //
        this.btnRepoBin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnRepoBin.Location = new System.Drawing.Point(134, 10);
        this.btnRepoBin.Name = "btnRepoBin";
        this.btnRepoBin.Size = new System.Drawing.Size(82, 26);
        this.btnRepoBin.TabIndex = 1;
        this.btnRepoBin.Text = "Repo Bin...";
        this.btnRepoBin.UseVisualStyleBackColor = true;
        this.btnRepoBin.Click += new System.EventHandler(this.btnRepoBin_Click);
        //
        // btnNewCourse
        //
        this.btnNewCourse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnNewCourse.Location = new System.Drawing.Point(222, 10);
        this.btnNewCourse.Name = "btnNewCourse";
        this.btnNewCourse.Size = new System.Drawing.Size(92, 26);
        this.btnNewCourse.TabIndex = 2;
        this.btnNewCourse.Text = "+ New Course";
        this.btnNewCourse.UseVisualStyleBackColor = true;
        this.btnNewCourse.Click += new System.EventHandler(this.btnNewCourse_Click);
        //
        // lblCourseCatalog
        //
        this.lblCourseCatalog.AutoSize = true;
        this.lblCourseCatalog.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
        this.lblCourseCatalog.Location = new System.Drawing.Point(8, 13);
        this.lblCourseCatalog.Name = "lblCourseCatalog";
        this.lblCourseCatalog.Size = new System.Drawing.Size(61, 19);
        this.lblCourseCatalog.TabIndex = 0;
        this.lblCourseCatalog.Text = "Courses";
        //
        // tabFaculty
        //
        this.tabFaculty.Controls.Add(this.tabSubmissions);
        this.tabFaculty.Controls.Add(this.tabTasks);
        this.tabFaculty.Controls.Add(this.tabTickets);
        this.tabFaculty.Controls.Add(this.tabInspect);
        this.tabFaculty.Controls.Add(this.tabDiscussions);
        this.tabFaculty.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabFaculty.Location = new System.Drawing.Point(0, 0);
        this.tabFaculty.Name = "tabFaculty";
        this.tabFaculty.SelectedIndex = 0;
        this.tabFaculty.Size = new System.Drawing.Size(700, 600);
        this.tabFaculty.TabIndex = 0;
        //
        // tabSubmissions
        //
        this.tabSubmissions.Controls.Add(this.dgvSubmissions);
        this.tabSubmissions.Controls.Add(this.pnlSubmissionsActions);
        this.tabSubmissions.Location = new System.Drawing.Point(4, 24);
        this.tabSubmissions.Name = "tabSubmissions";
        this.tabSubmissions.Padding = new System.Windows.Forms.Padding(3);
        this.tabSubmissions.Size = new System.Drawing.Size(692, 572);
        this.tabSubmissions.TabIndex = 0;
        this.tabSubmissions.Text = "Submissions & Grading Queue";
        this.tabSubmissions.UseVisualStyleBackColor = true;
        //
        // dgvSubmissions
        //
        this.dgvSubmissions.AllowUserToAddRows = false;
        this.dgvSubmissions.AllowUserToDeleteRows = false;
        this.dgvSubmissions.AllowUserToResizeRows = false;
        this.dgvSubmissions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvSubmissions.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvSubmissions.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvSubmissions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvSubmissions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvSubmissions.Location = new System.Drawing.Point(3, 41);
        this.dgvSubmissions.MultiSelect = false;
        this.dgvSubmissions.Name = "dgvSubmissions";
        this.dgvSubmissions.ReadOnly = true;
        this.dgvSubmissions.RowHeadersVisible = false;
        this.dgvSubmissions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvSubmissions.Size = new System.Drawing.Size(686, 528);
        this.dgvSubmissions.TabIndex = 1;
        this.dgvSubmissions.SelectionChanged += new System.EventHandler(this.dgvSubmissions_SelectionChanged);
        //
        // pnlSubmissionsActions
        //
        this.pnlSubmissionsActions.Controls.Add(this.btnGradeRubric);
        this.pnlSubmissionsActions.Controls.Add(this.btnToggleLock);
        this.pnlSubmissionsActions.Controls.Add(this.btnEditRepo);
        this.pnlSubmissionsActions.Controls.Add(this.btnTrashRepo);
        this.pnlSubmissionsActions.Controls.Add(this.btnCopyClone);
        this.pnlSubmissionsActions.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlSubmissionsActions.Location = new System.Drawing.Point(3, 3);
        this.pnlSubmissionsActions.Name = "pnlSubmissionsActions";
        this.pnlSubmissionsActions.Padding = new System.Windows.Forms.Padding(2, 4, 2, 4);
        this.pnlSubmissionsActions.Size = new System.Drawing.Size(686, 38);
        this.pnlSubmissionsActions.TabIndex = 0;
        //
        // btnGradeRubric
        //
        this.btnGradeRubric.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnGradeRubric.Location = new System.Drawing.Point(5, 4);
        this.btnGradeRubric.Name = "btnGradeRubric";
        this.btnGradeRubric.Size = new System.Drawing.Size(120, 28);
        this.btnGradeRubric.TabIndex = 0;
        this.btnGradeRubric.Text = "Grade Rubric...";
        this.btnGradeRubric.UseVisualStyleBackColor = true;
        this.btnGradeRubric.Click += new System.EventHandler(this.btnGradeRubric_Click);
        //
        // btnToggleLock
        //
        this.btnToggleLock.Location = new System.Drawing.Point(131, 4);
        this.btnToggleLock.Name = "btnToggleLock";
        this.btnToggleLock.Size = new System.Drawing.Size(120, 28);
        this.btnToggleLock.TabIndex = 1;
        this.btnToggleLock.Text = "Lock / Unlock Repo";
        this.btnToggleLock.UseVisualStyleBackColor = true;
        this.btnToggleLock.Click += new System.EventHandler(this.btnToggleLock_Click);
        //
        // btnEditRepo
        //
        this.btnEditRepo.Location = new System.Drawing.Point(257, 4);
        this.btnEditRepo.Name = "btnEditRepo";
        this.btnEditRepo.Size = new System.Drawing.Size(110, 28);
        this.btnEditRepo.TabIndex = 2;
        this.btnEditRepo.Text = "Settings...";
        this.btnEditRepo.UseVisualStyleBackColor = true;
        this.btnEditRepo.Click += new System.EventHandler(this.btnEditRepo_Click);
        //
        // btnTrashRepo
        //
        this.btnTrashRepo.Location = new System.Drawing.Point(373, 4);
        this.btnTrashRepo.Name = "btnTrashRepo";
        this.btnTrashRepo.Size = new System.Drawing.Size(110, 28);
        this.btnTrashRepo.TabIndex = 3;
        this.btnTrashRepo.Text = "Move to Bin";
        this.btnTrashRepo.UseVisualStyleBackColor = true;
        this.btnTrashRepo.Click += new System.EventHandler(this.btnTrashRepo_Click);
        //
        // btnCopyClone
        //
        this.btnCopyClone.Location = new System.Drawing.Point(489, 4);
        this.btnCopyClone.Name = "btnCopyClone";
        this.btnCopyClone.Size = new System.Drawing.Size(120, 28);
        this.btnCopyClone.TabIndex = 4;
        this.btnCopyClone.Text = "Copy Clone URL";
        this.btnCopyClone.UseVisualStyleBackColor = true;
        this.btnCopyClone.Click += new System.EventHandler(this.btnCopyClone_Click);
        //
        // tabTasks
        //
        this.tabTasks.Controls.Add(this.dgvTasks);
        this.tabTasks.Controls.Add(this.pnlTasksHeader);
        this.tabTasks.Location = new System.Drawing.Point(4, 24);
        this.tabTasks.Name = "tabTasks";
        this.tabTasks.Padding = new System.Windows.Forms.Padding(3);
        this.tabTasks.Size = new System.Drawing.Size(692, 572);
        this.tabTasks.TabIndex = 1;
        this.tabTasks.Text = "Coding Tasks";
        this.tabTasks.UseVisualStyleBackColor = true;
        //
        // dgvTasks
        //
        this.dgvTasks.AllowUserToAddRows = false;
        this.dgvTasks.AllowUserToDeleteRows = false;
        this.dgvTasks.AllowUserToResizeRows = false;
        this.dgvTasks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvTasks.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvTasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvTasks.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvTasks.Location = new System.Drawing.Point(3, 43);
        this.dgvTasks.MultiSelect = false;
        this.dgvTasks.Name = "dgvTasks";
        this.dgvTasks.ReadOnly = true;
        this.dgvTasks.RowHeadersVisible = false;
        this.dgvTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvTasks.Size = new System.Drawing.Size(686, 526);
        this.dgvTasks.TabIndex = 1;
        //
        // pnlTasksHeader
        //
        this.pnlTasksHeader.Controls.Add(this.btnNewTask);
        this.pnlTasksHeader.Controls.Add(this.lblTasksHeader);
        this.pnlTasksHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlTasksHeader.Location = new System.Drawing.Point(3, 3);
        this.pnlTasksHeader.Name = "pnlTasksHeader";
        this.pnlTasksHeader.Padding = new System.Windows.Forms.Padding(6);
        this.pnlTasksHeader.Size = new System.Drawing.Size(686, 40);
        this.pnlTasksHeader.TabIndex = 0;
        //
        // btnNewTask
        //
        this.btnNewTask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnNewTask.Location = new System.Drawing.Point(528, 6);
        this.btnNewTask.Name = "btnNewTask";
        this.btnNewTask.Size = new System.Drawing.Size(150, 26);
        this.btnNewTask.TabIndex = 1;
        this.btnNewTask.Text = "+ New Coding Task...";
        this.btnNewTask.UseVisualStyleBackColor = true;
        this.btnNewTask.Click += new System.EventHandler(this.btnNewTask_Click);
        //
        // lblTasksHeader
        //
        this.lblTasksHeader.AutoSize = true;
        this.lblTasksHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblTasksHeader.Location = new System.Drawing.Point(6, 12);
        this.lblTasksHeader.Name = "lblTasksHeader";
        this.lblTasksHeader.Size = new System.Drawing.Size(176, 15);
        this.lblTasksHeader.TabIndex = 0;
        this.lblTasksHeader.Text = "Assigned Tasks for Selected Course:";
        //
        // tabTickets
        //
        this.tabTickets.Controls.Add(this.dgvTickets);
        this.tabTickets.Controls.Add(this.pnlTicketsHeader);
        this.tabTickets.Location = new System.Drawing.Point(4, 24);
        this.tabTickets.Name = "tabTickets";
        this.tabTickets.Padding = new System.Windows.Forms.Padding(3);
        this.tabTickets.Size = new System.Drawing.Size(692, 572);
        this.tabTickets.TabIndex = 2;
        this.tabTickets.Text = "Groupmate Tickets";
        this.tabTickets.UseVisualStyleBackColor = true;
        //
        // dgvTickets
        //
        this.dgvTickets.AllowUserToAddRows = false;
        this.dgvTickets.AllowUserToDeleteRows = false;
        this.dgvTickets.AllowUserToResizeRows = false;
        this.dgvTickets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvTickets.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvTickets.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvTickets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvTickets.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvTickets.Location = new System.Drawing.Point(3, 43);
        this.dgvTickets.MultiSelect = false;
        this.dgvTickets.Name = "dgvTickets";
        this.dgvTickets.ReadOnly = true;
        this.dgvTickets.RowHeadersVisible = false;
        this.dgvTickets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvTickets.Size = new System.Drawing.Size(686, 526);
        this.dgvTickets.TabIndex = 1;
        //
        // pnlTicketsHeader
        //
        this.pnlTicketsHeader.Controls.Add(this.btnRejectTicket);
        this.pnlTicketsHeader.Controls.Add(this.btnApproveTicket);
        this.pnlTicketsHeader.Controls.Add(this.lblTicketsHeader);
        this.pnlTicketsHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlTicketsHeader.Location = new System.Drawing.Point(3, 3);
        this.pnlTicketsHeader.Name = "pnlTicketsHeader";
        this.pnlTicketsHeader.Padding = new System.Windows.Forms.Padding(6);
        this.pnlTicketsHeader.Size = new System.Drawing.Size(686, 40);
        this.pnlTicketsHeader.TabIndex = 0;
        //
        // btnRejectTicket
        //
        this.btnRejectTicket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnRejectTicket.Location = new System.Drawing.Point(578, 6);
        this.btnRejectTicket.Name = "btnRejectTicket";
        this.btnRejectTicket.Size = new System.Drawing.Size(100, 26);
        this.btnRejectTicket.TabIndex = 2;
        this.btnRejectTicket.Text = "Reject Ticket";
        this.btnRejectTicket.UseVisualStyleBackColor = true;
        this.btnRejectTicket.Click += new System.EventHandler(this.btnRejectTicket_Click);
        //
        // btnApproveTicket
        //
        this.btnApproveTicket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnApproveTicket.Location = new System.Drawing.Point(468, 6);
        this.btnApproveTicket.Name = "btnApproveTicket";
        this.btnApproveTicket.Size = new System.Drawing.Size(104, 26);
        this.btnApproveTicket.TabIndex = 1;
        this.btnApproveTicket.Text = "Approve Ticket";
        this.btnApproveTicket.UseVisualStyleBackColor = true;
        this.btnApproveTicket.Click += new System.EventHandler(this.btnApproveTicket_Click);
        //
        // lblTicketsHeader
        //
        this.lblTicketsHeader.AutoSize = true;
        this.lblTicketsHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblTicketsHeader.Location = new System.Drawing.Point(6, 12);
        this.lblTicketsHeader.Name = "lblTicketsHeader";
        this.lblTicketsHeader.Size = new System.Drawing.Size(209, 15);
        this.lblTicketsHeader.TabIndex = 0;
        this.lblTicketsHeader.Text = "Student Groupmate Access Tickets:";
        //
        // tabInspect
        //
        this.tabInspect.Controls.Add(this.splitInspect);
        this.tabInspect.Location = new System.Drawing.Point(4, 24);
        this.tabInspect.Name = "tabInspect";
        this.tabInspect.Padding = new System.Windows.Forms.Padding(3);
        this.tabInspect.Size = new System.Drawing.Size(692, 572);
        this.tabInspect.TabIndex = 3;
        this.tabInspect.Text = "Inspect Code & Commits";
        this.tabInspect.UseVisualStyleBackColor = true;
        //
        // splitInspect
        //
        this.splitInspect.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitInspect.Location = new System.Drawing.Point(3, 3);
        this.splitInspect.Name = "splitInspect";
        //
        // splitInspect.Panel1
        //
        this.splitInspect.Panel1.Controls.Add(this.tvFiles);
        this.splitInspect.Panel1MinSize = 160;
        //
        // splitInspect.Panel2
        //
        this.splitInspect.Panel2.Controls.Add(this.splitInspectRight);
        this.splitInspect.Panel2MinSize = 300;
        this.splitInspect.Size = new System.Drawing.Size(686, 566);
        this.splitInspect.SplitterDistance = 220;
        this.splitInspect.TabIndex = 0;
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
        // splitInspectRight
        //
        this.splitInspectRight.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitInspectRight.Location = new System.Drawing.Point(0, 0);
        this.splitInspectRight.Name = "splitInspectRight";
        this.splitInspectRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
        //
        // splitInspectRight.Panel1
        //
        this.splitInspectRight.Panel1.Controls.Add(this.rtbCode);
        this.splitInspectRight.Panel1.Controls.Add(this.pnlCodeHeader);
        this.splitInspectRight.Panel1MinSize = 180;
        //
        // splitInspectRight.Panel2
        //
        this.splitInspectRight.Panel2.Controls.Add(this.splitHistory);
        this.splitInspectRight.Panel2MinSize = 140;
        this.splitInspectRight.Size = new System.Drawing.Size(462, 566);
        this.splitInspectRight.SplitterDistance = 320;
        this.splitInspectRight.TabIndex = 0;
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
        this.rtbCode.Size = new System.Drawing.Size(462, 290);
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
        this.pnlCodeHeader.Padding = new System.Windows.Forms.Padding(6);
        this.pnlCodeHeader.Size = new System.Drawing.Size(462, 30);
        this.pnlCodeHeader.TabIndex = 0;
        //
        // lblFileSize
        //
        this.lblFileSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.lblFileSize.Location = new System.Drawing.Point(336, 8);
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
        // splitHistory
        //
        this.splitHistory.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitHistory.Location = new System.Drawing.Point(0, 0);
        this.splitHistory.Name = "splitHistory";
        //
        // splitHistory.Panel1
        //
        this.splitHistory.Panel1.Controls.Add(this.dgvCommits);
        this.splitHistory.Panel1.Controls.Add(this.lblCommitsHeader);
        //
        // splitHistory.Panel2
        //
        this.splitHistory.Panel2.Controls.Add(this.dgvPushes);
        this.splitHistory.Panel2.Controls.Add(this.lblPushesHeader);
        this.splitHistory.Size = new System.Drawing.Size(462, 242);
        this.splitHistory.SplitterDistance = 230;
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
        this.dgvCommits.Location = new System.Drawing.Point(0, 22);
        this.dgvCommits.MultiSelect = false;
        this.dgvCommits.Name = "dgvCommits";
        this.dgvCommits.ReadOnly = true;
        this.dgvCommits.RowHeadersVisible = false;
        this.dgvCommits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvCommits.Size = new System.Drawing.Size(230, 220);
        this.dgvCommits.TabIndex = 1;
        //
        // lblCommitsHeader
        //
        this.lblCommitsHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblCommitsHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblCommitsHeader.Location = new System.Drawing.Point(0, 0);
        this.lblCommitsHeader.Name = "lblCommitsHeader";
        this.lblCommitsHeader.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        this.lblCommitsHeader.Size = new System.Drawing.Size(230, 22);
        this.lblCommitsHeader.TabIndex = 0;
        this.lblCommitsHeader.Text = "Commit History:";
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
        this.dgvPushes.Location = new System.Drawing.Point(0, 22);
        this.dgvPushes.MultiSelect = false;
        this.dgvPushes.Name = "dgvPushes";
        this.dgvPushes.ReadOnly = true;
        this.dgvPushes.RowHeadersVisible = false;
        this.dgvPushes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvPushes.Size = new System.Drawing.Size(228, 220);
        this.dgvPushes.TabIndex = 1;
        //
        // lblPushesHeader
        //
        this.lblPushesHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblPushesHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblPushesHeader.Location = new System.Drawing.Point(0, 0);
        this.lblPushesHeader.Name = "lblPushesHeader";
        this.lblPushesHeader.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        this.lblPushesHeader.Size = new System.Drawing.Size(228, 22);
        this.lblPushesHeader.TabIndex = 0;
        this.lblPushesHeader.Text = "Push Ledger Timestamps:";
        //
        // tabDiscussions
        //
        this.tabDiscussions.Controls.Add(this.dgvDiscussions);
        this.tabDiscussions.Controls.Add(this.pnlCommentInput);
        this.tabDiscussions.Controls.Add(this.pnlDiscussionActions);
        this.tabDiscussions.Location = new System.Drawing.Point(4, 24);
        this.tabDiscussions.Name = "tabDiscussions";
        this.tabDiscussions.Padding = new System.Windows.Forms.Padding(3);
        this.tabDiscussions.Size = new System.Drawing.Size(692, 572);
        this.tabDiscussions.TabIndex = 4;
        this.tabDiscussions.Text = "Discussions & Moderation";
        this.tabDiscussions.UseVisualStyleBackColor = true;
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
        this.dgvDiscussions.Location = new System.Drawing.Point(3, 43);
        this.dgvDiscussions.MultiSelect = false;
        this.dgvDiscussions.Name = "dgvDiscussions";
        this.dgvDiscussions.ReadOnly = true;
        this.dgvDiscussions.RowHeadersVisible = false;
        this.dgvDiscussions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvDiscussions.Size = new System.Drawing.Size(686, 469);
        this.dgvDiscussions.TabIndex = 1;
        //
        // pnlCommentInput
        //
        this.pnlCommentInput.Controls.Add(this.btnPostComment);
        this.pnlCommentInput.Controls.Add(this.txtComment);
        this.pnlCommentInput.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlCommentInput.Location = new System.Drawing.Point(3, 512);
        this.pnlCommentInput.Name = "pnlCommentInput";
        this.pnlCommentInput.Padding = new System.Windows.Forms.Padding(4);
        this.pnlCommentInput.Size = new System.Drawing.Size(686, 57);
        this.pnlCommentInput.TabIndex = 2;
        //
        // btnPostComment
        //
        this.btnPostComment.Dock = System.Windows.Forms.DockStyle.Right;
        this.btnPostComment.Location = new System.Drawing.Point(592, 4);
        this.btnPostComment.Name = "btnPostComment";
        this.btnPostComment.Size = new System.Drawing.Size(90, 49);
        this.btnPostComment.TabIndex = 1;
        this.btnPostComment.Text = "Post Comment";
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
        this.txtComment.Size = new System.Drawing.Size(582, 49);
        this.txtComment.TabIndex = 0;
        //
        // pnlDiscussionActions
        //
        this.pnlDiscussionActions.Controls.Add(this.btnToggleModeration);
        this.pnlDiscussionActions.Controls.Add(this.lblDiscussionHeader);
        this.pnlDiscussionActions.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlDiscussionActions.Location = new System.Drawing.Point(3, 3);
        this.pnlDiscussionActions.Name = "pnlDiscussionActions";
        this.pnlDiscussionActions.Padding = new System.Windows.Forms.Padding(6);
        this.pnlDiscussionActions.Size = new System.Drawing.Size(686, 40);
        this.pnlDiscussionActions.TabIndex = 0;
        //
        // btnToggleModeration
        //
        this.btnToggleModeration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnToggleModeration.Location = new System.Drawing.Point(506, 7);
        this.btnToggleModeration.Name = "btnToggleModeration";
        this.btnToggleModeration.Size = new System.Drawing.Size(174, 26);
        this.btnToggleModeration.TabIndex = 1;
        this.btnToggleModeration.Text = "Hide / Unhide Comment";
        this.btnToggleModeration.UseVisualStyleBackColor = true;
        this.btnToggleModeration.Click += new System.EventHandler(this.btnToggleModeration_Click);
        //
        // lblDiscussionHeader
        //
        this.lblDiscussionHeader.AutoSize = true;
        this.lblDiscussionHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblDiscussionHeader.Location = new System.Drawing.Point(6, 12);
        this.lblDiscussionHeader.Name = "lblDiscussionHeader";
        this.lblDiscussionHeader.Size = new System.Drawing.Size(188, 15);
        this.lblDiscussionHeader.TabIndex = 0;
        this.lblDiscussionHeader.Text = "Peer Discussions & Moderation:";
        //
        // FacultyWorkspaceControl
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this.splitMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "FacultyWorkspaceControl";
        this.Size = new System.Drawing.Size(1024, 600);
        this.splitMain.Panel1.ResumeLayout(false);
        this.splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
        this.splitMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvCourses)).EndInit();
        this.pnlCoursesBottom.ResumeLayout(false);
        this.pnlCoursesTop.ResumeLayout(false);
        this.pnlCoursesTop.PerformLayout();
        this.tabFaculty.ResumeLayout(false);
        this.tabSubmissions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvSubmissions)).EndInit();
        this.pnlSubmissionsActions.ResumeLayout(false);
        this.tabTasks.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).EndInit();
        this.pnlTasksHeader.ResumeLayout(false);
        this.pnlTasksHeader.PerformLayout();
        this.tabTickets.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvTickets)).EndInit();
        this.pnlTicketsHeader.ResumeLayout(false);
        this.pnlTicketsHeader.PerformLayout();
        this.tabInspect.ResumeLayout(false);
        this.splitInspect.Panel1.ResumeLayout(false);
        this.splitInspect.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitInspect)).EndInit();
        this.splitInspect.ResumeLayout(false);
        this.splitInspectRight.Panel1.ResumeLayout(false);
        this.splitInspectRight.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitInspectRight)).EndInit();
        this.splitInspectRight.ResumeLayout(false);
        this.pnlCodeHeader.ResumeLayout(false);
        this.pnlCodeHeader.PerformLayout();
        this.splitHistory.Panel1.ResumeLayout(false);
        this.splitHistory.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitHistory)).EndInit();
        this.splitHistory.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvCommits)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPushes)).EndInit();
        this.tabDiscussions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvDiscussions)).EndInit();
        this.pnlCommentInput.ResumeLayout(false);
        this.pnlCommentInput.PerformLayout();
        this.pnlDiscussionActions.ResumeLayout(false);
        this.pnlDiscussionActions.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.SplitContainer splitMain;
    private System.Windows.Forms.Panel pnlCoursesTop;
    private System.Windows.Forms.Label lblCourseCatalog;
    private System.Windows.Forms.Button btnNewCourse;
    private System.Windows.Forms.Button btnRepoBin;
    private System.Windows.Forms.DataGridView dgvCourses;
    private System.Windows.Forms.FlowLayoutPanel pnlCoursesBottom;
    private System.Windows.Forms.Button btnEditCourse;
    private System.Windows.Forms.Button btnDeleteCourse;
    private System.Windows.Forms.TabControl tabFaculty;
    private System.Windows.Forms.TabPage tabSubmissions;
    private System.Windows.Forms.FlowLayoutPanel pnlSubmissionsActions;
    private System.Windows.Forms.Button btnGradeRubric;
    private System.Windows.Forms.Button btnToggleLock;
    private System.Windows.Forms.Button btnEditRepo;
    private System.Windows.Forms.Button btnTrashRepo;
    private System.Windows.Forms.Button btnCopyClone;
    private System.Windows.Forms.DataGridView dgvSubmissions;
    private System.Windows.Forms.TabPage tabTasks;
    private System.Windows.Forms.Panel pnlTasksHeader;
    private System.Windows.Forms.Label lblTasksHeader;
    private System.Windows.Forms.Button btnNewTask;
    private System.Windows.Forms.DataGridView dgvTasks;
    private System.Windows.Forms.TabPage tabTickets;
    private System.Windows.Forms.Panel pnlTicketsHeader;
    private System.Windows.Forms.Label lblTicketsHeader;
    private System.Windows.Forms.Button btnApproveTicket;
    private System.Windows.Forms.Button btnRejectTicket;
    private System.Windows.Forms.DataGridView dgvTickets;
    private System.Windows.Forms.TabPage tabInspect;
    private System.Windows.Forms.SplitContainer splitInspect;
    private System.Windows.Forms.TreeView tvFiles;
    private System.Windows.Forms.SplitContainer splitInspectRight;
    private System.Windows.Forms.Panel pnlCodeHeader;
    private System.Windows.Forms.Label lblFilePath;
    private System.Windows.Forms.Label lblFileSize;
    private System.Windows.Forms.RichTextBox rtbCode;
    private System.Windows.Forms.SplitContainer splitHistory;
    private System.Windows.Forms.Label lblCommitsHeader;
    private System.Windows.Forms.DataGridView dgvCommits;
    private System.Windows.Forms.Label lblPushesHeader;
    private System.Windows.Forms.DataGridView dgvPushes;
    private System.Windows.Forms.TabPage tabDiscussions;
    private System.Windows.Forms.Panel pnlDiscussionActions;
    private System.Windows.Forms.Label lblDiscussionHeader;
    private System.Windows.Forms.Button btnToggleModeration;
    private System.Windows.Forms.DataGridView dgvDiscussions;
    private System.Windows.Forms.Panel pnlCommentInput;
    private System.Windows.Forms.TextBox txtComment;
    private System.Windows.Forms.Button btnPostComment;
}
