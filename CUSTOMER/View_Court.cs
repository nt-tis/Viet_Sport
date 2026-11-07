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

namespace CUSTOMER
{
    public partial class View_Court : Form
    {
        private string _customerID;

        public View_Court()
        {
            InitializeComponent();

            cbCourtType.Items.Clear();
            cbCourtType.Items.AddRange(new object[] { "Tất cả", "BONGDA", "CAULONG", "BONGRO", "TENNIS", "FUTSAL" });
            cbCourtType.SelectedIndex = 0;

            cbSportCenter.Items.Clear();
            cbSportCenter.Items.AddRange(new object[] { "Tất cả", "CEN001", "CEN002", "CEN003", "CEN004", "CEN005", "CEN006", "CEN007", "CEN008", "CEN009", "CEN010" });
            cbSportCenter.SelectedIndex = 0;

            dtpStartTime.Format = DateTimePickerFormat.Custom;
            dtpStartTime.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpEndTime.Format = DateTimePickerFormat.Custom;
            dtpEndTime.CustomFormat = "dd/MM/yyyy HH:mm";

            // Đặt mặc định ngày là HÔM NAY theo đúng yêu cầu
            DateTime today = DateTime.Today;
            DateTime now = DateTime.Now;
            int startHour = Math.Min(Math.Max(now.Hour + 2, 7), 20);
            dtpStartTime.Value = new DateTime(today.Year, today.Month, today.Day, startHour, 0, 0);
            dtpEndTime.Value = dtpStartTime.Value.AddHours(2);

            // Cho phép nhấn Enter để tìm kiếm / chuyển bước
            this.KeyPreview = true;
            this.AcceptButton = btnSearchCourt;

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnSearchCourt_Click(this, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            cbCourtType.KeyDown += enterHandler;
            cbSportCenter.KeyDown += enterHandler;
            dtpStartTime.KeyDown += enterHandler;
            dtpEndTime.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        public View_Court(string customerID) : this()
        {
            _customerID = customerID;
        }

        private void View_Court_Load(object sender, EventArgs e)
        {
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
            dgvCourt.RowTemplate.Height = 35;
            dgvCourt.BackgroundColor = Color.White;
            dgvCourt.BorderStyle = BorderStyle.None;
            dgvCourt.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            dgvCourt.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCourt.ColumnHeadersDefaultCellStyle.BackColor = Color.ForestGreen;
            dgvCourt.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCourt.EnableHeadersVisualStyles = false;
            dgvCourt.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 250, 245);
            dgvCourt.CellClick += dgvCourt_CellClick;
            dgvCourt.CellPainting += dgvCourt_CellPainting;
            dgvCourt.CellFormatting += dgvCourt_CellFormatting;
            dgvCourt.CellMouseEnter += dgvCourt_CellMouseEnter;
            dgvCourt.CellMouseLeave += dgvCourt_CellMouseLeave;
            dgvCourt.KeyDown += dgvCourt_KeyDown;
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

                    // Định dạng tiêu đề cột thân thiện
                    if (dgvCourt.Columns.Contains("CourtID")) dgvCourt.Columns["CourtID"].HeaderText = "Mã Sân";
                    if (dgvCourt.Columns.Contains("CourtType")) dgvCourt.Columns["CourtType"].HeaderText = "Loại Sân";
                    if (dgvCourt.Columns.Contains("CourtTypeName")) dgvCourt.Columns["CourtTypeName"].HeaderText = "Tên Loại Sân";
                    if (dgvCourt.Columns.Contains("Capacity")) dgvCourt.Columns["Capacity"].HeaderText = "Sức Chứa";
                    if (dgvCourt.Columns.Contains("UnitPrice"))
                    {
                        dgvCourt.Columns["UnitPrice"].HeaderText = "Đơn Giá (VNĐ/Giờ)";
                        dgvCourt.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
                        dgvCourt.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    if (dgvCourt.Columns.Contains("CenterID")) dgvCourt.Columns["CenterID"].HeaderText = "Mã Trung Tâm";
                    if (dgvCourt.Columns.Contains("Status")) dgvCourt.Columns["Status"].HeaderText = "Trạng Thái";

                    // Thêm cột button "Đặt Sân Ngay" nếu chưa có
                    if (!dgvCourt.Columns.Contains("btnBook"))
                    {
                        DataGridViewButtonColumn btnBook = new DataGridViewButtonColumn
                        {
                            Name = "btnBook",
                            HeaderText = "Thao Tác",
                            Text = "Đặt Sân Ngay",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 130
                        };
                        btnBook.DefaultCellStyle.BackColor = Color.ForestGreen;
                        btnBook.DefaultCellStyle.ForeColor = Color.White;
                        btnBook.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 100, 0);
                        btnBook.DefaultCellStyle.SelectionForeColor = Color.White;
                        btnBook.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        dgvCourt.Columns.Add(btnBook);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tra cứu sân: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvCourt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && dgvCourt.CurrentRow != null && dgvCourt.CurrentRow.Index >= 0)
            {
                TriggerBookFromRow(dgvCourt.CurrentRow.Index);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void dgvCourt_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvCourt.Columns[e.ColumnIndex].Name == "btnBook")
            {
                bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                Color rowBg = isSelected
                    ? dgvCourt.DefaultCellStyle.SelectionBackColor
                    : (e.RowIndex % 2 == 1 && dgvCourt.AlternatingRowsDefaultCellStyle.BackColor != Color.Empty
                        ? dgvCourt.AlternatingRowsDefaultCellStyle.BackColor
                        : dgvCourt.DefaultCellStyle.BackColor);

                using (SolidBrush bgBrush = new SolidBrush(rowBg))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                Rectangle btnRect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y + 4, e.CellBounds.Width - 12, e.CellBounds.Height - 8);
                Color btnBg = isSelected ? Color.FromArgb(0, 100, 0) : Color.ForestGreen;

                using (SolidBrush btnBrush = new SolidBrush(btnBg))
                {
                    e.Graphics.FillRectangle(btnBrush, btnRect);
                }

                using (Pen borderPen = new Pen(Color.FromArgb(20, 90, 20), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, btnRect);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    "Đặt Sân Ngay",
                    new Font("Segoe UI", 9F, FontStyle.Bold),
                    btnRect,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                );

                e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                e.Handled = true;
            }
        }

        private void dgvCourt_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvCourt.Columns[e.ColumnIndex].Name == "btnBook")
            {
                e.CellStyle.BackColor = Color.ForestGreen;
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.SelectionBackColor = Color.FromArgb(0, 100, 0);
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        private void dgvCourt_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvCourt.Columns[e.ColumnIndex].Name == "btnBook")
            {
                dgvCourt.Cursor = Cursors.Hand;
            }
        }

        private void dgvCourt_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgvCourt.Cursor = Cursors.Default;
        }

        private void dgvCourt_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgvCourt.Columns[e.ColumnIndex].Name == "btnBook")
            {
                TriggerBookFromRow(e.RowIndex);
            }
        }

        private void TriggerBookFromRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvCourt.Rows.Count) return;

            string courtID = dgvCourt.Rows[rowIndex].Cells["CourtID"].Value?.ToString();
            string centerID = dgvCourt.Rows[rowIndex].Cells["CenterID"].Value?.ToString();
            string courtType = dgvCourt.Columns.Contains("CourtType") ? dgvCourt.Rows[rowIndex].Cells["CourtType"].Value?.ToString() : "";
            string courtTypeName = dgvCourt.Columns.Contains("CourtTypeName") ? dgvCourt.Rows[rowIndex].Cells["CourtTypeName"].Value?.ToString() : "";
            decimal unitPrice = 0;
            if (dgvCourt.Columns.Contains("UnitPrice") && dgvCourt.Rows[rowIndex].Cells["UnitPrice"].Value != DBNull.Value)
            {
                decimal.TryParse(dgvCourt.Rows[rowIndex].Cells["UnitPrice"].Value.ToString(), out unitPrice);
            }

            if (string.IsNullOrEmpty(courtID) || string.IsNullOrEmpty(centerID))
            {
                MessageBox.Show("Không thể lấy thông tin sân đã chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenBookCourtForm(courtID, centerID, courtType, courtTypeName, unitPrice, dtpStartTime.Value, dtpEndTime.Value);
        }

        private void OpenBookCourtForm(string courtID, string centerID, string courtType, string courtTypeName, decimal unitPrice, DateTime startTime, DateTime endTime)
        {
            Customer_Main mainForm = FindCustomerMainForm();
            if (mainForm == null)
            {
                MessageBox.Show("Không thể lấy thông tin người dùng", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Book_Court bookCourtForm = new Book_Court(
                courtID,
                centerID,
                courtType,
                courtTypeName,
                unitPrice,
                startTime,
                endTime,
                _customerID,
                mainForm.UserName
            );

            bookCourtForm.TopLevel = false;
            bookCourtForm.FormBorderStyle = FormBorderStyle.None;
            bookCourtForm.Dock = DockStyle.Fill;

            Panel parentPanel = this.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.Controls.Clear();
                parentPanel.Controls.Add(bookCourtForm);
                bookCourtForm.Show();
            }
        }

        private Customer_Main FindCustomerMainForm()
        {
            Form current = this.ParentForm;
            while (current != null)
            {
                if (current is Customer_Main)
                    return (Customer_Main)current;
                current = current.Owner ?? current.ParentForm;
            }

            foreach (Form form in Application.OpenForms)
            {
                if (form is Customer_Main)
                    return (Customer_Main)form;
            }

            return null;
        }
    }
}
