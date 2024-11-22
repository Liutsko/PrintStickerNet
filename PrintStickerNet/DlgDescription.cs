using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgDescription : Form {
        private string _strDescr = "";
        public DlgDescription() {
            InitializeComponent();
        }

        public void SetText(string strVal) {
            _strDescr = strVal;
        }
        public string GetText() {
            return _strDescr;
        }
        private void btCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void btOK_Click(object sender, EventArgs e) {
            _strDescr = txtDescr.Text;
            Close();
        }

        private void DlgDescription_Load(object sender, EventArgs e) {
            txtDescr.Text = _strDescr.Trim();
            
        }

        private void timer1_Tick(object sender, EventArgs e) {
            timer1.Stop();
            btCancel.Focus();
        }
    }
}
