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
using ZXing.Client.Result;

namespace barcodescanner.Forms
{
    public partial class AddCustomer : Form
    {
        private int _selectedId = 0;
        public event EventHandler DataSaved;
        public AddCustomer()
        {
            InitializeComponent();
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AddCustomer_Load(object sender, EventArgs e)
        {

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
                            var ok = db.Save("Customer",
                                new
                                {
                                    FirstName = txtFirstNme.Text.Trim(),
                                    MiddleName = txtMiddleName.Text.Trim(),
                                    LastName = txtLastName.Text.Trim(),
                                    Email = txtGmail.Text.Trim(),
                                    ContactNumber = txtContactNumber.Text.Trim(),
                                    Barangay = txtBarangay.Text.Trim(),
                                    City = txtCity.Text.Trim(),
                                    Province = txtProvince.Text.Trim()
                                });

                            MessageBox.Show(ok ? "Successfully Saved." : "Insert failed.");
                            DataSaved?.Invoke(this, EventArgs.Empty);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error saving customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void bunifuLabel1_Click(object sender, EventArgs e)
        {

        }

        private void txtMindeus_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel3_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel2_Click(object sender, EventArgs e)
        {

        }

        private void __________________________Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel9_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel10_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel5_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel6_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel7_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel8_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel11_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel12_Click(object sender, EventArgs e)
        {

        }

        private void txtMiddleName_TextChanged(object sender, EventArgs e)
        {

        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bunifuLabel4_Click(object sender, EventArgs e)
        {

        }

        private void txtLastName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtFirstNme_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtCity_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBarangay_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtProvince_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtGmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtContactNumber_TextChanged(object sender, EventArgs e)
        {

        }

        private void bunifuLabel13_Click(object sender, EventArgs e)
        {

        }

        private void txtFirstNme_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = true; 
            }
        }

        private void txtMiddleName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = true; 
            }
        }
        private void txtLastName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = true; 
            }
        }
        private void txtContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; 
            }
        }
    }
}
