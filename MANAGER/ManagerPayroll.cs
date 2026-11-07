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

namespace MANAGER
{
    public partial class ManagerPayroll : Form
    {
        private static string _connStr =
             "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        private readonly ManagerMainFrom _main;
        private string _managerEmployeeID; // lấy từ MainForm (EmployeeID label)
        private string _centerID;

        private readonly DataTable _dt = new DataTable();

        // ====== ASSUME CONTROL NAME (đổi đúng tên nếu bạn đặt khác) ======
        // ComboBox: cboMonth, cboYear
        // DataGridView: dgvPayroll
        // Button: btnSearch

        public ManagerPayroll(ManagerMainFrom main)
        {
            InitializeComponent();
            _main = main;
            this.Load += ManagerPayroll_Load;
        }

        public ManagerPayroll()
        {
            InitializeComponent();
            this.Load += ManagerPayroll_Load;
        }

        private void ManagerPayroll_Load(object sender, EventArgs e)
        {
            SetupComboMonthYear();
            SetupGrid();

            // lấy MID từ MainForm
            if (_main != null)
            {
                _managerEmployeeID = _main.CurrentEmployeeID;

                if (string.IsNullOrWhiteSpace(_managerEmployeeID) || _managerEmployeeID == "N/A")
                {
                    MessageBox.Show("Không lấy được EmployeeID của Manager từ MainForm.");
                    Close();
                    return;
                }

                LoadCenterByEmployeeID();
            }

            // default chọn tháng/năm hiện tại
            cboMonth.SelectedItem = DateTime.Today.Month.ToString();
            cboYear.SelectedItem = DateTime.Today.Year.ToString();

            // Tự động load dữ liệu bảng lương
            LoadPayroll();
        }

        // =================== UI SETUP ===================
        private void SetupComboMonthYear()
        {
            // Month
            cboMonth.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMonth.Items.Clear();
            for (int m = 1; m <= 12; m++)
                cboMonth.Items.Add(m.ToString());

            // Year
            cboYear.DropDownStyle = ComboBoxStyle.DropDownList;
            cboYear.Items.Clear();

            int yNow = DateTime.Today.Year;
            for (int y = yNow - 5; y <= yNow + 1; y++)
                cboYear.Items.Add(y.ToString());

            if (cboMonth.Items.Count > 0) cboMonth.SelectedIndex = 0;
            if (cboYear.Items.Count > 0) cboYear.SelectedIndex = cboYear.Items.Count - 2; // yNow
        }

        private void SetupGrid()
        {
            dgvPayroll.AutoGenerateColumns = true;
            dgvPayroll.AllowUserToAddRows = false;
            dgvPayroll.MultiSelect = false;
            dgvPayroll.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPayroll.RowHeadersVisible = false;
            dgvPayroll.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvPayroll.ReadOnly = true;
            dgvPayroll.EditMode = DataGridViewEditMode.EditProgrammatically;

            dgvPayroll.DataError += (s, e) => { e.ThrowException = false; };

            dgvPayroll.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvPayroll.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        // =================== Center Resolve (nếu bạn cần lọc theo center về sau) ===================
        private void LoadCenterByEmployeeID()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT CenterID FROM EMPLOYEE WHERE EmployeeID = @EID", conn))
            {
                cmd.Parameters.AddWithValue("@EID", _managerEmployeeID);
                conn.Open();

                object result = cmd.ExecuteScalar();
                if (result == null)
                {
                    MessageBox.Show("Không suy ra được CenterID từ EmployeeID Manager.");
                    Close();
                    return;
                }

                _centerID = result.ToString();
            }
        }

        // =================== LOAD PAYROLL ===================
        private void LoadPayroll()
        {
            if (string.IsNullOrWhiteSpace(_managerEmployeeID) || _managerEmployeeID == "N/A")
            {
                MessageBox.Show("Chưa có EmployeeID từ MainForm.");
                return;
            }

            if (cboMonth.SelectedItem == null || cboYear.SelectedItem == null)
            {
                MessageBox.Show("Chọn Month/Year.");
                return;
            }

            int month = int.Parse(cboMonth.SelectedItem.ToString());
            int year = int.Parse(cboYear.SelectedItem.ToString());

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_QL6_Manager_ViewPayrollByEmployee", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // NOTE: PROC đang xem lương theo EmployeeID truyền vào
                // Bạn bảo "EmployeeID lấy từ MainForm" => dùng _managerEmployeeID luôn
                cmd.Parameters.AddWithValue("@EmployeeID", _managerEmployeeID);
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@Year", year);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvPayroll.DataSource = _dt;

            BeautifyHeaders();
            FormatColumns();
        }

        private void BeautifyHeaders()
        {
            if (dgvPayroll.Columns.Contains("PayrollID")) dgvPayroll.Columns["PayrollID"].HeaderText = "Payroll ID";
            if (dgvPayroll.Columns.Contains("EmployeeID")) dgvPayroll.Columns["EmployeeID"].HeaderText = "Employee ID";
            if (dgvPayroll.Columns.Contains("FullName")) dgvPayroll.Columns["FullName"].HeaderText = "Full Name";
            if (dgvPayroll.Columns.Contains("Month")) dgvPayroll.Columns["Month"].HeaderText = "Month";
            if (dgvPayroll.Columns.Contains("Year")) dgvPayroll.Columns["Year"].HeaderText = "Year";

            if (dgvPayroll.Columns.Contains("BasicSalary")) dgvPayroll.Columns["BasicSalary"].HeaderText = "Basic Salary";
            if (dgvPayroll.Columns.Contains("Allowance")) dgvPayroll.Columns["Allowance"].HeaderText = "Allowance";
            if (dgvPayroll.Columns.Contains("ShiftPay")) dgvPayroll.Columns["ShiftPay"].HeaderText = "Shift Pay";
            if (dgvPayroll.Columns.Contains("SaleCommission")) dgvPayroll.Columns["SaleCommission"].HeaderText = "Commission";
            if (dgvPayroll.Columns.Contains("PenaltyFee")) dgvPayroll.Columns["PenaltyFee"].HeaderText = "Penalty";

            if (dgvPayroll.Columns.Contains("TotalIncome")) dgvPayroll.Columns["TotalIncome"].HeaderText = "Total Income";
            if (dgvPayroll.Columns.Contains("NetSalary")) dgvPayroll.Columns["NetSalary"].HeaderText = "Net Salary";
        }

        private void FormatColumns()
        {
            // money
            TryFormatMoney("BasicSalary");
            TryFormatMoney("Allowance");
            TryFormatMoney("ShiftPay");
            TryFormatMoney("SaleCommission");
            TryFormatMoney("PenaltyFee");
            TryFormatMoney("TotalIncome");
            TryFormatMoney("NetSalary");

            // align numeric
            TryAlignRight("Month");
            TryAlignRight("Year");

            // hide center nếu lỡ có
            if (dgvPayroll.Columns.Contains("CenterID")) dgvPayroll.Columns["CenterID"].Visible = false;
        }

        private void TryFormatMoney(string colName)
        {
            if (!dgvPayroll.Columns.Contains(colName)) return;

            dgvPayroll.Columns[colName].DefaultCellStyle.Format = "N0";
            dgvPayroll.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void TryAlignRight(string colName)
        {
            if (!dgvPayroll.Columns.Contains(colName)) return;
            dgvPayroll.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        // =================== EVENTS ===================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                LoadPayroll();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL error:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed:\n" + ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
