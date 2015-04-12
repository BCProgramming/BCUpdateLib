namespace BASeCamp.Licensing
{
    partial class frmRegister
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRegister));
            this.cmdRegister = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblOrganization = new System.Windows.Forms.Label();
            this.txtOrganization = new System.Windows.Forms.TextBox();
            this.txtKey = new System.Windows.Forms.MaskedTextBox();
            this.linkwebsite = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // cmdRegister
            // 
            this.cmdRegister.Location = new System.Drawing.Point(201, 130);
            this.cmdRegister.Name = "cmdRegister";
            this.cmdRegister.Size = new System.Drawing.Size(75, 23);
            this.cmdRegister.TabIndex = 0;
            this.cmdRegister.Text = "&Register";
            this.cmdRegister.UseVisualStyleBackColor = true;
            this.cmdRegister.Click += new System.EventHandler(this.cmdRegister_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(120, 130);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "&Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(80, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(168, 20);
            this.txtName.TabIndex = 3;
            // 
            // lblOrganization
            // 
            this.lblOrganization.AutoSize = true;
            this.lblOrganization.Location = new System.Drawing.Point(14, 48);
            this.lblOrganization.Name = "lblOrganization";
            this.lblOrganization.Size = new System.Drawing.Size(69, 13);
            this.lblOrganization.TabIndex = 4;
            this.lblOrganization.Text = "&Organization:";
            // 
            // txtOrganization
            // 
            this.txtOrganization.Location = new System.Drawing.Point(81, 46);
            this.txtOrganization.Name = "txtOrganization";
            this.txtOrganization.Size = new System.Drawing.Size(166, 20);
            this.txtOrganization.TabIndex = 5;
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(81, 77);
            this.txtKey.Mask = "AAAA-AAAA-AAAA-AAAA";
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(165, 20);
            this.txtKey.TabIndex = 6;
            // 
            // linkwebsite
            // 
            this.linkwebsite.AutoSize = true;
            this.linkwebsite.LinkArea = new System.Windows.Forms.LinkArea(0, 15);
            this.linkwebsite.Location = new System.Drawing.Point(5, 135);
            this.linkwebsite.Name = "linkwebsite";
            this.linkwebsite.Size = new System.Drawing.Size(96, 17);
            this.linkwebsite.TabIndex = 7;
            this.linkwebsite.TabStop = true;
            this.linkwebsite.Text = "How to Purchase?";
            this.linkwebsite.UseCompatibleTextRendering = true;
            this.linkwebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkwebsite_LinkClicked);
            // 
            // frmRegister
            // 
            this.AcceptButton = this.cmdRegister;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(288, 162);
            this.Controls.Add(this.linkwebsite);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.txtOrganization);
            this.Controls.Add(this.lblOrganization);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdRegister);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRegister";
            this.Text = "Register";
            this.Load += new System.EventHandler(this.frmRegister_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdRegister;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblOrganization;
        private System.Windows.Forms.TextBox txtOrganization;
        private System.Windows.Forms.MaskedTextBox txtKey;
        private System.Windows.Forms.LinkLabel linkwebsite;
    }
}