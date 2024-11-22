using DbfLib;
using System.Data;
using System.Diagnostics;
using PrintSticker.MarkingObjectsBase;
using System.ComponentModel;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым
#pragma warning disable IDE0057 //Substring можно упростить
#pragma warning disable CA1854 //Предпочитать вызов TryGetValue доступу к индексатору словаря

namespace PrintSticker {
    internal class MarkingSP() : Marking() {

        private readonly ColorSP _colorSP = new();
       
        protected override bool _InitPaths() {
            if (null != _markingPaths)
                return _markingPaths.UpdateLocalFiles();
            _markingPaths = new(_strSettingsID, _MARKINGTYPES);
            if (!_markingPaths.AllFilesExists())
                return false;
            return true;
        }
        protected override COLUMNS_PRODUCTS _GetCollumnsPRD() {
            return new() {
                BARCODE = 3,
                KOL = 11,
                GTIN = 27,
                KOL_KM = 28,
                IZDNAME = 7
            };
        }
        
        protected virtual string GetCvetSP(string strCvet) {
            if (!int.TryParse(strCvet, out _))
                return strCvet;
            int nColor = Convert.ToInt32(strCvet);

            return _colorSP.GetNameColor(nColor);            
        }
      
        protected override string GetNomencl(DataRow row) {
            int MOD = 7;
            int ART = 8;
            int RAZ = 9;
            int ART2 = 14;//Country
            int PATTERN = 15;
            int CCODE = 16;
            int CCLOTH = 17;

            string strMod = row[MOD].ToString();
            string strART = row[ART].ToString();
            string strRAZ = row[RAZ].ToString();
            string strPATTERN = row[PATTERN].ToString();
            string strCCLOTH = row[CCLOTH].ToString();
            string strART2 = row[ART2].ToString();
            string strCCODE = row[CCODE].ToString();

            string strRazmer = "";
            if ("" != strRAZ)
                strRazmer = " размер " + strRAZ;
            string strSostav = "";
            if ("" != _GetSostav(strCCLOTH))
                strSostav = " состав " + _GetSostav(strCCLOTH);
            string strCvet = "";
            if ("" != strCCODE)
                strCvet = " цвет " + strCCODE;
            string strRis = "";
            if ("" != strPATTERN)
                strRis = " рис." + strPATTERN;
            return strMod + " арт." + strART + strRazmer + strSostav + strCvet + strRis + " страна " + strART2;
        }
        public override string GetSettingsID() {
            return _strSettingsID;
        }
        protected override bool _EditProduct(ref DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count)
                return false;
            DlgAddToTableSoputka dlg = new();
            dlg.SetEditDataTableRow(dt);
            if (DialogResult.OK != dlg.ShowDialog())
                return false;
            return true;
        }

        public override bool AddNewProduct(Form parent, string strOrderID) {
            DlgAddToTableSoputka dlg = new();
            if (DialogResult.OK != dlg.ShowDialog(parent))
                return false;
            dlg.GetSelItems(out List<RestItem> listItems);
            if (0 == listItems.Count) return false;

            if (null == _markingPaths) return false;
            
            string strDestPathRez = _markingPaths.GetDestPath();
            string strSrcPathRez = _markingPaths.GetSrcPath();

            int nCodeOut = -1;
            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);

            int INO = 2;
            dtTableRez.Rows.Clear();
            foreach (RestItem item in listItems) {
                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
                rowAdd[INO] = strOrderID;
                rowAdd[RESTCOLID.IZD] = item.GetIzdByMOD();
                rowAdd[RESTCOLID.MOD] = item.GetMOD();
                rowAdd[RESTCOLID.ART] = item.GetART();
                rowAdd[RESTCOLID.RAZ] = item.GetRAZ();
                rowAdd[RESTCOLID.KOL] = item.GetKOL();
                rowAdd[RESTCOLID.CJ] = 0.0;
                rowAdd[RESTCOLID.ART2] = item.GetART2();
                rowAdd[RESTCOLID.PATTERN] = item.GetPATTERN();
                rowAdd[RESTCOLID.CCODE] = item.GetCCODE();
                rowAdd[RESTCOLID.CCLOTH] = item.GetCCLOTH();
                rowAdd[RESTCOLID.COTHER] = item.GetCOTHER();
                rowAdd[RESTCOLID.CSEASON] = item.GetCSEASON();
                rowAdd[RESTCOLID.CJ2] = 0.0;
                rowAdd[RESTCOLID.CR] = 0.0;
                rowAdd[RESTCOLID.GTIN] = 0;
                rowAdd[RESTCOLID.SM] = 0.0;
                rowAdd[RESTCOLID.KOL_KM] = 0;
                rowAdd[RESTCOLID.KOL_PRN] = 0;
                rowAdd[RESTCOLID.KOL_TOSHOP] = 0;

                dtTableRez.Rows.Add(rowAdd);
            }
            if (!Dbf.AddTable(strDestPathRez, dtTableRez)) {
                MessageBox.Show("Ошибка добавления в файл: " + strDestPathRez, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestPathRez, strSrcPathRez, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public override bool ShowPage(_Form1 parent,DataGridView dataGridView, GridViewExtensions.DataGridFilterExtender dataGridFilter, string strOrderID = "-1") {
            if (null == parent) return false;
            if (!_InitPaths()) return false;

            _dataGridView = dataGridView;
            _hNumTable = [];
            _dtOpenedFile = Win32.GetLastWriteTime(_markingPaths.GetSrcPath());
            _dtGtinOrdersMkLoaded = _GetLastWriteTime(_markingPaths.GetSrcPathOrdersKM());

            string strDest = "";
            string strSrc = "";
            if (!DbfWrapper.CheckIfFileExist(_markingPaths.GetProductsFileName(), ref strDest, ref strSrc, _markingPaths.GetGeneratedFolder())) return false;

            LoadDataSoputka(_markingPaths.GetProductsFileName(), dataGridView, dataGridFilter, ref _hNumTable, _markingPaths, strOrderID, _GetShopPrefix());
            parent.UpdateCount(dataGridView);
            return true;
        }
        public void LoadDataSoputka(string strFileName, DataGridView dgv, GridViewExtensions.DataGridFilterExtender dgFE, ref Dictionary<int, int> hNumTableSPO,
            MarkingPaths markingPaths = null, string strOrderID = "", string strShopPrefix = "") {
            _LoadDataSoputka(strFileName, dgv, dgFE, ref hNumTableSPO, PRODUCT.PR_SOPUTKA, markingPaths, strOrderID, strShopPrefix);
        }
        private void _LoadDataSoputka(string strFileName, DataGridView dgv, GridViewExtensions.DataGridFilterExtender dgFE, ref Dictionary<int, int> hNumTableSPO,
            PRODUCT product = PRODUCT.PR_SOPUTKA, MarkingPaths markingPaths = null, string strOrderID = "", string strShopPrefix = "") {
            string strDestPathSoputka = "";
            string strSrcPathSoputka = "";

            if (null == markingPaths) {
                if (PRODUCT.PR_SOPUTKA == product) {
                    if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, @"\Soputka\generated\")) { return; }
                }
            }
            string strDestPathOrdersKM = "";
            string strSrcPathOrdersKM = "";
            if (null == markingPaths) {
                if (PRODUCT.PR_SOPUTKA == product) {
                    if (!DbfWrapper.CheckIfFileExist("SoputkaOrdersKM.dbf", ref strDestPathOrdersKM, ref strSrcPathOrdersKM, @"\Soputka\")) { return; }
                }
            }

            //01
            Dictionary<string, int> dicGtinBarcodeShop_Count = [];
            int nIsIndexMDXNestand = -1;
            int nCodeOut = -1;
            int GTIN = 3;
            int STATUS = 9;
            int BARCODE3 = 10;
            string strDestPathKM = "";
            string strSrcPathKM = "";
            if (null == markingPaths) {
                if (PRODUCT.PR_SOPUTKA == product) {
                    if (!DbfWrapper.CheckIfFileExist("SoputkaKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\Soputka\")) { return; }
                }
            }
            if (null != markingPaths) {
                strDestPathSoputka = markingPaths.GetDestPath();
                strSrcPathSoputka = markingPaths.GetSrcPath();
                strDestPathOrdersKM = markingPaths.GetDestPathOrdersKM();
                strSrcPathOrdersKM = markingPaths.GetSrcPathOrdersKM();
                strDestPathKM = markingPaths.GetDestPathKM();
                strSrcPathKM = markingPaths.GetSrcPathKM();
            }
            DataTable dtTableSoputkaKM = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableSoputkaKM.Rows.Count; i++) {
                if (System.DBNull.Value == dtTableSoputkaKM.Rows[i][STATUS])
                    continue;
                string strStatus = (string)dtTableSoputkaKM.Rows[i][STATUS];
                long lGTIN = (long)dtTableSoputkaKM.Rows[i][GTIN];
                string strBarcode = (string)dtTableSoputkaKM.Rows[i][BARCODE3];

                string strKey = lGTIN.ToString() + "_" + strBarcode + "_" + strStatus;
                if (!dicGtinBarcodeShop_Count.ContainsKey(strKey))
                    dicGtinBarcodeShop_Count.Add(strKey, 1);
                else
                    dicGtinBarcodeShop_Count[strKey]++;

                if ("SGP" == strStatus)
                    continue;
                strKey = lGTIN.ToString() + "_" + strBarcode + "_ALL";
                if (!dicGtinBarcodeShop_Count.ContainsKey(strKey))
                    dicGtinBarcodeShop_Count.Add(strKey, 1);
                else
                    dicGtinBarcodeShop_Count[strKey]++;
            }
            //_

            Dictionary<string, long> dicNomenclGtin = [];
            if (!DbfWrapper.GetGtinsSoputka(ref dicNomenclGtin, markingPaths))
                return;

            dgFE.DataGridView = null;

            Dictionary<string, KMCOUNT> hGtin_Barcode_CountKM = [];

            Dictionary<string, string> hUseColumns = [];
            GetUseColumns(ref hUseColumns);//переделать          

            //_dtSoputkaGtinOrdersMkLoaded = _GetLastWriteTime(strSrcPathOrdersKM);

            DataTable dtTableSPO = null;
            DataTable dtTableOrdersKM = null;
            BackgroundWorker bw = new() {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };

            //int GTIN = 3;
            int KOL_KM_ALL = 6;
            int KOL_KM_PRN = 7;
            int BARCODE = 9;
            bw.DoWork += delegate (object sender2, DoWorkEventArgs e2) {
                bw.ReportProgress(0);
                dtTableOrdersKM = Dbf.LoadDbfWithAddColumns(strDestPathOrdersKM, out nIsIndexMDXNestand, ref nCodeOut, "DEL", "0");
                for (int i = 0; i < dtTableOrdersKM.Rows.Count; i++) {
                    if (System.DBNull.Value == dtTableOrdersKM.Rows[i][GTIN])
                        continue;
                    string strGTIN_Barcode = dtTableOrdersKM.Rows[i][GTIN].ToString() + "_" + dtTableOrdersKM.Rows[i][BARCODE].ToString();
                    int nCount = 0;
                    if (System.DBNull.Value != dtTableOrdersKM.Rows[i][KOL_KM_ALL])
                        nCount = (int)dtTableOrdersKM.Rows[i][KOL_KM_ALL];
                    int nCountPrn = 0;
                    if (System.DBNull.Value != dtTableOrdersKM.Rows[i][KOL_KM_PRN])
                        nCountPrn = (int)dtTableOrdersKM.Rows[i][KOL_KM_PRN];

                    if (!hGtin_Barcode_CountKM.ContainsKey(strGTIN_Barcode))
                        hGtin_Barcode_CountKM.Add(strGTIN_Barcode, new KMCOUNT(nCount, nCountPrn));
                    else {
                        hGtin_Barcode_CountKM[strGTIN_Barcode].nKOL_KM_ALL += nCount;
                        hGtin_Barcode_CountKM[strGTIN_Barcode].nKOL_KM_PRN += nCountPrn;
                    }
                }
                if ("" == strOrderID)
                    dtTableSPO = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "DEL", "0", hUseColumns);
                else
                    dtTableSPO = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "INO", strOrderID, hUseColumns, false, false);

                bw.ReportProgress(100);
            };
            DlgReportProgress dlg = new(bw, "Извлечение данных");
            dlg.ShowDialog();
            bool bPRINT_USER_SOPUTKA = false;
            if (bPRINT_USER_SOPUTKA && strFileName != "SoputkaRestShopsToStikers_SGP.dbf") {
                int ART = 8;
                for (int i = dtTableSPO.Rows.Count - 1; i >= 0; i--) {
                    if (System.DBNull.Value == dtTableSPO.Rows[i][ART]) {
                        dtTableSPO.Rows.Remove(dtTableSPO.Rows[i]);
                        continue;
                    }
                    string strArt = ((string)dtTableSPO.Rows[i][ART]).Trim();
                    //if (-1 == strArt.LastIndexOf("J.POKER"))
                    if (-1 == strArt.LastIndexOf("JASPER"))
                        dtTableSPO.Rows.Remove(dtTableSPO.Rows[i]);
                }
            }
            //this.Cursor = Cursors.WaitCursor;

            DataView dv = dtTableSPO.DefaultView;
            //dv.Sort = "Перед.СПО desc, № desc";
            dgv.DataSource = dv.ToTable();

            dgFE.DataGridView = dgv;////

            int BARCODE2 = 3;
            int GTIN2 = 27;
            int KOL_KM = 28;
            int KOL_PRN = 29;
            int KOL_TOSHOP = 30;
            hNumTableSPO = [];
            for (int i = 0; i < dgv.Rows.Count; i++) {
                hNumTableSPO.Add(1 + i, (int)dgv.Rows[i].Cells[0].Value);
                dgv.Rows[i].Cells[0].Value = 1 + i;

                //if (System.DBNull.Value == dgv.Rows[i].Cells[GTIN2].Value ||
                //    "0" == dgv.Rows[i].Cells[GTIN2].Value.ToString()) {

                DataRow row = ((DataRowView)dgv.Rows[i].DataBoundItem).Row;
                string strNomencl = GetNomencl(row);
                if (dicNomenclGtin.ContainsKey(strNomencl))
                    dgv.Rows[i].Cells[GTIN2].Value = dicNomenclGtin[strNomencl];
                else
                    dgv.Rows[i].Cells[GTIN2].Value = "0";
                //}
                if (System.DBNull.Value != dgv.Rows[i].Cells[GTIN2].Value) {
                    string strGTIN_Barcode = dgv.Rows[i].Cells[GTIN2].Value.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString();
                    if (hGtin_Barcode_CountKM.ContainsKey(strGTIN_Barcode)) {
                        dgv.Rows[i].Cells[KOL_KM].Value = hGtin_Barcode_CountKM[strGTIN_Barcode].nKOL_KM_ALL;
                        dgv.Rows[i].Cells[KOL_PRN].Value = hGtin_Barcode_CountKM[strGTIN_Barcode].nKOL_KM_PRN;
                    }
                }
                if (System.DBNull.Value != dgv.Rows[i].Cells[GTIN2].Value) {
                    long lGTIN = (long)dgv.Rows[i].Cells[GTIN2].Value;
                    string strKey = "";
                    //string strKey = lGTIN.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString() + "_" + "000";//_GetShopPrefix(PRODUCT.PR_SOPUTKA)
                    if ("" != strShopPrefix)
                        strKey = lGTIN.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString() + "_" + strShopPrefix;
                    if (!dicGtinBarcodeShop_Count.ContainsKey(strKey)) {
                        strKey = lGTIN.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString() + "_SGP";
                        if (!dicGtinBarcodeShop_Count.ContainsKey(strKey))
                            continue;
                        dgv.Rows[i].Cells[KOL_TOSHOP].Value = dicGtinBarcodeShop_Count[strKey].ToString();//SGP
                        continue;
                    }
                    dgv.Rows[i].Cells[KOL_TOSHOP].Value = dicGtinBarcodeShop_Count[strKey].ToString();
                }
            }
            InidataGridViewSoputka(ref dgv);
            //hGtin_CountKM
            //this.Cursor = Cursors.Default;
        }
        public static void GetUseColumns(ref Dictionary<string, string> hUseColumns) {
            hUseColumns.Add("№", "№");
            hUseColumns.Add("DEL", "DEL");
            hUseColumns.Add("INO", "INO");
            hUseColumns.Add("BARCODE", "Штрихкод");
            hUseColumns.Add("IZD", "IZD");
            hUseColumns.Add("KOD_P", "KOD_P");

            hUseColumns.Add("PRS", "PRS");
            hUseColumns.Add("MOD", "Наим. Изделия");
            hUseColumns.Add("ART", "Артикул");
            hUseColumns.Add("RAZ", "Размер");
            hUseColumns.Add("SRT", "SRT");

            hUseColumns.Add("KOL", "Количество");
            hUseColumns.Add("CJ", "CJ");
            hUseColumns.Add("SM", "SM");
            hUseColumns.Add("ART2", "Производитель");
            hUseColumns.Add("PATTERN", "Рисунок");

            hUseColumns.Add("CCODE", "Цвет");
            hUseColumns.Add("CCLOTH", "Состав");
            hUseColumns.Add("COTHER", "Поставщик");
            hUseColumns.Add("CSEASON", "CSEASON");
            hUseColumns.Add("CJ2", "CJ2");

            hUseColumns.Add("NLIST", "NLIST");
            hUseColumns.Add("DAT1", "DAT1");
            hUseColumns.Add("MAG", "MAG");
            hUseColumns.Add("MAG1", "MAG1");
            hUseColumns.Add("TTN", "TTN");

            hUseColumns.Add("CR", "CR");
            hUseColumns.Add("GTIN", "GTIN");
            hUseColumns.Add("KOL_KM", "Всего КМ запрошено в ЧЗ");
            hUseColumns.Add("KOL_PRN", "Получено КМ из ЧЗ");
            hUseColumns.Add("KOL_TOSHOP", "Распечатано КМ для магазинов");
        }


        public static void InidataGridViewSoputka(ref DataGridView dataGridView) {
            dataGridView.Columns[0].Visible = false;//№"
            dataGridView.Columns[1].Visible = false;//DEL
            dataGridView.Columns[2].Visible = false;//
            dataGridView.Columns[4].Visible = false;//IZD
            dataGridView.Columns[5].Visible = false;//KOD_P
            dataGridView.Columns[6].Visible = false;//PRS
            dataGridView.Columns[10].Visible = false;//SRT            
            dataGridView.Columns[12].Visible = false;//CJ
            dataGridView.Columns[13].Visible = false;//SM
            //dataGridView.Columns[14].Visible = false;//ART2            
            //dataGridView.Columns[19].Visible = false;//CSEASON
            dataGridView.Columns[20].Visible = false;//CJ2
            dataGridView.Columns[21].Visible = false;//NLIST
            dataGridView.Columns[22].Visible = false;//DAT1
            dataGridView.Columns[23].Visible = false;//MAG
            dataGridView.Columns[24].Visible = false;//MAG1
            dataGridView.Columns[25].Visible = false;//TTN
            dataGridView.Columns[26].Visible = false;//CR

            dataGridView.Columns[3].Width = 80;//BARCODE
            dataGridView.Columns[8].Width = 270;//ART
            dataGridView.Columns[9].Width = 80;//RAZ
            dataGridView.Columns[11].Width = 50;//KOL

            dataGridView.Columns[14].Width = 50;//ART2
            dataGridView.Columns[15].Width = 50;//PATTERN
            dataGridView.Columns[16].Width = 50;//CCODE
            dataGridView.Columns[17].Width = 50;//CCLOTH

            dataGridView.Columns[18].Width = 50;//COTHER

            dataGridView.Columns[27].Width = 110;//GTIN 
            dataGridView.Columns[28].Width = 60;//KOL_KM
            dataGridView.Columns[29].Width = 65;//KOL_PRN

            DataGridViewCellStyle style = new() {
                Font = new Font(dataGridView.Font, FontStyle.Bold)
            };
            dataGridView.DefaultCellStyle = style;
            //dataGridView.Columns[2].DisplayIndex = 1;//
        }
        protected override string _GetCountryFileName() {
            return "DbCountriesSP.dbf";
        }
        protected override string _GetProductFileName() {
            return "DbProductSP.dbf";
        }
        public override bool ExportToExcel(_Form1 parent) {
            if (null == _markingPaths || null == _dataGridView || 0 == _dataGridView.SelectedRows.Count) return false;
            if (null == parent) return false;

            string strDestPathToGTIN = "";
            string strSrcPathToGTIN = "";
            if (!DbfWrapper.CheckIfFileExist("toGTIN.xlsx", ref strDestPathToGTIN, ref strSrcPathToGTIN, @"\Soputka\patern\")) { return false; }

            string strNewDir = @"C:\Po_BOLSHEVICHKA\PrintSticker\ToGS1\";
            Directory.CreateDirectory(strNewDir);
            string strNewPath = strNewDir + "toGTIN.xlsx";
            File.Copy(strDestPathToGTIN, strNewPath, true);
            strDestPathToGTIN = strNewPath;

            HashSet<string> hProducts = [];
            _GetSelProducts(ref hProducts);

            HashSet<string> hNomenclWithGtin = [];
            _GetNomenclWithGtin(ref hNomenclWithGtin);

            List<ToExcelItem> listToExcelItems = [];
            foreach (string strProdict in hProducts) {
                if (hNomenclWithGtin.Contains(strProdict))
                    continue;
                int nPos0 = strProdict.IndexOf(" арт.");
                int nPos1 = strProdict.IndexOf(" размер ");
                int nPos2 = strProdict.IndexOf(" состав ");
                int nPos3 = strProdict.IndexOf(" цвет ");
                int nPos31 = strProdict.IndexOf(" рис.");
                int nPos4 = strProdict.IndexOf(" страна ");

                ToExcelItem tei = new() {
                    //product = .PR_MTM_REMAINS_SOPUTKA,
                    strColumn5_ProductName = strProdict
                };

                int nPos5 = strProdict.Length;
                int nPos0Next = nPos1;
                if (-1 == nPos0Next) nPos0Next = nPos2;
                if (-1 == nPos0Next) nPos0Next = nPos3;
                if (-1 == nPos0Next) nPos0Next = nPos31;
                if (-1 == nPos0Next) nPos0Next = nPos4;
                if (-1 == nPos0Next) nPos0Next = nPos5;

                int nPos1Next = nPos2;
                if (-1 == nPos1Next) nPos1Next = nPos3;
                if (-1 == nPos1Next) nPos1Next = nPos31;
                if (-1 == nPos1Next) nPos1Next = nPos4;
                if (-1 == nPos1Next) nPos1Next = nPos5;

                int nPos2Next = nPos3;
                if (-1 == nPos2Next) nPos2Next = nPos31;
                if (-1 == nPos2Next) nPos2Next = nPos4;
                if (-1 == nPos2Next) nPos2Next = nPos5;

                int nPos3Next = nPos31;
                if (-1 == nPos3Next) nPos2Next = nPos4;
                if (-1 == nPos3Next) nPos2Next = nPos5;

                string strRazmer = "ОТСУТСТВУЕТ";
                if (-1 != nPos1) strRazmer = strProdict.Substring(nPos1 + " размер ".Length, nPos1Next - nPos1 - " размер ".Length);

                string strSostav = "ОТСУТСТВУЕТ";
                if (-1 != nPos2) strSostav = strProdict.Substring(nPos2 + " состав ".Length, nPos2Next - nPos2 - " состав ".Length);

                string strCvet = "РАЗНОЦВЕТНЫЙ";
                if (-1 != nPos3) strCvet = strProdict.Substring(nPos3 + " цвет ".Length, nPos3Next - nPos3 - " цвет ".Length);

                string strCountry = "";
                if (-1 != nPos4) strCountry = strProdict.Substring(nPos4 + " страна ".Length);

                tei.strColumn19_Sostav = _GetSostav(strSostav);

                string strTypeProduct = strProdict[..nPos0];

                //string strMod1 = strProdict.Substring(nPosA + " мод.".Length, nPosANext - nPosA - " мод.".Length);
                //string strMod2 = strProdict.Substring(nPos0 + " арт.".Length, nPos0Next - nPos0 - " арт.".Length);

                tei.strColumn17_Mod = strProdict.Substring(nPos0 + " арт.".Length, nPos0Next - nPos0 - " арт.".Length);

                //tei.strColumn17_Mod = strMod1 + " " + strMod2;

                tei.strColumn7_Country = _GetCountry(strCountry);

                //tei.strColumn8_INN = "7708815300";
                //tei.strColumn9_IZGOTOVITEL = "ООО 'ВЕСПЕР ТРЕЙДИНГ'";

                tei.strColumn8_INN = "";
                tei.strColumn9_IZGOTOVITEL = "";


                tei.strColumn10_SortProduct = _GetTypeProduct(strTypeProduct);
                tei.strColumn12_TNVED = _GetTNVED(strTypeProduct, strSostav, tei.strColumn17_Mod);
                tei.strColumn14_Razmer = strRazmer;

                int nPos6 = strRazmer.LastIndexOf(')');
                if (-1 != nPos6)
                    tei.strColumn15_Rost = strRazmer.Substring(1, nPos6 - 1);

                tei.strColumn16_Cvet = GetCvetSP(strCvet);

                int nPos = tei.strColumn5_ProductName.LastIndexOf("КОСТЮМ");
                if (-1 != nPos) {
                    tei.strColumn26_Komplect = "Да";
                    tei.strColumn27_CountInKomplect = "2";
                    tei.strColumn28_DescriptionKomplect = "ПИДЖАК (1шт.); БРЮКИ (1шт.)";
                }
                nPos = tei.strColumn5_ProductName.LastIndexOf("КОСТЮМ ЖЕНСКИЙ С ЮБКОЙ");
                if (-1 != nPos) {
                    tei.strColumn26_Komplect = "Да";
                    tei.strColumn27_CountInKomplect = "2";
                    tei.strColumn28_DescriptionKomplect = "ПИДЖАК (1шт.); ЮБКА (1шт.)";
                }
                listToExcelItems.Add(tei);
            }
            if (0 == listToExcelItems.Count) {
                MessageBox.Show(parent, "Для всех выбранных элементов уже ранее были получены GTIN", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            ExcelExport excExp = new();
            if (!excExp.Open(strDestPathToGTIN, out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(parent, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!excExp.ExportData(xlsWorkSheet, ref listToExcelItems)) {
                MessageBox.Show(parent, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MessageBox.Show(parent, "Успешно сгенерирован файл:" + strDestPathToGTIN, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        //bool bMultySize,
        private bool _PrintRow(PrintParms pp, DataRow row, string strDestPathKM, string strDestPathPaternSP,
    ref int nPrintedAll, List<KM> listKM = null, bool bLastRow = false) {
            return _PrintRowSoputka(pp, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, bLastRow);
        }
        //bool bMultySizeIn,
        public override void Print(PrintParms pp, int nStcker = -1,
             string strBarcodePrint = "", string strCodeSpm = "", bool bFlabelToBarcode = false, bool bWithoutKM = false) {

            if (null == _markingPaths || null == _dataGridView) return;
            //if (null == _parent) return;

            if ("" == strCodeSpm.Trim())
                strCodeSpm = _GetShopPrefix();

            string strDestPathKM = _markingPaths.GetDestPathKM();
            string strDestPathPaternSP = "";
            string strSrcPathPaternSP = "";


            DataGridView dgv = _dataGridView;

            if (!DbfWrapper.CheckIfFileExist(GetZplName(), ref strDestPathPaternSP, ref strSrcPathPaternSP, @"\1c\Matrix\patern\")) return;
            string strRootFolder = _markingPaths.GetGeneratedFolder();

            int nPrintedAll = 0;
            int nCodeOut = -1;
            int N = 0;
            int GTIN2 = 3;
            int KM = 5;
            int STATUS = 9;
            int BARCODE = 10;

            if (nStcker != -1) {
                DataTable dtTableSoputkaKM0 = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "№", nStcker.ToString());
                if (System.DBNull.Value == dtTableSoputkaKM0.Rows[0][KM])
                    return;
                if (System.DBNull.Value == dtTableSoputkaKM0.Rows[0][STATUS])
                    return;
                if (System.DBNull.Value == dtTableSoputkaKM0.Rows[0][BARCODE])
                    return;
                if (System.DBNull.Value == dtTableSoputkaKM0.Rows[0][GTIN2])
                    return;

                string strKM = dtTableSoputkaKM0.Rows[0][KM].ToString();
                //string strStatus = dtTableSoputkaKM0.Rows[0][STATUS].ToString();
                string strBarcode = dtTableSoputkaKM0.Rows[0][BARCODE].ToString();
                string strGTIN = dtTableSoputkaKM0.Rows[0][GTIN2].ToString();

                string strDestPathSoputka = "";
                string strSrcPathSoputka = "";
                string strFileName = _markingPaths.GetProductsFileName();// "RestShopsToStikersSoputks_020.dbf";

                if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, strRootFolder)) return;

                DataTable dtTableSoputka = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "BARCODE", strBarcode);
                if (0 == dtTableSoputka.Rows.Count) {
                    MessageBox.Show(null, "Нет штрихкода: " + strBarcode + " В файле: " + strDestPathSoputka, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                List<KM> listKM = [];
                listKM.Add(new KM(nStcker, strKM));

                int GTIN = 23;
                dtTableSoputka.Rows[0][GTIN] = strGTIN;

                _PrintRow(pp, dtTableSoputka.Rows[0], strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM);//, false, false);
            } else if (strBarcodePrint != "") {
                int GTIN0 = 3;
                DataTable dtTableSoputkaKM0 = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "BARCODE", strBarcodePrint);
                if (0 == dtTableSoputkaKM0.Rows.Count)
                    return;
                try {
                    foreach (DataRow row in dtTableSoputkaKM0.Rows) {
                        if (System.DBNull.Value == row[KM])
                            continue;
                        if (System.DBNull.Value == row[STATUS])
                            continue;
                        if (System.DBNull.Value == row[GTIN0])
                            continue;
                        if (strCodeSpm != row[STATUS].ToString())
                            continue;

                        string strKM = row[KM].ToString();
                        string strDestPathSoputka = "";
                        string strSrcPathSoputka = "";
                        string strPrefixFileWithData = _markingPaths.GetProductsFileName().Split('_')[0] + "_";//"RestShopsToStikersSoputks_";
                        string strFileName = strPrefixFileWithData + strCodeSpm + ".dbf";
                        if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, strRootFolder)) return;

                        DataTable dtTableSoputka = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "BARCODE", strBarcodePrint);
                        if (1 != dtTableSoputka.Rows.Count) {
                            MessageBox.Show(null, "не равно 1 количество Штрихкода: " + strBarcodePrint + " В файле: " + strDestPathSoputka, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        List<KM> listKM = [];
                        listKM.Add(new KM((int)row[N], strKM));
                        int GTIN = 23;
                        string strGTIN = row[GTIN0].ToString();
                        dtTableSoputka.Rows[0][GTIN] = strGTIN;
                        _PrintRow(pp, dtTableSoputka.Rows[0], strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM);
                    }
                } catch (Exception) {
                    return;
                }

            } else {
                bool bLastRow = false;
                int nPos = 0;
                try {
                    foreach (DataGridViewRow gvRow in dgv.SelectedRows) {
                        nPos++;
                        if (nPos == dgv.SelectedRows.Count)
                            bLastRow = true;
                        DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                        if (bWithoutKM) {
                            List<KM> listKM = [];
                            listKM.Add(new KM(0, ""));
                            if (!_PrintRow(pp, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, bLastRow))
                                return;
                        } else {
                            if (!_PrintRow(pp, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, null, bLastRow))
                                return;
                        }
                    }
                } catch (Exception) {
                    return;
                }
            }
        }

        protected bool _PrintRowSoputka(PrintParms pp, DataRow row, string strDestPathKM, string strDestPathPaternSP, ref int nPrintedAll, List<KM> listKM = null, bool bLastRow = false) {

            if (null == pp) {
                pp = new() { bMultySize = true, strDate = "03 2024", strPrefix = "" };
            }

            int BARCODE = 3;
            int MOD = 7;
            int ART = 8;
            int RAZ = 9;
            int KOL = 11;
            int ART2 = 14;
            int PATTERN = 15;
            int CCODE = 16;
            int CCLOTH = 17;
            int CSEASON = 19;
            int GTIN = 27;            
            int STATUS = 9;
            int KM_KM = 5;
            int N = 0;                       
            int nCodeOut = -1;

            string strBARCODE = row[BARCODE].ToString();
            string strMOD = row[MOD].ToString();
            string strART = row[ART].ToString();
            string strART_second = "";
            int nLenLine = 18;//17; 18; 20;
            int nPosG = strART.ToUpper().LastIndexOf("ЖЕН");
            if (strART.Length > nLenLine && -1 == nPosG) {
                strART_second = strART.Substring(nLenLine);
                strART = strART[..nLenLine];
            }
            if (strART.Length > nLenLine && -1 != nPosG) {
                int nPosPr = strART.LastIndexOf(" ");
                string strPostfix = "";
                if (-1 != nPosPr) {
                    strPostfix = strART.Substring(nPosPr);
                    strART = strART[..nPosPr];
                    strART_second = strPostfix.Trim();
                }
                if (strART.Length > nLenLine) {
                    strART_second = strART.Substring(nLenLine) + strPostfix;
                    strART = strART.Substring(0, nLenLine);
                }
            }
            strART_second = strART_second.Trim();
            string strRAZ = row[RAZ].ToString();// +"56(170-112-120)";           

            string strGTIN = row[GTIN].ToString();
            string strART2 = row[ART2].ToString();
            //strART2 = strART2.Replace("ПОРТУГАЛИЯ", "ПОРТУГАЛ.");

            //if (product == .PR_WT_REMAINS_SOPUTKA) {
            string strTmp1 = strART2;
            string strTmp2 = strART2.ToLower();
            strART2 = strTmp1.Substring(0, 1) + strTmp2.Substring(1);
            //}


            string strCCODE = row[CCODE].ToString();//цвет           
            string strCCLOTH = row[CCLOTH].ToString();//состав

            string[] parmsCL = strCCLOTH.Split('|');
            string strCCLOTH1 = strCCLOTH;
            string strCCLOTH2 = "";
            if (2 == parmsCL.Length) {
                strCCLOTH1 = parmsCL[0];
                strCCLOTH2 = parmsCL[1];
            }


            string strCSEASON = row[CSEASON].ToString();
            string strPATTERN = row[PATTERN].ToString();//рисунок                               
            int nKol = (int)row[KOL];
            if (null != listKM)
                nKol = listKM.Count;
            try {
                int nPrinted = 0;
                int nNotKM = 0;
                int BARCODE2 = 10;

                if (null == listKM) {
                    listKM = [];
                    DataTable dtTableSoputkaKM = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "GTIN", strGTIN, null, false, false);
                    for (int i = 0; i < dtTableSoputkaKM.Rows.Count; i++) {
                        if (System.DBNull.Value != dtTableSoputkaKM.Rows[i][BARCODE2] && strBARCODE != dtTableSoputkaKM.Rows[i][BARCODE2].ToString())
                            continue;
                        if (System.DBNull.Value != dtTableSoputkaKM.Rows[i][STATUS]) {
                            nPrinted++;
                            continue;
                        }
                        if (System.DBNull.Value == dtTableSoputkaKM.Rows[i][KM_KM]) {
                            nNotKM++;
                            continue;
                        }
                        int nPos = (int)dtTableSoputkaKM.Rows[i][N];
                        string strKM = dtTableSoputkaKM.Rows[i][KM_KM].ToString();
                        string strStatus = dtTableSoputkaKM.Rows[i][STATUS].ToString();
                        listKM.Add(new KM(nPos, strKM));
                    }
                    if (nPrinted > 0 && 0 == listKM.Count && 0 == nNotKM) {
                        MessageBox.Show("Ошибка, для GTIN: " + strGTIN + " QR коды уже были распечатаны (повторно не печатаем)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                    if (listKM.Count < nKol) {
                        MessageBox.Show("Ошибка, свободных QR кодов меньше чем нужно, для GTIN: " + strGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }

                string strDir = _GetCurDir() + "\\tmp\\";
                Directory.CreateDirectory(strDir);
                string strNewFileName = strDir + "\\" + strBARCODE + ".zpl";

                string strPrefix = _GetShopPrefix();
                if ("" != pp.strPrefix)
                    strPrefix = pp.strPrefix;
                for (int z = 0; z < nKol; z++) {
                    string[] lines = File.ReadAllLines(strDestPathPaternSP, GetStickerEncoding());
                    for (int i = 0; i < lines.Length; i++) {
                        lines[i] = lines[i].Replace("#01", listKM[z].N.ToString("00000"));
                        lines[i] = lines[i].Replace("#02", strPrefix);
                        //lines[i] = lines[i].Replace("#03", txt03.Text);

                        // if (product == .PR_VETEX_IMPORT_SOPUTKA || product == .PR_WT_REMAINS_SOPUTKA || product == .PR_VETEX_REMAINS) {
                        strGTIN = listKM[z].strKN.Substring(3, 13);

                        if (EAN13_TYPE.GTIN == _GetEan13Type())
                            lines[i] = lines[i].Replace("#04", strGTIN);
                        else
                            lines[i] = lines[i].Replace("#04", strBARCODE);

                        lines[i] = lines[i].Replace("#05", strMOD.PadLeft(16));
                        lines[i] = lines[i].Replace("#06", strART);
                        lines[i] = lines[i].Replace("#07", strART_second);
                        //strRAZ = "188-194 50";

                        lines[i] = lines[i].Replace("#08", strRAZ);
                        lines[i] = lines[i].Replace("#09", strART2);
                        //lines[i] = lines[i].Replace("#10", txt10.Text);

                        if(int.TryParse(strCCODE, out int nColor))
                            lines[i] = lines[i].Replace("#11", _colorSP.GetNameShortColor(nColor));

                        //lines[i] = lines[i].Replace("#12", txt12.Text);
                        lines[i] = lines[i].Replace("#13", "");                        
                        lines[i] = lines[i].Replace("#14", _markingPaths.GetCompanyName());

                        lines[i] = lines[i].Replace("#15", "Россия, 107078");
                        lines[i] = lines[i].Replace("#16", "Москва, Каланчевская 17");
                        //lines[i] = lines[i].Replace("#17", txt17.Text);
                        //lines[i] = lines[i].Replace("#18", txt18.Text);
                        lines[i] = lines[i].Replace("#19", listKM[z].strKN);

                        if (85 != listKM[z].strKN.Length) {
                            if (29 != (int)listKM[z].strKN[31] && 29 != (int)listKM[z].strKN[38]) {
                                string strKN = listKM[z].strKN;
                                int nPos1 = 31;
                                string strValue1 = strKN.Substring(0, nPos1);
                                string strValue2 = strKN.Substring(nPos1, 6);
                                string strValue3 = strKN.Substring(nPos1 + 6);
                                strKN = strValue1 + ((char)29).ToString() + strValue2 + ((char)29).ToString() + strValue3;
                                listKM[z].strKN = strKN;
                            } else {
                                MessageBox.Show("Длина QR кода не равна 85: " + listKM[z].strKN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                            //MessageBox.Show("Длина QR кода не равна 85: " + listKM[z].strKN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //return;
                            // ;
                        }

                        //lines[i] = lines[i].Replace("#20", strCCLOTH.Replace('|', ' ').Replace(" ", string.Empty));
                        lines[i] = lines[i].Replace("#20", strCCLOTH1);
                        lines[i] = lines[i].Replace("#28", strCCLOTH2);

                        lines[i] = lines[i].Replace("#21", strPATTERN);

                        string strVirez = "";
                        if (-1 != strCSEASON.LastIndexOf("0102"))
                            strVirez = "Гольф";
                        if (-1 != strCSEASON.LastIndexOf("0103"))
                            strVirez = "V";
                        lines[i] = lines[i].Replace("#25", strVirez);
                        string strSiluet = "";
                        if (-1 != strCSEASON.LastIndexOf("0217"))
                            strSiluet = "Ж";
                        if (-1 != strCSEASON.LastIndexOf("0201"))
                            strSiluet = "Классика";
                        if (-1 != strCSEASON.LastIndexOf("0202"))
                            strSiluet = "Слим";
                        lines[i] = lines[i].Replace("#26", strSiluet);

                        string strRukava = "";
                        if (-1 != strCSEASON.LastIndexOf("0412"))
                            strRukava = "КР";
                        if (-1 != strCSEASON.LastIndexOf("0411"))
                            strRukava = "ДЛ";
                        //strRukava = "КР";
                        lines[i] = lines[i].Replace("#27", strRukava);

                        lines[i] = lines[i].Replace("#31", pp.strDate);
                    }
                    if (nPrintedAll % 30 == 0 && 0 != nPrintedAll) {
                        DialogResult rez = MessageBox.Show(null, "Напечатали: " + nPrintedAll.ToString() + ", стикеров, продолжить?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (rez == DialogResult.No) {
                            Trace.WriteLine("Прервали печать");
                            return false;
                        }
                        CheckStikers();
                    }
                    File.WriteAllLines(strNewFileName, lines, GetStickerEncoding());
                    SendTextFileToPrinter(strNewFileName);
                    //Trace.WriteLine("Напечатали стикер");
                    nPrintedAll++;
                }
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (bLastRow)
                CheckStikers();

            return true;
        }

    }
}
