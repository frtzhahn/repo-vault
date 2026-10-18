namespace RepoVault.Desktop.Forms;

partial class RegisterForm
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
        this.lblIdNumber = new System.Windows.Forms.Label();
        this.txtIdNumber = new System.Windows.Forms.TextBox();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblFullName = new System.Windows.Forms.Label();
        this.txtFullName = new System.Windows.Forms.TextBox();
        this.lblEmail = new System.Windows.Forms.Label();
        this.txtEmail = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.lblConfirm = new System.Windows.Forms.Label();
        this.txtConfirmPassword = new System.Windows.Forms.TextBox();
        this.lblPasswordHint = new System.Windows.Forms.Label();
        this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnRegister = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.tlpMain.SuspendLayout();
        this.flpButtons.SuspendLayout();
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
        this.tlpMain.Controls.Add(this.lblIdNumber, 0, 1);
        this.tlpMain.Controls.Add(this.txtIdNumber, 1, 1);
        this.tlpMain.Controls.Add(this.lblUsername, 0, 2);
        this.tlpMain.Controls.Add(this.txtUsername, 1, 2);
        this.tlpMain.Controls.Add(this.lblFullName, 0, 3);
        this.tlpMain.Controls.Add(this.txtFullName, 1, 3);
        this.tlpMain.Controls.Add(this.lblEmail, 0, 4);
        this.tlpMain.Controls.Add(this.txtEmail, 1, 4);
        this.tlpMain.Controls.Add(this.lblPassword, 0, 5);
        this.tlpMain.Controls.Add(this.txtPassword, 1, 5);
        this.tlpMain.Controls.Add(this.lblConfirm, 0, 6);
        this.tlpMain.Controls.Add(this.txtConfirmPassword, 1, 6);
        this.tlpMain.Controls.Add(this.lblPasswordHint, 0, 7);
        this.tlpMain.Controls.Add(this.flpButtons, 0, 8);
        this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tlpMain.Location = new System.Drawing.Point(0, 0);
        this.tlpMain.Name = "tlpMain";
        this.tlpMain.Padding = new System.Windows.Forms.Padding(24, 18, 24, 18);
        this.tlpMain.RowCount = 9;
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tlpMain.Size = new System.Drawing.Size(484, 471);
        this.tlpMain.TabIndex = 0;
        this.tlpMain.SetColumnSpan(this.lblTitle, 2);
        this.tlpMain.SetColumnSpan(this.lblPasswordHint, 2);
        this.tlpMain.SetColumnSpan(this.flpButtons, 2);
        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        this.lblTitle.Location = new System.Drawing.Point(27, 18);
        this.lblTitle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(211, 25);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "Create Student Account";
        //
        // lblIdNumber
        //
        this.lblIdNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblIdNumber.AutoSize = true;
        this.lblIdNumber.Location = new System.Drawing.Point(27, 60);
        this.lblIdNumber.Name = "lblIdNumber";
        this.lblIdNumber.Size = new System.Drawing.Size(70, 15);
        this.lblIdNumber.TabIndex = 1;
        this.lblIdNumber.Text = "&ID Number:";
        //
        // txtIdNumber
        //
        this.txtIdNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtIdNumber.Location = new System.Drawing.Point(140, 56);
        this.txtIdNumber.Name = "txtIdNumber";
        this.txtIdNumber.PlaceholderText = "2024-10042";
        this.txtIdNumber.Size = new System.Drawing.Size(317, 23);
        this.txtIdNumber.TabIndex = 2;
        //
        // lblUsername
        //
        this.lblUsername.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblUsername.AutoSize = true;
        this.lblUsername.Location = new System.Drawing.Point(27, 92);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new System.Drawing.Size(63, 15);
        this.lblUsername.TabIndex = 3;
        this.lblUsername.Text = "&Username:";
        //
        // txtUsername
        //
        this.txtUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtUsername.Location = new System.Drawing.Point(140, 88);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.Size = new System.Drawing.Size(317, 23);
        this.txtUsername.TabIndex = 4;
        //
        // lblFullName
        //
        this.lblFullName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblFullName.AutoSize = true;
        this.lblFullName.Location = new System.Drawing.Point(27, 124);
        this.lblFullName.Name = "lblFullName";
        this.lblFullName.Size = new System.Drawing.Size(64, 15);
        this.lblFullName.TabIndex = 5;
        this.lblFullName.Text = "Full &Name:";
        //
        // txtFullName
        //
        this.txtFullName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtFullName.Location = new System.Drawing.Point(140, 120);
        this.txtFullName.Name = "txtFullName";
        this.txtFullName.Size = new System.Drawing.Size(317, 23);
        this.txtFullName.TabIndex = 6;
        //
        // lblEmail
        //
        this.lblEmail.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblEmail.AutoSize = true;
        this.lblEmail.Location = new System.Drawing.Point(27, 156);
        this.lblEmail.Name = "lblEmail";
        this.lblEmail.Size = new System.Drawing.Size(39, 15);
        this.lblEmail.TabIndex = 7;
        this.lblEmail.Text = "&Email:";
        //
        // txtEmail
        //
        this.txtEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtEmail.Location = new System.Drawing.Point(140, 152);
        this.txtEmail.Name = "txtEmail";
        this.txtEmail.PlaceholderText = "student@ucc.edu.ph";
        this.txtEmail.Size = new System.Drawing.Size(317, 23);
        this.txtEmail.TabIndex = 8;
        //
        // lblPassword
        //
        this.lblPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblPassword.AutoSize = true;
        this.lblPassword.Location = new System.Drawing.Point(27, 188);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(60, 15);
        this.lblPassword.TabIndex = 9;
        this.lblPassword.Text = "&Password:";
        //
        // txtPassword
        //
        this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtPassword.Location = new System.Drawing.Point(140, 184);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.Size = new System.Drawing.Size(317, 23);
        this.txtPassword.TabIndex = 10;
        this.txtPassword.UseSystemPasswordChar = true;
        //
        // lblConfirm
        //
        this.lblConfirm.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblConfirm.AutoSize = true;
        this.lblConfirm.Location = new System.Drawing.Point(27, 220);
        this.lblConfirm.Name = "lblConfirm";
        this.lblConfirm.Size = new System.Drawing.Size(107, 15);
        this.lblConfirm.TabIndex = 11;
        this.lblConfirm.Text = "&Confirm Password:";
        //
        // txtConfirmPassword
        //
        this.txtConfirmPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.txtConfirmPassword.Location = new System.Drawing.Point(140, 216);
        this.txtConfirmPassword.Name = "txtConfirmPassword";
        this.txtConfirmPassword.Size = new System.Drawing.Size(317, 23);
        this.txtConfirmPassword.TabIndex = 12;
        this.txtConfirmPassword.UseSystemPasswordChar = true;
        //
        // lblPasswordHint
        //
        this.lblPasswordHint.AutoSize = true;
        this.lblPasswordHint.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblPasswordHint.Location = new System.Drawing.Point(27, 246);
        this.lblPasswordHint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        this.lblPasswordHint.Name = "lblPasswordHint";
        this.lblPasswordHint.Size = new System.Drawing.Size(330, 15);
        this.lblPasswordHint.TabIndex = 13;
        this.lblPasswordHint.Text = "8+ characters with at least one digit and one symbol.";
        //
        // flpButtons
        //
        this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.flpButtons.AutoSize = true;
        this.flpButtons.Controls.Add(this.btnRegister);
        this.flpButtons.Controls.Add(this.btnCancel);
        this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.flpButtons.Location = new System.Drawing.Point(271, 270);
        this.flpButtons.Name = "flpButtons";
        this.flpButtons.Size = new System.Drawing.Size(186, 29);
        this.flpButtons.TabIndex = 14;
        //
        // btnRegister
        //
        this.btnRegister.Location = new System.Drawing.Point(93, 3);
        this.btnRegister.Name = "btnRegister";
        this.btnRegister.Size = new System.Drawing.Size(90, 26);
        this.btnRegister.TabIndex = 0;
        this.btnRegister.Text = "&Register";
        this.btnRegister.UseVisualStyleBackColor = true;
        this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
        //
        // btnCancel
        //
        this.btnCancel.Location = new System.Drawing.Point(3, 3);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(84, 26);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        //
        // RegisterForm
        //
        this.AcceptButton = this.btnRegister;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(484, 471);
        this.Controls.Add(this.tlpMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "RegisterForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Repo Vault - Student Registration";
        this.tlpMain.ResumeLayout(false);
        this.tlpMain.PerformLayout();
        this.flpButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.TableLayoutPanel tlpMain;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblIdNumber;
    private System.Windows.Forms.TextBox txtIdNumber;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblFullName;
    private System.Windows.Forms.TextBox txtFullName;
    private System.Windows.Forms.Label lblEmail;
    private System.Windows.Forms.TextBox txtEmail;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblConfirm;
    private System.Windows.Forms.TextBox txtConfirmPassword;
    private System.Windows.Forms.Label lblPasswordHint;
    private System.Windows.Forms.FlowLayoutPanel flpButtons;
    private System.Windows.Forms.Button btnRegister;
    private System.Windows.Forms.Button btnCancel;
}
