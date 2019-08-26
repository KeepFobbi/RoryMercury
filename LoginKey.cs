using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rory_Mercury
{
    public partial class LoginKey : Form
    {
        public LoginKey()
        {
            InitializeComponent();
        }

        public LoginKey(string tempStr)
        {
            InitializeComponent();
            labelKey.Text = tempStr;
        }

        private void LabelKey_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(labelKey.Text);
            Close();
        }
    }
}
