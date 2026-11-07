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
    public partial class Book_Court : Form
    {
        private string _courtID;
        private string _centerID;
        private string _courtType;
        private string _courtTypeName;
        private decimal _unitPrice;
        private string _customerID;
        private string _username;
        private Dictionary<string, int> _equipmentStock = new Dictionary<string, int>();

        // Các control mở rộng cho giao diện đẹp theo chuẩn template
        private TextBox _txtDuration;
        private TextBox _txtEstimatedTotal;
        private Button _btnBack;

        public Book_Court()
        {
            InitializeComponent();
            InitializeQuantityDropdown();
        }

        public Book_Court(string courtID, string centerID, string courtType, string courtTypeName, decimal unitPrice, DateTime startTime, DateTime endTime, string customerID, string username) : this()
        {
            _courtID = courtID;
            _centerID = centerID;
            _courtType = courtType;
            _courtTypeName = courtTypeName;
            _unitPrice = unitPrice;
            _customerID = customerID;
            _username = username;

            InitCourtBookingUI(startTime, endTime);
        }

        public Book_Court(string courtID, string centerID, DateTime startTime, DateTime endTime, string customerID, string username) 
            : this(courtID, centerID, "", "", 0, startTime, endTime, customerID, username)
        {
        }

        private void InitCourtBookingUI(DateTime startTime, DateTime endTime)
        {
            // Tải thông tin loại sân & đơn giá nếu chưa có
            if (string.IsNullOrEmpty(_courtType) || _unitPrice <= 0)
            {
                FetchCourtDetails();
            }

            txtCourtID.Text = _courtID;
            txtCourtID.ReadOnly = true;
            txtCourtID.BackColor = Color.WhiteSmoke;

            txtCenterID.Text = _centerID;
            txtCenterID.ReadOnly = true;
            txtCenterID.BackColor = Color.WhiteSmoke;

            dtpStartTime.Value = startTime;
            dtpEndTime.Value = endTime;

            dtpStartTime.Format = DateTimePickerFormat.Custom;
            dtpStartTime.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpEndTime.Format = DateTimePickerFormat.Custom;
            dtpEndTime.CustomFormat = "dd/MM/yyyy HH:mm";

            dtpStartTime.ValueChanged += (s, e) => UpdateCalculations();
            dtpEndTime.ValueChanged += (s, e) => UpdateCalculations();

            // Cấu hình panel chính
            panelMain.Dock = DockStyle.Fill;
            panelMain.AutoScroll = true;
            panelMain.BackColor = Color.White;

            // Ẩn textBox1 cũ
            if (textBox1 != null) textBox1.Visible = false;

            // Tiêu đề form
            Label lblTitle = new Label
            {
                Text = "ĐẶT SÂN TRỰC TUYẾN (ONLINE BOOKING)",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.ForestGreen,
                Location = new Point(25, 20),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Hệ thống quản lý thể thao ViệtSport - Đặt sân nhanh chóng, tự động & bảo mật",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(27, 55),
                AutoSize = true
            };

            panelMain.Controls.Add(lblTitle);
            panelMain.Controls.Add(lblSubtitle);

            // Cập nhật nhãn tiếng Việt cho các trường cũ
            if (label3 != null) { label3.Text = "Mã Sân:"; label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold); label3.Location = new Point(25, 177); }
            if (label8 != null) { label8.Text = "Trung Tâm:"; label8.Font = new Font("Segoe UI", 9F, FontStyle.Bold); label8.Location = new Point(480, 177); }
            if (label5 != null) { label5.Text = "Thời Gian Bắt Đầu:"; label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold); label5.Location = new Point(25, 230); }
            if (label4 != null) { label4.Text = "Thời Gian Kết Thúc:"; label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold); label4.Location = new Point(25, 286); }
            if (label6 != null) { label6.Text = "Thuê Thiết Bị:"; label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold); label6.Location = new Point(25, 340); }
            if (label7 != null) { label7.Text = "Số Lượng Thuê:"; label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold); label7.Location = new Point(480, 343); }
            if (label9 != null) { label9.Text = "Tồn Kho Hiện Có:"; label9.Font = new Font("Segoe UI", 9F, FontStyle.Bold); label9.Location = new Point(480, 386); }

            txtCourtID.Location = new Point(190, 175);
            txtCenterID.Location = new Point(600, 175);
            dtpStartTime.Location = new Point(190, 227);
            dtpEndTime.Location = new Point(190, 283);
            cbEquipment.Location = new Point(190, 338);
            cbQuantity.Location = new Point(600, 340);
            txtAvailableQuantity.Location = new Point(600, 383);

            // Thêm trường hiển thị Loại sân & Đơn giá
            Label lblCT = new Label { Text = "Loại Sân:", Location = new Point(25, 125), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            TextBox txtCT = new TextBox
            {
                Text = string.IsNullOrEmpty(_courtTypeName) ? _courtType : $"{_courtTypeName} ({_courtType})",
                Location = new Point(190, 122),
                Size = new Size(200, 23),
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };

            Label lblUnitPrice = new Label { Text = "Đơn Giá / Giờ:", Location = new Point(480, 125), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            TextBox txtUP = new TextBox
            {
                Text = string.Format("{0:N0} VNĐ/giờ", _unitPrice),
                Location = new Point(600, 122),
                Size = new Size(200, 23),
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            // Thêm trường Thời lượng & Tiền sân dự tính
            Label lblDur = new Label { Text = "Thời Lượng Thuê:", Location = new Point(480, 230), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            _txtDuration = new TextBox
            {
                Location = new Point(600, 227),
                Size = new Size(200, 23),
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            Label lblEst = new Label { Text = "Dự Tính Tiền Sân:", Location = new Point(480, 286), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            _txtEstimatedTotal = new TextBox
            {
                Location = new Point(600, 283),
                Size = new Size(200, 23),
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Crimson,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // Ghi chú quy định đặt sân
            Label lblNotice = new Label
            {
                Text = "Lưu ý quy định: Phải đặt trước giờ bắt đầu tối thiểu 2 tiếng. Sân Cầu lông/Bóng rổ bội số 60p, Tennis bội số 120p, Bóng đá/Futsal bội số 90p.",
                Location = new Point(25, 420),
                AutoSize = true,
                ForeColor = Color.FromArgb(180, 100, 0),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };

            // Nút Quay Lại
            _btnBack = new Button
            {
                Text = "← Quay Lại Tra Cứu",
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(190, 455),
                Size = new Size(180, 42),
                Cursor = Cursors.Hand
            };
            _btnBack.FlatAppearance.BorderSize = 0;
            _btnBack.Click += btnBack_Click;

            // Nút Xác Nhận Đặt Sân
            btnBook.Text = "XÁC NHẬN ĐẶT SÂN & THANH TOÁN";
            btnBook.BackColor = Color.ForestGreen;
            btnBook.ForeColor = Color.White;
            btnBook.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnBook.FlatStyle = FlatStyle.Flat;
            btnBook.Location = new Point(390, 455);
            btnBook.Size = new Size(320, 42);
            btnBook.Cursor = Cursors.Hand;
            btnBook.FlatAppearance.BorderSize = 0;

            panelMain.Controls.AddRange(new Control[] { lblCT, txtCT, lblUnitPrice, txtUP, lblDur, _txtDuration, lblEst, _txtEstimatedTotal, lblNotice, _btnBack });

            cbEquipment.SelectedIndexChanged += cbEquipment_SelectedIndexChanged;
            LoadEquipment();
            UpdateCalculations();

            // Cho phép nhấn Enter để sang bước tiếp theo (xác nhận đặt sân & thanh toán)
            this.KeyPreview = true;
            this.AcceptButton = btnBook;

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnBook_Click(this, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            cbEquipment.KeyDown += enterHandler;
            cbQuantity.KeyDown += enterHandler;
            dtpStartTime.KeyDown += enterHandler;
            dtpEndTime.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        private void FetchCourtDetails()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT sc.CourtType, ct.CourtTypeName, sc.UnitPrice 
                      FROM SPORT_COURT sc 
                      LEFT JOIN COURT_TYPE ct ON sc.CourtType = ct.CourtType 
                      WHERE sc.CourtID = @CourtID", conn))
                {
                    cmd.Parameters.AddWithValue("@CourtID", _courtID);
                    conn.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            _courtType = rd["CourtType"]?.ToString();
                            _courtTypeName = rd["CourtTypeName"]?.ToString();
                            if (rd["UnitPrice"] != DBNull.Value)
                                _unitPrice = Convert.ToDecimal(rd["UnitPrice"]);
                        }
                    }
                }
            }
            catch { }
        }

        private void UpdateCalculations()
        {
            double totalMinutes = (dtpEndTime.Value - dtpStartTime.Value).TotalMinutes;
            double hours = totalMinutes / 60.0;
            if (_txtDuration != null)
            {
                _txtDuration.Text = string.Format("{0:0.#} giờ ({1} phút)", hours > 0 ? hours : 0, totalMinutes > 0 ? totalMinutes : 0);
            }
            if (_txtEstimatedTotal != null)
            {
                decimal total = (decimal)(hours > 0 ? hours : 0) * _unitPrice;
                _txtEstimatedTotal.Text = string.Format("{0:N0} VNĐ", total > 0 ? total : 0);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Customer_Main main = FindMainForm();
            if (main != null)
            {
                main.OpenViewCourt();
            }
            else
            {
                Panel parentPanel = this.Parent as Panel;
                if (parentPanel != null)
                {
                    parentPanel.Controls.Clear();
                    View_Court vc = new View_Court(_customerID);
                    vc.TopLevel = false;
                    vc.FormBorderStyle = FormBorderStyle.None;
                    vc.Dock = DockStyle.Fill;
                    parentPanel.Controls.Add(vc);
                    vc.Show();
                }
            }
        }

        private void InitializeQuantityDropdown()
        {
            cbQuantity.Items.Clear();
            for (int i = 1; i <= 10; i++)
            {
                cbQuantity.Items.Add(i.ToString());
            }
        }

        private void LoadEquipment()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand("SELECT REServiceID, EquipmentType, Quantity FROM RENTING_EQUIPMENT", conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    cbEquipment.Items.Clear();
                    cbEquipment.Items.Add("None");
                    _equipmentStock.Clear();

                    while (reader.Read())
                    {
                        string equipmentType = reader["EquipmentType"].ToString();
                        int quantity = Convert.ToInt32(reader["Quantity"]);
                        
                        cbEquipment.Items.Add(equipmentType);
                        _equipmentStock[equipmentType] = quantity;
                    }
                    cbEquipment.SelectedIndex = 0;
                    txtAvailableQuantity.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thiết bị: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbEquipment_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedEquipment = cbEquipment.SelectedItem?.ToString();
            
            if (string.IsNullOrEmpty(selectedEquipment) || selectedEquipment == "None")
            {
                txtAvailableQuantity.Text = "";
                cbQuantity.Enabled = false;
                cbQuantity.SelectedIndex = -1;
                return;
            }

            if (_equipmentStock.ContainsKey(selectedEquipment))
            {
                int availableQty = _equipmentStock[selectedEquipment];
                txtAvailableQuantity.Text = availableQty.ToString();
                cbQuantity.Enabled = true;

                cbQuantity.Items.Clear();
                int maxQty = Math.Min(availableQty, 10);
                for (int i = 1; i <= maxQty; i++)
                {
                    cbQuantity.Items.Add(i.ToString());
                }

                if (cbQuantity.Items.Count > 0)
                    cbQuantity.SelectedIndex = 0;
            }
        }

        private void btnBook_Click(object sender, EventArgs e)
        {
            DateTime startTime = dtpStartTime.Value;
            DateTime endTime = dtpEndTime.Value;

            // 1. Kiểm tra quy định đặt trước ít nhất 2 giờ
            if ((startTime - DateTime.Now).TotalMinutes < 120)
            {
                MessageBox.Show("Theo quy định của trung tâm thể thao, quý khách cần đặt sân trước giờ bắt đầu tối thiểu 2 tiếng!\nVui lòng chọn giờ bắt đầu muộn hơn.", "Quy định đặt sân", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kiểm tra giờ kết thúc sau giờ bắt đầu
            if (startTime >= endTime)
            {
                MessageBox.Show("Thời gian kết thúc phải sau thời gian bắt đầu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Kiểm tra block thời lượng phù hợp với loại sân
            int minutes = (int)(endTime - startTime).TotalMinutes;
            string cType = (_courtType ?? "").ToUpper();
            if ((cType.Contains("CAULONG") || cType.Contains("BONGRO")) && (minutes % 60 != 0))
            {
                MessageBox.Show("Sân Cầu lông / Bóng rổ phải đặt theo block tròn 60 phút (1 giờ, 2 giờ...)!", "Thời lượng không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cType.Contains("TENNIS") && (minutes % 120 != 0))
            {
                MessageBox.Show("Sân Tennis phải đặt theo block tròn 120 phút (2 giờ, 4 giờ...)!", "Thời lượng không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if ((cType.Contains("BONGDA") || cType.Contains("FUTSAL")) && (minutes % 90 != 0))
            {
                MessageBox.Show("Sân Bóng đá/Futsal phải đặt theo block 90 phút (1.5 giờ, 3 giờ...)!", "Thời lượng không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string equipmentType = cbEquipment.SelectedItem?.ToString();
            int? quantity = null;
            string serviceID = null;

            if (equipmentType != "None" && cbEquipment.SelectedIndex > 0)
            {
                if (cbQuantity.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn số lượng thiết bị thuê", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (!int.TryParse(cbQuantity.SelectedItem.ToString(), out int qty) || qty <= 0)
                {
                    MessageBox.Show("Số lượng thiết bị không hợp lệ", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                quantity = qty;

                try
                {
                    using (SqlConnection conn = Connection.GetConnection())
                    using (SqlCommand cmd = new SqlCommand("SELECT REServiceID FROM RENTING_EQUIPMENT WHERE EquipmentType = @EquipmentType", conn))
                    {
                        cmd.Parameters.AddWithValue("@EquipmentType", equipmentType);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            serviceID = result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lấy thông tin thiết bị: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            btnBook.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                string bookingID = null;

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    // 1. Tạo booking bằng Stored Procedure
                    using (SqlCommand cmd = new SqlCommand("sp_DatSanTrucTuyen", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CourtID", _courtID);
                        cmd.Parameters.AddWithValue("@CustomerID", _customerID);
                        cmd.Parameters.AddWithValue("@SportCenterID", _centerID);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);

                        if (!string.IsNullOrEmpty(serviceID))
                            cmd.Parameters.AddWithValue("@ServiceID", serviceID);
                        else
                            cmd.Parameters.AddWithValue("@ServiceID", DBNull.Value);

                        if (quantity.HasValue)
                            cmd.Parameters.AddWithValue("@Quantity", quantity.Value);
                        else
                            cmd.Parameters.AddWithValue("@Quantity", DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }

                    // 2. Lấy BookingID vừa tạo
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 BookingID 
                          FROM BOOKING_FORM 
                          WHERE CourtID = @CourtID 
                            AND CustomerID = @CustomerID 
                            AND StartTime = @StartTime 
                            AND EndTime = @EndTime 
                          ORDER BY OrderDate DESC, OrderTime DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@CourtID", _courtID);
                        cmd.Parameters.AddWithValue("@CustomerID", _customerID);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);

                        object res = cmd.ExecuteScalar();
                        if (res != null)
                            bookingID = res.ToString();
                    }

                    if (string.IsNullOrEmpty(bookingID))
                    {
                        MessageBox.Show("Không thể lấy thông tin booking vừa tạo", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 3. Mở popup thanh toán
                    Customer_Main mainForm = FindMainForm();
                    if (mainForm == null)
                    {
                        MessageBox.Show("Không thể lấy thông tin Customer_Main", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    BookCourt_PaymentForm paymentForm = new BookCourt_PaymentForm(
                        bookingID,
                        _courtID,
                        _customerID,
                        _username,
                        startTime,
                        mainForm.Level,
                        mainForm.BalanceValue
                    );

                    paymentForm.TopLevel = true;
                    paymentForm.ShowInTaskbar = true;
                    paymentForm.StartPosition = FormStartPosition.CenterScreen;
                    
                    DialogResult result = paymentForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // Chuyển hướng người dùng sang trang xem các sân đã đặt
                        mainForm.OpenBookedCourts();
                        this.Close();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Lỗi đặt sân", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBook.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private Customer_Main FindMainForm()
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
