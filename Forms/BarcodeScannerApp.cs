using AForge.Video;
using AForge.Video.DirectShow;
using barcodescanner.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace barcodescanner
{
    public partial class BarcodeScannerApp : Form
    {
        private FilterInfoCollection filterInfo; //filter for camera
        private VideoCaptureDevice device; //device to capture video
        private List<string> scanHistory; //list to store scan history
        private bool isScanning = false; //flag to prevent multiple scans
        private bool isClosing = false; //flag to indicate form is closing
        private decimal totalPrice = 0; //total price of scanned items
        private List<ScannedProduct> scannedProducts = new List<ScannedProduct>(); //list of scanned products
        private PrintDocument printDocument = new PrintDocument();

        public BarcodeScannerApp()
        {
            InitializeComponent();
            btnStop.Visible = false; //button close is false in load
            scanHistory = new List<string>(); //initialize scan history list
        }

        public class ScannedProduct
        {
            public string ProductName { get; set; } //name of the product
            public string Supplier { get; set; } //supplier name
            public string Company { get; set; } //company name
            public decimal Price { get; set; } //price of the product
            public int Quantity { get; set; } = 1; //quantity of the product //default is 1
            public string Timestamp { get; set; } //timestamp of the scan
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                filterInfo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (filterInfo.Count == 0)
                {
                    MessageBox.Show("No video capture devices found. Please connect a camera.", "No Camera Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnStart.Enabled = false;
                    return;
                }

                foreach (FilterInfo info in filterInfo)
                    cboCamera.Items.Add(info.Name);

                cboCamera.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cameras: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Trim and normalize spaces
        private string CleanForDisplay(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Trim();
            return string.Join(" ", input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cboCamera.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a camera.", "No Camera Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                StopCamera();

                device = new VideoCaptureDevice(filterInfo[cboCamera.SelectedIndex].MonikerString);
                device.NewFrame += VideoCaptureDevice_NewFrame;
                device.Start();

                btnStart.Visible = false;
                btnStop.Visible = true;
                cboCamera.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera: {ex.Message}", "Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (isClosing) return;

            Bitmap bitmap = null;
            try
            {
                bitmap = (Bitmap)eventArgs.Frame.Clone();

                Camera.Invoke(new MethodInvoker(() =>
                {
                    Camera.Image?.Dispose();
                    Camera.Image = (Bitmap)bitmap.Clone();
                }));

                var result = new BarcodeReader().Decode(bitmap);
                if (result != null && !isScanning)
                {
                    isScanning = true;
                    string qrText = result.Text;

                    txtInfo.Invoke(new MethodInvoker(() =>
                    {
                        txtInfo.Text = $"QR: {qrText}";
                        txtInfo.BackColor = Color.LightYellow;
                        Console.Beep(1000, 200);
                    }));

                    var db = AppDb.Instance;
                    DataTable dt = db.TableData(
                        @"SELECT p.ProductName, 
                                 s.FirstName + ' ' + s.LastName AS SupplierName, 
                                 s.CompanyName, 
                                 p.UnitPrice, 
                                 p.Quantity
                          FROM Product p
                          INNER JOIN Supplier s ON p.SupplierID = s.SupplierID
                          INNER JOIN QRCodes q ON p.QRCodeID = q.QRCodeID
                          WHERE q.QRCodeText = @QRCodeText",
                        new { QRCodeText = qrText }
                    );

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        string productName = CleanForDisplay(row["ProductName"].ToString());
                        string supplierName = CleanForDisplay(row["SupplierName"].ToString());
                        string companyName = CleanForDisplay(row["CompanyName"].ToString());
                        decimal price = decimal.TryParse(row["UnitPrice"].ToString(), out decimal p) ? p : 0;
                        int dbQuantity = int.TryParse(row["Quantity"].ToString(), out int q) ? q : 1;

                        // Ask user for quantity (no new form)
                        int chosenQty = AskQuantity(1);

                        // Add or update scanned product
                        var existing = scannedProducts.FirstOrDefault(x =>
                            x.ProductName == productName &&
                            x.Supplier == supplierName &&
                            x.Company == companyName &&
                            x.Price == price
                        );

                        if (existing != null)
                        {
                            existing.Quantity += chosenQty;
                        }
                        else
                        {
                            scannedProducts.Add(new ScannedProduct
                            {
                                ProductName = productName,
                                Supplier = supplierName,
                                Company = companyName,
                                Price = price,
                                Quantity = chosenQty,
                                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }



                        totalPrice = scannedProducts.Sum(x => x.Price * x.Quantity);

                        txtNumber.Invoke(new MethodInvoker(() =>
                        {
                            txtNumber.Text = totalPrice.ToString("F2");
                        }));

                        lstScanHistory.Invoke(new MethodInvoker(() =>
                        {
                            lstScanHistory.Items.Clear();
                            foreach (var item in scannedProducts)
                            {
                                string line = $"{item.ProductName} | {item.Supplier} | {item.Company} | {item.Price:F2} | Qty: {item.Quantity}";
                                lstScanHistory.Items.Add(line);
                            }
                        }));
                    }
                    else
                    {
                        txtNumber.Invoke(new MethodInvoker(() => txtNumber.Text = "0.00"));
                    }

                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        if (!isClosing) isScanning = false;
                    });
                }
            }
            catch { }
            finally
            {
                bitmap?.Dispose();
            }
        }

        private void StopCamera()
        {
            if (device != null)
            {
                try
                {
                    device.NewFrame -= VideoCaptureDevice_NewFrame;
                    if (device.IsRunning)
                    {
                        var stopTask = Task.Run(() => device.Stop());
                        stopTask.Wait(500);
                    }
                    device = null;
                    isScanning = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping camera: {ex.Message}");
                }
            }

            if (!isClosing)
            {
                btnStart.Visible = true;
                btnStop.Visible = false;
                cboCamera.Enabled = true;

                try
                {
                    Camera.Invoke(new MethodInvoker(() =>
                    {
                        if (isClosing) return;
                        Camera.Image?.Dispose();
                        Camera.Image = null;
                        txtInfo.Text = string.Empty;
                        txtInfo.BackColor = SystemColors.Window;
                    }));
                }
                catch { }
            }
        }

        private void btnStop_Click(object sender, EventArgs e) => StopCamera();

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            scannedProducts.Clear();
            lstScanHistory.Items.Clear();
            txtInfo.Text = "History cleared.";
            txtNumber.Text = "0.00";
            totalPrice = 0;
        }

        private void btnExportHistory_Click(object sender, EventArgs e)
        {
            if (scannedProducts.Count == 0)
            {
                MessageBox.Show("No scans to export.", "Nothing to Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv";
                saveDialog.FileName = $"ScanHistory_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                saveDialog.Title = "Export Scan History";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                        {
                            writer.WriteLine("Product,Supplier,Company,Quantity,Price,Subtotal");

                            decimal total = 0;
                            foreach (var item in scannedProducts)
                            {
                                decimal subtotal = item.Price * item.Quantity;
                                total += subtotal;

                                writer.WriteLine($"{item.ProductName},{item.Supplier},{item.Company},{item.Quantity},{item.Price:F2},{subtotal:F2}");
                            }

                            writer.WriteLine($",TOTAL,,,,{total:F2}");
                        }

                        MessageBox.Show("History exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void lstScanHistory_DoubleClick(object sender, EventArgs e)
        {
            if (lstScanHistory.SelectedIndex >= 0)
            {
                string selected = lstScanHistory.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selected))
                {
                    Clipboard.SetText(selected);
                    MessageBox.Show("Scan copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isClosing = true;
            StopCamera();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveHistoryToFile(@"C:\Users\Jayoffe\source\repos\barcodescanner\barcodescanner\bin\Debug\History\ScanHistoryOnClose.csv");
        }

        private void SaveHistoryToFile(string filename)
        {
            if (scanHistory.Count == 0) return;
            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.WriteLine("Timestamp,Info");
                    foreach (string entry in scanHistory)
                    {
                        int idx = entry.IndexOf("] ");
                        if (idx < 0) continue;
                        string timestamp = entry.Substring(1, idx - 1).Trim();
                        string info = entry.Substring(idx + 2).Trim();
                        writer.WriteLine($"\"{timestamp}\",\"{info}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving history: {ex.Message}");
            }
        }

        private void goHome_Click(object sender, EventArgs e)
        {
            this.Hide(); 
            using (Home newForm = new Home()) 
            { 
                newForm.ShowDialog(); 
            
            }
            this.Close();
        }
        private int AskQuantity(int defaultValue = 1)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter Quantity:",
                "Quantity",
                defaultValue.ToString()
            );

            if (int.TryParse(input, out int qty) && qty > 0)
                return qty;

            return defaultValue;
        }

        private void txtNumber_Click(object sender, EventArgs e)
        {

        }

        private bool SaveOrder()
        {
            try
            {
                // TODO: Implement actual saving to database or file
                // You could serialize scannedProducts or insert into SalesTransactionHistory table

                return true; // return true if save succeeds
            }
            catch
            {
                return false; // return false if save fails
            }
        }


        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (scannedProducts.Count == 0)
            {
                MessageBox.Show("No items scanned.");
                return;
            }

            pnlPayment.Visible = true;

            // Change form size when panel is visible
            this.Size = new Size(1344, 719);


            // Change picture box location when panel is visible
            Camera.Location = new Point(16, 135);
            goHome.Location = new Point(1226, 17);

            lblTotalAmount.Text = totalPrice.ToString("F2");
            txtCash.Text = "";
            lblChange.Text = "0.00";

            // Update texts
            txtTotal.Text = "Transacting";
            txtNumber.Text = "Now";
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            // Validate payment
            if (!decimal.TryParse(txtCash.Text, out decimal cash))
            {
                MessageBox.Show("Invalid payment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cash < totalPrice)
            {
                MessageBox.Show("Insufficient payment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Calculate change
            decimal change = cash - totalPrice;
            lblChange.Text = change.ToString("F2");

            MessageBox.Show("Payment Successful!", "Done");

            // Print invoice
            printDocument.PrintPage -= PrintDocument_PrintPage;
            printDocument.PrintPage += PrintDocument_PrintPage;

            using (PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = printDocument,
                Width = 900,
                Height = 700
            })
            {
                preview.ShowDialog();
            }

            // Save order
            bool saved = SaveOrder();
            if (!saved)
            {
                MessageBox.Show("Failed to save order.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Reset UI
            pnlPayment.Visible = false;
            scannedProducts.Clear();
            lstScanHistory.Items.Clear();
            totalPrice = 0;

            // Reset textboxes
            txtNumber.Text = "0.00";
            txtTotal.Text = "Total Price:";
            txtCash.Text = "";
            lblChange.Text = "0.00";

            // Optionally resize/relocate form or controls if needed
            this.Size = new Size(898, 719);
            goHome.Location = new Point(793, 12);
            Camera.Location = new Point(16, 135);
        }



        private void txtCash_TextChange(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtCash.Text, out decimal cash))
            {
                decimal change = cash - totalPrice;
                lblChange.Text = change >= 0 ? change.ToString("F2") : "0.00";
            }
            else
            {
                lblChange.Text = "0.00";
            }
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
            e.Graphics.DrawString("Invoice", titleFont, new SolidBrush(Color.DarkBlue), left, top);
            top += 40;

            // Headers
            e.Graphics.DrawString("Product", headerFont, Brushes.Black, left, top);
            e.Graphics.DrawString("Qty", headerFont, Brushes.Black, left + colProduct, top);
            e.Graphics.DrawString("Price", headerFont, Brushes.Black, left + colProduct + colQty, top);
            e.Graphics.DrawString("Total", headerFont, Brushes.Black, left + colProduct + colQty + colPrice, top);

            top += lineHeight;
            e.Graphics.DrawLine(Pens.Black, left, top, left + colProduct + colQty + colPrice + colTotal, top);
            top += 5;

            decimal totalAmount = 0m;

            foreach (var item in scannedProducts)
            {
                string product = item.ProductName;
                int qty = item.Quantity;
                decimal price = item.Price;
                decimal total = price * qty;
                totalAmount += total;

                e.Graphics.DrawString(product, rowFont, Brushes.Black, new RectangleF(left, top, colProduct, lineHeight), sf);
                e.Graphics.DrawString(qty.ToString(), rowFont, Brushes.Black, new RectangleF(left + colProduct, top, colQty, lineHeight), sf);
                e.Graphics.DrawString(price.ToString("N2"), rowFont, Brushes.Black, new RectangleF(left + colProduct + colQty, top, colPrice, lineHeight), sf);
                e.Graphics.DrawString(total.ToString("N2"), rowFont, Brushes.Black, new RectangleF(left + colProduct + colQty + colPrice, top, colTotal, lineHeight), sf);

                top += lineHeight;
            }

            top += 20;
            Font totalFont = new Font("Arial", 12, FontStyle.Bold);

            // Total Amount
            e.Graphics.DrawString("Total Amount: ₱" + totalAmount.ToString("N2"), totalFont, Brushes.Black, left, top);
            top += lineHeight;

            // Customer Payment
            if (decimal.TryParse(txtCash.Text, out decimal payment))
            {
                e.Graphics.DrawString("Payment: ₱" + payment.ToString("N2"), totalFont, Brushes.Black, left, top);
                top += lineHeight;

                // Change
                decimal change = payment - totalAmount;
                e.Graphics.DrawString("Change: ₱" + change.ToString("N2"), totalFont, Brushes.Black, left, top);
            }
        }


    }
}
