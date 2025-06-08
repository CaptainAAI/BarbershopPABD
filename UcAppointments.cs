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
            LoadComboBox("SELECT client_id, phone_number + ' - ' + first_name + ' ' + last_name AS name FROM clients", cmbClientID);
            LoadComboBox("SELECT employee_id, phone_number + ' - ' + first_name + ' ' + last_name AS name FROM employees", cmbEmployeeID);
            LoadComboBox("SELECT service_id, service_name FROM services", cmbServiceID);

            cmbClientID.SelectedIndex = -1;
            cmbEmployeeID.SelectedIndex = -1;
            cmbServiceID.SelectedIndex = -1;

            cmbStatusBooking.Items.Clear();
            cmbStatusBooking.Items.AddRange(new string[] {
                "Need Approval", "Pending", "Ongoing", "Completed", "Canceled"
            });
            cmbStatusBooking.SelectedIndex = -1;
        }

        // Helper untuk load data ke ComboBox dari query SQL
        private void LoadComboBox(string query, ComboBox comboBox)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                comboBox.DataSource = dt;
                comboBox.ValueMember = dt.Columns[0].ColumnName;
                comboBox.DisplayMember = dt.Columns[1].ColumnName;
            }
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

        // Load data appointment ke DataGridView, sekaligus maintenance data
        private void LoadAppointments()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // Hapus appointment yang lebih dari 3 bulan lalu
                string deleteOldQuery = @"
            DELETE FROM appointments
            WHERE start_time < DATEADD(MONTH, -3, GETDATE())";

                using (SqlCommand deleteCmd = new SqlCommand(deleteOldQuery, conn))
                {
                    deleteCmd.ExecuteNonQuery();
                }

                // Update status booking otomatis berdasarkan waktu
                string updateStatusQuery = @"
            UPDATE appointments
            SET StatusBooking = 
            CASE
                WHEN StatusBooking = 'Need Approval' AND GETDATE() >= start_time THEN 'Canceled'
                WHEN GETDATE() >= end_time_expected AND StatusBooking != 'Canceled' THEN 'Completed'
                WHEN GETDATE() >= start_time AND GETDATE() < end_time_expected AND StatusBooking NOT IN ('Need Approval', 'Canceled') THEN 'Ongoing'
                WHEN GETDATE() < start_time AND StatusBooking NOT IN ('Need Approval', 'Canceled') THEN 'Pending'
                ELSE StatusBooking
            END";

                using (SqlCommand updateCmd = new SqlCommand(updateStatusQuery, conn))
                {
                    updateCmd.ExecuteNonQuery();
                }

                // Load data appointment ke grid
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM appointments", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

        // Generate ID appointment baru dengan format AIxxxxxx
        private string GenerateAppointmentID()
        {
            string prefix = "AI";
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
                    newID = prefix + number.ToString("D6");
                }
            }

            return newID;
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

        // Event klik tombol Add, menambah appointment baru ke database
        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO appointments
                        (appointment_id, client_id, employee_id, service_id, start_time, end_time_expected, cancellation_reason, StatusBooking)
                        VALUES
                        (@id, @client, @employee, @service, @start, @end, @reason, @status)";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    string id = GenerateAppointmentID();
                    string client = cmbClientID.SelectedValue.ToString();
                    string employee = cmbEmployeeID.SelectedValue?.ToString();
                    string service = cmbServiceID.SelectedValue.ToString();

                    DateTime tanggal = dtpTanggal.Value.Date;
                    TimeSpan jamMulai = TimeSpan.Parse(cmbStartTime.SelectedItem.ToString());
                    DateTime start = tanggal + jamMulai;
                    int durasi = GetServiceDuration(service);
                    DateTime end = start.AddMinutes(durasi);

                    string status = cmbStatusBooking.SelectedItem?.ToString() ?? "Need Approval";

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@client", client);
                    cmd.Parameters.AddWithValue("@employee", string.IsNullOrEmpty(employee) ? DBNull.Value : (object)employee);
                    cmd.Parameters.AddWithValue("@service", service);
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(txtCancellationReason.Text) ? DBNull.Value : (object)txtCancellationReason.Text);
                    cmd.Parameters.AddWithValue("@status", status);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Appointment berhasil ditambahkan!");
                    LoadAppointments();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menambahkan: " + ex.Message);
                }
            }
        }

        // Event klik tombol Update, memperbarui data appointment yang dipilih
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
                try
                {
                    conn.Open();
                    string query = @"
                UPDATE appointments SET 
                    client_id = @client,
                    employee_id = @employee,
                    service_id = @service,
                    start_time = @start,
                    end_time_expected = @end,
                    cancellation_reason = @reason,
                    StatusBooking = @status
                WHERE appointment_id = @id";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    string client = cmbClientID.SelectedValue.ToString();
                    string employee = cmbEmployeeID.SelectedValue?.ToString();
                    string service = cmbServiceID.SelectedValue.ToString();
                    DateTime tanggal = dtpTanggal.Value.Date;
                    TimeSpan jamMulai = TimeSpan.Parse(cmbStartTime.SelectedItem.ToString());
                    DateTime start = tanggal + jamMulai;
                    int durasi = GetServiceDuration(service);
                    DateTime end = start.AddMinutes(durasi);

                    string status = cmbStatusBooking.SelectedItem?.ToString() ?? "Need Approval";

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@client", client);
                    cmd.Parameters.AddWithValue("@employee", string.IsNullOrEmpty(employee) ? DBNull.Value : (object)employee);
                    cmd.Parameters.AddWithValue("@service", service);
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(txtCancellationReason.Text) ? DBNull.Value : (object)txtCancellationReason.Text);
                    cmd.Parameters.AddWithValue("@status", status);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Appointment berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAppointments();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal update: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Refresh, reload data dan reset form
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAppointments();
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
