using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Barbershop
{
    public partial class UcServices : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2  ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Caching untuk data layanan
        private static DataTable cachedServices = null;

        public UcServices()
        {
            InitializeComponent();
        }

        // Memuat data layanan dari database ke DataGridView (pakai cache + error handling)
        private void LoadData()
        {
            try
            {
                if (cachedServices == null)
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    using (SqlCommand cmd = new SqlCommand("sp_get_services", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        cachedServices = dt;
                    }
                }
                dgvLayanan.DataSource = cachedServices.Copy();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data layanan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Invalidate cache setiap ada perubahan data
        private void InvalidateServiceCache()
        {
            cachedServices = null;
        }

        // Memuat data kategori layanan ke ComboBox (dengan error handling)
        private void LoadCategories()
        {
            try
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
                    cmbCategoryName.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data kategori: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event klik tombol Add, menambah layanan baru ke database (pakai stored procedure + transaksi + error handling)
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbCategoryName.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih kategori!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_insert_service", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@service_id", txtID.Text);
                        cmd.Parameters.AddWithValue("@service_name", txtServiceName.Text);
                        cmd.Parameters.AddWithValue("@service_description", txtServiceDescription.Text);
                        cmd.Parameters.AddWithValue("@service_price", Convert.ToDecimal(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@service_duration", Convert.ToInt32(txtDuration.Text));
                        cmd.Parameters.AddWithValue("@category_id", cmbCategoryName.SelectedValue);

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    MessageBox.Show("Data berhasil ditambahkan!");
                    InvalidateServiceCache(); // Invalidate cache setelah insert
                    LoadData();
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal menambahkan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Update, memperbarui data layanan yang dipilih (pakai stored procedure + transaksi + error handling)
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbCategoryName.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih kategori!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_update_service", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@service_id", txtID.Text);
                        cmd.Parameters.AddWithValue("@service_name", txtServiceName.Text);
                        cmd.Parameters.AddWithValue("@service_description", txtServiceDescription.Text);
                        cmd.Parameters.AddWithValue("@service_price", Convert.ToDecimal(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@service_duration", Convert.ToInt32(txtDuration.Text));
                        cmd.Parameters.AddWithValue("@category_id", cmbCategoryName.SelectedValue);

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    MessageBox.Show("Data berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    InvalidateServiceCache(); // Invalidate cache setelah update
                    LoadData();
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal memperbarui data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Delete, menghapus layanan yang dipilih (pakai stored procedure + transaksi + error handling)
        private void btnDelete_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_delete_service", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@service_id", txtID.Text);

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    MessageBox.Show("Data berhasil dihapus!");
                    InvalidateServiceCache(); // Invalidate cache setelah delete
                    LoadData();
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event klik tombol Refresh, reload data layanan (dengan error handling)
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                InvalidateServiceCache(); // Invalidate cache saat refresh
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal refresh data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event klik pada baris DataGridView, load data ke form input (dengan error handling)
        private void dgvLayanan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data ke form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event saat ComboBox kategori drop down, reload data kategori (dengan error handling)
        private void cmbCategoryName_DropDown(object sender, EventArgs e)
        {
            try
            {
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat kategori: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event saat UserControl dimuat, load data layanan dan kategori (dengan error handling)
        private void UcLayanan_Load(object sender, EventArgs e)
        {
            try
            {
                LoadData();
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data awal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}