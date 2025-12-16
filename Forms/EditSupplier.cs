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
    public partial class EditSupplier : Form
    {
        private string _SupplierID;
        public event EventHandler DataSaved;
        public EditSupplier(string SupplierID)
        {
            InitializeComponent();
            _SupplierID = SupplierID;
        }

        private void EditSupplier_Load(object sender, EventArgs e)
        {
            loadData();
        }
        private void loadData()
        {
            try
            {
                var db = AppDb.Instance;
                DataTable dt = db.TableData(
                    @"SELECT ContactNumber, FirstName, MiddleName, LastName,
                             Email, City, Barangay, Province, CompanyName
                      FROM Supplier
                      WHERE SupplierID = @id",
                    new { id = _SupplierID }
                );

                if (dt.Rows.Count > 0)
                {
                    txtContactNumber.Text = dt.Rows[0]["ContactNumber"].ToString();
                    txtFirstNme.Text = dt.Rows[0]["FirstName"].ToString();
                    txtMiddleName.Text = dt.Rows[0]["MiddleName"].ToString();
                    txtLastName.Text = dt.Rows[0]["LastName"].ToString();
                    txtEmail.Text = dt.Rows[0]["Email"].ToString();
                    txtCity.Text = dt.Rows[0]["City"].ToString();
                    txtBarangay.Text = dt.Rows[0]["Barangay"].ToString();
                    txtProvince.Text = dt.Rows[0]["Province"].ToString();
                    txtCompanyName.Text = dt.Rows[0]["CompanyName"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading supplier data: " + ex.Message);
            }
        }
        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            try
            {
                // Required fields validation
                if (string.IsNullOrWhiteSpace(txtFirstNme.Text) ||
                    string.IsNullOrWhiteSpace(txtMiddleName.Text) ||
                    string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtContactNumber.Text) ||
                    string.IsNullOrWhiteSpace(txtCompanyName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtCity.Text) ||
                    string.IsNullOrWhiteSpace(txtBarangay.Text) ||
                    string.IsNullOrWhiteSpace(txtProvince.Text))
                {
                    MessageBox.Show("All fields are required.", "Validation Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!txtEmail.Text.Trim().ToLower().EndsWith("@gmail.com"))
                {
                    MessageBox.Show("Email must end with @gmail.com", "Invalid Email",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var db = AppDb.Instance;

                bool updateOk = db.Update(
                    "Supplier",
                    new
                    {
                        SupplierID = _SupplierID,
                        FirstName = txtFirstNme.Text.Trim(),
                        MiddleName = txtMiddleName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        ContactNumber = txtContactNumber.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        City = txtCity.Text.Trim(),
                        Barangay = txtBarangay.Text.Trim(),
                        Province = txtProvince.Text.Trim(),
                        CompanyName = txtCompanyName.Text.Trim()
                    },
                    keyColumn: "SupplierID"
                );

                MessageBox.Show(updateOk ? "Supplier updated successfully!"
                                         : "Update failed.",
                                "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DataSaved?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating supplier: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
