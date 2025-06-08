using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;


namespace Barbershop
{
    public partial class UcTransactionHistory : UserControl
    {
        // String koneksi ke database SQL Azure
        private string connString = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=Barbershop;Persist Security Info=False;User ID=LordAAI;Password=OmkegasOmkegas2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

        // Konstruktor UserControl
        public UcTransactionHistory()
        {
            InitializeComponent();
        }

        // Event handler saat UserControl dimuat
        private void UcTransactionHistory_Load(object sender, EventArgs e)
        {
            LoadTransactionData(); // Memuat data transaksi
            LoadComboBoxes();      // Memuat data ke ComboBox filter
            dtpDateFrom.Value = DateTime.Today.AddDays(-7); // Set default tanggal filter dari
            dtpDateUntil.Value = DateTime.Today;             // Set default tanggal filter sampai
        }

        // Memuat data transaksi ke DataGridView, dengan filter opsional
        private void LoadTransactionData(string filterQuery = "")
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT 
                        transaction_id,
                        appointment_id,
                        client_name,
                        phone_number,
                        employee_id,
                        employee_name,
                        service_name,
                        service_price,
                        appointment_date,
                        start_time,
                        end_time,
                        status,
                        cancellation_reason,
                        recorded_at
                    FROM transaction_history
                    WHERE 1=1 " + filterQuery + @"
                    ORDER BY recorded_at DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvTransactionHistory.DataSource = dt; // Tampilkan data ke DataGridView
            }
        }

        // Memuat data ke ComboBox tertentu dari query SQL
        private void LoadComboBox(string query, ComboBox cmb, string displayMember, string valueMember)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmb.DataSource = dt;
                cmb.DisplayMember = displayMember;
                cmb.ValueMember = valueMember;
                cmb.SelectedIndex = -1; // Tidak ada yang dipilih secara default
            }
        }

        // Memuat semua ComboBox filter (client, employee, service, status)
        private void LoadComboBoxes()
        {
            // ComboBox client
            LoadComboBox("SELECT client_id, phone_number + ' - ' + first_name + ' ' + last_name AS display FROM clients", cmbClient, "display", "client_id");
            // ComboBox employee
            LoadComboBox("SELECT employee_id, phone_number + ' - ' + first_name + ' ' + last_name AS display FROM employees", cmbEmployee, "display", "employee_id");
            // ComboBox service
            LoadComboBox("SELECT service_id, service_id + ' - ' + service_name AS display FROM services", cmbService, "display", "service_id");

            // ComboBox status
            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new string[] {
                "Need Approval", "Pending", "Ongoing", "Completed", "Canceled"
            });
            cmbStatus.SelectedIndex = -1;
        }

        // Event handler tombol Filter, menerapkan filter ke data transaksi
        private void btnFilterData_Click(object sender, EventArgs e)
        {
            string filter = "";

            // Filter berdasarkan client
            if (cmbClient.SelectedValue != null)
                filter += $" AND client_name LIKE '%{cmbClient.Text.Split('-').Last().Trim()}%'";

            // Filter berdasarkan employee
            if (cmbEmployee.SelectedValue != null)
                filter += $" AND employee_name LIKE '%{cmbEmployee.Text.Split('-').Last().Trim()}%'";

            // Filter berdasarkan service
            if (cmbService.SelectedValue != null)
                filter += $" AND service_name LIKE '%{cmbService.Text.Split('-').Last().Trim()}%'";

            // Filter berdasarkan status
            if (!string.IsNullOrEmpty(cmbStatus.Text))
                filter += $" AND status = '{cmbStatus.Text}'";

            // Filter berdasarkan rentang tanggal
            filter += $" AND appointment_date BETWEEN '{dtpDateFrom.Value:yyyy-MM-dd}' AND '{dtpDateUntil.Value:yyyy-MM-dd}'";

            LoadTransactionData(filter); // Muat data dengan filter
        }

        // Event handler tombol Reset Filter, mengembalikan filter ke default
        private void btnResetFilter_Click(object sender, EventArgs e)
        {
            cmbClient.SelectedIndex = -1;
            cmbEmployee.SelectedIndex = -1;
            cmbService.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            dtpDateFrom.Value = DateTime.Today.AddDays(-7);
            dtpDateUntil.Value = DateTime.Today;
            LoadTransactionData(); // Muat ulang data tanpa filter
        }

        // Event handler tombol Refresh, memuat ulang data transaksi
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTransactionData();
        }

        private void dgvTransactionHistory_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbClient_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbEmployee_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbService_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbDateUntil_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbDateFrom_SelectedIndexChanged(object sender, EventArgs e) { }
        private void btnImportData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                DataTable dt = ReadExcelToDataTable(filePath);

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("File kosong atau tidak valid!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Cek apakah kolom wajib ada
                string[] requiredCols = {
            "client_name", "phone_number", "employee_id", "employee_name",
            "service_name", "service_price", "appointment_date",
            "start_time", "end_time", "status"
        };

                foreach (string col in requiredCols)
                {
                    if (!dt.Columns.Contains(col))
                    {
                        MessageBox.Show($"Kolom '{col}' tidak ditemukan dalam file Excel!", "Kolom Tidak Lengkap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Tampilkan preview meskipun isi belum divalidasi
                previewForm preview = new previewForm(dt); // Validasi dilakukan saat klik tombol Import di form preview
                preview.ShowDialog();
            }
        }

        private DataTable ReadExcelToDataTable(string filePath)
        {
            DataTable dt = new DataTable();

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0); // Ambil sheet pertama

                    IRow headerRow = sheet.GetRow(0);
                    int columnCount = headerRow.LastCellNum;

                    // Tambah kolom ke datatable
                    for (int i = 0; i < columnCount; i++)
                    {
                        string colName = headerRow.GetCell(i)?.ToString()?.Trim();
                        dt.Columns.Add(!string.IsNullOrEmpty(colName) ? colName : $"Col{i}");
                    }

                    // Tambah baris data
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;

                        DataRow dataRow = dt.NewRow();
                        for (int j = 0; j < columnCount; j++)
                        {
                            dataRow[j] = row.GetCell(j)?.ToString()?.Trim() ?? string.Empty;
                        }
                        dt.Rows.Add(dataRow);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return dt;
        }



        private void btnGenerateDataPdf_Click(object sender, EventArgs e) { }
        private void btnGenerateDataExcel_Click(object sender, EventArgs e) { }
        }
}
