using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace BASeCamp.Updating
{
    public partial class MarqueeProgress : Form
    {
        private String _useTitle = "Marquee";
        private String _Description = "Please Wait...";
        private EventHandler<EventArgs> _Cancelled;



        public MarqueeProgress(String pTitle,String pDescription,EventHandler<EventArgs> Cancelled)
        {
            _useTitle = pTitle;
            _Description=pDescription;
            _Cancelled = Cancelled;
            InitializeComponent();

            
        }

        private void MarqueeProgress_Load(object sender, EventArgs e)
        {
            Text = _useTitle;
            lblDescription.Text = _Description;
            pbarmarquee.Style=ProgressBarStyle.Marquee;
            pbarmarquee.MarqueeAnimationSpeed = 100;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            if (_Cancelled != null)
            {
                _Cancelled(this, new EventArgs());
            }
            Close();
        }
    }
}
