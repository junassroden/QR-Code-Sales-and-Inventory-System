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
    public partial class Inventory : Form
    {
        private string _selectedId = "";
        public Inventory()
        {
            InitializeComponent();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult confirmLogout = MessageBox.Show(
     "Are you sure you want to log out?",
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
        private async void btnDashboard_Click_1(object sender, EventArgs e)
        {
            Dashboard dash = new Dashboard();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();

            await Task.Delay(200);
            this.Close();
        }
        private void bunifuPanel1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DialogResult confirmLogout = MessageBox.Show(
            "Are you sure you want to go home??",
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

        private void btnInventory_Click(object sender, EventArgs e)
        {

        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED       
                return handleParam;
            }
        }

        private void Inventory_Load(object sender, EventArgs e)
        {
            loadData();
            panelInv.Visible = true;
            btnSeller.Enabled = true;
            btnSeller.Location = new Point(21, 446);
        }

        public void loadData()
        {
            try
            {
                dgvInventory.Columns.Clear();
                var db = AppDb.Instance;
                dgvInventory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                string query = @"
                SELECT 
                p.ProductID,
                c.CategoryName,
                s.FirstName AS SupplierName,
                p.ProductName,
                p.UnitPrice,
                p.Quantity
                FROM Product p
                LEFT JOIN Category c ON p.Category_ID = c.Category_ID
                LEFT JOIN Supplier s ON p.SupplierID = s.SupplierID
                ORDER BY p.ProductID ASC
               ";

                db.Table(
                    query,
                    dgvInventory,
                    header: new[] { "ProductID", "Category", "Supplier", "Product Name", "Unit Price", "Quantity" }
                );

                dgvInventory.Columns["ProductID"].Visible = false;
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvInventory.Columns.Add(editBtn);
                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvInventory.Columns.Add(deleteBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void dgvInventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _selectedId = dgvInventory.Rows[e.RowIndex].Cells["ProductID"].Value.ToString();
                if (dgvInventory.Columns[e.ColumnIndex].Name == "Edit")
                {
                    Form overlay = new Form();
                    overlay.BackColor = Color.Black;
                    overlay.Opacity = 0.5;
                    overlay.FormBorderStyle = FormBorderStyle.None;
                    overlay.WindowState = FormWindowState.Maximized;
                    overlay.ShowInTaskbar = false;
                    overlay.StartPosition = FormStartPosition.Manual;
                    overlay.Show();
                    editInventory editinventory = new editInventory(_selectedId.ToString());
                    editinventory.StartPosition = FormStartPosition.Manual;
                    int targetX = (Screen.PrimaryScreen.WorkingArea.Width - editinventory.Width) / 2;
                    int targetY = Screen.PrimaryScreen.WorkingArea.Height / 12;
                    int startY = targetY - 100;
                    editinventory.Location = new Point(targetX, startY);
                    editinventory.Opacity = 0;
                    editinventory.TopMost = true;
                    System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
                    animationTimer.Interval = 10;
                    animationTimer.Tick += (s, ev) =>
                    {
                        if (editinventory.Top < targetY)
                            editinventory.Top += 5;
                        if (editinventory.Opacity < 1)
                            editinventory.Opacity += 0.05;
                        if (editinventory.Top >= targetY && editinventory.Opacity >= 1)
                            animationTimer.Stop();
                    };
                    editinventory.FormClosed += (s, ev) =>
                    {
                        animationTimer.Stop();
                        overlay.Close();
                    };

                    editinventory.DataSaved += (s, ev) =>
                    {
                        loadData();
                    };

                    editinventory.Show();
                    animationTimer.Start();
                }
                else if (dgvInventory.Columns[e.ColumnIndex].Name == "Delete")
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
                        bool ok = db.Delete("Product", new { ProductID = _selectedId }, keyColumn: "ProductID");

                        MessageBox.Show(ok ? "Successfully Deleted." : "Delete failed.");
                        loadData();
                    }
                }
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

            AddInventory addItem = new AddInventory();
            addItem.StartPosition = FormStartPosition.Manual;

            int targetX = (Screen.PrimaryScreen.WorkingArea.Width - addItem.Width) / 2;
            int targetY = Screen.PrimaryScreen.WorkingArea.Height / 12;
            int startY = targetY - 100;

            addItem.Location = new Point(targetX, startY);
            addItem.Opacity = 0;
            addItem.TopMost = true;

            System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;

            animationTimer.Tick += (s, ev) =>
            {
                if (addItem.Top < targetY)
                    addItem.Top += 5;

                if (addItem.Opacity < 1)
                    addItem.Opacity += 0.05;

                if (addItem.Top >= targetY && addItem.Opacity >= 1)
                    animationTimer.Stop();
            };

            addItem.FormClosed += (s, ev) =>
            {
                animationTimer.Stop();
                overlay.Close();
            };

            addItem.DataSaved += (s, ev) =>
            {
                loadData();
            };
            addItem.Show();
            animationTimer.Start();
        }

        private async void btnSeller_Click(object sender, EventArgs e)
        {
            
        }

        private async void bunifuButton7_Click(object sender, EventArgs e)
        {
            Dashboard dash = new Dashboard();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();

            await Task.Delay(200);
            this.Close();
        }

        private void bunifuButton5_Click(object sender, EventArgs e)
        {

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

        private void bunifuButton3_Click(object sender, EventArgs e)
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

        private async void bunifuButton2_Click(object sender, EventArgs e)
        {
            Supplier supplier = new Supplier();
            supplier.StartPosition = FormStartPosition.Manual;
            supplier.Location = this.Location;
            supplier.Show();
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
            try
            {
                var db = AppDb.Instance;
                string filter = txtSearch.Text.Replace("'", "''");

                string query = $@"
            SELECT 
                p.ProductID,
                c.CategoryName,
                s.FirstName AS SupplierName,
                p.ProductName,
                p.UnitPrice,
                p.Quantity
            FROM Product p
            LEFT JOIN Category c ON p.Category_ID = c.Category_ID
            LEFT JOIN Supplier s ON p.SupplierID = s.SupplierID
            WHERE
                p.ProductName LIKE '%{filter}%'
                OR c.CategoryName LIKE '%{filter}%'
                OR s.FirstName LIKE '%{filter}%'
            ORDER BY p.ProductID ASC;
        ";

                DataTable dt = db.TableData(query);

                // Avoid flicker & prevent double columns
                dgvInventory.Columns.Clear();
                dgvInventory.DataSource = dt;

                // Hide PK
                if (dgvInventory.Columns.Contains("ProductID"))
                    dgvInventory.Columns["ProductID"].Visible = false;

                // Re-add EDIT button
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvInventory.Columns.Add(editBtn);

                // Re-add DELETE button
                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvInventory.Columns.Add(deleteBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message);
            }
        }
    }
}
