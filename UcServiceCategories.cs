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
    public partial class UcServiceCategories : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Konstruktor UserControl
        public UcServiceCategories()
        {
            InitializeComponent();
        }

        // Event saat UserControl dimuat
        private void UcKategoriLayanan_Load(object sender, EventArgs e)
        {
            LoadData();           // Memuat data kategori layanan ke DataGridView
            txtID.ReadOnly = true; // ID kategori tidak bisa diubah manual
        }

        // Memuat data kategori layanan dari database ke DataGridView
        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM service_categories", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvKategoriLayanan.DataSource = dt;
            }
        }

        // Event klik tombol Add, menambah kategori layanan baru ke database
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNamaKategori.Text))
            {
                MessageBox.Show("Nama kategori tidak boleh kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "INSERT INTO service_categories (category_name) VALUES (@nama)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nama", txtNamaKategori.Text);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearFields();
                MessageBox.Show("Kategori berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Event klik tombol Update, memperbarui data kategori layanan yang dipilih
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text) || string.IsNullOrWhiteSpace(txtNamaKategori.Text))
            {
                MessageBox.Show("Pilih kategori dan isi nama kategori.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Konfirmasi sebelum update kategori
            DialogResult confirm = MessageBox.Show($"Apakah Anda yakin ingin memperbarui kategori menjadi \"{txtNamaKategori.Text}\"?",
                                                   "Konfirmasi Update Kategori",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Question);
            if (confirm == DialogResult.No)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "UPDATE service_categories SET category_name = @nama WHERE category_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@nama", txtNamaKategori.Text);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                LoadData();
                ClearFields();
                MessageBox.Show("Kategori berhasil diperbarui.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Event klik tombol Delete, menghapus kategori layanan yang dipilih
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih kategori yang ingin dihapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus kategori ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "DELETE FROM service_categories WHERE category_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtID.Text);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    LoadData();
                    ClearFields();
                    MessageBox.Show("Kategori berhasil dihapus.", "Dihapus", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Event klik tombol Refresh, reload data dan reset form
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearFields();
            MessageBox.Show("Data diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Event klik pada cell DataGridView, load data ke form input
        private void dgvKategoriLayanan_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvKategoriLayanan.Rows[e.RowIndex];
                txtID.Text = row.Cells["category_id"].Value.ToString();
                txtNamaKategori.Text = row.Cells["category_name"].Value.ToString();
            }
        }

        // Mengosongkan semua field input
        private void ClearFields()
        {
            txtID.Clear();
            txtNamaKategori.Clear();
        }

        // Event perubahan pada txtID (tidak digunakan)
        private void txtID_TextChanged(object sender, EventArgs e) { }

        // Event perubahan pada txtNamaKategori (tidak digunakan)
        private void txtNamaKategori_TextChanged(object sender, EventArgs e) { }

        // Event klik pada label (tidak digunakan)
        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}

