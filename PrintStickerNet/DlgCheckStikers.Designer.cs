namespace PrintSticker
{
    partial class DlgCheckStikers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DlgCheckStikers));
            this.label19 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRead = new System.Windows.Forms.TextBox();
            this.txtBarcode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtQR = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.txtShop = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbCount = new System.Windows.Forms.Label();
            this.cbResetStiker = new System.Windows.Forms.CheckBox();
            this.cbNOT_EAN13 = new System.Windows.Forms.CheckBox();
            this.btExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(4, 23);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(96, 20);
            this.label19.TabIndex = 47;
            this.label19.Text = "Считываем";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 85);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(205, 20);
            this.label1.TabIndex = 43;
            this.label1.Text = "EAN13 Bar Code (Barcode)";
            // 
            // txtRead
            // 
            this.txtRead.BackColor = System.Drawing.Color.PaleGreen;
            this.txtRead.Location = new System.Drawing.Point(8, 49);
            this.txtRead.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtRead.Name = "txtRead";
            this.txtRead.Size = new System.Drawing.Size(944, 26);
            this.txtRead.TabIndex = 42;
            this.txtRead.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtRead_KeyUp);
            // 
            // txtBarcode
            // 
            this.txtBarcode.BackColor = System.Drawing.Color.LightGray;
            this.txtBarcode.Location = new System.Drawing.Point(8, 111);
            this.txtBarcode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new System.Drawing.Size(199, 26);
            this.txtBarcode.TabIndex = 44;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(219, 85);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 20);
            this.label2.TabIndex = 48;
            this.label2.Text = "QR Code";
            // 
            // txtQR
            // 
            this.txtQR.BackColor = System.Drawing.Color.LightGray;
            this.txtQR.Location = new System.Drawing.Point(222, 111);
            this.txtQR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtQR.Name = "txtQR";
            this.txtQR.Size = new System.Drawing.Size(730, 26);
            this.txtQR.TabIndex = 49;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 20;
            this.listBox1.Location = new System.Drawing.Point(9, 149);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(943, 424);
            this.listBox1.TabIndex = 50;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(842, 586);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 35);
            this.button1.TabIndex = 51;
            this.button1.Text = "Закрыть";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtShop
            // 
            this.txtShop.Location = new System.Drawing.Point(330, 11);
            this.txtShop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtShop.Name = "txtShop";
            this.txtShop.ReadOnly = true;
            this.txtShop.Size = new System.Drawing.Size(72, 26);
            this.txtShop.TabIndex = 52;
            this.txtShop.Text = "QQQ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(147, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 20);
            this.label3.TabIndex = 53;
            this.label3.Text = "Передаем в магазин";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 592);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(237, 20);
            this.label4.TabIndex = 54;
            this.label4.Text = "Подготовлено для передачи:";
            // 
            // lbCount
            // 
            this.lbCount.AutoSize = true;
            this.lbCount.Location = new System.Drawing.Point(243, 592);
            this.lbCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbCount.Name = "lbCount";
            this.lbCount.Size = new System.Drawing.Size(18, 20);
            this.lbCount.TabIndex = 55;
            this.lbCount.Text = "0";
            // 
            // cbResetStiker
            // 
            this.cbResetStiker.AutoSize = true;
            this.cbResetStiker.Location = new System.Drawing.Point(422, 14);
            this.cbResetStiker.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbResetStiker.Name = "cbResetStiker";
            this.cbResetStiker.Size = new System.Drawing.Size(252, 24);
            this.cbResetStiker.TabIndex = 56;
            this.cbResetStiker.Text = "сбросить переданный стикер";
            this.cbResetStiker.UseVisualStyleBackColor = true;
            this.cbResetStiker.CheckedChanged += new System.EventHandler(this.cbResetStiker_CheckedChanged);
            // 
            // cbNOT_EAN13
            // 
            this.cbNOT_EAN13.AutoSize = true;
            this.cbNOT_EAN13.Location = new System.Drawing.Point(690, 14);
            this.cbNOT_EAN13.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbNOT_EAN13.Name = "cbNOT_EAN13";
            this.cbNOT_EAN13.Size = new System.Drawing.Size(178, 24);
            this.cbNOT_EAN13.TabIndex = 57;
            this.cbNOT_EAN13.Text = "EAN13 не печатали";
            this.cbNOT_EAN13.UseVisualStyleBackColor = true;
            // 
            // btExit
            // 
            this.btExit.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btExit.Location = new System.Drawing.Point(294, 586);
            this.btExit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btExit.Name = "btExit";
            this.btExit.Size = new System.Drawing.Size(189, 35);
            this.btExit.TabIndex = 58;
            this.btExit.Text = "Выйти из программы";
            this.btExit.UseVisualStyleBackColor = true;
            this.btExit.Click += new System.EventHandler(this.btExit_Click);
            // 
            // DlgCheckStikers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(958, 626);
            this.Controls.Add(this.btExit);
            this.Controls.Add(this.cbNOT_EAN13);
            this.Controls.Add(this.cbResetStiker);
            this.Controls.Add(this.lbCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtShop);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtQR);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtRead);
            this.Controls.Add(this.txtBarcode);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DlgCheckStikers";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Передача сттикеров";
            this.Load += new System.EventHandler(this.CheckStikers_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRead;
        private System.Windows.Forms.TextBox txtBarcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtQR;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtShop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbCount;
        private System.Windows.Forms.CheckBox cbResetStiker;
        private System.Windows.Forms.CheckBox cbNOT_EAN13;
        private System.Windows.Forms.Button btExit;
    }
}