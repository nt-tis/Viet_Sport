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
    public partial class Cashier_WorkSchedule : Form
    {
        private string connectionString = DatabaseConfig.ConnectionString;

        // Thông tin đăng nhập
        private string _employeeID;
        private string _fullName;
        private string _username;

        public Cashier_WorkSchedule()
        {
            InitializeComponent();
            
            // Set default date range (current month to next month)
            dtpFromDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpToDate.Value = DateTime.Now.AddDays(30);
        }

        // Constructor nhận thông tin đăng nhập
        public Cashier_WorkSchedule(string employeeID, string fullName, string username) : this()
        {
            _employeeID = employeeID;
            _fullName = fullName;
            _username = username;
            
            LoadWorkSchedule();
        }

        private void LoadWorkSchedule()
        {
            try
            {
                // Sử dụng employeeID từ login, fallback về default nếu chưa có
                string currentEmployeeID = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);

                if (fromDate > toDate)
                {
                    MessageBox.Show("Ngày bắt đầu không thể lớn hơn ngày kết thúc!", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetWorkSchedule", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvSchedule.DataSource = dt;

                    if (dgvSchedule.Columns.Count > 0)
                    {
                        dgvSchedule.Columns["WorkDate"].HeaderText = "Ngày làm việc";
                        dgvSchedule.Columns["WorkDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        dgvSchedule.Columns["WorkTime"].HeaderText = "Giờ làm việc";
                        dgvSchedule.Columns["Shift"].HeaderText = "Ca làm";
                    }

                    // Hiển thị thông tin chi tiết
                    string scheduleInfo = $"Lịch làm việc ({fromDate:dd/MM/yyyy} - {toDate.Date:dd/MM/yyyy})";
                    if (!string.IsNullOrEmpty(_fullName))
                    {
                        scheduleInfo += $"\nNhân viên: {_fullName} ({currentEmployeeID})";
                    }
                    
                    label1.Text = scheduleInfo;

                    // Thống kê số ca làm việc
                    if (dt.Rows.Count == 0)
                    {
                        string message = $"Không có lịch làm việc trong khoảng thời gian này!";
                        if (!string.IsNullOrEmpty(_fullName))
                        {
                            message += $"\n\nNhân viên: {_fullName} ({currentEmployeeID})";
                        }
                        MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Đếm số ca theo loại ca
                        int morningShifts = 0;
                        int afternoonShifts = 0;
                        int eveningShifts = 0;
                        int fullDayShifts = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            string shift = row["Shift"].ToString();
                            switch (shift)
                            {
                                case "Sáng":
                                    morningShifts++;
                                    break;
                                case "Chiều":
                                    afternoonShifts++;
                                    break;
                                case "Tối":
                                    eveningShifts++;
                                    break;
                                case "Full Day":
                                    fullDayShifts++;
                                    break;
                            }
                        }

                        this.Text = $"Lịch Làm Việc - {_fullName ?? "Cashier"} ({dt.Rows.Count} ca)";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch làm việc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadWorkSchedule();
            
            // Hiển thị thông tin chi tiết khi tìm kiếm
            if (dgvSchedule.Rows.Count > 0)
            {
                int morningShifts = 0;
                int afternoonShifts = 0;
                int eveningShifts = 0;
                int fullDayShifts = 0;

                foreach (DataGridViewRow row in dgvSchedule.Rows)
                {
                    if (row.Cells["Shift"].Value != null)
                    {
                        string shift = row.Cells["Shift"].Value.ToString();
                        switch (shift)
                        {
                            case "Sáng":
                                morningShifts++;
                                break;
                            case "Chiều":
                                afternoonShifts++;
                                break;
                            case "Tối":
                                eveningShifts++;
                                break;
                            case "Full Day":
                                fullDayShifts++;
                                break;
                        }
                    }
                }

                string searchInfo = $"Tìm kiếm thành công!\n\n" +
                                  $"- Khoảng thời gian: {dtpFromDate.Value:dd/MM/yyyy} - {dtpToDate.Value:dd/MM/yyyy}\n" +
                                  $"- Tổng số ca: {dgvSchedule.Rows.Count}\n";
                
                if (morningShifts > 0) searchInfo += $"- Ca sáng: {morningShifts}\n";
                if (afternoonShifts > 0) searchInfo += $"- Ca chiều: {afternoonShifts}\n";
                if (eveningShifts > 0) searchInfo += $"- Ca tối: {eveningShifts}\n";
                if (fullDayShifts > 0) searchInfo += $"- Ca full day: {fullDayShifts}\n";
                
                if (!string.IsNullOrEmpty(_fullName))
                {
                    searchInfo += $"\n- Nhân viên: {_fullName} ({_employeeID})";
                }

                MessageBox.Show(searchInfo, "Kết quả tìm kiếm", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnThisMonth_Click(object sender, EventArgs e)
        {
            dtpFromDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpToDate.Value = DateTime.Now;
            LoadWorkSchedule();
            
            if (!string.IsNullOrEmpty(_fullName))
            {
                MessageBox.Show($"Đã tải lịch làm việc tháng này\nNhân viên: {_fullName}", 
                    "Tháng hiện tại", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dgvSchedule_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
