namespace RepoVault.Desktop.Forms;

partial class GradeRubricDialog
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
        this.lblHeader = new System.Windows.Forms.Label();
        this.lblCourseAndTeam = new System.Windows.Forms.Label();
        this.lblMetrics = new System.Windows.Forms.Label();
        this.tblScores = new System.Windows.Forms.TableLayoutPanel();
        this.lblArchTitle = new System.Windows.Forms.Label();
        this.nudArch = new System.Windows.Forms.NumericUpDown();
        this.lblFuncTitle = new System.Windows.Forms.Label();
        this.nudFunc = new System.Windows.Forms.NumericUpDown();
        this.lblQualTitle = new System.Windows.Forms.Label();
        this.nudQual = new System.Windows.Forms.NumericUpDown();
        this.lblDocTitle = new System.Windows.Forms.Label();
        this.nudDoc = new System.Windows.Forms.NumericUpDown();
        this.lblTotalScore = new System.Windows.Forms.Label();
        this.chkLockRepo = new System.Windows.Forms.CheckBox();
        this.lblNotesHeader = new System.Windows.Forms.Label();
        this.txtNotes = new System.Windows.Forms.TextBox();
        this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnSave = new System.Windows.Forms.Button();
        this.tblScores.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudArch)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudFunc)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudQual)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudDoc)).BeginInit();
        this.pnlButtons.SuspendLayout();
        this.SuspendLayout();
        //
        // lblHeader
        //
        this.lblHeader.AutoSize = true;
        this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblHeader.Location = new System.Drawing.Point(18, 16);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(142, 21);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "Grade Repository";
        //
        // lblCourseAndTeam
        //
        this.lblCourseAndTeam.AutoSize = true;
        this.lblCourseAndTeam.Location = new System.Drawing.Point(18, 42);
        this.lblCourseAndTeam.Name = "lblCourseAndTeam";
        this.lblCourseAndTeam.Size = new System.Drawing.Size(129, 15);
        this.lblCourseAndTeam.TabIndex = 1;
        this.lblCourseAndTeam.Text = "Course: -- | Student: --";
        //
        // lblMetrics
        //
        this.lblMetrics.AutoSize = true;
        this.lblMetrics.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetrics.Location = new System.Drawing.Point(18, 62);
        this.lblMetrics.Name = "lblMetrics";
        this.lblMetrics.Size = new System.Drawing.Size(107, 15);
        this.lblMetrics.TabIndex = 2;
        this.lblMetrics.Text = "Code Metrics: --";
        //
        // tblScores
        //
        this.tblScores.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.tblScores.ColumnCount = 2;
        this.tblScores.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72F));
        this.tblScores.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
        this.tblScores.Controls.Add(this.lblArchTitle, 0, 0);
        this.tblScores.Controls.Add(this.nudArch, 1, 0);
        this.tblScores.Controls.Add(this.lblFuncTitle, 0, 1);
        this.tblScores.Controls.Add(this.nudFunc, 1, 1);
        this.tblScores.Controls.Add(this.lblQualTitle, 0, 2);
        this.tblScores.Controls.Add(this.nudQual, 1, 2);
        this.tblScores.Controls.Add(this.lblDocTitle, 0, 3);
        this.tblScores.Controls.Add(this.nudDoc, 1, 3);
        this.tblScores.Location = new System.Drawing.Point(18, 92);
        this.tblScores.Name = "tblScores";
        this.tblScores.RowCount = 4;
        this.tblScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this.tblScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this.tblScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this.tblScores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this.tblScores.Size = new System.Drawing.Size(490, 128);
        this.tblScores.TabIndex = 3;
        //
        // lblArchTitle
        //
        this.lblArchTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblArchTitle.AutoSize = true;
        this.lblArchTitle.Location = new System.Drawing.Point(3, 8);
        this.lblArchTitle.Name = "lblArchTitle";
        this.lblArchTitle.Size = new System.Drawing.Size(155, 15);
        this.lblArchTitle.TabIndex = 0;
        this.lblArchTitle.Text = "Architecture & Design (0-25):";
        //
        // nudArch
        //
        this.nudArch.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.nudArch.Location = new System.Drawing.Point(355, 4);
        this.nudArch.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
        this.nudArch.Name = "nudArch";
        this.nudArch.Size = new System.Drawing.Size(80, 23);
        this.nudArch.TabIndex = 1;
        this.nudArch.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.nudArch.Value = new decimal(new int[] { 20, 0, 0, 0 });
        this.nudArch.ValueChanged += new System.EventHandler(this.ScoreInput_ValueChanged);
        //
        // lblFuncTitle
        //
        this.lblFuncTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblFuncTitle.AutoSize = true;
        this.lblFuncTitle.Location = new System.Drawing.Point(3, 40);
        this.lblFuncTitle.Name = "lblFuncTitle";
        this.lblFuncTitle.Size = new System.Drawing.Size(180, 15);
        this.lblFuncTitle.TabIndex = 2;
        this.lblFuncTitle.Text = "Functionality & Correctness (0-25):";
        //
        // nudFunc
        //
        this.nudFunc.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.nudFunc.Location = new System.Drawing.Point(355, 36);
        this.nudFunc.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
        this.nudFunc.Name = "nudFunc";
        this.nudFunc.Size = new System.Drawing.Size(80, 23);
        this.nudFunc.TabIndex = 3;
        this.nudFunc.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.nudFunc.Value = new decimal(new int[] { 20, 0, 0, 0 });
        this.nudFunc.ValueChanged += new System.EventHandler(this.ScoreInput_ValueChanged);
        //
        // lblQualTitle
        //
        this.lblQualTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblQualTitle.AutoSize = true;
        this.lblQualTitle.Location = new System.Drawing.Point(3, 72);
        this.lblQualTitle.Name = "lblQualTitle";
        this.lblQualTitle.Size = new System.Drawing.Size(161, 15);
        this.lblQualTitle.TabIndex = 4;
        this.lblQualTitle.Text = "Code Quality & Cleanliness (0-25):";
        //
        // nudQual
        //
        this.nudQual.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.nudQual.Location = new System.Drawing.Point(355, 68);
        this.nudQual.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
        this.nudQual.Name = "nudQual";
        this.nudQual.Size = new System.Drawing.Size(80, 23);
        this.nudQual.TabIndex = 5;
        this.nudQual.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.nudQual.Value = new decimal(new int[] { 20, 0, 0, 0 });
        this.nudQual.ValueChanged += new System.EventHandler(this.ScoreInput_ValueChanged);
        //
        // lblDocTitle
        //
        this.lblDocTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblDocTitle.AutoSize = true;
        this.lblDocTitle.Location = new System.Drawing.Point(3, 104);
        this.lblDocTitle.Name = "lblDocTitle";
        this.lblDocTitle.Size = new System.Drawing.Size(169, 15);
        this.lblDocTitle.TabIndex = 6;
        this.lblDocTitle.Text = "Documentation & Readme (0-25):";
        //
        // nudDoc
        //
        this.nudDoc.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.nudDoc.Location = new System.Drawing.Point(355, 100);
        this.nudDoc.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
        this.nudDoc.Name = "nudDoc";
        this.nudDoc.Size = new System.Drawing.Size(80, 23);
        this.nudDoc.TabIndex = 7;
        this.nudDoc.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.nudDoc.Value = new decimal(new int[] { 20, 0, 0, 0 });
        this.nudDoc.ValueChanged += new System.EventHandler(this.ScoreInput_ValueChanged);
        //
        // lblTotalScore
        //
        this.lblTotalScore.AutoSize = true;
        this.lblTotalScore.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        this.lblTotalScore.Location = new System.Drawing.Point(18, 232);
        this.lblTotalScore.Name = "lblTotalScore";
        this.lblTotalScore.Size = new System.Drawing.Size(192, 25);
        this.lblTotalScore.TabIndex = 4;
        this.lblTotalScore.Text = "Total Score: 80 / 100";
        //
        // chkLockRepo
        //
        this.chkLockRepo.AutoSize = true;
        this.chkLockRepo.Checked = true;
        this.chkLockRepo.CheckState = System.Windows.Forms.CheckState.Checked;
        this.chkLockRepo.Location = new System.Drawing.Point(18, 268);
        this.chkLockRepo.Name = "chkLockRepo";
        this.chkLockRepo.Size = new System.Drawing.Size(378, 19);
        this.chkLockRepo.TabIndex = 5;
        this.chkLockRepo.Text = "Lock repository after grading (freeze submissions and student pushes)";
        this.chkLockRepo.UseVisualStyleBackColor = true;
        //
        // lblNotesHeader
        //
        this.lblNotesHeader.AutoSize = true;
        this.lblNotesHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblNotesHeader.Location = new System.Drawing.Point(18, 302);
        this.lblNotesHeader.Name = "lblNotesHeader";
        this.lblNotesHeader.Size = new System.Drawing.Size(175, 15);
        this.lblNotesHeader.TabIndex = 6;
        this.lblNotesHeader.Text = "Faculty Critique & Critique Notes:";
        //
        // txtNotes
        //
        this.txtNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.txtNotes.Location = new System.Drawing.Point(18, 324);
        this.txtNotes.Multiline = true;
        this.txtNotes.Name = "txtNotes";
        this.txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtNotes.Size = new System.Drawing.Size(490, 150);
        this.txtNotes.TabIndex = 7;
        //
        // pnlButtons
        //
        this.pnlButtons.Controls.Add(this.btnCancel);
        this.pnlButtons.Controls.Add(this.btnSave);
        this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlButtons.Location = new System.Drawing.Point(0, 488);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this.pnlButtons.Size = new System.Drawing.Size(526, 44);
        this.pnlButtons.TabIndex = 8;
        //
        // btnCancel
        //
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(419, 11);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(80, 26);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(283, 11);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(130, 26);
        this.btnSave.TabIndex = 0;
        this.btnSave.Text = "Save Rubric Grade";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        //
        // GradeRubricDialog
        //
        this.AcceptButton = this.btnSave;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(526, 532);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.txtNotes);
        this.Controls.Add(this.lblNotesHeader);
        this.Controls.Add(this.chkLockRepo);
        this.Controls.Add(this.lblTotalScore);
        this.Controls.Add(this.tblScores);
        this.Controls.Add(this.lblMetrics);
        this.Controls.Add(this.lblCourseAndTeam);
        this.Controls.Add(this.lblHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "GradeRubricDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Grade Repository";
        this.tblScores.ResumeLayout(false);
        this.tblScores.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudArch)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudFunc)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudQual)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudDoc)).EndInit();
        this.pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblCourseAndTeam;
    private System.Windows.Forms.Label lblMetrics;
    private System.Windows.Forms.TableLayoutPanel tblScores;
    private System.Windows.Forms.Label lblArchTitle;
    private System.Windows.Forms.NumericUpDown nudArch;
    private System.Windows.Forms.Label lblFuncTitle;
    private System.Windows.Forms.NumericUpDown nudFunc;
    private System.Windows.Forms.Label lblQualTitle;
    private System.Windows.Forms.NumericUpDown nudQual;
    private System.Windows.Forms.Label lblDocTitle;
    private System.Windows.Forms.NumericUpDown nudDoc;
    private System.Windows.Forms.Label lblTotalScore;
    private System.Windows.Forms.CheckBox chkLockRepo;
    private System.Windows.Forms.Label lblNotesHeader;
    private System.Windows.Forms.TextBox txtNotes;
    private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnSave;
}
