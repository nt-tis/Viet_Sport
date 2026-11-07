using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace RECEPTIONIST
{
    public class Receptionist_Payroll : Form
    {
        private readonly string _employeeId;
        private readonly string _fullName;
        private readonly string _username;

        private Panel panelMain;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private Button btnFilter;
        private DataGridView dgvPayroll;
        private Label lblTotalSalary;

        public Receptionist_Payroll(string employeeId, string fullName, string username)
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _username = username;

            InitializeUI();
            LoadPayrollData();
        }

        public Receptionist_Payroll() : this("EMP0001", "N/A", "receptionist") { }

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
                Text = "TRA CỨU BẢNG LƯƠNG NHÂN VIÊN LỄ TÂN",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 116, 64),
                AutoSize = true,
                Location = new Point(25, 20)
            };
            panelMain.Controls.Add(lblTitle);

            Label lblEmp = new Label
            {
                Text = $"Nhân viên: {_fullName} (Mã NV: {_employeeId})",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.DarkSlateGray,
                Location = new Point(28, 55),
                AutoSize = true
            };
            panelMain.Controls.Add(lblEmp);

            // Filter panel
            Panel filterPanel = new Panel
            {
                Location = new Point(25, 85),
                Size = new Size(1030, 48),
                BackColor = Color.FromArgb(245, 248, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelMain.Controls.Add(filterPanel);

            Label lblFrom = new Label { Text = "Từ ngày:", Location = new Point(15, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpFromDate = new DateTimePicker
            {
                Location = new Point(80, 11),
                Width = 140,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            };

            Label lblTo = new Label { Text = "Đến ngày:", Location = new Point(245, 14), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpToDate = new DateTimePicker
            {
                Location = new Point(315, 11),
                Width = 140,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            btnFilter = new Button
            {
                Text = "Tra Cứu",
                Location = new Point(480, 8),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(24, 116, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => LoadPayrollData();

            filterPanel.Controls.Add(lblFrom);
            filterPanel.Controls.Add(dtpFromDate);
            filterPanel.Controls.Add(lblTo);
            filterPanel.Controls.Add(dtpToDate);
            filterPanel.Controls.Add(btnFilter);

            // DataGridView
            dgvPayroll = new DataGridView
            {
                Location = new Point(25, 145),
                Size = new Size(1030, 420),
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
                Location = new Point(25, 575),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(192, 64, 0),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            panelMain.Controls.Add(lblTotalSalary);
        }

        private void LoadPayrollData()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetPayrollByDateRange", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", _employeeId);
                    cmd.Parameters.AddWithValue("@FromDate", dtpFromDate.Value.Date);
                    cmd.Parameters.AddWithValue("@ToDate", dtpToDate.Value.Date);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPayroll.DataSource = dt;

                    if (dgvPayroll.Columns.Count > 0)
                    {
                        if (dgvPayroll.Columns.Contains("PayrollID")) dgvPayroll.Columns["PayrollID"].HeaderText = "Mã bảng lương";
                        if (dgvPayroll.Columns.Contains("Month")) dgvPayroll.Columns["Month"].Visible = false;
                        if (dgvPayroll.Columns.Contains("Year")) dgvPayroll.Columns["Year"].Visible = false;
                        if (dgvPayroll.Columns.Contains("Period")) dgvPayroll.Columns["Period"].HeaderText = "Kỳ lương";
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
                            dgvPayroll.Columns["NetSalary"].HeaderText = "Lương thực nhận";
                            dgvPayroll.Columns["NetSalary"].DefaultCellStyle.Format = "N0";
                            dgvPayroll.Columns["NetSalary"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        }
                    }

                    decimal total = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["NetSalary"] != DBNull.Value)
                        {
                            total += Convert.ToDecimal(row["NetSalary"]);
                        }
                    }

                    lblTotalSalary.Text = $"Tổng thực nhận trong kỳ: {total:N0} VNĐ ({dt.Rows.Count} phiếu lương)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin bảng lương: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
