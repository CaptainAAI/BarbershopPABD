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
                cmbNamaKaryawan.DisplayMember = "full_name"; // tampilannya
                cmbNamaKaryawan.ValueMember = "employee_id"; // value yg diproses
                cmbNamaKaryawan.SelectedIndex = -1;
            }
        }

    }
}