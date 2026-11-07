using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOGIN_E
{
    internal class Modify
    {
        public Modify()
        {

        }

        public int ResetPassword(string username, string cccd, string phone, string newPassword)
        {
            int res = 0;
            string query = "sp_ForgotPasswordE";

            try
            {
                using (SqlConnection sqlConnection = Connection.GetConnection())
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@CCCD", cccd);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@NewPassword", newPassword);

                        SqlParameter resultParam = new SqlParameter("@result", SqlDbType.Int);
                        resultParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(resultParam);

                        cmd.ExecuteNonQuery();

                        if (resultParam.Value != DBNull.Value)
                        {
                            res = Convert.ToInt32(resultParam.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thực thi đổi mật khẩu: " + ex.Message);
                res = -1;
            }

            return res;
        }

        public (bool success, string message) RegisterCustomer(string fullName, DateTime birthday, string phone, string email, string address, string account, string password)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_DangKyKhachHang", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Birthday", birthday);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Address", address);
                        cmd.Parameters.AddWithValue("@AccountLogin", account);
                        cmd.Parameters.AddWithValue("@Password", password);

                        cmd.ExecuteNonQuery();
                        return (true, "Đăng ký tài khoản thành công!");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                return (false, sqlEx.Message);
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống: " + ex.Message);
            }
        }

        public (bool success, string message) RegisterCustomerDLVer(string fullName, DateTime birthday, string phone, string email, string address, string account, string password)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_DangKyKhachHang_DeadLock1", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Birthday", birthday);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Address", address);
                        cmd.Parameters.AddWithValue("@AccountLogin", account);
                        cmd.Parameters.AddWithValue("@Password", password);

                        cmd.ExecuteNonQuery();
                        return (true, "Đăng ký tài khoản thành công (Deadlock version)!");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                return (false, sqlEx.Message);
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống: " + ex.Message);
            }
        }
    }
}
