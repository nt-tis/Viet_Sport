using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOGIN_C
{
    internal class Modify
    {
        public Modify()
        {

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

        public int ResetPassword(string username, string email, string phone, string newPassword)
        {
            int res = 0;
            string query = "sp_ForgotPasswordC";

            try
            {
                using (SqlConnection sqlConnection = Connection.GetConnection())
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // 1. Thêm các tham số đầu vào (Input)
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@NewPassword", newPassword);

                        // 2. Cấu hình tham số đầu ra (Output) để nhận kết quả trả về
                        SqlParameter resultParam = new SqlParameter("@result", SqlDbType.Int);
                        resultParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(resultParam);

                        // 3. Thực thi Procedure
                        cmd.ExecuteNonQuery();

                        // 4. Lấy giá trị từ biến Output
                        if (resultParam.Value != DBNull.Value)
                        {
                            res = Convert.ToInt32(resultParam.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi chi tiết nếu có
                MessageBox.Show("Lỗi thực thi đổi mật khẩu: " + ex.Message);
                res = -1; // Trả về mã lỗi hệ thống
            }

            return res;
        }
    }
}
