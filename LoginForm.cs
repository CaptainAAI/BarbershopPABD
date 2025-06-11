using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barbershop
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Menaruh panel1 di tengah saat form pertama kali dibuka
            panel1.Left = (this.ClientSize.Width - panel1.Width) / 2;
            panel1.Top = (this.ClientSize.Height - panel1.Height) / 2;
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "" || password == "")
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }


            // Ganti connection string sesuai server dan database kamu
            string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT COUNT(*) FROM barber_admin WHERE username = @username AND password = @password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password); // Belum di-hash

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                if (count == 1)
                {
                    MessageBox.Show("Login successful!", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();
                    // Ganti MainForm ke form utama admin kamu
                    MainForm dashboard = new MainForm();
                    dashboard.Show();
                }
                else
                {
                    lblMessage.Text = "Incorrect username or password.";
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnClient_Click(object sender, EventArgs e)
        {
            FormClientAppointments clientForm = new FormClientAppointments(); // versi tanpa client_id
            clientForm.Show();
            this.Hide(); //sembunyikan form login
        }
    }
}


