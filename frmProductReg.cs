using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BASeCamp.Licensing

{
    public partial class frmProductReg : Form
    {
        /// <summary>
        /// delegate function used for additional validity checks. Typically used to make sure the resulting ProductKey is for the correct product.
        /// This routine will only be called if the ProductKey is valid overall.
        /// </summary>
        /// <param name="pk">Constructed ProductKey object.</param>
        /// <returns>Empty string to indicate ProductKey is valid. Otherwise, error text to display.</returns>
        public delegate String AdditionalValidCheckFunction(ProductKey pk);
        private AdditionalValidCheckFunction additionalchecker = null;
        private ProductKey.Products ProductType;
        private String _ProductName = "";
        public ProductKey constructedPK = null;
        private String IDString = "";
        private String EnsureProductRoutine(ProductKey pk)
        {

            return pk.Product == ProductType ? "" : "Product Key not for this Product";


        }
        public frmProductReg(String pProductName,ProductKey.Products forproduct,String pIDString)
        {
            _ProductName = pProductName;
            ProductType = forproduct;
            additionalchecker = EnsureProductRoutine;
            IDString = pIDString;


        }
        public frmProductReg(String pProductName,AdditionalValidCheckFunction ValidCheck,String pIDString)
            : this()
        {
            _ProductName = pProductName;
            additionalchecker = ValidCheck;
            IDString = pIDString;
        }
        public frmProductReg()
        {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            
            if (!ProductKey.IsValidKey(txtProductKey.Text,txtID.Text))
            {

                MessageBox.Show("Invalid Product Key.", "Invalid");
                return;

            }
            else
            {
                ProductKey pk = new ProductKey(txtProductKey.Text,txtID.Text);
                String failmessage = "(Unspecified reason)";
                if (additionalchecker != null)
                {
                    if (!String.IsNullOrEmpty(failmessage = additionalchecker(pk)))
                    {
                        MessageBox.Show("Invalid Product Key - " + failmessage, "Invalid");
                        return;
                    }


                }
                //save the productkey.
                pk.Register(_ProductName);
                constructedPK = pk;
                Close();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmProductReg_Load(object sender, EventArgs e)
        {
            lblProductName.Text = _ProductName;
            txtID.Text = IDString;
        }
    }
}
