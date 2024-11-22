using System;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgSelectLenghtKM : Form {

        private int _nSelLen = -1;
        public DlgSelectLenghtKM() {
            InitializeComponent();
        }
        public int GetSelLength() { 
            return _nSelLen;
        }
        private void btOK_Click(object sender, EventArgs e) {

            if (rb31.Checked)
                _nSelLen = 31;
        }
    }
}
