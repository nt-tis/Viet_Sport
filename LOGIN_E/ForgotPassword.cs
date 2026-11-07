using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOGIN_E
{
    public partial class ForgotPassword : Form
    {
        public event Action BackToSignin;
        public ForgotPassword()
        {
            InitializeComponent();

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
            txtUsername.KeyDown += enterHandler;
            txtPhoneNumber.KeyDown += enterHandler;
            txtIDCard.KeyDown += enterHandler;
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

            lblPasswordRequirements.Text = $"Password Requirements:\r\n{line1}\r\n{line2}\r\n{line3}";
            lblPasswordRequirements.ForeColor = (hasMin8 && hasUpper && hasDigit) ? Color.ForestGreen : Color.Gray;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string newPass = txtNewPassword.Text.Trim();
            string confirmPass = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (newPass.Length < 8)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 8 ký tự (Minimum 8 characters)!", "Yêu cầu mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (!newPass.Any(char.IsUpper))
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất một chữ cái in HOA (A-Z)!", "Yêu cầu mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (!newPass.Any(char.IsDigit))
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất một chữ số (0-9)!", "Yêu cầu mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi xác nhận", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            Modify modify = new Modify();
            int res = modify.ResetPassword(txtUsername.Text.Trim(), txtIDCard.Text.Trim(),
                                txtPhoneNumber.Text.Trim(), newPass);
            if (res == 1)
            {
                MessageBox.Show("Đổi mật khẩu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Có lỗi xảy ra khi đổi mật khẩu, mã lỗi: {res}", "Thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            BackToSignin?.Invoke();
        }
    }
}
