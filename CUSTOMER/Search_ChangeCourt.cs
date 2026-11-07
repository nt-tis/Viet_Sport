using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CUSTOMER
{
    public partial class Search_ChangeCourt : Form
    {
        private string _bookingID;
        private string _oldCourtID;
        private string _oldCenterID;
        private DateTime _oldStartTime;
        private DateTime _oldEndTime;
        private string _customerID;

        public Search_ChangeCourt()
        {
            InitializeComponent();
        }

        public Search_ChangeCourt(string bookingID, string oldCourtID, string oldCenterID, 
            DateTime oldStartTime, DateTime oldEndTime, string customerID) : this()
        {
            _bookingID = bookingID;
            _oldCourtID = oldCourtID;
            _oldCenterID = oldCenterID;
            _oldStartTime = oldStartTime;
            _oldEndTime = oldEndTime;
            _customerID = customerID;

            InitializeControls();
        }

        private void InitializeControls()
        {
            if (cbCourtType.Items.Count > 0) cbCourtType.SelectedIndex = 0;
            if (cbSportCenter.Items.Count > 0) cbSportCenter.SelectedIndex = 0;

            dtpStartTime.Format = DateTimePickerFormat.Custom;
            dtpStartTime.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpEndTime.Format = DateTimePickerFormat.Custom;
            dtpEndTime.CustomFormat = "dd/MM/yyyy HH:mm";

            dtpStartTime.Value = _oldStartTime;
            dtpEndTime.Value = _oldEndTime;

            SetupGrid();
            btnSearchCourt_Click(this, EventArgs.Empty);
        }

        private void SetupGrid()
        {
            dgvCourt.AutoGenerateColumns = true;
            dgvCourt.ReadOnly = true;
            dgvCourt.AllowUserToAddRows = false;
            dgvCourt.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCourt.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCourt.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvCourt.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCourt.CellContentClick += DgvCourt_CellContentClick;
        }

        private void btnSearchCourt_Click(object sender, EventArgs e)
        {
            if (dtpStartTime.Value >= dtpEndTime.Value)
            {
                MessageBox.Show("Thời gian bắt đầu phải trước thời gian kết thúc!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_XemDanhSachSanMo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    string courtType = cbCourtType.Text?.Trim();
                    if (string.IsNullOrEmpty(courtType) || courtType == "Choose..." || courtType == "All" || courtType == "Tất cả")
                        cmd.Parameters.AddWithValue("@CourtType", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@CourtType", courtType);

                    cmd.Parameters.AddWithValue("@StartTime", dtpStartTime.Value);
                    cmd.Parameters.AddWithValue("@EndTime", dtpEndTime.Value);

                    string centerId = cbSportCenter.Text?.Trim();
                    if (string.IsNullOrEmpty(centerId) || centerId == "Choose..." || centerId == "All" || centerId == "Tất cả")
                        cmd.Parameters.AddWithValue("@SportCenterID", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@SportCenterID", centerId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvCourt.DataSource = dt;

                    // Định dạng tên cột thân thiện tiếng Việt
                    if (dgvCourt.Columns.Contains("CourtID")) dgvCourt.Columns["CourtID"].HeaderText = "Mã Sân";
                    if (dgvCourt.Columns.Contains("CourtType")) dgvCourt.Columns["CourtType"].HeaderText = "Loại Sân";
                    if (dgvCourt.Columns.Contains("CourtTypeName")) dgvCourt.Columns["CourtTypeName"].HeaderText = "Tên Loại Sân";
                    if (dgvCourt.Columns.Contains("Capacity")) dgvCourt.Columns["Capacity"].HeaderText = "Sức Chứa";
                    if (dgvCourt.Columns.Contains("UnitPrice"))
                    {
                        dgvCourt.Columns["UnitPrice"].HeaderText = "Đơn Giá (VNĐ)";
                        dgvCourt.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
                    }
                    if (dgvCourt.Columns.Contains("CenterID")) dgvCourt.Columns["CenterID"].HeaderText = "Mã Trung Tâm";
                    if (dgvCourt.Columns.Contains("Status")) dgvCourt.Columns["Status"].HeaderText = "Trạng Thái";

                    // Thêm cột button "Book"
                    if (!dgvCourt.Columns.Contains("btnBook"))
                    {
                        DataGridViewButtonColumn btnBook = new DataGridViewButtonColumn
                        {
                            Name = "btnBook",
                            HeaderText = "Thao Tác",
                            Text = "Đổi Sân Này",
                            UseColumnTextForButtonValue = true
                        };
                        dgvCourt.Columns.Add(btnBook);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tra cứu sân: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvCourt_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvCourt.Columns[e.ColumnIndex].Name == "btnBook")
            {
                string newCourtID = dgvCourt.Rows[e.RowIndex].Cells["CourtID"].Value?.ToString();
                string newCenterID = dgvCourt.Rows[e.RowIndex].Cells["CenterID"].Value?.ToString();

                if (string.IsNullOrEmpty(newCourtID))
                {
                    MessageBox.Show("Không thể lấy thông tin sân");
                    return;
                }

                // Mở form Change_Court với thông tin cũ và mới
                OpenChangeCourtForm(newCourtID, newCenterID);
            }
        }

        private void OpenChangeCourtForm(string newCourtID, string newCenterID)
        {
            DateTime newStartTime = dtpStartTime.Value;
            DateTime newEndTime = dtpEndTime.Value;

            Change_Court changeForm = new Change_Court(
                _bookingID,
                _oldCourtID,
                _oldCenterID,
                _oldStartTime,
                _oldEndTime,
                newCourtID,
                newCenterID,
                newStartTime,
                newEndTime,
                _customerID
            );

            // Cho phép minimize độc lập và hiển thị trong taskbar
            changeForm.TopLevel = true;
            changeForm.ShowInTaskbar = true;
            changeForm.StartPosition = FormStartPosition.CenterScreen;

            // Sử dụng Show() thay vì ShowDialog() để các form có thể hoạt động độc lập
            DialogResult result = changeForm.ShowDialog(this);

            // Chỉ đóng form search sau khi change thành công
            if (result == DialogResult.OK)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
