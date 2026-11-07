using System;
using System.IO;
using System.Data.SqlClient;

namespace LOGIN_E
{
    /// <summary>
    /// Helper ghi log và chuẩn hóa thông báo lỗi thân thiện cho ứng dụng
    /// </summary>
    public static class AppLogger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "application.log");
        private static readonly object LockObj = new object();

        /// <summary>
        /// Ghi nhận log hoạt động hoặc lỗi vào file log cục bộ
        /// </summary>
        public static void Log(string message, Exception ex = null)
        {
            try
            {
                lock (LockObj)
                {
                    using (StreamWriter sw = new StreamWriter(LogFilePath, true, System.Text.Encoding.UTF8))
                    {
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sw.WriteLine($"[{timestamp}] {message}");
                        if (ex != null)
                        {
                            sw.WriteLine($"    Exception: {ex.GetType().FullName}: {ex.Message}");
                            if (!string.IsNullOrEmpty(ex.StackTrace))
                            {
                                sw.WriteLine($"    StackTrace: {ex.StackTrace}");
                            }
                        }
                    }
                }
            }
            catch
            {
                // Tránh throw exception từ logger
            }
        }

        /// <summary>
        /// Chuyển đổi lỗi kỹ thuật (SqlException, v.v.) thành thông báo tiếng Việt thân thiện với người dùng
        /// </summary>
        public static string GetFriendlyErrorMessage(Exception ex)
        {
            if (ex == null) return "Đã xảy ra lỗi không xác định.";

            if (ex is SqlException sqlEx)
            {
                // Lỗi nghiệp vụ từ RAISERROR/THROW trong Stored Procedure (Number >= 50000)
                if (sqlEx.Number >= 50000)
                {
                    return sqlEx.Message;
                }

                switch (sqlEx.Number)
                {
                    case 2:     // Không tìm thấy SQL Server
                    case 53:
                        return "Không thể kết nối đến máy chủ cơ sở dữ liệu. Vui lòng kiểm tra dịch vụ SQL Server đã bật chưa.";
                    case 18456: // Lỗi xác thực đăng nhập SQL
                        return "Xác thực đăng nhập máy chủ cơ sở dữ liệu thất bại.";
                    case 2627:  // Trùng khóa chính / UNIQUE
                    case 2601:
                        return "Dữ liệu đã tồn tại trong hệ thống (trùng mã hoặc tên đăng nhập).";
                    case 547:   // Ràng buộc khóa ngoại Foreign Key
                        return "Dữ liệu đang được liên kết với thông tin khác, không thể xóa hoặc thay đổi.";
                    case 1205:  // Tranh chấp Deadlock
                        return "Hệ thống đang bận do có giao dịch đồng thời khác. Vui lòng thử lại sau giây lát.";
                    default:
                        return sqlEx.Message;
                }
            }

            return ex.Message;
        }
    }
}
