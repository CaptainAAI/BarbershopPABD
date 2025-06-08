using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Barbershop
{
    public partial class previewForm : Form
    {
        // String koneksi ke database SQL Azure
        private DataTable previewData;
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Konstruktor form preview, menerima DataTable hasil import Excel
        public previewForm(DataTable dt)
        {
            InitializeComponent();
            previewData = dt;
            dgvPreview.DataSource = previewData; // Tampilkan data ke DataGridView
        }

        // Event saat form dimuat, pastikan data tetap tampil
        private void previewForm_Load(object sender, EventArgs e)
        {
            dgvPreview.DataSource = previewData;
        }

        // Event klik tombol Import, validasi dan simpan data ke database
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // Loop setiap baris data yang di-preview
                foreach (DataRow row in previewData.Rows)
                {
                    string empId = row["employee_id"].ToString().Trim();
                    string empName = row["employee_name"].ToString().Trim();
                    string svcName = row["service_name"].ToString().Trim();
                    string svcPriceStr = row["service_price"].ToString().Trim();

                    // Validasi harga layanan harus numerik
                    if (!decimal.TryParse(svcPriceStr, out decimal svcPrice))
                    {
                        MessageBox.Show($"Baris dengan harga tidak valid: {svcPriceStr}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Validasi employee harus ada di database
                    string empQuery = @"SELECT COUNT(*) FROM employees WHERE employee_id = @id AND first_name + ' ' + last_name = @name";
                    using (SqlCommand cmd = new SqlCommand(empQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", empId);
                        cmd.Parameters.AddWithValue("@name", empName);
                        if ((int)cmd.ExecuteScalar() == 0)
                        {
                            MessageBox.Show($"Pegawai tidak ditemukan: {empId} - {empName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Validasi service harus ada di database
                    string svcQuery = @"SELECT COUNT(*) FROM services WHERE service_name = @name AND service_price = @price";
                    using (SqlCommand cmd = new SqlCommand(svcQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", svcName);
                        cmd.Parameters.AddWithValue("@price", svcPrice);
                        if ((int)cmd.ExecuteScalar() == 0)
                        {
                            MessageBox.Show($"Layanan tidak valid: {svcName} - {svcPriceStr}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Insert data ke tabel transaction_history
                    string insertQuery = @"
                INSERT INTO transaction_history (
                    appointment_id, client_name, phone_number,
                    employee_id, employee_name, service_name, service_price,
                    appointment_date, start_time, end_time, status, cancellation_reason, recorded_at
                )
                VALUES (
                    NULL, @client_name, @phone_number, @employee_id, @employee_name,
                    @service_name, @service_price, @appointment_date, @start_time,
                    @end_time, @status, @cancellation_reason, GETDATE()
                )";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@client_name", row["client_name"]);
                        cmd.Parameters.AddWithValue("@phone_number", row["phone_number"]);
                        cmd.Parameters.AddWithValue("@employee_id", empId);
                        cmd.Parameters.AddWithValue("@employee_name", empName);
                        cmd.Parameters.AddWithValue("@service_name", svcName);
                        cmd.Parameters.AddWithValue("@service_price", svcPrice);
                        cmd.Parameters.AddWithValue("@appointment_date", DateTime.Parse(row["appointment_date"].ToString()));
                        cmd.Parameters.AddWithValue("@start_time", TimeSpan.Parse(row["start_time"].ToString()));
                        cmd.Parameters.AddWithValue("@end_time", TimeSpan.Parse(row["end_time"].ToString()));
                        cmd.Parameters.AddWithValue("@status", row["status"]);
                        // Kolom cancellation_reason opsional
                        cmd.Parameters.AddWithValue("@cancellation_reason", row.Table.Columns.Contains("cancellation_reason") ? row["cancellation_reason"]?.ToString() : DBNull.Value.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Data berhasil diimport ke database.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Tutup form setelah selesai
            }
        }

        // Event klik tombol Cancel, menutup form preview tanpa import
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

