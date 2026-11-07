using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CASHIER
{
    public partial class Cashier_CancellationFee : Form
    {
        private string connectionString = DatabaseConfig.ConnectionString;

        // Thông tin đăng nhập
        private string _cashierID;
        private string _employeeID;
        private string _fullName;
        private string _username;
        private string _centerID; // THÊM CENTERID

        public Cashier_CancellationFee()
        {
            InitializeComponent();
        }

        // Constructor nhận thông tin đăng nhập VÀ CENTERID
        public Cashier_CancellationFee(string employeeID, string fullName, string username, string centerID) : this()
        {
            _employeeID = employeeID;
            _fullName = fullName;
            _username = username;
            _cashierID = username;
            _centerID = centerID; // LƯU CENTERID
            
            LoadCancellationForms();
        }

        private void LoadCancellationForms()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetPendingCancellations", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // TRUYỀN CENTERID
                    cmd.Parameters.AddWithValue("@CenterID", !string.IsNullOrEmpty(_centerID) ? (object)_centerID : DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvCancellations.DataSource = dt;
                    
                    // Format columns
                    if (dgvCancellations.Columns.Count > 0)
                    {
                        dgvCancellations.Columns["CancelFormID"].HeaderText = "Mã phiếu hủy";
                        dgvCancellations.Columns["BookingID"].HeaderText = "Mã đặt sân";
                        dgvCancellations.Columns["CustomerName"].HeaderText = "Khách hàng";
                        dgvCancellations.Columns["PhoneNumber"].HeaderText = "SĐT";
                        dgvCancellations.Columns["CourtID"].HeaderText = "Sân";
                        dgvCancellations.Columns["CourtType"].HeaderText = "Loại sân";
                        dgvCancellations.Columns["StartTime"].HeaderText = "Giờ bắt đầu";
                        dgvCancellations.Columns["StartTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dgvCancellations.Columns["EndTime"].HeaderText = "Giờ kết thúc";
                        dgvCancellations.Columns["EndTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dgvCancellations.Columns["CancelTime"].HeaderText = "Thời gian hủy";
                        dgvCancellations.Columns["CancelTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dgvCancellations.Columns["PenaltyPrice"].HeaderText = "Phí phạt";
                        dgvCancellations.Columns["PenaltyPrice"].DefaultCellStyle.Format = "N0";
                        dgvCancellations.Columns["Status"].HeaderText = "Trạng thái";
                        dgvCancellations.Columns["HoursBeforeStart"].HeaderText = "Giờ trước khi bắt đầu";
                        
                        // Hide CenterID column
                        if (dgvCancellations.Columns.Contains("CenterID"))
                        {
                            dgvCancellations.Columns["CenterID"].Visible = false;
                        }
                    }
                    
                    // Hiển thị thông tin center
                    if (!string.IsNullOrEmpty(_centerID) && dt.Rows.Count > 0)
                    {
                        this.Text = $"Phiếu Hủy - Center: {_centerID} ({dt.Rows.Count} phiếu)";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phiếu hủy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtBookingID.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadCancellationForms();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Sử dụng stored procedure sp_Cashier_SearchCancellation
                    SqlCommand cmd = new SqlCommand("sp_Cashier_SearchCancellation", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchText", searchText);
                    
                    // TRUYỀN CENTERID
                    cmd.Parameters.AddWithValue("@CenterID", !string.IsNullOrEmpty(_centerID) ? (object)_centerID : DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        dgvCancellations.DataSource = dt;
                        
                        // Nếu chỉ có 1 kết quả, tự động điền thông tin
                        if (dt.Rows.Count == 1)
                        {
                            txtCancelFormID.Text = dt.Rows[0]["CancelFormID"].ToString();
                            txtBookingID.Text = dt.Rows[0]["BookingID"].ToString();
                            txtCustomerName.Text = dt.Rows[0]["CustomerName"].ToString();
                            
                            decimal penaltyPrice = Convert.ToDecimal(dt.Rows[0]["PenaltyPrice"]);
                            txtPenaltyFee.Text = string.Format("{0:N0} VNĐ", penaltyPrice);
                        }
                        
                        string searchInfo = $"Tìm thấy {dt.Rows.Count} phiếu hủy!";
                        if (!string.IsNullOrEmpty(_centerID))
                        {
                            searchInfo += $"\n- Center: {_centerID}";
                        }
                        if (!string.IsNullOrEmpty(_fullName))
                        {
                            searchInfo += $"\n- Thu ngân: {_fullName}";
                        }
                        
                        MessageBox.Show(searchInfo, "Kết quả tìm kiếm", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy phiếu hủy nào!\n\nTìm kiếm theo: Mã phiếu hủy, Booking ID, hoặc SĐT", 
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCancellationForms();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnProcessFee_Click(object sender, EventArgs e)
        {
            string cancelFormID = txtCancelFormID.Text.Trim();
            string bookingID = txtBookingID.Text.Trim();
            string penaltyFeeText = txtPenaltyFee.Text.Replace(" VNĐ", "").Replace(",", "").Trim();

            if (string.IsNullOrEmpty(cancelFormID))
            {
                MessageBox.Show("Vui lòng chọn phiếu hủy từ danh sách!", "Thiếu thông tin", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(bookingID))
            {
                MessageBox.Show("Booking ID không hợp lệ!", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal penaltyPrice = 0;
            if (!decimal.TryParse(penaltyFeeText, out penaltyPrice))
            {
                MessageBox.Show("Phí phạt không hợp lệ!", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Confirmation
            string confirmMessage = $"Xác nhận duyệt phiếu hủy?\n\n" +
                                   $"- Mã phiếu hủy: {cancelFormID}\n" +
                                   $"- Booking ID: {bookingID}\n" +
                                   $"- Khách hàng: {txtCustomerName.Text}\n" +
                                   $"- Phí phạt: {penaltyPrice:N0} VNĐ";
            
            if (!string.IsNullOrEmpty(_centerID))
            {
                confirmMessage += $"\n- Center: {_centerID}";
            }
            if (!string.IsNullOrEmpty(_fullName))
            {
                confirmMessage += $"\n- Thu ngân: {_fullName} ({_cashierID})";
            }
            
            DialogResult confirm = MessageBox.Show(confirmMessage, "Xác nhận duyệt phiếu hủy", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Sử dụng stored procedure sp_Cashier_ProcessCancellation
                    SqlCommand cmd = new SqlCommand("sp_Cashier_ProcessCancellation", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CancelFormID", cancelFormID);
                    cmd.Parameters.AddWithValue("@BookingID", bookingID);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable result = new DataTable();
                    adapter.Fill(result);

                    if (result.Rows.Count > 0)
                    {
                        int resultCode = Convert.ToInt32(result.Rows[0]["Result"]);
                        string message = result.Rows[0]["Message"].ToString();

                        if (resultCode == 1)
                        {
                            string successMsg = $"Duyệt phiếu hủy thành công!\n\n" +
                                              $"- Mã phiếu hủy: {cancelFormID}\n" +
                                              $"- Booking đã hủy\n" +
                                              $"- Phí phạt: {penaltyPrice:N0} VNĐ";
                            
                            if (!string.IsNullOrEmpty(_centerID))
                            {
                                successMsg += $"\n- Center: {_centerID}";
                            }
                            if (!string.IsNullOrEmpty(_fullName))
                            {
                                successMsg += $"\n- Duyệt bởi: {_fullName}";
                            }
                            
                            MessageBox.Show(successMsg, "Duyệt thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            LoadCancellationForms();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show(message, "Lỗi duyệt phiếu hủy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi duyệt phiếu hủy:\n" + ex.Message, 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvCancellations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgvCancellations.Rows[e.RowIndex];
                    txtCancelFormID.Text = row.Cells["CancelFormID"].Value.ToString();
                    txtBookingID.Text = row.Cells["BookingID"].Value.ToString();
                    txtCustomerName.Text = row.Cells["CustomerName"].Value.ToString();
                    
                    decimal penaltyPrice = Convert.ToDecimal(row.Cells["PenaltyPrice"].Value);
                    txtPenaltyFee.Text = string.Format("{0:N0} VNĐ", penaltyPrice);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi chọn phiếu hủy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            txtCancelFormID.Clear();
            txtBookingID.Clear();
            txtCustomerName.Clear();
            txtPenaltyFee.Clear();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtBookingID.Clear();
            ClearForm();
            LoadCancellationForms();
            
            string refreshMsg = "Đã làm mới danh sách phiếu hủy";
            if (!string.IsNullOrEmpty(_centerID))
            {
                refreshMsg += $"\nCenter: {_centerID}";
            }
            if (!string.IsNullOrEmpty(_fullName))
            {
                refreshMsg += $"\nThu ngân: {_fullName}";
            }
            
            MessageBox.Show(refreshMsg, "Làm mới", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dgvCancellations_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
