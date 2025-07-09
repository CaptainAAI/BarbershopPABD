using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barbershop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Tangkap semua unhandled exception di thread UI
            Application.ThreadException += (sender, args) =>
            {
                if (args.Exception is System.Data.SqlClient.SqlException ||
                    args.Exception.Message.ToLower().Contains("network") ||
                    args.Exception.Message.ToLower().Contains("timeout"))
                {
                    MessageBox.Show("Koneksi ke server gagal. Periksa jaringan Anda.", "Koneksi Terputus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Isi dengan benar", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // Tangkap unhandled exception di luar thread UI
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                if (ex is System.Data.SqlClient.SqlException ||
                    ex?.Message.ToLower().Contains("network") == true ||
                    ex?.Message.ToLower().Contains("timeout") == true)
                {
                    MessageBox.Show("Koneksi ke server gagal. Periksa jaringan Anda.", "Koneksi Terputus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Isi dengan benar", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            Application.Run(new LoginForm());
        }
    }
}
