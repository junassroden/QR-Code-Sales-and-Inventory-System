    using barcodescanner.Classes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Data.SqlClient;


namespace barcodescanner.Forms
    {
        public partial class AddOrder : Form
        {
        public event EventHandler DataSaved;

        private Dictionary<string, Image> imageCache = new Dictionary<string, Image>();

        public AddOrder()
            {
              InitializeComponent();

             // Initialize smooth FlowLayoutPanel
            panel = new SmoothFlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Dock = DockStyle.Fill
            };
            this.Controls.Add(panel);

            // Double buffering for form
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            }

        // Custom panel for smooth scrolling
        public class SmoothFlowLayoutPanel : FlowLayoutPanel
        {
            public SmoothFlowLayoutPanel()
            {
                this.DoubleBuffered = true;
                this.ResizeRedraw = true;
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                              ControlStyles.AllPaintingInWmPaint |
                              ControlStyles.UserPaint, true);
            }
        }

        private void EnableSmoothScroll(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(control, true, null);
        }

        private void AddOrder_Load(object sender, EventArgs e)
            {
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = true;
            panel.AutoScroll = true;
            panel.HorizontalScroll.Enabled = false;
            panel.HorizontalScroll.Visible = false;
            EnableSmoothScroll(panel);
            LoadCustomers();
            LoadCategories();
            LoadProducts();
            //panel.MouseWheel += Panel_MouseWheel;

            cboCategory.SelectedIndexChanged += cboCategory_SelectedIndexChanged;

            SetupGridView();
        }

        //private void Panel_MouseWheel(object sender, MouseEventArgs e)
        //{
        //   //basta sa scroll ari hhahaha
        //    int scrollSpeed = 1; 
        //    int newValue = panel.VerticalScroll.Value - (e.Delta / 70) * scrollSpeed;

        //    newValue = Math.Max(panel.VerticalScroll.Minimum,
        //                        Math.Min(panel.VerticalScroll.Maximum, newValue));
        //    panel.VerticalScroll.Value = newValue;
        //    panel.PerformLayout();
        //}

        private void SetupGridView()
        {
            if (Items.Columns.Count == 0)
            {
                Items.AllowUserToAddRows = false;
                Items.Columns.Add("ProductName", "Product Name");
                Items.Columns.Add("Price", "Price"); // numeric value 0.00
                Items.Columns.Add("Quantity", "Quantity");

                if (Items.Columns["Delete"] == null)
                {
                    DataGridViewButtonColumn deleteBtn = new DataGridViewButtonColumn
                    {
                        Name = "Delete",
                        HeaderText = "Delete",
                        Text = "X",
                        UseColumnTextForButtonValue = true,
                        Width = 50
                    };
                    Items.Columns.Add(deleteBtn);
                }

                Items.Columns["Price"].DefaultCellStyle.Format = "N2";
            }

            Items.CellClick -= Items_CellClick;
            Items.CellContentClick -= Items_CellContentClick;
            Items.CellClick += Items_CellClick;
            Items.CellContentClick += Items_CellContentClick;
        }
        private void LoadProducts()
            {
            try
            {
                var db = AppDb.Instance;
                DataTable dt = db.TableData("SELECT * FROM Product");

                panel.Controls.Clear();

                string selectedCategoryId = cboCategory.SelectedValue?.ToString();

                foreach (DataRow row in dt.Rows)
                {
                    if (!string.IsNullOrEmpty(selectedCategoryId) &&
                        row["Category_ID"].ToString() != selectedCategoryId)
                        continue;

                    Bunifu.UI.WinForms.BunifuShadowPanel productPanel = new Bunifu.UI.WinForms.BunifuShadowPanel
                    {
                        Width = 150,
                        Height = 200,
                        Margin = new Padding(10),
                        BorderRadius = 15,
                        BorderColor = Color.DarkBlue,
                        BorderThickness = 1,
                        PanelColor = Color.White,
                        ShadowDept = 0,
                    };

                    productPanel.Tag = row;

                    // IMAGE
                    Bunifu.UI.WinForms.BunifuPictureBox picture = new Bunifu.UI.WinForms.BunifuPictureBox
                    {
                        Width = 90,
                        Height = 110,
                        Top = 15,
                        Left = (productPanel.Width - 90) / 2,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Type = Bunifu.UI.WinForms.BunifuPictureBox.Types.Square,
                    };

                    string imagePath = row["Image"].ToString();
                    string fullPath = Path.Combine(Application.StartupPath, imagePath);

                    // ✅ Use FileStream to avoid locking
                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(fullPath))
                    {
                        using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                        {
                            picture.Image = Image.FromStream(fs);
                        }
                    }
                    else
                    {
                        picture.Image = Properties.Resources.NoImage;
                    }

                    picture.Click += (s, e) => AddItemToGrid(row);
                    productPanel.Controls.Add(picture);

                    // NAME
                    Label lblName = new Label
                    {
                        AutoSize = false,
                        Width = productPanel.Width,
                        Top = picture.Bottom + 16,
                        Left = 0,
                        Text = row["ProductName"].ToString(),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                        Cursor = Cursors.Hand
                    };
                    lblName.Click += (s, e) => AddItemToGrid(row);
                    productPanel.Controls.Add(lblName);

                    // PRICE
                    Label lblPrice = new Label
                    {
                        AutoSize = false,
                        Width = productPanel.Width,
                        Top = lblName.Bottom,
                        Left = 0,
                        Text = Convert.ToDecimal(row["UnitPrice"]).ToString("N2"),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                        ForeColor = Color.Black,
                        Cursor = Cursors.Hand
                    };
                    lblPrice.Click += (s, e) => AddItemToGrid(row);
                    productPanel.Controls.Add(lblPrice);

                    productPanel.Click += (s, e) => AddItemToGrid(row);

                    panel.Controls.Add(productPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }

        private void AddItemToGrid(DataRow row)
        {
            string name = row["ProductName"].ToString();
            decimal price = Convert.ToDecimal(row["UnitPrice"]);
            int stock = Convert.ToInt32(row["Quantity"]); // STOCK FROM DB

            // Check if item already exists in cart
            foreach (DataGridViewRow r in Items.Rows)
            {
                if (r.Cells["ProductName"].Value?.ToString() == name)
                {
                    int currentQty = Convert.ToInt32(r.Cells["Quantity"].Value);

                    if (currentQty + 1 > stock)
                    {
                        MessageBox.Show("Not enough stock available!",
                            "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    r.Cells["Quantity"].Value = currentQty + 1;
                    UpdateGrandTotal();
                    return;
                }
            }

            // New item, check if stock is available
            if (stock <= 0)
            {
                MessageBox.Show("This item is out of stock.",
                    "No Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Items.Rows.Add(name, price, 1);
            UpdateGrandTotal();
        }


        private void UpdateGrandTotal()
        {
            decimal total = 0;

            foreach (DataGridViewRow row in Items.Rows)
            {
                if (row.IsNewRow) continue;

                decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                int qty = Convert.ToInt32(row.Cells["Quantity"].Value);

                total += price * qty;
            }

            txtTotal.Text = total.ToString("N2");
        }
        private void LoadCustomers()
            {
            try
            {
                var db = AppDb.Instance;
                DataTable dt = db.TableData("SELECT CustomerID, FirstName FROM Customer");
                cboCustomer.DataSource = dt;
                cboCustomer.DisplayMember = "FirstName";
                cboCustomer.ValueMember = "CustomerID";
                cboCustomer.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message);
            }

        }
        private void LoadCategories()
        {
            try
            {
                cboCategory.SelectedIndexChanged -= cboCategory_SelectedIndexChanged;

                var db = AppDb.Instance;
                DataTable dt = db.TableData("SELECT Category_ID, CategoryName FROM Category");

                cboCategory.DisplayMember = "CategoryName";
                cboCategory.ValueMember = "Category_ID";
                cboCategory.DataSource = dt;

                cboCategory.SelectedIndex = -1;
                cboCategory.SelectedIndexChanged += cboCategory_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

            private void panel_Paint(object sender, PaintEventArgs e)
            {
            }
            private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
            {
                LoadProducts();
            }

        private void Items_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Items_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == Items.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                Items.Rows.RemoveAt(e.RowIndex);
                UpdateGrandTotal();
            }
        }

        private void Items_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == Items.Columns["Quantity"].Index ||
               e.ColumnIndex == Items.Columns["Price"].Index)
            {
                UpdateGrandTotal();
            }
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cboCategory.SelectedItem = null;
            cboCustomer.SelectedItem = null;
            if (Items.Rows.Count == 0)
            {
                MessageBox.Show("Cart & Selection Has been Cleared", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Items.Rows.Clear();
            txtTotal.Text = "0.00";

            cboCategory.SelectedIndex = -1;
            cboCustomer.SelectedIndex = -1;

            MessageBox.Show("Order cleared successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (Items.Rows.Count == 0)
            {
                MessageBox.Show("No items to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cboCustomer.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var db = (SqlDatabase)AppDb.Instance;
                int customerID = Convert.ToInt32(cboCustomer.SelectedValue);
                DateTime dateSold = bunifuDatePicker1.Value;

                // Transaction
                bool transactionResult = db.ExecuteTransaction(tran =>
                {
                    foreach (DataGridViewRow row in Items.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string productName = row.Cells["ProductName"].Value.ToString();
                        decimal unitPrice = Convert.ToDecimal(row.Cells["Price"].Value);
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                        decimal totalPrice = unitPrice * quantity;

                        // Get ProductID
                        DataTable dtProduct = db.TableData(
                            "SELECT ProductID, Quantity FROM Product WHERE ProductName = @ProductName",
                            new { ProductName = productName }
                        );

                        if (dtProduct.Rows.Count == 0)
                        {
                            MessageBox.Show($"Product '{productName}' not found.");
                            return false;
                        }

                        int productID = Convert.ToInt32(dtProduct.Rows[0]["ProductID"]);
                        int currentStock = Convert.ToInt32(dtProduct.Rows[0]["Quantity"]);

                        // STOCK VALIDATION
                        if (quantity > currentStock)
                        {
                            MessageBox.Show(
                                $"Not enough stock for '{productName}'. Available: {currentStock}",
                                "Stock Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                            return false; // rollback
                        }

                        // Insert into SalesTransaction
                        int transactionID = db.SaveReturnID("SalesTransaction", new
                        {
                            ProductID = productID,
                            CustomerID = customerID,
                            DateSold = dateSold,
                            Quantity = quantity,
                            TotalPrice = totalPrice
                        });

                        if (transactionID <= 0)
                        {
                            MessageBox.Show($"Failed to save transaction for '{productName}'.");
                            return false;
                        }

                        // Insert into history
                        bool historyOK = db.Save("SalesTransactionHistory", new
                        {
                            TransactionID = transactionID,
                            ProductID = productID,
                            CustomerID = customerID,
                            DateSold = dateSold,
                            Quantity = quantity,
                            TotalPrice = totalPrice
                        });

                        if (!historyOK)
                        {
                            MessageBox.Show($"Failed to save history for '{productName}'.");
                            return false;
                        }
                        // bawas sa stocks
                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE Product SET Quantity = @NewQty WHERE ProductID = @ProductID",
                            tran.Connection,
                            tran))
                        {
                            cmd.Parameters.AddWithValue("@NewQty", currentStock - quantity);
                            cmd.Parameters.AddWithValue("@ProductID", productID);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected <= 0)
                            {
                                MessageBox.Show($"Failed to update stock for '{productName}'.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false; // rollback
                            }
                        }
                    }

                    return true; // commit
                });


                // AYos
                if (transactionResult)
                {
                    MessageBox.Show("Order saved successfully!", "Info",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear UI
                    Items.Rows.Clear();
                    txtTotal.Text = "0.00";
                    cboCategory.SelectedIndex = -1;
                    cboCustomer.SelectedIndex = -1;

                    DataSaved?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving order: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }   

        private void bunifuDatePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
