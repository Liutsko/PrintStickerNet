using System;
using PrintSticker.MarkingObjectsBase;
using System.Data;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgAddTnved : Form {
        private (string product, string tnved, string sex, string compos, string descr) _item;
        private DataTable _dataTableRow = null;
        //private readonly _Form1 _parent;

        private static int _nLastProduct = 0;
        private static int _nLastSex = 0;

        public DlgAddTnved() {
            InitializeComponent();
            //_parent = parent;
        }
        public (string product, string tnved, string sex, string compos, string descr) GetSelItems() {
            return _item;
        }
        public void SetEditDataTableRow(DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count) return;
            _dataTableRow = dt;
        }

        private void DlgAddTnved_Load(object sender, EventArgs e) {
            cbMOD.SelectedIndex = _nLastProduct;
            cbSex.SelectedIndex = _nLastSex;

            if (null != _dataTableRow) {
                Text = "Изменить";
                btOK.Text = "Изменить";

                int PRODUCT = 2;
                int TNVED = 3;
                int SEX = 4;
                int COMPOS = 5;
                int DESCR = 6;

                string strProduct = _dataTableRow.Rows[0][PRODUCT].ToString();
                for (int i = 0; i < cbMOD.Items.Count; i++) {
                    if (cbMOD.Items[i].ToString().ToLower() == strProduct.ToLower()) {
                        cbMOD.SelectedIndex = i;
                        break;
                    }
                }
                txtTNVED.Text = _dataTableRow.Rows[0][TNVED].ToString();
                string strSex = _dataTableRow.Rows[0][SEX].ToString();
                for (int i = 0; i < cbSex.Items.Count; i++) {
                    if (cbSex.Items[i].ToString().ToLower() == strSex.ToLower()) {
                        cbSex.SelectedIndex = i;
                        break;
                    }
                }
                txtCompos.Text = _dataTableRow.Rows[0][COMPOS].ToString();
                txtDESCR.Text = _dataTableRow.Rows[0][DESCR].ToString();
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
            if (!long.TryParse(txtTNVED.Text, out long num)) {
                MessageBox.Show(this, "В поле " + label23.Text + " должно быть число", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(10 != num.ToString().Length) {
                MessageBox.Show(this, "В поле " + label23.Text + " должно быть 10 цифр", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }           
            _item = new (cbMOD.Text, txtTNVED.Text, cbSex.Text, txtCompos.Text, txtDESCR.Text);

            if (null != _dataTableRow) {
                int PRODUCT = 2;
                int TNVED = 3;
                int SEX = 4;
                int COMPOS = 5;
                int DESCR = 6;
                _dataTableRow.Rows[0][PRODUCT] = cbMOD.Text;
                _dataTableRow.Rows[0][TNVED] = txtTNVED.Text;
                _dataTableRow.Rows[0][SEX] = cbSex.Text;
                _dataTableRow.Rows[0][COMPOS] = txtCompos.Text;
                _dataTableRow.Rows[0][DESCR] = txtDESCR.Text;
            }
            _nLastProduct = cbMOD.SelectedIndex;
            _nLastSex = cbSex.SelectedIndex;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btAddCompos_Click(object sender, EventArgs e) {
            Composition compos = new();           
            DlgHidenOrders dlg = new ();
            dlg.SetCaption("Выберите");
            dlg.Width = 300;

            List<string> listAllMaterials = compos.GetListMaterials();
            if ("" == listAllMaterials[0])
                listAllMaterials.RemoveAt(0);

            List<string> listSelMaterials = [];
            foreach (string str in listAllMaterials) {
                if (-1 != txtCompos.Text.LastIndexOf(compos.GetCodeMaterial(str)))
                    listSelMaterials.Add(str);
            }

            dlg.Add(listAllMaterials);
            dlg.Sel(listSelMaterials);

            if (DialogResult.OK != dlg.ShowDialog())
                return;

            List<string> list = dlg.GetSelItems();
            string strMaterials = "";
            foreach (string str in list) {
                strMaterials += compos.GetCodeMaterial(str);
                strMaterials += ",";
            }
            if ("" != strMaterials)
                strMaterials = strMaterials.Substring(0, strMaterials.Length - 1);

            txtCompos.Text =  strMaterials;

        }

        private void btCancel_Click(object sender, EventArgs e) {

        }
    }
}
