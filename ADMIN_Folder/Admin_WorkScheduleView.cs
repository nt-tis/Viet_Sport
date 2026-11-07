using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ADMIN
{
    public class Admin_WorkScheduleView : Form
    {
        private Panel panelMain;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private ComboBox cboCenter;
        private Button btnFilter;
        private DataGridView dgvSchedule;
        private Label lblTotalShifts;

        public Admin_WorkScheduleView()
        {
            InitializeUI();
            LoadCenters();
            LoadScheduleData();
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
                Text = "QUẢN LÝ LỊCH LÀM VIỆC TOÀN HỆ THỐNG (ADMIN)",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 116, 64),
                AutoSize = true,
                Location = new Point(25, 20)
            };
            panelMain.Controls.Add(lblTitle);

            // Filter bar
            Panel filterPanel = new Panel
            {
                Location = new Point(25, 65),
                Size = new Size(1040, 48),
                BackColor = Color.FromArgb(245, 248, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelMain.Controls.Add(filterPanel);

            Label lblFrom = new Label { Text = "Từ ngày:", Location = new Point(15, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpFromDate = new DateTimePicker
            {
                Location = new Point(75, 11),
                Width = 130,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            };

            Label lblTo = new Label { Text = "Đến ngày:", Location = new Point(220, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpToDate = new DateTimePicker
            {
                Location = new Point(290, 11),
                Width = 130,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(14)
            };

            Label lblC = new Label { Text = "Chi nhánh:", Location = new Point(440, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboCenter = new ComboBox
            {
                Location = new Point(515, 11),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnFilter = new Button
            {
                Text = "Lọc Lịch Làm",
                Location = new Point(775, 9),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(24, 116, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => LoadScheduleData();

            filterPanel.Controls.Add(lblFrom);
            filterPanel.Controls.Add(dtpFromDate);
            filterPanel.Controls.Add(lblTo);
            filterPanel.Controls.Add(dtpToDate);
            filterPanel.Controls.Add(lblC);
            filterPanel.Controls.Add(cboCenter);
            filterPanel.Controls.Add(btnFilter);

            // DataGridView
            dgvSchedule = new DataGridView
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
            dgvSchedule.EnableHeadersVisualStyles = false;
            dgvSchedule.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 116, 64);
            dgvSchedule.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvSchedule.ColumnHeadersHeight = 35;
            panelMain.Controls.Add(dgvSchedule);

            lblTotalShifts = new Label
            {
                Location = new Point(25, 580),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 116, 64),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            panelMain.Controls.Add(lblTotalShifts);
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

        private void LoadScheduleData()
        {
            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Admin_GetWorkScheduleList", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", dtpFromDate.Value.Date);
                    cmd.Parameters.AddWithValue("@ToDate", dtpToDate.Value.Date);

                    string selectedCenter = cboCenter.SelectedValue?.ToString();
                    if (!string.IsNullOrEmpty(selectedCenter))
                        cmd.Parameters.AddWithValue("@CenterID", selectedCenter);
                    else
                        cmd.Parameters.AddWithValue("@CenterID", DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvSchedule.DataSource = dt;

                    if (dgvSchedule.Columns.Count > 0)
                    {
                        if (dgvSchedule.Columns.Contains("EmployeeID")) dgvSchedule.Columns["EmployeeID"].HeaderText = "Mã NV";
                        if (dgvSchedule.Columns.Contains("FullName")) dgvSchedule.Columns["FullName"].HeaderText = "Họ và tên";
                        if (dgvSchedule.Columns.Contains("Role")) dgvSchedule.Columns["Role"].HeaderText = "Vị trí";
                        if (dgvSchedule.Columns.Contains("CenterID")) dgvSchedule.Columns["CenterID"].HeaderText = "Chi nhánh";
                        if (dgvSchedule.Columns.Contains("WorkDate"))
                        {
                            dgvSchedule.Columns["WorkDate"].HeaderText = "Ngày làm việc";
                            dgvSchedule.Columns["WorkDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        }
                        if (dgvSchedule.Columns.Contains("WorkTime")) dgvSchedule.Columns["WorkTime"].HeaderText = "Giờ làm";
                        if (dgvSchedule.Columns.Contains("Shift")) dgvSchedule.Columns["Shift"].HeaderText = "Ca làm việc";
                        if (dgvSchedule.Columns.Contains("ManagerID")) dgvSchedule.Columns["ManagerID"].HeaderText = "Quản lý phụ trách";
                    }

                    lblTotalShifts.Text = $"Tổng số ca làm việc được phân công: {dt.Rows.Count} ca";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch làm việc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
