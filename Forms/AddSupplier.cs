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
    public partial class AddSupplier : Form
    {
          public event EventHandler DataSaved;
        public AddSupplier()
        {
            InitializeComponent();
        }

        private void bunifuLabel2_Click(object sender, EventArgs e)
        {

        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string GenerateSupplierID()
        {
            var db = AppDb.Instance;
            object result = db.Scalar<object>(@"
        SELECT MAX(CAST(
            CASE 
                WHEN ISNUMERIC(SUBSTRING(SupplierID, 5, LEN(SupplierID))) = 1
                THEN SUBSTRING(SupplierID, 5, LEN(SupplierID))
                ELSE '0'
            END AS INT
        )) 
        FROM Supplier
    ");

            int next = 1;

            if (result != null && result != DBNull.Value)
                next = Convert.ToInt32(result) + 1;

            return $"SUP-{next:000}";
        }

        private string CleanName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
           //basta tanggalin lahat ng space kingina kasing bug yan
            input = input.Replace('\u00A0', ' ');
            return string.Join(" ", input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            try
            {
                string firstName = CleanName(txtFirstNme.Text);
                string middleName = CleanName(txtMiddleName.Text);
                string lastName = CleanName(txtLastName.Text);
                string contact = txtContactNumber.Text.Trim();
                string companyName = txtCompanyName.Text.Trim();
                string email = txtEmail.Text.Trim();
                string city = txtCity.Text.Trim();
                string barangay = txtBarangay.Text.Trim();
                string province = txtProvince.Text.Trim();

                if (string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(middleName) ||
                    string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(contact) ||
                    string.IsNullOrWhiteSpace(companyName) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(city) ||
                    string.IsNullOrWhiteSpace(barangay) ||
                    string.IsNullOrWhiteSpace(province))
                {
                    MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!email.ToLower().EndsWith("@gmail.com"))
                {
                    MessageBox.Show("Email must end with @gmail.com", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Generate new Supplier ID
                var db = AppDb.Instance;
                string newSupplierID = GenerateSupplierID();

                // Save to database
                bool ok = db.Save("Supplier",
                    new
                    {
                        SupplierID = newSupplierID,
                        FirstName = firstName,
                        MiddleName = middleName,
                        LastName = lastName,
                        ContactNumber = contact,
                        Email = email,
                        City = city,
                        Barangay = barangay,
                        Province = province,
                        CompanyName = companyName
                    });

                MessageBox.Show(ok ? "Successfully Saved." : "Insert failed.");

                // Trigger event for parent forms
                DataSaved?.Invoke(this, EventArgs.Empty);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void AddSupplier_Load(object sender, EventArgs e)
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
        private void txtContact_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; 
            }
        }

        private void bunifuLabel5_Click(object sender, EventArgs e)
        {

        }

        private void txtContact_TextChanged(object sender, EventArgs e)
        {

        }

        private void bunifuLabel11_Click(object sender, EventArgs e)
        {

        }
    }
}
