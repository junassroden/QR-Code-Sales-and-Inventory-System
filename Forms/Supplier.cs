using barcodescanner.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace barcodescanner.Forms
{
    public partial class Supplier : Form
    {
        private string _selectedId = "";
        


        public Supplier()
        {
            InitializeComponent();
        }

        private void Supplier_Load(object sender, EventArgs e)
        {
           loadData();  
        }

        public void loadData()
        {
            try
            {
                dgvSupplier.Columns.Clear();
                var db = AppDb.Instance;
                db.Table(
                    "SELECT SupplierID, FirstName, MiddleName, LastName, ContactNumber, Email, City, Barangay, Province, CompanyName FROM Supplier ORDER BY SupplierID ASC",
                    dgvSupplier,
                    header: new[] { "SupplierID", "First Name", "Middle Name", "Last Name", "Contact Number", "Email", "City", "Barangay", "Province", "Company Name" }
                );
                dgvSupplier.Columns["SupplierID"].Visible = false;
                dgvSupplier.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvSupplier.Columns.Add(editBtn);

                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvSupplier.Columns.Add(deleteBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }

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

            AddSupplier addSupplier = new AddSupplier();
            addSupplier.StartPosition = FormStartPosition.Manual;

            int targetX = (Screen.PrimaryScreen.WorkingArea.Width - addSupplier.Width) / 2;
            int targetY = Screen.PrimaryScreen.WorkingArea.Height / 16;
            int startY = targetY - 100;

            addSupplier.Location = new Point(targetX, startY);
            addSupplier.Opacity = 0;
            addSupplier.TopMost = true;

            System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;

            animationTimer.Tick += (s, ev) =>
            {
                if (addSupplier.Top < targetY)
                    addSupplier.Top += 5;

                if (addSupplier.Opacity < 1)
                    addSupplier.Opacity += 0.05;

                if (addSupplier.Top >= targetY && addSupplier.Opacity >= 1)
                    animationTimer.Stop();
            };

            addSupplier.FormClosed += (s, ev) =>
            {
                animationTimer.Stop();
                overlay.Close();
            };

            addSupplier.DataSaved += (s, ev) =>
            {
                loadData();
            };
            addSupplier.Show();
            animationTimer.Start();
        }
        private async void btnSeller_Click(object sender, EventArgs e)
        {
            Inventory inventory = new Inventory();
            inventory.StartPosition = FormStartPosition.Manual;
            inventory.Location = this.Location;
            inventory.Show();

            await Task.Delay(200);
            this.Close();
        }
        private void dgvInventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _selectedId = dgvSupplier.Rows[e.RowIndex].Cells["SupplierID"].Value.ToString();
                if (dgvSupplier.Columns[e.ColumnIndex].Name == "Edit")
                {
                    Form overlay = new Form();
                    overlay.BackColor = Color.Black;
                    overlay.Opacity = 0.5;
                    overlay.FormBorderStyle = FormBorderStyle.None;
                    overlay.WindowState = FormWindowState.Maximized;
                    overlay.ShowInTaskbar = false;
                    overlay.StartPosition = FormStartPosition.Manual;
                    overlay.Show();

                    EditSupplier editSupplier = new EditSupplier(_selectedId.ToString());
                    editSupplier.StartPosition = FormStartPosition.Manual;

                    int targetX = (Screen.PrimaryScreen.WorkingArea.Width - editSupplier.Width) / 2;
                    int targetY = Screen.PrimaryScreen.WorkingArea.Height / 16;
                    int startY = targetY - 100;

                    editSupplier.Location = new Point(targetX, startY);
                    editSupplier.Opacity = 0;
                    editSupplier.TopMost = true;

                    System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
                    animationTimer.Interval = 10;

                    animationTimer.Tick += (s, ev) =>
                    {
                        if (editSupplier.Top < targetY)
                            editSupplier.Top += 5;

                        if (editSupplier.Opacity < 1)
                            editSupplier.Opacity += 0.05;

                        if (editSupplier.Top >= targetY && editSupplier.Opacity >= 1)
                            animationTimer.Stop();
                    };

                    editSupplier.FormClosed += (s, ev) =>
                    {
                        animationTimer.Stop();
                        overlay.Close();
                    };

                    editSupplier.DataSaved += (s, ev) =>
                    {
                        loadData();
                    };

                    editSupplier.Show();
                    animationTimer.Start();
                }
                else if (dgvSupplier.Columns[e.ColumnIndex].Name == "Delete")
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

                        // IMPORTANT: pass correct key column name
                        bool ok = db.Delete("Supplier", new { SupplierID = _selectedId }, keyColumn: "SupplierID");

                        MessageBox.Show(ok ? "Successfully Deleted." : "Delete failed.");
                        loadData();
                    }
                }
            }
        }
        private async void btnInventory_Click(object sender, EventArgs e)
        {
            Inventory inventory = new Inventory();
            inventory.StartPosition = FormStartPosition.Manual;
            inventory.Location = this.Location;
            inventory.Show();

            await Task.Delay(200);
            this.Close();
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
            Dashboard dash = new Dashboard();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();

            await Task.Delay(200);
            this.Close();
        }

        private async void bunifuButton4_Click(object sender, EventArgs e)
        {
            technician seller = new technician();
            seller.StartPosition = FormStartPosition.Manual;
            seller.Location = this.Location;
            seller.Show();

            await Task.Delay(200);
            this.Close();
        }

        private async void bunifuButton1_Click(object sender, EventArgs e)
        {
            Category category = new Category();
            category.StartPosition = FormStartPosition.Manual;
            category.Location = this.Location;
            category.Show();

            await Task.Delay(200);
            this.Close();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
