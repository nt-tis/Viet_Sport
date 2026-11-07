
using System;
using System.Windows.Forms;

namespace RECEPTIONIST   // namespace phải trùng với namespace của Login.cs
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Receiptionist_Mainform());   // <<< form nào muốn chạy thì để vào đây
        }
    }
}
