namespace PrintSticker {
    partial class DlgSelSostav {
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
            this.btCancel = new System.Windows.Forms.Button();
            this.cb1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb2 = new System.Windows.Forms.ComboBox();
            this.cb3 = new System.Windows.Forms.ComboBox();
            this.txt1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt2 = new System.Windows.Forms.TextBox();
            this.txt3 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(244, 119);
            this.btOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(112, 35);
            this.btOK.TabIndex = 0;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.button1_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(365, 119);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(112, 35);
            this.btCancel.TabIndex = 1;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // cb1
            // 
            this.cb1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb1.DropDownWidth = 190;
            this.cb1.FormattingEnabled = true;
            this.cb1.Location = new System.Drawing.Point(10, 30);
            this.cb1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cb1.Name = "cb1";
            this.cb1.Size = new System.Drawing.Size(150, 28);
            this.cb1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Материал";
            // 
            // cb2
            // 
            this.cb2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb2.DropDownWidth = 190;
            this.cb2.FormattingEnabled = true;
            this.cb2.Location = new System.Drawing.Point(168, 30);
            this.cb2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cb2.Name = "cb2";
            this.cb2.Size = new System.Drawing.Size(150, 28);
            this.cb2.TabIndex = 4;
            // 
            // cb3
            // 
            this.cb3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb3.DropDownWidth = 190;
            this.cb3.FormattingEnabled = true;
            this.cb3.Location = new System.Drawing.Point(326, 30);
            this.cb3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cb3.Name = "cb3";
            this.cb3.Size = new System.Drawing.Size(150, 28);
            this.cb3.TabIndex = 6;
            // 
            // txt1
            // 
            this.txt1.Location = new System.Drawing.Point(10, 83);
            this.txt1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt1.MaxLength = 3;
            this.txt1.Name = "txt1";
            this.txt1.Size = new System.Drawing.Size(150, 26);
            this.txt1.TabIndex = 7;
            this.txt1.Text = "100";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 63);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Состав (в %)";
            // 
            // txt2
            // 
            this.txt2.Location = new System.Drawing.Point(168, 83);
            this.txt2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt2.MaxLength = 3;
            this.txt2.Name = "txt2";
            this.txt2.Size = new System.Drawing.Size(150, 26);
            this.txt2.TabIndex = 9;
            // 
            // txt3
            // 
            this.txt3.Location = new System.Drawing.Point(326, 83);
            this.txt3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt3.MaxLength = 3;
            this.txt3.Name = "txt3";
            this.txt3.Size = new System.Drawing.Size(150, 26);
            this.txt3.TabIndex = 10;
            // 
            // DlgSelSostav
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 161);
            this.Controls.Add(this.txt3);
            this.Controls.Add(this.txt2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt1);
            this.Controls.Add(this.cb3);
            this.Controls.Add(this.cb2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgSelSostav";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Состав";
            this.Load += new System.EventHandler(this.DlgSelSostav_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.ComboBox cb1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cb2;
        private System.Windows.Forms.ComboBox cb3;
        private System.Windows.Forms.TextBox txt1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt2;
        private System.Windows.Forms.TextBox txt3;
    }
}