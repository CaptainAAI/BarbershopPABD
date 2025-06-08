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
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Konstruktor Form
        public FormClientAppointments()
        {
            InitializeComponent();
            UpdateDateRange(); // Set range tanggal booking
            LoadLayanan();     // Load data layanan ke ComboBox
            LoadJam();         // Load pilihan jam ke ComboBox
        }

        // Event klik tombol Register, menambah client baru ke database
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
                // Tangani error duplikat nomor telepon
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    MessageBox.Show("Nomor telepon sudah terdaftar. Gunakan nomor lain atau lanjutkan booking.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Terjadi kesalahan saat registrasi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Book, menambah data booking appointment baru
        private void btnBook_Click(object sender, EventArgs e)
        {
            string phone = txtPhoneBooking.Text.Trim();
            string serviceId = cmbLayanan.SelectedValue?.ToString();
            string jamStr = cmbJam.SelectedItem?.ToString();

            // Validasi input booking
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(jamStr))
            {
                MessageBox.Show("Mohon lengkapi semua field untuk booking.");
                return;
            }

            DateTime tanggal = dtpTanggal.Value.Date;
            TimeSpan jam = TimeSpan.Parse(jamStr);
            DateTime start = tanggal + jam;

            // Validasi waktu booking tidak boleh di masa lalu
            if (start < DateTime.Now)
            {
                MessageBox.Show("Waktu booking tidak boleh di masa lalu. Silakan pilih waktu yang valid.", "Waktu Tidak Valid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

        // Memuat data layanan ke ComboBox
        private void LoadLayanan()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT service_id, service_name, service_price FROM services";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmbLayanan.DataSource = dt;
                cmbLayanan.DisplayMember = "service_name";
                cmbLayanan.ValueMember = "service_id";
                cmbLayanan.SelectedIndex = -1;
            }
        }

        // Memuat pilihan jam ke ComboBox (interval 5 menit)
        private void LoadJam()
        {
            cmbJam.Items.Clear();
            for (int jam = 0; jam <= 24; jam++)
            {
                for (int menit = 0; menit < 60; menit += 5)
                {
                    cmbJam.Items.Add($"{jam:D2}:{menit:D2}");
                }
            }
            cmbJam.SelectedIndex = -1;
        }

        // Generate ID client baru dengan format CIxxxxxx
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

        // Generate ID appointment baru dengan format AIxxxxxx
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

        // Mendapatkan client_id berdasarkan nomor telepon
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

        // Mendapatkan durasi layanan (dalam menit) berdasarkan service_id
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

        // Memuat status booking berdasarkan nomor telepon client
        private void LoadStatusBookingByPhone()
        {
            string phoneInput = txtCheckStatus.Text.Trim();

            if (string.IsNullOrWhiteSpace(phoneInput))
            {
                MessageBox.Show("Masukkan nomor telepon untuk melihat status booking.");
                return;
            }

            string query = @"
SELECT 
    a.appointment_id, 
    a.date_created, 
    a.client_id, 
    c.phone_number AS client_phone,
    a.employee_id, 
    a.service_id,
    s.service_name,          -- tambahkan kolom service_name
    a.start_time, 
    a.end_time_expected, 
    a.StatusBooking,
    s.service_price
FROM 
    appointments a
JOIN 
    clients c ON a.client_id = c.client_id
JOIN 
    services s ON a.service_id = s.service_id
WHERE 
    c.phone_number = @Phone
    AND a.StatusBooking IN ('Need Approval', 'Pending', 'Ongoing')
ORDER BY a.date_created DESC";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Phone", phoneInput);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Hilangkan kolom cancellation_reason karena sudah dihapus dari query

                dgvStatusBooking.DataSource = dt;

                // Format tanggal dan waktu
                dgvStatusBooking.Columns["start_time"].DefaultCellStyle.Format = "MM/dd/yyyy hh:mm tt";
                dgvStatusBooking.Columns["end_time_expected"].DefaultCellStyle.Format = "MM/dd/yyyy hh:mm tt";

                // Atur supaya kolom service_name muncul tepat setelah service_id
                dgvStatusBooking.Columns["service_id"].DisplayIndex = GetColumnIndex(dgvStatusBooking, "service_id");
                dgvStatusBooking.Columns["service_name"].DisplayIndex = dgvStatusBooking.Columns["service_id"].DisplayIndex + 1;

                dgvStatusBooking.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Hitung total harga
                decimal totalHarga = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (decimal.TryParse(row["service_price"].ToString(), out decimal price))
                    {
                        totalHarga += price;
                    }
                }

                txtTotalHarga.Text = "Rp " + totalHarga.ToString("N2"); // Format dengan 2 desimal
            }
        }

        // Helper method untuk mendapatkan index kolom berdasarkan nama
        private int GetColumnIndex(DataGridView dgv, string columnName)
        {
            return dgv.Columns[columnName].Index;
        }

        // Event klik tombol Lihat Status, menampilkan status booking client
        private void btnLihatStatus_Click(object sender, EventArgs e)
        {
            LoadStatusBookingByPhone();
        }

        // Event klik tombol Lihat Layanan, menampilkan form daftar layanan
        private void btnLihatLayanan_Click(object sender, EventArgs e)
        {
            ListLayanan formLayanan = new ListLayanan();
            formLayanan.Show();
        }

        // Event perubahan tanggal booking (belum diimplementasi)
        private void dtpTanggal_ValueChanged(object sender, EventArgs e)
        {

        }

        // Set range tanggal booking (mulai hari ini sampai 3 bulan ke depan)
        private void UpdateDateRange()
        {
            dtpTanggal.MinDate = DateTime.Today;
            dtpTanggal.MaxDate = DateTime.Today.AddMonths(3);
        }

        // Event perubahan pilihan jam (belum diimplementasi)
        private void cmbJam_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Event perubahan pilihan layanan (belum diimplementasi)
        private void cmbLayanan_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

