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
    public partial class UcClients : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=omkegas  ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Caching untuk data client
        private static DataTable cachedClients = null;

        public UcClients()
        {
            InitializeComponent();
            txtID.ReadOnly = true;
            GenerateClientID();

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



        // Event saat UserControl dimuat
        private void UcPelanggan_Load(object sender, EventArgs e)
        {
            try
            {
                LoadClient();
                txtID.Text = GenerateClientID(); // Set ID saat load
                txtID.ReadOnly = true; // Jadikan read-only
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data client: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Memuat data client dari database ke DataGridView (pakai cache)
        private void LoadClient()
        {
            if (cachedClients == null)
            {
                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("sp_get_clients", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cachedClients = dt;
                }
            }
            dgvClient.DataSource = cachedClients.Copy();
        }

        // Invalidate cache setiap ada perubahan data
        private void InvalidateClientCache()
        {
            cachedClients = null;
        }

        // Validasi input form agar tidak ada field yang kosong (kecuali email)
        // Validasi input form agar setiap field (kecuali email) harus diisi dan tampilkan pesan spesifik
        private bool IsInputValid()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Nama depan harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Nama belakang harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Nomor telepon harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }
			if (!txtPhone.Text.All(char.IsDigit))
			{
				MessageBox.Show("Nomor telepon harus berupa angka!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtPhone.Focus();
				return false;
			}

			return true; // Semua OK
        }


        // Mengosongkan semua field input
        private void ClearFields()
        {
            
            txtFirstName.Clear();
            txtLastName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
        }

        // Event klik tombol Add, menambah client baru ke database (pakai stored procedure + transaksi + error handling)
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!IsInputValid())
            {
                return; // Pesan sudah di-handle
            }


            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_insert_client", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@client_id", txtID.Text);
                        cmd.Parameters.AddWithValue("@first_name", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@last_name", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@phone_number", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@client_email", string.IsNullOrEmpty(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    MessageBox.Show("Pelanggan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    InvalidateClientCache(); // Invalidate cache setelah insert
                    LoadClient();
                    ClearFields();
                    txtID.Text = GenerateClientID(); // ID baru setelah insert

                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        MessageBox.Show("Gagal menambahkan! ID, Nomor telepon atau email sudah digunakan.",
                                        "Data Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik pada cell DataGridView (tidak digunakan)
        private void dgvClient_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Bisa kosong jika tidak digunakan
        }

        // Event klik pada label (tidak digunakan)
        private void label5_Click(object sender, EventArgs e)
        {
            // Bisa dihapus atau kosongkan jika tidak perlu
        }

        // Event klik tombol Update, memperbarui data client yang dipilih (pakai stored procedure + transaksi + error handling)
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih data yang ingin diedit!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsInputValid())
            {
                return; // Pesan sudah muncul di dalam IsInputValid
            }


            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin memperbarui data pelanggan ini?", "Konfirmasi Update",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_update_client", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@client_id", txtID.Text);
                        cmd.Parameters.AddWithValue("@first_name", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@last_name", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@phone_number", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@client_email", string.IsNullOrEmpty(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    MessageBox.Show("Data Pelanggan berhasil diperbarui!", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    InvalidateClientCache(); // Invalidate cache setelah update
                    LoadClient();
                    ClearFields();
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        MessageBox.Show("Gagal update! ID, Nomor telepon atau email sudah digunakan.",
                                        "Data Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Delete, menghapus client yang dipilih (pakai stored procedure + transaksi + error handling)
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih data yang ingin dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_delete_client", conn, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@client_id", txtID.Text);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        MessageBox.Show("Data berhasil dihapus.", "Dihapus", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        InvalidateClientCache(); // Invalidate cache setelah delete
                        LoadClient();
                        ClearFields();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Event klik tombol Refresh, reload data dan reset form
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                InvalidateClientCache();

                // Coba load data client
                LoadClient(); // Kalau gagal, langsung lompat ke catch

                ClearFields();
                txtID.Text = GenerateClientID(); // ID baru tiap refresh

                // Hanya ditampilkan jika semua langkah di atas berhasil
                MessageBox.Show("Data diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Gagal memuat data dari server. Periksa koneksi Anda.\n\n" + ex.Message,
                    "Koneksi Terputus", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal refresh data:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Event klik pada baris DataGridView, load data ke form input
        private void dgvClient_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvClient.Rows[e.RowIndex];
                    txtID.Text = row.Cells["client_id"].Value.ToString();
                    txtFirstName.Text = row.Cells["first_name"].Value.ToString();
                    txtLastName.Text = row.Cells["last_name"].Value.ToString();
                    txtPhone.Text = row.Cells["phone_number"].Value.ToString();
                    txtEmail.Text = row.Cells["client_email"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data ke form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim().ToLower();

            if (cachedClients == null)
                LoadClient(); // Jaga-jaga

            DataTable dtFiltered = cachedClients.Clone(); // Struktur sama

            foreach (DataRow row in cachedClients.Rows)
            {
                string namaLengkap = $"{row["first_name"]} {row["last_name"]}".ToLower();
                string noTelp = row["phone_number"]?.ToString()?.ToLower() ?? "";

                if (namaLengkap.Contains(searchText) || noTelp.Contains(searchText))
                {
                    dtFiltered.ImportRow(row);
                }
            }

            dgvClient.DataSource = dtFiltered;
        }

    }
}