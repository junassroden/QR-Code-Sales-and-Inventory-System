using barcodescanner.Classes;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace barcodescanner.Forms
{
    public partial class History : Form
    {
        public History()
        {
            InitializeComponent();
        }

        private void History_Load(object sender, EventArgs e)
        {
            LoadHistory();
        }

        private void button2_Click(object sender, EventArgs e)
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

        private async void btnDashboard_Click(object sender, EventArgs e)
        {
            OrderModuleForm dash = new OrderModuleForm
            {
                StartPosition = FormStartPosition.Manual,
                Location = this.Location
            };
            dash.Show();
            await System.Threading.Tasks.Task.Delay(200);
            this.Close();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            Order dash = new Order
            {
                StartPosition = FormStartPosition.Manual,
                Location = this.Location
            };
            dash.Show();
            this.Close();
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            Customers customers = new Customers
            {
                StartPosition = FormStartPosition.Manual,
                Location = this.Location
            };
            customers.Show();
            this.Close();
        }

        private void dgvHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional: handle clicks in the grid if needed
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var db = AppDb.Instance;
                string filter = txtSearch.Text.Replace("'", "''");

                string query = $@"
                    SELECT 
                        p.ProductName,
                        c.FirstName + ' ' + c.LastName AS CustomerName,
                        h.DateSold,
                        h.Quantity,
                        h.TotalPrice
                    FROM SalesTransactionHistory h
                    INNER JOIN Product p ON h.ProductID = p.ProductID
                    INNER JOIN Customer c ON h.CustomerID = c.CustomerID
                    WHERE 
                        p.ProductName LIKE '%{filter}%'
                        OR (c.FirstName + ' ' + c.LastName) LIKE '%{filter}%'
                        OR h.DateSold LIKE '%{filter}%'
                    ORDER BY h.DateSold DESC, h.TransactionID DESC;
                ";

                DataTable dt = db.TableData(query);

                dgvHistory.Columns.Clear();
                dgvHistory.DataSource = dt;

                if (dgvHistory.Columns.Contains("TotalPrice"))
                    dgvHistory.Columns["TotalPrice"].DefaultCellStyle.Format = "N2";
                if (dgvHistory.Columns.Contains("DateSold"))
                    dgvHistory.Columns["DateSold"].DefaultCellStyle.Format = "yyyy-MM-dd";

                dgvHistory.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                if (dgvHistory.Columns.Contains("DateSold"))
                    dgvHistory.Sort(dgvHistory.Columns["DateSold"], ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHistory()
        {
            try
            {
                var db = AppDb.Instance;

                string query = @"
                    SELECT 
                        p.ProductName,
                        c.FirstName + ' ' + c.LastName AS CustomerName,
                        h.DateSold,
                        h.Quantity,
                        h.TotalPrice
                    FROM SalesTransactionHistory h
                    INNER JOIN Product p ON h.ProductID = p.ProductID
                    INNER JOIN Customer c ON h.CustomerID = c.CustomerID
                    ORDER BY h.DateSold DESC, h.TransactionID DESC;
                ";

                DataTable dt = db.TableData(query);

                dgvHistory.DataSource = dt;

                if (dgvHistory.Columns.Contains("TotalPrice"))
                    dgvHistory.Columns["TotalPrice"].DefaultCellStyle.Format = "N2";
                if (dgvHistory.Columns.Contains("DateSold"))
                    dgvHistory.Columns["DateSold"].DefaultCellStyle.Format = "yyyy-MM-dd";

                dgvHistory.Sort(dgvHistory.Columns["DateSold"], ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading history: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
