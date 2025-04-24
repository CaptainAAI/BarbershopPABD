using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Barbershop
{
    public partial class FormClientAppointments : Form
    {
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=:4GuNg210105182040;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        public FormClientAppointments()
        {
            InitializeComponent();
            LoadLayanan();
            LoadJam();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string clientId = GenerateClientID();
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "INSERT INTO clients (client_id, first_name, last_name, phone_number, client_email) " +
                                   "VALUES (@id, @first, @last, @phone, @mail)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", clientId);
                    cmd.Parameters.AddWithValue("@first", firstName);
                    cmd.Parameters.AddWithValue("@last", lastName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@mail", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Registrasi berhasil. Silakan lanjut isi data booking di bawah.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // error code untuk duplicate key
                {
                    MessageBox.Show("Nomor telepon sudah terdaftar. Gunakan nomor lain atau lanjutkan booking.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Terjadi kesalahan saat registrasi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnBook_Click(object sender, EventArgs e)
        {
            string phone = txtPhoneBooking.Text.Trim();
            string serviceId = cmbLayanan.SelectedValue?.ToString();
            string jamStr = cmbJam.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(jamStr))
            {
                MessageBox.Show("Mohon lengkapi semua field untuk booking.");
                return;
            }

            DateTime tanggal = dtpTanggal.Value.Date;
            TimeSpan jam = TimeSpan.Parse(jamStr);
            DateTime start = tanggal + jam;
            DateTime end = start.AddMinutes(GetServiceDuration(serviceId));

            string clientId = GetClientIdByPhone(phone);
            if (clientId == null)
            {
                MessageBox.Show("Nomor tidak ditemukan. Silakan daftar terlebih dahulu.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string id = GenerateAppointmentID();
                string query = @"INSERT INTO appointments (appointment_id, client_id, service_id, start_time, end_time_expected, StatusBooking)
                                 VALUES (@id, @client, @service, @start, @end, 'Need Approval')";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@client", clientId);
                cmd.Parameters.AddWithValue("@service", serviceId);
                cmd.Parameters.AddWithValue("@start", start);
                cmd.Parameters.AddWithValue("@end", end);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Booking berhasil, menunggu persetujuan admin.");
        }

        private void LoadLayanan()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT service_id, service_name FROM services";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmbLayanan.DataSource = dt;
                cmbLayanan.DisplayMember = "service_name";
                cmbLayanan.ValueMember = "service_id";
                cmbLayanan.SelectedIndex = -1;
            }
        }

        private void LoadJam()
        {
            cmbJam.Items.Clear();
            for (int jam = 9; jam <= 21; jam++)
            {
                for (int menit = 0; menit < 60; menit += 5)
                {
                    cmbJam.Items.Add($"{jam:D2}:{menit:D2}");
                }
            }
            cmbJam.SelectedIndex = -1;
        }

        private string GenerateClientID()
        {
            string newID = "CI000001";
            string query = "SELECT TOP 1 client_id FROM clients WHERE client_id LIKE 'CI%' ORDER BY client_id DESC";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string lastID = result.ToString();
                    int number = int.Parse(lastID.Substring(2)) + 1;
                    newID = "CI" + number.ToString("D6");
                }
            }

            return newID;
        }

        private string GenerateAppointmentID()
        {
            string newID = "AI000001";
            string query = "SELECT TOP 1 appointment_id FROM appointments WHERE appointment_id LIKE 'AI%' ORDER BY appointment_id DESC";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string lastID = result.ToString();
                    int number = int.Parse(lastID.Substring(2)) + 1;
                    newID = "AI" + number.ToString("D6");
                }
            }

            return newID;
        }

        private string GetClientIdByPhone(string phone)
        {
            string query = "SELECT client_id FROM clients WHERE phone_number = @phone";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@phone", phone);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }

        private int GetServiceDuration(string serviceID)
        {
            string query = "SELECT service_duration FROM services WHERE service_id = @id";
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", serviceID);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 30;
            }
        }
    }
}