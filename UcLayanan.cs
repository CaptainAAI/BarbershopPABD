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
    public partial class UcLayanan : UserControl
    {
        private string connString = "Data Source=LEGIONSLIM5\\SQLEXPRESS;Initial Catalog=Barbershop;Integrated Security=True";

        public UcLayanan()
        {
            InitializeComponent();
            txtID.ReadOnly = true; // Disable manual input
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT s.service_id, s.service_name, s.service_description, s.service_price, s.service_duration, c.category_name " +
                               "FROM services s JOIN service_categories c ON s.category_id = c.category_id";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvLayanan.DataSource = dt;
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("SELECT category_id, category_name FROM service_categories", conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(reader);

                cmbCategoryName.DataSource = dt;
                cmbCategoryName.DisplayMember = "category_name";
                cmbCategoryName.ValueMember = "category_id";

                conn.Close();
            }
        }

        private void cmbCategoryName_Click(object sender, EventArgs e)
        {
            LoadCategories(); // Refresh kategori setiap klik
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string id = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(); // Generate ID otomatis

                string query = "INSERT INTO services (service_id, service_name, service_description, service_price, service_duration, category_id) " +
                               "VALUES (@id, @name, @desc, @price, @duration, @catid)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", txtServiceName.Text);
                cmd.Parameters.AddWithValue("@desc", txtServiceDescription.Text);
                cmd.Parameters.AddWithValue("@price", Convert.ToDecimal(txtPrice.Text));
                cmd.Parameters.AddWithValue("@duration", Convert.ToInt32(txtDuration.Text));
                cmd.Parameters.AddWithValue("@catid", cmbCategoryName.SelectedValue);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                LoadData();
                ClearFields();
                MessageBox.Show("Layanan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "UPDATE services SET service_name = @name, service_description = @desc, " +
                               "service_price = @price, service_duration = @duration, category_id = @catid " +
                               "WHERE service_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@name", txtServiceName.Text);
                cmd.Parameters.AddWithValue("@desc", txtServiceDescription.Text);
                cmd.Parameters.AddWithValue("@price", Convert.ToDecimal(txtPrice.Text));
                cmd.Parameters.AddWithValue("@duration", Convert.ToInt32(txtDuration.Text));
                cmd.Parameters.AddWithValue("@catid", cmbCategoryName.SelectedValue);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                LoadData();
                ClearFields();
                MessageBox.Show("Layanan berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtID.Text == "")
            {
                MessageBox.Show("Pilih layanan yang ingin dihapus terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus layanan ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "DELETE FROM services WHERE service_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtID.Text);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    LoadData();
                    ClearFields();
                    MessageBox.Show("Layanan berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearFields();
        }

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
                cmbCategoryName.Text = row.Cells["category_name"].Value.ToString();
            }
        }

        private void ClearFields()
        {
            txtID.Clear();
            txtServiceName.Clear();
            txtServiceDescription.Clear();
            txtPrice.Clear();
            txtDuration.Clear();
            cmbCategoryName.SelectedIndex = -1;
        }
    }
}
