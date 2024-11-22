namespace PrintSticker {
    partial class DlgSelectLenghtKM {
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
            this.rb31 = new System.Windows.Forms.RadioButton();
            this.rbFull = new System.Windows.Forms.RadioButton();
            this.btOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rb31
            // 
            this.rb31.AutoSize = true;
            this.rb31.Checked = true;
            this.rb31.Location = new System.Drawing.Point(13, 12);
            this.rb31.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rb31.Name = "rb31";
            this.rb31.Size = new System.Drawing.Size(176, 24);
            this.rb31.TabIndex = 0;
            this.rb31.TabStop = true;
            this.rb31.Text = "31 символ (для ЧЗ)";
            this.rb31.UseVisualStyleBackColor = true;
            // 
            // rbFull
            // 
            this.rbFull.AutoSize = true;
            this.rbFull.Location = new System.Drawing.Point(13, 46);
            this.rbFull.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbFull.Name = "rbFull";
            this.rbFull.Size = new System.Drawing.Size(150, 24);
            this.rbFull.TabIndex = 1;
            this.rbFull.Text = "полная (для 1С)";
            this.rbFull.UseVisualStyleBackColor = true;
            // 
            // btOK
            // 
            this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOK.Location = new System.Drawing.Point(86, 80);
            this.btOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(112, 35);
            this.btOK.TabIndex = 2;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // DlgSelectLenghtKM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(204, 125);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.rbFull);
            this.Controls.Add(this.rb31);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgSelectLenghtKM";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Выберите длину кода маркировки";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rb31;
        private System.Windows.Forms.RadioButton rbFull;
        private System.Windows.Forms.Button btOK;
    }
}