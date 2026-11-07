using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace LOGIN_C
{
    public partial class Login : Form
    {
        public event Action<Form> LoginSuccess;
        public event Action ForgotPasswordClicked;
        public event Action RegisterClicked;

        public Login()
        {
            InitializeComponent();
            txtPassword.UseSystemPasswordChar = true;

            this.KeyPreview = true;
            this.AcceptButton = btnLogin;
            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    button1_Click(this, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            txtUsername.KeyDown += enterHandler;
            txtPassword.KeyDown += enterHandler;
            this.KeyDown += enterHandler;
        }

        private void button1_Click(object sender, EventArgs e) // Login button
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập Username và Password.");
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand("dbo.sp_LoginC", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = password;


                    var pResult = new SqlParameter("@result", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pResult);

                    conn.Open();

                    string CusId = null, fullName = null;
                    string level = null;
                    decimal balance = 0;

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            CusId = rd["CustomerID"]?.ToString();
                            fullName = rd["FullName"]?.ToString();
                            balance = rd["Balance"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["Balance"]);
                            level = rd["MembershipLevel"]?.ToString();

                        }
                    }

                    int result = (pResult.Value == DBNull.Value) ? 0 : Convert.ToInt32(pResult.Value);

                    if (result == 0)
                    {
                        MessageBox.Show("Sai Username hoặc Password.");
                        return;
                    }

                    if (string.IsNullOrEmpty(CusId))
                    {
                        MessageBox.Show("Không lấy được thông tin khách hàng.");
                        return;
                    }

                    OpenByCusForm(CusId, fullName, username, level, balance);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message);
            }
        }
        

        private void OpenByCusForm(string employeeId, string fullName, string username, string level, decimal balance)
        {
            Form next = new CUSTOMER.Customer_Main(
                employeeId,
                fullName,
                "Customer",   // role
                username,
                level,
                balance
            );

            LoginSuccess?.Invoke(next);
        }


        private void llblForgotPass_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ForgotPasswordClicked?.Invoke();
        }

        private void llblRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RegisterClicked.Invoke();
        }
    }
}
