using System.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ADMIN
{
    public class Admin_ChangePassword : Form
    {
        private readonly string _employeeId;
        private readonly string _fullName;
        private readonly string _username;

        private Panel panelMain;
        private TextBox txtOldPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnSubmit;

        public Admin_ChangePassword(string employeeId, string fullName, string username)
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _username = username;

            InitializeUI();
        }

        public Admin_ChangePassword() : this("EMP0021", "Administrator", "admin") { }

        private void InitializeUI()
        {
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Size = new Size(1000, 600);

            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };
            this.Controls.Add(panelMain);

            Label lblTitle = new Label
            {
                Text = "ĐỔI MẬT KHẨU TÀI KHOẢN QUẢN TRỊ VIÊN (ADMIN)",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 116, 64),
                AutoSize = true,
                Location = new Point(30, 25)
            };
            panelMain.Controls.Add(lblTitle);

            GroupBox gb = new GroupBox
            {
                Text = "Thông tin tài khoản Admin",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(35, 75),
                Size = new Size(600, 360),
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            panelMain.Controls.Add(gb);

            GroupBox gbReq = new GroupBox
            {
                Text = "Password Information",
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
                Location = new Point(650, 75),
                Size = new Size(300, 125),
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            panelMain.Controls.Add(gbReq);

            Label lblReq = new Label
            {
                Text = "Password Requirements:\r\n- Minimum 8 characters\r\n- At least one uppercase letter (A-Z)\r\n- At least one number (0-9)",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                Location = new Point(16, 28),
                AutoSize = true
            };
            gbReq.Controls.Add(lblReq);

            Action updateReqUI = () =>
            {
                string pwd = txtNewPassword.Text.Trim();
                bool hasMin8 = pwd.Length >= 8;
                bool hasUpper = pwd.Any(char.IsUpper);
                bool hasDigit = pwd.Any(char.IsDigit);

                string line1 = hasMin8 ? "- Minimum 8 characters" : "- Minimum 8 characters";
                string line2 = hasUpper ? "- At least one uppercase letter (A-Z)" : "- At least one uppercase letter (A-Z)";
                string line3 = hasDigit ? "- At least one number (0-9)" : "- At least one number (0-9)";

                lblReq.Text = $"Password Requirements:\r\n{line1}\r\n{line2}\r\n{line3}";
                lblReq.ForeColor = (hasMin8 && hasUpper && hasDigit) ? Color.ForestGreen : Color.Gray;
            };
            txtNewPassword.TextChanged += (s, ev) => updateReqUI();

            this.KeyPreview = true;
            this.AcceptButton = btnSubmit;

            KeyEventHandler enterHandler = (s, ev) =>
            {
                if (ev.KeyCode == Keys.Enter)
                {
                    BtnSubmit_Click(this, EventArgs.Empty);
                    ev.Handled = true;
                }
            };
            txtOldPassword.KeyDown += enterHandler;
            txtNewPassword.KeyDown += enterHandler;
            txtConfirmPassword.KeyDown += enterHandler;
            this.KeyDown += enterHandler;

            Label lblEmp = new Label
            {
                Text = $"Quản trị viên: {_fullName} ({_employeeId}) | Username: {_username}",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.DarkSlateGray,
                Location = new Point(30, 35),
                AutoSize = true
            };
            gb.Controls.Add(lblEmp);

            // Old password
            Label lblOld = new Label { Text = "Mật khẩu hiện tại:", Location = new Point(30, 75), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            txtOldPassword = new TextBox { Location = new Point(30, 98), Width = 520, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10F) };
            gb.Controls.Add(lblOld);
            gb.Controls.Add(txtOldPassword);

            // New password
            Label lblNew = new Label { Text = "Mật khẩu mới (Tối thiểu 8 ký tự, có chữ HOA và số):", Location = new Point(30, 140), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            txtNewPassword = new TextBox { Location = new Point(30, 163), Width = 520, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10F) };
            gb.Controls.Add(lblNew);
            gb.Controls.Add(txtNewPassword);

            // Confirm password
            Label lblConfirm = new Label { Text = "Xác nhận mật khẩu mới:", Location = new Point(30, 205), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            txtConfirmPassword = new TextBox { Location = new Point(30, 228), Width = 520, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10F) };
            gb.Controls.Add(lblConfirm);
            gb.Controls.Add(txtConfirmPassword);

            // Submit Button
            btnSubmit = new Button
            {
                Text = "Cập Nhật Mật Khẩu",
                Location = new Point(30, 280),
                Size = new Size(220, 42),
                BackColor = Color.FromArgb(24, 116, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += BtnSubmit_Click;
            gb.Controls.Add(btnSubmit);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            string oldPass = txtOldPassword.Text.Trim();
            string newPass = txtNewPassword.Text.Trim();
            string confirm = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirm))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirm)
            {
                MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
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

            try
            {
                using (SqlConnection conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand verifyCmd = new SqlCommand("sp_Cashier_VerifyPassword", conn))
                    {
                        verifyCmd.CommandType = CommandType.StoredProcedure;
                        verifyCmd.Parameters.AddWithValue("@EmployeeID", _employeeId);
                        verifyCmd.Parameters.AddWithValue("@Password", oldPass);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(verifyCmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dt.Rows.Count == 0)
                            {
                                MessageBox.Show("Mật khẩu hiện tại không chính xác!", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtOldPassword.Focus();
                                return;
                            }
                        }
                    }

                    using (SqlCommand updateCmd = new SqlCommand("sp_Cashier_UpdatePassword", conn))
                    {
                        updateCmd.CommandType = CommandType.StoredProcedure;
                        updateCmd.Parameters.AddWithValue("@EmployeeID", _employeeId);
                        updateCmd.Parameters.AddWithValue("@NewPassword", newPass);

                        using (SqlDataAdapter updateAdapter = new SqlDataAdapter(updateCmd))
                        {
                            DataTable dtRes = new DataTable();
                            updateAdapter.Fill(dtRes);
                            if (dtRes.Rows.Count > 0 && Convert.ToInt32(dtRes.Rows[0]["Result"]) == 1)
                            {
                                MessageBox.Show("Đổi mật khẩu tài khoản Admin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                txtOldPassword.Clear();
                                txtNewPassword.Clear();
                                txtConfirmPassword.Clear();
                            }
                            else
                            {
                                MessageBox.Show("Cập nhật mật khẩu thất bại. Vui lòng thử lại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
