using System;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым


namespace PrintSticker {
    public partial class DlgDopParams : Form {
        private string _strOarams = "";
        public DlgDopParams(string strOarams) {
            _strOarams = strOarams;
            InitializeComponent();
        }

        public string GetParams() {
            return _strOarams;
        }
        private void button1_Click(object sender, EventArgs e) {
            _strOarams = "";
            if (rbKlassika.Checked) _strOarams += "0201_";
            if (rbSlim.Checked) _strOarams += "0202_";
            if (rbG.Checked) _strOarams += "0217_";

            if (rbLong.Checked) _strOarams += "0411_";
            if (rbShort.Checked) _strOarams += "0412_";

            if (rbGolf.Checked) _strOarams += "0102_";
            if (rbV.Checked) _strOarams += "0103_";

            if (_strOarams.Length > 1)
                _strOarams = _strOarams.Substring(0, _strOarams.Length - 1);
        }

        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        private void DlgDopParams_Load(object sender, EventArgs e) {
            if (-1 != _strOarams.LastIndexOf("0201")) rbKlassika.Checked = true;
            if (-1 != _strOarams.LastIndexOf("0202")) rbSlim.Checked = true;
            if (-1 != _strOarams.LastIndexOf("0217")) rbG.Checked = true;

            if (-1 != _strOarams.LastIndexOf("0411")) rbLong.Checked = true;
            if (-1 != _strOarams.LastIndexOf("0412")) rbShort.Checked = true;

            if (-1 != _strOarams.LastIndexOf("0102")) rbGolf.Checked = true;
            if (-1 != _strOarams.LastIndexOf("0103")) rbV.Checked = true;
            _Resize();
        }
    }
}
