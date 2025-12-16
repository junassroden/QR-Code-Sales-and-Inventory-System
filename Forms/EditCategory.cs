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
    public partial class EditCategory : Form
    {
        private string _CategoryID;
        public event EventHandler DataSaved;
        public EditCategory(string categoryID)
        {
            InitializeComponent();
            _CategoryID = categoryID;   
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void loadData()
        {
            try
            {
                var db = AppDb.Instance;
                DataTable dt = db.TableData(
                    "SELECT CategoryName FROM Category WHERE Category_ID = @id",
                    new { id = _CategoryID }
                );

                if (dt.Rows.Count > 0)
                {
                    txtPN.Text = dt.Rows[0]["CategoryName"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading supplier data: " + ex.Message);
            }
        }

        private void EditCategory_Load(object sender, EventArgs e)
        {
            loadData();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var db = AppDb.Instance;
                var updateOk = db.Update(
                    "Category",
                    new
                    {
                        Category_ID = _CategoryID,
                        CategoryName = txtPN.Text
                    },
                    keyColumn: "Category_ID"
                );

                MessageBox.Show(updateOk ? "Category updated successfully!" : "Update failed.",
                                "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DataSaved?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating Category: " + ex.Message);
            }
        }

        private void txtPN_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
