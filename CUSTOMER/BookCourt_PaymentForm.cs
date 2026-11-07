using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CUSTOMER
{
    public partial class BookCourt_PaymentForm : Form
    {
        private string _bookingID;
        private string _courtID;
        private string _customerID;
        private DateTime _rentTime;
        private string _level;
        private decimal _balance;
        private decimal _totalPrice;
        private string _username;

        public BookCourt_PaymentForm()
        {
            InitializeComponent();
        }

        public BookCourt_PaymentForm(string bookingID, string courtID, string customerID, string username, DateTime rentTime, string level, decimal balance) : this()
        {
            _bookingID = bookingID;
            _courtID = courtID;
            _customerID = customerID;
            _rentTime = rentTime;
            _level = level;
            _balance = balance;
            _username = username;

            // Cấu hình form để có thể minimize độc lập
            this.TopLevel = true;
            this.ShowInTaskbar = true;
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;

            LoadPaymentInfo();
            InitializePaymentMethods();

            // Cho phép nhấn Enter để xác nhận thanh toán
            this.KeyPreview = true;
            this.AcceptButton = btnPay;

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnPay_Click(this, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            comboMethod.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        private void InitializePaymentMethods()
        {
            comboMethod.Items.Clear();
            comboMethod.Items.Add("Online");
            comboMethod.Items.Add("Tiền mặt");
            comboMethod.SelectedIndex = 0;
        }

        private void LoadPaymentInfo()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    // Load booking details including CenterID, StartTime, EndTime
                    using (SqlCommand cmdBooking = new SqlCommand(
                        @"SELECT bf.StartTime, bf.EndTime, sc.CenterID
                          FROM BOOKING_FORM bf
                          JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
                          WHERE bf.BookingID = @BookingID", conn))
                    {
                        cmdBooking.Parameters.AddWithValue("@BookingID", _bookingID);
                        using (SqlDataReader reader = cmdBooking.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                StartTime.Value = reader.GetDateTime(0);
                                EndTime.Value = reader.GetDateTime(1);
                                CenterID.Text = reader["CenterID"].ToString();
                            }
                            }
                        }

                        // Load service information
                        using (SqlCommand cmdService = new SqlCommand(
                            @"SELECT s.ServiceName, sb.BookingQuantity
                              FROM SERVICE_BOOKING sb
                              JOIN SERVICE s ON sb.ServiceID = s.ServiceID
                              WHERE sb.BookingID = @BookingID", conn))
                        {
                            cmdService.Parameters.AddWithValue("@BookingID", _bookingID);
                            using (SqlDataReader reader = cmdService.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    ServiceName.Text = reader["ServiceName"].ToString();
                                    Quantity.Text = reader["BookingQuantity"].ToString();
                                }
                                else
                                {
                                    ServiceName.Text = "N/A";
                                    Quantity.Text = "0";
                                }
                            }
                        }

                        // Gọi sp_TaoInvoice với PaymentMethod = NULL để chỉ tính TotalPrice
                        using (SqlCommand cmd = new SqlCommand("sp_TaoInvoice", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            
                            cmd.Parameters.AddWithValue("@BookingID", _bookingID);
                            cmd.Parameters.AddWithValue("@CourtID", _courtID);
                            cmd.Parameters.AddWithValue("@CustomerID", _customerID);
                            cmd.Parameters.AddWithValue("@RentTime", _rentTime);
                            cmd.Parameters.AddWithValue("@PaymentMethod", DBNull.Value); // NULL = chỉ tính giá
                            cmd.Parameters.AddWithValue("@Level", _level);
                            cmd.Parameters.AddWithValue("@Balance", _balance);
                            
                            SqlParameter totalPriceParam = new SqlParameter("@TotalPrice", SqlDbType.Decimal)
                            {
                                Direction = ParameterDirection.Output,
                                Precision = 18,
                                Scale = 2
                            };
                            cmd.Parameters.Add(totalPriceParam);
                            
                            cmd.ExecuteNonQuery();
                            
                            if (totalPriceParam.Value != null && totalPriceParam.Value != DBNull.Value)
                            {
                                _totalPrice = Convert.ToDecimal(totalPriceParam.Value);
                            }
                        }

                        // Hiển thị thông tin
                        BookingID.Text = _bookingID;
                        CourtID.Text = _courtID;
                        CustomerID.Text = _customerID;
                        TotalPrice.Text = string.Format("{0:N0} VNĐ", _totalPrice);

                        // ReadOnly
                        BookingID.ReadOnly = true;
                        CourtID.ReadOnly = true;
                        CustomerID.ReadOnly = true;
                        CenterID.ReadOnly = true;
                        ServiceName.ReadOnly = true;
                        Quantity.ReadOnly = true;
                        TotalPrice.ReadOnly = true;
                        StartTime.Enabled = false;
                        EndTime.Enabled = false;
                    }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            string paymentMethod = comboMethod.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(paymentMethod))
            {
                MessageBox.Show("Vui lòng chọn phương thức thanh toán", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kiểm tra _username trước khi xử lý
            if (string.IsNullOrEmpty(_username))
            {
                MessageBox.Show("Lỗi: Không tìm thấy thông tin tài khoản người dùng.\nVui lòng đăng nhập lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    // Gọi sp_KH_ThanhToan
                    using (SqlCommand cmd = new SqlCommand("sp_KH_ThanhToan", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        // Input parameters
                        cmd.Parameters.AddWithValue("@BookingID", _bookingID);
                        cmd.Parameters.AddWithValue("@RentTime", _rentTime);
                        cmd.Parameters.AddWithValue("@Balance", _balance);
                        cmd.Parameters.AddWithValue("@TotalPrice", _totalPrice);
                        cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                        var pNewBalance = new SqlParameter("@NewBalance", SqlDbType.Decimal)
                        {
                            Direction = ParameterDirection.Output,
                            Precision = 18,
                            Scale = 2
                        };
                        cmd.Parameters.Add(pNewBalance);

                        cmd.ExecuteNonQuery();

                        // Hiển thị thông báo thành công
                        if (paymentMethod == "Online")
                        {
                            // Nhận số dư mới nguyên tử từ Stored Procedure
                            if (pNewBalance.Value != DBNull.Value)
                            {
                                _balance = Convert.ToDecimal(pNewBalance.Value);
                            }
                            else
                            {
                                _balance -= _totalPrice;
                            }

                            MessageBox.Show(
                                $"Thanh toán thành công!\nTổng tiền: {_totalPrice:N0} VNĐ\nSố dư mới: {_balance:N0} VNĐ", 
                                "Thông báo", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Information
                            );

                            // Cập nhật Customer_Main form để hiển thị số dư mới
                            Customer_Main mainForm = FindMainForm();
                            if (mainForm != null)
                            {
                                mainForm.UpdateBalance(_balance);
                            }

                            // Set DialogResult để form cha biết thanh toán thành công
                            this.DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            MessageBox.Show(
                                $"Đã ghi nhận đơn đặt sân!\nTổng tiền: {_totalPrice:N0} VNĐ\nVui lòng thanh toán {paymentMethod} tại quầy.", 
                                "Thông báo", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Information
                            );
                            
                            this.DialogResult = DialogResult.OK;
                        }

                        this.Close();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Username: {(_username ?? "NULL")}\nLỗi: {ex.Message}",
                    "Lỗi thanh toán",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private Customer_Main FindMainForm()
        {
            Form current = this;
            while (current != null)
            {
                if (current is Customer_Main)
                    return (Customer_Main)current;
                current = current.Owner;
            }

            // Tìm trong Application.OpenForms
            foreach (Form form in Application.OpenForms)
            {
                if (form is Customer_Main)
                    return (Customer_Main)form;
            }

            return null;
        }
    }
}
