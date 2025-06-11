using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Barbershop
{
    public partial class previewForm : Form
    {
        // Menyimpan data hasil import Excel untuk preview dan proses import
        private DataTable previewData;

        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2  ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Konstruktor form preview, menerima DataTable hasil import Excel
        public previewForm(DataTable dt)
        {
            InitializeComponent(); // Inisialisasi komponen form
            previewData = dt; // Simpan data hasil import ke variabel lokal
            dgvPreview.DataSource = previewData; // Tampilkan data ke DataGridView untuk preview
        }

        // Event saat form dimuat, pastikan data tetap tampil di DataGridView
        private void previewForm_Load(object sender, EventArgs e)
        {
            dgvPreview.DataSource = previewData; // Set ulang data source jika diperlukan
        }

        // Event klik tombol Import, validasi dan simpan data ke database
        private void btnImport_Click(object sender, EventArgs e)
        {
            // Membuka koneksi ke database
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open(); // Buka koneksi
                SqlTransaction transaction = conn.BeginTransaction(); // Mulai transaksi SQL

                try
                {
                    // Loop setiap baris data hasil import
                    foreach (DataRow row in previewData.Rows)
                    {
                        // Ambil data dari kolom yang diperlukan
                        string empId = row["employee_id"].ToString().Trim();
                        string empName = row["employee_name"].ToString().Trim();
                        string svcName = row["service_name"].ToString().Trim();
                        string svcPriceStr = row["service_price"].ToString().Trim();

                        // Validasi harga layanan harus numerik dan >= 0
                        if (!decimal.TryParse(svcPriceStr, out decimal svcPrice) || svcPrice < 0)
                        {
                            // Jika tidak valid, lempar exception
                            throw new Exception($"Baris dengan harga tidak valid (harus >= 0): {svcPriceStr}");
                        }

                        // Validasi employee harus ada di database
                        string empQuery = @"SELECT COUNT(*) FROM employees WHERE employee_id = @id AND first_name + ' ' + last_name = @name";
                        using (SqlCommand cmd = new SqlCommand(empQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", empId); // Parameter ID pegawai
                            cmd.Parameters.AddWithValue("@name", empName); // Parameter nama pegawai
                            if ((int)cmd.ExecuteScalar() == 0)
                            {
                                // Jika pegawai tidak ditemukan, lempar exception
                                throw new Exception($"Pegawai tidak ditemukan: {empId} - {empName}");
                            }
                        }

                        // Validasi service harus ada di database (hanya berdasarkan nama)
                        string svcQuery = @"SELECT COUNT(*) FROM services WHERE service_name = @name";
                        using (SqlCommand cmd = new SqlCommand(svcQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@name", svcName); // Parameter nama layanan
                            if ((int)cmd.ExecuteScalar() == 0)
                            {
                                // Jika layanan tidak ditemukan, lempar exception
                                throw new Exception($"Layanan tidak valid: {svcName}");
                            }
                        }

                        // Panggil stored procedure untuk insert (otomatis tambah client jika belum ada)
                        using (SqlCommand cmd = new SqlCommand("sp_add_manual_transaction", conn, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure; // Tipe command SP
                            cmd.Parameters.AddWithValue("@client_name", row["client_name"].ToString().Trim()); // Nama client
                            cmd.Parameters.AddWithValue("@phone_number", row["phone_number"].ToString().Trim()); // Nomor telepon client
                            cmd.Parameters.AddWithValue("@employee_id", empId); // ID pegawai
                            cmd.Parameters.AddWithValue("@employee_name", empName); // Nama pegawai
                            cmd.Parameters.AddWithValue("@service_name", svcName); // Nama layanan
                            cmd.Parameters.AddWithValue("@service_price", svcPrice); // Harga layanan
                            cmd.Parameters.AddWithValue("@appointment_date", DateTime.Parse(row["appointment_date"].ToString())); // Tanggal appointment
                            cmd.Parameters.AddWithValue("@start_time", TimeSpan.Parse(row["start_time"].ToString())); // Jam mulai
                            cmd.Parameters.AddWithValue("@end_time", TimeSpan.Parse(row["end_time"].ToString())); // Jam selesai
                            cmd.Parameters.AddWithValue("@status", row["status"].ToString()); // Status appointment

                            // Kolom cancellation_reason opsional, cek apakah ada di data
                            if (row.Table.Columns.Contains("cancellation_reason"))
                                cmd.Parameters.AddWithValue("@cancellation_reason", row["cancellation_reason"]?.ToString() ?? (object)DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@cancellation_reason", DBNull.Value);

                            cmd.ExecuteNonQuery(); // Eksekusi SP untuk insert data
                        }
                    }

					transaction.Commit(); // Commit transaksi jika semua berhasil
					MessageBox.Show("Data berhasil diimport ke database.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
					this.DialogResult = DialogResult.OK; // Penting! Agar parent form tahu import sukses
					this.Close(); // Tutup form setelah selesai import

				}
				catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback transaksi jika ada error
                    MessageBox.Show("Import data gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Cancel, menutup form preview tanpa import
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close(); // Tutup form tanpa melakukan import
        }
    }
}
