using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MANAGER
{
    public partial class ManagerApproveRequest : Form
    {
        private static string _connStr =
             "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        private readonly ManagerMainFrom _main;
        private string _managerEmployeeID;
        private string _centerID;
        private readonly DataTable _dt = new DataTable();

        // ====== THÊM: session A giữ transaction để demo phantom ======
        private SqlConnection _phantomConn;
        private SqlTransaction _phantomTran;
        // ============================================================

        public ManagerApproveRequest(ManagerMainFrom main)
        {
            InitializeComponent();
            _main = main;
        }

        public ManagerApproveRequest()
        {
            InitializeComponent();
        }

        private void ManagerApproveRequest_Load(object sender, EventArgs e)
        {
            SetupStatusCombo();
            SetupDatePicker();
            SetupGrid();

            if (_main != null)
            {
                _managerEmployeeID = _main.CurrentEmployeeID;

                if (!string.IsNullOrWhiteSpace(_managerEmployeeID) && _managerEmployeeID != "N/A")
                {
                    LoadCenterByEmployeeID();
                }
            }

            lblCenterInfo.Text = !string.IsNullOrWhiteSpace(_centerID) ? $"Cơ sở: {_centerID}" : "Chưa xác định cơ sở";

            // mặc định ngày hôm nay nhưng KHÔNG check (để không bị lọc cứng theo ngày, xem được toàn bộ đơn đang chờ của chi nhánh)
            dtpRequestDate.Value = DateTime.Today;
            dtpRequestDate.Checked = false;

            if (!string.IsNullOrWhiteSpace(_centerID))
            {
                LoadLeaveForms();
            }
        }

        // =================== UI SETUP ===================
        private void SetupStatusCombo()
        {
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Items.Clear();
            cboStatus.Items.Add("All");
            cboStatus.Items.Add("Pending");
            cboStatus.Items.Add("Approved");
            cboStatus.Items.Add("Rejected");
            cboStatus.SelectedIndex = 0;
        }

        private void SetupDatePicker()
        {
            dtpRequestDate.ShowCheckBox = true;
            dtpRequestDate.Checked = false;
            dtpRequestDate.Format = DateTimePickerFormat.Custom;
            dtpRequestDate.CustomFormat = "dd/MM/yyyy";
        }

        private void SetupGrid()
        {
            dgvRequests.AutoGenerateColumns = true;
            dgvRequests.AllowUserToAddRows = false;
            dgvRequests.MultiSelect = false;
            dgvRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRequests.RowHeadersVisible = false;
            dgvRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            AddCheckBoxColumnAtEnd();

            // chỉ tick 1 dòng
            dgvRequests.CellContentClick -= dgvRequests_CellContentClick;
            dgvRequests.CellContentClick += dgvRequests_CellContentClick;
        }

        // ✅ Select luôn ở cuối
        private void AddCheckBoxColumnAtEnd()
        {
            if (!dgvRequests.Columns.Contains("chkSelect"))
            {
                var col = new DataGridViewCheckBoxColumn
                {
                    Name = "chkSelect",
                    HeaderText = "Select",
                    Width = 60,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                };
                dgvRequests.Columns.Add(col);
            }

            // ép về cuối sau khi bind datasource
            dgvRequests.Columns["chkSelect"].DisplayIndex = dgvRequests.Columns.Count - 1;
        }

        // chỉ cho tick 1 row
        private void dgvRequests_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvRequests.Columns[e.ColumnIndex].Name != "chkSelect") return;

            dgvRequests.EndEdit();

            foreach (DataGridViewRow row in dgvRequests.Rows)
            {
                if (row.Index != e.RowIndex)
                    row.Cells["chkSelect"].Value = false;
            }
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

                _centerID = result.ToString(); // VD: CEN001
            }
        }

        // =================== LOAD DATA (Search) ===================
        private void LoadLeaveForms()
        {
            if (string.IsNullOrWhiteSpace(_centerID)) return;

            object createDate = dtpRequestDate.Checked ? (object)dtpRequestDate.Value.Date : DBNull.Value;

            string status = null;
            if (cboStatus.SelectedItem != null &&
                !cboStatus.SelectedItem.ToString().Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                status = cboStatus.SelectedItem.ToString();
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_QL2_Manager_ViewLeaveForms", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                cmd.Parameters.AddWithValue("@CreateDate", createDate);
                cmd.Parameters.AddWithValue("@Status",
                    string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvRequests.DataSource = _dt;

            AddCheckBoxColumnAtEnd();
            ApplyHeadersAndFormat();
        }

        // ====== THÊM: query 1 lần trong TRANSACTION A ======
        private async Task<DataTable> QueryLeaveFormsInTranAsync(SqlConnection conn, SqlTransaction tran)
        {
            DateTime createDate = dtpRequestDate.Value.Date;

            string status = null;
            if (cboStatus.SelectedItem != null &&
                !cboStatus.SelectedItem.ToString().Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                status = cboStatus.SelectedItem.ToString();
            }

            var dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand(@"
                DECLARE @d date = CAST(@CreateDate AS date);

                SELECT
                    lf.LeaveFormID, lf.EmployeeID, e.FullName,
                    lf.WorkDate, lf.WorkTime, lf.DateCreate, lf.Reason, lf.Status
                FROM LEAVEFORM lf
                INNER JOIN EMPLOYEE e ON e.EmployeeID = lf.EmployeeID
                WHERE e.CenterID = @CenterID
                  AND lf.DateCreate >= @d
                  AND lf.DateCreate < DATEADD(DAY, 1, @d)
                  AND (@Status IS NULL OR lf.Status = @Status)
                ORDER BY lf.DateCreate DESC, lf.LeaveFormID DESC;", conn, tran))
            {
                cmd.Parameters.Add("@CenterID", SqlDbType.NVarChar, 50).Value = _centerID;
                cmd.Parameters.Add("@CreateDate", SqlDbType.Date).Value = createDate;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;

                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(rd);
                }
            }

            return dt;
        }

        // ====== THÊM: demo phantom đúng kiểu video (A đọc 2 lần, B insert trong lúc chờ) ======
        private async Task PhantomDemo_ReadTwiceAsync()
        {
            // đóng session cũ nếu có
            try
            {
                _phantomTran?.Rollback();
            }
            catch { /* ignore */ }

            _phantomTran?.Dispose();
            _phantomConn?.Dispose();
            _phantomTran = null;
            _phantomConn = null;

            // Session A
            _phantomConn = new SqlConnection(_connStr);
            await _phantomConn.OpenAsync();

            // Đổi isolation tại đây để demo khác nhau:
            // ReadCommitted: phantom xảy ra
            // RepeatableRead: vẫn có phantom
            // Serializable: chặn phantom (B insert sẽ bị block)
            _phantomTran = _phantomConn.BeginTransaction(IsolationLevel.Serializable);

            // READ #1 (BEFORE)
            DataTable before = await QueryLeaveFormsInTranAsync(_phantomConn, _phantomTran);
            dgvRequests.DataSource = before;
            AddCheckBoxColumnAtEnd();
            ApplyHeadersAndFormat();

            /*MessageBox.Show(
                "PHANTOM DEMO\n" +
                $"BEFORE rows = {before.Rows.Count}\n\n" +
                "Bây giờ qua form Technician bấm ADD (trong vòng 10 giây).\n" +
                "Sau 10 giây Manager sẽ tự đọc lại lần 2 (AFTER)."
            );*/

            // chờ 10s để bạn qua Technician bấm Add
            await Task.Delay(10000);

            // READ #2 (AFTER)
            DataTable after = await QueryLeaveFormsInTranAsync(_phantomConn, _phantomTran);
            dgvRequests.DataSource = after;
            AddCheckBoxColumnAtEnd();
            ApplyHeadersAndFormat();

            /*MessageBox.Show(
                "PHANTOM DEMO\n" +
                $"AFTER rows = {after.Rows.Count}\n\n" +
                "Nếu AFTER > BEFORE => PHANTOM."
            );*/

            // commit/close để khỏi giữ lock lâu
            _phantomTran.Commit();
            _phantomTran.Dispose();
            _phantomTran = null;

            _phantomConn.Close();
            _phantomConn.Dispose();
            _phantomConn = null;
        }
        // ============================================================================

        private void ApplyHeadersAndFormat()
        {
            if (dgvRequests.Columns.Contains("LeaveFormID")) dgvRequests.Columns["LeaveFormID"].HeaderText = "Form ID";
            if (dgvRequests.Columns.Contains("EmployeeID")) dgvRequests.Columns["EmployeeID"].HeaderText = "Employee";
            if (dgvRequests.Columns.Contains("FullName")) dgvRequests.Columns["FullName"].HeaderText = "Full Name";
            if (dgvRequests.Columns.Contains("CenterID")) dgvRequests.Columns["CenterID"].HeaderText = "Center";
            if (dgvRequests.Columns.Contains("WorkDate")) dgvRequests.Columns["WorkDate"].HeaderText = "Work Date";
            if (dgvRequests.Columns.Contains("WorkTime")) dgvRequests.Columns["WorkTime"].HeaderText = "Work Time";
            if (dgvRequests.Columns.Contains("DateCreate")) dgvRequests.Columns["DateCreate"].HeaderText = "Created At";
            if (dgvRequests.Columns.Contains("Reason")) dgvRequests.Columns["Reason"].HeaderText = "Reason";
            if (dgvRequests.Columns.Contains("Status")) dgvRequests.Columns["Status"].HeaderText = "Status";

            if (dgvRequests.Columns.Contains("DateCreate"))
                dgvRequests.Columns["DateCreate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        // =================== SELECT 1 ROW ===================
        private DataRow GetCheckedRow()
        {
            dgvRequests.EndEdit();

            foreach (DataGridViewRow r in dgvRequests.Rows)
            {
                var v = r.Cells["chkSelect"].Value;
                if (v != null && v != DBNull.Value && Convert.ToBoolean(v))
                    return (r.DataBoundItem as DataRowView)?.Row;
            }
            return null;
        }

        // =================== EVENTS ===================
        private bool _isSearching = false;

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (_isSearching) return;

            try
            {
                _isSearching = true;
                btnSearch.Enabled = false;

                LoadLeaveForms();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed:\n" + ex.Message);
            }
            finally
            {
                btnSearch.Enabled = true;
                _isSearching = false;
            }
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            dgvRequests.EndEdit();

            DataRow row = GetCheckedRow();
            if (row == null)
            {
                MessageBox.Show("Tick Select 1 đơn để APPROVE.");
                return;
            }

            string curStatus = row["Status"]?.ToString();
            if (!string.Equals(curStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Chỉ duyệt đơn có trạng thái Pending.");
                return;
            }

            string leaveFormId = row["LeaveFormID"].ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_QL2_Manager_ApproveLeaveForm", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CenterID", _centerID);
                    cmd.Parameters.AddWithValue("@LeaveFormID", leaveFormId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("APPROVE thành công!");
                LoadLeaveForms();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Approve failed:\n" + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dgvRequests.EndEdit();

            DataRow row = GetCheckedRow();
            if (row == null)
            {
                MessageBox.Show("Tick Select 1 đơn để REJECT.");
                return;
            }

            string curStatus = row["Status"]?.ToString();
            if (!string.Equals(curStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Chỉ từ chối đơn có trạng thái Pending.");
                return;
            }

            string leaveFormId = row["LeaveFormID"].ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_QL2_Manager_RejectLeaveForm", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CenterID", _centerID);
                    cmd.Parameters.AddWithValue("@LeaveFormID", leaveFormId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("REJECT thành công!");
                LoadLeaveForms();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reject failed:\n" + ex.Message);
            }
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}


