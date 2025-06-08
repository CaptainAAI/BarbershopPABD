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
    public partial class UcServices : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Konstruktor UserControl
        public UcServices()
        {
            InitializeComponent();
        }

        // Memuat data layanan dari database ke DataGridView
        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM services", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvLayanan.DataSource = dt;
            }
        }

        // Memuat data kategori layanan ke ComboBox
        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT category_id, category_name FROM service_categories";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmbCategoryName.DisplayMember = "category_name";
                cmbCategoryName.ValueMember = "category_id";
                cmbCategoryName.DataSource = dt;
                cmbCategoryName.SelectedIndex = -1; // Tidak ada yang dipilih secara default
            }
        }

        // Event klik tombol Add, menambah layanan baru ke database
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbCategoryName.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih kategori!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "INSERT INTO services (service_id, service_name, service_description, service_price, service_duration, category_id) VALUES (@id, @name, @desc, @price, @duration, @category)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@name", txtServiceName.Text);
                cmd.Parameters.AddWithValue("@desc", txtServiceDescription.Text);
                cmd.Parameters.AddWithValue("@price", txtPrice.Text);
                cmd.Parameters.AddWithValue("@duration", txtDuration.Text);
                cmd.Parameters.AddWithValue("@category", cmbCategoryName.SelectedValue);

                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data berhasil ditambahkan!");
                    LoadData();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Gagal menambahkan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Update, memperbarui data layanan yang dipilih
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbCategoryName.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih kategori!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Konfirmasi sebelum update layanan
            DialogResult confirm = MessageBox.Show($"Apakah Anda yakin ingin memperbarui layanan \"{txtServiceName.Text}\"?",
                                                   "Konfirmasi Update Layanan",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Question);
            if (confirm == DialogResult.No)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "UPDATE services SET service_name = @name, service_description = @desc, service_price = @price, service_duration = @duration, category_id = @category WHERE service_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@name", txtServiceName.Text);
                cmd.Parameters.AddWithValue("@desc", txtServiceDescription.Text);
                cmd.Parameters.AddWithValue("@price", txtPrice.Text);
                cmd.Parameters.AddWithValue("@duration", txtDuration.Text);
                cmd.Parameters.AddWithValue("@category", cmbCategoryName.SelectedValue);

                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Gagal memperbarui data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Delete, menghapus layanan yang dipilih
        private void btnDelete_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "DELETE FROM services WHERE service_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);

                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data berhasil dihapus!");
                    LoadData();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Refresh, reload data layanan
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // Event klik pada baris DataGridView, load data ke form input
        private void dgvLayanan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvLayanan.Rows[e.RowIndex];
                txtID.Text = row.Cells["service_id"].Value.ToString();
                txtServiceName.Text = row.Cells["service_name"].Value.ToString();
                txtServiceDescription.Text = row.Cells["service_description"].Value.ToString();
                txtPrice.Text = row.Cells["service_price"].Value.ToString();
                txtDuration.Text = row.Cells["service_duration"].Value.ToString();
                cmbCategoryName.SelectedValue = row.Cells["category_id"].Value;
            }
        }

        // Event saat ComboBox kategori drop down, reload data kategori
        private void cmbCategoryName_DropDown(object sender, EventArgs e)
        {
            LoadCategories();
        }

        // Event saat UserControl dimuat, load data layanan dan kategori
        private void UcLayanan_Load(object sender, EventArgs e)
        {
            LoadData();
            LoadCategories();
        }
    }
}
