using barcodescanner.Forms;
using Bunifu.UI.WinForms.BunifuButton;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace barcodescanner
{
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
        }

        private void Home_Load(object sender, EventArgs e)
        {
        }

        private void btnX_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            DialogResult confirmLogout = MessageBox.Show(
            "Are you sure you want to log out?",
             "Confirm Logout",
                 MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
            );

            if (confirmLogout == DialogResult.Yes)
            {
                Login login = new Login();
                login.Show();
                this.Close();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            using (BarcodeScannerApp newForm = new BarcodeScannerApp())
            {
                newForm.ShowDialog();
            }
            this.Close();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            using (Dashboard dashboard = new Dashboard())
            {
                dashboard.ShowDialog();
            }
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (OrderModuleForm order = new OrderModuleForm())
            {
                order.ShowDialog();
            }
            this.Close();
        }
    }
}
