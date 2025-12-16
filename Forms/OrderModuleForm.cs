using barcodescanner.Classes;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace barcodescanner.Forms
{
    public partial class OrderModuleForm : Form
    {
        public OrderModuleForm()
        {
            InitializeComponent();
            pnlGraph.Paint += pnlGraph_Paint;
        }

        private void OrderModuleForm_Load(object sender, EventArgs e)
        {
            LoadSalesGraph();
            LoadDashboardStats();
        }

        private void LoadDashboardStats()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Customer", conn))
                    {
                        int totalCustomers = (int)cmd.ExecuteScalar();
                        lblTCustomers.Text = totalCustomers.ToString();
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SalesTransaction", conn))
                    {
                        int totalOrders = (int)cmd.ExecuteScalar();
                        lblTOrders.Text = totalOrders.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard stats: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Sales
        private void LoadSalesGraph()
        {
            pnlGraph.Controls.Clear();

            Chart salesChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(10)
            };

            // Chart area
            ChartArea chartArea = new ChartArea
            {
                Name = "SalesChartArea",
                BackColor = Color.WhiteSmoke
            };

            chartArea.AxisX.Title = "Date";
            chartArea.AxisX.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chartArea.AxisX.LabelStyle.Format = "MM/dd/yyyy";
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;

            chartArea.AxisY.Title = "Total Sales (₱)";
            chartArea.AxisY.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chartArea.AxisY.LabelStyle.Format = "₱#,0.00";
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            salesChart.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                Name = "Sales",
                ChartType = SeriesChartType.Column,
                XValueType = ChartValueType.Date,
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Color = Color.DarkBlue,
                LabelForeColor = Color.Black,
                LabelFormat = "₱#,0.00",
                BorderColor = Color.FromArgb(30, 30, 120),
                BorderWidth = 1
            };
            salesChart.Series.Add(series);

            Legend legend = new Legend
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.DarkBlue
            };
            salesChart.Legends.Add(legend);

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;

            // Get current month and year
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;

            string query = @"
        SELECT DateSold, SUM(TotalPrice) AS TotalSales
        FROM SalesTransactionHistory
        WHERE MONTH(DateSold) = @Month AND YEAR(DateSold) = @Year
        GROUP BY DateSold
        ORDER BY DateSold
    ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Month", currentMonth);
                cmd.Parameters.AddWithValue("@Year", currentYear);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime date = reader.GetDateTime(0);
                        decimal totalSales = reader.GetDecimal(1);
                        series.Points.AddXY(date, totalSales);
                    }
                }
            }

            foreach (DataPoint point in series.Points)
            {
                point.ToolTip = $"Date: {DateTime.FromOADate(point.XValue):MM/dd/yyyy}\nSales: ₱{point.YValues[0]:N2}";
            }

            Title title = new Title
            {
                Text = $"Daily Sales Summary - {DateTime.Now:MMMM yyyy}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Docking = Docking.Top
            };
            salesChart.Titles.Add(title);

            pnlGraph.Controls.Add(salesChart);
        }


        //Pabilog na UI
        private void pnlGraph_Paint(object sender, PaintEventArgs e)
        {
            if (pnlGraph.Width < 20 || pnlGraph.Height < 20) return;

            int radius = 15;
            int thickness = 2;

            using (Graphics g = e.Graphics)
            using (Pen pen = new Pen(Color.DarkBlue, thickness))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(
                    thickness / 2,
                    thickness / 2,
                    pnlGraph.Width - thickness,
                    pnlGraph.Height - thickness
                );

                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int d = radius * 2;
                    path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                    path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();

                    g.DrawPath(pen, path); // basta bilog na broder ari
                }
            }
        }

        //Other function, wala na ari hahah
        private void bunifuTextBox1_TextChanged(object sender, EventArgs e) { }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            DialogResult confirmLogout = MessageBox.Show(
                "Are you sure you want to go back to Home Page?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmLogout == DialogResult.Yes)
            {
                Home home = new Home();
                home.Show();
                this.Close();
            }
        }

        private async void btnCustomer_Click(object sender, EventArgs e)
        {
            Customers dash = new Customers
            {
                StartPosition = FormStartPosition.Manual,
                Location = this.Location
            };
            dash.Show();
            await Task.Delay(200);
            this.Close();
        }

        private void btnDashboard_Click(object sender, EventArgs e) { }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult confirmLogout = MessageBox.Show(
                "Are you sure you want to go home?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmLogout == DialogResult.Yes)
            {
                Home home = new Home();
                home.Show();
                this.Close();
            }
        }

        private async void btnOrder_Click(object sender, EventArgs e)
        {
            Order dash = new Order
            {
                StartPosition = FormStartPosition.Manual,
                Location = this.Location
            };
            dash.Show();
            await Task.Delay(200);
            this.Close();
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            History dash = new History
            {
                StartPosition = FormStartPosition.Manual,
                Location = this.Location
            };
            dash.Show();
            this.Close();
        }

        private void pnlGraph_Click(object sender, EventArgs e) { }

        private void pnlGraph_ControlAdded(object sender, ControlEventArgs e)
        {

        }
    }
}
