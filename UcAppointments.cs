using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Barbershop
{
    public partial class UcAppointments : UserControl
    {
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=omkegas  ;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        private static DataTable cachedClients;
        private static DataTable cachedEmployees;
        private static DataTable cachedServices;

        public UcAppointments()
        {
            InitializeComponent();
            UpdateDateRange();
            txtAppointmentID.ReadOnly = true;
            cmbClientID.TextUpdate += cmbClientID_TextUpdate;
            cmbEmployeeID.TextUpdate += cmbEmployeeID_TextUpdate;
            cmbServiceID.TextUpdate += cmbServiceID_TextUpdate;
        }

        private void UcDashboard_Load(object sender, EventArgs e)
        {
            LoadAppointments();
            LoadComboBoxes();
            LoadStartTimeCombo();
        }

        private void LoadComboBoxes()
        {
            cmbClientID.DataSource = GetClients();
            cmbClientID.ValueMember = "client_id";
            cmbClientID.DisplayMember = "name";
            cmbClientID.AutoCompleteMode = AutoCompleteMode.None;
            cmbClientID.AutoCompleteSource = AutoCompleteSource.None;
            cmbClientID.DropDownStyle = ComboBoxStyle.DropDown;

            cmbEmployeeID.DataSource = GetEmployees();
            cmbEmployeeID.ValueMember = "employee_id";
            cmbEmployeeID.DisplayMember = "name";
            cmbEmployeeID.AutoCompleteMode = AutoCompleteMode.None;
            cmbEmployeeID.AutoCompleteSource = AutoCompleteSource.None;
            cmbEmployeeID.DropDownStyle = ComboBoxStyle.DropDown;

            cmbServiceID.DataSource = GetServices();
            cmbServiceID.ValueMember = "service_id";
            cmbServiceID.DisplayMember = "service_name";
            cmbServiceID.AutoCompleteMode = AutoCompleteMode.None;
            cmbServiceID.AutoCompleteSource = AutoCompleteSource.None;
            cmbServiceID.DropDownStyle = ComboBoxStyle.DropDown;

            cmbClientID.SelectedIndex = -1;
            cmbEmployeeID.SelectedIndex = -1;
            cmbServiceID.SelectedIndex = -1;

            cmbStatusBooking.Items.Clear();
            cmbStatusBooking.Items.AddRange(new string[] {
                "Need Approval", "Pending", "Ongoing", "Completed", "Canceled"
            });
            cmbStatusBooking.SelectedIndex = -1;
        }

        private DataTable GetClients()
        {
            if (cachedClients == null)
            {
                string query = "SELECT client_id, phone_number + ' - ' + first_name + ' ' + last_name AS name FROM clients";
                cachedClients = QueryToDataTable(query);
            }
            return cachedClients.Copy();
        }

        private DataTable GetEmployees()
        {
            if (cachedEmployees == null)
            {
                string query = "SELECT employee_id, phone_number + ' - ' + first_name + ' ' + last_name AS name FROM employees";
                cachedEmployees = QueryToDataTable(query);
            }
            return cachedEmployees.Copy();
        }

        private DataTable GetServices()
        {
            if (cachedServices == null)
            {
                string query = "SELECT service_id, service_name FROM services";
                cachedServices = QueryToDataTable(query);
            }
            return cachedServices.Copy();
        }

        private DataTable QueryToDataTable(string query)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private void InvalidateComboBoxCache()
        {
            cachedClients = null;
            cachedEmployees = null;
            cachedServices = null;
        }

        private void LoadStartTimeCombo()
        {
            cmbStartTime.Items.Clear();
            for (int h = 0; h <= 23; h++)
            {
                for (int m = 0; m < 60; m++)
                {
                    cmbStartTime.Items.Add(new TimeSpan(h, m, 0).ToString(@"hh\:mm"));
                }
            }
            cmbStartTime.SelectedIndex = -1;
        }

        private void LoadAppointments()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // ✅ 1️⃣ HAPUS data appointment yg lebih dari 3 bulan lalu
                using (SqlCommand cmdDelete = new SqlCommand(
                    "DELETE FROM appointments WHERE start_time < DATEADD(MONTH, -3, GETDATE())", conn))
                {
                    cmdDelete.ExecuteNonQuery();
                }

                // ✅ 2️⃣ UPDATE status booking (yang sudah kamu punya)
                using (SqlCommand cmdUpdate = new SqlCommand("sp_update_status_booking", conn))
                {
                    cmdUpdate.CommandType = CommandType.StoredProcedure;
                    cmdUpdate.ExecuteNonQuery();
                }

                // ✅ 3️⃣ AMBIL DATA appointment
                using (SqlCommand cmd = new SqlCommand("sp_get_appointments", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
        }


        private int GetServiceDuration(string serviceID)
        {
            int duration = 30;
            string query = "SELECT service_duration FROM services WHERE service_id = @id";
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", serviceID);
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                    duration = Convert.ToInt32(result);
            }
            return duration;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (cmbClientID.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih Client terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbServiceID.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih Service terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbStartTime.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih Jam terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {


                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_insert_appointment", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        string client = cmbClientID.SelectedValue.ToString();
                        string employee = cmbEmployeeID.SelectedValue?.ToString();
                        string service = cmbServiceID.SelectedValue.ToString();

                        DateTime tanggal = dtpTanggal.Value.Date;
                        TimeSpan jamMulai = TimeSpan.Parse(cmbStartTime.SelectedItem.ToString());
                        DateTime start = tanggal + jamMulai;
                        int durasi = GetServiceDuration(service);
                        DateTime end = start.AddMinutes(durasi);

                        string status = cmbStatusBooking.SelectedItem?.ToString();

                        cmd.Parameters.AddWithValue("@client_id", client);
                        cmd.Parameters.AddWithValue("@employee_id", string.IsNullOrEmpty(employee) ? (object)DBNull.Value : employee);
                        cmd.Parameters.AddWithValue("@service_id", service);
                        cmd.Parameters.AddWithValue("@start_time", start);
                        cmd.Parameters.AddWithValue("@end_time_expected", end);
                        cmd.Parameters.AddWithValue("@cancellation_reason", string.IsNullOrEmpty(txtCancellationReason.Text) ? (object)DBNull.Value : txtCancellationReason.Text);
                        cmd.Parameters.AddWithValue("@StatusBooking", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                        var result = cmd.ExecuteScalar();
                        transaction.Commit();
                        MessageBox.Show("Appointment berhasil ditambahkan!");
                    }
                    LoadAppointments();
                    InvalidateComboBoxCache();
                    LoadComboBoxes();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menambahkan: " + ex.Message);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbClientID.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih Client terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbServiceID.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih Service terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbStartTime.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih Jam terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView1.CurrentRow == null) return;

            var result = MessageBox.Show(
                "Apakah kamu yakin ingin memperbarui appointment ini?",
                "Konfirmasi Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes) return;

            string id = txtAppointmentID.Text;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_update_appointment", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        string client = cmbClientID.SelectedValue.ToString();
                        string employee = cmbEmployeeID.SelectedValue?.ToString();
                        string service = cmbServiceID.SelectedValue.ToString();
                        DateTime tanggal = dtpTanggal.Value.Date;
                        TimeSpan jamMulai = TimeSpan.Parse(cmbStartTime.SelectedItem.ToString());
                        DateTime start = tanggal + jamMulai;
                        int durasi = GetServiceDuration(service);
                        DateTime end = start.AddMinutes(durasi);

                        string status = cmbStatusBooking.SelectedItem?.ToString();

                        cmd.Parameters.AddWithValue("@appointment_id", id);
                        cmd.Parameters.AddWithValue("@client_id", client);
                        cmd.Parameters.AddWithValue("@employee_id", string.IsNullOrEmpty(employee) ? (object)DBNull.Value : employee);
                        cmd.Parameters.AddWithValue("@service_id", service);
                        cmd.Parameters.AddWithValue("@start_time", start);
                        cmd.Parameters.AddWithValue("@end_time_expected", end);
                        cmd.Parameters.AddWithValue("@cancellation_reason", string.IsNullOrEmpty(txtCancellationReason.Text) ? (object)DBNull.Value : txtCancellationReason.Text);
                        cmd.Parameters.AddWithValue("@StatusBooking", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        MessageBox.Show("Appointment berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    LoadAppointments();
                    InvalidateComboBoxCache();
                    LoadComboBoxes();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal update: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAppointments();
            InvalidateComboBoxCache();
            LoadComboBoxes();
            LoadStartTimeCombo();
            ClearForm();

            MessageBox.Show("Data berhasil direfresh!", "Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void ClearForm()
        {
            txtAppointmentID.Clear();
            cmbClientID.SelectedIndex = -1;
            cmbEmployeeID.SelectedIndex = -1;
            cmbServiceID.SelectedIndex = -1;
            cmbStartTime.SelectedIndex = -1;
            cmbStatusBooking.SelectedIndex = -1;
            txtCancellationReason.Clear();
            dtpTanggal.Value = DateTime.Now;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                // Klik header → biarkan saja
                return;
            }

            if (dataGridView1.Rows[e.RowIndex].IsNewRow)
            {
                MessageBox.Show("Data tidak valid atau kosong. Silakan pilih baris yang memiliki data.",
                                "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            txtAppointmentID.Text = row.Cells["appointment_id"].Value?.ToString();
            cmbClientID.SelectedValue = row.Cells["client_id"].Value?.ToString();

            object empValue = row.Cells["employee_id"].Value;
            if (empValue != DBNull.Value && empValue != null)
                cmbEmployeeID.SelectedValue = empValue.ToString();
            else
                cmbEmployeeID.SelectedIndex = -1;

            cmbServiceID.SelectedValue = row.Cells["service_id"].Value?.ToString();

            if (row.Cells["start_time"].Value != DBNull.Value)
            {
                DateTime startTime = Convert.ToDateTime(row.Cells["start_time"].Value);
                dtpTanggal.Value = startTime.Date;
                cmbStartTime.SelectedItem = startTime.ToString("HH:mm");
            }

            txtCancellationReason.Text = row.Cells["cancellation_reason"].Value?.ToString();
            cmbStatusBooking.SelectedItem = row.Cells["StatusBooking"].Value?.ToString();
        }

        private void dtpTanggal_ValueChanged(object sender, EventArgs e) { }

        private void UpdateDateRange()
        {
            dtpTanggal.MinDate = DateTime.Today.AddMonths(-3);
            dtpTanggal.MaxDate = DateTime.Today.AddMonths(3);
        }

        private void cmbEmployeeID_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbClientID_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbServiceID_SelectedIndexChanged(object sender, EventArgs e) { }

        // ComboBox filter (search-as-you-type)
        private void cmbClientID_TextUpdate(object sender, EventArgs e)
        {
            FilterComboBox(cmbClientID, GetClients(), "name", "client_id");
        }

        private void cmbEmployeeID_TextUpdate(object sender, EventArgs e)
        {
            FilterComboBox(cmbEmployeeID, GetEmployees(), "name", "employee_id");
        }

        private void cmbServiceID_TextUpdate(object sender, EventArgs e)
        {
            FilterComboBox(cmbServiceID, GetServices(), "service_name", "service_id");
        }

        private void FilterComboBox(ComboBox combo, DataTable dt, string displayMember, string valueMember)
        {
            string searchText = combo.Text.Trim().ToLower();
            DataTable filtered;

            if (!string.IsNullOrEmpty(searchText))
            {
                var rows = dt.AsEnumerable()
                    .Where(r => r.Field<string>(displayMember).ToLower().Contains(searchText));
                filtered = rows.Any() ? rows.CopyToDataTable() : dt.Clone();
            }
            else
            {
                filtered = dt;
            }

            string currentText = combo.Text;

            combo.DataSource = filtered;
            combo.DisplayMember = displayMember;
            combo.ValueMember = valueMember;
            combo.DroppedDown = true;

            // Jangan set ke 0, selalu -1 agar tidak error
            combo.SelectedIndex = -1;

            combo.Text = currentText;
            combo.SelectionStart = currentText.Length;
            combo.SelectionLength = 0;
        }




    }
}