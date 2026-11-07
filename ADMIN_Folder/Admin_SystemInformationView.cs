using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class Admin_SystemInformationView : Form
    {
        public Admin_SystemInformationView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Gọi stored procedure sp_Admin_Dashboard_ShowAllParams
                string query = "EXEC sp_Admin_Dashboard_ShowAllParams";
                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                // Xóa hết cột cũ
                dataGridView1.Columns.Clear();
                
                // Cho phép tự động tạo cột từ data
                dataGridView1.AutoGenerateColumns = true;
                
                // Bind dữ liệu
                dataGridView1.DataSource = dt;
                
                // Tùy chỉnh hiển thị cột (optional)
                if (dataGridView1.Columns.Count > 0)
                {
                    // Ẩn cột AdminID nếu không cần thiết
                    if (dataGridView1.Columns.Contains("AdminID"))
                        dataGridView1.Columns["AdminID"].Visible = false;
                    
                    // Đặt tên hiển thị thân thiện hơn
                    dataGridView1.Columns["SystemPID"].HeaderText = "System ID";
                    dataGridView1.Columns["SetupDate"].HeaderText = "Setup Date";
                    dataGridView1.Columns["SetupTime"].HeaderText = "Setup Time";
                    dataGridView1.Columns["Price"].HeaderText = "Price";
                    dataGridView1.Columns["MinBookingTime"].HeaderText = "Min Booking (min)";
                    dataGridView1.Columns["MaxBookingTime"].HeaderText = "Max Booking (min)";
                    dataGridView1.Columns["MaxCourtsPerCustomer"].HeaderText = "Max Courts/Customer";
                    
                    // Format hiển thị tiền
                    dataGridView1.Columns["Price"].DefaultCellStyle.Format = "N0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void panel5_Paint(object sender, PaintEventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void button4_Click(object sender, EventArgs e) { }
        private void button5_Click(object sender, EventArgs e) { }
        private void button9_Click(object sender, EventArgs e) { }
        private void button15_Click(object sender, EventArgs e) { }
        private void button6_Click(object sender, EventArgs e) { }
        private void button14_Click(object sender, EventArgs e) { }
        private void button17_Click(object sender, EventArgs e) { }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
    }
}
