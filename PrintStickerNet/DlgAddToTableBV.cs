using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;
using PrintSticker.MarkingObjectsBase;
using System.Diagnostics.Eventing.Reader;
//using Excel;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgAddToTableBV : Form {
        private readonly List<RestItemBV> _listItems = [];
        DataTable _dataTableRow = null;
        private bool _bEnableBarcode = false;
        public DlgAddToTableBV() {
            InitializeComponent();
        }
        public void SetEditDataTableRow(DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count) return;
            _dataTableRow = dt;
        }
        public void EnableBarcode(bool bEnable) {
            _bEnableBarcode = bEnable;
        }

        private void DlgAddToTable_Load(object sender, EventArgs e) {
            //cbMOD.SelectedIndex = 0;
            //cbArt2.SelectedIndex = 0;
            //txtEAN13.Enabled = false;

            txtEAN13.Enabled = _bEnableBarcode;
            DlgTMark dlg = new();
            List<string> listTM = dlg.GetTM();
            listTM.Sort();
            cbTMark.Items.Clear();
            foreach (string strTM in listTM)
                cbTMark.Items.Add(strTM);
            cbTMark.SelectedIndex = 0;

            //
            DlgCountriesBvSp dlgCountriesBV = new(true);
            List<string> listNames = dlgCountriesBV.GetNames();
            listNames.Sort();
            cbArt2.Items.Clear();
            foreach (string strCity in listNames)
                cbArt2.Items.Add(strCity);
            if(listNames.Count > 0)
                cbArt2.SelectedIndex = 0;
            //
            //
            DlgProductBvSp dlgProduct = new(true);
            listNames = dlgProduct.GetNames();
            listNames.Sort();
            cbMOD.Items.Clear();
            foreach (string strProduct in listNames)
                cbMOD.Items.Add(strProduct);
            if (listNames.Count > 0)
                cbMOD.SelectedIndex = 0;
            //


            if (null != _dataTableRow) {
                Text = "Изменить";
                btOK.Text = "Изменить";
                string strNameIZD = _GetNameByIZD(_dataTableRow.Rows[0][RESTCOLIDBV.IZD].ToString());
                for (int i = 0; i < cbMOD.Items.Count; i++) {
                    if (cbMOD.Items[i].ToString().ToLower() == strNameIZD.ToLower()) {
                        cbMOD.SelectedIndex = i;
                        break;
                    }
                }
                txtMOD.Text = _dataTableRow.Rows[0][RESTCOLIDBV.MOD].ToString();
                txtPRS.Text = _dataTableRow.Rows[0][RESTCOLIDBV.PRS].ToString();
                txtART.Text = _dataTableRow.Rows[0][RESTCOLIDBV.ART].ToString();
                txtEAN13.Text = _dataTableRow.Rows[0][RESTCOLIDBV.BARCODE].ToString();
                txtPATTERN.Text = _dataTableRow.Rows[0][RESTCOLIDBV.PATTERN].ToString();
                txtCCLOTH.Text = _dataTableRow.Rows[0][RESTCOLIDBV.CCLOTH2].ToString();


                cbTMark.Text = "";
                if (DBNull.Value != _dataTableRow.Rows[0][RESTCOLIDBV.TMARK])
                    cbTMark.Text = _dataTableRow.Rows[0][RESTCOLIDBV.TMARK].ToString();

                ColorBV colorBV = new();
                string strColor = _dataTableRow.Rows[0][RESTCOLIDBV.CCODE].ToString();
                if (int.TryParse(strColor, out int nColor))
                    txtCCODE.Text = colorBV.GetNameColorWithCode(nColor);
                else
                    txtCCODE.Text = strColor;

                txtSRT.Text = _dataTableRow.Rows[0][RESTCOLIDBV.SRT].ToString();

                string strArt2 = _dataTableRow.Rows[0][RESTCOLIDBV.ART2].ToString();
                for (int i = 0; i < cbArt2.Items.Count; i++) {
                    if (cbArt2.Items[i].ToString().ToLower() == strArt2.ToLower()) {
                        cbArt2.SelectedIndex = i;
                        break;
                    }
                }
                bool bEnabledRazm = true;
                if (System.DBNull.Value != _dataTableRow.Rows[0][RESTCOLIDBV.GTIN]) {
                    string strGtin = _dataTableRow.Rows[0][RESTCOLIDBV.GTIN].ToString();
                    if ("" != strGtin && strGtin.Length > 12) {
                        cbMOD.Enabled = false;
                        btProductDB.Enabled = false;
                        txtMOD.Enabled = false;
                        txtPRS.Enabled = false;
                        txtART.Enabled = false;
                        txtPATTERN.Enabled = false;
                        txtCCODE.Enabled = false;
                        cbArt2.Enabled = false;
                        bEnabledRazm = false;
                        btColor.Enabled = false;
                        btTMarkaDB.Enabled = false;
                        cbTMark.Enabled = false;
                        btCityBvDB.Enabled = false;
                        btSostav.Enabled = false;
                        btСompositionDB.Enabled = false;
                    }
                }


                string strRAZ = _dataTableRow.Rows[0][RESTCOLIDBV.RAZ].ToString();
                string strKOL = _dataTableRow.Rows[0][RESTCOLIDBV.KOL].ToString();
                _SetInTableRazmers(strRAZ, strKOL, bEnabledRazm);
                _OnTimer();
            } 
            _Resize();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        private int _GetColumnInTableRazmers(int nHeight) {
            if (158 == nHeight) return 0;
            if (164 == nHeight) return 1;
            if (170 == nHeight) return 2;
            if (176 == nHeight) return 3;
            if (182 == nHeight) return 4;
            if (188 == nHeight) return 5;
            if (194 == nHeight) return 6;
            if (200 == nHeight) return 7;
            return -1;
        }
        private int _GetRowInTableRazmers(int nRaz, int nPoln) {
            if (88 == nRaz && 76 == nPoln) return 0;
            if (92 == nRaz && 80 == nPoln) return 1;
            if (96 == nRaz && 84 == nPoln) return 2;
            if (100 == nRaz && 88 == nPoln) return 3;
            if (104 == nRaz && 92 == nPoln) return 4;
            if (108 == nRaz && 96 == nPoln) return 5;
            if (112 == nRaz && 100 == nPoln) return 6;
            if (116 == nRaz && 104 == nPoln) return 7;
            if (120 == nRaz && 108 == nPoln) return 8;
            if (124 == nRaz && 112 == nPoln) return 9;
            if (128 == nRaz && 116 == nPoln) return 10;
            return -1;
        }

        private TextBox[,] _GetArrayTB() {
            TextBox[,] arr = { { txt00_00, txt00_01, txt00_02, txt00_03, txt00_04, txt00_05, txt00_06, txt00_07 },
                               { txt01_00, txt01_01, txt01_02, txt01_03, txt01_04, txt01_05, txt01_06, txt01_07 },
                               { txt02_00, txt02_01, txt02_02, txt02_03, txt02_04, txt02_05, txt02_06, txt02_07 },
                               { txt03_00, txt03_01, txt03_02, txt03_03, txt03_04, txt03_05, txt03_06, txt03_07 },
                               { txt04_00, txt04_01, txt04_02, txt04_03, txt04_04, txt04_05, txt04_06, txt04_07 },
                               { txt05_00, txt05_01, txt05_02, txt05_03, txt05_04, txt05_05, txt05_06, txt05_07 },
                               { txt06_00, txt06_01, txt06_02, txt06_03, txt06_04, txt06_05, txt06_06, txt06_07 },
                               { txt07_00, txt07_01, txt07_02, txt07_03, txt07_04, txt07_05, txt07_06, txt07_07 },
                               { txt08_00, txt08_01, txt08_02, txt08_03, txt08_04, txt08_05, txt08_06, txt08_07 },
                               { txt09_00, txt09_01, txt09_02, txt09_03, txt09_04, txt09_05, txt09_06, txt09_07 },
                               { txt10_00, txt10_01, txt10_02, txt10_03, txt10_04, txt10_05, txt10_06, txt10_07 },
            };
            return arr;
           
        }
        private void _SetInTableRazmers(string strRAZ, string strKOL, bool bEnabledRazm) {
            string[] parms = strRAZ.Split('-');
            if (3 != parms.Length)
                return;
            if (!int.TryParse(parms[0], out int nHeight))
                return;
            if (!int.TryParse(parms[1], out int nRaz))
                return;
            if (!int.TryParse(parms[2], out int nPoln))
                return;

            int nColumn = _GetColumnInTableRazmers(nHeight);
            int nRow = _GetRowInTableRazmers(nRaz, nPoln);
            
            TextBox[,] arrTB = _GetArrayTB();
            for (int i = 0; i < 11; i++)
                for (int j = 0; j < 8; j++)
                    arrTB[i, j].Enabled = bEnabledRazm;

            if (nRow > 0 && nColumn > 0) {
                arrTB[nRow, nColumn].Text = strKOL;
                arrTB[nRow, nColumn].Enabled = true;
            }
        }
        public void GetSelItems(out List<RestItemBV> listItems) {
            listItems = _listItems;
        }
        private void _ShowError2(long lFrom = 0, long lTo = 1000) {
            MessageBox.Show("Поле с размером не должно быть пустым и должно быть более " + lFrom + " и менее " + lTo + "", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        private void _ShowError3(string strNameField, long lFrom = 0, long lTo = 1000) {
            MessageBox.Show("Поле: " + "\"" + strNameField + "\"" + " не должно быть пустым и должно быть более " + lFrom + " и менее " + lTo + "", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        private bool _CheckValue(TextBox txt, long lFrom = 0, long lTo = 999) {
            if (!long.TryParse(txt.Text, out _) || long.Parse(txt.Text) < lFrom || long.Parse(txt.Text) > lTo)
                return false;
            return true;
        }
        private void _ShowError1(string strNameField, int nMinLen = 1) {
            if (nMinLen == 1)
                MessageBox.Show("Поле: " + "\"" + strNameField + "\"" + " не должно быть пустым", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("Поле: " + "\"" + strNameField + "\"" + " не должно быть пустым и длиною менее " + nMinLen + " символов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        private bool _CheckValues() {
            if ("" == txtMOD.Text.Trim() || txtMOD.Text.Trim().Length < 2) { _ShowError1(label20.Text, 2); return false; }
            //if ("" == txtPRS.Text.Trim() || txtPRS.Text.Trim().Length < 2) { _ShowError1(label21.Text, 2); return false; }
            if ("" == txtART.Text.Trim() || txtART.Text.Trim().Length < 2) { _ShowError1(label23.Text, 2); return false; }
            if ("" == txtPATTERN.Text.Trim()) { _ShowError1(label22.Text); return false; }
            if ("" == txtCCODE.Text.Trim() || txtCCODE.Text.Trim().Length < 2) { _ShowError1(label24.Text, 2); return false; }
            if ("" == txtSRT.Text.Trim() || txtSRT.Text.Trim().Length < 2) { _ShowError1(label25.Text, 2); return false; }

            if (!_CheckValue(txtART, 1, 999999999999999)) { _ShowError3(label23.Text, 1, 999999999999999); return false; }
            if (!_CheckValue(txtPATTERN, 1, 9)) { _ShowError3(label22.Text, 1, 9); return false; }

            if ("" == txtCCODE.Text.Trim() || txtCCODE.Text.Trim().Length < 2) { _ShowError1(label24.Text, 2); return false; }

            //if (!_CheckValue(txtCCODE, 1, 999)) { _ShowError3(label24.Text, 1, 999); return false; }
            if (!_CheckValue(txtSRT, 1, 10)) { _ShowError3(label25.Text, 1, 10); return false; }            

            if (!_CheckValue(txt00_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt00_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt00_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt00_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt00_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt00_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt00_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt00_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt01_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt01_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt01_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt01_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt01_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt01_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt01_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt01_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt02_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt02_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt02_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt02_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt02_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt02_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt02_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt02_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt03_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt03_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt03_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt03_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt03_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt03_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt03_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt03_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt04_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt04_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt04_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt04_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt04_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt04_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt04_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt04_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt05_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt05_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt05_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt05_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt05_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt05_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt05_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt05_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt06_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt06_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt06_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt06_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt06_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt06_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt06_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt06_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt07_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt07_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt07_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt07_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt07_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt07_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt07_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt07_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt08_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt08_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt08_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt08_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt08_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt08_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt08_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt08_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt09_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt09_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt09_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt09_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt09_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt09_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt09_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt09_07)) { _ShowError2(); return false; }

            if (!_CheckValue(txt10_00)) { _ShowError2(); return false; }
            if (!_CheckValue(txt10_01)) { _ShowError2(); return false; }
            if (!_CheckValue(txt10_02)) { _ShowError2(); return false; }
            if (!_CheckValue(txt10_03)) { _ShowError2(); return false; }
            if (!_CheckValue(txt10_04)) { _ShowError2(); return false; }
            if (!_CheckValue(txt10_05)) { _ShowError2(); return false; }
            if (!_CheckValue(txt10_06)) { _ShowError2(); return false; }
            if (!_CheckValue(txt10_07)) { _ShowError2(); return false; }

            return true;
        }
        private void _AddItem(ref Dictionary<string, int> dicRazmCount, string strRazm, TextBox txt) {
            if (int.Parse(txt.Text) > 0)
                dicRazmCount.Add(strRazm, int.Parse(txt.Text));
        }
        private void _GetAllRazmers(ref Dictionary<string, int>  dicRazmCount)
        {
            _AddItem(ref dicRazmCount, "158-088-076", txt00_00);
            _AddItem(ref dicRazmCount, "164-088-076", txt00_01);
            _AddItem(ref dicRazmCount, "170-088-076", txt00_02);
            _AddItem(ref dicRazmCount, "176-088-076", txt00_03);
            _AddItem(ref dicRazmCount, "182-088-076", txt00_04);
            _AddItem(ref dicRazmCount, "188-088-076", txt00_05);
            _AddItem(ref dicRazmCount, "194-088-076", txt00_06);
            _AddItem(ref dicRazmCount, "200-088-076", txt00_07);

            _AddItem(ref dicRazmCount, "158-092-080", txt01_00);
            _AddItem(ref dicRazmCount, "164-092-080", txt01_01);
            _AddItem(ref dicRazmCount, "170-092-080", txt01_02);
            _AddItem(ref dicRazmCount, "176-092-080", txt01_03);
            _AddItem(ref dicRazmCount, "182-092-080", txt01_04);
            _AddItem(ref dicRazmCount, "188-092-080", txt01_05);
            _AddItem(ref dicRazmCount, "194-092-080", txt01_06);
            _AddItem(ref dicRazmCount, "200-092-080", txt01_07);

            _AddItem(ref dicRazmCount, "158-096-084", txt02_00);
            _AddItem(ref dicRazmCount, "164-096-084", txt02_01);
            _AddItem(ref dicRazmCount, "170-096-084", txt02_02);
            _AddItem(ref dicRazmCount, "176-096-084", txt02_03);
            _AddItem(ref dicRazmCount, "182-096-084", txt02_04);
            _AddItem(ref dicRazmCount, "188-096-084", txt02_05);
            _AddItem(ref dicRazmCount, "194-096-084", txt02_06);
            _AddItem(ref dicRazmCount, "200-096-084", txt02_07);

            _AddItem(ref dicRazmCount, "158-100-088", txt03_00);
            _AddItem(ref dicRazmCount, "164-100-088", txt03_01);
            _AddItem(ref dicRazmCount, "170-100-088", txt03_02);
            _AddItem(ref dicRazmCount, "176-100-088", txt03_03);
            _AddItem(ref dicRazmCount, "182-100-088", txt03_04);
            _AddItem(ref dicRazmCount, "188-100-088", txt03_05);
            _AddItem(ref dicRazmCount, "194-100-088", txt03_06);
            _AddItem(ref dicRazmCount, "200-100-088", txt03_07);

            _AddItem(ref dicRazmCount, "158-104-092", txt04_00);
            _AddItem(ref dicRazmCount, "164-104-092", txt04_01);
            _AddItem(ref dicRazmCount, "170-104-092", txt04_02);
            _AddItem(ref dicRazmCount, "176-104-092", txt04_03);
            _AddItem(ref dicRazmCount, "182-104-092", txt04_04);
            _AddItem(ref dicRazmCount, "188-104-092", txt04_05);
            _AddItem(ref dicRazmCount, "194-104-092", txt04_06);
            _AddItem(ref dicRazmCount, "200-104-092", txt04_07);

            _AddItem(ref dicRazmCount, "158-108-096", txt05_00);
            _AddItem(ref dicRazmCount, "164-108-096", txt05_01);
            _AddItem(ref dicRazmCount, "170-108-096", txt05_02);
            _AddItem(ref dicRazmCount, "176-108-096", txt05_03);
            _AddItem(ref dicRazmCount, "182-108-096", txt05_04);
            _AddItem(ref dicRazmCount, "188-108-096", txt05_05);
            _AddItem(ref dicRazmCount, "194-108-096", txt05_06);
            _AddItem(ref dicRazmCount, "200-108-096", txt05_07);

            _AddItem(ref dicRazmCount, "158-112-100", txt06_00);
            _AddItem(ref dicRazmCount, "164-112-100", txt06_01);
            _AddItem(ref dicRazmCount, "170-112-100", txt06_02);
            _AddItem(ref dicRazmCount, "176-112-100", txt06_03);
            _AddItem(ref dicRazmCount, "182-112-100", txt06_04);
            _AddItem(ref dicRazmCount, "188-112-100", txt06_05);
            _AddItem(ref dicRazmCount, "194-112-100", txt06_06);
            _AddItem(ref dicRazmCount, "200-112-100", txt06_07);

            _AddItem(ref dicRazmCount, "158-116-104", txt07_00);
            _AddItem(ref dicRazmCount, "164-116-104", txt07_01);
            _AddItem(ref dicRazmCount, "170-116-104", txt07_02);
            _AddItem(ref dicRazmCount, "176-116-104", txt07_03);
            _AddItem(ref dicRazmCount, "182-116-104", txt07_04);
            _AddItem(ref dicRazmCount, "188-116-104", txt07_05);
            _AddItem(ref dicRazmCount, "194-116-104", txt07_06);
            _AddItem(ref dicRazmCount, "200-116-104", txt07_07);

            _AddItem(ref dicRazmCount, "158-120-108", txt08_00);
            _AddItem(ref dicRazmCount, "164-120-108", txt08_01);
            _AddItem(ref dicRazmCount, "170-120-108", txt08_02);
            _AddItem(ref dicRazmCount, "176-120-108", txt08_03);
            _AddItem(ref dicRazmCount, "182-120-108", txt08_04);
            _AddItem(ref dicRazmCount, "188-120-108", txt08_05);
            _AddItem(ref dicRazmCount, "194-120-108", txt08_06);
            _AddItem(ref dicRazmCount, "200-120-108", txt08_07);

            _AddItem(ref dicRazmCount, "158-124-112", txt09_00);
            _AddItem(ref dicRazmCount, "164-124-112", txt09_01);
            _AddItem(ref dicRazmCount, "170-124-112", txt09_02);
            _AddItem(ref dicRazmCount, "176-124-112", txt09_03);
            _AddItem(ref dicRazmCount, "182-124-112", txt09_04);
            _AddItem(ref dicRazmCount, "188-124-112", txt09_05);
            _AddItem(ref dicRazmCount, "194-124-112", txt09_06);
            _AddItem(ref dicRazmCount, "200-124-112", txt09_07);

            _AddItem(ref dicRazmCount, "158-128-116", txt10_00);
            _AddItem(ref dicRazmCount, "164-128-116", txt10_01);
            _AddItem(ref dicRazmCount, "170-128-116", txt10_02);
            _AddItem(ref dicRazmCount, "176-128-116", txt10_03);
            _AddItem(ref dicRazmCount, "182-128-116", txt10_04);
            _AddItem(ref dicRazmCount, "188-128-116", txt10_05);
            _AddItem(ref dicRazmCount, "194-128-116", txt10_06);
            _AddItem(ref dicRazmCount, "200-128-116", txt10_07);
        }

        private string _GetCodeIDS(string strIDZ) {
            strIDZ = strIDZ.Trim().ToLower();
            if (-1 != strIDZ.LastIndexOf("костюм"))
                return "01";
            if (-1 != strIDZ.LastIndexOf("брюки"))
                return "02";
            if (-1 != strIDZ.LastIndexOf("пиджак"))
                return "04";
            return "";
        }
        private string _GetNameByIZD(string strIZD) {
            if (strIZD == "01") return "костюм";
            if (strIZD == "02") return "брюки";
            if (strIZD == "04") return "пиджак";
            return "????";
        }

        private void button1_Click(object sender, EventArgs e) {
            if (!_CheckValues())
                return;
            _listItems.Clear();

            string strCodeIZD = _GetCodeIDS(cbMOD.Text);
            Dictionary<string, int> dicRazmCount = [];
            _GetAllRazmers(ref dicRazmCount);
            if (null != _dataTableRow && 1 != dicRazmCount.Count) {
                MessageBox.Show("Должен быть выбран только один размер", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ColorBV colorBV = new();
            int nColor = colorBV.GetCodeColor(txtCCODE.Text);

            foreach (KeyValuePair<string, int> item in dicRazmCount) {
                RestItemBV ri = new();
                ri.SetIZD(strCodeIZD);
                ri.SetMOD(txtMOD.Text);
                ri.SetPRS(txtPRS.Text);
                ri.SetART(txtART.Text);
                ri.SetPATTERN(txtPATTERN.Text);
                ri.SetCCODE(nColor.ToString());
                ri.SetSRT(txtSRT.Text);

                ri.SetRAZ(item.Key);
                ri.SetKOL(item.Value);
                ri.SetART2(cbArt2.Text);
                ri.SetBARCODE(txtEAN13.Text.Trim());
                ri.SetTMark(cbTMark.Text);
                ri.SetCCLOTH2(txtCCLOTH.Text);
                _listItems.Add(ri);
            }
            if (null != _dataTableRow) {
                _dataTableRow.Rows[0][RESTCOLIDBV.IZD] = _listItems[0].GetIZD();
                _dataTableRow.Rows[0][RESTCOLIDBV.PRS] = _listItems[0].GetPRS();
                _dataTableRow.Rows[0][RESTCOLIDBV.MOD] = _listItems[0].GetMOD();
                _dataTableRow.Rows[0][RESTCOLIDBV.ART] = _listItems[0].GetART().Trim();
                _dataTableRow.Rows[0][RESTCOLIDBV.RAZ] = _listItems[0].GetRAZ();
                _dataTableRow.Rows[0][RESTCOLIDBV.KOL] = _listItems[0].GetKOL();
                _dataTableRow.Rows[0][RESTCOLIDBV.ART2] = _listItems[0].GetART2();
                _dataTableRow.Rows[0][RESTCOLIDBV.PATTERN] = _listItems[0].GetPATTERN();
                _dataTableRow.Rows[0][RESTCOLIDBV.CCODE] = _listItems[0].GetCCODE();
                _dataTableRow.Rows[0][RESTCOLIDBV.SRT] = _listItems[0].GetSRT();
                _dataTableRow.Rows[0][RESTCOLIDBV.BARCODE] = _listItems[0].GetBARCODE_0();
                _dataTableRow.Rows[0][RESTCOLIDBV.IZDNAME] = RestItemBV.GetIZDName(_listItems[0].GetIZD());
                _dataTableRow.Rows[0][RESTCOLIDBV.TMARK] = cbTMark.Text;
                _dataTableRow.Rows[0][RESTCOLIDBV.CCLOTH2] = txtCCLOTH.Text;
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }        
        private void _SetColorAndSumm(TextBox tb, ref int nSum) {            
            if (int.TryParse(tb.Text, out int nRez))
                nSum += nRez; 

            if ("0" == tb.Text) {
                if (tb.BackColor == Color.White)
                    return;
                tb.BackColor = Color.White;
            } else {
                if (tb.BackColor == Color.Yellow)
                    return;
                tb.BackColor = Color.Yellow;
            }
        }
        private void _OnTimer() {
            int nSum = 0;
            _SetColorAndSumm(txt00_00, ref nSum);
            _SetColorAndSumm(txt00_01, ref nSum);
            _SetColorAndSumm(txt00_02, ref nSum);
            _SetColorAndSumm(txt00_03, ref nSum);
            _SetColorAndSumm(txt00_04, ref nSum);
            _SetColorAndSumm(txt00_05, ref nSum);
            _SetColorAndSumm(txt00_06, ref nSum);
            _SetColorAndSumm(txt00_07, ref nSum);

            _SetColorAndSumm(txt01_00, ref nSum);
            _SetColorAndSumm(txt01_01, ref nSum);
            _SetColorAndSumm(txt01_02, ref nSum);
            _SetColorAndSumm(txt01_03, ref nSum);
            _SetColorAndSumm(txt01_04, ref nSum);
            _SetColorAndSumm(txt01_05, ref nSum);
            _SetColorAndSumm(txt01_06, ref nSum);
            _SetColorAndSumm(txt01_07, ref nSum);

            _SetColorAndSumm(txt02_00, ref nSum);
            _SetColorAndSumm(txt02_01, ref nSum);
            _SetColorAndSumm(txt02_02, ref nSum);
            _SetColorAndSumm(txt02_03, ref nSum);
            _SetColorAndSumm(txt02_04, ref nSum);
            _SetColorAndSumm(txt02_05, ref nSum);
            _SetColorAndSumm(txt02_06, ref nSum);
            _SetColorAndSumm(txt02_07, ref nSum);

            _SetColorAndSumm(txt03_00, ref nSum);
            _SetColorAndSumm(txt03_01, ref nSum);
            _SetColorAndSumm(txt03_02, ref nSum);
            _SetColorAndSumm(txt03_03, ref nSum);
            _SetColorAndSumm(txt03_04, ref nSum);
            _SetColorAndSumm(txt03_05, ref nSum);
            _SetColorAndSumm(txt03_06, ref nSum);
            _SetColorAndSumm(txt03_07, ref nSum);

            _SetColorAndSumm(txt04_00, ref nSum);
            _SetColorAndSumm(txt04_01, ref nSum);
            _SetColorAndSumm(txt04_02, ref nSum);
            _SetColorAndSumm(txt04_03, ref nSum);
            _SetColorAndSumm(txt04_04, ref nSum);
            _SetColorAndSumm(txt04_05, ref nSum);
            _SetColorAndSumm(txt04_06, ref nSum);
            _SetColorAndSumm(txt04_07, ref nSum);

            _SetColorAndSumm(txt05_00, ref nSum);
            _SetColorAndSumm(txt05_01, ref nSum);
            _SetColorAndSumm(txt05_02, ref nSum);
            _SetColorAndSumm(txt05_03, ref nSum);
            _SetColorAndSumm(txt05_04, ref nSum);
            _SetColorAndSumm(txt05_05, ref nSum);
            _SetColorAndSumm(txt05_06, ref nSum);
            _SetColorAndSumm(txt05_07, ref nSum);

            _SetColorAndSumm(txt06_00, ref nSum);
            _SetColorAndSumm(txt06_01, ref nSum);
            _SetColorAndSumm(txt06_02, ref nSum);
            _SetColorAndSumm(txt06_03, ref nSum);
            _SetColorAndSumm(txt06_04, ref nSum);
            _SetColorAndSumm(txt06_05, ref nSum);
            _SetColorAndSumm(txt06_06, ref nSum);
            _SetColorAndSumm(txt06_07, ref nSum);

            _SetColorAndSumm(txt07_00, ref nSum);
            _SetColorAndSumm(txt07_01, ref nSum);
            _SetColorAndSumm(txt07_02, ref nSum);
            _SetColorAndSumm(txt07_03, ref nSum);
            _SetColorAndSumm(txt07_04, ref nSum);
            _SetColorAndSumm(txt07_05, ref nSum);
            _SetColorAndSumm(txt07_06, ref nSum);
            _SetColorAndSumm(txt07_07, ref nSum);

            _SetColorAndSumm(txt08_00, ref nSum);
            _SetColorAndSumm(txt08_01, ref nSum);
            _SetColorAndSumm(txt08_02, ref nSum);
            _SetColorAndSumm(txt08_03, ref nSum);
            _SetColorAndSumm(txt08_04, ref nSum);
            _SetColorAndSumm(txt08_05, ref nSum);
            _SetColorAndSumm(txt08_06, ref nSum);
            _SetColorAndSumm(txt08_07, ref nSum);

            _SetColorAndSumm(txt09_00, ref nSum);
            _SetColorAndSumm(txt09_01, ref nSum);
            _SetColorAndSumm(txt09_02, ref nSum);
            _SetColorAndSumm(txt09_03, ref nSum);
            _SetColorAndSumm(txt09_04, ref nSum);
            _SetColorAndSumm(txt09_05, ref nSum);
            _SetColorAndSumm(txt09_06, ref nSum);
            _SetColorAndSumm(txt09_07, ref nSum);

            _SetColorAndSumm(txt10_00, ref nSum);
            _SetColorAndSumm(txt10_01, ref nSum);
            _SetColorAndSumm(txt10_02, ref nSum);
            _SetColorAndSumm(txt10_03, ref nSum);
            _SetColorAndSumm(txt10_04, ref nSum);
            _SetColorAndSumm(txt10_05, ref nSum);
            _SetColorAndSumm(txt10_06, ref nSum);
            _SetColorAndSumm(txt10_07, ref nSum);

            lbItogo.Text = nSum.ToString();
        }
        private void timer1_Tick(object sender, EventArgs e) {
            _OnTimer();            
        }

        private void btCancel_Click(object sender, EventArgs e) {

        }

        private void btColorDB_Click(object sender, EventArgs e) {
            DlgColors2 dlg = new();
            dlg.ShowDialog(this);

        }

        private void btColor_Click(object sender, EventArgs e) {
            ColorBV colorBV = new();
            int nColor = colorBV.GetCodeColor(txtCCODE.Text);


            DlgSelColor dlg = new(nColor, colorBV.GetListColor());
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            txtCCODE.Text = dlg.GetColor();

        }
        private void btTMarkaDB_Click(object sender, EventArgs e) {
            DlgTMark dlg = new();
            dlg.ShowDialog(this);

            if (!dlg.IsChanged())
                return;

            List<string> listTM = dlg.GetTM();
            listTM.Sort();
            cbTMark.Items.Clear();
            foreach (string strTM in listTM)
                cbTMark.Items.Add(strTM);
            cbTMark.SelectedIndex = 0;
        }

        private void btCityBvDB_Click(object sender, EventArgs e) {
            DlgCountriesBvSp dlg = new(true);
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

        private void btProductDB_Click(object sender, EventArgs e) {
            DlgProductBvSp dlg = new(true);
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
        private void btСompositionDB_Click(object sender, EventArgs e) {
            DlgComposition dlg = new();
            dlg.ShowDialog(this);
        }
        private void btSostav_Click(object sender, EventArgs e) {
            Composition compos = new();
            DlgSelSostav dlg = new(txtCCLOTH.Text, compos.GetListMaterials());
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            txtCCLOTH.Text = dlg.GetSostav();
        }
    }
}
