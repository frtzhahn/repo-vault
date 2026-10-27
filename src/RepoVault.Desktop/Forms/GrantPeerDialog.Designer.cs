namespace RepoVault.Desktop.Forms;

partial class GrantPeerDialog
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
        this.lblStudent = new System.Windows.Forms.Label();
        this.cboStudents = new System.Windows.Forms.ComboBox();
        this.lblNote = new System.Windows.Forms.Label();
        this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnGrant = new System.Windows.Forms.Button();
        this.pnlButtons.SuspendLayout();
        this.SuspendLayout();
        //
        // lblHeader
        //
        this.lblHeader.AutoSize = true;
        this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold);
        this.lblHeader.Location = new System.Drawing.Point(16, 16);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(183, 20);
        this.lblHeader.TabIndex = 0;
        this.lblHeader.Text = "Grant Peer Viewer Access";
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
        // lblNote
        //
        this.lblNote.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblNote.Location = new System.Drawing.Point(16, 108);
        this.lblNote.Name = "lblNote";
        this.lblNote.Size = new System.Drawing.Size(350, 48);
        this.lblNote.TabIndex = 3;
        this.lblNote.Text = "Peer viewers can inspect repository metadata, commit logs, metrics, and README.md, but cannot view source code or clone/push.";
        //
        // pnlButtons
        //
        this.pnlButtons.Controls.Add(this.btnCancel);
        this.pnlButtons.Controls.Add(this.btnGrant);
        this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlButtons.Location = new System.Drawing.Point(16, 168);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Size = new System.Drawing.Size(350, 36);
        this.pnlButtons.TabIndex = 4;
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
        // btnGrant
        //
        this.btnGrant.Location = new System.Drawing.Point(157, 3);
        this.btnGrant.Name = "btnGrant";
        this.btnGrant.Size = new System.Drawing.Size(102, 28);
        this.btnGrant.TabIndex = 0;
        this.btnGrant.Text = "Grant Access";
        this.btnGrant.UseVisualStyleBackColor = true;
        this.btnGrant.Click += new System.EventHandler(this.btnGrant_Click);
        //
        // GrantPeerDialog
        //
        this.AcceptButton = this.btnGrant;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(384, 216);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.lblNote);
        this.Controls.Add(this.cboStudents);
        this.Controls.Add(this.lblStudent);
        this.Controls.Add(this.lblHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "GrantPeerDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Grant Peer Viewer Access";
        this.pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblStudent;
    private System.Windows.Forms.ComboBox cboStudents;
    private System.Windows.Forms.Label lblNote;
    private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnGrant;
}
