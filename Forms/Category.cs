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
    public partial class Category : Form
    {
        private string _selectedId = "";
        public Category()
        {
            InitializeComponent();
        }
        protected override CreateParams CreateParams
        {
            //used to reduce flickering
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED       
                return handleParam;
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

        private async void btnInventory_Click(object sender, EventArgs e)
        {
            Inventory inventory = new Inventory();
            inventory.StartPosition = FormStartPosition.Manual;
            inventory.Location = this.Location;
            inventory.Show();

            await Task.Delay(100);
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

        public void loadData()
        {
            try
            {
                dgvCategory.Columns.Clear();
                var db = AppDb.Instance;
                db.Table(
                    "SELECT Category_ID, CategoryName FROM Category ORDER BY Category_ID ASC",
                    dgvCategory,
                    header: new[] { "Category_ID", "Category Name" }
                );
                dgvCategory.Columns["Category_ID"].Visible = false;
                dgvCategory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCategory.Columns.Add(editBtn);

                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCategory.Columns.Add(deleteBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void Category_Load(object sender, EventArgs e)
        {
            loadData();
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

            AddCategory editCategory = new AddCategory();
            editCategory.StartPosition = FormStartPosition.Manual;

            int targetX = (Screen.PrimaryScreen.WorkingArea.Width - editCategory.Width) / 2;
            int targetY = Screen.PrimaryScreen.WorkingArea.Height / 6;
            int startY = targetY - 100;

            editCategory.Location = new Point(targetX, startY);
            editCategory.Opacity = 0;
            editCategory.TopMost = true;

            System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;

            animationTimer.Tick += (s, ev) =>
            {
                if (editCategory.Top < targetY)
                    editCategory.Top += 5;

                if (editCategory.Opacity < 1)
                    editCategory.Opacity += 0.05;

                if (editCategory.Top >= targetY && editCategory.Opacity >= 1)
                    animationTimer.Stop();
            };

            editCategory.FormClosed += (s, ev) =>
            {
                animationTimer.Stop();
                overlay.Close();
            };

            editCategory.DataSaved += (s, ev) =>
            {
                loadData();
            };
            editCategory.Show();
            animationTimer.Start();
        }

        private void dgvCategory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _selectedId = dgvCategory.Rows[e.RowIndex].Cells["Category_ID"].Value.ToString();
                if (dgvCategory.Columns[e.ColumnIndex].Name == "Edit")
                {
                    Form overlay = new Form();
                    overlay.BackColor = Color.Black;
                    overlay.Opacity = 0.5;
                    overlay.FormBorderStyle = FormBorderStyle.None;
                    overlay.WindowState = FormWindowState.Maximized;
                    overlay.ShowInTaskbar = false;
                    overlay.StartPosition = FormStartPosition.Manual;
                    overlay.Show();
                    EditCategory editSupplier = new EditCategory(_selectedId.ToString());
                    editSupplier.StartPosition = FormStartPosition.Manual;
                    int targetX = (Screen.PrimaryScreen.WorkingArea.Width - editSupplier.Width) / 2;
                    int targetY = Screen.PrimaryScreen.WorkingArea.Height / 6;
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
                else if (dgvCategory.Columns[e.ColumnIndex].Name == "Delete")
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
                        bool ok = db.Delete("Category", new { Category_ID = _selectedId }, keyColumn: "Category_ID");

                        MessageBox.Show(ok ? "Successfully Deleted." : "Delete failed.");
                        loadData();
                    }
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var db = AppDb.Instance;
                var results = db.Search(
                    "Category",
                    new[] { "CategoryName" },
                    txtSearch.Text
                );
                dgvCategory.Columns.Clear();
                db.Table(
                    results,
                    dgvCategory,
                    header: new[] { "Category_ID", "Category Name" }
                );
                if (dgvCategory.Columns.Count == 0)
                    return;
                if (dgvCategory.Columns.Contains("Category_ID"))
                    dgvCategory.Columns["Category_ID"].Visible = false;
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCategory.Columns.Add(editBtn);
                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvCategory.Columns.Add(deleteBtn);

                dgvCategory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message);
            }
        }
    }
}
