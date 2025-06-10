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
        // UI Caching: Simpan instance UserControl agar tidak dibuat ulang setiap navigasi
        private UcClients _ucClients;
        private UcAppointments _ucAppointments;
        private UcServices _ucServices;
        private UcEmployee _ucEmployee;
        private UcEmployeeSchedule _ucEmployeeSchedule;
        private UcServiceCategories _ucServiceCategories;
        private UcTransactionHistory _ucTransactionHistory;

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
            // Clients
            if (_ucClients == null)
                _ucClients = new UcClients();
            LoadUserControl(_ucClients);
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Tampilkan form login kembali
            LoginForm loginForm = new LoginForm();
            loginForm.Show();

            // Sembunyikan (atau tutup) form ini
            this.Hide(); // atau this.Close(); kalau ingin benar-benar menutup
        }

        private void panelContent_Paint(object sender, PaintEventArgs e)
        {

        }

        // Optimasi: Jangan load ulang UserControl yang sama
        private void LoadUserControl(UserControl uc)
        {
            if (panelContent.Controls.Count > 0 && panelContent.Controls[0] == uc)
                return;

            panelContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelContent.Controls.Add(uc);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Appointments
            if (_ucAppointments == null)
                _ucAppointments = new UcAppointments();
            LoadUserControl(_ucAppointments);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Services
            if (_ucServices == null)
                _ucServices = new UcServices();
            LoadUserControl(_ucServices);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Employee
            if (_ucEmployee == null)
                _ucEmployee = new UcEmployee();
            LoadUserControl(_ucEmployee);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Employee Schedule
            if (_ucEmployeeSchedule == null)
                _ucEmployeeSchedule = new UcEmployeeSchedule();
            LoadUserControl(_ucEmployeeSchedule);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Service Categories
            if (_ucServiceCategories == null)
                _ucServiceCategories = new UcServiceCategories();
            LoadUserControl(_ucServiceCategories);
        }

        private void btnLaporanOperasional_Click(object sender, EventArgs e)
        {
            // Transaction History
            if (_ucTransactionHistory == null)
                _ucTransactionHistory = new UcTransactionHistory();
            LoadUserControl(_ucTransactionHistory);
        }
    }
}
