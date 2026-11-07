using System.Drawing;
﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CASHIER
{
    public partial class Cashier_ChangePassword : Form
    {
        private string connectionString = DatabaseConfig.ConnectionString;

        // Thông tin đăng nhập
        private string _employeeID;
        private string _fullName;
        private string _username;

        public Cashier_ChangePassword()
        {
            InitializeComponent();
            InitEnhancements();
        }

        // Constructor nhận thông tin đăng nhập
        public Cashier_ChangePassword(string employeeID, string fullName, string username) : this()
        {
            _employeeID = employeeID;
            _fullName = fullName;
            _username = username;
            
            // Hiển thị thông tin nhân viên
            lblEmployeeID.Text = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
            
            if (!string.IsNullOrEmpty(_fullName))
            {
                this.Text = $"Đổi Mật Khẩu - {_fullName}";
            }
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string currentEmployeeID = !string.IsNullOrEmpty(_employeeID) ? _employeeID : "EMP0016";
            string currentPassword = txtCurrentPassword.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(currentPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu hiện tại!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCurrentPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Vui lòng xác nhận mật khẩu mới!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                txtConfirmPassword.SelectAll();
                return;
            }

            if (currentPassword == newPassword)
            {
                MessageBox.Show("Mật khẩu mới phải khác mật khẩu hiện tại!", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            string validationMessage = ValidatePasswordStrength(newPassword);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                MessageBox.Show(validationMessage, 
                    "Mật khẩu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            // Xác nhận trước khi đổi mật khẩu
            string confirmMsg = $"Xác nhận đổi mật khẩu?\n\n";
            if (!string.IsNullOrEmpty(_fullName))
            {
                confirmMsg += $"- Nhân viên: {_fullName}\n";
            }
            confirmMsg += $"- Mã NV: {currentEmployeeID}\n\n";
            confirmMsg += "Bạn sẽ cần sử dụng mật khẩu mới cho lần đăng nhập tiếp theo.";

            DialogResult confirmResult = MessageBox.Show(confirmMsg, "Xác nhận đổi mật khẩu", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Xác thực mật khẩu hiện tại
                    SqlCommand verifyCmd = new SqlCommand("sp_Cashier_VerifyPassword", conn);
                    verifyCmd.CommandType = CommandType.StoredProcedure;
                    verifyCmd.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);
                    verifyCmd.Parameters.AddWithValue("@Password", currentPassword);

                    SqlDataAdapter adapter = new SqlDataAdapter(verifyCmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Mật khẩu hiện tại không chính xác!\n\nVui lòng thử lại.", 
                            "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCurrentPassword.Focus();
                        txtCurrentPassword.SelectAll();
                        return;
                    }

                    string employeeName = dt.Rows[0]["FullName"].ToString();

                    // Cập nhật mật khẩu mới
                    SqlCommand updateCmd = new SqlCommand("sp_Cashier_UpdatePassword", conn);
                    updateCmd.CommandType = CommandType.StoredProcedure;
                    updateCmd.Parameters.AddWithValue("@EmployeeID", currentEmployeeID);
                    updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);

                    SqlDataAdapter updateAdapter = new SqlDataAdapter(updateCmd);
                    DataTable updateResult = new DataTable();
                    updateAdapter.Fill(updateResult);

                    if (updateResult.Rows.Count > 0 && Convert.ToInt32(updateResult.Rows[0]["Result"]) == 1)
                    {
                        string successMsg = $"Đổi mật khẩu thành công!\n\n" +
                                          $"- Nhân viên: {employeeName}\n" +
                                          $"- Mã NV: {currentEmployeeID}\n" +
                                          $"- Username: {_username ?? "N/A"}\n" +
                                          $"- Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n\n" +
                                          $"Yêu cầu bảo mật:\n" +
                                          $"  - Không chia sẻ mật khẩu với người khác\n" +
                                          $"  - Sử dụng mật khẩu mới cho lần đăng nhập tiếp theo\n" +
                                          $"  - Đổi mật khẩu định kỳ để bảo mật tài khoản";

                        MessageBox.Show(successMsg, "Đổi mật khẩu thành công", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Đổi mật khẩu thất bại!\n\nVui lòng thử lại hoặc liên hệ quản trị viên.", 
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đổi mật khẩu:\n\n{ex.Message}\n\nVui lòng liên hệ quản trị viên nếu lỗi tiếp diễn.", 
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ValidatePasswordStrength(string password)
        {
            if (password.Length < 8)
            {
                return "Mật khẩu phải có ít nhất 8 ký tự!\n\n" +
                       "Yêu cầu mật khẩu mạnh:\n" +
                       "  - Tối thiểu 8 ký tự\n" +
                       "  - Ít nhất 1 chữ cái viết hoa (A-Z)\n" +
                       "  - Ít nhất 1 chữ số (0-9)";
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return "Mật khẩu phải chứa ít nhất một chữ cái viết hoa!\n\n" +
                       "Ví dụ: Password123";
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return "Mật khẩu phải chứa ít nhất một chữ số!\n\n" +
                       "Ví dụ: Password123";
            }

            return null; // Mật khẩu hợp lệ
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Hủy bỏ thay đổi mật khẩu?", "Xác nhận", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                ClearForm();
            }
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowPassword.Checked)
            {
                txtCurrentPassword.PasswordChar = '\0';
                txtNewPassword.PasswordChar = '\0';
                txtConfirmPassword.PasswordChar = '\0';
            }
            else
            {
                txtCurrentPassword.PasswordChar = '*';
                txtNewPassword.PasswordChar = '*';
                txtConfirmPassword.PasswordChar = '*';
            }
        }

        private void InitEnhancements()
        {
            this.KeyPreview = true;
            this.AcceptButton = btnChangePassword;

            txtNewPassword.TextChanged += (s, e) => UpdatePasswordRequirementsUI();
            UpdatePasswordRequirementsUI();

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnChangePassword_Click(this, EventArgs.Empty);
                    e.Handled = true;
                }
            };
            txtCurrentPassword.KeyDown += enterHandler;
            txtNewPassword.KeyDown += enterHandler;
            txtConfirmPassword.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        private void UpdatePasswordRequirementsUI()
        {
            string pwd = txtNewPassword.Text.Trim();
            bool hasMin8 = pwd.Length >= 8;
            bool hasUpper = Regex.IsMatch(pwd, @"[A-Z]");
            bool hasDigit = Regex.IsMatch(pwd, @"[0-9]");

            string line1 = hasMin8 ? "- Minimum 8 characters" : "- Minimum 8 characters";
            string line2 = hasUpper ? "- At least one uppercase letter (A-Z)" : "- At least one uppercase letter (A-Z)";
            string line3 = hasDigit ? "- At least one number (0-9)" : "- At least one number (0-9)";

            label2.Text = $"Password Requirements:\r\n{line1}\r\n{line2}\r\n{line3}";
            label2.ForeColor = (hasMin8 && hasUpper && hasDigit) ? Color.ForestGreen : Color.Gray;
        }

        private void ClearForm()
        {
            txtCurrentPassword.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
            chkShowPassword.Checked = false;
            txtCurrentPassword.Focus();
        }

        private void panelTop_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
        }

        private void Cashier_ChangePassword_Load(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
