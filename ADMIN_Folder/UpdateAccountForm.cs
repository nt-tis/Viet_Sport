using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class UpdateAccountForm : Form
    {
        private readonly string initialEmployeeId;

        public UpdateAccountForm(string employeeId = null)
        {
            InitializeComponent();
            initialEmployeeId = employeeId;

            // If caller provided an EmployeeID, prefill and try to load current username
            if (!string.IsNullOrEmpty(initialEmployeeId))
            {
                txtEmployeeID.Text = initialEmployeeId;
                LoadCurrentUsername(initialEmployeeId);
            }
        }

        private void LoadCurrentUsername(string employeeId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmployeeID", SqlDbType.NVarChar, 50) { Value = employeeId }
                };

                DataTable dt = DatabaseConnection.ExecuteQuery(
                    "SELECT AccountLogin FROM EMPLOYEE WHERE EmployeeID = @EmployeeID",
                    parameters
                );

                if (dt != null && dt.Rows.Count > 0)
                {
                    var account = dt.Rows[0]["AccountLogin"];
                    txtNewUsername.Text = account == DBNull.Value ? string.Empty : account.ToString();
                }
            }
            catch
            {
                // swallow - not critical to block editing
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            string employeeId = txtEmployeeID.Text.Trim();
            string newUsername = txtNewUsername.Text.Trim();
            string newPassword = txtNewPassword.Text;

            if (string.IsNullOrEmpty(employeeId))
            {
                MessageBox.Show("Vui lòng nhập EmployeeID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmployeeID.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newUsername))
            {
                MessageBox.Show("Vui lòng nhập Username mới.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                MessageBox.Show("Vui lòng nhập Password mới (tối thiểu 6 ký tự).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_AccountEmployees_UpdateAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.NVarChar, 50) { Value = employeeId });
                    cmd.Parameters.Add(new SqlParameter("@NewUsername", SqlDbType.NVarChar, 50) { Value = newUsername });
                    cmd.Parameters.Add(new SqlParameter("@NewPassword", SqlDbType.NVarChar, 255) { Value = newPassword });

                    var returnParam = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    // Treat 0 or 1 as success for backward compatibility
                    if (ret == 0 || ret == 1)
                    {
                        MessageBox.Show("Cập nhật tài khoản thành công.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }

                    // Map new stored-procedure return codes
                    // According to updated proc:
                    // -1 => Employee does not exist
                    // -2 => Employee has no account to update
                    // -3 => New username already exists
                    switch (ret)
                    {
                        case -1:
                            MessageBox.Show($"Lỗi: Nhân viên có mã '{employeeId}' không tồn tại trên hệ thống.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case -2:
                            MessageBox.Show("Lỗi: Nhân viên này chưa có tài khoản để cập nhật. Hãy dùng nút Create.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case -3:
                            MessageBox.Show("Lỗi: Tên đăng nhập mới đã tồn tại.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        default:
                            MessageBox.Show("Lỗi khi cập nhật tài khoản (internal).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi cập nhật tài khoản: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật tài khoản: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Optional: when EmployeeID text changes, try to refresh current username
        private void TxtEmployeeID_TextChanged(object sender, EventArgs e)
        {
            int pos = txtEmployeeID.SelectionStart;
            txtEmployeeID.Text = txtEmployeeID.Text.ToUpper();
            txtEmployeeID.SelectionStart = pos;
        }
    }
}
