namespace PrintSticker {
    partial class DlgAddTnved {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.cbMOD = new System.Windows.Forms.ComboBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.label23 = new System.Windows.Forms.Label();
            this.txtTNVED = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbSex = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCompos = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDESCR = new System.Windows.Forms.TextBox();
            this.btAddCompos = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 20);
            this.label1.TabIndex = 151;
            this.label1.Text = "Наименование изделия";
            // 
            // cbMOD
            // 
            this.cbMOD.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMOD.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbMOD.FormattingEnabled = true;
            this.cbMOD.Items.AddRange(new object[] {
            "БЛУЗА",
            "БЛУЗА-ТОП",
            "БЛУЗКА",
            "БРЮКИ",
            "ВОДОЛАЗКА",
            "ГАЛСТУК",
            "ДЖЕМПЕР",
            "ДЖИНСЫ",
            "ЖАКЕТ",
            "ЖИЛЕТ",
            "КАРДИГАН",
            "КОМПЛЕКТ",
            "КОСТЮМ",
            "ПИДЖАК",
            "ПЛАТЬЕ",
            "ПОЛО",
            "РУБАШКА",
            "САРАФАН",
            "СВИТЕР",
            "СВИТШОТ",
            "СОРОЧКА",
            "ТОП",
            "ТУНИКА",
            "ФУТБОЛКА",
            "ХУДИ",
            "ШОРТЫ",
            "ЮБКА",
            "ПАЛЬТО"});
            this.cbMOD.Location = new System.Drawing.Point(7, 30);
            this.cbMOD.Name = "cbMOD";
            this.cbMOD.Size = new System.Drawing.Size(236, 28);
            this.cbMOD.TabIndex = 150;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCancel.Location = new System.Drawing.Point(459, 182);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(112, 35);
            this.btCancel.TabIndex = 153;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btOK.Location = new System.Drawing.Point(340, 182);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(112, 35);
            this.btOK.TabIndex = 152;
            this.btOK.Text = "Добавить";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label23.Location = new System.Drawing.Point(246, 5);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(65, 20);
            this.label23.TabIndex = 155;
            this.label23.Text = "ТНВЭД";
            // 
            // txtTNVED
            // 
            this.txtTNVED.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtTNVED.Location = new System.Drawing.Point(252, 30);
            this.txtTNVED.MaxLength = 10;
            this.txtTNVED.Name = "txtTNVED";
            this.txtTNVED.Size = new System.Drawing.Size(140, 26);
            this.txtTNVED.TabIndex = 154;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(392, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 20);
            this.label2.TabIndex = 157;
            this.label2.Text = "Пол";
            // 
            // cbSex
            // 
            this.cbSex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSex.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbSex.FormattingEnabled = true;
            this.cbSex.Items.AddRange(new object[] {
            "М",
            "Ж"});
            this.cbSex.Location = new System.Drawing.Point(398, 30);
            this.cbSex.Name = "cbSex";
            this.cbSex.Size = new System.Drawing.Size(102, 28);
            this.cbSex.TabIndex = 156;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(1, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 20);
            this.label5.TabIndex = 159;
            this.label5.Text = "Состав";
            // 
            // txtCompos
            // 
            this.txtCompos.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtCompos.Location = new System.Drawing.Point(7, 90);
            this.txtCompos.MaxLength = 50;
            this.txtCompos.Name = "txtCompos";
            this.txtCompos.Size = new System.Drawing.Size(385, 26);
            this.txtCompos.TabIndex = 158;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(3, 125);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 20);
            this.label6.TabIndex = 161;
            this.label6.Text = "Комментарий";
            // 
            // txtDESCR
            // 
            this.txtDESCR.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtDESCR.Location = new System.Drawing.Point(7, 150);
            this.txtDESCR.MaxLength = 100;
            this.txtDESCR.Name = "txtDESCR";
            this.txtDESCR.Size = new System.Drawing.Size(564, 26);
            this.txtDESCR.TabIndex = 160;
            // 
            // btAddCompos
            // 
            this.btAddCompos.Location = new System.Drawing.Point(398, 84);
            this.btAddCompos.Name = "btAddCompos";
            this.btAddCompos.Size = new System.Drawing.Size(35, 35);
            this.btAddCompos.TabIndex = 162;
            this.btAddCompos.Text = "...";
            this.btAddCompos.UseVisualStyleBackColor = true;
            this.btAddCompos.Click += new System.EventHandler(this.btAddCompos_Click);
            // 
            // DlgAddTnved
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 226);
            this.Controls.Add(this.btAddCompos);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtDESCR);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtCompos);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbSex);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.txtTNVED);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbMOD);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgAddTnved";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавить ТНВЭД";
            this.Load += new System.EventHandler(this.DlgAddTnved_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbMOD;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox txtTNVED;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbSex;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCompos;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDESCR;
        private System.Windows.Forms.Button btAddCompos;
    }
}