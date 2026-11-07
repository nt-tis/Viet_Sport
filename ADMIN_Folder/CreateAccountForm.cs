using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADMIN
{
    public partial class CreateAccountForm : Form
    {
        public CreateAccountForm()
        {
            InitializeComponent();
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            string employeeId = txtEmployeeID.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text; // consider hashing later

            // Basic validation
            if (string.IsNullOrEmpty(employeeId))
            {
                MessageBox.Show("Vui lòng nhập EmployeeID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmployeeID.Focus();
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập Username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                MessageBox.Show("Vui lòng nhập Password (tối thiểu 6 ký tự).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand("sp_Admin_AccountEmployees_CreateAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.NVarChar, 50) { Value = employeeId });
                    cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username });
                    cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 255) { Value = password });

                    var returnParam = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue,
                        SqlDbType = SqlDbType.Int
                    };
                    cmd.Parameters.Add(returnParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int ret = (returnParam.Value != DBNull.Value) ? Convert.ToInt32(returnParam.Value) : 0;

                    // Treat explicit success (1) and legacy/no-return (0) as success.
                    if (ret == 1 || ret == 0)
                    {
                        MessageBox.Show($"Tạo tài khoản '{username}' cho nhân viên '{employeeId}' thành công.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }

                    // Map negative error codes from stored procedure
                    switch (ret)
                    {
                        case -1:
                            MessageBox.Show("Lỗi: Nhân viên không tồn tại.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case -2:
                            MessageBox.Show("Lỗi: Nhân viên này đã có tài khoản. Hãy dùng chức năng Update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case -3:
                            MessageBox.Show("Lỗi: Tên đăng nhập này đã có người sử dụng.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        default:
                            MessageBox.Show("Lỗi khi tạo tài khoản (internal).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi khi tạo tài khoản: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
