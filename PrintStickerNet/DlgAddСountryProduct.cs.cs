using System;
using System.Data;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgAddСountryProduct : Form {

        private (string name, string fullName) _item;
        private DataTable _dataTableRow = null;
        private readonly  string _strCaption;
        public void SetEditDataTableRow(DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count) return;
            _dataTableRow = dt;
        }
        public (string name, string fullName) GetSelItems() {
            return _item;
        }
        public DlgAddСountryProduct(string strCaption) {
            _strCaption = strCaption;
            InitializeComponent();
        }

        private void DlgAddСountry_Load(object sender, EventArgs e) {
            Text = _strCaption;
            if (null != _dataTableRow) {
                Text = "Изменить";
                btOK.Text = "Изменить";

                int NAME = 2;
                int NAMEFULL = 3;
                txtName.Text = _dataTableRow.Rows[0][NAME].ToString();
                txtFullName.Text = _dataTableRow.Rows[0][NAMEFULL].ToString();
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
            _item = new(txtName.Text, txtFullName.Text);
            if (null != _dataTableRow) {
                int NAME = 2;
                int NAMEFULL = 3;
                _dataTableRow.Rows[0][NAME] = txtName.Text;
                _dataTableRow.Rows[0][NAMEFULL] = txtFullName.Text;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
