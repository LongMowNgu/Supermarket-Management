using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
namespace QuanLyBanHangSupermarket
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("vi-VN");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("vi-VN");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Ui.Error(e.Exception);
            using (var login = new LoginForm())
                if (login.ShowDialog() == DialogResult.OK) Application.Run(new MainForm());
        }
    }
}