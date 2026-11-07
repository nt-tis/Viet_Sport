using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using TECHNICIAN;
using ADMIN;
using CASHIER;
using RECEPTIONIST;
using MANAGER;
using CUSTOMER;

namespace LOGIN_E
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
        }

        private void button1_Click(object sender, EventArgs e) // Login button
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập Username và Password.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. Kiểm tra tài khoản nhân viên
                if (TryLoginEmployee(username, password, out string empId, out string empName, out string role))
                {
                    AppLogger.Log($"Đăng nhập thành công nhân viên: {username} - {empName} ({role})");
                    OpenByRole(empId, empName, role, username);
                    return;
                }

                // 2. Kiểm tra tài khoản khách hàng
                int cusResult = TryLoginCustomer(username, password, out string cusId, out string cusName, out string level, out decimal balance);
                if (cusResult == 1) // Hợp lệ và đã được liên kết với hồ sơ khách hàng
                {
                    AppLogger.Log($"Đăng nhập thành công khách hàng: {username} - {cusName} ({cusId})");
                    OpenCustomer(cusId, cusName, username, level, balance);
                    return;
                }
                else if (cusResult == 2) // Tài khoản khách hàng đúng nhưng chưa liên kết với bất kỳ hồ sơ khách hàng nào
                {
                    AppLogger.Log($"Cảnh báo: Tài khoản {username} chưa xác thực liên kết hồ sơ khách hàng.");
                    MessageBox.Show("Thông tin khách hàng chưa được xác thực.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Không khớp cả nhân viên lẫn khách hàng
                AppLogger.Log($"Đăng nhập thất bại cho tài khoản: {username}");
                MessageBox.Show("Sai Username hoặc Password.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                AppLogger.Log("Ngoại lệ khi đăng nhập tài khoản " + username, ex);
                string friendly = AppLogger.GetFriendlyErrorMessage(ex);
                MessageBox.Show("Lỗi đăng nhập: " + friendly, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryLoginEmployee(string username, string password, out string empId, out string fullName, out string role)
        {
            empId = null;
            fullName = null;
            role = null;

            using (SqlConnection conn = Connection.GetConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.sp_LoginE", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = password;

                var pResult = new SqlParameter("@result", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(pResult);

                conn.Open();

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        empId = rd["EmployeeID"]?.ToString();
                        fullName = rd["FullName"]?.ToString();
                        role = rd["Role"]?.ToString();
                    }
                }

                int result = (pResult.Value == DBNull.Value) ? 0 : Convert.ToInt32(pResult.Value);
                return result == 1 && !string.IsNullOrWhiteSpace(role);
            }
        }

        private int TryLoginCustomer(string username, string password, out string cusId, out string fullName, out string level, out decimal balance)
        {
            cusId = null;
            fullName = null;
            level = null;
            balance = 0;

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

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        cusId = rd["CustomerID"]?.ToString();
                        fullName = rd["FullName"]?.ToString();
                        balance = (rd["Balance"] == DBNull.Value) ? 0 : Convert.ToDecimal(rd["Balance"]);
                        level = rd["MembershipLevel"]?.ToString();
                    }
                }

                int result = (pResult.Value == DBNull.Value) ? 0 : Convert.ToInt32(pResult.Value);
                return result;
            }
        }

        private void OpenByRole(string employeeId, string fullName, string role, string username)
        {
            string r = role.Trim();
            Form next = null;

            switch (r)
            {
                case "Admin":
                    next = new Admin_Mainform(employeeId, fullName, role, username);
                    break;
                case "Manager":
                    next = new ManagerMainFrom(employeeId, fullName, role, username);
                    break;
                case "Technician":
                    next = new TechnicianMainForm(employeeId, fullName, role, username);
                    break;
                case "Cashier":
                    next = new Cashier_Main(employeeId, fullName, role, username);
                    break;
                case "Receptionist":
                    next = new Receiptionist_Mainform(employeeId, fullName, role, username);
                    break;
                default:
                    MessageBox.Show("Role không hợp lệ: " + role);
                    return;
            }
            LoginSuccess?.Invoke(next);
        }

        private void OpenCustomer(string customerId, string fullName, string username, string level, decimal balance)
        {
            Form next = new Customer_Main(
                customerId,
                fullName,
                "Customer",
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
            RegisterClicked?.Invoke();
        }
    }
}
