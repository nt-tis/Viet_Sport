using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CUSTOMER
{
    public partial class Change_Court : Form
    {
        private string _bookingID;
        private string _oldCourtID;
        private string _oldCenterID;
        private DateTime _oldStartTime;
        private DateTime _oldEndTime;
        private string _newCourtID;
        private string _newCenterID;
        private DateTime _newStartTime;
        private DateTime _newEndTime;
        private string _customerID;

        public Change_Court()
        {
            InitializeComponent();
        }

        public Change_Court(string bookingID, string oldCourtID, string oldCenterID,
            DateTime oldStartTime, DateTime oldEndTime, string newCourtID, string newCenterID,
            DateTime newStartTime, DateTime newEndTime, string customerID) : this()
        {
            _bookingID = bookingID;
            _oldCourtID = oldCourtID;
            _oldCenterID = oldCenterID;
            _oldStartTime = oldStartTime;
            _oldEndTime = oldEndTime;
            _newCourtID = newCourtID;
            _newCenterID = newCenterID;
            _newStartTime = newStartTime;
            _newEndTime = newEndTime;
            _customerID = customerID;

            LoadFormData();
        }

        private void LoadFormData()
        {
            // Fill old booking info (read-only)
            OldCourtID.Text = _oldCourtID;
            OldCourtID.ReadOnly = true;

            OldCenterID.Text = _oldCenterID;
            OldCenterID.ReadOnly = true;

            OldStart.Value = _oldStartTime;
            OldStart.Enabled = false;

            OldEnd.Value = _oldEndTime;
            OldEnd.Enabled = false;

            // Fill new booking info (read-only)
            NewCourtID.Text = _newCourtID;
            NewCourtID.ReadOnly = true;

            NewStart.Value = _newStartTime;
            NewStart.Format = DateTimePickerFormat.Custom;
            NewStart.CustomFormat = "dd/MM/yyyy HH:mm";

            NewEnd.Value = _newEndTime;
            NewEnd.Format = DateTimePickerFormat.Custom;
            NewEnd.CustomFormat = "dd/MM/yyyy HH:mm";
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DateTime newStartTime = NewStart.Value;
            DateTime newEndTime = NewEnd.Value;

            if (newStartTime >= newEndTime)
            {
                MessageBox.Show("Giờ bắt đầu phải nhỏ hơn giờ kết thúc", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Xác nhận đổi sân?\n\n" +
                $"Từ: Sân {_oldCourtID} ({_oldStartTime:dd/MM/yyyy HH:mm} - {_oldEndTime:dd/MM/yyyy HH:mm})\n" +
                $"Sang: Sân {_newCourtID} ({newStartTime:dd/MM/yyyy HH:mm} - {newEndTime:dd/MM/yyyy HH:mm})",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                ChangeBooking(newStartTime, newEndTime);
            }
        }

        private void ChangeBooking(DateTime newStartTime, DateTime newEndTime)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_KH_DoiSanHoacGio", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@BookingID", _bookingID);
                        cmd.Parameters.AddWithValue("@CustomerID", _customerID);
                        cmd.Parameters.AddWithValue("@NewCourtID", _newCourtID);
                        cmd.Parameters.AddWithValue("@NewStartTime", newStartTime);
                        cmd.Parameters.AddWithValue("@NewEndTime", newEndTime);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show(
                            "Đổi sân thành công!",
                            "Thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi SQL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
