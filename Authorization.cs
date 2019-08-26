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
    public partial class Authorization : Form
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void SignIn_Click(object sender, EventArgs e)
        {
            //to do
            Main form = new Main();
            form.Show();
            Hide();
        }

        private void SignUp_Click(object sender, EventArgs e)
        {
            //to do
            string keyStr = GenRandomString
                ("QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm0123456789", 10);
            LoginKey key = new LoginKey(keyStr);
            key.ShowDialog();
        }

        private string GenRandomString(string Alphabet, int Length)
        {
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder(Length - 1);
            int Position = 0;

            for (int i = 0; i < Length; i++)
            {
                Position = rnd.Next(0, Alphabet.Length - 1);
                sb.Append(Alphabet[Position]);
            }

            return sb.ToString();
        }

        private void Authorization_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void Password_Enter(object sender, EventArgs e)
        {
            Password.Clear();
        }

        private void Login_Enter(object sender, EventArgs e)
        {
            Login.Clear();
        }
    }
}
