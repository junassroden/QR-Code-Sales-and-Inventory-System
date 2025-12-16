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
    public partial class AddCategory : Form
    {
        private int _selectedId = 0;
        public event EventHandler DataSaved;
        public AddCategory()
        {
            InitializeComponent();
        }

        private void AddCategory_Load(object sender, EventArgs e)
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
                WHEN ISNUMERIC(SUBSTRING(Category_ID, 5, LEN(Category_ID))) = 1
                THEN SUBSTRING(Category_ID, 5, LEN(Category_ID))
                ELSE '0'
            END AS INT)
        ) 
        FROM Category
    ");
            int next = 1;

            if (result != null && result != DBNull.Value)
                next = Convert.ToInt32(result) + 1;

            return $"CAT-{next:000}";
        }

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            var db = AppDb.Instance;
            string CategoryID = GenerateSupplierID();
            var ok = db.Save("Category",
                new
                {
                    Category_ID = CategoryID,
                    CategoryName = txtPN.Text,
                });

            MessageBox.Show(ok ? "Successfully Saved." : "Insert failed.");
            DataSaved?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        private void txtPN_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Cancel the input
            }
        }

        private void txtPN_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
