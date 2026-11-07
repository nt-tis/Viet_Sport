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
    public partial class ManagerReport : Form
    {
        private static string _connStr =
             "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        private readonly ManagerMainFrom _main;
        private string _managerEmployeeID;
        private string _centerID;

        private readonly DataTable _dt = new DataTable();

        public ManagerReport(ManagerMainFrom main)
        {
            InitializeComponent();
            _main = main;
            this.Load += ManagerReport_Load;
        }

        public ManagerReport()
        {
            InitializeComponent();
            this.Load += ManagerReport_Load;
        }

        private void ManagerReport_Load(object sender, EventArgs e)
        {
            SetupCombo();
            SetupDatePickers();
            SetupGrid();

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

            dtpFrom.Value = new DateTime(DateTime.Today.Year, 1, 1);
            dtpTo.Value = DateTime.Today;

            // Tự động load báo cáo
            LoadReport();
        }

        // =================== UI SETUP ===================
        private void SetupCombo()
        {
            if (cboRevenue == null) return;

            cboRevenue.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRevenue.Items.Clear();
            cboRevenue.Items.Add("Sân");
            cboRevenue.Items.Add("Dịch vụ");
            cboRevenue.Items.Add("Tổng doanh thu");
            cboRevenue.SelectedIndex = 0;
        }

        private void SetupDatePickers()
        {
            if (dtpFrom != null)
            {
                dtpFrom.Format = DateTimePickerFormat.Custom;
                dtpFrom.CustomFormat = "dd/MM/yyyy";
            }

            if (dtpTo != null)
            {
                dtpTo.Format = DateTimePickerFormat.Custom;
                dtpTo.CustomFormat = "dd/MM/yyyy";
            }
        }

        private void SetupGrid()
        {
            dgvReport.AutoGenerateColumns = true;
            dgvReport.AllowUserToAddRows = false;
            dgvReport.MultiSelect = false;
            dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReport.RowHeadersVisible = false;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvReport.ReadOnly = true;
            dgvReport.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgvReport.DataError += (s, e) => { e.ThrowException = false; };

            dgvReport.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Khóa reorder để không tự kéo đổi vị trí cột
            dgvReport.AllowUserToOrderColumns = false;
        }

        // =================== Center Resolve ===================
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

        // =================== PROC PICKER ===================
        private string GetProcName()
        {
            string type = cboRevenue?.SelectedItem?.ToString() ?? "";

            if (type.Equals("Sân", StringComparison.OrdinalIgnoreCase))
                return "sp_QL4_Manager_RevenueByCourt";

            if (type.Equals("Dịch vụ", StringComparison.OrdinalIgnoreCase))
                return "sp_QL4_Manager_RevenueByService";

            return "sp_QL4_Manager_TotalRevenue";
        }

        // =================== LOAD REPORT ===================
        private void LoadReport()
        {
            if (string.IsNullOrWhiteSpace(_centerID))
            {
                MessageBox.Show("Chưa xác định CenterID.");
                return;
            }

            DateTime fromDate = dtpFrom.Value.Date;
            DateTime toDate = dtpTo.Value.Date;

            if (fromDate > toDate)
            {
                MessageBox.Show("FromDate không được lớn hơn ToDate.");
                return;
            }

            string proc = GetProcName();

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(proc, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvReport.DataSource = _dt;

            // ✅ FIX THEO YÊU CẦU
            RemoveSelectColumnHard();   // bỏ hẳn Select
            BeautifyHeaders();
            FormatColumns();
            ApplyView();                // show/hide theo loại report
            MoveMoneyColumnsToEnd();    // đưa cột tiền ra cuối
            SetTotalLabelIfAny();
        }

        // =================== BỎ SELECT (REMOVE HẲN) ===================
        private void RemoveSelectColumnHard()
        {
            // remove checkbox column nếu lỡ có
            if (dgvReport.Columns.Contains("chkSelect"))
                dgvReport.Columns.Remove("chkSelect");

            // remove cột "Select" nếu designer / data tạo ra
            if (dgvReport.Columns.Contains("Select"))
                dgvReport.Columns.Remove("Select");
        }

        // =================== VIEW FILTER (ẨN/HIỆN CỘT) ===================
        private void ApplyView()
        {
            string type = cboRevenue?.SelectedItem?.ToString() ?? "";

            // Ẩn hết trước
            HideIfExists("CourtID");
            HideIfExists("CourtType");
            HideIfExists("CourtStatus");
            HideIfExists("Revenue");
            HideIfExists("InvoiceCount");

            HideIfExists("ServiceID");
            HideIfExists("ServiceName");
            HideIfExists("TotalQty");

            HideIfExists("TotalRevenue");
            HideIfExists("AvgInvoice");
            HideIfExists("ServiceRevenue");

            if (type.Equals("Sân", StringComparison.OrdinalIgnoreCase))
            {
                // Theo sân: bỏ CourtStatus
                ShowIfExists("CourtID");
                ShowIfExists("CourtType");
                // CourtStatus: KHÔNG HIỆN
                ShowIfExists("InvoiceCount");
                ShowIfExists("Revenue");
            }
            else if (type.Equals("Dịch vụ", StringComparison.OrdinalIgnoreCase))
            {
                // Theo dịch vụ: chỉ hiện đúng phần dịch vụ + Revenue
                ShowIfExists("ServiceID");
                ShowIfExists("ServiceName");
                ShowIfExists("TotalQty");
                ShowIfExists("Revenue");
            }
            else
            {
                // Tổng doanh thu: chỉ hiện tổng
                ShowIfExists("InvoiceCount");
                ShowIfExists("AvgInvoice");
                ShowIfExists("ServiceRevenue");
                ShowIfExists("TotalRevenue");
            }
        }

        private void HideIfExists(string colName)
        {
            if (dgvReport.Columns.Contains(colName))
                dgvReport.Columns[colName].Visible = false;
        }

        private void ShowIfExists(string colName)
        {
            if (dgvReport.Columns.Contains(colName))
                dgvReport.Columns[colName].Visible = true;
        }

        // =================== ĐƯA CỘT TIỀN RA CUỐI ===================
        private void MoveMoneyColumnsToEnd()
        {
            // Chỉ move các cột đang Visible (đỡ bị lệch)
            string[] moneyCols = { "Revenue", "ServiceRevenue", "AvgInvoice", "TotalRevenue" };

            int idx = dgvReport.Columns.Count - 1;

            // đưa lần lượt ra cuối theo thứ tự mong muốn
            foreach (string col in moneyCols)
            {
                if (dgvReport.Columns.Contains(col) && dgvReport.Columns[col].Visible)
                {
                    dgvReport.Columns[col].DisplayIndex = idx;
                    idx--;
                }
            }
        }

        // =================== HEADER + FORMAT ===================
        private void BeautifyHeaders()
        {
            if (dgvReport.Columns.Contains("CourtID")) dgvReport.Columns["CourtID"].HeaderText = "Court ID";
            if (dgvReport.Columns.Contains("CourtType")) dgvReport.Columns["CourtType"].HeaderText = "Court Type";
            if (dgvReport.Columns.Contains("CourtStatus")) dgvReport.Columns["CourtStatus"].HeaderText = "Court Status";

            if (dgvReport.Columns.Contains("ServiceID")) dgvReport.Columns["ServiceID"].HeaderText = "Service ID";
            if (dgvReport.Columns.Contains("ServiceName")) dgvReport.Columns["ServiceName"].HeaderText = "Service Name";
            if (dgvReport.Columns.Contains("TotalQty")) dgvReport.Columns["TotalQty"].HeaderText = "Total Qty";

            if (dgvReport.Columns.Contains("Revenue")) dgvReport.Columns["Revenue"].HeaderText = "Revenue";
            if (dgvReport.Columns.Contains("InvoiceCount")) dgvReport.Columns["InvoiceCount"].HeaderText = "Invoices";

            if (dgvReport.Columns.Contains("TotalRevenue")) dgvReport.Columns["TotalRevenue"].HeaderText = "Total Revenue";
            if (dgvReport.Columns.Contains("AvgInvoice")) dgvReport.Columns["AvgInvoice"].HeaderText = "Avg Invoice";
            if (dgvReport.Columns.Contains("ServiceRevenue")) dgvReport.Columns["ServiceRevenue"].HeaderText = "Service Revenue";
        }

        private void FormatColumns()
        {
            TryFormatMoneyColumn("Revenue");
            TryFormatMoneyColumn("TotalRevenue");
            TryFormatMoneyColumn("ServiceRevenue");
            TryFormatMoneyColumn("AvgInvoice");

            TryFormatIntColumn("InvoiceCount");
            TryFormatIntColumn("TotalQty");
        }

        private void TryFormatMoneyColumn(string colName)
        {
            if (!dgvReport.Columns.Contains(colName)) return;

            dgvReport.Columns[colName].DefaultCellStyle.Format = "N0";
            dgvReport.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void TryFormatIntColumn(string colName)
        {
            if (!dgvReport.Columns.Contains(colName)) return;

            dgvReport.Columns[colName].DefaultCellStyle.Format = "N0";
            dgvReport.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        // =================== TOTAL LABEL (OPTIONAL) ===================
        private void SetTotalLabelIfAny()
        {
            Control[] found = this.Controls.Find("lblTotalRevenue", true);
            if (found.Length == 0) return;

            var lbl = found[0] as Label;
            if (lbl == null) return;

            if (_dt.Rows.Count > 0)
            {
                if (_dt.Columns.Contains("TotalRevenue"))
                {
                    decimal v = SafeDec(_dt.Rows[0]["TotalRevenue"]);
                    lbl.Text = $"Total: {v:N0}";
                    return;
                }

                if (_dt.Columns.Contains("Revenue"))
                {
                    decimal sum = 0;
                    foreach (DataRow r in _dt.Rows) sum += SafeDec(r["Revenue"]);
                    lbl.Text = $"Total: {sum:N0}";
                    return;
                }
            }

            lbl.Text = "Total: 0";
        }

        private decimal SafeDec(object x)
        {
            if (x == null || x == DBNull.Value) return 0;
            try { return Convert.ToDecimal(x); } catch { return 0; }
        }

        // =================== EVENTS ===================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                LoadReport();
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