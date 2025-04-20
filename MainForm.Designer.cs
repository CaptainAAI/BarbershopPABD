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
            this.button2 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.button8 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button2.Location = new System.Drawing.Point(12, 152);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(271, 69);
            this.button2.TabIndex = 6;
            this.button2.Text = "KATEGORI LAYANAN";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button7.Location = new System.Drawing.Point(12, 527);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(271, 75);
            this.button7.TabIndex = 11;
            this.button7.Text = "LAPORAN OPERASIONAL";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button3.Location = new System.Drawing.Point(12, 227);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(271, 69);
            this.button3.TabIndex = 7;
            this.button3.Text = "LIST LAYANAN";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button6
            // 
            this.button6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button6.Location = new System.Drawing.Point(12, 452);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(271, 69);
            this.button6.TabIndex = 10;
            this.button6.Text = "JADWAL KARYAWAN";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button4
            // 
            this.button4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button4.Location = new System.Drawing.Point(12, 302);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(271, 69);
            this.button4.TabIndex = 8;
            this.button4.Text = "PELANGGAN";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button5.Location = new System.Drawing.Point(12, 377);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(271, 69);
            this.button5.TabIndex = 9;
            this.button5.Text = "KARYAWAN";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button1
            // 
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(271, 134);
            this.button1.TabIndex = 5;
            this.button1.Text = "DASHBOARD";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelContent.Location = new System.Drawing.Point(289, 12);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(1603, 1017);
            this.panelContent.TabIndex = 16;
            this.panelContent.Paint += new System.Windows.Forms.PaintEventHandler(this.panelContent_Paint);
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button8.Location = new System.Drawing.Point(12, 954);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(271, 75);
            this.button8.TabIndex = 12;
            this.button8.Text = "LOG OUT";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Button button8;
    }
}