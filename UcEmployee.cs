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
    public partial class UcEmployee : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=omkegas  ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Variabel cache untuk data karyawan
        private DataTable _employeeCache = null;
        private DateTime _employeeCacheTime;
        private readonly TimeSpan _employeeCacheDuration = TimeSpan.FromMinutes(5); // Durasi cache 5 menit

        // Konstruktor UserControl
        public UcEmployee()
        {
            InitializeComponent(); // Inisialisasi komponen UI
        }

        // Event saat UserControl dimuat
        private void UcKaryawan_Load(object sender, EventArgs e)
        {
            LoadEmployees(); // Memuat data karyawan ke DataGridView
        }

        // Memuat data karyawan dari database ke DataGridView
        private void LoadEmployees(bool forceRefresh = false)
        {
            // Jika cache masih valid dan tidak force refresh, gunakan cache
            if (!forceRefresh && _employeeCache != null && (DateTime.Now - _employeeCacheTime) < _employeeCacheDuration)
            {
                dgvEmployees.DataSource = _employeeCache; // Tampilkan data dari cache
                return;
            }

            // Query data karyawan dari database
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter da = new SqlDataAdapter("sp_employee_get_all", conn); // Pakai stored procedure
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                da.Fill(dt); // Isi DataTable dengan hasil query
                dgvEmployees.DataSource = dt; // Tampilkan ke DataGridView

                // Simpan ke cache
                _employeeCache = dt;
                _employeeCacheTime = DateTime.Now;
            }
        }

        // Validasi input form agar tidak ada field yang kosong
        private bool IsInputValid()
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("ID Karyawan tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (txtID.Text.Contains(" "))
            {
                MessageBox.Show("ID Karyawan tidak boleh mengandung spasi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

			if (!txtPhone.Text.All(char.IsDigit))
			{
				MessageBox.Show("Nomor telepon harus berupa angka!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtPhone.Focus();
				return false;
			}

			if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
			{
                MessageBox.Show("Nama, Nomor Telepon, dan Email tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }


        // Mengosongkan semua field input
        private void ClearFields()
        {
            txtID.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtID.ReadOnly = false; // Supaya bisa input ID baru
        }


        // Event klik tombol Add, menambah karyawan baru ke database
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validasi input
            if (!IsInputValid())
            {
                MessageBox.Show("Semua data harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Proses insert data karyawan baru
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open(); // Buka koneksi
                SqlTransaction transaction = conn.BeginTransaction(); // Mulai transaksi SQL
                SqlCommand cmd = new SqlCommand("sp_employee_add", conn, transaction); // Pakai stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // Tambahkan parameter ke stored procedure
                cmd.Parameters.AddWithValue("@employee_id", txtID.Text);
                cmd.Parameters.AddWithValue("@first_name", txtFirstName.Text);
                cmd.Parameters.AddWithValue("@last_name", txtLastName.Text);
                cmd.Parameters.AddWithValue("@phone_number", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);

                try
                {
                    cmd.ExecuteNonQuery(); // Eksekusi perintah insert
                    transaction.Commit(); // Commit transaksi

                    MessageBox.Show("Karyawan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadEmployees(forceRefresh: true); // Refresh data
                    ClearFields(); // Kosongkan input
                }
                catch (SqlException ex)
                {
                    transaction.Rollback(); // Rollback jika gagal
                    // Cek error duplikat data
                    if (ex.Message.Contains("UNIQUE KEY") || ex.Number == 2627 || ex.Number == 2601)
                    {
                        MessageBox.Show("Gagal menambahkan! Data duplikat ditemukan (ID, email, atau nomor telepon mungkin sudah ada).",
                            "Duplikat Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Terjadi kesalahan saat menyimpan data:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Event klik pada cell DataGridView (tidak digunakan)
        private void dgvEmployees_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Bisa kosong jika tidak digunakan
        }

        // Event klik pada label (tidak digunakan)
        private void label5_Click(object sender, EventArgs e)
        {
            // Bisa dihapus atau kosongkan jika tidak perlu
        }

        // Event klik tombol Update, memperbarui data karyawan yang dipilih
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validasi ID harus terisi
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih data yang ingin diedit!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validasi input
            if (!IsInputValid())
            {
                MessageBox.Show("Semua data harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Konfirmasi update data
            DialogResult confirmResult = MessageBox.Show("Apakah Anda yakin ingin memperbarui data karyawan ini?",
                                                         "Konfirmasi Update",
                                                         MessageBoxButtons.YesNo,
                                                         MessageBoxIcon.Question);
            if (confirmResult == DialogResult.No)
            {
                return;
            }

            // Proses update data karyawan
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand("sp_employee_update", conn, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                // Tambahkan parameter ke stored procedure
                cmd.Parameters.AddWithValue("@employee_id", txtID.Text);
                cmd.Parameters.AddWithValue("@first_name", txtFirstName.Text);
                cmd.Parameters.AddWithValue("@last_name", txtLastName.Text);
                cmd.Parameters.AddWithValue("@phone_number", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);

                try
                {
                    cmd.ExecuteNonQuery(); // Eksekusi update
                    transaction.Commit(); // Commit transaksi

                    MessageBox.Show("Data karyawan berhasil diperbarui!", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadEmployees(forceRefresh: true); // Refresh data
                    ClearFields(); // Kosongkan input
                }
                catch (SqlException ex)
                {
                    transaction.Rollback(); // Rollback jika gagal
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
            }
        }

        // Event klik tombol Delete, menghapus karyawan yang dipilih
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Validasi ID harus terisi
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih data yang ingin dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Konfirmasi hapus data
            var confirm = MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();
                    SqlCommand cmd = new SqlCommand("sp_employee_delete", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@employee_id", txtID.Text);

                    try
                    {
                        cmd.ExecuteNonQuery(); // Eksekusi delete
                        transaction.Commit(); // Commit transaksi
                        MessageBox.Show("Data berhasil dihapus.", "Dihapus", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadEmployees(forceRefresh: true); // Refresh data
                        ClearFields(); // Kosongkan input
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Rollback jika gagal
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
                LoadEmployees(forceRefresh: true); // Bisa lempar error
                ClearFields();

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
        private void dgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];
                txtID.Text = row.Cells["employee_id"].Value.ToString();
                txtFirstName.Text = row.Cells["first_name"].Value.ToString();
                txtLastName.Text = row.Cells["last_name"].Value.ToString();
                txtPhone.Text = row.Cells["phone_number"].Value.ToString();
                txtEmail.Text = row.Cells["email"].Value.ToString();
                txtID.ReadOnly = true; // Pastikan ID terkunci setelah dipilih
            }
        }


        // Event-event berikut belum diimplementasikan (bisa dikosongkan)
        private void label6_Click(object sender, EventArgs e) { }
        private void txtID_TextChanged(object sender, EventArgs e) { }
        private void txtFirstName_TextChanged(object sender, EventArgs e) { }
        private void txtLastName_TextChanged(object sender, EventArgs e) { }
        private void txtPhone_TextChanged(object sender, EventArgs e) { }
        private void txtEmail_TextChanged(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim().ToLower();

            if (_employeeCache == null)
                LoadEmployees(); // Jaga-jaga kalau belum ada data

            DataTable filtered = _employeeCache.Clone(); // Buat struktur tabel kosong

            foreach (DataRow row in _employeeCache.Rows)
            {
                string fullName = $"{row["first_name"]} {row["last_name"]}".ToLower();
                string phone = row["phone_number"]?.ToString()?.ToLower() ?? "";
                string email = row["email"]?.ToString()?.ToLower() ?? "";

                if (fullName.Contains(searchText) || phone.Contains(searchText) || email.Contains(searchText))
                {
                    filtered.ImportRow(row);
                }
            }

            dgvEmployees.DataSource = filtered;
        }

    }
}
