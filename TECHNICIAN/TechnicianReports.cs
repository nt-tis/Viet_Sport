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
    public partial class TechnicianReports : Form
    {
        private readonly string _connStr = "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        private readonly TechnicianMainForm _main;
        private string _techEmployeeID;
        private string _centerID;
        private readonly DataTable _dt = new DataTable();

        private const string COL_SELECT = "chkSelect";

        public TechnicianReports(TechnicianMainForm main)
        {
            InitializeComponent();
            _main = main;
            this.Load += TechnicianReports_Load;
        }

        public TechnicianReports()
        {
            InitializeComponent();
            this.Load += TechnicianReports_Load;
        }

        private void TechnicianReports_Load(object sender, EventArgs e)
        {
            if (_main == null)
            {
                MessageBox.Show("Form chưa nhận MainForm (TechnicianMainForm).");
                return;
            }

            _techEmployeeID = _main.CurrentEmployeeID;
            if (string.IsNullOrWhiteSpace(_techEmployeeID) || _techEmployeeID == "N/A")
            {
                MessageBox.Show("Không lấy được EmployeeID của Technician.");
                return;
            }

            LoadCenterByEmployeeID();
            if (string.IsNullOrWhiteSpace(_centerID))
            {
                MessageBox.Show("Không suy ra được CenterID.");
                return;
            }

            SetupCourtStatusCombo();  // combobox filter
            SetupGrid();              // grid columns
        }

        // ================= FILTER COMBO =================
        private void SetupCourtStatusCombo()
        {
            cboCourtStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCourtStatus.Items.Clear();
            cboCourtStatus.Items.Add("All");
            cboCourtStatus.Items.Add("Đã xử lý");
            cboCourtStatus.Items.Add("Vẫn đang xử lý");
            cboCourtStatus.Items.Add("Thiếu vật liệu");
            cboCourtStatus.SelectedIndex = 0;
        }

        // ================= GRID =================
        private void SetupGrid()
        {
            dgvReport.AutoGenerateColumns = false;
            dgvReport.AllowUserToAddRows = true;
            dgvReport.MultiSelect = false;
            dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReport.RowHeadersVisible = false;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvReport.Columns.Clear();

            // ReportID (readonly)
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReportID",
                DataPropertyName = "ReportID",
                HeaderText = "Report ID",
                ReadOnly = true
            });

            // TimeRepair (DateTimePicker in-cell)
            dgvReport.Columns.Add(new CalendarColumn
            {
                Name = "TimeRepair",
                DataPropertyName = "TimeRepair",
                HeaderText = "TimeRepair" // hiển thị dd/MM/yyyy (hoặc dd/MM/yyyy HH:mm)
            });

            // CourtStatus (ComboBox in-cell)
            var colStatus = new DataGridViewComboBoxColumn
            {
                Name = "CourtStatus",
                DataPropertyName = "CourtStatus",
                HeaderText = "CourtStatus",
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            colStatus.Items.Add("Đã xử lý");
            colStatus.Items.Add("Vẫn đang xử lý");
            colStatus.Items.Add("Thiếu vật liệu");
            dgvReport.Columns.Add(colStatus);

            // IssueDescription (text)
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IssueDescription",
                DataPropertyName = "IssueDescription",
                HeaderText = "Issue"
            });

            // CourtID (text)
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CourtID",
                DataPropertyName = "CourtID",
                HeaderText = "CourtID"
            });

            // CenterID (readonly)
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CenterID",
                DataPropertyName = "CenterID",
                HeaderText = "Center",
                ReadOnly = true
            });

            // TID (readonly)
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TID",
                DataPropertyName = "TID",
                HeaderText = "Technician",
                ReadOnly = true
            });

            // Select checkbox
            dgvReport.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = COL_SELECT,
                HeaderText = "Select",
                Width = 60,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });

            dgvReport.CellContentClick -= dgvReport_CellContentClick;
            dgvReport.CellContentClick += dgvReport_CellContentClick;

            dgvReport.DataError -= dgvReport_DataError;
            dgvReport.DataError += dgvReport_DataError;

            // auto-fill CenterID/TID cho row mới
            dgvReport.DefaultValuesNeeded -= dgvReport_DefaultValuesNeeded;
            dgvReport.DefaultValuesNeeded += dgvReport_DefaultValuesNeeded;
        }

        private void dgvReport_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            // Khi user tạo row mới, fill sẵn CenterID + TID + TimeRepair = Now
            if (e.Row.Cells["CenterID"] != null) e.Row.Cells["CenterID"].Value = _centerID;
            if (e.Row.Cells["TID"] != null) e.Row.Cells["TID"].Value = _techEmployeeID;
            if (e.Row.Cells["TimeRepair"] != null) e.Row.Cells["TimeRepair"].Value = DateTime.Now;
        }

        private void dgvReport_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // tránh lỗi combobox khi cell null
            e.ThrowException = false;
        }

        // tick 1 dòng
        private void dgvReport_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvReport.Columns[e.ColumnIndex].Name != COL_SELECT) return;

            dgvReport.EndEdit();

            foreach (DataGridViewRow row in dgvReport.Rows)
            {
                if (row.Index != e.RowIndex)
                    row.Cells[COL_SELECT].Value = false;
            }
        }

        private DataGridViewRow GetCheckedGridRow()
        {
            dgvReport.EndEdit();

            foreach (DataGridViewRow r in dgvReport.Rows)
            {
                if (r.IsNewRow) continue;
                var v = r.Cells[COL_SELECT].Value;
                if (v != null && v != DBNull.Value && Convert.ToBoolean(v))
                    return r;
            }
            return null;
        }

        // ================= Center =================
        private void LoadCenterByEmployeeID()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT CenterID FROM EMPLOYEE WHERE EmployeeID = @EID", conn))
            {
                cmd.Parameters.AddWithValue("@EID", _techEmployeeID);
                conn.Open();

                object rs = cmd.ExecuteScalar();
                _centerID = rs?.ToString();
            }
        }

        private bool IsCourtBelongsToCenter(string courtId)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM SPORT_COURT WHERE CourtID=@CourtID AND CenterID=@CenterID", conn))
            {
                cmd.Parameters.AddWithValue("@CourtID", courtId);
                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ================= LOAD DATA =================
        private void LoadReports(string courtStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("sp_TN3_Tech_ViewMaintenanceReports", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CenterID", _centerID);
                cmd.Parameters.AddWithValue("@CourtStatus",
                    string.IsNullOrWhiteSpace(courtStatus) ? (object)DBNull.Value : courtStatus);

                _dt.Clear();
                conn.Open();
                da.Fill(_dt);
            }

            dgvReport.DataSource = _dt;

            // format hiển thị time
            if (dgvReport.Columns.Contains("TimeRepair"))
                dgvReport.Columns["TimeRepair"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        // ================= EVENTS =================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string status = cboCourtStatus.SelectedItem?.ToString();
                if (status == "All") status = null;
                LoadReports(status);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed:\n" + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvReport.EndEdit();

            var r = GetCheckedGridRow();
            if (r == null)
            {
                MessageBox.Show("Tick Select 1 dòng để ADD report.");
                return;
            }

            string courtId = Convert.ToString(r.Cells["CourtID"].Value)?.Trim();
            string courtStatus = Convert.ToString(r.Cells["CourtStatus"].Value)?.Trim();
            string issue = Convert.ToString(r.Cells["IssueDescription"].Value)?.Trim();

            DateTime? timeRepair = null;
            if (r.Cells["TimeRepair"].Value != null && DateTime.TryParse(r.Cells["TimeRepair"].Value.ToString(), out DateTime d))
                timeRepair = d;

            if (string.IsNullOrWhiteSpace(courtId))
            {
                MessageBox.Show("Bạn phải nhập CourtID.");
                return;
            }

            if (!IsCourtBelongsToCenter(courtId))
            {
                MessageBox.Show("CourtID không thuộc trung tâm bạn đang làm việc.");
                return;
            }

            if (string.IsNullOrWhiteSpace(courtStatus))
            {
                MessageBox.Show("Bạn phải chọn CourtStatus (Đã xử lý / Vẫn đang xử lý / Thiếu vật liệu).");
                return;
            }

            if (courtStatus != "Đã xử lý" && courtStatus != "Vẫn đang xử lý" && courtStatus != "Thiếu vật liệu")
            {
                MessageBox.Show("CourtStatus chỉ được: Đã xử lý / Vẫn đang xử lý / Thiếu vật liệu.");
                return;
            }

            if (timeRepair == null)
            {
                MessageBox.Show("Bạn phải chọn TimeRepair.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_TN3_Tech_AddMaintenanceReport", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CenterID", _centerID);
                    cmd.Parameters.AddWithValue("@CourtID", courtId);
                    cmd.Parameters.AddWithValue("@CourtStatus", courtStatus);
                    cmd.Parameters.AddWithValue("@IssueDescription",
                        string.IsNullOrWhiteSpace(issue) ? (object)DBNull.Value : issue);
                    cmd.Parameters.AddWithValue("@TimeRepair", timeRepair.Value);
                    cmd.Parameters.AddWithValue("@TID", _techEmployeeID);

                    conn.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            int result = Convert.ToInt32(rd["Result"]);
                            string msg = rd["Message"].ToString();
                            MessageBox.Show(msg);

                            if (result == 1)
                                LoadReports(null);
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

    // ================= DATE TIME PICKER COLUMN (IN-CELL) =================
    public class CalendarColumn : DataGridViewColumn
    {
        public CalendarColumn() : base(new CalendarCell()) { }

        public override DataGridViewCell CellTemplate
        {
            get => base.CellTemplate;
            set
            {
                if (value != null && !value.GetType().IsAssignableFrom(typeof(CalendarCell)))
                    throw new InvalidCastException("Must be a CalendarCell");
                base.CellTemplate = value;
            }
        }
    }

    public class CalendarCell : DataGridViewTextBoxCell
    {
        public CalendarCell()
        {
            Style.Format = "dd/MM/yyyy HH:mm";
        }

        public override Type EditType => typeof(CalendarEditingControl);
        public override Type ValueType => typeof(DateTime);
        public override object DefaultNewRowValue => DateTime.Now;

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            var ctl = DataGridView.EditingControl as CalendarEditingControl;

            if (Value == null || Value == DBNull.Value) ctl.Value = DateTime.Now;
            else ctl.Value = Convert.ToDateTime(Value);
        }
    }

    class CalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
    {
        DataGridView _dgv;
        bool _valueChanged;
        int _rowIndex;

        public CalendarEditingControl()
        {
            Format = DateTimePickerFormat.Custom;
            CustomFormat = "dd/MM/yyyy HH:mm";
            ShowUpDown = true; // chọn giờ/phút nhanh
        }

        public object EditingControlFormattedValue
        {
            get => Value.ToString("dd/MM/yyyy HH:mm");
            set
            {
                if (value is string s && DateTime.TryParse(s, out DateTime d))
                    Value = d;
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) => EditingControlFormattedValue;

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            Font = dataGridViewCellStyle.Font;
            CalendarForeColor = dataGridViewCellStyle.ForeColor;
            CalendarMonthBackground = dataGridViewCellStyle.BackColor;
        }

        public int EditingControlRowIndex { get => _rowIndex; set => _rowIndex = value; }

        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) => true;

        public void PrepareEditingControlForEdit(bool selectAll) { }

        public bool RepositionEditingControlOnValueChange => false;

        public DataGridView EditingControlDataGridView { get => _dgv; set => _dgv = value; }

        public bool EditingControlValueChanged { get => _valueChanged; set => _valueChanged = value; }

        public Cursor EditingPanelCursor => base.Cursor;

        protected override void OnValueChanged(EventArgs eventargs)
        {
            _valueChanged = true;
            _dgv?.NotifyCurrentCellDirty(true);
            base.OnValueChanged(eventargs);
        }
    }
}