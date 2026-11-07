using LOGIN_E;
using System;
using System.Windows.Forms;

namespace LOGIN_E   // namespace phải trùng với namespace của Login.cs
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginMainForm());   // <<< form nào muốn chạy thì để vào đây
        }
    }
}
