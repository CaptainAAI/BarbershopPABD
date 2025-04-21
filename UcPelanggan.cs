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
    public partial class UcPelanggan : UserControl
    {

        private string connString = "Data Source=LEGIONSLIM5\\SQLEXPRESS;Initial Catalog=Barbershop;Integrated Security=True";

        public UcPelanggan()
        {
            InitializeComponent();
        }



        private void UcPelanggan_Load(object sender, EventArgs e)
        {
            LoadClient();
        }

        private void LoadClient()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT * FROM clients";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvClient.DataSource = dt;
            }
        }

        private bool IsInputValid()
        {
            return !string.IsNullOrWhiteSpace(txtID.Text)
                && !string.IsNullOrWhiteSpace(txtFirstName.Text)
                && !string.IsNullOrWhiteSpace(txtLastName.Text)
                && !string.IsNullOrWhiteSpace(txtPhone.Text);
                
        }

        private void ClearFields()
        {
            txtID.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
        }



        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!IsInputValid())
            {
                MessageBox.Show("Semua data harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "INSERT INTO clients (client_id, first_name, last_name, phone_number, client_email) " +
                               "VALUES (@id, @fname, @lname, @phone, @email)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@fname", txtFirstName.Text);
                cmd.Parameters.AddWithValue("@lname", txtLastName.Text);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Pelanggan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadClient();
            ClearFields();
        }

        private void dgvClient_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Bisa kosong jika tidak digunakan
        }

        private void label5_Click(object sender, EventArgs e)
        {
            // Bisa dihapus atau kosongkan jika tidak perlu
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih data yang ingin diedit!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsInputValid())
            {
                MessageBox.Show("Semua data harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "UPDATE clients SET first_name=@fname, last_name=@lname, phone_number=@phone, client_email=@email " +
                               "WHERE client_id=@id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@fname", txtFirstName.Text);
                cmd.Parameters.AddWithValue("@lname", txtLastName.Text);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Data Pelanggan berhasil diperbarui!", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadClient();
            ClearFields();
        }

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
                    string query = "DELETE FROM clients WHERE client_id=@id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtID.Text);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data berhasil dihapus.", "Dihapus", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadClient();
                ClearFields();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadClient();
            ClearFields();
            MessageBox.Show("Data diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvClient_CellClick(object sender, DataGridViewCellEventArgs e)
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
    }
}
