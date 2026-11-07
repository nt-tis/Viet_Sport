using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RECEPTIONIST
{
    public partial class Receiptionist_Mainform : Form
    {
        private Form currentChildForm;
        private Color sidebarDefaultColor;

        // New fields to support ViewListBooking + search
        private DataTable currentBookings;
        private DataGridView dgvAllBookings;
        private DateTimePicker searchDatePicker;
        private Button searchButton;
        private Button clearSearchButton;

        private readonly string _username;    // MN0001
        private readonly string _employeeId;  // EMP0001 (nếu có)
        private readonly string _fullName;
        private readonly string _role;


        public Receiptionist_Mainform()
        {
            InitializeComponent();

            // capture default color so we can restore when switching selection
            sidebarDefaultColor = button9?.BackColor ?? panelMenu.BackColor;

            // wire Service button (button1) to open Receiptionist_Service
            this.button1.Click += new EventHandler(this.button1_Click);

            // wire Log out (button2)
            this.button2.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(
                    "Bạn chắc chắn muốn đăng xuất không?",
                    "Xác nhận đăng xuất",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (confirm == DialogResult.Yes)
                {
                    this.Close();
                }
            };

            // wire Change Password (button3)
            this.button3.Click += (s, e) =>
            {
                OpenChildFormInPanel(new Receptionist_ChangePassword(_employeeId, _fullName, _username));
            };

            // wire Payroll (button4)
            this.button4.Click += (s, e) =>
            {
                OpenChildFormInPanel(new Receptionist_Payroll(_employeeId, _fullName, _username));
            };

            // wire Leave Application (button5)
            this.button5.Click += (s, e) =>
            {
                OpenChildFormInPanel(new Receptionist_LeaveApplication(_employeeId, _fullName, _username));
            };
        }

        private void ApplyUserInfo()
        {
            // Demo/fallback nếu login chưa truyền
            EmployeeID.Text = string.IsNullOrWhiteSpace(_employeeId) ? "EMP0001" : _employeeId;
            Username.Text = string.IsNullOrWhiteSpace(_username) ? "MN0001" : _username;
            EmployeeName.Text = string.IsNullOrWhiteSpace(_fullName) ? "N/A" : _fullName;
            Role.Text = string.IsNullOrWhiteSpace(_role) ? "N/A" : _role;
        }

        public Receiptionist_Mainform(string employeeId, string fullName, string role, string username) : this()
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _role = role;
            _username = username;
            ApplyUserInfo();
        }

        // Open and host a child Form inside panelContent, keeping main menu intact.
        // Prefer embedding the child's ContentPanel (if exposed) instead of the full Form.
        private void OpenChildFormInPanel(Form child)
        {
            if (child == null) return;

            // dispose previous child and remove hosted content
            if (currentChildForm != null && currentChildForm != child)
            {
                try
                {
                    // If previous child exposed a ContentPanel that was moved into panelContent,
                    // it will be removed below as we clear panelContent.
                    currentChildForm.Close();
                    currentChildForm.Dispose();
                }
                catch { }
                currentChildForm = null;
                panelContent.Controls.Clear();
            }

            currentChildForm = child;

            // Try to embed the child's ContentPanel (preferred)
            try
            {
                var pi = child.GetType().GetProperty("ContentPanel");
                if (pi != null)
                {
                    var content = pi.GetValue(child) as Panel;
                    if (content != null)
                    {
                        // Detach from child (if still parented)
                        if (content.Parent != null && content.Parent != panelContent)
                        {
                            // remove from its current parent (the child form)
                            content.Parent.Controls.Remove(content);
                        }

                        content.Dock = DockStyle.Fill;
                        panelContent.Controls.Clear();
                        panelContent.Controls.Add(content);

                        // ensure visibility
                        content.Visible = true;

                        // Keep the child instance alive for event wiring
                        currentChildForm = child;

                        // Call PrepareEmbeddedMode on child if it exists (some forms do this in ctor)
                        var prep = child.GetType().GetMethod("PrepareEmbeddedMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        prep?.Invoke(child, null);

                        return;
                    }
                }
            }
            catch
            {
                // fallback to embedding the whole form below
            }

            // Fallback: embed the whole form
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;

            panelContent.Controls.Clear();
            panelContent.Controls.Add(child);
            child.Show();
        }

        // Reset sidebar buttons to default and highlight the selected one orange.
        private void HighlightSidebarSelection(Control selected)
        {
            foreach (Control c in panelMenu.Controls.OfType<Button>())
            {
                try
                {
                    c.BackColor = sidebarDefaultColor;
                }
                catch { }
            }

            if (selected is Button btn)
            {
                try { btn.BackColor = Color.Orange; } catch { }
            }
        }

        // Court button click: host Receptionist_court inside panelContent and highlight the button.
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                var courtForm = new RECEPTIONIST.Receptionist_court();
                OpenChildFormInPanel(courtForm);
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Court: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Booking Form button click: host Receiptionist_BookingForm inside panelContent and highlight the button.
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                // Use the embedded constructor so the booking form prepares its ContentPanel
                var bookingForm = new RECEPTIONIST.Receiptionist_BookingForm(true);
                OpenChildFormInPanel(bookingForm);
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Booking Form: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Cancellation Form button click: host Receiptionist_CancellationForm inside panelContent and highlight the button.
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                // Use embedded ctor so the child prepares its ContentPanel for hosting
                var cancelForm = new RECEPTIONIST.Receiptionist_CancellationForm(true);
                OpenChildFormInPanel(cancelForm);
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Cancellation Form: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // NEW: Service button click: open Receiptionist_Service and keep menu
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var serviceForm = new RECEPTIONIST.Receiptionist_Service();
                OpenChildFormInPanel(serviceForm);
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Service: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ViewListBooking button handler: show a DataGridView of all bookings inside panelContent
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                panelContent.Controls.Clear();
                currentChildForm = null;

                // Fetch booking data once and keep for filtering
                currentBookings = DatabaseHelper.ExecuteStoredProcedure("SP_GetAllBookings");

                // Create search toolbar (OrderDate only)
                var toolbar = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40, // a bit taller so toolbar won't overlap header
                    Padding = new Padding(6),
                    BackColor = panelContent.BackColor
                };

                var lblDate = new Label { Text = "Order Date:", AutoSize = true, Location = new Point(8, 10) };
                searchDatePicker = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Short,
                    Size = new Size(110, 24),
                    Location = new Point(lblDate.Right + 8, 8)
                };

                searchButton = new Button
                {
                    Text = "Search",
                    BackColor = Color.Green,
                    ForeColor = Color.White,
                    Size = new Size(80, 24),
                    Location = new Point(searchDatePicker.Right + 12, 8)
                };
                searchButton.Click += (s, ea) => ApplyFilter();

                clearSearchButton = new Button
                {
                    Text = "Clear",
                    Size = new Size(60, 24),
                    Location = new Point(searchButton.Right + 8, 8)
                };
                clearSearchButton.Click += (s, ea) => { ResetFilter(); };

                var btnCheckIn = new Button
                {
                    Text = "Check-in Sân",
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Size = new Size(110, 24),
                    Location = new Point(clearSearchButton.Right + 12, 8)
                };
                btnCheckIn.Click += (s, ea) => HandleCheckInSelectedBooking();

                var btnComplete = new Button
                {
                    Text = "Hoàn Tất Phiếu",
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Size = new Size(125, 24),
                    Location = new Point(btnCheckIn.Right + 8, 8)
                };
                btnComplete.Click += (s, ea) => HandleCompleteSelectedBooking();

                // Add controls to toolbar
                toolbar.Controls.Add(lblDate);
                toolbar.Controls.Add(searchDatePicker);
                toolbar.Controls.Add(searchButton);
                toolbar.Controls.Add(clearSearchButton);
                toolbar.Controls.Add(btnCheckIn);
                toolbar.Controls.Add(btnComplete);

                // Create DataGridView
                dgvAllBookings = new DataGridView
                {
                    Name = "dgvAllBookings",
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AutoGenerateColumns = true,
                    DataSource = currentBookings,
                    BorderStyle = BorderStyle.None
                };

                // Handle data errors to avoid default exception dialog on formatting/parsing issues
                dgvAllBookings.DataError += (s, ea) =>
                {
                    // suppress the default dialog (log or show friendly message if needed)
                    ea.ThrowException = false;
                };

                dgvAllBookings.DataBindingComplete += (s, ea) =>
                {
                    SafeFormatColumn(dgvAllBookings, currentBookings, "OrderDate", "dd/MM/yyyy");
                    SafeFormatColumn(dgvAllBookings, currentBookings, "StartTime", "dd/MM/yyyy HH:mm");
                    SafeFormatColumn(dgvAllBookings, currentBookings, "EndTime", "dd/MM/yyyy HH:mm");
                    dgvAllBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                };

                // IMPORTANT: Add toolbar first (Dock = Top), then add DataGridView (Dock = Fill)
                // so DataGridView's header is not overlaid by the toolbar.

                // Create TableLayoutPanel to host toolbar (auto-sized) and grid (fills remaining space)
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                };
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // toolbar row
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // grid row

                // Add toolbar and grid into layout
                layout.Controls.Add(toolbar, 0, 0);
                layout.Controls.Add(dgvAllBookings, 0, 1);

                // Clear and add layout into panelContent
                panelContent.Controls.Add(layout);

                // Highlight sidebar
                HighlightSidebarSelection(sender as Control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách đặt sân: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Apply filter from searchDatePicker to currentBookings and rebind grid (OrderDate only)
        private void ApplyFilter()
        {
            if (currentBookings == null || currentBookings.Rows.Count == 0 || dgvAllBookings == null) return;

            DateTime? dateFilter = null;

            try
            {
                dateFilter = searchDatePicker?.Value.Date;
            }
            catch { dateFilter = null; }

            var rows = currentBookings.AsEnumerable();

            if (dateFilter.HasValue)
            {
                rows = rows.Where(r =>
                {
                    var obj = r.Table.Columns.Contains("OrderDate") ? r["OrderDate"] : null;
                    DateTime dt;
                    if (obj == null || obj == DBNull.Value) return false;
                    if (DateTime.TryParse(obj.ToString(), out dt))
                        return dt.Date == dateFilter.Value.Date;
                    return false;
                });
            }

            DataTable filtered;
            try
            {
                filtered = rows.Any() ? rows.CopyToDataTable() : currentBookings.Clone();
            }
            catch
            {
                filtered = currentBookings.Clone();
            }

            dgvAllBookings.DataSource = filtered;
        }

        // Reset filter and show all rows
        private void ResetFilter()
        {
            if (dgvAllBookings == null || currentBookings == null) return;
            dgvAllBookings.DataSource = currentBookings;
        }

        // Safe formatter: only set DefaultCellStyle.Format when underlying data type supports it
        private void SafeFormatColumn(DataGridView dgv, DataTable table, string columnName, string format)
        {
            try
            {
                // Prefer checking DataTable column type if available
                if (table != null && table.Columns.Contains(columnName))
                {
                    var colType = table.Columns[columnName].DataType;
                    if (colType == typeof(DateTime) || colType == typeof(TimeSpan))
                    {
                        if (dgv.Columns.Contains(columnName))
                            dgv.Columns[columnName].DefaultCellStyle.Format = format;
                    }
                    return;
                }

                // Fallback: check DataGridViewColumn.ValueType
                if (dgv.Columns.Contains(columnName))
                {
                    var valueType = dgv.Columns[columnName].ValueType;
                    if (valueType == typeof(DateTime) || valueType == typeof(TimeSpan))
                    {
                        dgv.Columns[columnName].DefaultCellStyle.Format = format;
                    }
                }
            }
            catch
            {
                // ignore formatting failures
            }
        }

        private void panelContent_Paint(object sender, PaintEventArgs e)
        {

        }

        // Check-in selected booking
        private void HandleCheckInSelectedBooking()
        {
            if (dgvAllBookings == null || dgvAllBookings.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn 1 phiếu đặt sân trên bảng để Check-in!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var row = dgvAllBookings.CurrentRow;
                string bookingId = row.Cells["BookingID"]?.Value?.ToString();
                string status = row.Cells["Status"]?.Value?.ToString();

                if (string.IsNullOrEmpty(bookingId))
                {
                    MessageBox.Show("Không tìm thấy mã Booking hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (status == "InUse")
                {
                    MessageBox.Show("Khách hàng đã Check-in sân này trước đó!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (status == "Cancelled")
                {
                    MessageBox.Show("Phiếu đặt đã bị hủy trước đó (quá hạn hoặc khách hủy)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult dr = MessageBox.Show("Xác nhận làm thủ tục Check-in cho phiếu đặt " + bookingId + "?", "Xác nhận Check-in", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;

                var param = new SqlParameter[] {
                    new SqlParameter("@BookingID", SqlDbType.NVarChar, 50) { Value = bookingId }
                };

                DatabaseHelper.ExecuteStoredProcedure("sp_Receptionist_CheckInBooking", param);
                MessageBox.Show("Check-in thành công cho phiếu đặt " + bookingId + "! Sân chuyển sang trạng thái Đang sử dụng (InUse).", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reload danh sách
                currentBookings = DatabaseHelper.ExecuteStoredProcedure("SP_GetAllBookings");
                ApplyFilter();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(sqlEx.Message, "Quy định Check-in", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                currentBookings = DatabaseHelper.ExecuteStoredProcedure("SP_GetAllBookings");
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện Check-in: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hoan tat phieu dat san va tra dung cu
        private void HandleCompleteSelectedBooking()
        {
            if (dgvAllBookings == null || dgvAllBookings.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn 1 phiếu đặt sân trên bảng để hoàn tất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var row = dgvAllBookings.CurrentRow;
                string bookingId = row.Cells["BookingID"]?.Value?.ToString();
                string status = row.Cells["Status"]?.Value?.ToString();

                if (string.IsNullOrEmpty(bookingId))
                {
                    MessageBox.Show("Không tìm thấy mã Booking hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (status == "Completed")
                {
                    MessageBox.Show("Phiếu đặt sân này đã hoàn tất trước đó!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (status == "Cancelled")
                {
                    MessageBox.Show("Phiếu đặt đã bị hủy trước đó!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult dr = MessageBox.Show("Xác nhận hoàn tất dịch vụ và trả sân cho phiếu đặt " + bookingId + "?", "Xác nhận Hoàn tất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;

                var param = new SqlParameter[] {
                    new SqlParameter("@BookingID", SqlDbType.NVarChar, 50) { Value = bookingId }
                };

                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_Receptionist_CompleteBooking", param);

                string receiptDetails = "PHIẾU ĐẶT SÂN HOÀN TẤT\n" +
                                        "----------------------------------------\n" +
                                        "- Mã phiếu: " + bookingId + "\n" +
                                        "- Trạng thái: Hoàn tất (Completed)\n" +
                                        "- Thiết bị thuê: Đã thu hồi và kiểm kê đầy đủ\n" +
                                        "- Sân: Sẵn sàng phục vụ lượt tiếp theo\n" +
                                        "----------------------------------------\n" +
                                        "Cảm ơn quý khách đã sử dụng dịch vụ tại VietSport!";

                if (dt != null && dt.Rows.Count > 0)
                {
                    var r = dt.Rows[0];
                    string custName = r["CustomerName"]?.ToString() ?? "N/A";
                    string courtId = r["CourtID"]?.ToString() ?? "N/A";
                    string courtType = r["CourtType"]?.ToString() ?? "N/A";
                    string invId = r["InvoiceID"]?.ToString() ?? "N/A";
                    decimal tot = 0;
                    if (r["TotalPrice"] != DBNull.Value) decimal.TryParse(r["TotalPrice"].ToString(), out tot);

                    receiptDetails = "PHIẾU ĐẶT SÂN HOÀN TẤT\n" +
                                     "----------------------------------------\n" +
                                     "- Mã phiếu đặt: " + bookingId + "\n" +
                                     "- Mã hóa đơn: " + invId + "\n" +
                                     "- Khách hàng: " + custName + "\n" +
                                     "- Sân thi đấu: " + courtId + " (" + courtType + ")\n" +
                                     "- Tổng chi phí: " + tot.ToString("N0") + " VNĐ\n" +
                                     "- Trạng thái: Hoàn tất (Completed)\n" +
                                     "- Thiết bị: Đã bàn giao và nhập kho đầy đủ\n" +
                                     "- Lễ tân xác nhận: " + (_fullName ?? _username ?? "Lễ tân") + "\n" +
                                     "----------------------------------------\n" +
                                     "Dịch vụ đã hoàn tất thành công!";
                }

                MessageBox.Show(receiptDetails, "Phiếu Đặt Sân Hoàn Tất", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reload danh sách
                currentBookings = DatabaseHelper.ExecuteStoredProcedure("SP_GetAllBookings");
                ApplyFilter();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(sqlEx.Message, "Quy định Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                currentBookings = DatabaseHelper.ExecuteStoredProcedure("SP_GetAllBookings");
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện hoàn tất phiếu đặt: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
