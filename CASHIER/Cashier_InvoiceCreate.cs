using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CASHIER
{
    public partial class Cashier_InvoiceCreate : Form
    {
        private string connectionString = DatabaseConfig.ConnectionString;
        
        private decimal originalPrice = 0;
        private decimal discountPercentage = 0;

        // Thông tin đăng nhập
        private string _cashierID;
        private string _employeeID;
        private string _fullName;
        private string _username;
        private string _centerID;

        public Cashier_InvoiceCreate()
        {
            InitializeComponent();
        }

        // Constructor nhận thông tin đăng nhập
        public Cashier_InvoiceCreate(string employeeID, string fullName, string username, string centerID) : this()
        {
            _employeeID = employeeID;
            _fullName = fullName;
            _username = username;
            _cashierID = username;
            _centerID = centerID;

            LoadPendingBookings();
            LoadAvailableDiscounts();
        }

        private void LoadAvailableDiscounts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Sử dụng stored procedure sp_Cashier_GetAvailableDiscounts
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetAvailableDiscounts", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Thêm option "No Discount" vào đầu danh sách
                    DataRow emptyRow = dt.NewRow();
                    emptyRow["DiscountID"] = DBNull.Value;
                    emptyRow["DiscountDisplay"] = "-- Không áp dụng giảm giá --";
                    emptyRow["DiscountName"] = "";
                    emptyRow["Percentage"] = 0;
                    emptyRow["TargetUser"] = "";
                    dt.Rows.InsertAt(emptyRow, 0);

                    cboDiscount.DisplayMember = "DiscountDisplay";
                    cboDiscount.ValueMember = "DiscountID";
                    cboDiscount.DataSource = dt;
                    cboDiscount.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách giảm giá: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPendingBookings()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetPendingPayments", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // TRUYỀN CENTERID
                    cmd.Parameters.AddWithValue("@CenterID", !string.IsNullOrEmpty(_centerID) ? (object)_centerID : DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvBookings.DataSource = dt;
                    
                    // Format DataGridView columns
                    if (dgvBookings.Columns.Count > 0)
                    {
                        dgvBookings.Columns["BookingID"].HeaderText = "Booking ID";
                        dgvBookings.Columns["CustomerName"].HeaderText = "Khách hàng";
                        dgvBookings.Columns["PhoneNumber"].HeaderText = "SĐT";
                        dgvBookings.Columns["CourtID"].HeaderText = "Sân";
                        dgvBookings.Columns["CourtType"].HeaderText = "Loại sân";
                        dgvBookings.Columns["StartTime"].HeaderText = "Bắt đầu";
                        dgvBookings.Columns["StartTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dgvBookings.Columns["EndTime"].HeaderText = "Kết thúc";
                        dgvBookings.Columns["EndTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dgvBookings.Columns["Duration"].HeaderText = "Thời gian (h)";
                        dgvBookings.Columns["TotalPrice"].HeaderText = "Tổng tiền";
                        dgvBookings.Columns["TotalPrice"].DefaultCellStyle.Format = "N0";
                        dgvBookings.Columns["PaymentStatus"].HeaderText = "Trạng thái";
                        
                        // Hide unnecessary columns
                        if (dgvBookings.Columns.Contains("CustomerID"))
                            dgvBookings.Columns["CustomerID"].Visible = false;
                        if (dgvBookings.Columns.Contains("CenterID"))
                            dgvBookings.Columns["CenterID"].Visible = false;
                        if (dgvBookings.Columns.Contains("PricePerHour"))
                            dgvBookings.Columns["PricePerHour"].Visible = false;
                        if (dgvBookings.Columns.Contains("BookingTime"))
                            dgvBookings.Columns["BookingTime"].Visible = false;
                        if (dgvBookings.Columns.Contains("PaymentDeadline"))
                            dgvBookings.Columns["PaymentDeadline"].Visible = false;
                        if (dgvBookings.Columns.Contains("BookingStatus"))
                        {
                            dgvBookings.Columns["BookingStatus"].HeaderText = "Booking Status";
                            dgvBookings.Columns["BookingStatus"].Visible = true;
                        }
                    }
                    
                    // Hiển thị thông tin center
                    if (!string.IsNullOrEmpty(_centerID) && dt.Rows.Count > 0)
                    {
                        this.Text = $"Tạo Hóa Đơn - Center: {_centerID} ({dt.Rows.Count} booking)";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách booking: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtBookingID.Text.Trim();
            
            // Nếu rỗng, load tất cả bookings
            if (string.IsNullOrEmpty(searchText))
            {
                LoadPendingBookings();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Sử dụng stored procedure sp_Cashier_SearchBooking
                    SqlCommand cmd = new SqlCommand("sp_Cashier_SearchBooking", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchText", searchText);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        dgvBookings.DataSource = dt;
                        
                        // Nếu chỉ có 1 kết quả, tự động điền thông tin
                        if (dt.Rows.Count == 1)
                        {
                            txtCourtID.Text = dt.Rows[0]["CourtID"].ToString();
                            txtCustomerName.Text = dt.Rows[0]["CustomerName"].ToString();
                            originalPrice = Convert.ToDecimal(dt.Rows[0]["TotalPrice"]);
                            txtTotalPrice.Text = string.Format("{0:N0} VNĐ", originalPrice);
                            txtFinalPrice.Text = string.Format("{0:N0} VNĐ", originalPrice);
                            
                            // Reset discount
                            cboDiscount.SelectedIndex = 0;
                            txtDiscountAmount.Clear();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy booking phù hợp!\n\nTìm kiếm theo: BookingID, Tên khách hàng, hoặc Số điện thoại", 
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPendingBookings();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnApplyDiscount_Click(object sender, EventArgs e)
        {
            if (originalPrice == 0)
            {
                MessageBox.Show("Vui lòng chọn booking trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboDiscount.SelectedIndex <= 0)
            {
                // No discount selected - reset về giá gốc
                txtDiscountAmount.Text = "0 VNĐ";
                txtFinalPrice.Text = string.Format("{0:N0} VNĐ", originalPrice);
                discountPercentage = 0;
                MessageBox.Show("Đã bỏ giảm giá. Giá thanh toán: " + string.Format("{0:N0} VNĐ", originalPrice), 
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                DataRowView selectedRow = (DataRowView)cboDiscount.SelectedItem;
                discountPercentage = Convert.ToDecimal(selectedRow["Percentage"]);
                string discountName = selectedRow["DiscountName"].ToString();
                string targetUser = selectedRow["TargetUser"].ToString();
                
                decimal discountAmount = originalPrice * (discountPercentage / 100);
                decimal finalPrice = originalPrice - discountAmount;

                txtDiscountAmount.Text = string.Format("{0:N0} VNĐ ({1}%)", discountAmount, discountPercentage);
                txtFinalPrice.Text = string.Format("{0:N0} VNĐ", finalPrice);

                MessageBox.Show($"Đã áp dụng: {discountName}\n" +
                               $"- Đối tượng: {targetUser}\n" +
                               $"- Giảm giá: {discountPercentage}%\n" +
                               $"- Số tiền giảm: {discountAmount:N0} VNĐ\n" +
                               $"- Thành tiền: {finalPrice:N0} VNĐ", 
                    "Áp dụng giảm giá thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi áp dụng giảm giá: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreateInvoice_Click(object sender, EventArgs e)
        {
            // Validation
            string bookingID = txtBookingID.Text.Trim();
            
            if (string.IsNullOrEmpty(bookingID))
            {
                MessageBox.Show("Vui lòng chọn booking!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvBookings.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dgvBookings.SelectedRows[0];

                if (dgvBookings.Columns.Contains("PaymentStatus") && selectedRow.Cells["PaymentStatus"].Value != null)
                {
                    string paymentStatus = selectedRow.Cells["PaymentStatus"].Value.ToString();
                    if (paymentStatus == "Overdue")
                    {
                        MessageBox.Show(
                            "Không thể tạo hóa đơn!\n\n" +
                            "Booking này đã QUÁ HẠN thanh toán.\n" +
                            "Hạn thanh toán: 2 giờ trước giờ chơi.\n\n" +
                            "Vui lòng liên hệ khách hàng để đặt lại sân.",
                            "Booking Overdue",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }
                }

                if (dgvBookings.Columns.Contains("BookingStatus") && selectedRow.Cells["BookingStatus"].Value != null)
                {
                    string bookingStatus = selectedRow.Cells["BookingStatus"].Value.ToString();
                    if (bookingStatus == "Cancelled")
                    {
                        MessageBox.Show(
                            "Không thể tạo hóa đơn!\n\n" +
                            "Booking này đã bị HỦY (Cancelled).\n\n" +
                            "Lý do: Khách hàng không thanh toán trong 30 phút sau khi đặt.",
                            "Booking Cancelled",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }
                }
            }

            if (cboPaymentMethod.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn phương thức thanh toán!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboPaymentMethod.Focus();
                return;
            }

            string paymentMethod = cboPaymentMethod.Text;
            string discountID = cboDiscount.SelectedIndex > 0 ? cboDiscount.SelectedValue.ToString() : null;

            // Confirmation dialog
            decimal finalAmount = string.IsNullOrEmpty(txtFinalPrice.Text) ? 0 : 
                                  Convert.ToDecimal(txtFinalPrice.Text.Replace(" VNĐ", "").Replace(",", ""));
            
            string confirmMessage = $"Xác nhận tạo hóa đơn?\n\n" +
                                   $"- Booking ID: {bookingID}\n" +
                                   $"- Khách hàng: {txtCustomerName.Text}\n" +
                                   $"- Tổng tiền: {finalAmount:N0} VNĐ\n" +
                                   $"- Phương thức TT: {paymentMethod}";
            
            if (discountID != null)
            {
                confirmMessage += $"\n- Đã áp dụng giảm giá";
            }

            if (!string.IsNullOrEmpty(_fullName))
            {
                confirmMessage += $"\n- Thu ngân: {_fullName} ({_cashierID})";
            }

            DialogResult confirm = MessageBox.Show(confirmMessage, "Xác nhận tạo hóa đơn", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_CreateInvoice", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookingID", bookingID);
                    cmd.Parameters.AddWithValue("@DiscountID", (object)discountID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                    
                    // Truyền CashierID từ thông tin đăng nhập
                    cmd.Parameters.AddWithValue("@CashierID", !string.IsNullOrEmpty(_cashierID) ? (object)_cashierID : DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable result = new DataTable();
                    adapter.Fill(result);

                    if (result.Rows.Count > 0)
                    {
                        int resultCode = Convert.ToInt32(result.Rows[0]["Result"]);
                        string message = result.Rows[0]["Message"].ToString();
                        string generatedInvoiceID = result.Rows[0]["InvoiceID"] != DBNull.Value ?
                                                   result.Rows[0]["InvoiceID"].ToString() : "N/A";
                        decimal totalPrice = Convert.ToDecimal(result.Rows[0]["TotalPrice"]);

                        if (resultCode == 1)
                        {
                            string successMsg = $"{message}\n\n" +
                                              $"- Mã hóa đơn: {generatedInvoiceID}\n" +
                                              $"- Tổng tiền: {totalPrice:N0} VNĐ\n" +
                                              $"- Phương thức: {paymentMethod}\n" +
                                              $"- Trạng thái: Đã thanh toán";
                            
                            if (!string.IsNullOrEmpty(_fullName))
                            {
                                successMsg += $"\n- Thu ngân: {_fullName}";
                            }

                            MessageBox.Show(successMsg, "Tạo hóa đơn thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearForm();
                            LoadPendingBookings();
                        }
                        else
                        {
                            MessageBox.Show(message, "Lỗi tạo hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối database:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvBookings_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgvBookings.Rows[e.RowIndex];

                    string paymentStatus = row.Cells["PaymentStatus"].Value?.ToString();
                    string bookingStatus = dgvBookings.Columns.Contains("BookingStatus") ?
                                         row.Cells["BookingStatus"].Value?.ToString() : null;

                    if (paymentStatus == "Overdue")
                    {
                        MessageBox.Show(
                            "! Booking này đã QUÁ HẠN thanh toán!\n\n" +
                            "Không thể tạo invoice cho booking overdue.",
                            "Cảnh báo Overdue",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }

                    if (bookingStatus == "Cancelled")
                    {
                        MessageBox.Show(
                            "! Booking này đã bị HỦY!\n\n" +
                            "Không thể tạo invoice cho booking cancelled.",
                            "Cảnh báo Cancelled",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }

                    txtBookingID.Text = row.Cells["BookingID"].Value.ToString();
                    txtCourtID.Text = row.Cells["CourtID"].Value.ToString();
                    txtCustomerName.Text = row.Cells["CustomerName"].Value.ToString();
                    originalPrice = Convert.ToDecimal(row.Cells["TotalPrice"].Value);
                    txtTotalPrice.Text = string.Format("{0:N0} VNĐ", originalPrice);
                    txtFinalPrice.Text = string.Format("{0:N0} VNĐ", originalPrice);

                    cboDiscount.SelectedIndex = 0;
                    txtDiscountAmount.Text = "0 VNĐ";
                    discountPercentage = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi chọn booking: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            txtBookingID.Clear();
            txtCourtID.Clear();
            txtCustomerName.Clear();
            txtTotalPrice.Clear();
            txtDiscountAmount.Clear();
            txtFinalPrice.Clear();
            cboPaymentMethod.SelectedIndex = -1;
            cboDiscount.SelectedIndex = 0;
            originalPrice = 0;
            discountPercentage = 0;
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {
        }

        private void dgvBookings_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }
    }
}
