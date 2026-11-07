using System.Linq;
﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CUSTOMER
{
    public partial class CustomerChangePassword : Form
    {
        private readonly Customer_Main _main;

        // Constructor nhận MainForm
        public CustomerChangePassword(Customer_Main main)
        {
            InitializeComponent();
            _main = main;
            InitEnhancements();
        }

        // Constructor mặc định (designer)
        public CustomerChangePassword()
        {
            InitializeComponent();
            InitEnhancements();
        }

        private void InitEnhancements()
        {
            this.KeyPreview = true;
            this.AcceptButton = btnOK;

            txtNewPassword.TextChanged += (s, e) => UpdatePasswordRequirementsUI();
            UpdatePasswordRequirementsUI();

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnOK_Click(this, EventArgs.Empty);
                    e.Handled = true;
                }
            };
            txtOldPassword.KeyDown += enterHandler;
            txtNewPassword.KeyDown += enterHandler;
            txtConfirmPassword.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        private void UpdatePasswordRequirementsUI()
        {
            string pwd = txtNewPassword.Text.Trim();
            bool hasMin8 = pwd.Length >= 8;
            bool hasUpper = pwd.Any(char.IsUpper);
            bool hasDigit = pwd.Any(char.IsDigit);

            string line1 = hasMin8 ? "- Minimum 8 characters" : "- Minimum 8 characters";
            string line2 = hasUpper ? "- At least one uppercase letter (A-Z)" : "- At least one uppercase letter (A-Z)";
            string line3 = hasDigit ? "- At least one number (0-9)" : "- At least one number (0-9)";

            label2.Text = $"Password Requirements:\r\n{line1}\r\n{line2}\r\n{line3}";
            label2.ForeColor = (hasMin8 && hasUpper && hasDigit) ? Color.ForestGreen : Color.Gray;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_main == null)
            {
                MessageBox.Show("MainForm chưa được truyền vào.", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string accountLogin = _main.UserName;
            string oldPass = txtOldPassword.Text.Trim();
            string newPass = txtNewPassword.Text.Trim();
            string confirm = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(accountLogin))
            {
                MessageBox.Show("Không lấy được tên đăng nhập từ hệ thống.", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(oldPass) ||
                string.IsNullOrWhiteSpace(newPass) ||
                string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Cảnh báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Cảnh báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                txtConfirmPassword.SelectAll();
                return;
            }

            // Validate password requirements
            if (!ValidatePassword(newPass))
            {
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_KH_DoiMatKhau", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@AccountLogin", accountLogin);
                        cmd.Parameters.AddWithValue("@OldPassword", oldPass);
                        cmd.Parameters.AddWithValue("@NewPassword", newPass);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show(
                            "Đổi mật khẩu thành công!\n", 
                            "Thành công", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Information
                        );

                        txtOldPassword.Clear();
                        txtNewPassword.Clear();
                        txtConfirmPassword.Clear();
                        txtOldPassword.Focus();
                    }
                }
            }
            catch (SqlException ex)
            {
                string errorMessage = ex.Message;

                // Xử lý các lỗi đặc biệt từ stored procedure
                if (errorMessage.Contains("Mật khẩu cũ không đúng") || 
                    errorMessage.Contains("không chính xác"))
                {
                    MessageBox.Show(
                        "Mật khẩu cũ không chính xác!\nVui lòng kiểm tra lại.", 
                        "Lỗi", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error
                    );
                    txtOldPassword.Focus();
                    txtOldPassword.SelectAll();
                }
                else if (errorMessage.Contains("Tài khoản không tồn tại") ||
                         errorMessage.Contains("không tìm thấy"))
                {
                    MessageBox.Show(
                        "Tài khoản không tồn tại trong hệ thống!", 
                        "Lỗi", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error
                    );
                }
                else if (errorMessage.Contains("Mật khẩu mới phải khác mật khẩu cũ") ||
                         errorMessage.Contains("phải khác"))
                {
                    MessageBox.Show(
                        "Mật khẩu mới phải khác mật khẩu cũ!", 
                        "Cảnh báo", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Warning
                    );
                    txtNewPassword.Focus();
                    txtNewPassword.SelectAll();
                }
                else
                {
                    MessageBox.Show(
                        $"Đổi mật khẩu thất bại:\n{errorMessage}", 
                        "Lỗi", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi hệ thống:\n{ex.Message}", 
                    "Lỗi", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error
                );
            }
        }

        private bool ValidatePassword(string password)
        {
            // Minimum 8 characters
            if (password.Length < 8)
            {
                MessageBox.Show(
                    "Mật khẩu phải có ít nhất 8 ký tự!", 
                    "Yêu cầu mật khẩu", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning
                );
                txtNewPassword.Focus();
                txtNewPassword.SelectAll();
                return false;
            }

            // At least one uppercase letter
            bool hasUpperCase = false;
            foreach (char c in password)
            {
                if (char.IsUpper(c))
                {
                    hasUpperCase = true;
                    break;
                }
            }

            if (!hasUpperCase)
            {
                MessageBox.Show(
                    "Mật khẩu phải có ít nhất một chữ HOA (A-Z)!", 
                    "Yêu cầu mật khẩu", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning
                );
                txtNewPassword.Focus();
                txtNewPassword.SelectAll();
                return false;
            }

            // At least one number
            bool hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsDigit(c))
                {
                    hasDigit = true;
                    break;
                }
            }

            if (!hasDigit)
            {
                MessageBox.Show(
                    "Mật khẩu phải có ít nhất một chữ số (0-9)!", 
                    "Yêu cầu mật khẩu", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning
                );
                txtNewPassword.Focus();
                txtNewPassword.SelectAll();
                return false;
            }

            return true;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
