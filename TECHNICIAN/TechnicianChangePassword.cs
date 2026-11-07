using System.Drawing;
using System.Linq;
﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TECHNICIAN
{
    public partial class TechnicianChangePassword : Form
    {
        private readonly string _connStr = "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
        private readonly TechnicianMainForm _main;

        // ctor nhận MainForm
        public TechnicianChangePassword(TechnicianMainForm main)
        {
            InitializeComponent();
            _main = main;
            InitEnhancements();
        }

        // ctor mặc định (designer)
        public TechnicianChangePassword()
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
                MessageBox.Show("MainForm chưa được truyền vào.");
                return;
            }

            string accountLogin = _main.CurrentUsername; // ví dụ: TN0001 / account của technician
            string oldPass = txtOldPassword.Text.Trim();
            string newPass = txtNewPassword.Text.Trim();
            string confirm = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(accountLogin))
            {
                MessageBox.Show("Không lấy được AccountLogin từ MainForm.");
                return;
            }

            if (string.IsNullOrWhiteSpace(oldPass) ||
                string.IsNullOrWhiteSpace(newPass) ||
                string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            if (newPass != confirm)
            {
                MessageBox.Show("Confirm Password không khớp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand("sp_TN6_Tech_ChangePassword", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@AccountLogin", accountLogin);
                    cmd.Parameters.AddWithValue("@OldPassword", oldPass);
                    cmd.Parameters.AddWithValue("@NewPassword", newPass);

                    conn.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            int result = Convert.ToInt32(rd["Result"]);
                            string msg = rd["Message"].ToString();

                            MessageBox.Show(msg);

                            if (result == 1)
                            {
                                txtOldPassword.Clear();
                                txtNewPassword.Clear();
                                txtConfirmPassword.Clear();
                                txtOldPassword.Focus();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Không nhận được phản hồi từ stored procedure.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đổi mật khẩu thất bại:\n" + ex.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
