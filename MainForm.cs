using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barbershop
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UcPelanggan());
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Tampilkan form login kembali
            Form1 loginForm = new Form1();
            loginForm.Show();

            // Sembunyikan (atau tutup) form ini
            this.Hide(); // atau this.Close(); kalau ingin benar-benar menutup
        }

        private void panelContent_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LoadUserControl(UserControl uc)
        {
            panelContent.Controls.Clear(); // Hapus isi sebelumnya
            uc.Dock = DockStyle.Fill;      // Isi penuh panel
            panelContent.Controls.Add(uc); // Tambah yang baru
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UcDashboard());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UcLayanan());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UcKaryawan());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UcJadwalKaryawan());
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        
    }
}
