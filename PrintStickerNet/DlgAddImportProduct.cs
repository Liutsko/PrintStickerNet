using System;
using System.Data;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgAddImportProduct : Form {

        private static int _nLastSex = 0;
        private DataTable _dataTableRow = null;
        private (string product, string import) _item;

        public DlgAddImportProduct() {
            InitializeComponent();
        }
        public (string product, string import) GetSelItems() {
            return _item;
        }
        public void SetEditDataTableRow(DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count) return;
            _dataTableRow = dt;
        }
        private void DlgAddImportProduct_Load(object sender, EventArgs e) {
            cbImport.SelectedIndex = _nLastSex;

            if (null != _dataTableRow) {
                Text = "Изменить";
                btOK.Text = "Изменить";

                int PRODUCT = 2;
                int IMPORT = 3;

                txtProduct.Text = _dataTableRow.Rows[0][PRODUCT].ToString();
                string strImport = _dataTableRow.Rows[0][IMPORT].ToString();
                for (int i = 0; i < cbImport.Items.Count; i++) {
                    if (cbImport.Items[i].ToString().ToLower() == strImport.ToLower()) {
                        cbImport.SelectedIndex = i;
                        break;
                    }
                }
            }
            _Resize();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }

        private void btCancel_Click(object sender, EventArgs e) {

        }

        private void btOK_Click(object sender, EventArgs e) {
            if ("" == txtProduct.Text.Trim()) {
                MessageBox.Show(this, "Поле " + label23.Text + " должно быть пустым", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _item = new(txtProduct.Text, cbImport.Text);

            if (null != _dataTableRow) {
                int PRODUCT = 2;
                int IMPORT = 3;
                _dataTableRow.Rows[0][PRODUCT] = txtProduct.Text;
                _dataTableRow.Rows[0][IMPORT] = cbImport.Text;
            }
            _nLastSex = cbImport.SelectedIndex;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            timer1.Stop();
            txtProduct.Focus();
        }
    }
}
