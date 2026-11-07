using System;

namespace CASHIER
{
    /// <summary>
    /// Quản lý cấu hình kết nối database chung cho toàn bộ ứng dụng CASHIER
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// Connection string chung cho tất cả forms
        /// </summary>
        public static readonly string ConnectionString = "Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        /// <summary>
        /// Kiểm tra kết nối database
        /// </summary>
        /// <returns>True nếu kết nối thành công</returns>
        public static bool TestConnection()
        {
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy connection string với timeout tùy chỉnh
        /// </summary>
        /// <param name="timeoutSeconds">Thời gian timeout (giây)</param>
        /// <returns>Connection string với timeout</returns>
        public static string GetConnectionStringWithTimeout(int timeoutSeconds)
        {
            return ConnectionString + $";Connection Timeout={timeoutSeconds}";
        }

        /// <summary>
        /// Server name
        /// </summary>
        public static string ServerName => @"LAPTOP-LTE5438L\SQLEXPRESS";

        /// <summary>
        /// Database name
        /// </summary>
        public static string DatabaseName => "VIET_SPORT";
    }
}