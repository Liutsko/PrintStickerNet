namespace PrintSticker {
    partial class DlgEditToRaskroyny {
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
            this.btOK = new System.Windows.Forms.Button();
            this.txtCost = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbTM = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbCLOTH = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbSeason = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbOther = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btOK
            // 
            this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOK.Location = new System.Drawing.Point(137, 288);
            this.btOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(112, 35);
            this.btOK.TabIndex = 3;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // txtCost
            // 
            this.txtCost.Location = new System.Drawing.Point(12, 27);
            this.txtCost.MaxLength = 9;
            this.txtCost.Name = "txtCost";
            this.txtCost.Size = new System.Drawing.Size(139, 26);
            this.txtCost.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Закупочная цена";
            // 
            // cbTM
            // 
            this.cbTM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTM.DropDownWidth = 350;
            this.cbTM.FormattingEnabled = true;
            this.cbTM.Location = new System.Drawing.Point(12, 248);
            this.cbTM.Name = "cbTM";
            this.cbTM.Size = new System.Drawing.Size(237, 28);
            this.cbTM.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 225);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Код торговой марки";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "Код состава ткани";
            // 
            // cbCLOTH
            // 
            this.cbCLOTH.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCLOTH.DropDownWidth = 237;
            this.cbCLOTH.FormattingEnabled = true;
            this.cbCLOTH.Location = new System.Drawing.Point(12, 80);
            this.cbCLOTH.Name = "cbCLOTH";
            this.cbCLOTH.Size = new System.Drawing.Size(215, 28);
            this.cbCLOTH.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 169);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "Код сезона";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // cbSeason
            // 
            this.cbSeason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSeason.FormattingEnabled = true;
            this.cbSeason.Location = new System.Drawing.Point(12, 192);
            this.cbSeason.Name = "cbSeason";
            this.cbSeason.Size = new System.Drawing.Size(164, 28);
            this.cbSeason.TabIndex = 10;
            this.cbSeason.SelectedIndexChanged += new System.EventHandler(this.cbSeason_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 113);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 20);
            this.label5.TabIndex = 13;
            this.label5.Text = "Доп признаки";
            // 
            // cbOther
            // 
            this.cbOther.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOther.FormattingEnabled = true;
            this.cbOther.Location = new System.Drawing.Point(12, 136);
            this.cbOther.Name = "cbOther";
            this.cbOther.Size = new System.Drawing.Size(164, 28);
            this.cbOther.TabIndex = 12;
            // 
            // DlgEditToRaskroyny
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 329);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbOther);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbSeason);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbCLOTH);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbTM);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCost);
            this.Controls.Add(this.btOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgEditToRaskroyny";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Изменить";
            this.Load += new System.EventHandler(this.DlgEditToRaskroyny_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.TextBox txtCost;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbTM;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbCLOTH;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbSeason;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbOther;
    }
}