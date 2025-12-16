using barcodescanner.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace barcodescanner.Forms
{
    public partial class editInventory : Form
    {
        private string _ProductID;
        public event EventHandler DataSaved;
        private EventHandler Datasaved;
        private string selectedImagePath = "";
        private Bitmap qrBitmap;// downloaded nuget package ZXing.Net   
        Bitmap QRImage; // for printing
        string generatedBarcode; // declarion of printing
        public editInventory(string productID)
        {
            InitializeComponent();
            _ProductID = productID;
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadCategoryDropdown()
        {
            var db = AppDb.Instance;

            DataTable dtCategory = db.TableData("SELECT Category_ID, CategoryName FROM Category");
            dpCategory.DataSource = dtCategory;
            dpCategory.DisplayMember = "CategoryName";
            dpCategory.ValueMember = "Category_ID";
            dpCategory.SelectedIndex = -1;
        }
        private void LoadSupplierDropdown()
        {
            var db = AppDb.Instance;
            DataTable dtSupplier = db.TableData("SELECT SupplierID, CompanyName FROM Supplier");
            dpSupplier.DataSource = dtSupplier;
            dpSupplier.DisplayMember = "CompanyName"; 
            dpSupplier.ValueMember = "SupplierID";   
            dpSupplier.SelectedIndex = -1;           
        }
        public void laodData()
        {
            try
            {
                var db = AppDb.Instance;
                DataTable dt = db.TableData(
                    "SELECT Category_ID, SupplierID, ProductName, UnitPrice, Quantity, Image, QRCodeID FROM Product WHERE ProductID = @id",
                    new { id = _ProductID }
                );

                if (dt.Rows.Count > 0)
                {
                    dpCategory.SelectedValue = dt.Rows[0]["Category_ID"].ToString();
                    dpSupplier.SelectedValue = dt.Rows[0]["SupplierID"].ToString();
                    txtPN.Text = dt.Rows[0]["ProductName"].ToString();
                    txtUP.Text = dt.Rows[0]["UnitPrice"].ToString();
                    txtQuan.Text = dt.Rows[0]["Quantity"].ToString();

                    // Load product image
                    string imagePath = dt.Rows[0]["Image"].ToString();
                    string fullPath = Path.Combine(Application.StartupPath, imagePath);
                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(fullPath))
                    {
                        pcbItem.Image = Image.FromFile(fullPath);
                        pcbItem.Tag = imagePath;
                    }
                    else
                    {
                        pcbItem.Image = Properties.Resources.NoImage;
                        pcbItem.Tag = null;
                    }

                    // Load QR code text
                    if (dt.Rows[0]["QRCodeID"] != DBNull.Value)
                    {
                        int qrId = Convert.ToInt32(dt.Rows[0]["QRCodeID"]);
                        DataTable dtQr = db.TableData(
                            "SELECT QRCodeText, QRCodeImage FROM QRCodes WHERE QRCodeID = @id",
                            new { id = qrId }
                        );

                        if (dtQr.Rows.Count > 0)
                        {
                            txtQRCode.Text = dtQr.Rows[0]["QRCodeText"].ToString();

                            // Load QR image
                            byte[] qrBytes = (byte[])dtQr.Rows[0]["QRCodeImage"];
                            using (MemoryStream ms = new MemoryStream(qrBytes))
                            {
                                picQR.Image = Image.FromStream(ms);
                                qrBitmap = new Bitmap(picQR.Image); // assign to qrBitmap for printing
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading product data: " + ex.Message);
            }
        }

        private void editInventory_Load(object sender, EventArgs e)
        {
            LoadCategoryDropdown();
            LoadSupplierDropdown();
            laodData();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (dpCategory.SelectedValue == null ||
                    dpSupplier.SelectedValue == null ||
                    string.IsNullOrWhiteSpace(txtPN.Text) ||
                    string.IsNullOrWhiteSpace(txtUP.Text) ||
                    string.IsNullOrWhiteSpace(txtQuan.Text))
                {
                    MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Validate numeric fields
                if (!decimal.TryParse(txtUP.Text.Trim(), out decimal unitPrice))
                {
                    MessageBox.Show("Unit Price must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtQuan.Text.Trim(), out int quantity))
                {
                    MessageBox.Show("Quantity must be a valid integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var db = AppDb.Instance;
                // Handle image path
                string imagePath = pcbItem.Tag?.ToString() ?? "";

                var updateOk = db.Update(
                    "Product",
                    new
                    {
                        ProductID = _ProductID,
                        Category_ID = dpCategory.SelectedValue?.ToString(),
                        SupplierID = dpSupplier.SelectedValue?.ToString(),
                        ProductName = txtPN.Text.Trim(),
                        UnitPrice = unitPrice,
                        Quantity = quantity,
                        Image = imagePath
                    },
                    keyColumn: "ProductID"
                );

                MessageBox.Show(updateOk ? "Product updated successfully!" : "Update failed.",
                                "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DataSaved?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bunifuButton1_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = ofd.FileName;
                    string destFolder = Path.Combine(Application.StartupPath, "ProductImages");
                    if (!Directory.Exists(destFolder))
                        Directory.CreateDirectory(destFolder);

                    string fileName = Path.GetFileName(selectedImagePath);
                    string destPath = Path.Combine(destFolder, fileName);
                    File.Copy(selectedImagePath, destPath, true);

                    selectedImagePath = "ProductImages\\" + fileName;

                    pcbItem.Image = Image.FromFile(destPath);
                }
            }
        }
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string qrText = "ITEM-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            txtQRCode.Text = qrText;

            var writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = picQR.Height,
                    Width = picQR.Width
                }
            };

            qrBitmap = writer.Write(qrText);
            picQR.Image = qrBitmap;  // Show QR in PictureBox for preview
            string folderPath = Path.Combine(Application.StartupPath, "QRCodes");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"{qrText}.png");
            qrBitmap.Save(filePath);

            // Save QR to database
            var db = AppDb.Instance;
            var ok = db.Save("QRCodes",
                new
                {
                    QRCodeText = qrText,
                    QRCodeImage = File.ReadAllBytes(filePath)
                });

            MessageBox.Show(ok ? "QR code saved successfully!" : "Save failed.");
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (qrBitmap == null)
            {
                MessageBox.Show("Please generate a QR code first.");
                return;
            }

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (s, ev) =>
            {
                int x = (ev.PageBounds.Width - qrBitmap.Width) / 2;
                int y = (ev.PageBounds.Height - qrBitmap.Height) / 2;
                ev.Graphics.DrawImage(qrBitmap, x, y);
            };

            PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = pd,
                Width = 800,
                Height = 600,
                StartPosition = FormStartPosition.CenterParent
            };

            // Make sure the dialog appears on top of the current form
            preview.Show(this); // <-- Non-modal, main form stays active
        }
    }
}
