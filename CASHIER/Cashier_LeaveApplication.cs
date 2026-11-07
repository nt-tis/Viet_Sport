using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CASHIER
{
    public partial class Cashier_LeaveApplication : Form
    {
        private string connectionString = DatabaseConfig.ConnectionString;

        // Thông tin đăng nhập
        private string _employeeID;
        private string _fullName;
        private string _username;

        public Cashier_LeaveApplication()
        {
            InitializeComponent();
        }

        // Constructor nhận thông tin đăng nhập
        public Cashier_LeaveApplication(string employeeID, string fullName, string username) : this()
        {
            _employeeID = employeeID;
            _fullName = fullName;
            _username = username;
            
            // Hiển thị thông tin nhân viên
            lblEmployeeID.Text = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
            
            if (!string.IsNullOrEmpty(_fullName))
            {
                this.Text = $"Đơn Xin Nghỉ Phép - {_fullName}";
            }
            
            LoadUpcomingSchedules();
            LoadLeaveApplicationHistory();
        }

        private void LoadUpcomingSchedules()
        {
            try
            {
                // Sử dụng employeeID từ login, fallback về default nếu chưa có
                string currentEmployeeID = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
                
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetUpcomingShifts", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cboSchedule.DisplayMember = "ShiftInfo";
                    cboSchedule.ValueMember = "WorkDate";
                    cboSchedule.DataSource = dt;

                    btnSubmit.Enabled = dt.Rows.Count > 0;
                    
                    if (dt.Rows.Count == 0 && !string.IsNullOrEmpty(_fullName))
                    {
                        MessageBox.Show($"Không có ca làm việc sắp tới!\n\nNhân viên: {_fullName} ({currentEmployeeID})", 
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách ca làm việc: " + ex.Message, 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLeaveApplicationHistory()
        {
            try
            {
                // Sử dụng employeeID từ login, fallback về default nếu chưa có
                string currentEmployeeID = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
                
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetLeaveApplicationHistory", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvLeaveApplications.DataSource = dt;

                    if (dgvLeaveApplications.Columns.Count > 0)
                    {
                        dgvLeaveApplications.Columns["LeaveFormID"].HeaderText = "Mã đơn";
                        dgvLeaveApplications.Columns["ShiftInfo"].HeaderText = "Ca làm";
                        dgvLeaveApplications.Columns["Reason"].HeaderText = "Lý do";
                        dgvLeaveApplications.Columns["Status"].Visible = false;
                        dgvLeaveApplications.Columns["StatusDisplay"].HeaderText = "Trạng thái";

                        // Đếm số đơn theo trạng thái
                        int pendingCount = 0;
                        int approvedCount = 0;
                        int rejectedCount = 0;

                        foreach (DataGridViewRow row in dgvLeaveApplications.Rows)
                        {
                            string status = row.Cells["Status"].Value?.ToString();
                            switch (status)
                            {
                                case "Approved":
                                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                                    approvedCount++;
                                    break;
                                case "Rejected":
                                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                                    rejectedCount++;
                                    break;
                                case "Pending":
                                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                                    pendingCount++;
                                    break;
                            }
                        }

                        // Có thể hiển thị thống kê (tùy chỉnh theo UI)
                        if (dt.Rows.Count > 0 && !string.IsNullOrEmpty(_fullName))
                        {
                            // Thông tin thống kê có thể dùng cho label hoặc status bar
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử đơn xin nghỉ: " + ex.Message, 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (cboSchedule.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn ca làm việc cần xin nghỉ!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Vui lòng nhập lý do xin nghỉ!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReason.Focus();
                return;
            }

            try
            {
                string currentEmployeeID = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
                DataRowView selectedRow = (DataRowView)cboSchedule.SelectedItem;
                DateTime workDate = Convert.ToDateTime(selectedRow["WorkDate"]);
                TimeSpan workTime = TimeSpan.Parse(selectedRow["WorkTime"].ToString());
                string shift = selectedRow["Shift"].ToString();
                string reason = txtReason.Text.Trim();

                string leaveFormID = "LF" + DateTime.Now.ToString("yyyyMMddHHmmss");

                string confirmMessage = $"Xác nhận nộp đơn xin nghỉ phép?\n\n" +
                                       $"- Ngày: {workDate:dd/MM/yyyy}\n" +
                                       $"- Giờ: {workTime}\n" +
                                       $"- Ca: {shift}\n" +
                                       $"- Lý do: {reason}";
                
                if (!string.IsNullOrEmpty(_fullName))
                {
                    confirmMessage += $"\n- Nhân viên: {_fullName} ({currentEmployeeID})";
                }

                DialogResult confirm = MessageBox.Show(confirmMessage, "Xác nhận", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_SubmitLeaveApplication", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LeaveFormID", leaveFormID);
                    cmd.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);
                    cmd.Parameters.AddWithValue("@WorkDate", workDate);
                    cmd.Parameters.AddWithValue("@WorkTime", workTime);
                    cmd.Parameters.AddWithValue("@Reason", reason);

                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable result = new DataTable();
                    adapter.Fill(result);

                    if (result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0]["Success"]) == 1)
                    {
                        string successMsg = $"Đã nộp đơn xin nghỉ phép thành công!\n\n" +
                                          $"- Mã đơn: {leaveFormID}\n" +
                                          $"- Trạng thái: Chờ duyệt\n";
                        
                        if (!string.IsNullOrEmpty(_fullName))
                        {
                            successMsg += $"- Nhân viên: {_fullName}\n";
                        }
                        
                        successMsg += $"\nĐơn sẽ được quản lý xem xét.";
                        
                        MessageBox.Show(successMsg, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                        LoadUpcomingSchedules();
                        LoadLeaveApplicationHistory();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nộp đơn xin nghỉ:\n" + ex.Message, 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUpcomingSchedules();
            LoadLeaveApplicationHistory();
            
            string refreshMsg = "Đã làm mới dữ liệu!";
            if (!string.IsNullOrEmpty(_fullName))
            {
                refreshMsg += $"\nNhân viên: {_fullName}";
                
                // Thống kê nhanh
                int totalApplications = dgvLeaveApplications.Rows.Count;
                int pendingCount = 0;
                foreach (DataGridViewRow row in dgvLeaveApplications.Rows)
                {
                    if (row.Cells["Status"].Value?.ToString() == "Pending")
                        pendingCount++;
                }
                
                if (totalApplications > 0)
                {
                    refreshMsg += $"\n- Tổng đơn: {totalApplications}";
                    refreshMsg += $"\n- Đang chờ duyệt: {pendingCount}";
                }
            }
            
            MessageBox.Show(refreshMsg, "Thông báo", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearForm()
        {
            if (cboSchedule.Items.Count > 0)
            {
                cboSchedule.SelectedIndex = 0;
            }
            txtReason.Clear();
            txtReason.Focus();
        }

        private void panelTop_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
