namespace RepoVault.Desktop.Forms;

partial class RequestTicketDialog
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
        this.lblStudent = new System.Windows.Forms.Label();
        this.cboStudents = new System.Windows.Forms.ComboBox();
        this.lblRole = new System.Windows.Forms.Label();
        this.txtTeamRole = new System.Windows.Forms.TextBox();
        this.lblNote = new System.Windows.Forms.Label();
        this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnSubmit = new System.Windows.Forms.Button();
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
        this.lblHeader.Size = new System.Drawing.Size(200, 20);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "Request Groupmate Ticket";
        //
        // lblStudent
        //
        this.lblStudent.AutoSize = true;
        this.lblStudent.Location = new System.Drawing.Point(16, 52);
        this.lblStudent.Name = "lblStudent";
        this.lblStudent.Size = new System.Drawing.Size(63, 15);
        this.lblStudent.TabIndex = 1;
        this.lblStudent.Text = "Classmate:";
        //
        // cboStudents
        //
        this.cboStudents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboStudents.FormattingEnabled = true;
        this.cboStudents.Location = new System.Drawing.Point(16, 72);
        this.cboStudents.Name = "cboStudents";
        this.cboStudents.Size = new System.Drawing.Size(350, 23);
        this.cboStudents.TabIndex = 2;
        //
        // lblRole
        //
        this.lblRole.AutoSize = true;
        this.lblRole.Location = new System.Drawing.Point(16, 110);
        this.lblRole.Name = "lblRole";
        this.lblRole.Size = new System.Drawing.Size(65, 15);
        this.lblRole.TabIndex = 3;
        this.lblRole.Text = "Team Role:";
        //
        // txtTeamRole
        //
        this.txtTeamRole.Location = new System.Drawing.Point(16, 130);
        this.txtTeamRole.Name = "txtTeamRole";
        this.txtTeamRole.Size = new System.Drawing.Size(350, 23);
        this.txtTeamRole.TabIndex = 4;
        this.txtTeamRole.Text = "Developer";
        //
        // lblNote
        //
        this.lblNote.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblNote.Location = new System.Drawing.Point(16, 168);
        this.lblNote.Name = "lblNote";
        this.lblNote.Size = new System.Drawing.Size(350, 48);
        this.lblNote.TabIndex = 5;
        this.lblNote.Text = "Groupmate tickets require instructor approval. Your classmate will not have code or push access until the ticket is approved.";
        //
        // pnlButtons
        //
        this.pnlButtons.Controls.Add(this.btnCancel);
        this.pnlButtons.Controls.Add(this.btnSubmit);
        this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlButtons.Location = new System.Drawing.Point(16, 226);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Size = new System.Drawing.Size(350, 36);
        this.pnlButtons.TabIndex = 6;
        //
        // btnCancel
        //
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(265, 3);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(82, 28);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        //
        // btnSubmit
        //
        this.btnSubmit.Location = new System.Drawing.Point(157, 3);
        this.btnSubmit.Name = "btnSubmit";
        this.btnSubmit.Size = new System.Drawing.Size(102, 28);
        this.btnSubmit.TabIndex = 0;
        this.btnSubmit.Text = "Submit Ticket";
        this.btnSubmit.UseVisualStyleBackColor = true;
        this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
        //
        // errorProvider
        //
        this.errorProvider.ContainerControl = this;
        //
        // RequestTicketDialog
        //
        this.AcceptButton = this.btnSubmit;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(384, 276);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.lblNote);
        this.Controls.Add(this.txtTeamRole);
        this.Controls.Add(this.lblRole);
        this.Controls.Add(this.cboStudents);
        this.Controls.Add(this.lblStudent);
        this.Controls.Add(this.lblHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "RequestTicketDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Request Groupmate Ticket";
        this.pnlButtons.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblStudent;
    private System.Windows.Forms.ComboBox cboStudents;
    private System.Windows.Forms.Label lblRole;
    private System.Windows.Forms.TextBox txtTeamRole;
    private System.Windows.Forms.Label lblNote;
    private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnSubmit;
    private System.Windows.Forms.ErrorProvider errorProvider;
}
