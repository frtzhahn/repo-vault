namespace RepoVault.Desktop.Forms;

partial class MainForm
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
        this.components = new System.ComponentModel.Container();
        this.menuMain = new System.Windows.Forms.MenuStrip();
        this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuLogout = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuFileSeparator = new System.Windows.Forms.ToolStripSeparator();
        this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuServer = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuStartServer = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuStopServer = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuServerSeparator = new System.Windows.Forms.ToolStripSeparator();
        this.mnuCopyUrl = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuViewLanIp = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuTools = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuSeed = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuOpenStorage = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
        this.statusMain = new System.Windows.Forms.StatusStrip();
        this.lblUserStatus = new System.Windows.Forms.ToolStripStatusLabel();
        this.lblServerStatus = new System.Windows.Forms.ToolStripStatusLabel();
        this.btnServerToggle = new System.Windows.Forms.ToolStripButton();
        this.lblNotification = new System.Windows.Forms.ToolStripStatusLabel();
        this.lblPushCounter = new System.Windows.Forms.ToolStripStatusLabel();
        this.pnlWorkspace = new System.Windows.Forms.Panel();
        this.flpWelcome = new System.Windows.Forms.FlowLayoutPanel();
        this.lblWelcome = new System.Windows.Forms.Label();
        this.lblIdentity = new System.Windows.Forms.Label();
        this.lblRoleHint = new System.Windows.Forms.Label();
        this.lblStats = new System.Windows.Forms.Label();
        this.notifyTimer = new System.Windows.Forms.Timer(this.components);
        this.menuMain.SuspendLayout();
        this.statusMain.SuspendLayout();
        this.pnlWorkspace.SuspendLayout();
        this.flpWelcome.SuspendLayout();
        this.SuspendLayout();
        //
        // menuMain
        //
        this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuServer,
            this.mnuTools,
            this.mnuHelp});
        this.menuMain.Location = new System.Drawing.Point(0, 0);
        this.menuMain.Name = "menuMain";
        this.menuMain.Size = new System.Drawing.Size(1024, 24);
        this.menuMain.TabIndex = 0;
        //
        // mnuFile
        //
        this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLogout,
            this.mnuFileSeparator,
            this.mnuExit});
        this.mnuFile.Name = "mnuFile";
        this.mnuFile.Size = new System.Drawing.Size(37, 20);
        this.mnuFile.Text = "&File";
        //
        // mnuLogout
        //
        this.mnuLogout.Name = "mnuLogout";
        this.mnuLogout.Size = new System.Drawing.Size(118, 22);
        this.mnuLogout.Text = "&Logout";
        this.mnuLogout.Click += new System.EventHandler(this.mnuLogout_Click);
        //
        // mnuFileSeparator
        //
        this.mnuFileSeparator.Name = "mnuFileSeparator";
        this.mnuFileSeparator.Size = new System.Drawing.Size(115, 6);
        //
        // mnuExit
        //
        this.mnuExit.Name = "mnuExit";
        this.mnuExit.Size = new System.Drawing.Size(118, 22);
        this.mnuExit.Text = "E&xit";
        this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
        //
        // mnuServer
        //
        this.mnuServer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStartServer,
            this.mnuStopServer,
            this.mnuServerSeparator,
            this.mnuCopyUrl,
            this.mnuViewLanIp});
        this.mnuServer.Name = "mnuServer";
        this.mnuServer.Size = new System.Drawing.Size(55, 20);
        this.mnuServer.Text = "&Server";
        //
        // mnuStartServer
        //
        this.mnuStartServer.Name = "mnuStartServer";
        this.mnuStartServer.Size = new System.Drawing.Size(180, 22);
        this.mnuStartServer.Text = "&Start Server";
        this.mnuStartServer.Click += new System.EventHandler(this.mnuStartServer_Click);
        //
        // mnuStopServer
        //
        this.mnuStopServer.Name = "mnuStopServer";
        this.mnuStopServer.Size = new System.Drawing.Size(180, 22);
        this.mnuStopServer.Text = "S&top Server";
        this.mnuStopServer.Click += new System.EventHandler(this.mnuStopServer_Click);
        //
        // mnuServerSeparator
        //
        this.mnuServerSeparator.Name = "mnuServerSeparator";
        this.mnuServerSeparator.Size = new System.Drawing.Size(177, 6);
        //
        // mnuCopyUrl
        //
        this.mnuCopyUrl.Name = "mnuCopyUrl";
        this.mnuCopyUrl.Size = new System.Drawing.Size(180, 22);
        this.mnuCopyUrl.Text = "&Copy Clone URL";
        this.mnuCopyUrl.Click += new System.EventHandler(this.mnuCopyUrl_Click);
        //
        // mnuViewLanIp
        //
        this.mnuViewLanIp.Name = "mnuViewLanIp";
        this.mnuViewLanIp.Size = new System.Drawing.Size(180, 22);
        this.mnuViewLanIp.Text = "View LAN &IP";
        this.mnuViewLanIp.Click += new System.EventHandler(this.mnuViewLanIp_Click);
        //
        // mnuTools
        //
        this.mnuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSeed,
            this.mnuOpenStorage});
        this.mnuTools.Name = "mnuTools";
        this.mnuTools.Size = new System.Drawing.Size(48, 20);
        this.mnuTools.Text = "&Tools";
        //
        // mnuSeed
        //
        this.mnuSeed.Name = "mnuSeed";
        this.mnuSeed.Size = new System.Drawing.Size(221, 22);
        this.mnuSeed.Text = "&Seed / Re-index Workspaces";
        this.mnuSeed.Click += new System.EventHandler(this.mnuSeed_Click);
        //
        // mnuOpenStorage
        //
        this.mnuOpenStorage.Name = "mnuOpenStorage";
        this.mnuOpenStorage.Size = new System.Drawing.Size(221, 22);
        this.mnuOpenStorage.Text = "&Open Storage Folder";
        this.mnuOpenStorage.Click += new System.EventHandler(this.mnuOpenStorage_Click);
        //
        // mnuHelp
        //
        this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAbout});
        this.mnuHelp.Name = "mnuHelp";
        this.mnuHelp.Size = new System.Drawing.Size(44, 20);
        this.mnuHelp.Text = "&Help";
        //
        // mnuAbout
        //
        this.mnuAbout.Name = "mnuAbout";
        this.mnuAbout.Size = new System.Drawing.Size(180, 22);
        this.mnuAbout.Text = "&About Repo Vault";
        this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
        //
        // statusMain
        //
        this.statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblUserStatus,
            this.lblServerStatus,
            this.btnServerToggle,
            this.lblNotification,
            this.lblPushCounter});
        this.statusMain.Location = new System.Drawing.Point(0, 618);
        this.statusMain.Name = "statusMain";
        this.statusMain.Size = new System.Drawing.Size(1024, 22);
        this.statusMain.TabIndex = 2;
        //
        // lblUserStatus
        //
        this.lblUserStatus.Name = "lblUserStatus";
        this.lblUserStatus.Size = new System.Drawing.Size(76, 17);
        this.lblUserStatus.Text = "Logged in: -";
        //
        // lblServerStatus
        //
        this.lblServerStatus.Name = "lblServerStatus";
        this.lblServerStatus.Size = new System.Drawing.Size(92, 17);
        this.lblServerStatus.Text = "Server: Stopped";
        //
        // btnServerToggle
        //
        this.btnServerToggle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        this.btnServerToggle.Name = "btnServerToggle";
        this.btnServerToggle.Size = new System.Drawing.Size(95, 20);
        this.btnServerToggle.Text = "Start Git Server";
        this.btnServerToggle.Click += new System.EventHandler(this.btnServerToggle_Click);
        //
        // lblNotification
        //
        this.lblNotification.Name = "lblNotification";
        this.lblNotification.Size = new System.Drawing.Size(0, 17);
        this.lblNotification.Spring = true;
        this.lblNotification.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblPushCounter
        //
        this.lblPushCounter.Name = "lblPushCounter";
        this.lblPushCounter.Size = new System.Drawing.Size(111, 17);
        this.lblPushCounter.Text = "Pushes Received: 0";
        //
        // pnlWorkspace
        //
        this.pnlWorkspace.Controls.Add(this.flpWelcome);
        this.pnlWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlWorkspace.Location = new System.Drawing.Point(0, 24);
        this.pnlWorkspace.Name = "pnlWorkspace";
        this.pnlWorkspace.Size = new System.Drawing.Size(1024, 594);
        this.pnlWorkspace.TabIndex = 1;
        //
        // flpWelcome
        //
        this.flpWelcome.AutoScroll = true;
        this.flpWelcome.Controls.Add(this.lblWelcome);
        this.flpWelcome.Controls.Add(this.lblIdentity);
        this.flpWelcome.Controls.Add(this.lblRoleHint);
        this.flpWelcome.Controls.Add(this.lblStats);
        this.flpWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
        this.flpWelcome.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.flpWelcome.Location = new System.Drawing.Point(0, 0);
        this.flpWelcome.Name = "flpWelcome";
        this.flpWelcome.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
        this.flpWelcome.Size = new System.Drawing.Size(1024, 594);
        this.flpWelcome.TabIndex = 0;
        this.flpWelcome.WrapContents = false;
        //
        // lblWelcome
        //
        this.lblWelcome.AutoSize = true;
        this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        this.lblWelcome.Location = new System.Drawing.Point(31, 24);
        this.lblWelcome.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
        this.lblWelcome.Name = "lblWelcome";
        this.lblWelcome.Size = new System.Drawing.Size(130, 32);
        this.lblWelcome.TabIndex = 0;
        this.lblWelcome.Text = "Welcome!";
        //
        // lblIdentity
        //
        this.lblIdentity.AutoSize = true;
        this.lblIdentity.Font = new System.Drawing.Font("Segoe UI", 11F);
        this.lblIdentity.Location = new System.Drawing.Point(31, 62);
        this.lblIdentity.Margin = new System.Windows.Forms.Padding(3, 0, 3, 16);
        this.lblIdentity.Name = "lblIdentity";
        this.lblIdentity.Size = new System.Drawing.Size(60, 20);
        this.lblIdentity.TabIndex = 1;
        this.lblIdentity.Text = "identity";
        //
        // lblRoleHint
        //
        this.lblRoleHint.AutoSize = true;
        this.lblRoleHint.Location = new System.Drawing.Point(31, 98);
        this.lblRoleHint.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
        this.lblRoleHint.MaximumSize = new System.Drawing.Size(860, 0);
        this.lblRoleHint.Name = "lblRoleHint";
        this.lblRoleHint.Size = new System.Drawing.Size(40, 15);
        this.lblRoleHint.TabIndex = 2;
        this.lblRoleHint.Text = "hint";
        //
        // lblStats
        //
        this.lblStats.AutoSize = true;
        this.lblStats.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblStats.Location = new System.Drawing.Point(31, 125);
        this.lblStats.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.lblStats.MaximumSize = new System.Drawing.Size(860, 0);
        this.lblStats.Name = "lblStats";
        this.lblStats.Size = new System.Drawing.Size(40, 15);
        this.lblStats.TabIndex = 3;
        this.lblStats.Text = "stats";
        //
        // notifyTimer
        //
        this.notifyTimer.Interval = 4000;
        this.notifyTimer.Tick += new System.EventHandler(this.NotifyTimer_Tick);
        //
        // MainForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(1024, 640);
        this.Controls.Add(this.pnlWorkspace);
        this.Controls.Add(this.statusMain);
        this.Controls.Add(this.menuMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MainMenuStrip = this.menuMain;
        this.MinimumSize = new System.Drawing.Size(860, 540);
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Repo Vault - UCC BSCS 2A";
        this.menuMain.ResumeLayout(false);
        this.menuMain.PerformLayout();
        this.statusMain.ResumeLayout(false);
        this.statusMain.PerformLayout();
        this.pnlWorkspace.ResumeLayout(false);
        this.flpWelcome.ResumeLayout(false);
        this.flpWelcome.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.MenuStrip menuMain;
    private System.Windows.Forms.ToolStripMenuItem mnuFile;
    private System.Windows.Forms.ToolStripMenuItem mnuLogout;
    private System.Windows.Forms.ToolStripSeparator mnuFileSeparator;
    private System.Windows.Forms.ToolStripMenuItem mnuExit;
    private System.Windows.Forms.ToolStripMenuItem mnuServer;
    private System.Windows.Forms.ToolStripMenuItem mnuStartServer;
    private System.Windows.Forms.ToolStripMenuItem mnuStopServer;
    private System.Windows.Forms.ToolStripSeparator mnuServerSeparator;
    private System.Windows.Forms.ToolStripMenuItem mnuCopyUrl;
    private System.Windows.Forms.ToolStripMenuItem mnuViewLanIp;
    private System.Windows.Forms.ToolStripMenuItem mnuTools;
    private System.Windows.Forms.ToolStripMenuItem mnuSeed;
    private System.Windows.Forms.ToolStripMenuItem mnuOpenStorage;
    private System.Windows.Forms.ToolStripMenuItem mnuHelp;
    private System.Windows.Forms.ToolStripMenuItem mnuAbout;
    private System.Windows.Forms.StatusStrip statusMain;
    private System.Windows.Forms.ToolStripStatusLabel lblUserStatus;
    private System.Windows.Forms.ToolStripStatusLabel lblServerStatus;
    private System.Windows.Forms.ToolStripButton btnServerToggle;
    private System.Windows.Forms.ToolStripStatusLabel lblNotification;
    private System.Windows.Forms.ToolStripStatusLabel lblPushCounter;
    private System.Windows.Forms.Panel pnlWorkspace;
    private System.Windows.Forms.FlowLayoutPanel flpWelcome;
    private System.Windows.Forms.Label lblWelcome;
    private System.Windows.Forms.Label lblIdentity;
    private System.Windows.Forms.Label lblRoleHint;
    private System.Windows.Forms.Label lblStats;
    private System.Windows.Forms.Timer notifyTimer;
}
