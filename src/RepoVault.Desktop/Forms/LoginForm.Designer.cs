namespace RepoVault.Desktop.Forms;

partial class LoginForm
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
        this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
        this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblSubtitle = new System.Windows.Forms.Label();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.btnLogin = new System.Windows.Forms.Button();
        this.flowQuick = new System.Windows.Forms.FlowLayoutPanel();
        this.lblQuick = new System.Windows.Forms.Label();
        this.cboDemoAccount = new System.Windows.Forms.ComboBox();
        this.btnQuickFill = new System.Windows.Forms.Button();
        this.lnkRegister = new System.Windows.Forms.LinkLabel();
        this.lblStatus = new System.Windows.Forms.Label();
        this.tlpMain.SuspendLayout();
        this.flowQuick.SuspendLayout();
        this.SuspendLayout();
        //
        // errorProvider
        //
        this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
        this.errorProvider.ContainerControl = this;
        //
        // tlpMain
        //
        this.tlpMain.ColumnCount = 2;
        this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tlpMain.Controls.Add(this.lblTitle, 0, 0);
        this.tlpMain.Controls.Add(this.lblSubtitle, 0, 1);
        this.tlpMain.Controls.Add(this.lblUsername, 0, 2);
        this.tlpMain.Controls.Add(this.txtUsername, 1, 2);
        this.tlpMain.Controls.Add(this.lblPassword, 0, 3);
        this.tlpMain.Controls.Add(this.txtPassword, 1, 3);
        this.tlpMain.Controls.Add(this.btnLogin, 0, 4);
        this.tlpMain.Controls.Add(this.flowQuick, 0, 5);
        this.tlpMain.Controls.Add(this.lnkRegister, 0, 6);
        this.tlpMain.Controls.Add(this.lblStatus, 0, 7);
        this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tlpMain.Location = new System.Drawing.Point(0, 0);
        this.tlpMain.Name = "tlpMain";
        this.tlpMain.Padding = new System.Windows.Forms.Padding(24, 18, 24, 18);
        this.tlpMain.RowCount = 8;
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.Size = new System.Drawing.Size(444, 421);
        this.tlpMain.TabIndex = 0;
        this.tlpMain.SetColumnSpan(this.lblTitle, 2);
        this.tlpMain.SetColumnSpan(this.lblSubtitle, 2);
        this.tlpMain.SetColumnSpan(this.btnLogin, 2);
        this.tlpMain.SetColumnSpan(this.flowQuick, 2);
        this.tlpMain.SetColumnSpan(this.lnkRegister, 2);
        this.tlpMain.SetColumnSpan(this.lblStatus, 2);
        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        this.lblTitle.Location = new System.Drawing.Point(27, 18);
        this.lblTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 2);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(129, 32);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "Repo Vault";
        //
        // lblSubtitle
        //
        this.lblSubtitle.AutoSize = true;
        this.lblSubtitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblSubtitle.Location = new System.Drawing.Point(27, 52);
        this.lblSubtitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 14);
        this.lblSubtitle.Name = "lblSubtitle";
        this.lblSubtitle.Size = new System.Drawing.Size(210, 15);
        this.lblSubtitle.TabIndex = 1;
        this.lblSubtitle.Text = "UCC BSCS 2A - self-hosted git archive";
        //
        // lblUsername
        //
        this.lblUsername.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblUsername.AutoSize = true;
        this.lblUsername.Location = new System.Drawing.Point(27, 90);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new System.Drawing.Size(63, 15);
        this.lblUsername.TabIndex = 2;
        this.lblUsername.Text = "&Username:";
        //
        // txtUsername
        //
        this.txtUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtUsername.Location = new System.Drawing.Point(120, 86);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.Size = new System.Drawing.Size(297, 23);
        this.txtUsername.TabIndex = 3;
        //
        // lblPassword
        //
        this.lblPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblPassword.AutoSize = true;
        this.lblPassword.Location = new System.Drawing.Point(27, 126);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(60, 15);
        this.lblPassword.TabIndex = 4;
        this.lblPassword.Text = "&Password:";
        //
        // txtPassword
        //
        this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtPassword.Location = new System.Drawing.Point(120, 122);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.Size = new System.Drawing.Size(297, 23);
        this.txtPassword.TabIndex = 5;
        this.txtPassword.UseSystemPasswordChar = true;
        //
        // btnLogin
        //
        this.btnLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.btnLogin.Location = new System.Drawing.Point(27, 158);
        this.btnLogin.Name = "btnLogin";
        this.btnLogin.Size = new System.Drawing.Size(390, 30);
        this.btnLogin.TabIndex = 6;
        this.btnLogin.Text = "&Sign In";
        this.btnLogin.UseVisualStyleBackColor = true;
        this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
        //
        // flowQuick
        //
        this.flowQuick.AutoSize = true;
        this.flowQuick.Controls.Add(this.lblQuick);
        this.flowQuick.Controls.Add(this.cboDemoAccount);
        this.flowQuick.Controls.Add(this.btnQuickFill);
        this.flowQuick.Location = new System.Drawing.Point(27, 194);
        this.flowQuick.Name = "flowQuick";
        this.flowQuick.Size = new System.Drawing.Size(390, 29);
        this.flowQuick.TabIndex = 7;
        //
        // lblQuick
        //
        this.lblQuick.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblQuick.AutoSize = true;
        this.lblQuick.Location = new System.Drawing.Point(3, 7);
        this.lblQuick.Margin = new System.Windows.Forms.Padding(3, 0, 8, 0);
        this.lblQuick.Name = "lblQuick";
        this.lblQuick.Size = new System.Drawing.Size(71, 15);
        this.lblQuick.TabIndex = 0;
        this.lblQuick.Text = "Quick demo:";
        //
        // cboDemoAccount
        //
        this.cboDemoAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboDemoAccount.FormattingEnabled = true;
        this.cboDemoAccount.Items.AddRange(new object[] {
            "prof_santos (Faculty)",
            "frtzhahn (Student)"});
        this.cboDemoAccount.Location = new System.Drawing.Point(85, 3);
        this.cboDemoAccount.Name = "cboDemoAccount";
        this.cboDemoAccount.Size = new System.Drawing.Size(170, 23);
        this.cboDemoAccount.TabIndex = 1;
        //
        // btnQuickFill
        //
        this.btnQuickFill.Location = new System.Drawing.Point(261, 3);
        this.btnQuickFill.Name = "btnQuickFill";
        this.btnQuickFill.Size = new System.Drawing.Size(120, 23);
        this.btnQuickFill.TabIndex = 2;
        this.btnQuickFill.Text = "Quick Demo Login";
        this.btnQuickFill.UseVisualStyleBackColor = true;
        this.btnQuickFill.Click += new System.EventHandler(this.btnQuickFill_Click);
        //
        // lnkRegister
        //
        this.lnkRegister.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.lnkRegister.AutoSize = true;
        this.lnkRegister.Location = new System.Drawing.Point(139, 240);
        this.lnkRegister.Name = "lnkRegister";
        this.lnkRegister.Size = new System.Drawing.Size(166, 15);
        this.lnkRegister.TabIndex = 8;
        this.lnkRegister.TabStop = true;
        this.lnkRegister.Text = "New student? Register here";
        this.lnkRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRegister_LinkClicked);
        //
        // lblStatus
        //
        this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.lblStatus.AutoEllipsis = true;
        this.lblStatus.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblStatus.Location = new System.Drawing.Point(27, 265);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(390, 15);
        this.lblStatus.TabIndex = 9;
        this.lblStatus.Text = "sign in to continue.";
        //
        // LoginForm
        //
        this.AcceptButton = this.btnLogin;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(444, 421);
        this.Controls.Add(this.tlpMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "LoginForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Repo Vault - Sign In";
        this.tlpMain.ResumeLayout(false);
        this.tlpMain.PerformLayout();
        this.flowQuick.ResumeLayout(false);
        this.flowQuick.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.TableLayoutPanel tlpMain;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblSubtitle;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.FlowLayoutPanel flowQuick;
    private System.Windows.Forms.Label lblQuick;
    private System.Windows.Forms.ComboBox cboDemoAccount;
    private System.Windows.Forms.Button btnQuickFill;
    private System.Windows.Forms.LinkLabel lnkRegister;
    private System.Windows.Forms.Label lblStatus;
}
