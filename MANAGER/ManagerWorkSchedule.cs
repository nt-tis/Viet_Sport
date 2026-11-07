using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MANAGER
{
    public partial class ManagerWorkSchedule : Form
    {
        private static string _connStr =
             "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        private readonly ManagerMainFrom _main;
        private string _managerEmployeeID; // MID = EmployeeID manager
        private string _centerID;          // center của manager
        private readonly DataTable _dt = new DataTable();

        public ManagerWorkSchedule(ManagerMainFrom main)
        {
            InitializeComponent();
            _main = main;
            this.Load += ManagerWorkSchedule_Load;
        }

        public ManagerWorkSchedule()
        {
            InitializeComponent();
            this.Load += ManagerWorkSchedule_Load;
        }

        private void ManagerWorkSchedule_Load(object sender, EventArgs e)
        {
            SetupShiftCombo();
            SetupDatePicker();
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

            dtpDate.Value = DateTime.Today;
            LoadSchedules();
        }

        // =================== UI SETUP ===================
        private void SetupDatePicker()
        {
            dtpDate.Format = DateTimePickerFormat.Custom;
            dtpDate.CustomFormat = "dd/MM/yyyy";
        }

        private void SetupShiftCombo()
        {
            cboShift.DropDownStyle = ComboBoxStyle.DropDownList;
            cboShift.Items.Clear();
            cboShift.Items.Add("Tất cả");
            cboShift.Items.Add("Ca sáng");
            cboShift.Items.Add("Ca chiều");
            cboShift.Items.Add("Ca tối");
            cboShift.SelectedIndex = 0;
        }

        private void SetupGrid()
        {
            dgvSchedules.AutoGenerateColumns = true;
            dgvSchedules.AllowUserToAddRows = true;
            dgvSchedules.MultiSelect = false;
            dgvSchedules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSchedules.RowHeadersVisible = false;
            dgvSchedules.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // mượt hơn khi nhập trực tiếp
            dgvSchedules.EditMode = DataGridViewEditMode.EditOnEnter;

            // tránh popup lỗi format Date/Time khi user gõ
            dgvSchedules.DataError += (s, e) => { e.ThrowException = false; e.Cancel = true; };

            // validate & default values
            dgvSchedules.CellValidating -= dgvSchedules_CellValidating;
            dgvSchedules.CellValidating += dgvSchedules_CellValidating;

            dgvSchedules.CellEndEdit -= dgvSchedules_CellEndEdit;
            dgvSchedules.CellEndEdit += dgvSchedules_CellEndEdit;

            dgvSchedules.DefaultValuesNeeded -= dgvSchedules_DefaultValuesNeeded;
            dgvSchedules.DefaultValuesNeeded += dgvSchedules_DefaultValuesNeeded;

            AddCheckBoxColumnAtEnd();

            dgvSchedules.CellContentClick -= dgvSchedules_CellContentClick;
            dgvSchedules.CellContentClick += dgvSchedules_CellContentClick;

            dgvSchedules.DataBindingComplete -= dgvSchedules_DataBindingComplete;
            dgvSchedules.DataBindingComplete += dgvSchedules_DataBindingComplete;
        }

        private void dgvSchedules_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // format hiển thị (nếu cột là DateTime/TimeSpan)
            if (dgvSchedules.Columns.Contains("WorkDate"))
                dgvSchedules.Columns["WorkDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

            if (dgvSchedules.Columns.Contains("WorkTime"))
                dgvSchedules.Columns["WorkTime"].DefaultCellStyle.Format = @"hh\:mm";

            // ép Select ở cuối
            if (dgvSchedules.Columns.Contains("chkSelect"))
                dgvSchedules.Columns["chkSelect"].DisplayIndex = dgvSchedules.Columns.Count - 1;
        }

        private void AddCheckBoxColumnAtEnd()
        {
            if (!dgvSchedules.Columns.Contains("chkSelect"))
            {
                dgvSchedules.Columns.Add(new DataGridViewCheckBoxColumn
                {
                    Name = "chkSelect",
                    HeaderText = "Select",
                    Width = 60,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }

            dgvSchedules.Columns["chkSelect"].DisplayIndex = dgvSchedules.Columns.Count - 1;
            dgvSchedules.Columns["chkSelect"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvSchedules.Columns["chkSelect"].Width = 60;
        }

        // chỉ tick 1 row
        private void dgvSchedules_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvSchedules.Columns[e.ColumnIndex].Name != "chkSelect") return;

            dgvSchedules.EndEdit();

            foreach (DataGridViewRow row in dgvSchedules.Rows)
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

                _centerID = result.ToString();
            }
        }

        // =================== LOAD DATA (Search) ===================
        private void LoadSchedules()
        {
            string employeeId = string.IsNullOrWhiteSpace(txtEmployeeID.Text) ? null : txtEmployeeID.Text.Trim();
            DateTime workDate = dtpDate.Value.Date;

            string shift = null;
            if (cboShift.SelectedItem != null &&
                !cboShift.SelectedItem.ToString().Equals("Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                shift = cboShift.SelectedItem.ToString();
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_QL3_Manager_GetWorkSchedule", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                cmd.Parameters.AddWithValue("@EmployeeID", (object)employeeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WorkDate", workDate);
                cmd.Parameters.AddWithValue("@Shift", (object)shift ?? DBNull.Value);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvSchedules.DataSource = _dt;

            // mấu chốt để nhập mượt: ép WorkDate/WorkTime thành TEXT khi đang gõ
            ForceEditableTextColumns();

            AddCheckBoxColumnAtEnd();

            // header đẹp
            if (dgvSchedules.Columns.Contains("EmployeeID")) dgvSchedules.Columns["EmployeeID"].HeaderText = "Employee ID";
            if (dgvSchedules.Columns.Contains("EmployeeName")) dgvSchedules.Columns["EmployeeName"].HeaderText = "Employee Name";
            if (dgvSchedules.Columns.Contains("CenterID")) dgvSchedules.Columns["CenterID"].HeaderText = "Center";
            if (dgvSchedules.Columns.Contains("WorkDate")) dgvSchedules.Columns["WorkDate"].HeaderText = "Work Date";
            if (dgvSchedules.Columns.Contains("WorkTime")) dgvSchedules.Columns["WorkTime"].HeaderText = "Work Time";
            if (dgvSchedules.Columns.Contains("Shift")) dgvSchedules.Columns["Shift"].HeaderText = "Shift";
            if (dgvSchedules.Columns.Contains("MID")) dgvSchedules.Columns["MID"].HeaderText = "Manager ID";
            if (dgvSchedules.Columns.Contains("ManagerName")) dgvSchedules.Columns["ManagerName"].HeaderText = "Manager Name";

            // không cho sửa các cột lookup
            if (dgvSchedules.Columns.Contains("EmployeeName")) dgvSchedules.Columns["EmployeeName"].ReadOnly = true;
            if (dgvSchedules.Columns.Contains("ManagerName")) dgvSchedules.Columns["ManagerName"].ReadOnly = true;
            if (dgvSchedules.Columns.Contains("CenterID")) dgvSchedules.Columns["CenterID"].ReadOnly = true;
        }

        private void ForceEditableTextColumns()
        {
            if (dgvSchedules.Columns.Contains("WorkTime"))
            {
                dgvSchedules.Columns["WorkTime"].ValueType = typeof(string);
                dgvSchedules.Columns["WorkTime"].DefaultCellStyle.Format = "";
            }

            if (dgvSchedules.Columns.Contains("WorkDate"))
            {
                dgvSchedules.Columns["WorkDate"].ValueType = typeof(string);
                dgvSchedules.Columns["WorkDate"].DefaultCellStyle.Format = "";
            }
        }

        // =================== SELECT 1 ROW ===================
        private DataRow GetCheckedRow()
        {
            dgvSchedules.EndEdit();

            foreach (DataGridViewRow r in dgvSchedules.Rows)
            {
                if (r.IsNewRow) continue;

                var v = r.Cells["chkSelect"].Value;
                if (v != null && v != DBNull.Value && Convert.ToBoolean(v))
                {
                    return (r.DataBoundItem as DataRowView)?.Row;
                }
            }
            return null;
        }

        // =================== ADD (Assign) ===================
        private bool TryReadTime(object value, out TimeSpan time)
        {
            time = default;

            if (value == null || value == DBNull.Value) return false;

            if (value is TimeSpan ts)
            {
                time = ts;
                return true;
            }

            if (TimeSpan.TryParse(value.ToString(), out var parsed))
            {
                time = parsed;
                return true;
            }

            if (DateTime.TryParse(value.ToString(), out var dt))
            {
                time = dt.TimeOfDay;
                return true;
            }

            // hỗ trợ "8" => 08:00
            if (int.TryParse(value.ToString().Trim(), out int hour) && hour >= 0 && hour <= 23)
            {
                time = new TimeSpan(hour, 0, 0);
                return true;
            }

            return false;
        }

        private void AssignScheduleFromSelectedRow()
        {
            if (string.IsNullOrWhiteSpace(_centerID))
            {
                MessageBox.Show("Chưa xác định CenterID.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_managerEmployeeID) || _managerEmployeeID == "N/A")
            {
                MessageBox.Show("Không lấy được ManagerID (MID). Hãy mở form bằng MainForm (LoadForm<ManagerWorkSchedule>).");
                return;
            }

            dgvSchedules.EndEdit();

            DataRow row = GetCheckedRow();

            if (row == null)
            {
                if (dgvSchedules.CurrentRow == null || dgvSchedules.CurrentRow.IsNewRow)
                {
                    MessageBox.Show("Hãy tick Select 1 dòng (hoặc chọn dòng) để phân công.");
                    return;
                }
                row = (dgvSchedules.CurrentRow.DataBoundItem as DataRowView)?.Row;
            }

            if (row == null)
            {
                MessageBox.Show("Không đọc được dữ liệu từ dòng đã chọn.");
                return;
            }

            string employeeId = row.Table.Columns.Contains("EmployeeID") ? row["EmployeeID"]?.ToString() : null;
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                MessageBox.Show("EmployeeID không được trống trong dòng đã chọn.");
                return;
            }

            // WorkDate có thể là string => parse
            DateTime workDate;
            var wdObj = row["WorkDate"];
            if (wdObj is DateTime d) workDate = d.Date;
            else if (!DateTime.TryParse(wdObj?.ToString(), out workDate))
                workDate = dtpDate.Value.Date;
            workDate = workDate.Date;

            if (!TryReadTime(row["WorkTime"], out var workTime))
            {
                MessageBox.Show("WorkTime không hợp lệ. Nhập dạng HH:mm (vd 08:00).");
                return;
            }

            string shift = row["Shift"]?.ToString();
            if (string.IsNullOrWhiteSpace(shift))
            {
                MessageBox.Show("Shift không được trống.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_QL3_Manager_AssignWorkSchedule", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.Parameters.AddWithValue("@WorkDate", workDate);
                cmd.Parameters.AddWithValue("@WorkTime", workTime);
                cmd.Parameters.AddWithValue("@Shift", shift);
                cmd.Parameters.AddWithValue("@MID", _managerEmployeeID);

                var pOut = new SqlParameter("@result", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(pOut);

                conn.Open();
                cmd.ExecuteNonQuery();

                int result = (pOut.Value == DBNull.Value) ? -1 : Convert.ToInt32(pOut.Value);

                switch (result)
                {
                    case 1:
                        MessageBox.Show("Phân công thành công!");
                        break;
                    case 0:
                        MessageBox.Show("Nhân viên không tồn tại hoặc không thuộc center của Manager.");
                        break;
                    case -2:
                        MessageBox.Show("Manager không tồn tại hoặc không thuộc center.");
                        break;
                    case -3:
                        MessageBox.Show("Không thể phân công ngày trong quá khứ.");
                        break;
                    case -5:
                        MessageBox.Show("Shift không được để trống.");
                        break;
                    case -6:
                        MessageBox.Show("Trùng lịch: nhân viên đã có lịch tại thời điểm này.");
                        break;
                    case -7:
                        MessageBox.Show("Nhân viên có đơn nghỉ phép Approved đúng ca này.");
                        break;
                    default:
                        MessageBox.Show("Lỗi hệ thống khi phân công.");
                        break;
                }
            }
        }

        // =================== GRID INPUT UX HELPERS ===================
        private void dgvSchedules_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            if (dgvSchedules.Columns.Contains("CenterID") && !string.IsNullOrWhiteSpace(_centerID))
                e.Row.Cells["CenterID"].Value = _centerID;

            if (dgvSchedules.Columns.Contains("MID") && !string.IsNullOrWhiteSpace(_managerEmployeeID))
                e.Row.Cells["MID"].Value = _managerEmployeeID;

            if (dgvSchedules.Columns.Contains("WorkDate"))
                e.Row.Cells["WorkDate"].Value = dtpDate.Value.ToString("dd/MM/yyyy");

            if (dgvSchedules.Columns.Contains("Shift") && cboShift.SelectedItem != null &&
                !cboShift.SelectedItem.ToString().Equals("Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.Cells["Shift"].Value = cboShift.SelectedItem.ToString();
            }

            if (dgvSchedules.Columns.Contains("WorkTime"))
                e.Row.Cells["WorkTime"].Value = "08:00";
        }

        private void dgvSchedules_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvSchedules.Rows[e.RowIndex].IsNewRow) return;

            string col = dgvSchedules.Columns[e.ColumnIndex].Name;
            string text = (e.FormattedValue ?? "").ToString().Trim();

            if (col == "WorkTime" && text.Length > 0)
            {
                if (!TryParseFlexibleTime(text, out _))
                {
                    e.Cancel = true;
                    dgvSchedules.Rows[e.RowIndex].ErrorText = "WorkTime nhập HH:mm (vd 08:00)";
                }
            }

            if (col == "WorkDate" && text.Length > 0)
            {
                if (!DateTime.TryParse(text, out _))
                {
                    e.Cancel = true;
                    dgvSchedules.Rows[e.RowIndex].ErrorText = "WorkDate không hợp lệ (vd 17/01/2026)";
                }
            }
        }

        private void dgvSchedules_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                dgvSchedules.Rows[e.RowIndex].ErrorText = "";
        }

        private bool TryParseFlexibleTime(string s, out TimeSpan time)
        {
            time = default;
            s = (s ?? "").Trim();

            if (int.TryParse(s, out int hour) && hour >= 0 && hour <= 23)
            {
                time = new TimeSpan(hour, 0, 0);
                return true;
            }

            if (TimeSpan.TryParse(s, out time)) return true;

            if (DateTime.TryParse(s, out var dt))
            {
                time = dt.TimeOfDay;
                return true;
            }

            return false;
        }

        // =================== BUTTON EVENTS ===================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_centerID))
                {
                    MessageBox.Show("Chưa xác định CenterID.");
                    return;
                }

                LoadSchedules();
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
                AssignScheduleFromSelectedRow();
                LoadSchedules();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Assign failed:\n" + ex.Message);
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {
            // không cần xử lý gì
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // không cần xử lý gì
        }


    }
}
