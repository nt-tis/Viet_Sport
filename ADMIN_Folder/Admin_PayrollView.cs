using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ADMIN
{
    public class Admin_PayrollView : Form
    {
        private Panel panelMain;
        private ComboBox cboMonth;
        private NumericUpDown numYear;
        private ComboBox cboCenter;
        private Button btnFilter;
        private DataGridView dgvPayroll;
        private Label lblTotalSalary;

        public Admin_PayrollView()
        {
            InitializeUI();
            LoadCenters();
            LoadPayrollData();
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
                Text = "QUẢN LÝ BẢNG LƯƠNG TOÀN HỆ THỐNG (ADMIN)",
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

            Label lblM = new Label { Text = "Tháng:", Location = new Point(15, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboMonth = new ComboBox
            {
                Location = new Point(65, 11),
                Width = 90,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboMonth.Items.Add("Tất cả");
            for (int i = 1; i <= 12; i++) cboMonth.Items.Add(i.ToString());
            cboMonth.SelectedIndex = 0;

            Label lblY = new Label { Text = "Năm:", Location = new Point(175, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numYear = new NumericUpDown
            {
                Location = new Point(215, 12),
                Width = 80,
                Minimum = 2020,
                Maximum = 2030,
                Value = DateTime.Now.Year
            };

            Label lblC = new Label { Text = "Chi nhánh:", Location = new Point(315, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboCenter = new ComboBox
            {
                Location = new Point(385, 11),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnFilter = new Button
            {
                Text = "Lọc Dữ Liệu",
                Location = new Point(645, 9),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(24, 116, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => LoadPayrollData();

            filterPanel.Controls.Add(lblM);
            filterPanel.Controls.Add(cboMonth);
            filterPanel.Controls.Add(lblY);
            filterPanel.Controls.Add(numYear);
            filterPanel.Controls.Add(lblC);
            filterPanel.Controls.Add(cboCenter);
            filterPanel.Controls.Add(btnFilter);

            // DataGridView
            dgvPayroll = new DataGridView
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
            dgvPayroll.EnableHeadersVisualStyles = false;
            dgvPayroll.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 116, 64);
            dgvPayroll.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPayroll.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvPayroll.ColumnHeadersHeight = 35;
            panelMain.Controls.Add(dgvPayroll);

            lblTotalSalary = new Label
            {
                Location = new Point(25, 580),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(192, 64, 0),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            panelMain.Controls.Add(lblTotalSalary);
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

        private void LoadPayrollData()
        {
            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Admin_GetPayrollList", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (cboMonth.SelectedIndex > 0)
                        cmd.Parameters.AddWithValue("@Month", Convert.ToInt32(cboMonth.SelectedItem));
                    else
                        cmd.Parameters.AddWithValue("@Month", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Year", Convert.ToInt32(numYear.Value));

                    string selectedCenter = cboCenter.SelectedValue?.ToString();
                    if (!string.IsNullOrEmpty(selectedCenter))
                        cmd.Parameters.AddWithValue("@CenterID", selectedCenter);
                    else
                        cmd.Parameters.AddWithValue("@CenterID", DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPayroll.DataSource = dt;

                    if (dgvPayroll.Columns.Count > 0)
                    {
                        if (dgvPayroll.Columns.Contains("PayrollID")) dgvPayroll.Columns["PayrollID"].HeaderText = "Mã bảng lương";
                        if (dgvPayroll.Columns.Contains("EmployeeID")) dgvPayroll.Columns["EmployeeID"].HeaderText = "Mã NV";
                        if (dgvPayroll.Columns.Contains("FullName")) dgvPayroll.Columns["FullName"].HeaderText = "Họ và tên";
                        if (dgvPayroll.Columns.Contains("Role")) dgvPayroll.Columns["Role"].HeaderText = "Vị trí";
                        if (dgvPayroll.Columns.Contains("CenterID")) dgvPayroll.Columns["CenterID"].HeaderText = "Chi nhánh";
                        if (dgvPayroll.Columns.Contains("Month")) dgvPayroll.Columns["Month"].HeaderText = "Tháng";
                        if (dgvPayroll.Columns.Contains("Year")) dgvPayroll.Columns["Year"].HeaderText = "Năm";
                        if (dgvPayroll.Columns.Contains("BasicSalary"))
                        {
                            dgvPayroll.Columns["BasicSalary"].HeaderText = "Lương cơ bản";
                            dgvPayroll.Columns["BasicSalary"].DefaultCellStyle.Format = "N0";
                        }
                        if (dgvPayroll.Columns.Contains("Allowance"))
                        {
                            dgvPayroll.Columns["Allowance"].HeaderText = "Phụ cấp";
                            dgvPayroll.Columns["Allowance"].DefaultCellStyle.Format = "N0";
                        }
                        if (dgvPayroll.Columns.Contains("ShiftPay"))
                        {
                            dgvPayroll.Columns["ShiftPay"].HeaderText = "Lương ca";
                            dgvPayroll.Columns["ShiftPay"].DefaultCellStyle.Format = "N0";
                        }
                        if (dgvPayroll.Columns.Contains("SaleCommission"))
                        {
                            dgvPayroll.Columns["SaleCommission"].HeaderText = "Hoa hồng";
                            dgvPayroll.Columns["SaleCommission"].DefaultCellStyle.Format = "N0";
                        }
                        if (dgvPayroll.Columns.Contains("PenaltyFee"))
                        {
                            dgvPayroll.Columns["PenaltyFee"].HeaderText = "Phạt";
                            dgvPayroll.Columns["PenaltyFee"].DefaultCellStyle.Format = "N0";
                        }
                        if (dgvPayroll.Columns.Contains("NetSalary"))
                        {
                            dgvPayroll.Columns["NetSalary"].HeaderText = "Thực nhận";
                            dgvPayroll.Columns["NetSalary"].DefaultCellStyle.Format = "N0";
                            dgvPayroll.Columns["NetSalary"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        }
                    }

                    decimal total = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["NetSalary"] != DBNull.Value) total += Convert.ToDecimal(row["NetSalary"]);
                    }

                    lblTotalSalary.Text = $"Tổng quỹ lương chi trả: {total:N0} VNĐ (Số bản ghi: {dt.Rows.Count})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin bảng lương: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
