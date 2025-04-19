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
    public partial class UcJadwalKaryawan : UserControl
    {
        private string connString = @"Data Source=LEGIONSLIM5\SQLEXPRESS;Initial Catalog=Barbershop;Integrated Security=True";

        public UcJadwalKaryawan()
        {
            InitializeComponent();
        }

        private void UcJadwalKaryawan_Load(object sender, EventArgs e)
        {
            LoadKaryawan();
            IsiSemuaComboJam(); // Isi jam kerja ke semua ComboBox saat load
        }

        private void LoadKaryawan()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                SELECT 
                    employee_id, 
                    employee_id + ' - ' + first_name + ' ' + last_name AS full_name 
                FROM employees";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbNamaKaryawan.DataSource = dt;
                cmbNamaKaryawan.DisplayMember = "full_name";
                cmbNamaKaryawan.ValueMember = "employee_id";
                cmbNamaKaryawan.SelectedIndex = -1;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbNamaKaryawan.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih karyawan terlebih dahulu.");
                return;
            }

            string empID = cmbNamaKaryawan.SelectedValue.ToString();
            string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            foreach (string day in days)
            {
                CheckBox chk = FindControlRecursive(this, "chk" + day) as CheckBox;
                ComboBox cmbFrom = FindControlRecursive(this, "cmbFrom" + day) as ComboBox;
                ComboBox cmbTo = FindControlRecursive(this, "cmbTo" + day) as ComboBox;

                int dayID = Array.IndexOf(days, day);
                string scheduleID = empID + dayID;

                if (chk != null && chk.Checked)
                {
                    if (cmbFrom.SelectedItem == null || cmbTo.SelectedItem == null)
                    {
                        MessageBox.Show($"Lengkapi jam kerja untuk hari {day}.");
                        return;
                    }

                    TimeSpan from = TimeSpan.Parse(cmbFrom.SelectedItem.ToString());
                    TimeSpan to = TimeSpan.Parse(cmbTo.SelectedItem.ToString());

                    if (from >= to)
                    {
                        MessageBox.Show($"Jam mulai harus lebih awal dari jam selesai pada hari {day}.");
                        return;
                    }

                    SimpanJadwal(scheduleID, empID, dayID, from, to);
                }
                else
                {
                    HapusJadwal(empID, dayID); // hapus jika uncheck
                }
            }

            MessageBox.Show("Jadwal berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Control FindControlRecursive(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name)
                    return c;

                Control found = FindControlRecursive(c, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void SimpanJadwal(string id, string empID, int dayID, TimeSpan from, TimeSpan to)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    string deleteQuery = "DELETE FROM employees_schedule WHERE employee_id=@emp AND day_id=@day";
                    string insertQuery = "INSERT INTO employees_schedule (id, employee_id, day_id, from_hour, to_hour) " +
                                         "VALUES (@id, @emp, @day, @from, @to)";

                    SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                    deleteCmd.Parameters.AddWithValue("@emp", empID);
                    deleteCmd.Parameters.AddWithValue("@day", dayID);

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@id", id);
                    insertCmd.Parameters.AddWithValue("@emp", empID);
                    insertCmd.Parameters.AddWithValue("@day", dayID);
                    insertCmd.Parameters.AddWithValue("@from", from);
                    insertCmd.Parameters.AddWithValue("@to", to);

                    conn.Open();
                    deleteCmd.ExecuteNonQuery();
                    insertCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan jadwal: " + ex.Message);
                }
            }
        }

        private void HapusJadwal(string empID, int dayID)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string deleteQuery = "DELETE FROM employees_schedule WHERE employee_id=@emp AND day_id=@day";
                SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                cmd.Parameters.AddWithValue("@emp", empID);
                cmd.Parameters.AddWithValue("@day", dayID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void IsiComboJam(ComboBox combo)
        {
            combo.Items.Clear();
            for (int jam = 5; jam <= 23; jam++)
            {
                combo.Items.Add($"{jam:D2}:00");
                combo.Items.Add($"{jam:D2}:15");
                combo.Items.Add($"{jam:D2}:30");
                combo.Items.Add($"{jam:D2}:45");
            }
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void IsiSemuaComboJam()
        {
            IsiComboJam(cmbFromSunday); IsiComboJam(cmbToSunday);
            IsiComboJam(cmbFromMonday); IsiComboJam(cmbToMonday);
            IsiComboJam(cmbFromTuesday); IsiComboJam(cmbToTuesday);
            IsiComboJam(cmbFromWednesday); IsiComboJam(cmbToWednesday);
            IsiComboJam(cmbFromThursday); IsiComboJam(cmbToThursday);
            IsiComboJam(cmbFromFriday); IsiComboJam(cmbToFriday);
            IsiComboJam(cmbFromSaturday); IsiComboJam(cmbToSaturday);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (cmbNamaKaryawan.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih karyawan terlebih dahulu sebelum reset.");
                return;
            }

            string empID = cmbNamaKaryawan.SelectedValue.ToString();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string deleteAll = "DELETE FROM employees_schedule WHERE employee_id=@emp";
                SqlCommand cmd = new SqlCommand(deleteAll, conn);
                cmd.Parameters.AddWithValue("@emp", empID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Semua jadwal karyawan berhasil dihapus!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            IsiSemuaComboJam();
            MessageBox.Show("Jam kerja berhasil diperbarui!", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnTampilkanData_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
            SELECT s.id, s.employee_id, e.first_name + ' ' + e.last_name AS nama, 
                   s.day_id, s.from_hour, s.to_hour
            FROM employees_schedule s
            JOIN employees e ON s.employee_id = e.employee_id
            ORDER BY s.employee_id, s.day_id";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvJadwal.DataSource = dt; // pastikan dgvJadwal sudah ada di form
            }
        }

        
    }
}
