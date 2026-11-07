using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MANAGER
{
    public partial class ManagerDiscount : Form
    {
        private static string _connStr =
             "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        private readonly ManagerMainFrom _main;
        private string _managerEmployeeID;
        private readonly DataTable _dt = new DataTable();
        private bool _inited = false;

        public ManagerDiscount(ManagerMainFrom main)
        {
            InitializeComponent();
            _main = main;
            this.Load += ManagerDiscount_Load; // ✅ chắc chắn Load chạy
        }

        public ManagerDiscount()
        {
            InitializeComponent();
            this.Load += ManagerDiscount_Load; // ✅ mở kiểu nào cũng init
        }

        private void ManagerDiscount_Load(object sender, EventArgs e)
        {
            if (_inited) return;
            _inited = true;

            SetupDatePickers();
            SetupGrid();

            // lấy MID từ main nếu có
            if (_main != null)
                _managerEmployeeID = _main.CurrentEmployeeID;

            // set default date
            dtpFrom.Value = new DateTime(DateTime.Today.Year, 1, 1);
            dtpTo.Value = DateTime.Today.AddMonths(6);

            // Tự động load danh sách discount
            LoadDiscounts();
        }

        // =================== UI SETUP ===================
        private void SetupDatePickers()
        {
            dtpFrom.Format = DateTimePickerFormat.Custom;
            dtpFrom.CustomFormat = "dd/MM/yyyy";

            dtpTo.Format = DateTimePickerFormat.Custom;
            dtpTo.CustomFormat = "dd/MM/yyyy";
        }

        private void SetupGrid()
        {
            dgvDiscounts.AutoGenerateColumns = false;
            dgvDiscounts.AllowUserToAddRows = true; // ✅ thêm trực tiếp
            dgvDiscounts.MultiSelect = false;
            dgvDiscounts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDiscounts.RowHeadersVisible = false;
            dgvDiscounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // mượt khi nhập trực tiếp
            dgvDiscounts.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvDiscounts.StandardTab = true;

            // tránh popup mặc định
            dgvDiscounts.DataError += (s, e) => { e.ThrowException = false; e.Cancel = true; };

            // validate + default values (giống WorkSchedule)
            dgvDiscounts.DefaultValuesNeeded -= dgvDiscounts_DefaultValuesNeeded;
            dgvDiscounts.DefaultValuesNeeded += dgvDiscounts_DefaultValuesNeeded;

            dgvDiscounts.CellValidating -= dgvDiscounts_CellValidating;
            dgvDiscounts.CellValidating += dgvDiscounts_CellValidating;

            dgvDiscounts.CellEndEdit -= dgvDiscounts_CellEndEdit;
            dgvDiscounts.CellEndEdit += dgvDiscounts_CellEndEdit;

            dgvDiscounts.Columns.Clear();

            // DiscountID (read-only vì auto generate)
            dgvDiscounts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DiscountID",
                DataPropertyName = "DiscountID",
                HeaderText = "Discount ID",
                ReadOnly = true
            });

            // DiscountName
            dgvDiscounts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DiscountName",
                DataPropertyName = "DiscountName",
                HeaderText = "Discount Name"
            });

            // ✅ StartDate / EndDate: TEXT (KHÔNG dùng CalendarColumn)
            dgvDiscounts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StartDate",
                DataPropertyName = "StartDate",
                HeaderText = "Start Date"
            });

            dgvDiscounts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EndDate",
                DataPropertyName = "EndDate",
                HeaderText = "End Date"
            });

            // Percentage
            dgvDiscounts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Percentage",
                DataPropertyName = "Percentage",
                HeaderText = "Percent"
            });

            // TargetUser
            dgvDiscounts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TargetUser",
                DataPropertyName = "TargetUser",
                HeaderText = "Target User"
            });

            // MID (read-only: sẽ set theo manager khi Update/Add)
            dgvDiscounts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MID",
                DataPropertyName = "MID",
                HeaderText = "Manager ID",
                ReadOnly = true
            });

            AddCheckBoxColumnAtEnd();

            dgvDiscounts.CellContentClick -= dgvDiscounts_CellContentClick;
            dgvDiscounts.CellContentClick += dgvDiscounts_CellContentClick;
        }

        // ✅ Select luôn ở cuối + chỉ tick 1 row
        private void AddCheckBoxColumnAtEnd()
        {
            if (!dgvDiscounts.Columns.Contains("chkSelect"))
            {
                dgvDiscounts.Columns.Add(new DataGridViewCheckBoxColumn
                {
                    Name = "chkSelect",
                    HeaderText = "Select",
                    Width = 60,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }

            dgvDiscounts.Columns["chkSelect"].DisplayIndex = dgvDiscounts.Columns.Count - 1;
            dgvDiscounts.Columns["chkSelect"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvDiscounts.Columns["chkSelect"].Width = 60;
        }

        private void dgvDiscounts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvDiscounts.Columns[e.ColumnIndex].Name != "chkSelect") return;

            dgvDiscounts.EndEdit();

            foreach (DataGridViewRow row in dgvDiscounts.Rows)
            {
                if (row.Index != e.RowIndex)
                    row.Cells["chkSelect"].Value = false;
            }
        }

        // =================== DATA LOAD (Search) ===================
        private void LoadDiscounts()
        {
            string discountName = string.IsNullOrWhiteSpace(txtDiscountName.Text) ? null : txtDiscountName.Text.Trim();
            string targetUser = string.IsNullOrWhiteSpace(txtTargetUser.Text) ? null : txtTargetUser.Text.Trim();

            DateTime startDate = dtpFrom.Value.Date;
            DateTime endDate = dtpTo.Value.Date;

            if (startDate > endDate)
            {
                MessageBox.Show("From không được lớn hơn To.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_QL5_Manager_SearchDiscount", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DiscountName", (object)discountName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                cmd.Parameters.AddWithValue("@TargetUser", (object)targetUser ?? DBNull.Value);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvDiscounts.DataSource = _dt;

            // ✅ mấu chốt mượt: ép StartDate/EndDate thành TEXT để gõ không bị khựng
            ForceEditableTextColumns();

            AddCheckBoxColumnAtEnd();
        }

        // GIỮ LẠI HÀM (dù không cần nữa cho CalendarColumn)
        private static void EnsureColumnType(DataTable dt, string colName, Type type)
        {
            if (!dt.Columns.Contains(colName)) return;
            if (dt.Columns[colName].DataType == type) return;

            var newCol = new DataColumn(colName + "_tmp", type);
            int ordinal = dt.Columns[colName].Ordinal;
            dt.Columns.Add(newCol);

            foreach (DataRow r in dt.Rows)
            {
                if (r[colName] == DBNull.Value) r[newCol] = DBNull.Value;
                else r[newCol] = Convert.ToDateTime(r[colName]);
            }

            dt.Columns.Remove(colName);
            newCol.ColumnName = colName;
            newCol.SetOrdinal(ordinal);
        }

        // ✅ thêm: ép grid date columns thành TEXT
        private void ForceEditableTextColumns()
        {
            if (dgvDiscounts.Columns.Contains("StartDate"))
            {
                dgvDiscounts.Columns["StartDate"].ValueType = typeof(string);
                dgvDiscounts.Columns["StartDate"].DefaultCellStyle.Format = "";
            }

            if (dgvDiscounts.Columns.Contains("EndDate"))
            {
                dgvDiscounts.Columns["EndDate"].ValueType = typeof(string);
                dgvDiscounts.Columns["EndDate"].DefaultCellStyle.Format = "";
            }
        }

        // =================== SELECT 1 ROW ===================
        private DataGridViewRow GetCheckedGridRow()
        {
            dgvDiscounts.EndEdit();

            foreach (DataGridViewRow r in dgvDiscounts.Rows)
            {
                if (r.IsNewRow) continue;
                var v = r.Cells["chkSelect"].Value;
                if (v != null && v != DBNull.Value && Convert.ToBoolean(v))
                    return r;
            }
            return null;
        }

        // =================== GRID UX HELPERS (mượt như phân công) ===================
        private void dgvDiscounts_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            if (dgvDiscounts.Columns.Contains("StartDate"))
                e.Row.Cells["StartDate"].Value = DateTime.Today.ToString("dd/MM/yyyy");

            if (dgvDiscounts.Columns.Contains("EndDate"))
                e.Row.Cells["EndDate"].Value = DateTime.Today.ToString("dd/MM/yyyy");

            if (dgvDiscounts.Columns.Contains("Percentage"))
                e.Row.Cells["Percentage"].Value = "10";
        }

        private void dgvDiscounts_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvDiscounts.Rows[e.RowIndex].IsNewRow) return;

            string col = dgvDiscounts.Columns[e.ColumnIndex].Name;
            string text = (e.FormattedValue ?? "").ToString().Trim();

            if ((col == "StartDate" || col == "EndDate") && text.Length > 0)
            {
                if (!DateTime.TryParse(text, out _))
                {
                    e.Cancel = true;
                    dgvDiscounts.Rows[e.RowIndex].ErrorText = "Ngày nhập dd/MM/yyyy (vd 17/01/2026)";
                }
            }
        }

        private void dgvDiscounts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                dgvDiscounts.Rows[e.RowIndex].ErrorText = "";
        }

        // =================== BUTTON EVENTS ===================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try { LoadDiscounts(); }
            catch (Exception ex) { MessageBox.Show("Search failed:\n" + ex.Message); }
        }

        // ✅ UPDATE: lấy đúng row tick + lấy giá trị từ grid (người dùng sửa trực tiếp)
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var r = GetCheckedGridRow();
                if (r == null)
                {
                    MessageBox.Show("Tick Select 1 discount để UPDATE.");
                    return;
                }

                string discountId = Convert.ToString(r.Cells["DiscountID"].Value);
                if (string.IsNullOrWhiteSpace(discountId))
                {
                    MessageBox.Show("Dòng này chưa có DiscountID (dòng mới). Dùng ADD.");
                    return;
                }

                string name = Convert.ToString(r.Cells["DiscountName"].Value)?.Trim();

                // ✅ parse TEXT date
                if (!DateTime.TryParse(Convert.ToString(r.Cells["StartDate"].Value), out DateTime start) ||
                    !DateTime.TryParse(Convert.ToString(r.Cells["EndDate"].Value), out DateTime end))
                {
                    MessageBox.Show("StartDate/EndDate không hợp lệ. Nhập dd/MM/yyyy.");
                    return;
                }

                decimal percent;
                if (!decimal.TryParse(Convert.ToString(r.Cells["Percentage"].Value), NumberStyles.Any, CultureInfo.InvariantCulture, out percent))
                {
                    if (!decimal.TryParse(Convert.ToString(r.Cells["Percentage"].Value), out percent))
                    {
                        MessageBox.Show("Percent không hợp lệ.");
                        return;
                    }
                }

                string target = r.Cells["TargetUser"].Value == DBNull.Value ? null : Convert.ToString(r.Cells["TargetUser"].Value);

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("DiscountName không được để trống.");
                    return;
                }
                if (start > end)
                {
                    MessageBox.Show("StartDate không được lớn hơn EndDate.");
                    return;
                }
                if (percent <= 0 || percent > 100)
                {
                    MessageBox.Show("Percent phải trong (0,100].");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_QL5_Manager_UpdateDiscount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@DiscountID", discountId);
                    cmd.Parameters.AddWithValue("@DiscountName", name);
                    cmd.Parameters.AddWithValue("@StartDate", start.Date);
                    cmd.Parameters.AddWithValue("@EndDate", end.Date);
                    cmd.Parameters.AddWithValue("@Percentage", percent);
                    cmd.Parameters.AddWithValue("@TargetUser", (object)target ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MID", (object)_managerEmployeeID ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("UPDATE thành công!");
                LoadDiscounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed:\n" + ex.Message);
            }
        }

        // ✅ ADD: user tạo dòng mới trực tiếp trên grid -> tick dòng đó -> ADD
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var r = GetCheckedGridRow();
                if (r == null)
                {
                    MessageBox.Show("Tick Select dòng muốn ADD (dòng mới ở cuối bảng).");
                    return;
                }

                string existedId = Convert.ToString(r.Cells["DiscountID"].Value);
                if (!string.IsNullOrWhiteSpace(existedId))
                {
                    MessageBox.Show("Dòng này đã có DiscountID rồi. Nếu muốn sửa thì bấm UPDATE.");
                    return;
                }

                string name = Convert.ToString(r.Cells["DiscountName"].Value)?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("DiscountName không được để trống.");
                    return;
                }

                // ✅ parse TEXT date
                if (!DateTime.TryParse(Convert.ToString(r.Cells["StartDate"].Value), out DateTime start) ||
                    !DateTime.TryParse(Convert.ToString(r.Cells["EndDate"].Value), out DateTime end))
                {
                    MessageBox.Show("StartDate/EndDate không hợp lệ. Nhập dd/MM/yyyy.");
                    return;
                }

                decimal percent;
                if (!decimal.TryParse(Convert.ToString(r.Cells["Percentage"].Value), NumberStyles.Any, CultureInfo.InvariantCulture, out percent))
                {
                    if (!decimal.TryParse(Convert.ToString(r.Cells["Percentage"].Value), out percent))
                    {
                        MessageBox.Show("Percent không hợp lệ.");
                        return;
                    }
                }

                string target = r.Cells["TargetUser"].Value == DBNull.Value ? null : Convert.ToString(r.Cells["TargetUser"].Value);

                if (start > end)
                {
                    MessageBox.Show("StartDate không được lớn hơn EndDate.");
                    return;
                }
                if (percent <= 0 || percent > 100)
                {
                    MessageBox.Show("Percent phải trong (0,100].");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_QL5_Manager_AddDiscount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@DiscountName", name);
                    cmd.Parameters.AddWithValue("@StartDate", start.Date);
                    cmd.Parameters.AddWithValue("@EndDate", end.Date);
                    cmd.Parameters.AddWithValue("@Percentage", percent);
                    cmd.Parameters.AddWithValue("@TargetUser", (object)target ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MID", (object)_managerEmployeeID ?? DBNull.Value);

                    conn.Open();
                    object newId = cmd.ExecuteScalar(); // proc SELECT NewDiscountID
                    MessageBox.Show("ADD thành công! New ID: " + (newId?.ToString() ?? "(null)"));
                }

                LoadDiscounts();
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

    // =================== GIỮ LẠI TẤT CẢ CÁC HÀM/COLUMN CLASS (KHÔNG DÙNG NỮA) ===================
    // (Bạn yêu cầu giữ lại tất cả hàm — để nguyên cho project không lỗi reference nếu nơi khác dùng)

    public class DataGridViewCalendarColumn : DataGridViewColumn
    {
        public DataGridViewCalendarColumn() : base(new DataGridViewCalendarCell()) { }

        public override DataGridViewCell CellTemplate
        {
            get => base.CellTemplate;
            set
            {
                if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewCalendarCell)))
                    throw new InvalidCastException("Must be a DataGridViewCalendarCell");
                base.CellTemplate = value;
            }
        }
    }

    public class DataGridViewCalendarCell : DataGridViewTextBoxCell
    {
        public DataGridViewCalendarCell()
        {
            this.Style.Format = "dd/MM/yyyy";
        }

        public override Type EditType => typeof(DataGridViewCalendarEditingControl);
        public override Type ValueType => typeof(DateTime);
        public override object DefaultNewRowValue => DateTime.Today;

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            var ctl = DataGridView.EditingControl as DataGridViewCalendarEditingControl;

            if (this.Value == null || this.Value == DBNull.Value)
                ctl.Value = DateTime.Today;
            else
                ctl.Value = Convert.ToDateTime(this.Value);
        }
    }

    class DataGridViewCalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
    {
        DataGridView _grid;
        private bool _valueChanged = false;
        int _rowIndex;

        public DataGridViewCalendarEditingControl()
        {
            this.Format = DateTimePickerFormat.Custom;
            this.CustomFormat = "dd/MM/yyyy";
        }

        public object EditingControlFormattedValue
        {
            get => this.Value.ToString("dd/MM/yyyy");
            set
            {
                if (value is string s && DateTime.TryParse(s, out var dt))
                    this.Value = dt;
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) => EditingControlFormattedValue;

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
            this.CalendarForeColor = dataGridViewCellStyle.ForeColor;
            this.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
        }

        public int EditingControlRowIndex { get => _rowIndex; set => _rowIndex = value; }
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) => true;
        public void PrepareEditingControlForEdit(bool selectAll) { }
        public bool RepositionEditingControlOnValueChange => false;

        public DataGridView EditingControlDataGridView { get => _grid; set => _grid = value; }
        public bool EditingControlValueChanged { get => _valueChanged; set => _valueChanged = value; }
        public Cursor EditingPanelCursor => base.Cursor;

        protected override void OnValueChanged(EventArgs eventargs)
        {
            _valueChanged = true;
            _grid?.NotifyCurrentCellDirty(true);
            base.OnValueChanged(eventargs);
        }
    }
}
