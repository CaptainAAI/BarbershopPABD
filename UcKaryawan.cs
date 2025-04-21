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
    public partial class UcKaryawan : UserControl
    {
        private string connString = "Data Source=LEGIONSLIM5\\SQLEXPRESS;Initial Catalog=Barbershop;Integrated Security=True";

        public UcKaryawan()
        {
            InitializeComponent();
        }

        private void UcKaryawan_Load(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT * FROM employees";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvEmployees.DataSource = dt;
            }
        }

        private bool IsInputValid()
        {
            return !string.IsNullOrWhiteSpace(txtID.Text)
                && !string.IsNullOrWhiteSpace(txtFirstName.Text)
                && !string.IsNullOrWhiteSpace(txtLastName.Text)
                && !string.IsNullOrWhiteSpace(txtPhone.Text)
                && !string.IsNullOrWhiteSpace(txtEmail.Text);
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
                string query = "INSERT INTO employees (employee_id, first_name, last_name, phone_number, email) " +
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

            MessageBox.Show("Karyawan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadEmployees();
            ClearFields();
        }

        private void dgvEmployees_CellContentClick(object sender, DataGridViewCellEventArgs e)
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
                string query = "UPDATE employees SET first_name=@fname, last_name=@lname, phone_number=@phone, email=@email " +
                               "WHERE employee_id=@id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", txtID.Text);
                cmd.Parameters.AddWithValue("@fname", txtFirstName.Text);
                cmd.Parameters.AddWithValue("@lname", txtLastName.Text);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Data karyawan berhasil diperbarui!", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadEmployees();
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
                    string query = "DELETE FROM employees WHERE employee_id=@id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtID.Text);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data berhasil dihapus.", "Dihapus", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadEmployees();
                ClearFields();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadEmployees();
            ClearFields();
            MessageBox.Show("Data diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

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
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void txtID_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
