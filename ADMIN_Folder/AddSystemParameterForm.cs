using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class AddSystemParameterForm : Form
    {
        private string adminID;

        public AddSystemParameterForm(string adminEmployeeID)
        {
            InitializeComponent();
            this.adminID = adminEmployeeID;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (!ValidateInputs(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Parse inputs
                decimal price = numPrice.Value;
                int minBookingTime = (int)numMinBookingTime.Value;
                int maxBookingTime = (int)numMaxBookingTime.Value;
                int maxCourts = (int)numMaxCourts.Value;

                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_SystemParameters_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var pPrice = new SqlParameter("@Price", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = price };
                    cmd.Parameters.Add(pPrice);
                    cmd.Parameters.Add(new SqlParameter("@MinBookingTime", SqlDbType.Int) { Value = minBookingTime });
                    cmd.Parameters.Add(new SqlParameter("@MaxBookingTime", SqlDbType.Int) { Value = maxBookingTime });
                    cmd.Parameters.Add(new SqlParameter("@MaxCourtsPerCustomer", SqlDbType.Int) { Value = maxCourts });
                    cmd.Parameters.Add(new SqlParameter("@AID", SqlDbType.NVarChar, 50) { Value = adminID });

                    var returnParam = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    // Stored procedure returns negative error codes (-1..-5). Treat 0 or positive as success.
                    if (ret < 0)
                    {
                        string msg;
                        switch (ret)
                        {
                            case -1:
                                msg = $"Lỗi: Nhân viên '{adminID}' không có quyền Admin để thực hiện thao tác này.";
                                break;
                            case -2:
                                msg = "Lỗi: Đơn giá sân (Price) phải lớn hơn 0.";
                                break;
                            case -3:
                                msg = "Lỗi: Số lượng sân đặt tối đa phải lớn hơn 0.";
                                break;
                            case -4:
                                msg = "Lỗi: Hệ thống đã đạt giới hạn tối đa 999 mã tham số (SPM999).";
                                break;
                            case -5:
                                msg = "Lỗi: MinBookingTime phải nhỏ hơn MaxBookingTime và các giá trị phải lớn hơn 0.";
                                break;
                            default:
                                msg = "Lỗi không rõ từ stored procedure.";
                                break;
                        }

                        MessageBox.Show(msg, "Add Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Success (ret >= 0 or no return value provided)
                    MessageBox.Show("Thêm mới tham số hệ thống thành công!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi thêm tham số: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm tham số: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs(out string errorMessage)
        {
            errorMessage = string.Empty;

            // Price must be > 0
            if (numPrice.Value <= 0)
            {
                errorMessage = "Đơn giá sân (Price) phải lớn hơn 0.";
                return false;
            }

            // Min < Max
            if (numMinBookingTime.Value >= numMaxBookingTime.Value)
            {
                errorMessage = "Thời gian đặt tối thiểu phải nhỏ hơn thời gian đặt tối đa.";
                return false;
            }

            // Min > 0
            if (numMinBookingTime.Value <= 0)
            {
                errorMessage = "Thời gian đặt tối thiểu phải lớn hơn 0.";
                return false;
            }

            // Max > 0
            if (numMaxBookingTime.Value <= 0)
            {
                errorMessage = "Thời gian đặt tối đa phải lớn hơn 0.";
                return false;
            }

            // MaxCourts > 0
            if (numMaxCourts.Value <= 0)
            {
                errorMessage = "Số lượng sân đặt tối đa phải lớn hơn 0.";
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AddSystemParameterForm_Load(object sender, EventArgs e)
        {

        }
    }
}
