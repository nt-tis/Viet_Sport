using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace RECEPTIONIST
{
    public partial class Receiptionist_CancellationForm : Form
    {
        public Receiptionist_CancellationForm()
        {
            InitializeComponent();
        }

        // New constructor for embedded usage
        public Receiptionist_CancellationForm(bool embedded) : this()
        {
            if (embedded)
            {
                PrepareEmbeddedMode();
            }
        }

        // Expose panels so host (Receptionist_court) can move them
        public Panel SidebarPanel
        {
            get { return this.panel2; }    // designer sidebar panel
        }

        public Panel ContentPanel
        {
            get { return this.panelMain; } // designer content panel
        }

        public Panel TopBarPanel
        {
            get { return this.panel1; }    // designer top bar panel
        }

        // Prepare form so its panels can be detached/attached by host.
        // Do not hide sidebar here; host will attach the sidebar into its left area.
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
            catch
            {
                // swallow designer mismatches to avoid runtime crash
            }
        }

        // Helper: find control by name in this form (search children)
        // Updated to also search the detached ContentPanel and SidebarPanel so handlers still work
        // when panels have been moved into a host form.
        private Control FindCtrl(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            // 1) Search this form's controls (default)
            Control[] found = this.Controls.Find(name, true);
            if (found != null && found.Length > 0) return found[0];

            // 2) If ContentPanel (panelMain) exists, search inside it (handles case where panelMain
            // was detached from this form and hosted elsewhere)
            try
            {
                if (this.panelMain != null)
                {
                    found = this.panelMain.Controls.Find(name, true);
                    if (found != null && found.Length > 0) return found[0];
                }
            }
            catch
            {
                // ignore any unexpected cross-thread/parenting issues
            }

            // 3) Also check sidebar panel in case some controls were moved
            try
            {
                if (this.panel2 != null)
                {
                    found = this.panel2.Controls.Find(name, true);
                    if (found != null && found.Length > 0) return found[0];
                }
            }
            catch
            {
                // ignore
            }

            // 4) As a last resort try to locate within the active form's controls (host scenario)
            try
            {
                var active = Form.ActiveForm;
                if (active != null)
                {
                    found = active.Controls.Find(name, true);
                    if (found != null && found.Length > 0) return found[0];
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        // Helper: get trimmed text from a textbox-like control (returns empty if not found)
        private string GetText(string controlName)
        {
            var c = FindCtrl(controlName);
            if (c is TextBox tb) return tb.Text.Trim();
            if (c is ComboBox cb) return (cb.SelectedItem?.ToString() ?? cb.Text ?? "").Trim();
            if (c is Label lbl) return lbl.Text.Trim();
            return string.Empty;
        }

        // Helper: get DateTimePicker value or DateTime.Now if not found
        private DateTime GetDate(string controlName)
        {
            var c = FindCtrl(controlName);
            if (c is DateTimePicker dt) return dt.Value.Date;
            return DateTime.Now.Date;
        }

        // Helper: get TimeSpan from DateTimePicker (time portion) or TimeSpan.Zero
        private TimeSpan GetTime(string controlName)
        {
            var c = FindCtrl(controlName);
            if (c is DateTimePicker dt) return dt.Value.TimeOfDay;
            return TimeSpan.Zero;
        }

        // Helper: get integer from NumericUpDown or TextBox (fallback)
        private int GetInt(string controlName)
        {
            var c = FindCtrl(controlName);
            if (c is NumericUpDown nud) return (int)nud.Value;
            if (c is ComboBox cb)
            {
                int v;
                if (int.TryParse(cb.SelectedItem?.ToString() ?? cb.Text, out v)) return v;
            }
            if (c is TextBox tb)
            {
                int v;
                if (int.TryParse(tb.Text.Trim(), out v)) return v;
            }
            return 0;
        }

        // Raise events or navigation
        private void button3_Click(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void panel5_Paint(object sender, PaintEventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void button4_Click(object sender, EventArgs e) { }
        private void button5_Click(object sender, EventArgs e) { }
        private void button9_Click(object sender, EventArgs e) { }
        private void button15_Click(object sender, EventArgs e) { }
        private void button5_Click_1(object sender, EventArgs e) { }
        private void button5_Click_2(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void button22_Click(object sender, EventArgs e) { }
        private void panel7_Paint(object sender, PaintEventArgs e) { }
        private void button18_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) { }
        private void button11_Click(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label3_Click_1(object sender, EventArgs e) { }
        private void panelMain_Paint(object sender, PaintEventArgs e) { }

        // Confirm button handler: collect inputs and call sp_Receptionist_Booking_Create
        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                // Read fields from the designer: textBox2 = Old Booking ID, textBox3 = New Booking ID (optional)
                string bookingId = GetText("textBox2");
                string newBookingId = GetText("textBox3");

                if (string.IsNullOrWhiteSpace(bookingId))
                {
                    MessageBox.Show("Vui lòng nhập Old Booking ID.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Build parameters for sp_Receptionist_Cancellation_Create
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@BookingID", SqlDbType.NVarChar, 50) { Value = bookingId },
                    new SqlParameter("@NewBookingID", SqlDbType.NVarChar, 50) { Value = string.IsNullOrWhiteSpace(newBookingId) ? (object)DBNull.Value : newBookingId }
                };

                // return value parameter
                var returnParam = new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                parameters.Add(returnParam);

                // Execute cancellation stored procedure
                DatabaseHelper.ExecuteStoredProcedure("sp_Receptionist_Cancellation_Create", parameters.ToArray());

                // Read return value (proc uses RETURN -1, -2 for errors; default 0 for success)
                int retVal = 0;
                if (returnParam.Value != null && returnParam.Value != DBNull.Value)
                    int.TryParse(returnParam.Value.ToString(), out retVal);

                if (retVal == 0)
                {
                    MessageBox.Show("Hủy thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Map known error codes from stored proc
                string userMessage;
                switch (retVal)
                {
                    case -1:
                        userMessage = "Lỗi: Không tìm thấy phiếu đặt sân!";
                        break;
                    case -2:
                        userMessage = "Lỗi: Giờ bắt đầu sân đã trôi qua, không thể hủy phiếu!";
                        break;
                    default:
                        userMessage = "Lỗi không xác định (code: " + retVal + ")";
                        break;
                }

                MessageBox.Show(userMessage, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SqlException sqlEx)
            {
                // RAISERROR in the proc will surface here
                MessageBox.Show("Lỗi khi gọi Stored Procedure: " + sqlEx.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi bất ngờ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
