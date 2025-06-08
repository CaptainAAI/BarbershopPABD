namespace Barbershop
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnKategoriLayanan = new System.Windows.Forms.Button();
            this.btnLaporanOperasional = new System.Windows.Forms.Button();
            this.btnListLayanan = new System.Windows.Forms.Button();
            this.btnJadwalKaryawan = new System.Windows.Forms.Button();
            this.btnPelanggan = new System.Windows.Forms.Button();
            this.btnKaryawan = new System.Windows.Forms.Button();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.button8 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnKategoriLayanan
            // 
            this.btnKategoriLayanan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnKategoriLayanan.Location = new System.Drawing.Point(8, 237);
            this.btnKategoriLayanan.Margin = new System.Windows.Forms.Padding(2);
            this.btnKategoriLayanan.Name = "btnKategoriLayanan";
            this.btnKategoriLayanan.Size = new System.Drawing.Size(181, 45);
            this.btnKategoriLayanan.TabIndex = 6;
            this.btnKategoriLayanan.Text = "SERVICE CATEGORIES";
            this.btnKategoriLayanan.UseVisualStyleBackColor = true;
            this.btnKategoriLayanan.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnLaporanOperasional
            // 
            this.btnLaporanOperasional.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLaporanOperasional.Location = new System.Drawing.Point(8, 480);
            this.btnLaporanOperasional.Margin = new System.Windows.Forms.Padding(2);
            this.btnLaporanOperasional.Name = "btnLaporanOperasional";
            this.btnLaporanOperasional.Size = new System.Drawing.Size(181, 49);
            this.btnLaporanOperasional.TabIndex = 11;
            this.btnLaporanOperasional.Text = "TRANSACTION HISTORY";
            this.btnLaporanOperasional.UseVisualStyleBackColor = true;
            this.btnLaporanOperasional.Click += new System.EventHandler(this.btnLaporanOperasional_Click);
            // 
            // btnListLayanan
            // 
            this.btnListLayanan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnListLayanan.Location = new System.Drawing.Point(8, 285);
            this.btnListLayanan.Margin = new System.Windows.Forms.Padding(2);
            this.btnListLayanan.Name = "btnListLayanan";
            this.btnListLayanan.Size = new System.Drawing.Size(181, 45);
            this.btnListLayanan.TabIndex = 7;
            this.btnListLayanan.Text = "SERVICE";
            this.btnListLayanan.UseVisualStyleBackColor = true;
            this.btnListLayanan.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnJadwalKaryawan
            // 
            this.btnJadwalKaryawan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnJadwalKaryawan.Location = new System.Drawing.Point(8, 432);
            this.btnJadwalKaryawan.Margin = new System.Windows.Forms.Padding(2);
            this.btnJadwalKaryawan.Name = "btnJadwalKaryawan";
            this.btnJadwalKaryawan.Size = new System.Drawing.Size(181, 45);
            this.btnJadwalKaryawan.TabIndex = 10;
            this.btnJadwalKaryawan.Text = "EMPLOYEE SCHEDULE";
            this.btnJadwalKaryawan.UseVisualStyleBackColor = true;
            this.btnJadwalKaryawan.Click += new System.EventHandler(this.button6_Click);
            // 
            // btnPelanggan
            // 
            this.btnPelanggan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPelanggan.Location = new System.Drawing.Point(8, 334);
            this.btnPelanggan.Margin = new System.Windows.Forms.Padding(2);
            this.btnPelanggan.Name = "btnPelanggan";
            this.btnPelanggan.Size = new System.Drawing.Size(181, 45);
            this.btnPelanggan.TabIndex = 8;
            this.btnPelanggan.Text = "CLIENTS";
            this.btnPelanggan.UseVisualStyleBackColor = true;
            this.btnPelanggan.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnKaryawan
            // 
            this.btnKaryawan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnKaryawan.Location = new System.Drawing.Point(8, 383);
            this.btnKaryawan.Margin = new System.Windows.Forms.Padding(2);
            this.btnKaryawan.Name = "btnKaryawan";
            this.btnKaryawan.Size = new System.Drawing.Size(181, 45);
            this.btnKaryawan.TabIndex = 9;
            this.btnKaryawan.Text = "EMPLOYEE";
            this.btnKaryawan.UseVisualStyleBackColor = true;
            this.btnKaryawan.Click += new System.EventHandler(this.button5_Click);
            // 
            // btnDashboard
            // 
            this.btnDashboard.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDashboard.Location = new System.Drawing.Point(8, 188);
            this.btnDashboard.Margin = new System.Windows.Forms.Padding(2);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(181, 45);
            this.btnDashboard.TabIndex = 5;
            this.btnDashboard.Text = "APPOINTMENTS";
            this.btnDashboard.UseVisualStyleBackColor = true;
            this.btnDashboard.Click += new System.EventHandler(this.button1_Click);
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelContent.Controls.Add(this.pictureBox2);
            this.panelContent.Location = new System.Drawing.Point(193, 8);
            this.panelContent.Margin = new System.Windows.Forms.Padding(2);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(1075, 708);
            this.panelContent.TabIndex = 16;
            this.panelContent.Paint += new System.Windows.Forms.PaintEventHandler(this.panelContent_Paint);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(202, 41);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(605, 646);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button8.Location = new System.Drawing.Point(8, 666);
            this.button8.Margin = new System.Windows.Forms.Padding(2);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(181, 49);
            this.button8.TabIndex = 12;
            this.button8.Text = "LOG OUT";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(181, 176);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1274, 723);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.btnLaporanOperasional);
            this.Controls.Add(this.btnKategoriLayanan);
            this.Controls.Add(this.btnJadwalKaryawan);
            this.Controls.Add(this.btnListLayanan);
            this.Controls.Add(this.btnKaryawan);
            this.Controls.Add(this.btnPelanggan);
            this.Controls.Add(this.btnDashboard);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnLaporanOperasional;
        private System.Windows.Forms.Button btnJadwalKaryawan;
        private System.Windows.Forms.Button btnKaryawan;
        private System.Windows.Forms.Button btnPelanggan;
        private System.Windows.Forms.Button btnListLayanan;
        private System.Windows.Forms.Button btnKategoriLayanan;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}