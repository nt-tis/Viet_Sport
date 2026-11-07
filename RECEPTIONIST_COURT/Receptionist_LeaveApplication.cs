using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace RECEPTIONIST
{
    public class Receptionist_LeaveApplication : Form
    {
        private readonly string _employeeId;
        private readonly string _fullName;
        private readonly string _username;

        private Panel panelMain;
        private ComboBox cboSchedule;
        private TextBox txtReason;
        private Button btnSubmit;
        private DataGridView dgvHistory;

        public Receptionist_LeaveApplication(string employeeId, string fullName, string username)
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _username = username;

            InitializeUI();
            LoadUpcomingSchedules();
            LoadLeaveHistory();
        }

        public Receptionist_LeaveApplication() : this("EMP0001", "N/A", "receptionist") { }

        private void InitializeUI()
        {
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Size = new Size(1100, 650);

            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(25)
            };
            this.Controls.Add(panelMain);

            Label lblTitle = new Label
            {
                Text = "ĐƠN XIN NGHỈ PHÉP NHÂN VIÊN LỄ TÂN",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 116, 64),
                AutoSize = true,
                Location = new Point(25, 20)
            };
            panelMain.Controls.Add(lblTitle);

            // Left panel for submitting application
            GroupBox gbSubmit = new GroupBox
            {
                Text = "Nộp Đơn Xin Nghỉ Mới",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(25, 65),
                Size = new Size(420, 520),
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            panelMain.Controls.Add(gbSubmit);

            Label lblEmp = new Label
            {
                Text = $"Nhân viên: {_fullName} ({_employeeId})",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.DarkSlateGray,
                Location = new Point(20, 30),
                AutoSize = true
            };
            gbSubmit.Controls.Add(lblEmp);

            Label lblShift = new Label { Text = "Chọn ca làm việc muốn nghỉ:", Location = new Point(20, 65), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            cboSchedule = new ComboBox
            {
                Location = new Point(20, 88),
                Width = 375,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F)
            };
            gbSubmit.Controls.Add(lblShift);
            gbSubmit.Controls.Add(cboSchedule);

            Label lblReason = new Label { Text = "Lý do xin nghỉ:", Location = new Point(20, 130), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            txtReason = new TextBox
            {
                Location = new Point(20, 153),
                Width = 375,
                Height = 120,
                Multiline = true,
                Font = new Font("Segoe UI", 9.5F)
            };
            gbSubmit.Controls.Add(lblReason);
            gbSubmit.Controls.Add(txtReason);

            btnSubmit = new Button
            {
                Text = "Gửi Đơn Xin Nghỉ",
                Location = new Point(20, 290),
                Size = new Size(375, 42),
                BackColor = Color.FromArgb(24, 116, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += BtnSubmit_Click;
            gbSubmit.Controls.Add(btnSubmit);

            // Right panel for application history
            GroupBox gbHistory = new GroupBox
            {
                Text = "Lịch Sử Đơn Xin Nghỉ Phép",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(460, 65),
                Size = new Size(595, 520),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            panelMain.Controls.Add(gbHistory);

            dgvHistory = new DataGridView
            {
                Location = new Point(15, 30),
                Size = new Size(565, 470),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F)
            };
            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 116, 64);
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvHistory.ColumnHeadersHeight = 35;
            gbHistory.Controls.Add(dgvHistory);
        }

        private void LoadUpcomingSchedules()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetUpcomingShifts", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", _employeeId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cboSchedule.DisplayMember = "ShiftInfo";
                    cboSchedule.ValueMember = "WorkDate";
                    cboSchedule.DataSource = dt;

                    btnSubmit.Enabled = dt.Rows.Count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải ca làm việc sắp tới: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLeaveHistory()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetLeaveApplications", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", _employeeId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvHistory.DataSource = dt;

                    if (dgvHistory.Columns.Count > 0)
                    {
                        if (dgvHistory.Columns.Contains("LeaveFormID")) dgvHistory.Columns["LeaveFormID"].HeaderText = "Mã đơn";
                        if (dgvHistory.Columns.Contains("ShiftInfo")) dgvHistory.Columns["ShiftInfo"].HeaderText = "Ca làm";
                        if (dgvHistory.Columns.Contains("WorkDate")) dgvHistory.Columns["WorkDate"].Visible = false;
                        if (dgvHistory.Columns.Contains("WorkTime")) dgvHistory.Columns["WorkTime"].Visible = false;
                        if (dgvHistory.Columns.Contains("Reason")) dgvHistory.Columns["Reason"].HeaderText = "Lý do";
                        if (dgvHistory.Columns.Contains("Status")) dgvHistory.Columns["Status"].Visible = false;
                        if (dgvHistory.Columns.Contains("StatusDisplay"))
                        {
                            dgvHistory.Columns["StatusDisplay"].HeaderText = "Trạng thái";
                            dgvHistory.Columns["StatusDisplay"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        }
                        if (dgvHistory.Columns.Contains("SubmitTime"))
                        {
                            dgvHistory.Columns["SubmitTime"].HeaderText = "Ngày nộp";
                            dgvHistory.Columns["SubmitTime"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        }

                        foreach (DataGridViewRow row in dgvHistory.Rows)
                        {
                            string status = row.Cells["Status"]?.Value?.ToString();
                            if (status == "Approved") row.DefaultCellStyle.BackColor = Color.LightGreen;
                            else if (status == "Rejected") row.DefaultCellStyle.BackColor = Color.LightCoral;
                            else row.DefaultCellStyle.BackColor = Color.LightYellow;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử đơn xin nghỉ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            DataRowView selectedRow = cboSchedule.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Vui lòng chọn ca làm việc muốn xin nghỉ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = txtReason.Text.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Vui lòng nhập lý do xin nghỉ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReason.Focus();
                return;
            }

            DateTime workDate = Convert.ToDateTime(selectedRow["WorkDate"]);
            TimeSpan workTime = TimeSpan.Parse(selectedRow["WorkTime"].ToString());
            string shift = selectedRow["Shift"].ToString();
            string leaveFormID = "LF" + DateTime.Now.ToString("yyyyMMddHHmmss");

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_SubmitLeaveApplication", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LeaveFormID", leaveFormID);
                    cmd.Parameters.AddWithValue("@EmployeeID", _employeeId);
                    cmd.Parameters.AddWithValue("@WorkDate", workDate);
                    cmd.Parameters.AddWithValue("@WorkTime", workTime);
                    cmd.Parameters.AddWithValue("@Reason", reason);

                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0 && (dt.Columns.Contains("Success") && Convert.ToInt32(dt.Rows[0]["Success"]) == 1 || dt.Columns.Contains("Result") && Convert.ToInt32(dt.Rows[0]["Result"]) == 1))
                    {
                        MessageBox.Show("Đã nộp đơn xin nghỉ phép thành công! Đang chờ quản lý duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtReason.Clear();
                        LoadLeaveHistory();
                        LoadUpcomingSchedules();
                    }
                    else
                    {
                        string msg = dt.Rows.Count > 0 && dt.Columns.Contains("Message") ? dt.Rows[0]["Message"]?.ToString() : "Nộp đơn thất bại.";
                        MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi đơn xin nghỉ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
