// Code-behind partial class: single place for logic, event handlers and helpers.
// Keep only one definition of each method here.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace RECEPTIONIST
{
    public partial class Receiptionist_BookingForm : Form
    {
        public Receiptionist_BookingForm()
        {
            InitializeComponent();

            if (!this.DesignMode)
            {
                numericUpDownQuantity.Minimum = 0;
                numericUpDownQuantity.Maximum = 100;
                numericUpDownQuantity.Value = 1;

                // Chỉ khôi phục fields nếu thực sự thiếu (tránh Designer bị crash)
                EnsureBookingFields();
            }
        }

        // Events host can subscribe to (request navigation)
        public event EventHandler ShowCourtRequested;
        public event EventHandler ShowCancellationRequested;

        public Receiptionist_BookingForm(bool embedded) : this()
        {
            if (embedded) PrepareEmbeddedMode();
        }

        private void PrepareEmbeddedMode()
        {
            try
            {
                if (this.ContentPanel != null)
                {
                    this.ContentPanel.Dock = DockStyle.Fill;
                    this.ContentPanel.Location = new Point(0, 0);
                }

                this.FormBorderStyle = FormBorderStyle.None;
                this.BackColor = SystemColors.Control;
                this.Padding = new Padding(0);
            }
            catch { }
        }

        // Expose panels so host can embed panels if needed
        public Panel SidebarPanel => this.panel2;
        public Panel ContentPanel => this.panelMain;
        public Panel TopBarPanel => this.panel1;

        // Simple no-op designer handlers (one place only)
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void panel1_Click_noop(object sender, EventArgs e) { }
        private void button5_Click_3(object sender, EventArgs e) { ShowCourtRequested?.Invoke(this, EventArgs.Empty); }
        private void button19_Click(object sender, EventArgs e) { ShowCancellationRequested?.Invoke(this, EventArgs.Empty); }
        private void button3_Click(object sender, EventArgs e) { }
        private void button4_Click(object sender, EventArgs e) { }
        private void button11_Click(object sender, EventArgs e) { }

        // Main booking button handler: builds parameters and calls stored-proc
        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                string courtId = textBoxCourtID.Text.Trim();
                string customerId = textBoxCustomerID.Text.Trim();
                string sportCenterId = textBoxSportCenterID.Text.Trim();
                string rid = textBoxRID.Text.Trim();

                DateTime startDate = dateTimePicker1.Value.Date;
                TimeSpan startTime = dateTimePicker2.Value.TimeOfDay;
                TimeSpan endTime = dateTimePicker3.Value.TimeOfDay;

                string serviceId = textBoxServiceID.Text.Trim();
                int quantity = (int)numericUpDownQuantity.Value;

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CourtID", SqlDbType.NVarChar, 50) { Value = string.IsNullOrEmpty(courtId) ? (object)DBNull.Value : courtId },
                    new SqlParameter("@CustomerID", SqlDbType.NVarChar, 50) { Value = string.IsNullOrEmpty(customerId) ? (object)DBNull.Value : customerId },
                    new SqlParameter("@SportCenterID", SqlDbType.NVarChar, 50) { Value = string.IsNullOrEmpty(sportCenterId) ? (object)DBNull.Value : sportCenterId },
                    new SqlParameter("@RID", SqlDbType.NVarChar, 50) { Value = string.IsNullOrEmpty(rid) ? (object)DBNull.Value : rid },
                    new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate },
                    new SqlParameter("@StartTime_In", SqlDbType.Time) { Value = startTime },
                    new SqlParameter("@EndTime_In", SqlDbType.Time) { Value = endTime },
                    new SqlParameter("@ServiceID", SqlDbType.NVarChar, 50) { Value = string.IsNullOrEmpty(serviceId) ? (object)DBNull.Value : serviceId },
                    new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity }
                };

                // Add return value parameter (Direction = ReturnValue)
                var returnParam = new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                parameters.Add(returnParam);

                // Execute stored procedure using existing DatabaseHelper helper
                DatabaseHelper.ExecuteStoredProcedure("sp_Receptionist_Booking_Create", parameters.ToArray());

                // Read return value if set
                int retVal = 0;
                if (returnParam.Value != null && returnParam.Value != DBNull.Value)
                {
                    int.TryParse(returnParam.Value.ToString(), out retVal);
                }

                // The stored procedure now returns 1 on success, negative values for specific errors.
                if (retVal == 1)
                {
                    MessageBox.Show("Đặt sân thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Map return codes from the updated stored procedure to friendly messages
                string userMessage;
                switch (retVal)
                {
                    case -1:
                        userMessage = "Lỗi: Giờ bắt đầu phải nhỏ hơn giờ kết thúc!";
                        break;
                    case -2:
                        userMessage = "Lỗi: Mã khách hàng không tồn tại!";
                        break;
                    case -3:
                        userMessage = "Lỗi: Mã trung tâm thể thao không tồn tại!";
                        break;
                    case -4:
                        userMessage = "Lỗi: Mã sân không tồn tại!";
                        break;
                    case -5:
                        userMessage = "Lỗi: Sân được chọn không thuộc Trung tâm thể thao này!";
                        break;
                    case -6:
                        userMessage = "Lỗi: Mã nhân viên không tồn tại hoặc không có quyền Lễ tân!";
                        break;
                    case -7:
                        userMessage = "Lỗi: Mã dịch vụ không tồn tại!";
                        break;
                    case -8:
                        userMessage = "Lỗi: Dịch vụ này hiện đang ngừng hoạt động!";
                        break;
                    case -9:
                        userMessage = "Lỗi: Số lượng dịch vụ phải lớn hơn 0!";
                        break;
                    case -10:
                        userMessage = "Lỗi: Sân này đã được đặt trong khoảng thời gian bạn chọn!";
                        break;
                    case -11:
                        userMessage = "Lỗi: Sân đang bảo trì, không thể đặt!";
                        break;
                    case -12:
                        userMessage = "LỖI: Trạng thái sân đã bị thay đổi trong khi đang xử lý (Unrepeatable Read). Vui lòng thử lại.";
                        break;
                    case -13:
                        userMessage = "Lỗi: Không tìm thấy thông tin tồn kho!";
                        break;
                    case -14:
                        userMessage = "Lỗi: Kho không đủ hàng!";
                        break;
                    default:
                        // If the helper swallowed a SqlException (DatabaseHelper shows its own message box),
                        // return a generic fallback when return value is not provided.
                        if (retVal == 0)
                            userMessage = "Không nhận được mã trả về từ Stored Procedure. Kiểm tra thông báo lỗi chi tiết.";
                        else
                            userMessage = "Lỗi không xác định. Mã lỗi: " + retVal;
                        break;
                }

                MessageBox.Show(userMessage, "Đặt sân thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException sqlEx)
            {
                // If the proc used RAISERROR and an exception propagated (DatabaseHelper may swallow it),
                // show the DB message here.
                MessageBox.Show("Database error: " + sqlEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add this helper to the existing code-behind (inside the partial class).
        // It will create the booking input controls at runtime if the Designer lost them.
        // This is a safe recovery measure — you can later move controls back to the Designer.
        private void EnsureBookingFields()
        {
            // panelMain must exist (it is declared in Designer). If not, bail out.
            if (panelMain == null) return;

            // Color palette (primary accent, secondary accent, background)
            var primary = Color.FromArgb(0, 120, 215);        // blue
            var primaryHover = Color.FromArgb(0, 95, 170);
            var secondary = Color.FromArgb(255, 140, 0);     // warm orange
            var subtleBg = Color.FromArgb(249, 250, 252);    // very light
            var textPrimary = Color.FromArgb(40, 40, 40);
            var mutedText = Color.FromArgb(100, 100, 100);

            // Standard styling values
            var labelFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            var inputFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            var titleFont = new Font("Segoe UI", 14F, FontStyle.Bold);
            var btnFont = new Font("Segoe UI", 9F, FontStyle.Bold);

            // ToolTip for friendly hints
            var tt = new ToolTip();
            tt.AutoPopDelay = 6000;
            tt.InitialDelay = 500;
            tt.ReshowDelay = 200;
            tt.ShowAlways = true;

            // helper creators
            Func<string, Label> makeLabel = (text) =>
            {
                return new Label
                {
                    AutoSize = true,
                    Text = text,
                    Font = labelFont,
                    ForeColor = textPrimary,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 8, 3, 3)
                };
            };

            Func<string, TextBox> makeTextBox = (name) =>
            {
                return new TextBox
                {
                    Name = name,
                    Font = inputFont,
                    Size = new Size(180, 26),
                    Margin = new Padding(3),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    ForeColor = textPrimary
                };
            };

            // small helper to style a button with hover
            Action<Button, Color, Color> styleButton = (btn, baseColor, hoverColor) =>
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = baseColor;
                btn.ForeColor = Color.White;
                btn.Font = btnFont;
                btn.Cursor = Cursors.Hand;

                // remove previous handlers to avoid duplicate attachments
                btn.MouseEnter -= Btn_MouseEnter;
                btn.MouseLeave -= Btn_MouseLeave;

                // store hover color in Tag tuple
                btn.Tag = new Tuple<Color, Color>(baseColor, hoverColor);

                btn.MouseEnter += Btn_MouseEnter;
                btn.MouseLeave += Btn_MouseLeave;
            };

            void Btn_MouseEnter(object s, EventArgs e)
            {
                var b = s as Button;
                if (b?.Tag is Tuple<Color, Color> t) b.BackColor = t.Item2;
            }
            void Btn_MouseLeave(object s, EventArgs e)
            {
                var b = s as Button;
                if (b?.Tag is Tuple<Color, Color> t) b.BackColor = t.Item1;
            }

            // Determine if we need to create a fresh set (Designer lost them)
            bool missingAny =
                panelMain.Controls["dateTimePicker1"] == null ||
                panelMain.Controls["dateTimePicker2"] == null ||
                panelMain.Controls["dateTimePicker3"] == null ||
                panelMain.Controls["textBoxCourtID"] == null ||
                panelMain.Controls["textBoxCustomerID"] == null ||
                panelMain.Controls["textBoxSportCenterID"] == null ||
                panelMain.Controls["textBoxRID"] == null ||
                panelMain.Controls["textBoxServiceID"] == null ||
                panelMain.Controls["numericUpDownQuantity"] == null ||
                panelMain.Controls["button12"] == null;

            if (missingAny)
            {
                // Create boxed container to visually separate the form
                var grp = new GroupBox
                {
                    Text = "",
                    Font = labelFont,
                    BackColor = subtleBg,
                    Size = new Size(680, 360),
                    Location = new Point(40, 60),
                    Padding = new Padding(12)
                };

                // Title label centered
                var lblTitle = new Label
                {
                    Text = "BOOKING FORM",
                    Font = titleFont,
                    AutoSize = false,
                    ForeColor = primary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 34
                };
                grp.Controls.Add(lblTitle);

                // TableLayout for alignment: 4 columns to allow labels + inputs grouped
                var tlp = new TableLayoutPanel
                {
                    ColumnCount = 4,
                    RowCount = 6,
                    Dock = DockStyle.Bottom,
                    AutoSize = false,
                    Size = new Size(656, 290),
                    Location = new Point(10, 46),
                    Padding = new Padding(6),
                    BackColor = Color.Transparent
                };
                // columns: label / control / label / control
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); // label
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));   // input
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); // label
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));   // input

                for (int r = 0; r < tlp.RowCount; r++)
                    tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

                // Row 0: Booking Date | Court ID
                var dt1 = new DateTimePicker { Name = "dateTimePicker1", Format = DateTimePickerFormat.Short, Font = inputFont, Dock = DockStyle.Fill, Margin = new Padding(3) };
                var lblDate = makeLabel("Booking Date");
                var lblCourt = makeLabel("Court ID");
                var tbCourt = makeTextBox("textBoxCourtID");

                tlp.Controls.Add(lblDate, 0, 0);
                tlp.Controls.Add(dt1, 1, 0);
                tlp.Controls.Add(lblCourt, 2, 0);
                tlp.Controls.Add(tbCourt, 3, 0);

                // Row 1: Start Time | CustomerID
                var dt2 = new DateTimePicker { Name = "dateTimePicker2", Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = inputFont, Dock = DockStyle.Fill, Margin = new Padding(3) };
                var lblStart = makeLabel("Start Time");
                var lblCust = makeLabel("Customer ID");
                var tbCust = makeTextBox("textBoxCustomerID");

                tlp.Controls.Add(lblStart, 0, 1);
                tlp.Controls.Add(dt2, 1, 1);
                tlp.Controls.Add(lblCust, 2, 1);
                tlp.Controls.Add(tbCust, 3, 1);

                // Row 2: End Time | SportCenterID
                var dt3 = new DateTimePicker { Name = "dateTimePicker3", Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = inputFont, Dock = DockStyle.Fill, Margin = new Padding(3) };
                var lblEnd = makeLabel("End Time");
                var lblCenter = makeLabel("Sport Center ID");
                var tbCenter = makeTextBox("textBoxSportCenterID");

                tlp.Controls.Add(lblEnd, 0, 2);
                tlp.Controls.Add(dt3, 1, 2);
                tlp.Controls.Add(lblCenter, 2, 2);
                tlp.Controls.Add(tbCenter, 3, 2);

                // Row 3: ServiceID | RID
                var lblService = makeLabel("Service ID");
                var tbService = makeTextBox("textBoxServiceID");
                var lblRid = makeLabel("RID");
                var tbRid = makeTextBox("textBoxRID");

                tlp.Controls.Add(lblService, 0, 3);
                tlp.Controls.Add(tbService, 1, 3);
                tlp.Controls.Add(lblRid, 2, 3);
                tlp.Controls.Add(tbRid, 3, 3);

                // Row 4: Quantity (spans two columns)
                var lblQty = makeLabel("Quantity");
                var nud = new NumericUpDown
                {
                    Name = "numericUpDownQuantity",
                    Minimum = 0,
                    Maximum = 100,
                    Value = 1,
                    Font = inputFont,
                    Dock = DockStyle.Left,
                    Size = new Size(100, 24),
                    Margin = new Padding(3)
                };
                tlp.Controls.Add(lblQty, 0, 4);
                tlp.Controls.Add(nud, 1, 4);

                // Row 5: Buttons (Book and Clear)
                var btnBook = new Button
                {
                    Name = "button12",
                    Text = "Book",
                    Size = new Size(100, 32),
                    Margin = new Padding(3)
                };
                styleButton(btnBook, primary, primaryHover);
                btnBook.Click += button12_Click;

                var btnClear = new Button
                {

                    Name = "buttonClearBooking",
                    Text = "Clear",
                    Size = new Size(100, 32),
                    Margin = new Padding(3)
                };
                styleButton(btnClear, Color.FromArgb(200, 200, 200), Color.FromArgb(175, 175, 175));
                btnClear.ForeColor = textPrimary;
                btnClear.Click += (s, e) =>
                {
                    // clear fields safely
                    tbCourt.Text = "";
                    tbCust.Text = "";
                    tbCenter.Text = "";
                    tbRid.Text = "";
                    tbService.Text = "";
                    nud.Value = 1;
                    dt1.Value = DateTime.Today;
                    dt2.Value = DateTime.Now;
                    dt3.Value = DateTime.Now.AddHours(1);
                };

                // Place buttons in bottom row across columns
                var buttonsPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Margin = new Padding(3)
                };
                buttonsPanel.Controls.Add(btnBook);
                buttonsPanel.Controls.Add(btnClear);

                // Add an empty spacer to align nicely
                tlp.Controls.Add(new Panel { BackColor = Color.Transparent }, 2, 5);
                tlp.Controls.Add(buttonsPanel, 3, 5);

                // Add table to group and add to panelMain
                grp.Controls.Add(tlp);
                panelMain.Controls.Add(grp);

                // Assign fields so other code uses them
                dateTimePicker1 = dt1;
                dateTimePicker2 = dt2;
                dateTimePicker3 = dt3;
                textBoxCourtID = tbCourt;
                textBoxCustomerID = tbCust;
                textBoxSportCenterID = tbCenter;
                textBoxRID = tbRid;
                textBoxServiceID = tbService;
                numericUpDownQuantity = nud;
                button12 = btnBook;

                // Tooltips
                tt.SetToolTip(dt1, "Select booking date");
                tt.SetToolTip(dt2, "Choose start time");
                tt.SetToolTip(dt3, "Choose end time");
                tt.SetToolTip(tbService, "Optional service ID");
                tt.SetToolTip(nud, "Number of service units");
                tt.SetToolTip(btnBook, "Click to create booking");
            }
            else
            {
                // Controls exist — apply consistent styling and tooltips
                // Label and input fonts
                foreach (Control c in panelMain.Controls)
                {
                    if (c is Label lbl)
                    {
                        lbl.Font = labelFont;
                        lbl.ForeColor = textPrimary;
                    }
                }

                // Safe styling for known inputs
                if (panelMain.Controls["dateTimePicker1"] is DateTimePicker d1) { d1.Format = DateTimePickerFormat.Short; d1.Font = inputFont; tt.SetToolTip(d1, "Select booking date"); }
                if (panelMain.Controls["dateTimePicker2"] is DateTimePicker d2) { d2.Format = DateTimePickerFormat.Time; d2.ShowUpDown = true; d2.Font = inputFont; tt.SetToolTip(d2, "Start time"); }
                if (panelMain.Controls["dateTimePicker3"] is DateTimePicker d3) { d3.Format = DateTimePickerFormat.Time; d3.ShowUpDown = true; d3.Font = inputFont; tt.SetToolTip(d3, "End time"); }

                if (panelMain.Controls["textBoxCourtID"] is TextBox tbCt) { tbCt.Font = inputFont; tbCt.Size = new Size(180, 24); tbCt.BackColor = Color.White; tbCt.ForeColor = textPrimary; tt.SetToolTip(tbCt, "Court identifier"); }
                if (panelMain.Controls["textBoxCustomerID"] is TextBox tbCu) { tbCu.Font = inputFont; tbCu.Size = new Size(180, 24); tbCu.BackColor = Color.White; tbCu.ForeColor = textPrimary; tt.SetToolTip(tbCu, "Customer identifier"); }
                if (panelMain.Controls["textBoxSportCenterID"] is TextBox tbSc) { tbSc.Font = inputFont; tbSc.Size = new Size(180, 24); tbSc.BackColor = Color.White; tbSc.ForeColor = textPrimary; tt.SetToolTip(tbSc, "Sport center identifier"); }
                if (panelMain.Controls["textBoxRID"] is TextBox tbR) { tbR.Font = inputFont; tbR.Size = new Size(180, 24); tbR.BackColor = Color.White; tbR.ForeColor = textPrimary; tt.SetToolTip(tbR, "Receptionist ID"); }
                if (panelMain.Controls["textBoxServiceID"] is TextBox tbS) { tbS.Font = inputFont; tbS.Size = new Size(180, 24); tbS.BackColor = Color.White; tbS.ForeColor = textPrimary; tt.SetToolTip(tbS, "Service ID (optional)"); }

                if (panelMain.Controls["numericUpDownQuantity"] is NumericUpDown nqd)
                {
                    nqd.Font = inputFont;
                    nqd.Minimum = 0;
                    nqd.Maximum = 100;
                    nqd.Value = Math.Max(1, nqd.Value);
                    tt.SetToolTip(nqd, "Quantity");
                }

                if (panelMain.Controls["button12"] is Button btn)
                {
                    styleButton(btn, primary, primaryHover);
                    tt.SetToolTip(btn, "Create booking");
                }

                // If there's a title TextBox, style it
                if (panelMain.Controls["textBox1"] is TextBox tbTitle)
                {
                    tbTitle.Font = titleFont;
                    tbTitle.BackColor = subtleBg;
                    tbTitle.BorderStyle = BorderStyle.None;
                    tbTitle.TextAlign = HorizontalAlignment.Center;
                    tbTitle.ForeColor = primary;
                }
            }
        }

        private void Receiptionist_BookingForm_Load(object sender, EventArgs e)
        {

        }
    }
}
