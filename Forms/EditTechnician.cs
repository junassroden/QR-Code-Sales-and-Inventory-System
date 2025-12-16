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

namespace barcodescanner.Forms
{
    public partial class EditTechnician : Form
    {
        private string _TechnicianID;
        public event EventHandler DataSaved;
        private string selectedImagePath = "";
        public EditTechnician(string technicianID)
        {
            InitializeComponent();
            _TechnicianID = technicianID;
        }
        public void loadData()
        {
            try
            {
                var db = AppDb.Instance;
                DataTable dt = db.TableData(
                    "SELECT FirstName, MiddleName, LastName, Specialization, Email, PhoneNumber FROM Technician WHERE TechnicianID = @id",
                    new { id = _TechnicianID } // Make sure _TechnicianID is set
                );

                if (dt.Rows.Count > 0)
                {
                    txtFirstNme.Text = dt.Rows[0]["FirstName"].ToString().Trim();
                    txtMiddleName.Text = dt.Rows[0]["MiddleName"].ToString().Trim();
                    txtLastName.Text = dt.Rows[0]["LastName"].ToString().Trim();
                    cboSpecialization.Text = dt.Rows[0]["Specialization"].ToString().Trim();
                    txtGmail.Text = dt.Rows[0]["Email"].ToString().Trim();
                    txtContactNumber.Text = dt.Rows[0]["PhoneNumber"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading technician data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void EditTechnician_Load(object sender, EventArgs e)
        {
            loadData();
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            this.Close();   
        }

        private void bttnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFirstNme.Text) &&
    !string.IsNullOrWhiteSpace(txtMiddleName.Text) &&
    !string.IsNullOrWhiteSpace(txtLastName.Text) &&
    !string.IsNullOrWhiteSpace(cboSpecialization.Text) &&
    !string.IsNullOrWhiteSpace(txtGmail.Text) &&
    !string.IsNullOrWhiteSpace(txtContactNumber.Text))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(txtContactNumber.Text.Trim(), @"^\d+$"))
                {
                    if (txtGmail.Text.Trim().EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var db = AppDb.Instance;

                            // Get image path (if any)

                            bool ok = db.Update("Technician",
                                new
                                {
                                    TechnicianID = _TechnicianID,
                                    FirstName = txtFirstNme.Text.Trim(),
                                    MiddleName = txtMiddleName.Text.Trim(),
                                    LastName = txtLastName.Text.Trim(),
                                    Specialization = cboSpecialization.Text.Trim(),
                                    Email = txtGmail.Text.Trim(),
                                    PhoneNumber = txtContactNumber.Text.Trim(), // store as string to allow long numbers
                                },
                                keyColumn: "TechnicianID"
                            );

                            MessageBox.Show(ok ? "Successfully Updated." : "Update failed.", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DataSaved?.Invoke(this, EventArgs.Empty);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error updating technician: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Email must end with @gmail.com.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Phone number must be numeric.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
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

                }
            }
        }

        private void txtFirstNme_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, Backspace, and space
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true; // Block everything else
            }
        }

        private void txtMiddleName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, Backspace, and space
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true; // Block everything else
            }
        }

        private void txtLastName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, Backspace, and space
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true; // Block everything else
            }
        }

        private void txtContactNumber_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow numbers, Backspace, and control keys
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Block everything else
            }
        }
    }
}
