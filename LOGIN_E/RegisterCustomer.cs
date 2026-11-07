using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOGIN_E
{
    public partial class RegisterCustomer : Form
    {
        public event Action BackToSignin;

        public RegisterCustomer()
        {
            InitializeComponent();

            txtPassword.TextChanged += (s, e) => UpdatePasswordRequirementsUI();
            UpdatePasswordRequirementsUI();

            this.KeyPreview = true;
            this.AcceptButton = btnRegister;

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnRegister_Click(this, EventArgs.Empty);
                    e.Handled = true;
                }
            };

            txtFullName.KeyDown += enterHandler;
            txtPhoneNumber.KeyDown += enterHandler;
            txtEmail.KeyDown += enterHandler;
            txtAddress.KeyDown += enterHandler;
            dtpBirthday.KeyDown += enterHandler;
            txtUsername.KeyDown += enterHandler;
            txtPassword.KeyDown += enterHandler;
            txtConfirmPassword.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        private void UpdatePasswordRequirementsUI()
        {
            string pwd = txtPassword.Text.Trim();
            bool hasMin8 = pwd.Length >= 8;
            bool hasUpper = pwd.Any(char.IsUpper);
            bool hasDigit = pwd.Any(char.IsDigit);

            string line1 = hasMin8 ? "- Minimum 8 characters" : "- Minimum 8 characters";
            string line2 = hasUpper ? "- At least one uppercase letter (A-Z)" : "- At least one uppercase letter (A-Z)";
            string line3 = hasDigit ? "- At least one number (0-9)" : "- At least one number (0-9)";

            lblPasswordRequirements.Text = $"Password Requirements:\r\n{line1}\r\n{line2}\r\n{line3}";

            if (hasMin8 && hasUpper && hasDigit)
            {
                lblPasswordRequirements.ForeColor = Color.ForestGreen;
            }
            else
            {
                lblPasswordRequirements.ForeColor = Color.Gray;
            }
        }

        private bool ValidateRegistrationInputs(out string errorMessage)
        {
            errorMessage = string.Empty;

            string fullName = txtFullName.Text.Trim();
            string phone = txtPhoneNumber.Text.Trim();
            string email = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            // 1. Kiểm tra các trường không được để trống
            if (string.IsNullOrWhiteSpace(fullName))
            {
                errorMessage = "Vui lòng nhập Họ và tên (Full name)!";
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Vui lòng nhập Số điện thoại (Phone number)!";
                txtPhoneNumber.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Vui lòng nhập Địa chỉ Email!";
                txtEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                errorMessage = "Vui lòng nhập Địa chỉ (Address)!";
                txtAddress.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Vui lòng nhập Tên đăng nhập (Username)!";
                txtUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Vui lòng nhập Mật khẩu (Password)!";
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                errorMessage = "Vui lòng xác nhận lại Mật khẩu (Confirm password)!";
                txtConfirmPassword.Focus();
                return false;
            }

            // 2. Kiểm tra định dạng số điện thoại (Việt Nam: 10 chữ số, bắt đầu bằng 0 hoặc +84)
            var phoneRegex = new Regex(@"^(0|\+84)[0-9]{9,10}$");
            if (!phoneRegex.IsMatch(phone))
            {
                errorMessage = "Số điện thoại không đúng định dạng! Vui lòng nhập 10 chữ số hợp lệ (ví dụ: 0912345678 hoặc +84912345678).";
                txtPhoneNumber.Focus();
                return false;
            }

            // 3. Kiểm tra định dạng Email
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
            {
                errorMessage = "Địa chỉ Email không đúng định dạng! Vui lòng nhập đúng dạng abc@domain.com.";
                txtEmail.Focus();
                return false;
            }

            // 4. Kiểm tra độ tuổi (Khách hàng phải từ 10 tuổi trở lên và ngày sinh không được ở tương lai)
            DateTime birthday = dtpBirthday.Value.Date;
            if (birthday > DateTime.Today)
            {
                errorMessage = "Ngày sinh không hợp lệ (không thể lớn hơn ngày hiện tại)!";
                dtpBirthday.Focus();
                return false;
            }

            int age = DateTime.Today.Year - birthday.Year;
            if (birthday > DateTime.Today.AddYears(-age)) age--;
            if (age < 10)
            {
                errorMessage = "Khách hàng phải từ 10 tuổi trở lên để đăng ký tài khoản!";
                dtpBirthday.Focus();
                return false;
            }

            // 5. Kiểm tra định dạng Username (chữ và số, không khoảng trắng, từ 4-30 ký tự)
            var usernameRegex = new Regex(@"^[a-zA-Z0-9_]{4,30}$");
            if (!usernameRegex.IsMatch(username))
            {
                errorMessage = "Tên đăng nhập chỉ được gồm chữ cái, chữ số hoặc dấu gạch dưới, độ dài từ 4 đến 30 ký tự (không có khoảng trắng hay ký tự đặc biệt)!";
                txtUsername.Focus();
                return false;
            }

            // 6. Kiểm tra yêu cầu mật khẩu (Password Requirements)
            // 6.1 Tối thiểu 8 ký tự
            if (password.Length < 8)
            {
                errorMessage = "Mật khẩu phải có ít nhất 8 ký tự (Minimum 8 characters)!";
                txtPassword.Focus();
                return false;
            }

            // 6.2 Ít nhất một chữ cái in hoa (A-Z)
            if (!password.Any(char.IsUpper))
            {
                errorMessage = "Mật khẩu phải có ít nhất một chữ cái in HOA (A-Z)!";
                txtPassword.Focus();
                return false;
            }

            // 6.3 Ít nhất một chữ số (0-9)
            if (!password.Any(char.IsDigit))
            {
                errorMessage = "Mật khẩu phải có ít nhất một chữ số (0-9)!";
                txtPassword.Focus();
                return false;
            }

            // 7. Kiểm tra xác nhận mật khẩu
            if (password != confirmPassword)
            {
                errorMessage = "Mật khẩu xác nhận không khớp với mật khẩu đã nhập!";
                txtConfirmPassword.Focus();
                return false;
            }

            return true;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            btnRegister.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                if (!ValidateRegistrationInputs(out string validationError))
                {
                    MessageBox.Show(this, validationError, "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Modify modify = new Modify();
                var (success, message) = modify.RegisterCustomer(
                    txtFullName.Text.Trim(),
                    dtpBirthday.Value.Date,
                    txtPhoneNumber.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtAddress.Text.Trim(),
                    txtUsername.Text.Trim(),
                    txtPassword.Text.Trim()
                );

                if (success)
                {
                    // Luôn luôn hiển thị rõ thông báo thành công cho người dùng với TopMost và focus
                    MessageBox.Show(this, "Chúc mừng! " + message + "\nBạn sẽ được chuyển về màn hình đăng nhập.", "Đăng ký thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    BackToSignin?.Invoke();
                }
                else
                {
                    MessageBox.Show(this, message, "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnRegister.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btnDL_Click(object sender, EventArgs e)
        {
            btnDL.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                if (!ValidateRegistrationInputs(out string validationError))
                {
                    MessageBox.Show(this, validationError, "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Modify modify = new Modify();
                var (success, message) = modify.RegisterCustomerDLVer(
                    txtFullName.Text.Trim(),
                    dtpBirthday.Value.Date,
                    txtPhoneNumber.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtAddress.Text.Trim(),
                    txtUsername.Text.Trim(),
                    txtPassword.Text.Trim()
                );

                if (success)
                {
                    MessageBox.Show(this, "Chúc mừng! " + message + "\nBạn sẽ được chuyển về màn hình đăng nhập.", "Đăng ký thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    BackToSignin?.Invoke();
                }
                else
                {
                    MessageBox.Show(this, message, "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnDL.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void ClearForm()
        {
            txtFullName.Clear();
            txtPhoneNumber.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            dtpBirthday.Value = new DateTime(2000, 1, 1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BackToSignin?.Invoke();
        }
    }
}
