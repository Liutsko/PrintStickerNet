using System;
using System.Collections.Generic;
using System.Windows.Forms;
#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым


namespace PrintSticker {
    public partial class DlgSample : Form {
        private List<(string name, string barcode)> _listNomenkl = null;

        //private readonly string _strSample = "";
        public DlgSample() {
            InitializeComponent();
            //_strSample = strSample;
        }

        private void DlgSample_Load(object sender, EventArgs e) {
            if (null == _listNomenkl)
                return;

            foreach ((string name, string barcode) in _listNomenkl) {
                ListViewItem lvi = new (name);
                lvi.SubItems.Add(barcode);
                listView1.Items.Add(lvi);
            }

            _Resize();
        }
        private void _Resize() {
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        public void To1СNomenkl(List<(string name, string barcode)> listNomenkl) {
            if (0 == listNomenkl.Count)
                return;

            _listNomenkl = listNomenkl;



        }
        private void btCancel_Click(object sender, EventArgs e) {

        }
    }
}
