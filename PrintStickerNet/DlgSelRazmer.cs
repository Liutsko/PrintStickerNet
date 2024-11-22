using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker
{
    public partial class DlgSelRazmer : Form
    {
        private bool _bMultySize = false;
        private static string _strDate = "";
        private string _strPrefix = "";
        private readonly bool _bSoputks = false;
        public DlgSelRazmer(bool bSoputks = false)
        {
            _bSoputks = bSoputks;
            InitializeComponent();
        }
        public bool IsSelectMultySize() { return _bMultySize; }
        public string GetDate() { return _strDate; }
        public string GetPrefix() { return _strPrefix; }

        public PrintParms GetPrintParms() {
            PrintParms pp = new() {
                bMultySize = _bMultySize,
                strDate = _strDate,
                strPrefix = _strPrefix
            };
            return pp;
        }
        private void _GetDataToReturn() {
            _bMultySize = false;
            if (radioButton1.Checked)
                _bMultySize = true;
            _strDate = txtDate.Text;
            _strPrefix = txtPrefix.Text;
        }
        private void button1_Click(object sender, EventArgs e) {
            _GetDataToReturn();
        }
        private void DlgSelRazmer_FormClosing(object sender, FormClosingEventArgs e) {
            _GetDataToReturn();
        }

        private void DlgSelRazmer_Load(object sender, EventArgs e) {
            if (_bSoputks) {
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                DateTime dt = DateTime.Now;
                txtDate.Text = dt.Month.ToString("00") + " " +dt.Year.ToString();
            }
            _Resize();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btOK.Left = this.ClientSize.Width - btOK.Width - 4;
        }
        private void timer1_Tick(object sender, EventArgs e) {
            timer1.Stop();
            txtPrefix.Focus();
        }
    }
}
