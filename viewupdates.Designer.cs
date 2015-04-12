using System.Windows.Forms;

namespace BASeCamp.Updating
{
    partial class frmUpdates
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmUpdates));
            this.fraavailupdates = new System.Windows.Forms.GroupBox();
            this.lvwUpdates = new System.Windows.Forms.ListView();
            this.panLower = new System.Windows.Forms.Panel();
            this.btnDownload = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.StripContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.grpBasicUpdate = new System.Windows.Forms.Panel();
            this.lblRemaining = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblDlRate = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.lblImmProgress = new System.Windows.Forms.Label();
            this.lblImmSpeed = new System.Windows.Forms.Label();
            this.lblAction = new System.Windows.Forms.Label();
            this.lblImmAction = new System.Windows.Forms.Label();
            this.pbarImmediate = new System.Windows.Forms.ProgressBar();
            this.panRefreshing = new System.Windows.Forms.Panel();
            this.txtException = new System.Windows.Forms.TextBox();
            this.lblrefreshing = new System.Windows.Forms.Label();
            this.fraavailupdates.SuspendLayout();
            this.panLower.SuspendLayout();
            this.StripContext.SuspendLayout();
            this.grpBasicUpdate.SuspendLayout();
            this.panRefreshing.SuspendLayout();
            this.SuspendLayout();
            // 
            // fraavailupdates
            // 
            this.fraavailupdates.Controls.Add(this.lvwUpdates);
            this.fraavailupdates.Location = new System.Drawing.Point(2, 1);
            this.fraavailupdates.Name = "fraavailupdates";
            this.fraavailupdates.Size = new System.Drawing.Size(403, 232);
            this.fraavailupdates.TabIndex = 2;
            this.fraavailupdates.TabStop = false;
            this.fraavailupdates.Text = "Available Updates";
            this.fraavailupdates.Resize += new System.EventHandler(this.fraavailupdates_Resize);
            // 
            // lvwUpdates
            // 
            this.lvwUpdates.CheckBoxes = true;
            this.lvwUpdates.FullRowSelect = true;
            this.lvwUpdates.GridLines = true;
            this.lvwUpdates.HideSelection = false;
            this.lvwUpdates.Location = new System.Drawing.Point(0, 19);
            this.lvwUpdates.Name = "lvwUpdates";
            this.lvwUpdates.OwnerDraw = true;
            this.lvwUpdates.Size = new System.Drawing.Size(397, 207);
            this.lvwUpdates.TabIndex = 1;
            this.lvwUpdates.UseCompatibleStateImageBehavior = false;
            this.lvwUpdates.View = System.Windows.Forms.View.Details;
            this.lvwUpdates.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.lvwUpdates_DrawColumnHeader);
            this.lvwUpdates.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lvwUpdates_DrawItem);
            this.lvwUpdates.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.lvwUpdates_DrawSubItem);
            this.lvwUpdates.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvwUpdates_ItemSelectionChanged);
            // 
            // panLower
            // 
            this.panLower.Controls.Add(this.btnDownload);
            this.panLower.Controls.Add(this.cmdClose);
            this.panLower.Location = new System.Drawing.Point(2, 233);
            this.panLower.Name = "panLower";
            this.panLower.Size = new System.Drawing.Size(403, 37);
            this.panLower.TabIndex = 3;
            this.panLower.Resize += new System.EventHandler(this.panLower_Resize_1);
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(3, 3);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(92, 25);
            this.btnDownload.TabIndex = 3;
            this.btnDownload.Text = "&Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // cmdClose
            // 
            this.cmdClose.Location = new System.Drawing.Point(310, 3);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(90, 26);
            this.cmdClose.TabIndex = 2;
            this.cmdClose.Text = "&Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // StripContext
            // 
            this.StripContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openContainingFolderToolStripMenuItem,
            this.cancelToolStripMenuItem});
            this.StripContext.Name = "contextMenuStrip1";
            this.StripContext.Size = new System.Drawing.Size(202, 48);
            this.StripContext.Opening += new System.ComponentModel.CancelEventHandler(this.StripContext_Opening);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open Containing Folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.cancelToolStripMenuItem.Text = "Cancel";
            this.cancelToolStripMenuItem.Click += new System.EventHandler(this.cancelToolStripMenuItem_Click);
            // 
            // grpBasicUpdate
            // 
            this.grpBasicUpdate.Controls.Add(this.lblRemaining);
            this.grpBasicUpdate.Controls.Add(this.label1);
            this.grpBasicUpdate.Controls.Add(this.cmdCancel);
            this.grpBasicUpdate.Controls.Add(this.lblDlRate);
            this.grpBasicUpdate.Controls.Add(this.lblSpeed);
            this.grpBasicUpdate.Controls.Add(this.lblImmProgress);
            this.grpBasicUpdate.Controls.Add(this.lblImmSpeed);
            this.grpBasicUpdate.Controls.Add(this.lblAction);
            this.grpBasicUpdate.Controls.Add(this.lblImmAction);
            this.grpBasicUpdate.Controls.Add(this.pbarImmediate);
            this.grpBasicUpdate.Location = new System.Drawing.Point(2, 1);
            this.grpBasicUpdate.Name = "grpBasicUpdate";
            this.grpBasicUpdate.Size = new System.Drawing.Size(290, 133);
            this.grpBasicUpdate.TabIndex = 5;
            // 
            // lblRemaining
            // 
            this.lblRemaining.AutoSize = true;
            this.lblRemaining.Location = new System.Drawing.Point(73, 86);
            this.lblRemaining.Name = "lblRemaining";
            this.lblRemaining.Size = new System.Drawing.Size(53, 13);
            this.lblRemaining.TabIndex = 16;
            this.lblRemaining.Text = "Unknown";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Remaining:";
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(211, 103);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(72, 25);
            this.cmdCancel.TabIndex = 14;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblDlRate
            // 
            this.lblDlRate.AutoSize = true;
            this.lblDlRate.Location = new System.Drawing.Point(74, 70);
            this.lblDlRate.Name = "lblDlRate";
            this.lblDlRate.Size = new System.Drawing.Size(40, 13);
            this.lblDlRate.TabIndex = 13;
            this.lblDlRate.Text = "0 KB/s";
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(6, 70);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(33, 13);
            this.lblSpeed.TabIndex = 12;
            this.lblSpeed.Text = "Rate:";
            // 
            // lblImmProgress
            // 
            this.lblImmProgress.AutoSize = true;
            this.lblImmProgress.Location = new System.Drawing.Point(74, 54);
            this.lblImmProgress.Name = "lblImmProgress";
            this.lblImmProgress.Size = new System.Drawing.Size(21, 13);
            this.lblImmProgress.TabIndex = 11;
            this.lblImmProgress.Text = "0%";
            // 
            // lblImmSpeed
            // 
            this.lblImmSpeed.AutoSize = true;
            this.lblImmSpeed.Location = new System.Drawing.Point(6, 54);
            this.lblImmSpeed.Name = "lblImmSpeed";
            this.lblImmSpeed.Size = new System.Drawing.Size(62, 13);
            this.lblImmSpeed.TabIndex = 10;
            this.lblImmSpeed.Text = "Completion:";
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.Location = new System.Drawing.Point(6, 41);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(40, 13);
            this.lblAction.TabIndex = 9;
            this.lblAction.Text = "Action:";
            // 
            // lblImmAction
            // 
            this.lblImmAction.AutoSize = true;
            this.lblImmAction.Location = new System.Drawing.Point(73, 41);
            this.lblImmAction.Name = "lblImmAction";
            this.lblImmAction.Size = new System.Drawing.Size(46, 13);
            this.lblImmAction.TabIndex = 8;
            this.lblImmAction.Text = "Initiating";
            this.lblImmAction.Click += new System.EventHandler(this.lblImmAction_Click);
            // 
            // pbarImmediate
            // 
            this.pbarImmediate.Location = new System.Drawing.Point(6, 11);
            this.pbarImmediate.Name = "pbarImmediate";
            this.pbarImmediate.Size = new System.Drawing.Size(261, 27);
            this.pbarImmediate.TabIndex = 6;
            // 
            // panRefreshing
            // 
            this.panRefreshing.Controls.Add(this.txtException);
            this.panRefreshing.Controls.Add(this.lblrefreshing);
            this.panRefreshing.Location = new System.Drawing.Point(0, 0);
            this.panRefreshing.Name = "panRefreshing";
            this.panRefreshing.Size = new System.Drawing.Size(292, 175);
            this.panRefreshing.TabIndex = 6;
            this.panRefreshing.Visible = false;
            this.panRefreshing.Resize += new System.EventHandler(this.panRefreshing_Resize);
            // 
            // txtException
            // 
            this.txtException.Location = new System.Drawing.Point(8, 32);
            this.txtException.Multiline = true;
            this.txtException.Name = "txtException";
            this.txtException.Size = new System.Drawing.Size(277, 140);
            this.txtException.TabIndex = 1;
            // 
            // lblrefreshing
            // 
            this.lblrefreshing.AutoSize = true;
            this.lblrefreshing.Location = new System.Drawing.Point(11, 13);
            this.lblrefreshing.Name = "lblrefreshing";
            this.lblrefreshing.Size = new System.Drawing.Size(35, 13);
            this.lblrefreshing.TabIndex = 0;
            this.lblrefreshing.Text = "label2";
            // 
            // frmUpdates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 272);
            this.Controls.Add(this.panRefreshing);
            this.Controls.Add(this.grpBasicUpdate);
            this.Controls.Add(this.panLower);
            this.Controls.Add(this.fraavailupdates);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmUpdates";
            this.Text = "BASeCamp Updater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmUpdates_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmUpdates_FormClosed);
            this.Load += new System.EventHandler(this.frmUpdates_Load);
            this.Resize += new System.EventHandler(this.frmUpdates_Resize);
            this.fraavailupdates.ResumeLayout(false);
            this.panLower.ResumeLayout(false);
            this.StripContext.ResumeLayout(false);
            this.grpBasicUpdate.ResumeLayout(false);
            this.grpBasicUpdate.PerformLayout();
            this.panRefreshing.ResumeLayout(false);
            this.panRefreshing.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox fraavailupdates;
        private System.Windows.Forms.Panel panLower;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.ContextMenuStrip StripContext;
        private System.Windows.Forms.ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private System.Windows.Forms.Panel grpBasicUpdate;
        private System.Windows.Forms.Label lblImmAction;
        private System.Windows.Forms.ProgressBar pbarImmediate;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.Label lblImmProgress;
        private System.Windows.Forms.Label lblImmSpeed;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.Label lblDlRate;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblRemaining;
        private System.Windows.Forms.Label label1;
        private ToolStripMenuItem cancelToolStripMenuItem;
        private ListView lvwUpdates;
        private Panel panRefreshing;
        private Label lblrefreshing;
        private TextBox txtException;
    }
}