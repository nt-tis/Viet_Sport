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
    public partial class View_BookedCourts : Form
    {
        private readonly string _customerId;

        public View_BookedCourts(string customerId)
        {
            InitializeComponent();
            _customerId = customerId;

            FromDate.Format = DateTimePickerFormat.Custom;
            FromDate.CustomFormat = "dd/MM/yyyy";
            ToDate.Format = DateTimePickerFormat.Custom;
            ToDate.CustomFormat = "dd/MM/yyyy";

            // Mặc định ngày là HÔM NAY theo đúng yêu cầu
            FromDate.Value = DateTime.Today;
            ToDate.Value = DateTime.Today.AddDays(30);

            // Cho phép nhấn Enter để tra cứu
            this.KeyPreview = true;
            this.AcceptButton = btnSearch;

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadDanhSachSanDaDat();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            FromDate.KeyDown += enterHandler;
            ToDate.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        private void View_BookedCourts_Load(object sender, EventArgs e)
        {
            SetupGrid();
            LoadDanhSachSanDaDat();
        }

        private void SetupGrid()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersVisible = false; // Loai bo cot mui ten thau de bang khop khung thang tap
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersHeight = 36;
            dataGridView1.RowTemplate.Height = 35;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.ForestGreen;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False; // Hien thi day du tren mot dong, khong xuong dong
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 250, 245);
            dataGridView1.CellPainting += DataGridView1_CellPainting;
            dataGridView1.CellMouseEnter += DataGridView1_CellMouseEnter;
            dataGridView1.CellMouseLeave += DataGridView1_CellMouseLeave;
        }

        private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string colName = dataGridView1.Columns[e.ColumnIndex].Name;
                if (colName == "btnChange" || colName == "btnCancel")
                {
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                    Color rowBg = isSelected
                        ? dataGridView1.DefaultCellStyle.SelectionBackColor
                        : (e.RowIndex % 2 == 1 && dataGridView1.AlternatingRowsDefaultCellStyle.BackColor != Color.Empty
                            ? dataGridView1.AlternatingRowsDefaultCellStyle.BackColor
                            : dataGridView1.DefaultCellStyle.BackColor);

                    using (SolidBrush bgBrush = new SolidBrush(rowBg))
                    {
                        e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                    }

                    Rectangle btnRect = new Rectangle(e.CellBounds.X + 4, e.CellBounds.Y + 4, e.CellBounds.Width - 8, e.CellBounds.Height - 8);
                    Color btnBg = colName == "btnChange" ? Color.SteelBlue : Color.Crimson;
                    string text = colName == "btnChange" ? "Đổi Sân" : "Hủy Sân";

                    using (SolidBrush btnBrush = new SolidBrush(btnBg))
                    {
                        e.Graphics.FillRectangle(btnBrush, btnRect);
                    }

                    TextRenderer.DrawText(
                        e.Graphics,
                        text,
                        new Font("Segoe UI", 8.5F, FontStyle.Bold),
                        btnRect,
                        Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                    );

                    e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                    e.Handled = true;
                }
            }
        }

        private void DataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string colName = dataGridView1.Columns[e.ColumnIndex].Name;
                if (colName == "btnChange" || colName == "btnCancel")
                {
                    dataGridView1.Cursor = Cursors.Hand;
                }
            }
        }

        private void DataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.Cursor = Cursors.Default;
        }

        private void AddActionButtons()
        {
            // Xóa columns cũ nếu đã tồn tại
            if (dataGridView1.Columns.Contains("btnChange"))
                dataGridView1.Columns.Remove("btnChange");
            if (dataGridView1.Columns.Contains("btnCancel"))
                dataGridView1.Columns.Remove("btnCancel");

            if (dataGridView1.Rows.Count == 0) return;

            // Thêm button "Change"
            DataGridViewButtonColumn btnChange = new DataGridViewButtonColumn
            {
                Name = "btnChange",
                HeaderText = "Đổi Sân",
                Text = "Đổi Sân",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat,
                MinimumWidth = 100,
                FillWeight = 10
            };
            btnChange.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            btnChange.DefaultCellStyle.BackColor = Color.SteelBlue;
            btnChange.DefaultCellStyle.ForeColor = Color.White;
            btnChange.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridView1.Columns.Add(btnChange);

            // Thêm button "Cancel"
            DataGridViewButtonColumn btnCancel = new DataGridViewButtonColumn
            {
                Name = "btnCancel",
                HeaderText = "Hủy Đặt Sân",
                Text = "Hủy Sân",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat,
                MinimumWidth = 125,
                FillWeight = 12
            };
            btnCancel.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            btnCancel.DefaultCellStyle.BackColor = Color.Crimson;
            btnCancel.DefaultCellStyle.ForeColor = Color.White;
            btnCancel.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridView1.Columns.Add(btnCancel);

            // Đăng ký event CellClick
            dataGridView1.CellClick -= DataGridView1_CellClick;
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            string bookingID = row.Cells["BookingID"].Value?.ToString();

            if (string.IsNullOrEmpty(bookingID))
            {
                MessageBox.Show("Không tìm thấy thông tin booking", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Xử lý button "Change"
            if (dataGridView1.Columns[e.ColumnIndex].Name == "btnChange")
            {
                OpenChangeCourtForm(row);
            }
            // Xử lý button "Cancel"
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "btnCancel")
            {
                CancelBooking(bookingID);
            }
        }

        private void OpenChangeCourtForm(DataGridViewRow oldBookingRow)
        {
            // Lấy thông tin booking cũ
            string bookingID = oldBookingRow.Cells["BookingID"].Value?.ToString();
            string oldCourtID = oldBookingRow.Cells["CourtID"].Value?.ToString();
            DateTime oldStartTime = Convert.ToDateTime(oldBookingRow.Cells["StartTime"].Value);
            DateTime oldEndTime = Convert.ToDateTime(oldBookingRow.Cells["EndTime"].Value);

            // Lấy CenterID từ database thay vì từ DataGridView
            string oldCenterID = null;
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT sc.CenterID 
                                    FROM BOOKING_FORM bf
                                    JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
                                    WHERE bf.BookingID = @BookingID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);
                        object res = cmd.ExecuteScalar();
                        if (res != null)
                        {
                            oldCenterID = res.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy thông tin center: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(oldCenterID))
            {
                MessageBox.Show("Không thể lấy thông tin trung tâm", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Mở form Search_ChangeCourt để cho phép tìm kiếm sân mới
            Search_ChangeCourt searchForm = new Search_ChangeCourt(
                bookingID,
                oldCourtID,
                oldCenterID,
                oldStartTime,
                oldEndTime,
                _customerId
            );

            DialogResult result = searchForm.ShowDialog();

            // Reload danh sách sau khi đóng form Search_ChangeCourt
            if (result == DialogResult.OK)
            {
                LoadDanhSachSanDaDat();
            }
        }

        private void CancelBooking(string bookingID)
        {
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn hủy booking {bookingID}?\n\n" +
                "Lưu ý: Phí phạt sẽ được tính dựa trên thời gian hủy:\n" +
                "- Hủy trước 24h: 10% tổng giá trị\n" +
                "- Hủy trong vòng 24h: 50% tổng giá trị\n\n" +
                "Tổng giá trị đã bao gồm giảm giá theo hạng thành viên.",
                "Xác nhận hủy",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = Connection.GetConnection())
                    {
                        conn.Open();

                        // Lấy Level của customer trước
                        string customerLevel = "Bronze"; // default
                        using (SqlCommand cmdLevel = new SqlCommand(
                            "SELECT MembershipLevel FROM MEMBER_CARD WHERE CustomerID = @CustomerID", conn))
                        {
                            cmdLevel.Parameters.AddWithValue("@CustomerID", _customerId);
                            object levelResult = cmdLevel.ExecuteScalar();
                            if (levelResult != null && levelResult != DBNull.Value)
                            {
                                customerLevel = levelResult.ToString();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand("sp_HuyDatSan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@BookingID", bookingID);
                            cmd.Parameters.AddWithValue("@CustomerID", _customerId);
                            cmd.Parameters.AddWithValue("@Level", customerLevel);

                            // Thêm parameter output để nhận penalty price
                            SqlParameter penaltyParam = new SqlParameter("@PenaltyPrice", SqlDbType.Decimal)
                            {
                                Direction = ParameterDirection.Output,
                                Precision = 10,
                                Scale = 2
                            };
                            cmd.Parameters.Add(penaltyParam);

                            cmd.ExecuteNonQuery();

                            // Lấy penalty price từ output parameter
                            decimal penaltyPrice = 0;
                            if (penaltyParam.Value != DBNull.Value)
                            {
                                penaltyPrice = Convert.ToDecimal(penaltyParam.Value);
                            }

                            string message = "Hủy đặt sân thành công!";
                            if (penaltyPrice > 0)
                            {
                                message += $"\n\nPhí phạt: {penaltyPrice:N0} VNĐ đã được trừ vào số dư của bạn.";
                            }
                            else
                            {
                                message += "\n\nKhông có phí phạt.";
                            }

                            MessageBox.Show(
                                message,
                                "Thành công",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );

                            // Load lại danh sách sân đã đặt
                            LoadDanhSachSanDaDat();

                            // Refresh form cha (Customer_Main) để cập nhật số dư
                            Form owner = this.Owner;
                            if (owner != null)
                            {
                                // Tìm Customer_Main form
                                Customer_Main mainForm = owner as Customer_Main;
                                if (mainForm != null)
                                {
                                    mainForm.RefreshBalance();
                                }
                                else
                                {
                                    // Nếu owner không phải Customer_Main, tìm trong Application.OpenForms
                                    foreach (Form openForm in Application.OpenForms)
                                    {
                                        if (openForm is Customer_Main)
                                        {
                                            ((Customer_Main)openForm).RefreshBalance();
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Nếu không có Owner, tìm trong Application.OpenForms
                                foreach (Form openForm in Application.OpenForms)
                                {
                                    if (openForm is Customer_Main)
                                    {
                                        ((Customer_Main)openForm).RefreshBalance();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    string errorMessage = ex.Message;

                    // Xử lý các lỗi đặc biệt
                    if (errorMessage.Contains("Không thể huỷ khi đã tới giờ sử dụng sân"))
                    {
                        MessageBox.Show(
                            "Không thể hủy sân vì đã tới giờ sử dụng!",
                            "Không thể hủy",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else if (errorMessage.Contains("Booking không tồn tại"))
                    {
                        MessageBox.Show(
                            "Booking không tồn tại hoặc không thuộc về bạn!",
                            "Lỗi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                    else if (errorMessage.Contains("Số dư không đủ"))
                    {
                        MessageBox.Show(
                            "Số dư tài khoản không đủ để thanh toán phí phạt!\n" +
                            "Vui lòng nạp thêm tiền vào tài khoản.",
                            "Số dư không đủ",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Lỗi hủy sân: {errorMessage}",
                            "Lỗi SQL",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Lỗi hệ thống: {ex.Message}",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadDanhSachSanDaDat();
        }

        private void btnViewAll_Click(object sender, EventArgs e)
        {
            LoadDanhSachSanDaDat(true);
        }

        private void LoadDanhSachSanDaDat(bool viewAll = false)
        {
            string customerId = _customerId;

            DateTime startTime = FromDate.Value.Date;
            DateTime endTime = ToDate.Value.Date.AddDays(1).AddSeconds(-1);

            if (!viewAll && FromDate.Value.Date > ToDate.Value.Date)
            {
                MessageBox.Show("Từ ngày phải nhỏ hơn hoặc bằng đến ngày!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_KH_XemDanhSachSanDaDat", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerID", SqlDbType.NVarChar, 50).Value = customerId;

                    if (viewAll)
                    {
                        cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = DBNull.Value;
                        cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = startTime;
                        cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;

                    // Định dạng tiêu đề và độ rộng từng cột (khớp tỷ lệ, không xuống dòng)
                    if (dataGridView1.Columns.Contains("BookingID"))
                    {
                        dataGridView1.Columns["BookingID"].HeaderText = "Mã Đặt Sân";
                        dataGridView1.Columns["BookingID"].MinimumWidth = 110;
                        dataGridView1.Columns["BookingID"].FillWeight = 11;
                        dataGridView1.Columns["BookingID"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }
                    if (dataGridView1.Columns.Contains("CourtID"))
                    {
                        dataGridView1.Columns["CourtID"].HeaderText = "Mã Sân";
                        dataGridView1.Columns["CourtID"].MinimumWidth = 85;
                        dataGridView1.Columns["CourtID"].FillWeight = 9;
                        dataGridView1.Columns["CourtID"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }
                    if (dataGridView1.Columns.Contains("OrderDate"))
                    {
                        dataGridView1.Columns["OrderDate"].HeaderText = "Ngày Đặt";
                        dataGridView1.Columns["OrderDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        dataGridView1.Columns["OrderDate"].MinimumWidth = 105;
                        dataGridView1.Columns["OrderDate"].FillWeight = 11;
                        dataGridView1.Columns["OrderDate"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }
                    if (dataGridView1.Columns.Contains("StartTime"))
                    {
                        dataGridView1.Columns["StartTime"].HeaderText = "Giờ Bắt Đầu";
                        dataGridView1.Columns["StartTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dataGridView1.Columns["StartTime"].MinimumWidth = 155;
                        dataGridView1.Columns["StartTime"].FillWeight = 16;
                        dataGridView1.Columns["StartTime"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }
                    if (dataGridView1.Columns.Contains("EndTime"))
                    {
                        dataGridView1.Columns["EndTime"].HeaderText = "Giờ Kết Thúc";
                        dataGridView1.Columns["EndTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dataGridView1.Columns["EndTime"].MinimumWidth = 155;
                        dataGridView1.Columns["EndTime"].FillWeight = 16;
                        dataGridView1.Columns["EndTime"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }
                    if (dataGridView1.Columns.Contains("BookingStatus"))
                    {
                        dataGridView1.Columns["BookingStatus"].HeaderText = "Trạng Thái";
                        dataGridView1.Columns["BookingStatus"].MinimumWidth = 110;
                        dataGridView1.Columns["BookingStatus"].FillWeight = 11;
                        dataGridView1.Columns["BookingStatus"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }
                    if (dataGridView1.Columns.Contains("TotalPrice"))
                    {
                        dataGridView1.Columns["TotalPrice"].HeaderText = "Tổng Tiền";
                        dataGridView1.Columns["TotalPrice"].DefaultCellStyle.Format = "N0";
                        dataGridView1.Columns["TotalPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dataGridView1.Columns["TotalPrice"].MinimumWidth = 110;
                        dataGridView1.Columns["TotalPrice"].FillWeight = 11;
                        dataGridView1.Columns["TotalPrice"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }
                    if (dataGridView1.Columns.Contains("PaymentStatus"))
                    {
                        dataGridView1.Columns["PaymentStatus"].HeaderText = "Thanh Toán";
                        dataGridView1.Columns["PaymentStatus"].MinimumWidth = 120;
                        dataGridView1.Columns["PaymentStatus"].FillWeight = 12;
                        dataGridView1.Columns["PaymentStatus"].HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    }

                    AddActionButtons();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Không có sân nào được đặt"))
                {
                    dataGridView1.DataSource = null;
                    if (dataGridView1.Columns.Contains("btnChange")) dataGridView1.Columns.Remove("btnChange");
                    if (dataGridView1.Columns.Contains("btnCancel")) dataGridView1.Columns.Remove("btnCancel");
                }
                else
                {
                    MessageBox.Show(ex.Message, "Lỗi SQL");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi hệ thống");
            }
        }
    }
}
