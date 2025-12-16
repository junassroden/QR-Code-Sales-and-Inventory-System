using barcodescanner.Classes;
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

namespace barcodescanner.Forms
{
    public partial class Dashboard : Form
    {

        public Dashboard()
        {
            InitializeComponent();
        }
        public Dashboard(string username)
        {
            InitializeComponent();
        }
       

        private void btnDashboard_Click(object sender, EventArgs e)
        {

        }
        private async void btnInventory_Click_1(object sender, EventArgs e)
        {
            Inventory inventory = new Inventory();
            inventory.StartPosition = FormStartPosition.Manual;
            inventory.Location = this.Location;
            inventory.Show();

            await Task.Delay(100);
            this.Close();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DialogResult confirmLogout = MessageBox.Show(
            "Are you sure you want to go home?",
             "Confirm Logout",
                 MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
            );

            if (confirmLogout == DialogResult.Yes)
            {
                Home home = new Home();
                home.Show();
                this.Close();
            }
        }
        private void btnSeller_Click(object sender, EventArgs e)
        {
            technician seller = new technician();
            seller.StartPosition = FormStartPosition.Manual;
            seller.Location = this.Location;

            seller.Show();
            this.Close();

        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            LoadDashboardCounts();

        }

        private void LoadDashboardCounts()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmdTech = new SqlCommand("SELECT COUNT(*) FROM Technician", conn);//select the count of rows and then 
                int techCount = (int)cmdTech.ExecuteScalar();// convert into int
                lblTechnicians.Text = techCount.ToString();//convert into string to display
                
                SqlCommand cmdSup = new SqlCommand("SELECT COUNT(*) FROM Supplier", conn);
                int supCount = (int)cmdSup.ExecuteScalar();
                lblSuppliers.Text = supCount.ToString();

                SqlCommand cmdInv = new SqlCommand("SELECT COUNT(*) FROM QRCodes", conn);
                int invCount = (int)cmdInv.ExecuteScalar();
                lblItems.Text = invCount.ToString();

                SqlCommand cmdCat = new SqlCommand("SELECT COUNT(*) FROM Category", conn);
                int catCount = (int)cmdCat.ExecuteScalar();
                lblCategories.Text = catCount.ToString();
            }
        }

    }
}
