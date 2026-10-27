namespace RepoVault.Desktop.Forms;

partial class RepositoryEditorDialog
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
        this.lblHeader = new System.Windows.Forms.Label();
        this.lblTitle = new System.Windows.Forms.Label();
        this.txtTitle = new System.Windows.Forms.TextBox();
        this.lblDesc = new System.Windows.Forms.Label();
        this.txtDesc = new System.Windows.Forms.TextBox();
        this.chkHasDeadline = new System.Windows.Forms.CheckBox();
        this.dtpDeadline = new System.Windows.Forms.DateTimePicker();
        this.chkLock = new System.Windows.Forms.CheckBox();
        this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnSave = new System.Windows.Forms.Button();
        this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
        this.pnlButtons.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
        this.SuspendLayout();
        //
        // lblHeader
        //
        this.lblHeader.AutoSize = true;
        this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold);
        this.lblHeader.Location = new System.Drawing.Point(16, 16);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(149, 20);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "Repository Settings";
        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Location = new System.Drawing.Point(16, 48);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(95, 15);
        this.lblTitle.TabIndex = 1;
        this.lblTitle.Text = "Repository Title:";
        //
        // txtTitle
        //
        this.txtTitle.Location = new System.Drawing.Point(16, 68);
        this.txtTitle.Name = "txtTitle";
        this.txtTitle.Size = new System.Drawing.Size(350, 23);
        this.txtTitle.TabIndex = 2;
        //
        // lblDesc
        //
        this.lblDesc.AutoSize = true;
        this.lblDesc.Location = new System.Drawing.Point(16, 102);
        this.lblDesc.Name = "lblDesc";
        this.lblDesc.Size = new System.Drawing.Size(70, 15);
        this.lblDesc.TabIndex = 3;
        this.lblDesc.Text = "Description:";
        //
        // txtDesc
        //
        this.txtDesc.Location = new System.Drawing.Point(16, 122);
        this.txtDesc.Multiline = true;
        this.txtDesc.Name = "txtDesc";
        this.txtDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtDesc.Size = new System.Drawing.Size(350, 70);
        this.txtDesc.TabIndex = 4;
        //
        // chkHasDeadline
        //
        this.chkHasDeadline.AutoSize = true;
        this.chkHasDeadline.Location = new System.Drawing.Point(16, 204);
        this.chkHasDeadline.Name = "chkHasDeadline";
        this.chkHasDeadline.Size = new System.Drawing.Size(158, 19);
        this.chkHasDeadline.TabIndex = 5;
        this.chkHasDeadline.Text = "Set Submission Deadline:";
        this.chkHasDeadline.UseVisualStyleBackColor = true;
        this.chkHasDeadline.CheckedChanged += new System.EventHandler(this.chkHasDeadline_CheckedChanged);
        //
        // dtpDeadline
        //
        this.dtpDeadline.CustomFormat = "yyyy-MM-dd HH:mm";
        this.dtpDeadline.Enabled = false;
        this.dtpDeadline.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpDeadline.Location = new System.Drawing.Point(16, 228);
        this.dtpDeadline.Name = "dtpDeadline";
        this.dtpDeadline.Size = new System.Drawing.Size(350, 23);
        this.dtpDeadline.TabIndex = 6;
        //
        // chkLock
        //
        this.chkLock.AutoSize = true;
        this.chkLock.Location = new System.Drawing.Point(16, 268);
        this.chkLock.Name = "chkLock";
        this.chkLock.Size = new System.Drawing.Size(306, 19);
        this.chkLock.TabIndex = 7;
        this.chkLock.Text = "Lock Repository (prevent students from git pushing)";
        this.chkLock.UseVisualStyleBackColor = true;
        //
        // pnlButtons
        //
        this.pnlButtons.Controls.Add(this.btnCancel);
        this.pnlButtons.Controls.Add(this.btnSave);
        this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlButtons.Location = new System.Drawing.Point(0, 308);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this.pnlButtons.Size = new System.Drawing.Size(384, 44);
        this.pnlButtons.TabIndex = 8;
        //
        // btnCancel
        //
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(277, 11);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(80, 26);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(171, 11);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(100, 26);
        this.btnSave.TabIndex = 0;
        this.btnSave.Text = "Save Settings";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        //
        // errorProvider
        //
        this.errorProvider.ContainerControl = this;
        //
        // RepositoryEditorDialog
        //
        this.AcceptButton = this.btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(384, 352);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.chkLock);
        this.Controls.Add(this.dtpDeadline);
        this.Controls.Add(this.chkHasDeadline);
        this.Controls.Add(this.txtDesc);
        this.Controls.Add(this.lblDesc);
        this.Controls.Add(this.txtTitle);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "RepositoryEditorDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Repository Settings";
        this.pnlButtons.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.TextBox txtTitle;
    private System.Windows.Forms.Label lblDesc;
    private System.Windows.Forms.TextBox txtDesc;
    private System.Windows.Forms.CheckBox chkHasDeadline;
    private System.Windows.Forms.DateTimePicker dtpDeadline;
    private System.Windows.Forms.CheckBox chkLock;
    private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.ErrorProvider errorProvider;
}
