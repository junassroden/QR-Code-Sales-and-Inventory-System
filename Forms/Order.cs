using barcodescanner.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace barcodescanner.Forms
{
    public partial class Order : Form
    {
        private int scrollOffset = 0;
        private PrintDocument printDocument = new PrintDocument();
        private string printTitle = "Mindeus Sales Transactions Report";


        public Order()
        {
            InitializeComponent();
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
        private async void btnCustomer_Click(object sender, EventArgs e)
        {
            Customers customer = new Customers();
            customer.StartPosition = FormStartPosition.Manual;
            customer.Location = this.Location;
            customer.Show();
            await Task.Delay(200);
            this.Close();
        }
        private void button2_Click_1(object sender, EventArgs e)
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
        private void Order_Load(object sender, EventArgs e)
        {
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = true;
            panel.AutoScroll = true;
            panel.HorizontalScroll.Enabled = false;
            panel.HorizontalScroll.Visible = false;
            panel.MouseWheel += Panel_MouseWheel;

            cboCategory.SelectedIndexChanged += cboCategory_SelectedIndexChanged;

            LoadCustomers();
            LoadCategories();
            LoadProducts();
            SetupGridView();
        }
        private void Panel_MouseWheel(object sender, MouseEventArgs e)
        {
            int scrollSpeed = 1;
            int newValue = panel.VerticalScroll.Value - (e.Delta / 70) * scrollSpeed;
            newValue = Math.Max(panel.VerticalScroll.Minimum,
                                Math.Min(panel.VerticalScroll.Maximum, newValue));
            panel.VerticalScroll.Value = newValue;
            panel.PerformLayout();
        }

        private void SetupGridView()
        {
            if (Items.Columns.Count == 0)
            {
                Items.AllowUserToAddRows = false;
                Items.Columns.Add("ProductName", "Product Name");
                Items.Columns.Add("Price", "Price");
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
            Items.CellEndEdit += Items_CellEndEdit;
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
                        ShadowDept = 0
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

                    picture.Image = !string.IsNullOrEmpty(imagePath) && File.Exists(fullPath)
                        ? Image.FromFile(fullPath)
                        : Properties.Resources.NoImage;

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

                    // QUANTITY LABEL at bottom-left
                    Label lblQty = new Label
                    {
                        AutoSize = true,
                        Text = "Qty: " + row["Quantity"].ToString(),
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                        Left = 5,
                        Top = productPanel.Height - 25 // bottom-left
                    };
                    productPanel.Controls.Add(lblQty);

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
            int stock = Convert.ToInt32(row["Quantity"]);

            foreach (DataGridViewRow r in Items.Rows)
            {
                if (r.Cells["ProductName"].Value?.ToString() == name)
                {
                    int currentQty = Convert.ToInt32(r.Cells["Quantity"].Value);
                    if (currentQty + 1 > stock)
                    {
                        MessageBox.Show("Not enough stock available!", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    r.Cells["Quantity"].Value = currentQty + 1;
                    UpdateGrandTotal();
                    return;
                }
            }

            if (stock <= 0)
            {
                MessageBox.Show("This item is out of stock.", "No Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private async void btnDashboard_Click_1(object sender, EventArgs e)
        {
            OrderModuleForm dash = new OrderModuleForm();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();
            await Task.Delay(200);
            this.Close();
        }

        private async void btnCustomer_Click_1(object sender, EventArgs e)
        {
            Customers dash = new Customers();
            dash.StartPosition = FormStartPosition.Manual;
            dash.Location = this.Location;
            dash.Show();
            await Task.Delay(200);
            this.Close();
        }
        private void btnHistory_Click(object sender, EventArgs e)
        {
            History history = new History();
            history.StartPosition = FormStartPosition.Manual;
            history.Location = this.Location;
            history.Show();
            this.Close();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            // Print logic first
            printDocument.PrintPage -= PrintDocument_PrintPage;
            printDocument.PrintPage += PrintDocument_PrintPage;

            using (PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = printDocument,
                Width = 900,
                Height = 700
            })
            {
                DialogResult result = preview.ShowDialog();

                // If user closed preview, continue to save
                if (result == DialogResult.OK || result == DialogResult.Cancel)
                {
                    // Save the order after printing
                    bool saved = SaveOrder();
                    if (!saved)
                    {
                        MessageBox.Show("Failed to save order after printing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // New method to save order
        private bool SaveOrder()
        {
            if (Items.Rows.Count == 0)
            {
                MessageBox.Show("No items to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (cboCustomer.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
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

                        // Get ProductID and current stock
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

                        // Update stock
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
                                return false;
                            }
                        }
                    }

                    return true; // commit
                });

                if (transactionResult)
                {
                   
                    // Clear UI
                    Items.Rows.Clear();
                    txtTotal.Text = "0.00";
                    cboCategory.SelectedIndex = -1;
                    cboCustomer.SelectedIndex = -1;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            int left = 50, top = 50, lineHeight = 28;
            int colProduct = 200, colQty = 60, colPrice = 80, colTotal = 100;

            Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            Font headerFont = new Font("Arial", 11, FontStyle.Bold);
            Font rowFont = new Font("Arial", 10);
            StringFormat sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };

            // Title
            e.Graphics.DrawString(printTitle, titleFont, new SolidBrush(Color.DarkBlue), left, top);
            top += 40;

            // Headers
            e.Graphics.DrawString("Product", headerFont, Brushes.Black, left, top);
            e.Graphics.DrawString("Qty", headerFont, Brushes.Black, left + colProduct, top);
            e.Graphics.DrawString("Price", headerFont, Brushes.Black, left + colProduct + colQty, top);
            e.Graphics.DrawString("Total", headerFont, Brushes.Black, left + colProduct + colQty + colPrice, top);

            top += lineHeight;
            e.Graphics.DrawLine(Pens.Black, left, top, left + colProduct + colQty + colPrice + colTotal, top);
            top += 5;

            decimal totalSales = 0m;

            foreach (DataGridViewRow row in Items.Rows)
            {
                if (row.IsNewRow) continue;

                string product = row.Cells["ProductName"].Value?.ToString() ?? "";
                int qty = Convert.ToInt32(row.Cells["Quantity"].Value);
                decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                decimal total = price * qty;
                totalSales += total;

                e.Graphics.DrawString(product, rowFont, Brushes.Black, new RectangleF(left, top, colProduct, lineHeight), sf);
                e.Graphics.DrawString(qty.ToString(), rowFont, Brushes.Black, new RectangleF(left + colProduct, top, colQty, lineHeight), sf);
                e.Graphics.DrawString(price.ToString("N2"), rowFont, Brushes.Black, new RectangleF(left + colProduct + colQty, top, colPrice, lineHeight), sf);
                e.Graphics.DrawString(total.ToString("N2"), rowFont, Brushes.Black, new RectangleF(left + colProduct + colQty + colPrice, top, colTotal, lineHeight), sf);

                top += lineHeight;
            }

            top += 10;
            Font totalFont = new Font("Arial", 12, FontStyle.Bold);
            e.Graphics.DrawString("TOTAL SALES: ₱" + totalSales.ToString("N2"), totalFont, Brushes.Black, left, top);
        }


        private void cboCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProducts();
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

        private void Items_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == Items.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                Items.Rows.RemoveAt(e.RowIndex);
                UpdateGrandTotal();
            }
        }

        private void Items_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Items_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == Items.Columns["Quantity"].Index ||
             e.ColumnIndex == Items.Columns["Price"].Index)
            {
                UpdateGrandTotal();
            }
        }
    }
}
