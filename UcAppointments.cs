using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Barbershop
{
    public partial class UcAppointments : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Caching for ComboBox data
        private static DataTable cachedClients;
        private static DataTable cachedEmployees;
        private static DataTable cachedServices;

        // Konstruktor UserControl
        public UcAppointments()
        {
            InitializeComponent();
            UpdateDateRange(); // Set range tanggal appointment
            txtAppointmentID.ReadOnly = true; // ID appointment tidak bisa diubah manual
        }

        // Event saat UserControl dimuat
        private void UcDashboard_Load(object sender, EventArgs e)
        {
            LoadAppointments();   // Load data appointment ke grid
            LoadComboBoxes();     // Load data ke ComboBox
            LoadStartTimeCombo(); // Load pilihan jam mulai
        }

        // Load data ke ComboBox client, employee, service, dan status booking
        private void LoadComboBoxes()
        {
            cmbClientID.DataSource = GetClients();
            cmbClientID.ValueMember = "client_id";
            cmbClientID.DisplayMember = "name";

            cmbEmployeeID.DataSource = GetEmployees();
            cmbEmployeeID.ValueMember = "employee_id";
            cmbEmployeeID.DisplayMember = "name";

            cmbServiceID.DataSource = GetServices();
            cmbServiceID.ValueMember = "service_id";
            cmbServiceID.DisplayMember = "service_name";

            cmbClientID.SelectedIndex = -1;
            cmbEmployeeID.SelectedIndex = -1;
            cmbServiceID.SelectedIndex = -1;

            cmbStatusBooking.Items.Clear();
            cmbStatusBooking.Items.AddRange(new string[] {
                "Need Approval", "Pending", "Ongoing", "Completed", "Canceled"
            });
            cmbStatusBooking.SelectedIndex = -1;
        }

        // Caching helpers
        private DataTable GetClients()
        {
            if (cachedClients == null)
            {
                string query = "SELECT client_id, phone_number + ' - ' + first_name + ' ' + last_name AS name FROM clients";
                cachedClients = QueryToDataTable(query);
            }
            return cachedClients.Copy();
        }

        private DataTable GetEmployees()
        {
            if (cachedEmployees == null)
            {
                string query = "SELECT employee_id, phone_number + ' - ' + first_name + ' ' + last_name AS name FROM employees";
                cachedEmployees = QueryToDataTable(query);
            }
            return cachedEmployees.Copy();
        }

        private DataTable GetServices()
        {
            if (cachedServices == null)
            {
                string query = "SELECT service_id, service_name FROM services";
                cachedServices = QueryToDataTable(query);
            }
            return cachedServices.Copy();
        }

        private DataTable QueryToDataTable(string query)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Invalidate cache after any data change
        private void InvalidateComboBoxCache()
        {
            cachedClients = null;
            cachedEmployees = null;
            cachedServices = null;
        }

        // Load pilihan jam mulai (05:00 - 23:59, tiap menit)
        private void LoadStartTimeCombo()
        {
            cmbStartTime.Items.Clear();
            for (int jam = 5; jam <= 23; jam++)
            {
                for (int menit = 0; menit < 60; menit += 1)
                {
                    cmbStartTime.Items.Add(new TimeSpan(jam, menit, 0).ToString(@"hh\:mm"));
                }
            }
            cmbStartTime.SelectedIndex = -1;
        }

        // Load data appointment ke DataGridView menggunakan stored procedure
        private void LoadAppointments()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("sp_get_appointments", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
        }

        // Ambil durasi layanan (dalam menit) berdasarkan service_id
        private int GetServiceDuration(string serviceID)
        {
            int duration = 30;
            string query = "SELECT service_duration FROM services WHERE service_id = @id";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", serviceID);
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                    duration = Convert.ToInt32(result);
            }

            return duration;
        }

        // Event klik tombol Add, menambah appointment baru ke database via stored procedure
        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_insert_appointment", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        string client = cmbClientID.SelectedValue.ToString();
                        string employee = cmbEmployeeID.SelectedValue?.ToString();
                        string service = cmbServiceID.SelectedValue.ToString();

                        DateTime tanggal = dtpTanggal.Value.Date;
                        TimeSpan jamMulai = TimeSpan.Parse(cmbStartTime.SelectedItem.ToString());
                        DateTime start = tanggal + jamMulai;
                        int durasi = GetServiceDuration(service);
                        DateTime end = start.AddMinutes(durasi);

                        string status = cmbStatusBooking.SelectedItem?.ToString();

                        cmd.Parameters.AddWithValue("@client_id", client);
                        cmd.Parameters.AddWithValue("@employee_id", string.IsNullOrEmpty(employee) ? (object)DBNull.Value : employee);
                        cmd.Parameters.AddWithValue("@service_id", service);
                        cmd.Parameters.AddWithValue("@start_time", start);
                        cmd.Parameters.AddWithValue("@end_time_expected", end);
                        cmd.Parameters.AddWithValue("@cancellation_reason", string.IsNullOrEmpty(txtCancellationReason.Text) ? (object)DBNull.Value : txtCancellationReason.Text);
                        cmd.Parameters.AddWithValue("@StatusBooking", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                        var result = cmd.ExecuteScalar();
                        transaction.Commit();
                        MessageBox.Show("Appointment berhasil ditambahkan! ID: " + result?.ToString());
                    }
                    LoadAppointments();
                    InvalidateComboBoxCache();
                    LoadComboBoxes();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menambahkan: " + ex.Message);
                }
            }
        }


        // Event klik tombol Update, memperbarui data appointment yang dipilih via stored procedure
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            var result = MessageBox.Show(
                "Apakah kamu yakin ingin memperbarui appointment ini?",
                "Konfirmasi Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes) return;

            string id = txtAppointmentID.Text;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_update_appointment", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        string client = cmbClientID.SelectedValue.ToString();
                        string employee = cmbEmployeeID.SelectedValue?.ToString();
                        string service = cmbServiceID.SelectedValue.ToString();
                        DateTime tanggal = dtpTanggal.Value.Date;
                        TimeSpan jamMulai = TimeSpan.Parse(cmbStartTime.SelectedItem.ToString());
                        DateTime start = tanggal + jamMulai;
                        int durasi = GetServiceDuration(service);
                        DateTime end = start.AddMinutes(durasi);

                        string status = cmbStatusBooking.SelectedItem?.ToString();

                        cmd.Parameters.AddWithValue("@appointment_id", id);
                        cmd.Parameters.AddWithValue("@client_id", client);
                        cmd.Parameters.AddWithValue("@employee_id", string.IsNullOrEmpty(employee) ? (object)DBNull.Value : employee);
                        cmd.Parameters.AddWithValue("@service_id", service);
                        cmd.Parameters.AddWithValue("@start_time", start);
                        cmd.Parameters.AddWithValue("@end_time_expected", end);
                        cmd.Parameters.AddWithValue("@cancellation_reason", string.IsNullOrEmpty(txtCancellationReason.Text) ? (object)DBNull.Value : txtCancellationReason.Text);
                        cmd.Parameters.AddWithValue("@StatusBooking", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        MessageBox.Show("Appointment berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    LoadAppointments();
                    InvalidateComboBoxCache();
                    LoadComboBoxes();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal update: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        // Event klik tombol Refresh, reload data dan reset form
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAppointments();
            InvalidateComboBoxCache(); // Invalidate cache on refresh
            LoadComboBoxes();
            LoadStartTimeCombo();
            ClearForm();
        }

        // Reset semua input form ke default
        private void ClearForm()
        {
            txtAppointmentID.Clear();
            cmbClientID.SelectedIndex = -1;
            cmbEmployeeID.SelectedIndex = -1;
            cmbServiceID.SelectedIndex = -1;
            cmbStartTime.SelectedIndex = -1;
            cmbStatusBooking.SelectedIndex = -1;
            txtCancellationReason.Clear();
            dtpTanggal.Value = DateTime.Now;
        }

        // Event klik pada DataGridView, load data ke form input
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].IsNewRow)
            {
                MessageBox.Show("Data tidak valid atau kosong. Silakan pilih baris yang memiliki data.",
                                "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            txtAppointmentID.Text = row.Cells["appointment_id"].Value?.ToString();
            cmbClientID.SelectedValue = row.Cells["client_id"].Value?.ToString();

            object empValue = row.Cells["employee_id"].Value;
            if (empValue != DBNull.Value && empValue != null)
                cmbEmployeeID.SelectedValue = empValue.ToString();
            else
                cmbEmployeeID.SelectedIndex = -1; // kosongkan pilihan jika null

            cmbServiceID.SelectedValue = row.Cells["service_id"].Value?.ToString();

            if (row.Cells["start_time"].Value != DBNull.Value)
            {
                DateTime startTime = Convert.ToDateTime(row.Cells["start_time"].Value);
                dtpTanggal.Value = startTime.Date;
                cmbStartTime.SelectedItem = startTime.ToString("HH:mm");
            }

            txtCancellationReason.Text = row.Cells["cancellation_reason"].Value?.ToString();
            cmbStatusBooking.SelectedItem = row.Cells["StatusBooking"].Value?.ToString();
        }

        // Event perubahan tanggal appointment (belum diimplementasi)
        private void dtpTanggal_ValueChanged(object sender, EventArgs e)
        {

        }

        // Set range tanggal appointment (3 bulan sebelum dan sesudah hari ini)
        private void UpdateDateRange()
        {
            dtpTanggal.MinDate = DateTime.Today.AddMonths(-3);
            dtpTanggal.MaxDate = DateTime.Today.AddMonths(3);
        }

        // Event perubahan pilihan employee (belum diimplementasi)
        private void cmbEmployeeID_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}