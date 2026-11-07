using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    // Make this partial so it merges with the designer-generated partial class.
    // The visual controls (dtpFrom, dtpTo, btnOk, btnCancel, ...) must be defined
    // in DateRangePickerForm.Designer.cs (created by the Designer).
    public partial class DateRangePickerForm : Form
    {
        public DateTime FromDate => dtpFrom.Value.Date;
        public DateTime ToDate   => dtpTo.Value.Date;

        public DateRangePickerForm()
        {
            InitializeComponent();

            dtpFrom.Value = new DateTime(DateTime.Today.Year - 1, 1, 1);
            dtpTo.Value = DateTime.Today.AddDays(30);

            // Wire validation handler (designer may already wire it; this is safe)
            btnOk.Click -= BtnOk_Click;
            btnOk.Click += BtnOk_Click;

            // optional: set dialog buttons
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            // Fast client-side check first
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Call the search stored procedure to read its return code (procedure may return -1 for invalid range)
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_SystemParameters_Search", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.Date) { Value = dtpFrom.Value.Date });
                    cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.Date) { Value = dtpTo.Value.Date });

                    var returnParam = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    // execute procedure (we're only interested in its return value here)
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    if (ret < 0)
                    {
                        // Map stored-procedure error codes to user messages
                        switch (ret)
                        {
                            case -1:
                                MessageBox.Show("Lỗi: Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;
                            default:
                                MessageBox.Show("Lỗi xác thực ngày (mã lỗi: " + ret + ").", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;
                        }

                        this.DialogResult = DialogResult.None; // prevent dialog closing
                        return;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi khi kiểm tra ngày: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kiểm tra ngày: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
                return;
            }

            // If we reach here, validation passed and stored procedure did not report an error.
            // Dialog will close with DialogResult.OK (button may already set it).
        }
    }
}
