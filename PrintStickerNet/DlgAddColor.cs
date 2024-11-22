using System;
using System.Data;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgAddColor : Form {

        private (int num, string name, string shortName) _item;
        private DataTable _dataTableRow = null;
        public DlgAddColor() {
            InitializeComponent();
        }
        public void SetEditDataTableRow(DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count) return;
            _dataTableRow = dt;
        }

        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        public (int num, string name, string shortName) GetSelItems() {
            return _item;
        }
        private void DlgAddColor_Load(object sender, EventArgs e) {
            if (null != _dataTableRow) {
                Text = "Изменить";
                btOK.Text = "Изменить";

                int NCOLOR = 2;
                int COLOR = 3;
                int SHORT = 4;
                txtNum.Text = _dataTableRow.Rows[0][NCOLOR].ToString();
                txtName.Text = _dataTableRow.Rows[0][COLOR].ToString();
                txtShortName.Text = _dataTableRow.Rows[0][SHORT].ToString();
            }
            _Resize();
        }
        private void btOK_Click(object sender, EventArgs e) {
            if (!int.TryParse(txtNum.Text, out int num)) {
                MessageBox.Show(this, "В поле " + label1.Text + " должно быть число", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _item = new(num, txtName.Text, txtShortName.Text);

            if (null != _dataTableRow) {
                int NCOLOR = 2;
                int COLOR = 3;
                int SHORT = 4;
                _dataTableRow.Rows[0][NCOLOR] = txtNum.Text;
                _dataTableRow.Rows[0][COLOR] = txtName.Text;
                _dataTableRow.Rows[0][SHORT] = txtShortName.Text;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        private void btCancel_Click(object sender, EventArgs e) {

        }
    }
}
