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

namespace CASHIER
{
    public partial class Cashier_Payment : Form
    {
        private string connectionString = DatabaseConfig.ConnectionString;

        // Thông tin đăng nhập
        private string _cashierID;
        private string _employeeID;
        private string _fullName;
        private string _username;
        private string _centerID; // THÊM CENTERID

        public Cashier_Payment()
        {
            InitializeComponent();
            
            // Đổi label tìm kiếm sang "SĐT KH:"
            label1.Text = "SĐT KH:";
            
            cboStatus.SelectedIndex = 0; // Select "Tất cả" by default
        }

        // Constructor nhận thông tin đăng nhập VÀ CENTERID
        public Cashier_Payment(string employeeID, string fullName, string username, string centerID) : this()
        {
            _employeeID = employeeID;
            _fullName = fullName;
            _username = username;
            _cashierID = username;
            _centerID = centerID; // LƯU CENTERID
            
            LoadAllInvoices();
        }

        private void LoadAllInvoices()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_Cashier_GetAllInvoices", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // TRUYỀN CENTERID
                    cmd.Parameters.AddWithValue("@CenterID", !string.IsNullOrEmpty(_centerID) ? (object)_centerID : DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvInvoices.DataSource = dt;

                    // Format columns
                    if (dgvInvoices.Columns.Count > 0)
                    {
                        dgvInvoices.Columns["InvoiceID"].HeaderText = "Mã hóa đơn";
                        dgvInvoices.Columns["BookingID"].HeaderText = "Mã đặt sân";
                        dgvInvoices.Columns["CustomerName"].HeaderText = "Khách hàng";
                        dgvInvoices.Columns["TotalPrice"].HeaderText = "Tổng tiền";
                        dgvInvoices.Columns["TotalPrice"].DefaultCellStyle.Format = "N0";
                        dgvInvoices.Columns["PaymentMethod"].HeaderText = "Phương thức TT";
                        dgvInvoices.Columns["CreateTime"].HeaderText = "Ngày tạo";
                        dgvInvoices.Columns["CreateTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dgvInvoices.Columns["Status"].HeaderText = "Trạng thái";
                        dgvInvoices.Columns["CurrentStatus"].HeaderText = "TT Hiện tại";
                        dgvInvoices.Columns["PaymentDeadline"].HeaderText = "Hạn chốt";
                        dgvInvoices.Columns["PaymentDeadline"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                        // Hide unnecessary columns
                        if (dgvInvoices.Columns.Contains("StartTime"))
                        {
                            dgvInvoices.Columns["StartTime"].Visible = false;
                        }
                        if (dgvInvoices.Columns.Contains("CenterID"))
                        {
                            dgvInvoices.Columns["CenterID"].Visible = false;
                        }

                        // Color code by status
                        foreach (DataGridViewRow row in dgvInvoices.Rows)
                        {
                            string status = row.Cells["CurrentStatus"].Value?.ToString();
                            if (status == "Paid")
                            {
                                row.DefaultCellStyle.BackColor = Color.LightGreen;
                            }
                            else if (status == "Overdue")
                            {
                                row.DefaultCellStyle.BackColor = Color.LightCoral;
                            }
                            else if (status == "Pending")
                            {
                                row.DefaultCellStyle.BackColor = Color.LightYellow;
                            }
                            else if (status == "Processing")
                            {
                                row.DefaultCellStyle.BackColor = Color.LightBlue;
                            }
                            else if (status == "Cancelled")
                            {
                                row.DefaultCellStyle.BackColor = Color.LightGray;
                            }
                        }
                    }
                    
                    // Hiển thị thông tin center
                    if (!string.IsNullOrEmpty(_centerID) && dt.Rows.Count > 0)
                    {
                        this.Text = $"Thanh Toán - Center: {_centerID} ({dt.Rows.Count} hóa đơn)";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi tải danh sách hóa đơn:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string phoneNumber = txtSearchInvoice.Text.Trim();
                string statusFilter = cboStatus.Text;
                
                // Mapping status tiếng Việt sang database
                string dbStatus = "All";
                switch (statusFilter)
                {
                    case "Tất cả":
                        dbStatus = "All";
                        break;
                    case "Chưa thanh toán":
                        dbStatus = "Chưa thanh toán";
                        break;
                    case "Đã thanh toán":
                        dbStatus = "Đã thanh toán";
                        break;
                    case "Hủy":
                        dbStatus = "Hủy";
                        break;
                    case "Đang xử lý":
                        dbStatus = "Đang xử lý";
                        break;
                    case "Quá hạn":
                        dbStatus = "Overdue";
                        break;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Sử dụng stored procedure sp_Cashier_SearchInvoiceByPhoneStatus
                    SqlCommand cmd = new SqlCommand("sp_Cashier_SearchInvoiceByPhoneStatus", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    // Truyền parameters
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@PhoneNumber", DBNull.Value);
                    }
                    
                    cmd.Parameters.AddWithValue("@StatusFilter", dbStatus);
                    
                    // TRUYỀN CENTERID
                    cmd.Parameters.AddWithValue("@CenterID", !string.IsNullOrEmpty(_centerID) ? (object)_centerID : DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvInvoices.DataSource = dt;

                    // Reapply formatting
                    ApplyGridFormatting();
                    
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy hóa đơn nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string searchInfo = $"Tìm thấy {dt.Rows.Count} hóa đơn";
                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            searchInfo += $"\n- SĐT: {phoneNumber}";
                        }
                        if (statusFilter != "Tất cả")
                        {
                            searchInfo += $"\n- Trạng thái: {statusFilter}";
                        }
                        if (!string.IsNullOrEmpty(_centerID))
                        {
                            searchInfo += $"\n- Center: {_centerID}";
                        }
                        if (!string.IsNullOrEmpty(_fullName))
                        {
                            searchInfo += $"\n- Thu ngân: {_fullName}";
                        }
                        
                        MessageBox.Show(searchInfo, "Kết quả tìm kiếm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearchInvoice.Clear();
            cboStatus.SelectedIndex = 0;
            LoadAllInvoices();
            
            string refreshMsg = "Đã làm mới danh sách hóa đơn";
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

        private void btnCheckDeadline_Click(object sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dgvInvoices.SelectedRows[0];
            string invoiceID = selectedRow.Cells["InvoiceID"].Value.ToString();
            string status = selectedRow.Cells["Status"].Value.ToString();
            DateTime paymentDeadline = Convert.ToDateTime(selectedRow.Cells["PaymentDeadline"].Value);

            if (status == "Đã thanh toán")
            {
                MessageBox.Show("Hóa đơn đã được thanh toán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TimeSpan remaining = paymentDeadline - DateTime.Now;

            string message = "";
            MessageBoxIcon icon = MessageBoxIcon.Information;

            if (remaining.TotalHours < 0)
            {
                message = $"⚠ Hóa đơn {invoiceID} đã quá hạn thanh toán!\n\n";
                message += $"- Hạn chót: {paymentDeadline:dd/MM/yyyy HH:mm}\n";
                message += $"- Quá hạn: {Math.Abs(remaining.Hours)} giờ {Math.Abs(remaining.Minutes)} phút";
                icon = MessageBoxIcon.Warning;
            }
            else if (remaining.TotalHours < 1)
            {
                message = $"⏰ Hóa đơn {invoiceID} sắp đến hạn thanh toán!\n\n";
                message += $"- Hạn chót: {paymentDeadline:dd/MM/yyyy HH:mm}\n";
                message += $"- Còn lại: {remaining.Minutes} phút";
                icon = MessageBoxIcon.Warning;
            }
            else
            {
                message = $"Hóa đơn {invoiceID}\n\n";
                message += $"- Hạn chót: {paymentDeadline:dd/MM/yyyy HH:mm}\n";
                message += $"- Còn lại: {(int)remaining.TotalHours} giờ {remaining.Minutes} phút";
                icon = MessageBoxIcon.Information;
            }

            if (!string.IsNullOrEmpty(_fullName))
            {
                message += $"\n- Kiểm tra bởi: {_fullName}";
            }

            MessageBox.Show(message, "Kiểm tra Hạn Thanh Toán", MessageBoxButtons.OK, icon);
        }

        private void dgvInvoices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgvInvoices.Rows[e.RowIndex];
                    
                    // Hiển thị tổng tiền
                    decimal totalPrice = Convert.ToDecimal(row.Cells["TotalPrice"].Value);
                    txtTotalPrice.Text = string.Format("{0:N0} VNĐ", totalPrice);
                    
                    // Hiển thị trạng thái
                    string currentStatus = row.Cells["CurrentStatus"].Value.ToString();
                    string dbStatus = row.Cells["Status"].Value.ToString();
                    
                    // Mapping status sang tiếng Việt để hiển thị
                    string displayStatus = dbStatus;
                    switch (dbStatus)
                    {
                        case "Chưa thanh toán":
                            displayStatus = "Chưa thanh toán";
                            txtStatus.ForeColor = Color.Orange;
                            break;
                        case "Đã thanh toán":
                            displayStatus = "Đã thanh toán";
                            txtStatus.ForeColor = Color.Green;
                            break;
                        case "Hủy":
                            displayStatus = "Đã hủy";
                            txtStatus.ForeColor = Color.Gray;
                            break;
                        case "Đang xử lý":
                            displayStatus = "Đang xử lý";
                            txtStatus.ForeColor = Color.Blue;
                            break;
                        default:
                            txtStatus.ForeColor = Color.Black;
                            break;
                    }
                    
                    if (currentStatus == "Overdue")
                    {
                        displayStatus += " (Quá hạn)";
                        txtStatus.ForeColor = Color.Red;
                    }
                    
                    txtStatus.Text = displayStatus;
                    
                    // Hiển thị phương thức thanh toán
                    txtPaymentMethod.Text = row.Cells["PaymentMethod"].Value?.ToString() ?? "Chưa chọn";
                    
                    // Hiển thị deadline
                    DateTime deadline = Convert.ToDateTime(row.Cells["PaymentDeadline"].Value);
                    TimeSpan remaining = deadline - DateTime.Now;
                    
                    if (remaining.TotalHours < 0)
                    {
                        txtDeadline.Text = $"{deadline:dd/MM/yyyy HH:mm} (Quá hạn {Math.Abs((int)remaining.TotalHours)}h)";
                        txtDeadline.ForeColor = Color.Red;
                    }
                    else if (remaining.TotalHours < 1)
                    {
                        txtDeadline.Text = $"{deadline:dd/MM/yyyy HH:mm} (Còn {remaining.Minutes}m)";
                        txtDeadline.ForeColor = Color.Orange;
                    }
                    else
                    {
                        txtDeadline.Text = $"{deadline:dd/MM/yyyy HH:mm} (Còn {(int)remaining.TotalHours}h {remaining.Minutes}m)";
                        txtDeadline.ForeColor = Color.Green;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi hiển thị chi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyGridFormatting()
        {
            if (dgvInvoices.Columns.Count > 0)
            {
                dgvInvoices.Columns["TotalPrice"].DefaultCellStyle.Format = "N0";
                dgvInvoices.Columns["CreateTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                dgvInvoices.Columns["PaymentDeadline"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                if (dgvInvoices.Columns.Contains("StartTime"))
                {
                    dgvInvoices.Columns["StartTime"].Visible = false;
                }
                
                if (dgvInvoices.Columns.Contains("CenterID"))
                {
                    dgvInvoices.Columns["CenterID"].Visible = false;
                }

                // Color code by status
                foreach (DataGridViewRow row in dgvInvoices.Rows)
                {
                    string status = row.Cells["CurrentStatus"].Value?.ToString();
                    if (status == "Paid")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                    }
                    else if (status == "Overdue")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                    else if (status == "Pending")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                    else if (status == "Processing")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    else if (status == "Cancelled")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                    }
                }
            }
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
