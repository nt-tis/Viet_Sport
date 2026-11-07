using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ADMIN
{
    public class Admin_LeaveApplicationView : Form
    {
        private Panel panelMain;
        private ComboBox cboStatus;
        private ComboBox cboCenter;
        private Button btnFilter;
        private Button btnApprove;
        private Button btnReject;
        private DataGridView dgvLeave;
        private Label lblTotalLeaves;

        public Admin_LeaveApplicationView()
        {
            InitializeUI();
            LoadCenters();
            LoadLeaveData();
        }

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
                Text = "QUẢN LÝ VÀ PHÊ DUYỆT ĐƠN NGHỈ PHÉP TOÀN HỆ THỐNG (ADMIN)",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 116, 64),
                AutoSize = true,
                Location = new Point(25, 20)
            };
            panelMain.Controls.Add(lblTitle);

            // Filter and Action bar
            Panel filterPanel = new Panel
            {
                Location = new Point(25, 65),
                Size = new Size(1040, 48),
                BackColor = Color.FromArgb(245, 248, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelMain.Controls.Add(filterPanel);

            Label lblS = new Label { Text = "Trạng thái:", Location = new Point(15, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboStatus = new ComboBox
            {
                Location = new Point(90, 11),
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new object[] { "Tất cả", "Pending", "Approved", "Rejected" });
            cboStatus.SelectedIndex = 0;

            Label lblC = new Label { Text = "Chi nhánh:", Location = new Point(235, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboCenter = new ComboBox
            {
                Location = new Point(310, 11),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnFilter = new Button
            {
                Text = "Lọc",
                Location = new Point(545, 9),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(24, 116, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => LoadLeaveData();

            btnApprove = new Button
            {
                Text = "Duyệt Đơn",
                Location = new Point(650, 9),
                Size = new Size(115, 30),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += (s, e) => ProcessLeave("Approved");

            btnReject = new Button
            {
                Text = "Từ Chối",
                Location = new Point(780, 9),
                Size = new Size(115, 30),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += (s, e) => ProcessLeave("Rejected");

            filterPanel.Controls.Add(lblS);
            filterPanel.Controls.Add(cboStatus);
            filterPanel.Controls.Add(lblC);
            filterPanel.Controls.Add(cboCenter);
            filterPanel.Controls.Add(btnFilter);
            filterPanel.Controls.Add(btnApprove);
            filterPanel.Controls.Add(btnReject);

            // DataGridView
            dgvLeave = new DataGridView
            {
                Location = new Point(25, 125),
                Size = new Size(1040, 440),
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
            dgvLeave.EnableHeadersVisualStyles = false;
            dgvLeave.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 116, 64);
            dgvLeave.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLeave.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvLeave.ColumnHeadersHeight = 35;
            panelMain.Controls.Add(dgvLeave);

            lblTotalLeaves = new Label
            {
                Location = new Point(25, 580),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 116, 64),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            panelMain.Controls.Add(lblTotalLeaves);
        }

        private void LoadCenters()
        {
            try
            {
                DataTable dt = DatabaseConnection.ExecuteQuery("SELECT CenterID, (CenterID + ' - ' + ISNULL(Address, '')) AS CenterName FROM SPORT_CENTER ORDER BY CenterID");
                DataRow dr = dt.NewRow();
                dr["CenterID"] = "";
                dr["CenterName"] = "-- Tất cả chi nhánh --";
                dt.Rows.InsertAt(dr, 0);

                cboCenter.DisplayMember = "CenterName";
                cboCenter.ValueMember = "CenterID";
                cboCenter.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách chi nhánh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLeaveData()
        {
            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Admin_GetLeaveApplications", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string statusVal = cboStatus.SelectedIndex > 0 ? cboStatus.SelectedItem.ToString() : null;
                    cmd.Parameters.AddWithValue("@Status", (object)statusVal ?? DBNull.Value);

                    string selectedCenter = cboCenter.SelectedValue?.ToString();
                    if (!string.IsNullOrEmpty(selectedCenter))
                        cmd.Parameters.AddWithValue("@CenterID", selectedCenter);
                    else
                        cmd.Parameters.AddWithValue("@CenterID", DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvLeave.DataSource = dt;

                    if (dgvLeave.Columns.Count > 0)
                    {
                        if (dgvLeave.Columns.Contains("LeaveFormID")) dgvLeave.Columns["LeaveFormID"].HeaderText = "Mã đơn";
                        if (dgvLeave.Columns.Contains("EmployeeID")) dgvLeave.Columns["EmployeeID"].HeaderText = "Mã NV";
                        if (dgvLeave.Columns.Contains("FullName")) dgvLeave.Columns["FullName"].HeaderText = "Họ và tên";
                        if (dgvLeave.Columns.Contains("Role")) dgvLeave.Columns["Role"].HeaderText = "Vị trí";
                        if (dgvLeave.Columns.Contains("CenterID")) dgvLeave.Columns["CenterID"].HeaderText = "Chi nhánh";
                        if (dgvLeave.Columns.Contains("DateCreate"))
                        {
                            dgvLeave.Columns["DateCreate"].HeaderText = "Ngày nộp đơn";
                            dgvLeave.Columns["DateCreate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        }
                        if (dgvLeave.Columns.Contains("WorkDate"))
                        {
                            dgvLeave.Columns["WorkDate"].HeaderText = "Ngày xin nghỉ";
                            dgvLeave.Columns["WorkDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        }
                        if (dgvLeave.Columns.Contains("WorkTime")) dgvLeave.Columns["WorkTime"].HeaderText = "Giờ ca";
                        if (dgvLeave.Columns.Contains("Reason")) dgvLeave.Columns["Reason"].HeaderText = "Lý do xin nghỉ";
                        if (dgvLeave.Columns.Contains("Status"))
                        {
                            dgvLeave.Columns["Status"].HeaderText = "Trạng thái";
                            dgvLeave.Columns["Status"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        }
                    }

                    lblTotalLeaves.Text = $"Tổng số đơn xin nghỉ tìm thấy: {dt.Rows.Count} đơn";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách đơn xin nghỉ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessLeave(string newStatus)
        {
            if (dgvLeave.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn 1 đơn xin nghỉ trong danh sách để xử lý!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvLeave.SelectedRows[0];
            string formId = row.Cells["LeaveFormID"]?.Value?.ToString();
            string empName = row.Cells["FullName"]?.Value?.ToString();
            string currentStatus = row.Cells["Status"]?.Value?.ToString();

            if (string.IsNullOrEmpty(formId)) return;

            string actionName = newStatus == "Approved" ? "DUYỆT" : "TỪ CHỐI";
            DialogResult dr = MessageBox.Show($"Bạn có chắc chắn muốn {actionName} đơn nghỉ phép [{formId}] của nhân viên [{empName}]?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Admin_ApproveLeaveApplication", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LeaveFormID", formId);
                    cmd.Parameters.AddWithValue("@NewStatus", newStatus);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["Result"]) == 1)
                    {
                        MessageBox.Show($"Đã cập nhật trạng thái đơn thành [{newStatus}] thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadLeaveData();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật đơn thất bại. Vui lòng thử lại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xử lý đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
