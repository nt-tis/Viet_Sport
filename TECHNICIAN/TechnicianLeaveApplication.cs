using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace TECHNICIAN
{
    public partial class TechnicianLeaveApplication : Form
    {
        private readonly string _connStr =
            "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        private readonly TechnicianMainForm _main;
        private string _techEmployeeID;

        private readonly DataTable _dt = new DataTable();

        public TechnicianLeaveApplication(TechnicianMainForm main)
        {
            InitializeComponent();
            _main = main;
            this.Load += TechnicianLeaveApplication_Load;
        }

        public TechnicianLeaveApplication()
        {
            InitializeComponent();
            this.Load += TechnicianLeaveApplication_Load;
        }

        private void TechnicianLeaveApplication_Load(object sender, EventArgs e)
        {
            SetupDatePicker();
            SetupGrid();
            InitDataTableSchema();

            if (_main == null)
            {
                MessageBox.Show("Form chưa nhận MainForm (TechnicianMainForm).");
                Close();
                return;
            }

            _techEmployeeID = _main.CurrentEmployeeID;
            if (string.IsNullOrWhiteSpace(_techEmployeeID) || _techEmployeeID == "N/A")
            {
                MessageBox.Show("Không lấy được EmployeeID của Technician.");
                Close();
                return;
            }

            dtpRequestDate.Value = DateTime.Today;
            LoadLeaveForms();
        }

        // =================== UI SETUP ===================
        private void SetupDatePicker()
        {
            dtpRequestDate.ShowCheckBox = false;
            dtpRequestDate.Format = DateTimePickerFormat.Custom;
            dtpRequestDate.CustomFormat = "dd/MM/yyyy";
        }

        private void SetupGrid()
        {
            dgvLeave.AutoGenerateColumns = false;
            dgvLeave.AllowUserToAddRows = false;
            dgvLeave.MultiSelect = false;

            // ✅ mượt nhập: chọn theo ô + edit on enter
            dgvLeave.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvLeave.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvLeave.RowHeadersVisible = false;
            dgvLeave.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // ✅ tránh autosize row/wrap làm lag khi Reason dài
            dgvLeave.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvLeave.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // ✅ tránh popup lỗi mặc định
            dgvLeave.DataError += (s, e) => { e.ThrowException = false; e.Cancel = true; };

            dgvLeave.Columns.Clear();

            dgvLeave.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LeaveFormID",
                DataPropertyName = "LeaveFormID",
                HeaderText = "Mã Đơn",
                ReadOnly = true
            });

            dgvLeave.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EmployeeID",
                DataPropertyName = "EmployeeID",
                HeaderText = "Employee",
                ReadOnly = true
            });

            dgvLeave.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "WorkDate",
                DataPropertyName = "WorkDate",
                HeaderText = "Ngày Nghỉ (dd/MM/yyyy)"
            });

            dgvLeave.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "WorkTime",
                DataPropertyName = "WorkTime",
                HeaderText = "Giờ Nghỉ (HH:mm)"
            });

            dgvLeave.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DateCreate",
                DataPropertyName = "DateCreate",
                HeaderText = "Ngày Tạo",
                ReadOnly = true
            });

            dgvLeave.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Reason",
                DataPropertyName = "Reason",
                HeaderText = "Lý Do"
            });

            dgvLeave.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Trạng Thái",
                ReadOnly = true
            });

            AddCheckBoxColumnAtEnd();

            dgvLeave.CellContentClick -= dgvLeave_CellContentClick;
            dgvLeave.CellContentClick += dgvLeave_CellContentClick;

            // bỏ auto-commit dirty (gây lag khi gõ Time/Reason)
            dgvLeave.CurrentCellDirtyStateChanged -= dgvLeave_CurrentCellDirtyStateChanged;
        }

        private void AddCheckBoxColumnAtEnd()
        {
            if (!dgvLeave.Columns.Contains("chkSelect"))
            {
                var col = new DataGridViewCheckBoxColumn
                {
                    Name = "chkSelect",
                    HeaderText = "Select",
                    Width = 60,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DataPropertyName = "chkSelect"
                };
                dgvLeave.Columns.Add(col);
            }

            dgvLeave.Columns["chkSelect"].DisplayIndex = dgvLeave.Columns.Count - 1;
        }

        // ✅ GIỮ HÀM nhưng không dùng nữa (để project khỏi lỗi reference nếu designer còn)
        private void dgvLeave_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // intentionally left blank (removed to avoid lag while typing)
        }

        private void dgvLeave_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvLeave.Columns[e.ColumnIndex].Name != "chkSelect") return;

            // ✅ commit đúng lúc click checkbox (không commit khi đang gõ)
            dgvLeave.CommitEdit(DataGridViewDataErrorContexts.Commit);
            dgvLeave.EndEdit();

            foreach (DataGridViewRow row in dgvLeave.Rows)
            {
                if (row.Index != e.RowIndex)
                    row.Cells["chkSelect"].Value = false;
            }
        }

        // =================== DATATABLE SCHEMA ===================
        private void InitDataTableSchema()
        {
            _dt.Clear();
            _dt.Columns.Clear();

            _dt.Columns.Add("LeaveFormID", typeof(string));
            _dt.Columns.Add("EmployeeID", typeof(string));
            _dt.Columns.Add("WorkDate", typeof(string));   // dd/MM/yyyy
            _dt.Columns.Add("WorkTime", typeof(string));   // HH:mm
            _dt.Columns.Add("DateCreate", typeof(string)); // dd/MM/yyyy HH:mm
            _dt.Columns.Add("Reason", typeof(string));
            _dt.Columns.Add("Status", typeof(string));
            _dt.Columns.Add("chkSelect", typeof(bool));
        }

        // ✅ NEW nằm DƯỚI
        private void AddInputRowAtBottom()
        {
            DataRow r = _dt.NewRow();
            r["LeaveFormID"] = "";
            r["EmployeeID"] = _techEmployeeID;
            r["WorkDate"] = "";
            r["WorkTime"] = "";
            r["DateCreate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            r["Reason"] = "";
            r["Status"] = "NEW";
            r["chkSelect"] = false;

            _dt.Rows.Add(r);
        }

        // =================== LOAD DATA (Search) ===================
        private void LoadLeaveForms()
        {
            DateTime createDate = dtpRequestDate.Value.Date;
            _dt.Clear();

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("dbo.sp_TN4_Tech_SearchLeaveFormsByCreateDate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@EmployeeID", SqlDbType.NVarChar, 50).Value = _techEmployeeID;
                cmd.Parameters.Add("@DateCreate", SqlDbType.Date).Value = createDate;

                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        DataRow row = _dt.NewRow();
                        row["LeaveFormID"] = rd["LeaveFormID"].ToString();
                        row["EmployeeID"] = rd["EmployeeID"].ToString();

                        row["WorkDate"] = Convert.ToDateTime(rd["WorkDate"]).ToString("dd/MM/yyyy");

                        TimeSpan ts = (rd["WorkTime"] is TimeSpan tss)
                            ? tss
                            : TimeSpan.Parse(rd["WorkTime"].ToString());
                        row["WorkTime"] = ts.ToString(@"hh\:mm");

                        row["DateCreate"] = Convert.ToDateTime(rd["DateCreate"]).ToString("dd/MM/yyyy HH:mm");

                        row["Reason"] = rd["Reason"].ToString();
                        row["Status"] = rd["Status"].ToString();
                        row["chkSelect"] = false;

                        _dt.Rows.Add(row);
                    }
                }
            }

            AddInputRowAtBottom();
            dgvLeave.DataSource = _dt;
            AddCheckBoxColumnAtEnd();

            // focus dòng NEW
            if (dgvLeave.Rows.Count > 0)
            {
                int last = dgvLeave.Rows.Count - 1;
                dgvLeave.ClearSelection();
                dgvLeave.CurrentCell = dgvLeave.Rows[last].Cells["WorkDate"];
            }
        }

        // =================== SELECT 1 ROW ===================
        private DataRow GetCheckedRow()
        {
            dgvLeave.EndEdit();

            foreach (DataGridViewRow r in dgvLeave.Rows)
            {
                var v = r.Cells["chkSelect"].Value;
                if (v != null && v != DBNull.Value && Convert.ToBoolean(v))
                    return (r.DataBoundItem as DataRowView)?.Row;
            }
            return null;
        }

        // =================== VALIDATION ===================
        private bool TryParseWorkDate(string s, out DateTime d)
        {
            return DateTime.TryParseExact(
                s,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out d
            );
        }

        private bool TryParseWorkTime(string s, out TimeSpan t)
        {
            return TimeSpan.TryParseExact(
                s,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out t
            );
        }

        private bool ExistsInWorkSchedule(DateTime workDate, TimeSpan workTime)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(1)
                FROM WORK_SCHEDULE
                WHERE EmployeeID = @EID
                  AND WorkDate   = @WDate
                  AND WorkTime   = @WTime;", conn))
            {
                cmd.Parameters.Add("@EID", SqlDbType.NVarChar, 50).Value = _techEmployeeID;
                cmd.Parameters.Add("@WDate", SqlDbType.Date).Value = workDate.Date;
                cmd.Parameters.Add("@WTime", SqlDbType.Time).Value = workTime;

                conn.Open();
                int c = Convert.ToInt32(cmd.ExecuteScalar());
                return c > 0;
            }
        }

        // =================== EVENTS ===================
        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            try
            {
                LoadLeaveForms();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed:\n" + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow row = GetCheckedRow();
                if (row == null)
                {
                    MessageBox.Show("Tick Select 1 dòng để ADD.");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(row["LeaveFormID"]?.ToString()))
                {
                    MessageBox.Show("Vui lòng tick dòng NEW (LeaveFormID trống) để thêm.");
                    return;
                }

                string wDateStr = row["WorkDate"]?.ToString().Trim();
                string wTimeStr = row["WorkTime"]?.ToString().Trim();
                string reason = row["Reason"]?.ToString().Trim();

                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("Nhập Lý do.");
                    return;
                }

                if (!TryParseWorkDate(wDateStr, out DateTime workDate))
                {
                    MessageBox.Show("Ngày nghỉ sai định dạng dd/MM/yyyy. Ví dụ: 10/01/2026");
                    return;
                }

                if (workDate.Date < DateTime.Today)
                {
                    MessageBox.Show("Chỉ được xin nghỉ từ ngày hiện tại trở đi.");
                    return;
                }

                if (!TryParseWorkTime(wTimeStr, out TimeSpan workTime))
                {
                    MessageBox.Show("Giờ nghỉ sai định dạng HH:mm. Ví dụ: 07:00 / 13:00 / 18:00");
                    return;
                }

                if (!ExistsInWorkSchedule(workDate, workTime))
                {
                    MessageBox.Show("Ca này không tồn tại trong WorkSchedule của bạn (EmployeeID, WorkDate, WorkTime).");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("dbo.sp_TN4_Tech_AddLeaveForm", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@EmployeeID", SqlDbType.NVarChar, 50).Value = _techEmployeeID;
                    cmd.Parameters.Add("@WorkDate", SqlDbType.Date).Value = workDate.Date;
                    cmd.Parameters.Add("@WorkTime", SqlDbType.Time).Value = workTime;
                    cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 255).Value = reason;

                    conn.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            MessageBox.Show(rd["Message"].ToString());
                            if (Convert.ToInt32(rd["Result"]) == 1)
                                LoadLeaveForms();
                        }
                        else
                        {
                            MessageBox.Show("Add failed (no response).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add failed:\n" + ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}
