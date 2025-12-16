using barcodescanner.Classes;
using barcodescanner.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace barcodescanner
{
    public partial class Login : Form
    {
        bool passwordHidden = false;
        public Login()
        {
            InitializeComponent();
        }

        private void btnX_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            var db = AppDb.Instance;//all variables

            txtPassword.UseSystemPasswordChar = true; // hide password
            passwordHidden = true; // sync with logic
            pictureBoxHide.Image = Image.FromFile("Hide.png"); // "hidden" icon
        }
        private void loginbtn_Click(object sender, EventArgs e)
        {
            var db = AppDb.Instance;
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            var parameters = new Dictionary<string, object>
            {
                { "Name", username },
                { "Password", password }
            };

            DataTable dt = db.TableData(
                "SELECT ID FROM tblUsers WHERE Name=@Name AND Password=@Password",
                parameters
            );

            if (dt.Rows.Count > 0)
            {
                Home home = new Home();
                home.StartPosition = FormStartPosition.CenterScreen;
                home.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string basePath = Application.StartupPath; 

            if (passwordHidden)
            {
                txtPassword.UseSystemPasswordChar = false;
                pictureBoxHide.Image = Image.FromFile(Path.Combine(basePath, "unhideee.png"));
                passwordHidden = false;
            }
            else
            {
                txtPassword.UseSystemPasswordChar = true;
                pictureBoxHide.Image = Image.FromFile(Path.Combine(basePath, "Hide.png"));
                passwordHidden = true;
            }
        }
        private void txtPassword_Enter(object sender, EventArgs e)
        {

        }

        private void txtPassword_Leave(object sender, EventArgs e)
        {

        }
    }
}
