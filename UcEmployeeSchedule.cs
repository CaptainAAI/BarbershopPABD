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
    public partial class UcEmployeeSchedule : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=ytta;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        private DataTable _scheduleCache = null;
        private DateTime _scheduleCacheTime;
        private readonly TimeSpan _scheduleCacheDuration = TimeSpan.FromMinutes(5); // cache 5 menit

        // Konstruktor UserControl
        public UcEmployeeSchedule()
        {
            InitializeComponent();
            // Event handler untuk perubahan pilihan karyawan
            cmbNamaKaryawan.SelectedIndexChanged += CmbNamaKaryawan_SelectedIndexChanged;
        }

        // Event saat UserControl dimuat
        private void UcJadwalKaryawan_Load(object sender, EventArgs e)
        {
            LoadKaryawan();         // Memuat data karyawan ke ComboBox
            IsiSemuaComboJam();     // Mengisi semua ComboBox jam kerja
        }

        // Memuat daftar karyawan ke ComboBox
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
                cmbNamaKaryawan.SelectedIndex = -1; // Tidak ada yang dipilih secara default
            }
        }

        // Event klik tombol Update, menyimpan jadwal karyawan ke database
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbNamaKaryawan.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih karyawan terlebih dahulu.");
                return;
            }

            // Konfirmasi sebelum update jadwal
            DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin memperbarui jadwal karyawan ini?",
                                                   "Konfirmasi Update Jadwal",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Question);
            if (confirm == DialogResult.No)
            {
                return;
            }

            string empID = cmbNamaKaryawan.SelectedValue.ToString();
            string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
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
                                transaction.Rollback();
                                return;
                            }

                            TimeSpan from = TimeSpan.Parse(cmbFrom.SelectedItem.ToString());
                            TimeSpan to = TimeSpan.Parse(cmbTo.SelectedItem.ToString());

                            if (from >= to)
                            {
                                MessageBox.Show($"Jam mulai harus lebih awal dari jam selesai pada hari {day}.");
                                transaction.Rollback();
                                return;
                            }

                            // Simpan jadwal ke database dengan transaction
                            SimpanJadwal(scheduleID, empID, dayID, from, to, conn, transaction);
                        }
                        else
                        {
                            // Hapus jadwal jika tidak dicentang dengan transaction
                            HapusJadwal(empID, dayID, conn, transaction);
                        }
                    }
                    transaction.Commit();
                    MessageBox.Show("Jadwal berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllSchedules(forceRefresh: true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal memperbarui jadwal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Mencari kontrol secara rekursif berdasarkan nama
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

        // Menyimpan jadwal karyawan ke database (dengan transaction)
        private void SimpanJadwal(string id, string empID, int dayID, TimeSpan from, TimeSpan to, SqlConnection conn, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand("sp_employee_schedule_upsert", conn, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@employee_id", empID);
                cmd.Parameters.AddWithValue("@day_id", dayID);
                cmd.Parameters.AddWithValue("@from_hour", from);
                cmd.Parameters.AddWithValue("@to_hour", to);
                cmd.ExecuteNonQuery();
            }
        }

        // Menghapus jadwal karyawan untuk hari tertentu (dengan transaction)
        private void HapusJadwal(string empID, int dayID, SqlConnection conn, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand("sp_employee_schedule_delete", conn, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@employee_id", empID);
                cmd.Parameters.AddWithValue("@day_id", dayID);
                cmd.ExecuteNonQuery();
            }
        }

        // Mengisi ComboBox jam kerja (05:00 - 23:55, interval 5 menit)
        private void IsiComboJam(ComboBox combo)
        {
            combo.Items.Clear();
            for (int jam = 00; jam <= 23; jam++)
            {
                combo.Items.Add($"{jam:D2}:00");
                combo.Items.Add($"{jam:D2}:05");
                combo.Items.Add($"{jam:D2}:10");
                combo.Items.Add($"{jam:D2}:15");
                combo.Items.Add($"{jam:D2}:20");
                combo.Items.Add($"{jam:D2}:25");
                combo.Items.Add($"{jam:D2}:30");
                combo.Items.Add($"{jam:D2}:35");
                combo.Items.Add($"{jam:D2}:40");
                combo.Items.Add($"{jam:D2}:45");
                combo.Items.Add($"{jam:D2}:50");
                combo.Items.Add($"{jam:D2}:55");
            }
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // Mengisi semua ComboBox jam kerja untuk setiap hari
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

        // Event klik tombol Tampilkan Data, menampilkan seluruh jadwal karyawan di DataGridView
        private void LoadAllSchedules(bool forceRefresh = false)
        {
            if (!forceRefresh && _scheduleCache != null && (DateTime.Now - _scheduleCacheTime) < _scheduleCacheDuration)
            {
                dgvJadwal.DataSource = _scheduleCache;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter da = new SqlDataAdapter("sp_employee_schedule_get_all", conn);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvJadwal.DataSource = dt;

                // Simpan ke cache
                _scheduleCache = dt;
                _scheduleCacheTime = DateTime.Now;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllSchedules();
        }

        // Event saat ComboBox karyawan berubah, load jadwal karyawan yang dipilih
        private void CmbNamaKaryawan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNamaKaryawan.SelectedIndex != -1)
            {
                string empID = cmbNamaKaryawan.SelectedValue.ToString();
                LoadJadwalKaryawan(empID);
            }
        }

        // Memuat jadwal karyawan dari database dan menampilkan ke form
        private void LoadJadwalKaryawan(string empID)
        {
            string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("sp_employee_schedule_get_by_employee", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@employee_id", empID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                // Reset semua checkbox dan combobox
                foreach (string day in days)
                {
                    CheckBox chk = FindControlRecursive(this, "chk" + day) as CheckBox;
                    ComboBox cmbFrom = FindControlRecursive(this, "cmbFrom" + day) as ComboBox;
                    ComboBox cmbTo = FindControlRecursive(this, "cmbTo" + day) as ComboBox;

                    if (chk != null) chk.Checked = false;
                    if (cmbFrom != null) cmbFrom.SelectedIndex = -1;
                    if (cmbTo != null) cmbTo.SelectedIndex = -1;
                }

                // Set jadwal yang ada di database
                while (reader.Read())
                {
                    int dayID = reader.GetByte(0); // day_id bertipe TINYINT
                    string from = reader.GetTimeSpan(1).ToString(@"hh\:mm");
                    string to = reader.GetTimeSpan(2).ToString(@"hh\:mm");

                    string dayName = days[dayID];

                    CheckBox chk = FindControlRecursive(this, "chk" + dayName) as CheckBox;
                    ComboBox cmbFrom = FindControlRecursive(this, "cmbFrom" + dayName) as ComboBox;
                    ComboBox cmbTo = FindControlRecursive(this, "cmbTo" + dayName) as ComboBox;

                    if (chk != null) chk.Checked = true;
                    if (cmbFrom != null) cmbFrom.SelectedItem = from;
                    if (cmbTo != null) cmbTo.SelectedItem = to;
                }

                reader.Close();
            }
        }

        // Event perubahan pada richTextBox (tidak digunakan)
        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
