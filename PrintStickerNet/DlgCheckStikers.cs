using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using DbfLib;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker
{
    public partial class DlgCheckStikers : Form {
        //private readonly _Form1 _parent = null;
        private readonly  Dictionary<string, KMINFO> _dicKM_KMINFO = [];
        private string _strSrcPathKM = "";
        private readonly string _strShopPrefix = "";
        private readonly EAN13_TYPE? _ean13Type = null;
        private readonly PRODUCT? _product = null;
        private readonly MarkingPaths _markingPaths = null;
        private readonly MARKINGTYPES _markingTypes;

        public DlgCheckStikers(string strShopPrefix, EAN13_TYPE? ean13Type, PRODUCT? product, MarkingPaths markingPaths, MARKINGTYPES markingTypes) {
            InitializeComponent();
            //_parent = parent;
            _strShopPrefix = strShopPrefix;
            _ean13Type = ean13Type;
            _product = product;
            _markingPaths = markingPaths;
            _markingTypes = markingTypes;
        }
        private void CheckStikers_Load(object sender, EventArgs e) {
            txtShop.Text = _strShopPrefix;
            string strDestPathKM = "";
            Dictionary<string, long> dicNomenclGtin = [];
            if (_product == PRODUCT.PR_SOPUTKA) {
                if (!DbfWrapper.CheckIfFileExist("SoputkaKM.dbf", ref strDestPathKM, ref _strSrcPathKM, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }
                if (!DbfWrapper.GetGtinsSoputka(ref dicNomenclGtin))
                    return;
            }
            else if(_product == PRODUCT.PR_BV) {
                if (!DbfWrapper.CheckIfFileExist("BvKM.dbf", ref strDestPathKM, ref _strSrcPathKM, @"\BV\")) { this.Cursor = Cursors.Default; return; }
                if (!DbfWrapper.GetGtinsBv(ref dicNomenclGtin))
                    return; 
            }            
            if (null != _markingPaths) {
                strDestPathKM = _markingPaths.GetDestPathKM();
                _strSrcPathKM = _markingPaths.GetSrcPathKM();
            }

            if (_markingTypes == MARKINGTYPES.IMPORT_SOPUTKA) {
                cbNOT_EAN13.Checked = true;
                cbNOT_EAN13.Enabled = false;
            } else {
                cbNOT_EAN13.Checked = false;
                cbNOT_EAN13.Visible = false;
            }

            //int BARCODE = 3;
            //int GTIN = 27;
            int nCodeOut = -1;           
            int N = 0;
            int GTIN2 = 3;
            int KM = 5;
            int STATUS = 9;
            int BARCODE2 = 10;

            DataTable dtTableKM = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut);
            for (int i = 0; i < dtTableKM.Rows.Count; i++) {
                //if (System.DBNull.Value != dtTableKM.Rows[i][STATUS])
                //    continue;
                long lGTIN = (long)dtTableKM.Rows[i][GTIN2];
                string strKM = dtTableKM.Rows[i][KM].ToString();

                char[] chars = [(char)29];//{ (char)29};
                string strDel = new(chars);

                string strBARCODE = dtTableKM.Rows[i][BARCODE2].ToString();
                if (_ean13Type == EAN13_TYPE.GTIN)
                    strBARCODE = lGTIN.ToString();

                string strStatus = "";
                if (System.DBNull.Value != dtTableKM.Rows[i][STATUS])
                    strStatus = (string)dtTableKM.Rows[i][STATUS];

                strKM = strKM.Replace(strDel, string.Empty);
                _dicKM_KMINFO.Add(strKM, new KMINFO((int)dtTableKM.Rows[i][N], strBARCODE, strStatus));//.Substring(0, 31)
            }
            txtRead.Focus();
        }

        private void button1_Click(object sender, EventArgs e) {
            Close();
        }
        private void _AddStickerToShop() {
            if ("" == txtShop.Text)
                label4.Text = "Сброшено стикеров";

            string strQR = txtQR.Text;//.Substring(0,31);                      
            string strBarcode = txtBarcode.Text;
            if (!_dicKM_KMINFO.ContainsKey(strQR)) {
                Console.Beep();
                MessageBox.Show(this, "Ошибка, неизвестный QR код:" + strQR, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            KMINFO kmi = _dicKM_KMINFO[strQR];
            if (kmi.strBARCODE != strBarcode) {
                Console.Beep();
                MessageBox.Show(this, "Ошибка, штрихкод не от этого QR кода", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if ("" != kmi.strSTATUS && !cbResetStiker.Checked) {
                Console.Beep();
                MessageBox.Show(this, "Ошибка, этот стикер уже добавляли", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nLenStatus = 36;
            DateTime dtOpenedFile = Win32.GetLastWriteTime(_strSrcPathKM);
            if (!Dbf.SetValue(_strSrcPathKM, "№", kmi.N.ToString(), "STATUS", txtShop.Text.PadRight(nLenStatus), dtOpenedFile)) {
                Console.Beep();
                MessageBox.Show("Ошибка записи в файл: " + _strSrcPathKM, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Trace.WriteLine("Ошибка записи в файл: " + _strSrcPathKM);
                return;
            }
            _dicKM_KMINFO[strQR].strSTATUS = txtShop.Text;
            //
            dtOpenedFile = Win32.GetLastWriteTime(_strSrcPathKM);
            int nLenKM = 100;
            if (!Dbf.SetValue(_strSrcPathKM, "№", kmi.N.ToString(), "KM", strQR.PadRight(nLenKM), dtOpenedFile)) {
                Console.Beep();
                MessageBox.Show("Ошибка записи в файл: " + _strSrcPathKM, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Trace.WriteLine("Ошибка записи в файл: " + _strSrcPathKM);
                return;
            }
            //
            listBox1.Items.Add(kmi.strBARCODE + " " + strQR);
            lbCount.Text = listBox1.Items.Count.ToString();
            Trace.WriteLine("В магазин:" + txtShop.Text + " отправлен QR:" + strQR + ", BARCODE:" + kmi.strBARCODE);
        }
        private void txtRead_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode != Keys.Enter)
                return;

            if ("" != txtBarcode.Text && "" != txtQR.Text) {
                MessageBox.Show(this, "Ошибка, перед чтением долно быть пусто в полях:" + label1.Text + " и " + label2.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRead.Text = "";
                return;
            }
            if (13 == txtRead.Text.Length && "" == txtBarcode.Text && "" == txtQR.Text) {
                txtBarcode.Text = txtRead.Text;
                txtRead.Text = "";
                return;
            }
            if (cbNOT_EAN13.Checked && txtRead.Text.Length > 80)
                txtBarcode.Text = txtRead.Text.Substring(3, 13);

            if (txtRead.Text.Length > 13 && "" == txtBarcode.Text && "" == txtQR.Text) {
                txtQR.Text = txtRead.Text;
                txtRead.Text = "";
                return;
            }

            if (13 == txtRead.Text.Length && "" == txtBarcode.Text) {
                txtBarcode.Text = txtRead.Text;
                _AddStickerToShop();
                txtRead.Text = "";
                txtBarcode.Text = "";
                txtQR.Text = "";
                return;
            }
            if (txtRead.Text.Length > 13 && "" == txtQR.Text) {
                txtQR.Text = txtRead.Text;
                _AddStickerToShop();
                txtRead.Text = "";
                txtBarcode.Text = "";
                txtQR.Text = "";
                return;
            }
            txtRead.Text = "";
            txtBarcode.Text = "";
            txtQR.Text = "";
        }
        private void cbResetStiker_CheckedChanged(object sender, EventArgs e) {
            txtShop.Text = "";
            txtRead.Focus();
        }

        private void btExit_Click(object sender, EventArgs e) {
            if (DialogResult.Yes != MessageBox.Show("Закрыть программу?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                return;
            Application.Exit();
        }
    }
    class KMINFO(int nN, string BARCODE, string STATUS) {
        public int N = nN;
        public string strBARCODE = BARCODE;
        public string strSTATUS = STATUS;
    }
}
