using barcodescanner.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace barcodescanner.Forms
{
    public partial class AddInventory : Form
    {
        private int _selectedId = 0;
        public event EventHandler DataSaved;
        private string selectedImagePath = "";
        private Bitmap qrBitmap;// downloaded nuget package ZXing.Net   
        Bitmap QRImage; // for printing
        string generatedBarcode; // declarion of printing

        public AddInventory()
        {
            InitializeComponent();
        }

        private void AddInventory_Load(object sender, EventArgs e)
        {
            LoadCategoryDropdown();
            LoadSupplierDropdown(); 


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
        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            this.Close();   
        }
        public void clear()
        {
            txtPN.Text = "";
            txtQuan.Text = "";
            dpSupplier.SelectedItem = "";
            txtUP.Text = "";
            dpCategory.SelectedItem = null;
        }
        private void bttnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtPN.Text) &&
    !string.IsNullOrWhiteSpace(txtUP.Text) &&
    !string.IsNullOrWhiteSpace(txtQuan.Text) &&
    !string.IsNullOrWhiteSpace(txtQRCode.Text) &&
    dpCategory.SelectedIndex != -1 &&
    dpSupplier.SelectedIndex != -1)
            {
                if (decimal.TryParse(txtUP.Text.Trim(), out decimal unitPrice))
                {
                    if (int.TryParse(txtQuan.Text.Trim(), out int quantity))
                    {
                        try
                        {
                            var db = AppDb.Instance;
                            string qrText = txtQRCode.Text.Trim();

                            // Get QRCodeID based on QR Text
                            DataTable dtQr = db.TableData(
                                "SELECT QRCodeID FROM QRCodes WHERE QRCodeText = @txt",
                                new { txt = qrText }
                            );

                            if (dtQr.Rows.Count == 0)
                            {
                                MessageBox.Show("QR code does not exist in the database. Please generate first.",
                                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            int qrCodeID = Convert.ToInt32(dtQr.Rows[0]["QRCodeID"]);

                            // Check if this QR is already used
                            DataTable dtCheck = db.TableData(
                                "SELECT ProductID FROM Product WHERE QRCodeID = @id",
                                new { id = qrCodeID }
                            );

                            if (dtCheck.Rows.Count > 0)
                            {
                                MessageBox.Show("This QR code is already assigned to another product.",
                                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Save product
                            var ok = db.Save("Product",
                                new
                                {
                                    Category_ID = dpCategory.SelectedValue.ToString(),
                                    SupplierID = dpSupplier.SelectedValue.ToString(),
                                    ProductName = txtPN.Text.Trim(),
                                    UnitPrice = unitPrice,
                                    Quantity = quantity,
                                    QRCodeID = qrCodeID,
                                    Image = string.IsNullOrEmpty(selectedImagePath) ? null : selectedImagePath
                                });

                            MessageBox.Show(ok ? "Successfully Saved." : "Insert failed.",
                                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            if (ok) DataSaved?.Invoke(this, EventArgs.Empty);

                            clear();
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error saving product: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Quantity must be a valid number.",
                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Unit Price must be a valid number.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please complete all fields.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtMindeus_Click(object sender, EventArgs e)
        {

        }
        private void btnAddImage_Click(object sender, EventArgs e)
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
                    Height =picQR.Height,
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
