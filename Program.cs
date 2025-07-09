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
                MessageBox.Show("Isi dengan benar", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            // Tangkap unhandled exception di luar thread UI
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                MessageBox.Show("Isi dengan benar", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            Application.Run(new LoginForm());
        }
    }
}
