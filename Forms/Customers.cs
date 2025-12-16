using barcodescanner.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace barcodescanner.Forms
{
    public partial class Customers : Form
    {
        private string _selectedId = "";
        public Customers()
        {
            InitializeComponent();
        }

        private void Customers_Load(object sender, EventArgs e)
        {
            loadData();
        }
        public void loadData()
        {
            try
            {
                dgvCustomer.Columns.Clear();
                var db = AppDb.Instance;
                dgvCustomer.DataSource = db;
                dgvCustomer.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                db.Table(
        "SELECT CustomerID, FirstName, MiddleName, LastName, Email, ContactNumber, Barangay, City, Province " +
        "FROM Customer ORDER BY CustomerID ASC",
        dgvCustomer,
        header: new[] { "CustomerID", "First Name", "Middle Name", "Last Name", "Email", "Contact Number", "Barangay", "City", "Province" }
    );

                dgvCustomer.Columns["CustomerID"].Visible = false;
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCustomer.Columns.Add(editBtn);
                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCustomer.Columns.Add(deleteBtn);
                dgvCustomer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvCustomer.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvCustomer.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult confirmLogout = MessageBox.Show(
           "Are you sure you want to go back to Home Page?",
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
            OrderModuleForm dash = new OrderModuleForm();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();
            await Task.Delay(200);
            this.Close();
        }

        private async void btnOrder_Click(object sender, EventArgs e)
        {
            Order order = new Order();
            order.StartPosition = FormStartPosition.Manual;
            order.Location = this.Location;
            order.Show();
            await Task.Delay(200);
            this.Close();
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            Form overlay = new Form();
            overlay.BackColor = Color.Black;
            overlay.Opacity = 0.5;
            overlay.FormBorderStyle = FormBorderStyle.None;
            overlay.WindowState = FormWindowState.Maximized;
            overlay.ShowInTaskbar = false;
            overlay.StartPosition = FormStartPosition.Manual;
            overlay.Show();

            AddCustomer addcustomers = new AddCustomer();
            addcustomers.StartPosition = FormStartPosition.Manual;

            int targetX = (Screen.PrimaryScreen.WorkingArea.Width - addcustomers.Width) / 2;
            int targetY = Screen.PrimaryScreen.WorkingArea.Height / 12;
            int startY = targetY - 100;

            addcustomers.Location = new Point(targetX, startY);
            addcustomers.Opacity = 0;
            addcustomers.TopMost = true;

            System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;

            animationTimer.Tick += (s, ev) =>
            {
                if (addcustomers.Top < targetY)
                    addcustomers.Top += 5;

                if (addcustomers.Opacity < 1)
                    addcustomers.Opacity += 0.05;

                if (addcustomers.Top >= targetY && addcustomers.Opacity >= 1)
                    animationTimer.Stop();
            };

            addcustomers.FormClosed += (s, ev) =>
            {
                animationTimer.Stop();
                overlay.Close();
            };

            addcustomers.DataSaved += (s, ev) =>
            {
                loadData();
            };
            addcustomers.Show();
            animationTimer.Start();
        }

        private void dgvCustomer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _selectedId = dgvCustomer.Rows[e.RowIndex].Cells["CustomerID"].Value.ToString();
                if (dgvCustomer.Columns[e.ColumnIndex].Name == "Edit")
                {
                    Form overlay = new Form();
                    overlay.BackColor = Color.Black;
                    overlay.Opacity = 0.5;
                    overlay.FormBorderStyle = FormBorderStyle.None;
                    overlay.WindowState = FormWindowState.Maximized;
                    overlay.ShowInTaskbar = false;
                    overlay.StartPosition = FormStartPosition.Manual;
                    overlay.Show();
                    editCustomer editcustomer = new editCustomer(_selectedId.ToString());
                    editcustomer.StartPosition = FormStartPosition.Manual;
                    int targetX = (Screen.PrimaryScreen.WorkingArea.Width - editcustomer.Width) / 2;
                    int targetY = Screen.PrimaryScreen.WorkingArea.Height / 12;
                    int startY = targetY - 100;
                    editcustomer.Location = new Point(targetX, startY);
                    editcustomer.Opacity = 0;
                    editcustomer.TopMost = true;
                    System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
                    animationTimer.Interval = 10;
                    animationTimer.Tick += (s, ev) =>
                    {
                        if (editcustomer.Top < targetY)
                            editcustomer.Top += 5;
                        if (editcustomer.Opacity < 1)
                            editcustomer.Opacity += 0.05;
                        if (editcustomer.Top >= targetY && editcustomer.Opacity >= 1)
                            animationTimer.Stop();
                    };
                    editcustomer.FormClosed += (s, ev) =>
                    {
                        animationTimer.Stop();
                        overlay.Close();
                    };

                    editcustomer.DataSaved += (s, ev) =>
                    {
                        loadData();
                    };

                    editcustomer.Show();
                    animationTimer.Start();
                }
                else if (dgvCustomer.Columns[e.ColumnIndex].Name == "Delete")
                {
                    if (string.IsNullOrEmpty(_selectedId))
                    {
                        MessageBox.Show("No record selected.");
                        return;
                    }
                    var confirmResult = MessageBox.Show(
                        "Are you sure you want to delete this record?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (confirmResult == DialogResult.Yes)
                    {
                        var db = AppDb.Instance;
                        bool ok = db.Delete("Customer", new { CustomerID = _selectedId }, keyColumn: "CustomerID");

                        MessageBox.Show(ok ? "Successfully Deleted." : "Delete failed.");
                        loadData();
                    }
                }
            }
        }

        private async void btnDashboard_Click_1(object sender, EventArgs e)
        {
            OrderModuleForm dash = new OrderModuleForm();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();
            await Task.Delay(200);
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

        private void bunifuButton2_Click(object sender, EventArgs e)
        {

        }

        private async void btnOrder_Click_1(object sender, EventArgs e)
        {
            Order dash = new Order();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();
            await Task.Delay(200);
            this.Close();
        }

        private void bunifuPanel1_Click(object sender, EventArgs e)
        {

        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            History history = new History();
            history.StartPosition = FormStartPosition.Manual;
            history.Location = this.Location;
            history.Show();
            this.Close();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var db = AppDb.Instance;
                var results = db.Search(
                    "Customer",
                    new[] { "FirstName", "LastName", "Email", "ContactNumber" },
                    txtSearch.Text
                );
                dgvCustomer.Columns.Clear();
                db.Table(
                    results,
                    dgvCustomer,
                    header: new[]
                    {
                "CustomerID", "First Name", "Middle Name", "Last Name",
                "Email", "Contact Number", "Barangay", "City", "Province"
                    }
                );
                if (dgvCustomer.Columns.Count == 0)
                    return;
                if (dgvCustomer.Columns.Contains("CustomerID"))
                    dgvCustomer.Columns["CustomerID"].Visible = false;
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCustomer.Columns.Add(editBtn);

                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCustomer.Columns.Add(deleteBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message);
            }
        }
    }
}
