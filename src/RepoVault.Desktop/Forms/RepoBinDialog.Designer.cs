namespace RepoVault.Desktop.Forms;

partial class RepoBinDialog
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
        this.pnlHeader = new System.Windows.Forms.Panel();
        this.lblSubtitle = new System.Windows.Forms.Label();
        this.lblTitle = new System.Windows.Forms.Label();
        this.dgvRepoBin = new System.Windows.Forms.DataGridView();
        this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.btnClose = new System.Windows.Forms.Button();
        this.btnPurge = new System.Windows.Forms.Button();
        this.btnRestore = new System.Windows.Forms.Button();
        this.pnlHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRepoBin)).BeginInit();
        this.pnlButtons.SuspendLayout();
        this.SuspendLayout();
        //
        // pnlHeader
        //
        this.pnlHeader.Controls.Add(this.lblSubtitle);
        this.pnlHeader.Controls.Add(this.lblTitle);
        this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlHeader.Location = new System.Drawing.Point(0, 0);
        this.pnlHeader.Name = "pnlHeader";
        this.pnlHeader.Padding = new System.Windows.Forms.Padding(16, 14, 16, 8);
        this.pnlHeader.Size = new System.Drawing.Size(720, 60);
        this.pnlHeader.TabIndex = 0;
        //
        // lblSubtitle
        //
        this.lblSubtitle.AutoSize = true;
        this.lblSubtitle.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblSubtitle.Location = new System.Drawing.Point(16, 36);
        this.lblSubtitle.Name = "lblSubtitle";
        this.lblSubtitle.Size = new System.Drawing.Size(496, 15);
        this.lblSubtitle.TabIndex = 1;
        this.lblSubtitle.Text = "Trashed repositories can be restored back to Active status, or permanently purged from disk.";
        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblTitle.Location = new System.Drawing.Point(16, 12);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(127, 21);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "Repository Bin";
        //
        // dgvRepoBin
        //
        this.dgvRepoBin.AllowUserToAddRows = false;
        this.dgvRepoBin.AllowUserToDeleteRows = false;
        this.dgvRepoBin.AllowUserToResizeRows = false;
        this.dgvRepoBin.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvRepoBin.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvRepoBin.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvRepoBin.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvRepoBin.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvRepoBin.Location = new System.Drawing.Point(0, 60);
        this.dgvRepoBin.MultiSelect = false;
        this.dgvRepoBin.Name = "dgvRepoBin";
        this.dgvRepoBin.ReadOnly = true;
        this.dgvRepoBin.RowHeadersVisible = false;
        this.dgvRepoBin.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvRepoBin.Size = new System.Drawing.Size(720, 316);
        this.dgvRepoBin.TabIndex = 1;
        //
        // pnlButtons
        //
        this.pnlButtons.Controls.Add(this.btnClose);
        this.pnlButtons.Controls.Add(this.btnPurge);
        this.pnlButtons.Controls.Add(this.btnRestore);
        this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.pnlButtons.Location = new System.Drawing.Point(0, 376);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
        this.pnlButtons.Size = new System.Drawing.Size(720, 44);
        this.pnlButtons.TabIndex = 2;
        //
        // btnClose
        //
        this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnClose.Location = new System.Drawing.Point(613, 11);
        this.btnClose.Name = "btnClose";
        this.btnClose.Size = new System.Drawing.Size(80, 26);
        this.btnClose.TabIndex = 2;
        this.btnClose.Text = "Close";
        this.btnClose.UseVisualStyleBackColor = true;
        //
        // btnPurge
        //
        this.btnPurge.Location = new System.Drawing.Point(477, 11);
        this.btnPurge.Name = "btnPurge";
        this.btnPurge.Size = new System.Drawing.Size(130, 26);
        this.btnPurge.TabIndex = 1;
        this.btnPurge.Text = "Purge Permanently";
        this.btnPurge.UseVisualStyleBackColor = true;
        this.btnPurge.Click += new System.EventHandler(this.btnPurge_Click);
        //
        // btnRestore
        //
        this.btnRestore.Location = new System.Drawing.Point(341, 11);
        this.btnRestore.Name = "btnRestore";
        this.btnRestore.Size = new System.Drawing.Size(130, 26);
        this.btnRestore.TabIndex = 0;
        this.btnRestore.Text = "Restore Selected";
        this.btnRestore.UseVisualStyleBackColor = true;
        this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
        //
        // RepoBinDialog
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.btnClose;
        this.ClientSize = new System.Drawing.Size(720, 420);
        this.Controls.Add(this.dgvRepoBin);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.pnlHeader);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "RepoBinDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Repository Bin - Trashed Repositories";
        this.pnlHeader.ResumeLayout(false);
        this.pnlHeader.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRepoBin)).EndInit();
        this.pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel pnlHeader;
    private System.Windows.Forms.Label lblSubtitle;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.DataGridView dgvRepoBin;
    private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Button btnPurge;
    private System.Windows.Forms.Button btnRestore;
}
