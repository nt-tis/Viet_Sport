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

namespace TECHNICIAN
{
    public partial class TechnicianMaintenanceList : Form
    {
        private readonly string _connStr = "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        private readonly TechnicianMainForm _main;
        private string _techEmployeeID;
        private string _centerID;
        private readonly DataTable _dt = new DataTable();

        // ✅ CTOR nhận main
        public TechnicianMaintenanceList(TechnicianMainForm main)
        {
            InitializeComponent();
            _main = main;
            this.Load += TechnicianMaintenanceList_Load;
        }

        // ✅ CTOR default (designer)
        public TechnicianMaintenanceList()
        {
            InitializeComponent();
            this.Load += TechnicianMaintenanceList_Load;
        }

        private void TechnicianMaintenanceList_Load(object sender, EventArgs e)
        {
            SetupStatusCombo();
            SetupGrid();

            if (_main == null)
            {
                MessageBox.Show("Form chưa nhận MainForm (TechnicianMainForm). Hãy mở bằng LoadForm<TechnicianMaintenanceList>().");
                return;
            }

            _techEmployeeID = _main.CurrentEmployeeID;
            if (string.IsNullOrWhiteSpace(_techEmployeeID) || _techEmployeeID == "N/A")
            {
                MessageBox.Show("Không lấy được EmployeeID của Technician từ MainForm.");
                return;
            }

            LoadCenterByEmployeeID();
        }

        // =================== UI SETUP ===================
        private void SetupStatusCombo()
        {
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.DataSource = null;
            cboStatus.Items.Clear();
            cboStatus.Items.Add("All");
            cboStatus.Items.Add("Có thể sử dụng");
            cboStatus.Items.Add("Đang bảo trì");
            cboStatus.SelectedIndex = 0;
        }

        private void SetupGrid()
        {
            dgvCourts.AutoGenerateColumns = true;
            dgvCourts.AllowUserToAddRows = false;
            dgvCourts.MultiSelect = false;
            dgvCourts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCourts.RowHeadersVisible = false;
            dgvCourts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvCourts.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvCourts.DataError -= dgvCourts_DataError;
            dgvCourts.DataError += dgvCourts_DataError;

            dgvCourts.CellContentClick -= dgvCourts_CellContentClick;
            dgvCourts.CellContentClick += dgvCourts_CellContentClick;
        }

        private void dgvCourts_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // tránh crash khi ComboBox nhận giá trị lạ
            e.ThrowException = false;
        }

        // ✅ Select luôn ở cuối
        private void AddCheckBoxColumnAtEnd()
        {
            if (!dgvCourts.Columns.Contains("chkSelect"))
            {
                var col = new DataGridViewCheckBoxColumn
                {
                    Name = "chkSelect",
                    HeaderText = "Select",
                    Width = 60,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                };
                dgvCourts.Columns.Add(col);
            }

            dgvCourts.Columns["chkSelect"].DisplayIndex = dgvCourts.Columns.Count - 1;
        }

        // chỉ cho tick 1 row
        private void dgvCourts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCourts.Columns[e.ColumnIndex].Name != "chkSelect") return;

            dgvCourts.EndEdit();

            foreach (DataGridViewRow row in dgvCourts.Rows)
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
                cmd.Parameters.AddWithValue("@EID", _techEmployeeID);
                conn.Open();

                object result = cmd.ExecuteScalar();
                if (result == null)
                {
                    MessageBox.Show("Không suy ra được CenterID từ EmployeeID Technician.");
                    return;
                }

                _centerID = result.ToString();
            }
        }

        // =================== GRID: Status Combo Column ===================
        private void ApplyStatusComboColumn()
        {
            if (!dgvCourts.Columns.Contains("Status")) return;

            // Nếu đã là ComboBox column rồi thì thôi
            if (dgvCourts.Columns["Status"] is DataGridViewComboBoxColumn) return;

            int displayIndex = dgvCourts.Columns["Status"].DisplayIndex;
            string header = dgvCourts.Columns["Status"].HeaderText;

            dgvCourts.Columns.Remove("Status");

            var statusCol = new DataGridViewComboBoxColumn
            {
                Name = "Status",
                HeaderText = header,
                DataPropertyName = "Status", // bind với DataTable cột Status
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat
            };

            statusCol.Items.Add("Có thể sử dụng");
            statusCol.Items.Add("Đang bảo trì");

            dgvCourts.Columns.Add(statusCol);
            dgvCourts.Columns["Status"].DisplayIndex = displayIndex;
        }

        private void LockColumnsExceptStatusAndSelect()
        {
            foreach (DataGridViewColumn c in dgvCourts.Columns)
                c.ReadOnly = true;

            if (dgvCourts.Columns.Contains("Status"))
                dgvCourts.Columns["Status"].ReadOnly = false;

            if (dgvCourts.Columns.Contains("chkSelect"))
                dgvCourts.Columns["chkSelect"].ReadOnly = false;
        }

        // =================== LOAD DATA (Search) ===================
        private void LoadCourts(string courtId = null)
        {
            if (string.IsNullOrWhiteSpace(_centerID))
            {
                MessageBox.Show("Chưa xác định CenterID.");
                return;
            }

            courtId = string.IsNullOrWhiteSpace(courtId) ? null : courtId.Trim();

            // All => NULL
            string status = null;
            if (cboStatus.SelectedItem != null &&
                !cboStatus.SelectedItem.ToString().Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                status = cboStatus.SelectedItem.ToString();
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_TN1_Tech_GetCourtMaintenance", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                cmd.Parameters.AddWithValue("@CourtID", (object)(courtId ?? (string)null) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object)(status ?? (string)null) ?? DBNull.Value);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvCourts.DataSource = _dt;

            // ✅ header theo DB
            if (dgvCourts.Columns.Contains("CourtID")) dgvCourts.Columns["CourtID"].HeaderText = "Court ID";
            if (dgvCourts.Columns.Contains("CourtType")) dgvCourts.Columns["CourtType"].HeaderText = "Court Type";
            if (dgvCourts.Columns.Contains("Capacity")) dgvCourts.Columns["Capacity"].HeaderText = "Capacity";
            if (dgvCourts.Columns.Contains("UnitPrice")) dgvCourts.Columns["UnitPrice"].HeaderText = "Unit Price";
            if (dgvCourts.Columns.Contains("CenterID")) dgvCourts.Columns["CenterID"].HeaderText = "Center";
            if (dgvCourts.Columns.Contains("Status")) dgvCourts.Columns["Status"].HeaderText = "Status";

            // ✅ biến Status thành combobox trong grid
            ApplyStatusComboColumn();

            // ✅ checkbox select ở cuối
            AddCheckBoxColumnAtEnd();

            // ✅ chỉ cho sửa Status + tick Select
            LockColumnsExceptStatusAndSelect();
        }

        // =================== SELECT 1 ROW ===================
        private DataRow GetCheckedRow()
        {
            dgvCourts.EndEdit();

            foreach (DataGridViewRow r in dgvCourts.Rows)
            {
                var v = r.Cells["chkSelect"].Value;
                if (v != null && v != DBNull.Value && Convert.ToBoolean(v))
                    return (r.DataBoundItem as DataRowView)?.Row;
            }
            return null;
        }

        // =================== EVENTS ===================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string courtId = txtCourtID.Text.Trim();
                LoadCourts(string.IsNullOrWhiteSpace(courtId) ? null : courtId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed:\n" + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            dgvCourts.EndEdit();

            DataRow row = GetCheckedRow();
            if (row == null)
            {
                MessageBox.Show("Tick Select 1 sân để UPDATE.");
                return;
            }

            string courtId = row["CourtID"]?.ToString();
            string newStatus = row["Status"]?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(courtId))
            {
                MessageBox.Show("CourtID không hợp lệ.");
                return;
            }

            if (newStatus != "Có thể sử dụng" && newStatus != "Đang bảo trì")
            {
                MessageBox.Show("Status không hợp lệ. Chỉ chọn: Có thể sử dụng / Đang bảo trì.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_TN1_Tech_UpdateCourtStatusOnly", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CenterID", _centerID);
                    cmd.Parameters.AddWithValue("@CourtID", courtId);
                    cmd.Parameters.AddWithValue("@Status", newStatus);

                    conn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            int result = Convert.ToInt32(rd["Result"]);
                            string msg = rd["Message"].ToString();
                            MessageBox.Show(msg);

                            if (result == 1) LoadCourts(txtCourtID.Text.Trim());
                        }
                        else
                        {
                            MessageBox.Show("Update failed (no response).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed:\n" + ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}