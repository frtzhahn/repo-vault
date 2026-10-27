namespace RepoVault.Desktop.Forms;

partial class CourseEditorDialog
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
        this.lblCode = new System.Windows.Forms.Label();
        this.txtCourseCode = new System.Windows.Forms.TextBox();
        this.lblCourseTitle = new System.Windows.Forms.Label();
        this.txtCourseTitle = new System.Windows.Forms.TextBox();
        this.lblDepartment = new System.Windows.Forms.Label();
        this.txtDepartment = new System.Windows.Forms.TextBox();
        this.lblSemester = new System.Windows.Forms.Label();
        this.txtSemester = new System.Windows.Forms.TextBox();
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
        this.lblHeader.Size = new System.Drawing.Size(108, 20);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "Course Details";
        //
        // lblCode
        //
        this.lblCode.AutoSize = true;
        this.lblCode.Location = new System.Drawing.Point(16, 48);
        this.lblCode.Name = "lblCode";
        this.lblCode.Size = new System.Drawing.Size(78, 15);
        this.lblCode.TabIndex = 1;
        this.lblCode.Text = "Course Code:";
        //
        // txtCourseCode
        //
        this.txtCourseCode.Location = new System.Drawing.Point(16, 68);
        this.txtCourseCode.Name = "txtCourseCode";
        this.txtCourseCode.Size = new System.Drawing.Size(350, 23);
        this.txtCourseCode.TabIndex = 2;
        //
        // lblCourseTitle
        //
        this.lblCourseTitle.AutoSize = true;
        this.lblCourseTitle.Location = new System.Drawing.Point(16, 102);
        this.lblCourseTitle.Name = "lblCourseTitle";
        this.lblCourseTitle.Size = new System.Drawing.Size(72, 15);
        this.lblCourseTitle.TabIndex = 3;
        this.lblCourseTitle.Text = "Course Title:";
        //
        // txtCourseTitle
        //
        this.txtCourseTitle.Location = new System.Drawing.Point(16, 122);
        this.txtCourseTitle.Name = "txtCourseTitle";
        this.txtCourseTitle.Size = new System.Drawing.Size(350, 23);
        this.txtCourseTitle.TabIndex = 4;
        //
        // lblDepartment
        //
        this.lblDepartment.AutoSize = true;
        this.lblDepartment.Location = new System.Drawing.Point(16, 156);
        this.lblDepartment.Name = "lblDepartment";
        this.lblDepartment.Size = new System.Drawing.Size(73, 15);
        this.lblDepartment.TabIndex = 5;
        this.lblDepartment.Text = "Department:";
        //
        // txtDepartment
        //
        this.txtDepartment.Location = new System.Drawing.Point(16, 176);
        this.txtDepartment.Name = "txtDepartment";
        this.txtDepartment.Size = new System.Drawing.Size(350, 23);
        this.txtDepartment.TabIndex = 6;
        this.txtDepartment.Text = "BSCS";
        //
        // lblSemester
        //
        this.lblSemester.AutoSize = true;
        this.lblSemester.Location = new System.Drawing.Point(16, 210);
        this.lblSemester.Name = "lblSemester";
        this.lblSemester.Size = new System.Drawing.Size(58, 15);
        this.lblSemester.TabIndex = 7;
        this.lblSemester.Text = "Semester:";
        //
        // txtSemester
        //
        this.txtSemester.Location = new System.Drawing.Point(16, 230);
        this.txtSemester.Name = "txtSemester";
        this.txtSemester.Size = new System.Drawing.Size(350, 23);
        this.txtSemester.TabIndex = 8;
        this.txtSemester.Text = "1st Sem 2026-2027";
        //
        // pnlButtons
        //
        this.pnlButtons.Controls.Add(this.btnCancel);
        this.pnlButtons.Controls.Add(this.btnSave);
        this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlButtons.Location = new System.Drawing.Point(0, 276);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this.pnlButtons.Size = new System.Drawing.Size(384, 44);
        this.pnlButtons.TabIndex = 9;
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
        this.btnSave.Text = "Save Course";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        //
        // errorProvider
        //
        this.errorProvider.ContainerControl = this;
        //
        // CourseEditorDialog
        //
        this.AcceptButton = this.btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(384, 320);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.txtSemester);
        this.Controls.Add(this.lblSemester);
        this.Controls.Add(this.txtDepartment);
        this.Controls.Add(this.lblDepartment);
        this.Controls.Add(this.txtCourseTitle);
        this.Controls.Add(this.lblCourseTitle);
        this.Controls.Add(this.txtCourseCode);
        this.Controls.Add(this.lblCode);
        this.Controls.Add(this.lblHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CourseEditorDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Course Editor";
        this.pnlButtons.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblCode;
    private System.Windows.Forms.TextBox txtCourseCode;
    private System.Windows.Forms.Label lblCourseTitle;
    private System.Windows.Forms.TextBox txtCourseTitle;
    private System.Windows.Forms.Label lblDepartment;
    private System.Windows.Forms.TextBox txtDepartment;
    private System.Windows.Forms.Label lblSemester;
    private System.Windows.Forms.TextBox txtSemester;
    private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.ErrorProvider errorProvider;
}
