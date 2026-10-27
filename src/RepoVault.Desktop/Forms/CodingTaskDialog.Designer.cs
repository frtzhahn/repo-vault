namespace RepoVault.Desktop.Forms;

partial class CodingTaskDialog
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
        this.lblInstructions = new System.Windows.Forms.Label();
        this.txtInstructions = new System.Windows.Forms.TextBox();
        this.lblTemplate = new System.Windows.Forms.Label();
        this.cboTemplate = new System.Windows.Forms.ComboBox();
        this.chkHasDeadline = new System.Windows.Forms.CheckBox();
        this.dtpDeadline = new System.Windows.Forms.DateTimePicker();
        this.chkAllowGroupWork = new System.Windows.Forms.CheckBox();
        this.lblMaxGroup = new System.Windows.Forms.Label();
        this.numMaxGroupSize = new System.Windows.Forms.NumericUpDown();
        this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnCreate = new System.Windows.Forms.Button();
        this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
        ((System.ComponentModel.ISupportInitialize)(this.numMaxGroupSize)).BeginInit();
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
        this.lblHeader.Size = new System.Drawing.Size(147, 20);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "Create Coding Task";
        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Location = new System.Drawing.Point(16, 48);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(58, 15);
        this.lblTitle.TabIndex = 1;
        this.lblTitle.Text = "Task Title:";
        //
        // txtTitle
        //
        this.txtTitle.Location = new System.Drawing.Point(16, 68);
        this.txtTitle.Name = "txtTitle";
        this.txtTitle.Size = new System.Drawing.Size(380, 23);
        this.txtTitle.TabIndex = 2;
        //
        // lblInstructions
        //
        this.lblInstructions.AutoSize = true;
        this.lblInstructions.Location = new System.Drawing.Point(16, 102);
        this.lblInstructions.Name = "lblInstructions";
        this.lblInstructions.Size = new System.Drawing.Size(126, 15);
        this.lblInstructions.TabIndex = 3;
        this.lblInstructions.Text = "Instructions & Overview:";
        //
        // txtInstructions
        //
        this.txtInstructions.Location = new System.Drawing.Point(16, 122);
        this.txtInstructions.Multiline = true;
        this.txtInstructions.Name = "txtInstructions";
        this.txtInstructions.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtInstructions.Size = new System.Drawing.Size(380, 80);
        this.txtInstructions.TabIndex = 4;
        //
        // lblTemplate
        //
        this.lblTemplate.AutoSize = true;
        this.lblTemplate.Location = new System.Drawing.Point(16, 212);
        this.lblTemplate.Name = "lblTemplate";
        this.lblTemplate.Size = new System.Drawing.Size(133, 15);
        this.lblTemplate.TabIndex = 5;
        this.lblTemplate.Text = "Starter Code Template:";
        //
        // cboTemplate
        //
        this.cboTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboTemplate.FormattingEnabled = true;
        this.cboTemplate.Items.AddRange(new object[] {
            "csharp",
            "java",
            "c",
            "python"});
        this.cboTemplate.Location = new System.Drawing.Point(16, 232);
        this.cboTemplate.Name = "cboTemplate";
        this.cboTemplate.Size = new System.Drawing.Size(380, 23);
        this.cboTemplate.TabIndex = 6;
        //
        // chkHasDeadline
        //
        this.chkHasDeadline.AutoSize = true;
        this.chkHasDeadline.Checked = true;
        this.chkHasDeadline.CheckState = System.Windows.Forms.CheckState.Checked;
        this.chkHasDeadline.Location = new System.Drawing.Point(16, 272);
        this.chkHasDeadline.Name = "chkHasDeadline";
        this.chkHasDeadline.Size = new System.Drawing.Size(158, 19);
        this.chkHasDeadline.TabIndex = 7;
        this.chkHasDeadline.Text = "Set Submission Deadline:";
        this.chkHasDeadline.UseVisualStyleBackColor = true;
        this.chkHasDeadline.CheckedChanged += new System.EventHandler(this.chkHasDeadline_CheckedChanged);
        //
        // dtpDeadline
        //
        this.dtpDeadline.CustomFormat = "yyyy-MM-dd HH:mm";
        this.dtpDeadline.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpDeadline.Location = new System.Drawing.Point(16, 296);
        this.dtpDeadline.Name = "dtpDeadline";
        this.dtpDeadline.Size = new System.Drawing.Size(380, 23);
        this.dtpDeadline.TabIndex = 8;
        //
        // chkAllowGroupWork
        //
        this.chkAllowGroupWork.AutoSize = true;
        this.chkAllowGroupWork.Location = new System.Drawing.Point(16, 334);
        this.chkAllowGroupWork.Name = "chkAllowGroupWork";
        this.chkAllowGroupWork.Size = new System.Drawing.Size(206, 19);
        this.chkAllowGroupWork.TabIndex = 9;
        this.chkAllowGroupWork.Text = "Allow Groupmate Collaboration";
        this.chkAllowGroupWork.UseVisualStyleBackColor = true;
        this.chkAllowGroupWork.CheckedChanged += new System.EventHandler(this.chkAllowGroupWork_CheckedChanged);
        //
        // lblMaxGroup
        //
        this.lblMaxGroup.AutoSize = true;
        this.lblMaxGroup.Location = new System.Drawing.Point(16, 362);
        this.lblMaxGroup.Name = "lblMaxGroup";
        this.lblMaxGroup.Size = new System.Drawing.Size(126, 15);
        this.lblMaxGroup.TabIndex = 10;
        this.lblMaxGroup.Text = "Maximum Group Size:";
        //
        // numMaxGroupSize
        //
        this.numMaxGroupSize.Enabled = false;
        this.numMaxGroupSize.Location = new System.Drawing.Point(160, 360);
        this.numMaxGroupSize.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        this.numMaxGroupSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numMaxGroupSize.Name = "numMaxGroupSize";
        this.numMaxGroupSize.Size = new System.Drawing.Size(80, 23);
        this.numMaxGroupSize.TabIndex = 11;
        this.numMaxGroupSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numMaxGroupSize.Value = new decimal(new int[] { 1, 0, 0, 0 });
        //
        // pnlButtons
        //
        this.pnlButtons.Controls.Add(this.btnCancel);
        this.pnlButtons.Controls.Add(this.btnCreate);
        this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlButtons.Location = new System.Drawing.Point(0, 408);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this.pnlButtons.Size = new System.Drawing.Size(414, 44);
        this.pnlButtons.TabIndex = 12;
        //
        // btnCancel
        //
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(307, 11);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(80, 26);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        //
        // btnCreate
        //
        this.btnCreate.Location = new System.Drawing.Point(161, 11);
        this.btnCreate.Name = "btnCreate";
        this.btnCreate.Size = new System.Drawing.Size(140, 26);
        this.btnCreate.TabIndex = 0;
        this.btnCreate.Text = "Create Coding Task";
        this.btnCreate.UseVisualStyleBackColor = true;
        this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
        //
        // errorProvider
        //
        this.errorProvider.ContainerControl = this;
        //
        // CodingTaskDialog
        //
        this.AcceptButton = this.btnCreate;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(414, 452);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.numMaxGroupSize);
        this.Controls.Add(this.lblMaxGroup);
        this.Controls.Add(this.chkAllowGroupWork);
        this.Controls.Add(this.dtpDeadline);
        this.Controls.Add(this.chkHasDeadline);
        this.Controls.Add(this.cboTemplate);
        this.Controls.Add(this.lblTemplate);
        this.Controls.Add(this.txtInstructions);
        this.Controls.Add(this.lblInstructions);
        this.Controls.Add(this.txtTitle);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CodingTaskDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Create Coding Task";
        ((System.ComponentModel.ISupportInitialize)(this.numMaxGroupSize)).EndInit();
        this.pnlButtons.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.TextBox txtTitle;
    private System.Windows.Forms.Label lblInstructions;
    private System.Windows.Forms.TextBox txtInstructions;
    private System.Windows.Forms.Label lblTemplate;
    private System.Windows.Forms.ComboBox cboTemplate;
    private System.Windows.Forms.CheckBox chkHasDeadline;
    private System.Windows.Forms.DateTimePicker dtpDeadline;
    private System.Windows.Forms.CheckBox chkAllowGroupWork;
    private System.Windows.Forms.Label lblMaxGroup;
    private System.Windows.Forms.NumericUpDown numMaxGroupSize;
    private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnCreate;
    private System.Windows.Forms.ErrorProvider errorProvider;
}
