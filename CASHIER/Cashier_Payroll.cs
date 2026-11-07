using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CASHIER
{
    public partial class Cashier_Payroll : Form
    {
        private string connectionString = DatabaseConfig.ConnectionString;

        // Thông tin đăng nhập
        private string _employeeID;
        private string _fullName;
        private string _username;

        public Cashier_Payroll()
        {
            InitializeComponent();
            InitializeDatePickers();
        }

        // Constructor nhận thông tin đăng nhập
        public Cashier_Payroll(string employeeID, string fullName, string username) : this()
        {
            _employeeID = employeeID;
            _fullName = fullName;
            _username = username;
            
            LoadPayrollData();
        }

        private void InitializeDatePickers()
        {
            dtpFromDate.Value = new DateTime(DateTime.Now.Year, 1, 1);
            dtpToDate.Value = DateTime.Now;
        }

        private void LoadPayrollData()
        {
            try
            {
                // Sử dụng employeeID từ login, fallback về default nếu chưa có
                string currentEmployeeID = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetPayrollByDateRange", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPayroll.DataSource = dt;

                    if (dgvPayroll.Columns.Count > 0)
                    {
                        dgvPayroll.Columns["PayrollID"].HeaderText = "Mã bảng lương";
                        dgvPayroll.Columns["Month"].Visible = false;
                        dgvPayroll.Columns["Year"].Visible = false;
                        dgvPayroll.Columns["Period"].HeaderText = "Kỳ lương";
                        dgvPayroll.Columns["BasicSalary"].HeaderText = "Lương cơ bản";
                        dgvPayroll.Columns["BasicSalary"].DefaultCellStyle.Format = "N0";
                        dgvPayroll.Columns["Allowance"].HeaderText = "Phụ cấp";
                        dgvPayroll.Columns["Allowance"].DefaultCellStyle.Format = "N0";
                        dgvPayroll.Columns["ShiftPay"].HeaderText = "Lương ca";
                        dgvPayroll.Columns["ShiftPay"].DefaultCellStyle.Format = "N0";
                        dgvPayroll.Columns["SaleCommission"].HeaderText = "Hoa hồng";
                        dgvPayroll.Columns["SaleCommission"].DefaultCellStyle.Format = "N0";
                        dgvPayroll.Columns["SaleCommission"].DefaultCellStyle.ForeColor = Color.Green;
                        dgvPayroll.Columns["PenaltyFee"].HeaderText = "Phạt";
                        dgvPayroll.Columns["PenaltyFee"].DefaultCellStyle.Format = "N0";
                        dgvPayroll.Columns["PenaltyFee"].DefaultCellStyle.ForeColor = Color.Red;
                        dgvPayroll.Columns["NetSalary"].HeaderText = "Lương thực nhận";
                        dgvPayroll.Columns["NetSalary"].DefaultCellStyle.Format = "N0";
                        dgvPayroll.Columns["NetSalary"].DefaultCellStyle.Font = new Font(dgvPayroll.Font, FontStyle.Bold);
                        dgvPayroll.Columns["NetSalary"].DefaultCellStyle.ForeColor = Color.Blue;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        string message = $"Không tìm thấy dữ liệu lương từ {fromDate:MM/yyyy} đến {toDate:MM/yyyy}!";

                        MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Tính tổng lương trong khoảng thời gian
                        decimal totalNetSalary = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            totalNetSalary += Convert.ToDecimal(row["NetSalary"]);
                        }

                        string summaryMessage = $"Tìm thấy {dt.Rows.Count} kỳ lương\n" +
                                              $"Tổng thu nhập: {totalNetSalary:N0} VNĐ";
                        
                        if (!string.IsNullOrEmpty(_fullName))
                        {
                            summaryMessage += $"\n\nNhân viên: {_fullName} ({currentEmployeeID})";
                        }

                        // Hiển thị thông tin tổng hợp (có thể tùy chỉnh theo UI)
                        this.Text = $"Bảng Lương - {_fullName ?? "Cashier"}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu lương: " + ex.Message, 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (dtpFromDate.Value > dtpToDate.Value)
            {
                MessageBox.Show("Ngày bắt đầu không thể lớn hơn ngày kết thúc!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadPayrollData();
            
            // Hiển thị thông báo tìm kiếm thành công với thông tin nhân viên
            if (dgvPayroll.Rows.Count > 0)
            {
                decimal totalNetSalary = 0;
                foreach (DataGridViewRow row in dgvPayroll.Rows)
                {
                    if (row.Cells["NetSalary"].Value != null)
                    {
                        totalNetSalary += Convert.ToDecimal(row.Cells["NetSalary"].Value);
                    }
                }

                string searchInfo = $"Tìm kiếm thành công!\n\n" +
                                  $"- Kỳ lương: {dtpFromDate.Value:MM/yyyy} - {dtpToDate.Value:MM/yyyy}\n" +
                                  $"- Số bảng lương: {dgvPayroll.Rows.Count}\n" +
                                  $"- Tổng thu nhập: {totalNetSalary:N0} VNĐ";
                
                if (!string.IsNullOrEmpty(_fullName))
                {
                    searchInfo += $"\n- Nhân viên: {_fullName} ({_employeeID})";
                }

                MessageBox.Show(searchInfo, "Kết quả tìm kiếm", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
