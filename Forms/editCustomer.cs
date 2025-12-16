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
    public partial class editCustomer : Form
    {
        private string _CustomerID;
        public event EventHandler DataSaved;
        public editCustomer(string CustomerID)
        {
            InitializeComponent();
            _CustomerID = CustomerID;
        }

        private void editCustomer_Load(object sender, EventArgs e)
        {
            loadData();
        }
        public void loadData()
        {
            try
            {
                var db = AppDb.Instance;
                DataTable dt = db.TableData(
                    "SELECT FirstName, MiddleName, LastName, Email, ContactNumber, Barangay, City, Province FROM Customer WHERE CustomerID = @id",
                    new { id = _CustomerID }
                );

                if (dt.Rows.Count > 0)
                {
                    txtFirstNme.Text = dt.Rows[0]["FirstName"].ToString();
                    txtMiddleName.Text = dt.Rows[0]["MiddleName"].ToString();
                    txtLastName.Text = dt.Rows[0]["LastName"].ToString();
                    txtGmail.Text = dt.Rows[0]["Email"].ToString();
                    txtContactNumber.Text = dt.Rows[0]["ContactNumber"].ToString();
                    txtBarangay.Text = dt.Rows[0]["Barangay"].ToString();
                    txtCity.Text = dt.Rows[0]["City"].ToString();
                    txtProvince.Text = dt.Rows[0]["Province"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customer data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void bttnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFirstNme.Text) &&
    !string.IsNullOrWhiteSpace(txtMiddleName.Text) &&
    !string.IsNullOrWhiteSpace(txtLastName.Text) &&
    !string.IsNullOrWhiteSpace(txtGmail.Text) &&
    !string.IsNullOrWhiteSpace(txtContactNumber.Text) &&
    !string.IsNullOrWhiteSpace(txtBarangay.Text) &&
    !string.IsNullOrWhiteSpace(txtCity.Text) &&
    !string.IsNullOrWhiteSpace(txtProvince.Text))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(txtContactNumber.Text.Trim(), @"^\d{11}$"))
                {
                    if (txtGmail.Text.Trim().EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var db = AppDb.Instance;
                            var ok = db.Update("Customer",
                                new
                                {
                                    CustomerID = _CustomerID,
                                    FirstName = txtFirstNme.Text.Trim(),
                                    MiddleName = txtMiddleName.Text.Trim(),
                                    LastName = txtLastName.Text.Trim(),
                                    Email = txtGmail.Text.Trim(),
                                    ContactNumber = txtContactNumber.Text.Trim(),
                                    Barangay = txtBarangay.Text.Trim(),
                                    City = txtCity.Text.Trim(),
                                    Province = txtProvince.Text.Trim()
                                },
                                keyColumn: "CustomerID"
                                );
                            MessageBox.Show(ok ? "Successfully Updated." : "Insert failed.");
                            DataSaved?.Invoke(this, EventArgs.Empty);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error updating customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Email must end with @gmail.com.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Contact number must be exactly 11 digits.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits and control characters (like Backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Ignore the key press
            }
        }
    }
}
