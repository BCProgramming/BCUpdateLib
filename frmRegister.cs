using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using BASeCamp.Licensing;

namespace BASeCamp.Licensing
{
    public partial class frmRegister : Form
    {
        public new String ProductName = "";
        public String LicensedName = "";
        public String LicensedOrganization = "";
        public String LicenseKey = "";
        
        public static void DoRegister(String ProductName,IWin32Window owner)
        {
            registerAgain:
            frmRegister useform = new frmRegister(ProductName);
            DialogResult dr = owner == null ? useform.ShowDialog() : useform.ShowDialog(owner);
            if (dr == DialogResult.OK)
            {
                
                SecureString generatedkey = LicensedFeatureData.GenKey(useform.LicensedName, useform.LicensedOrganization, useform.ProductName);
                SecureString copiedKey = new SecureString();
                foreach (char iterate in useform.LicenseKey)
                {
                    copiedKey.AppendChar(iterate);


                }

                //if (generatedkey == useform.LicenseKey)
                if(LicensedFeatureData.SecureStringEqual(generatedkey,copiedKey))
                {
                    //Registration Successful.
                    LicensedFeatureData.StoreLicenseData(ProductName, useform.LicensedName, useform.LicensedOrganization, useform.LicenseKey);
                    MessageBox.Show("Thank you for registering " + ProductName);
                    
                }
                else
                {
                    if (MessageBox.Show("Invalid Key.", "Key Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Retry)
                    {
                        goto registerAgain;

                    }
                }



            }

        }

        protected frmRegister(String pProductName)
            : this()
        {
            ProductName = pProductName;

        }

        private frmRegister()
        {
            InitializeComponent();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdRegister_Click(object sender, EventArgs e)
        {
            LicensedName = txtName.Text;
            LicensedOrganization = txtOrganization.Text;
            LicenseKey = txtKey.Text;
            DialogResult = DialogResult.OK;
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            Text = "Register " + ProductName;
        }
        private static readonly string linkvisit = "http://bc-programming.com/";

        private void linkwebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkwebsite.LinkVisited = true;
            System.Diagnostics.Process.Start(linkvisit);

        }
    }
}
