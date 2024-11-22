using System;
using System.Data;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgAddComposition : Form {
        private (string name, string shortName) _item;
        private DataTable _dataTableRow = null;

        public DlgAddComposition() {
            InitializeComponent();
        }
        public void SetEditDataTableRow(DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count) return;
            _dataTableRow = dt;
        }
        public (string name, string shortName) GetSelItems() {
            return _item;
        }
        private void DlgAddComposition_Load(object sender, EventArgs e) {
            if (null != _dataTableRow) {
                Text = "Изменить";
                btOK.Text = "Изменить";

                int NAME = 2;
                int SHORT = 3;
                txtName.Text = _dataTableRow.Rows[0][NAME].ToString();
                txtShortName.Text = _dataTableRow.Rows[0][SHORT].ToString();
            }
            _Resize();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        private void btOK_Click(object sender, EventArgs e) {
            _item = new(txtName.Text, txtShortName.Text);

            if (null != _dataTableRow) {
                int NAME = 2;
                int SHORT = 3;
                _dataTableRow.Rows[0][NAME] = txtName.Text;
                _dataTableRow.Rows[0][SHORT] = txtShortName.Text;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
