//using Excel;
using PrintSticker.MarkingObjectsBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;


#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker
{
    public partial class DlgAddToTableSoputka : Form
    {
        private static int _nMod = 0;
        private static int _nArt2 = 0;
        private static string _strArt = "";
        private static string _strRaz = "";
        private static string _strKol = "1";
        private static string _strPATTERN = "1";
        private static string _strCCODE = "";
        private static string _strCCLOTH = "";
        private static string _strCSEASON = "";

        private readonly List<RestItem> _listItems = [];
        private DataTable _dataTableRow = null;

        public DlgAddToTableSoputka()
        {
            InitializeComponent();
        }
        public void SetEditDataTableRow(DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count) return;
            _dataTableRow = dt;
        }

        public void GetSelItems(out List<RestItem> listItems) {
            listItems = _listItems;
        }
        private void DlgAddToTableSoputka_Load(object sender, EventArgs e) {
            //cbMOD.SelectedIndex = _nMod;
            txtART.Text = _strArt;
            txtRAZ.Text = _strRaz;
            txtKOL.Text = _strKol;
            txtPATTERN.Text = _strPATTERN;

            //
            DlgCountriesBvSp dlgCountries = new(false);
            List<string> listNames = dlgCountries.GetNames();
            listNames.Sort();
            cbArt2.Items.Clear();
            foreach (string strCity in listNames)
                cbArt2.Items.Add(strCity);
            if (listNames.Count > 0)
                cbArt2.SelectedIndex = _nArt2;
            //
            DlgProductBvSp dlgProduct = new(false);
            listNames = dlgProduct.GetNames();
            listNames.Sort();
            cbMOD.Items.Clear();
            foreach (string strProduct in listNames)
                cbMOD.Items.Add(strProduct);
            if (listNames.Count > 0)
                cbMOD.SelectedIndex = _nMod;
            //


            ColorSP colorSP = new();
            if (int.TryParse(_strCCODE, out int nColor))
                txtCCODE.Text = colorSP.GetNameColorWithCode(nColor);
            else
                txtCCODE.Text = _strCCODE;

            txtCCLOTH.Text = _strCCLOTH;
            txtCSEASON.Text = _strCSEASON;

            if (null != _dataTableRow) {
                Text = "Изменить";
                btOK.Text = "Изменить";

                string strNameIZD = _dataTableRow.Rows[0][RESTCOLID.MOD].ToString();
                for (int i = 0; i < cbMOD.Items.Count; i++) {
                    if (cbMOD.Items[i].ToString().ToLower() == strNameIZD.ToLower()) {
                        cbMOD.SelectedIndex = i;
                        break;
                    }
                }
                txtART.Text = _dataTableRow.Rows[0][RESTCOLID.ART].ToString();
                txtRAZ.Text = _dataTableRow.Rows[0][RESTCOLID.RAZ].ToString();
                txtKOL.Text = _dataTableRow.Rows[0][RESTCOLID.KOL].ToString();
                txtPATTERN.Text = _dataTableRow.Rows[0][RESTCOLID.PATTERN].ToString();
                                
                string strColor = _dataTableRow.Rows[0][RESTCOLID.CCODE].ToString();
                if (int.TryParse(strColor, out nColor))
                    txtCCODE.Text = colorSP.GetNameColorWithCode(nColor);
                else
                    txtCCODE.Text = strColor;

                txtCCLOTH.Text = _dataTableRow.Rows[0][RESTCOLID.CCLOTH].ToString();
                txtCSEASON.Text = _dataTableRow.Rows[0][RESTCOLID.CSEASON].ToString();
                string strArt2 = _dataTableRow.Rows[0][RESTCOLID.ART2].ToString();
                for (int i = 0; i < cbArt2.Items.Count; i++) {
                    if (cbArt2.Items[i].ToString().ToLower() == strArt2.ToLower()) {
                        cbArt2.SelectedIndex = i;
                        break;
                    }
                }
                if (System.DBNull.Value != _dataTableRow.Rows[0][RESTCOLID.GTIN]) {
                    string strGtin = _dataTableRow.Rows[0][RESTCOLID.GTIN].ToString();
                    if ("" != strGtin && strGtin.Length > 12) {
                        txtRAZ.Enabled = false;
                        txtPATTERN.Enabled = false;
                        txtCCODE.Enabled = false;
                        cbMOD.Enabled = false;
                        btProductDB.Enabled = false;
                        cbArt2.Enabled = false;
                        txtART.Enabled = false;
                        txtCCLOTH.Enabled = false;
                        btCountryDB.Enabled = false;
                    }
                }
            }
            _Resize();
        }        
        private void _ShowError1(string strNameField, int nMinLen = 1) {
            if (nMinLen == 1)
                MessageBox.Show("Поле: " + "\"" + strNameField + "\"" + " не должно быть пустым", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("Поле: " + "\"" + strNameField + "\"" + " не должно быть пустым и длиною менее " + nMinLen + " символов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        private bool _CheckValue(TextBox txt, long lFrom = 1, long lTo = 999) {
            if (!long.TryParse(txt.Text, out _) || long.Parse(txt.Text) < lFrom || long.Parse(txt.Text) > lTo)
                return false;
            return true;
        }
        private void _ShowError2(long lFrom = 0, long lTo = 1000) {
            MessageBox.Show("Поле с размером не должно быть пустым и должно быть более " + lFrom + " и менее " + lTo + "", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        private bool _CheckValues() {
            if ("" == txtART.Text.Trim() || txtART.Text.Trim().Length < 2) { _ShowError1(label23.Text, 2); return false; }
            if ("" == txtKOL.Text.Trim()) { _ShowError1(label22.Text, 1); return false; }

            if (!_CheckValue(txtKOL)) { _ShowError2(); return false; }

            return true;
        }
        private void btOK_Click(object sender, EventArgs e) {
            if (!_CheckValues())
                return;
            _listItems.Clear();
            RestItem ri = new();
            ri.SetMOD(cbMOD.Text);
            ri.SetART2(cbArt2.Text);
            ri.SetIZD(ri.GetIzdByMOD());
            ri.SetART(txtART.Text);
            ri.SetRAZ(txtRAZ.Text);
            ri.SetKOL(int.Parse(txtKOL.Text));
            ri.SetPATTERN(txtPATTERN.Text);

            ColorSP colorSP = new();
            int nColor = colorSP.GetCodeColor(txtCCODE.Text);

            ri.SetCCODE(nColor.ToString());
            ri.SetCCLOTH(txtCCLOTH.Text);
            ri.SetCSEASON(txtCSEASON.Text);                    
            _listItems.Add(ri);

            _nMod = cbMOD.SelectedIndex;
            _nArt2 = cbArt2.SelectedIndex;
            _strArt = txtART.Text;
            _strRaz = txtRAZ.Text;
            _strKol = txtKOL.Text;
            _strPATTERN = txtPATTERN.Text;
            _strCCODE = txtCCODE.Text;
            _strCCLOTH = txtCCLOTH.Text;
            _strCSEASON = txtCSEASON.Text;
            if (null != _dataTableRow) {
                _dataTableRow.Rows[0][RESTCOLID.IZD] = _listItems[0].GetIzdByMOD(); 
                _dataTableRow.Rows[0][RESTCOLID.MOD] = _listItems[0].GetMOD();
                _dataTableRow.Rows[0][RESTCOLID.ART] = _listItems[0].GetART();
                _dataTableRow.Rows[0][RESTCOLID.RAZ] = _listItems[0].GetRAZ();
                _dataTableRow.Rows[0][RESTCOLID.KOL] = _listItems[0].GetKOL();
                _dataTableRow.Rows[0][RESTCOLID.ART2] = _listItems[0].GetART2();
                _dataTableRow.Rows[0][RESTCOLID.PATTERN] = _listItems[0].GetPATTERN();
                _dataTableRow.Rows[0][RESTCOLID.CCODE] = _listItems[0].GetCCODE();
                _dataTableRow.Rows[0][RESTCOLID.CCLOTH] = _listItems[0].GetCCLOTH();
                _dataTableRow.Rows[0][RESTCOLID.CSEASON] = _listItems[0].GetCSEASON();
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btSostav_Click(object sender, EventArgs e) {            
            Composition compos = new();
            DlgSelSostav dlg = new(txtCCLOTH.Text, compos.GetListMaterials());
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            txtCCLOTH.Text = dlg.GetSostav();
        }

        private void btAdditionak_Click(object sender, EventArgs e) {            
            DlgDopParams dlg = new(txtCSEASON.Text);
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            txtCSEASON.Text = dlg.GetParams();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        private void btCancel_Click(object sender, EventArgs e) {

        }

        private void btColor_Click(object sender, EventArgs e) {            
            ColorSP colorSP = new();
            int nColor = colorSP.GetCodeColor(txtCCODE.Text);


            DlgSelColor dlg = new(nColor, colorSP.GetListColor());
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            txtCCODE.Text = dlg.GetColor();
        }

        private void btColorDB_Click(object sender, EventArgs e) {
            DlgColors dlg = new();
            dlg.ShowDialog(this);
        }

        private void btСompositionDB_Click(object sender, EventArgs e) {
            DlgComposition dlg = new();
            dlg.ShowDialog(this);
        }

        private void btCountryDB_Click(object sender, EventArgs e) {
            DlgCountriesBvSp dlg = new(false);
            dlg.ShowDialog(this);
            if (!dlg.IsChanged())
                return;
            List<string> listNames = dlg.GetNames();
            listNames.Sort();
            cbArt2.Items.Clear();
            foreach (string strCity in listNames)
                cbArt2.Items.Add(strCity);
            cbArt2.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e) {
            DlgProductBvSp dlg = new(false);
            dlg.ShowDialog(this);
            if (!dlg.IsChanged())
                return;
            List<string> listNames = dlg.GetNames();
            listNames.Sort();
            cbMOD.Items.Clear();
            foreach (string strCity in listNames)
                cbMOD.Items.Add(strCity);
            cbMOD.SelectedIndex = 0;
        }
    }
}
