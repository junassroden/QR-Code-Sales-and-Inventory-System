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
    public partial class technician : Form
    {
        private string _selectedId = "";
        public technician()
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
        private  async void bunifuButton1_Click(object sender, EventArgs e)
        {
            Dashboard dash = new Dashboard();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();

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
        public void loadData()
        {
            try
            {
                dgvOrder.Columns.Clear();
                var db = AppDb.Instance;
                db.Table(
                    "SELECT TechnicianID, FirstName, MiddleName, LastName, Specialization, Email, PhoneNumber FROM Technician ORDER BY TechnicianID ASC",
                    dgvOrder,
                    header: new[] { "TechnicianID", "First Name", "Middle Name", "Last Name", "Specialization", "Email", "Phone Number" }
                );
                // Optionally hide the ID column
                dgvOrder.Columns["TechnicianID"].Visible = false;

                // Center align headers
                dgvOrder.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // Edit button
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvOrder.Columns.Add(editBtn);
                // Delete button
                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvOrder.Columns.Add(deleteBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }
        private void btnSeller_Click(object sender, EventArgs e)
        {

        }

        private void Seller_Load(object sender, EventArgs e)
        {
            loadData();
        }

        private void dgvOrder_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _selectedId = dgvOrder.Rows[e.RowIndex].Cells["TechnicianID"].Value.ToString();
                if (dgvOrder.Columns[e.ColumnIndex].Name == "Edit")
                {
                    Form overlay = new Form();
                    overlay.BackColor = Color.Black;
                    overlay.Opacity = 0.5;
                    overlay.FormBorderStyle = FormBorderStyle.None;
                    overlay.WindowState = FormWindowState.Maximized;
                    overlay.ShowInTaskbar = false;
                    overlay.StartPosition = FormStartPosition.Manual;
                    overlay.Show();
                    EditTechnician editTech = new EditTechnician(_selectedId.ToString());
                    editTech.StartPosition = FormStartPosition.Manual;
                    int targetX = (Screen.PrimaryScreen.WorkingArea.Width - editTech.Width) / 2;
                    int targetY = Screen.PrimaryScreen.WorkingArea.Height / 6;
                    int startY = targetY - 100;
                    editTech.Location = new Point(targetX, startY);
                    editTech.Opacity = 0;
                    editTech.TopMost = true;
                    System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
                    animationTimer.Interval = 10;
                    animationTimer.Tick += (s, ev) =>
                    {
                        if (editTech.Top < targetY)
                            editTech.Top += 5;
                        if (editTech.Opacity < 1)
                            editTech.Opacity += 0.05;
                        if (editTech.Top >= targetY && editTech.Opacity >= 1)
                            animationTimer.Stop();
                    };
                    editTech.FormClosed += (s, ev) =>
                    {
                        animationTimer.Stop();
                        overlay.Close();
                    };

                    editTech.DataSaved += (s, ev) =>
                    {
                        loadData();
                    };

                    editTech.Show();
                    animationTimer.Start();
                }
                else if (dgvOrder.Columns[e.ColumnIndex].Name == "Delete")
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
                        bool ok = db.Delete("Technician", new { TechnicianID = _selectedId }, keyColumn: "TechnicianID");

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

            Admin addTech = new Admin();
            addTech.StartPosition = FormStartPosition.Manual;

            int targetX = (Screen.PrimaryScreen.WorkingArea.Width - addTech.Width) / 2;
            int targetY = Screen.PrimaryScreen.WorkingArea.Height / 12;
            int startY = targetY - 100;

            addTech.Location = new Point(targetX, startY);
            addTech.Opacity = 0;
            addTech.TopMost = true;

            System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;

            animationTimer.Tick += (s, ev) =>
            {
                if (addTech.Top < targetY)
                    addTech.Top += 5;

                if (addTech.Opacity < 1)
                    addTech.Opacity += 0.05;

                if (addTech.Top >= targetY && addTech.Opacity >= 1)
                    animationTimer.Stop();
            };

            addTech.FormClosed += (s, ev) =>
            {
                animationTimer.Stop();
                overlay.Close();
            };

            addTech.DataSaved += (s, ev) =>
            {
                loadData();
            };
            addTech.Show();
            animationTimer.Start();
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            Form overlay = new Form();
            overlay.BackColor = Color.Black;
            overlay.Opacity = 0.5;
            overlay.FormBorderStyle = FormBorderStyle.None;
            overlay.WindowState = FormWindowState.Maximized;
            overlay.ShowInTaskbar = false;
            overlay.StartPosition = FormStartPosition.Manual;
            overlay.Show();

            Admin addItem = new Admin();
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

        private void button2_Click_3(object sender, EventArgs e)
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var db = AppDb.Instance;
                var results = db.Search(
                    "Technician",
                    new[] { "FirstName", "MiddleName", "LastName", "Specialization", "Email", "PhoneNumber" },
                    txtSearch.Text
                );

                dgvOrder.Columns.Clear();
                db.Table(
                    results,
                    dgvOrder,
                    header: new[] { "TechnicianID", "First Name", "Middle Name", "Last Name", "Specialization", "Email", "Phone Number" }
                );

                if (dgvOrder.Columns.Contains("TechnicianID"))
                    dgvOrder.Columns["TechnicianID"].Visible = false;

                dgvOrder.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                DataGridViewButtonColumn editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvOrder.Columns.Add(editBtn);

                DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dgvOrder.Columns.Add(deleteBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message);
            }
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
    }
}
