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
        }

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
                cmbCategoryName.SelectedIndex = -1;
            }
        }

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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbCategoryName.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih kategori!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Data berhasil diperbarui!");
                    LoadData();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Gagal memperbarui data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
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
                cmbCategoryName.SelectedValue = row.Cells["category_id"].Value;
            }
        }

        private void cmbCategoryName_DropDown(object sender, EventArgs e)
        {
            LoadCategories();
        }

        private void UcLayanan_Load(object sender, EventArgs e)
        {
            LoadData();
            LoadCategories();
        }
    }
}