using DbfLib;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using PrintSticker.MarkingObjectsBase;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым
#pragma warning disable IDE0057 //Substring можно упростить
#pragma warning disable CA1864 //Чтобы избежать двойного поиска, вызовите "TryAdd" вместо вызова "Add"
#pragma warning disable CA1854 //Предпочитать вызов TryGetValue доступу к индексатору словаря

namespace PrintSticker {
    internal class MarkingBV() : Marking() {
        private readonly ColorBV _colorBV = new();

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
                BARCODE = 27,
                KOL = 10,
                GTIN = 23,
                KOL_KM = 24,
                IZDNAME = 28
            };
        }
        protected override string _GetProductFileName() {
            return "DbProductBV.dbf";
        }

        protected string _GetCvetBV(string strCvet) { //DBCOLOR.DBF
            if (!int.TryParse(strCvet,out int nColor))
                return strCvet;           
            string strColor = _colorBV.GetNameColor(nColor);
            if (strColor != nColor.ToString())
                return strColor;

            if (strColor == nColor.ToString()) {
                if (nColor >= 110 && nColor <= 119) return "Черный";
                if (nColor >= 120 && nColor <= 129) return "Т.Сер.с Синим";
                if (nColor >= 210 && nColor <= 259) return "Серый";
                if (nColor >= 130 && nColor <= 139) return "Серый - Тем.";
                if (nColor >= 270 && nColor <= 399) return "Коричневый";
                if (nColor >= 410 && nColor <= 499) return "Синий";
                if (nColor >= 510 && nColor <= 599) return "Зеленый";
                if (nColor >= 610 && nColor <= 669) return "Вишневый";
                if (nColor >= 710 && nColor <= 789) return "Белый";
                if (nColor >= 810 && nColor <= 869) return "Желтый";
                if (nColor >= 910 && nColor <= 949) return "Красный";
                if (nColor >= 950 && nColor <= 969) return "Вишневый";
                if (nColor >= 970 && nColor <= 989) return "Розовый";
                if (nColor >= 990 && nColor <= 999) return "Фуксия";

            }
            MessageBox.Show(null, $"Не найден цвет в справочнике BV для кода {nColor}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "";
        }
        protected override string GetNomencl(DataRow row) {
            int IZD = 3;
            int PRS = 5;
            int MOD = 6;
            int ART = 7;
            int RAZ = 8;
            int ART2 = 13;//Country
            int PATTERN = 14;
            int CCODE = 15;
            int CCLOTH = 16;
            int TMARK = 29;

            string strPRS = "";
            if (System.DBNull.Value != row[PRS])
                strPRS = row[PRS].ToString().Trim();
            string strMod = row[MOD].ToString().Trim() + " " + strPRS;
            string strIzd = row[IZD].ToString();
            string strIzdName = RestItemBV.GetIZDName(strIzd);

            string strART = row[ART].ToString().Trim();
            string strRAZ = row[RAZ].ToString();
            string[] parms = strRAZ.Split('-');
            //if (4 == parms.Length)//
            //    strRAZ = "(" + parms[0] + "-" + parms[1] + ")" + "-" + parms[2] + "-" + parms[3];
            if (3 == parms.Length) {
                int nRost = Convert.ToInt32(parms[0]);
                strRAZ = "(" + nRost.ToString() + "-" + (nRost + 6).ToString() + ")" + "-" + parms[1] + "-" + parms[2];
            }
            string strPATTERN = row[PATTERN].ToString();
            string strCCLOTH = row[CCLOTH].ToString();
            string strCountry = "РОССИЯ";//row[ART2].ToString();

            string strART2 = "";
            if (System.DBNull.Value != row[ART2]) {
                if ("06" == strIzd)
                    strART2 = " " + row[ART2].ToString().Trim();
                else
                    strCountry = row[ART2].ToString().Trim().ToUpper();
            }

            string strTMark = "";
            if (System.DBNull.Value != row[TMARK])
                strTMark = row[TMARK].ToString().Trim();
           
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

            string strTMark2 = "";
            if ("" != strTMark)
                strTMark2 = " марка " + strTMark;          
            return strIzdName + " мод." + strMod + " арт." + strART + strART2 + strRazmer + strSostav + strCvet + strRis + " страна " + strCountry + strTMark2;
        }
        protected string _CorrectRazm(string strRAZ) {
            string[] parms = strRAZ.Split('-');
            string str2;
            if (parms.Length > 2) {
                string str1 = parms[parms.Length - 1];
                if (str1.Length > 2 && "0" == str1.Substring(0, 1)) {
                    str2 = str1.Substring(1);
                    strRAZ = strRAZ.Replace(str1, str2);
                }
                str1 = parms[parms.Length - 2];
                if (str1.Length > 2 && "0" == str1.Substring(0, 1)) {
                    str2 = str1.Substring(1);
                    strRAZ = strRAZ.Replace(str1, str2);
                }
            }
            return strRAZ;
        }
        public override string GetSettingsID() {
            return _strSettingsID;
        }

        private void _GetUseColumn(out Dictionary<string, string> hUseColumns) {
            hUseColumns = [];
            hUseColumns.Add("№", "№");//0
            hUseColumns.Add("DEL", "DEL");//1
            hUseColumns.Add("NLIST", "NLIST");//2
            hUseColumns.Add("CJ", "Закупочная цена");//3
            hUseColumns.Add("PATTERN", "PATTERN");//4
            hUseColumns.Add("CCODE", "CCODE");//5
            hUseColumns.Add("CCLOTH", "Код состава ткани");//6
            hUseColumns.Add("COTHER", "Доп признаки");//7
            hUseColumns.Add("CSEASON", "Код сезона");//8
            hUseColumns.Add("IZD", "Код изделия");//9
            hUseColumns.Add("MOD", "Модель");//10
            hUseColumns.Add("PRS", "Прейскурант");//11
            hUseColumns.Add("RAZ", "Размер");//12
            hUseColumns.Add("ART", "Артикул");//13
            hUseColumns.Add("ART2", "ART2");//14
            hUseColumns.Add("LABEL", "LABEL");//15
            hUseColumns.Add("BRAKET", "Код торговой марки");//16
            hUseColumns.Add("FLOOR", "FLOOR");//17
            hUseColumns.Add("NOTE", "NOTE");//18
            hUseColumns.Add("VC", "VC");//19
        }
        public override void ExportToRaskroyny(DataGridView dataGridView) {
            if (dataGridView.SelectedRows.Count == 0)
                return;
            if (null == _markingPaths) return;
            //string NLIST = "1";
            //decimal CJ = 2;
            //string PATTERN = "3";
            //string CCODE = "4";
            //string CCLOTH = "5";
            //string COTHER = "6";
            //string CSEASON = "7";
            //string IZD = "8";
            //string MOD = "9";
            //string PRS = "10";
            //string RAZ = "11";
            //string ART = "12";
            //string ART2 = "13";
            //string LABEL = "14";
            //string BRAKET = "15";
            //string FLOOR = "16";
            //string NOTE = "17";
            //decimal VC = 18;

            string strFileName = "F300000.dbf";
            string strDestPath = "";
            string strSrcPath = "";
            DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, @"\ooo\patern\");

            string strNewDir = @"C:\Po_BOLSHEVICHKA\PrintSticker\ToRaskr\";
            Directory.CreateDirectory(strNewDir);
            string strNewPath = strNewDir + strFileName;
            File.Copy(strDestPath, strNewPath, true);
            strDestPath = strNewPath;

            _GetUseColumn(out Dictionary<string, string> hUseColumns);

            int nCodeOut = -1;
            DataTable dtblResult = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0", hUseColumns);
            if (0 != dtblResult.Rows.Count) {
                Trace.WriteLine("Ошибка, в шаблоне есть данные, в файле: " + strDestPath);
                return;
            }         
            int N = 0;
            int IZD = 3;
            int PRS = 5;
            int MOD = 6;
            int ART = 7;
            int RAZ = 8;
            //int KOL = 10;
            int PATTERN = 14;
            int CCODE = 15;
            //int CCLOTH = 16;
            //int COTHER = 17;
            //int CSEASON = 18;

            int CJ_REZ = 3;
            int CCLOTH_REZ = 6;
            int COTHER_REZ = 7;
            int CSEASON_REZ = 8;
            int IZD_REZ = 9;
            int MOD_REZ = 10;
            int PRS_REZ = 11;
            int RAZ_REZ = 12;
            int ART_REZ = 13;
            int BRAKET_REZ = 16;
            

            foreach (DataGridViewRow row in dataGridView.SelectedRows) {
                //int nCol = Convert.ToInt32(row.Cells[KOL].Value);
                int nCol = 1;
                for (int i = 1; i <= nCol; i++) {
                    //string strArt = row.Cells[ART].Value.ToString();
                    //int nPrice = 0;
                    //if ("207201" == strArt) nPrice = 10665;
                    //if ("207200" == strArt) nPrice = 11685;
                    string strRazmer = row.Cells[RAZ].Value.ToString();
                    if (strRazmer.Length != 11)
                        strRazmer = _CorrectRazmAddNull(strRazmer);
                    var r = dtblResult.NewRow();
                    r["№"] = row.Cells[N].Value;
                    r["NLIST"] = "";
                    r[CJ_REZ] = 0;//Закупочная цена
                    r["PATTERN"] = row.Cells[PATTERN].Value;
                    r["CCODE"] = row.Cells[CCODE].Value;
                    r[CCLOTH_REZ] = "";//1 - ПШ, Код состава ткани \\192.168.0.199\ps\BV.2\MAIL\REF\CLOTH.DBF
                    r[COTHER_REZ] = "";//"00" доп признаки \\192.168.0.199\ps\BV.2\MAIL\REF\OTHER.DBF
                    r[CSEASON_REZ] = "";//сезон М - Меж., Л - Лето, З - Зима \\192.168.0.199\ps\BV.2\MAIL\REF\SEASON.DBF
                    r[IZD_REZ] = row.Cells[IZD].Value;
                    r[MOD_REZ] = row.Cells[MOD].Value;
                    r[PRS_REZ] = " " + row.Cells[PRS].Value.ToString().Trim();
                    r[RAZ_REZ] = strRazmer;
                    r[ART_REZ] = row.Cells[ART].Value.ToString().PadLeft(15);
                    r["ART2"] = "";
                    r["LABEL"] = "01";
                    r[BRAKET_REZ] = "";//код торговой марки TRADEMAR.DBF  \\192.168.0.199\ps\BV.2\MAIL\REF\TRADEMAR.DBF
                    r["FLOOR"] = "";
                    r["NOTE"] = "";
                    r["VC"] = 0;
                    dtblResult.Rows.Add(r);
                }
            }
            DlgToRaskroyny dlg = new (ref dtblResult);
            if (DialogResult.OK != dlg.ShowDialog())
                return;

            if (dtblResult.Rows.Count > 0) {
                if (!File.Exists(strDestPath)) {
                    MessageBox.Show(null, "Отсутствует файл : " + strDestPath, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (!Dbf.AddTable(strDestPath, dtblResult)) {
                    MessageBox.Show("Ошибка добавления в файл: " + strDestPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                MessageBox.Show(null, "Экспорт прошел успешно, путь к экспортируемому файлу : " + strDestPath, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //File.Copy(strDestPath, strSrcPath, true);
            }
        }
        protected virtual bool IsChangeBarcode() {
            return false;
        }

        protected override bool _EditProduct(ref DataTable dt) {
            if(null == dt || 1 != dt.Rows.Count)
                return false;
            DlgAddToTableBV dlg = new();
            dlg.EnableBarcode(IsChangeBarcode());
            
            dlg.SetEditDataTableRow(dt);
            if (DialogResult.OK != dlg.ShowDialog())
                return false;
            return true;
        }


        public override bool AddNewProduct(Form parent, string strOrderID) {
            DlgAddToTableBV dlg = new();
            dlg.EnableBarcode(IsChangeBarcode());
            if (DialogResult.OK != dlg.ShowDialog(parent))
                return false;
            dlg.GetSelItems(out List<RestItemBV> listItems);
            if (0 == listItems.Count) return false;

            if (null == _markingPaths) return false;

            string strDestPath = _markingPaths.GetDestPath();           
            int nCodeOut = -1;
            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut);

            int INO = 2;
            dtTableRez.Rows.Clear();
            foreach (RestItemBV item in listItems) {
                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
                rowAdd[INO] = strOrderID;
                rowAdd[RESTCOLIDBV.IZD] = item.GetIZD();
                rowAdd[RESTCOLIDBV.KOD_P] = 0;

                rowAdd[RESTCOLIDBV.PRS] = item.GetPRS();
                rowAdd[RESTCOLIDBV.MOD] = item.GetMOD();
                rowAdd[RESTCOLIDBV.ART] = item.GetART().Trim();
                rowAdd[RESTCOLIDBV.RAZ] = item.GetRAZ();
                rowAdd[RESTCOLIDBV.KOL] = item.GetKOL();

                rowAdd[RESTCOLIDBV.ART2] = item.GetART2();

                rowAdd[RESTCOLIDBV.CJ] = 0.0;
                rowAdd[RESTCOLIDBV.SM] = 0.0;

                rowAdd[RESTCOLIDBV.PATTERN] = item.GetPATTERN();
                rowAdd[RESTCOLIDBV.CCODE] = item.GetCCODE();
                rowAdd[RESTCOLIDBV.CCLOTH] = item.GetCCLOTH();
                rowAdd[RESTCOLIDBV.COTHER] = item.GetCOTHER();
                rowAdd[RESTCOLIDBV.CSEASON] = item.GetCSEASON();

                rowAdd[RESTCOLIDBV.CJ2] = 0.0;
                rowAdd[RESTCOLIDBV.CR] = 0.0;

                rowAdd[RESTCOLIDBV.SRT] = item.GetSRT();

                rowAdd[RESTCOLIDBV.KOL_KM] = 0;
                rowAdd[RESTCOLIDBV.KOL_PRN] = 0;
                rowAdd[RESTCOLIDBV.KOL_TOSHOP] = 0;

                rowAdd[RESTCOLIDBV.BARCODE] = item.GetBARCODE_0();
                rowAdd[RESTCOLIDBV.IZDNAME] = RestItemBV.GetIZDName(item.GetIZD());
                rowAdd[RESTCOLIDBV.TMARK] = item.GetTMark();
                rowAdd[RESTCOLIDBV.CCLOTH2] = item.GetCCLOTH2();

                dtTableRez.Rows.Add(rowAdd);
            }
            if (!Dbf.AddTable(strDestPath, dtTableRez)) {
                MessageBox.Show("Ошибка добавления в файл: " + strDestPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestPath, _markingPaths.GetSrcPath(), true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        protected override string _GetCountryFileName() {
            return "DbCountriesBV.dbf";
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
                int nPosA = strProdict.IndexOf(" мод.");
                int nPos0 = strProdict.IndexOf(" арт.");
                int nPos1 = strProdict.IndexOf(" размер ");
                int nPos2 = strProdict.IndexOf(" состав ");
                int nPos3 = strProdict.IndexOf(" цвет ");
                int nPos31 = strProdict.IndexOf(" рис.");
                int nPos4 = strProdict.IndexOf(" страна ");
                int nPos5 = strProdict.IndexOf(" марка ");

                ToExcelItem tei = new () {
                    //product = .PR_WT_REMAINS_BV,
                    strColumn5_ProductName = strProdict
                };

                int nPos6 = strProdict.Length;

                int nPosANext = nPos0;
                if (-1 == nPosANext) nPosANext = nPos1;
                if (-1 == nPosANext) nPosANext = nPos2;
                if (-1 == nPosANext) nPosANext = nPos3;
                if (-1 == nPosANext) nPosANext = nPos31;
                if (-1 == nPosANext) nPosANext = nPos4;
                if (-1 == nPosANext) nPosANext = nPos5;
                if (-1 == nPosANext) nPosANext = nPos6;

                int nPos0Next = nPos1;
                if (-1 == nPos0Next) nPos0Next = nPos2;
                if (-1 == nPos0Next) nPos0Next = nPos3;
                if (-1 == nPos0Next) nPos0Next = nPos31;
                if (-1 == nPos0Next) nPos0Next = nPos4;
                if (-1 == nPos0Next) nPos0Next = nPos5;
                if (-1 == nPos0Next) nPos0Next = nPos6;

                int nPos1Next = nPos2;
                if (-1 == nPos1Next) nPos1Next = nPos3;
                if (-1 == nPos1Next) nPos1Next = nPos31;
                if (-1 == nPos1Next) nPos1Next = nPos4;
                if (-1 == nPos1Next) nPos1Next = nPos5;
                if (-1 == nPos1Next) nPos1Next = nPos6;

                int nPos2Next = nPos3;
                if (-1 == nPos2Next) nPos2Next = nPos31;
                if (-1 == nPos2Next) nPos2Next = nPos4;
                if (-1 == nPos2Next) nPos2Next = nPos5;
                if (-1 == nPos2Next) nPos2Next = nPos6;

                int nPos3Next = nPos31;
                if (-1 == nPos3Next) nPos3Next = nPos4;
                if (-1 == nPos3Next) nPos3Next = nPos5;
                if (-1 == nPos3Next) nPos3Next = nPos6;

                int nPos4Next = nPos5;                
                if (-1 == nPos4Next) nPos4Next = nPos6;


                string strRazmer = "ОТСУТСТВУЕТ";
                if (-1 != nPos1) strRazmer = strProdict.Substring(nPos1 + " размер ".Length, nPos1Next - nPos1 - " размер ".Length);

                string strSostav = "ОТСУТСТВУЕТ";
                if (-1 != nPos2) strSostav = strProdict.Substring(nPos2 + " состав ".Length, nPos2Next - nPos2 - " состав ".Length);

                string strCvet = "РАЗНОЦВЕТНЫЙ";
                if (-1 != nPos3) strCvet = strProdict.Substring(nPos3 + " цвет ".Length, nPos3Next - nPos3 - " цвет ".Length);

                string strCountry = "";
                if (-1 != nPos4) strCountry = strProdict.Substring(nPos4 + " страна ".Length, nPos4Next - nPos4 - " страна ".Length);

                string strMarka = "";
                if (-1 != nPos5) strMarka = strProdict.Substring(nPos5 + " марка ".Length);

                tei.strColumn19_Sostav = _GetSostav(strSostav);

                string strTypeProduct = strProdict.Substring(0, nPosA);//nPos0

                string strMod1 = strProdict.Substring(nPosA + " мод.".Length, nPosANext - nPosA - " мод.".Length);
                string strMod2 = strProdict.Substring(nPos0 + " арт.".Length, nPos0Next - nPos0 - " арт.".Length);
                tei.strColumn17_Mod = strMod1 + " " + strMod2;

                if("" != strMarka)
                    tei.strColumn6_Country = strMarka;

                tei.strColumn7_Country = _GetCountry(strCountry);

                //tei.strColumn8_INN = "7708815300";
                //tei.strColumn9_IZGOTOVITEL = "ООО 'ВЕСПЕР ТРЕЙДИНГ'";

                tei.strColumn8_INN = "";
                tei.strColumn9_IZGOTOVITEL = "";                
                tei.strColumn10_SortProduct = _GetTypeProduct(strTypeProduct);
                tei.strColumn12_TNVED = _GetTNVED(strTypeProduct, strSostav, tei.strColumn17_Mod);
                tei.strColumn14_Razmer = strRazmer;

                int nPos66 = strRazmer.LastIndexOf(')');
                if (-1 != nPos66)
                    tei.strColumn15_Rost = strRazmer.Substring(1, nPos66 - 1);

                tei.strColumn16_Cvet = _GetCvetBV(strCvet);

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
                MessageBox.Show(null, "Для всех выбранных элементов уже ранее были получены GTIN", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            ExcelExport excExp = new();
            if (!excExp.Open(strDestPathToGTIN, out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(null, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!excExp.ExportData(xlsWorkSheet, ref listToExcelItems)) {
                MessageBox.Show(null, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MessageBox.Show(null, "Успешно сгенерирован файл:" + strDestPathToGTIN, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
      
        public override bool ShowPage(_Form1 parent, DataGridView dataGridView, GridViewExtensions.DataGridFilterExtender dataGridFilter, string strOrderID = "-1") {
            if (null == parent) return false;
            if (!_InitPaths()) return false;

            _dataGridView = dataGridView;
            _hNumTable = [];
            _dtOpenedFile = Win32.GetLastWriteTime(_markingPaths.GetSrcPath());
            _dtGtinOrdersMkLoaded = _GetLastWriteTime(_markingPaths.GetSrcPathOrdersKM());

            string strDest = "";
            string strSrc = "";
            if (!DbfWrapper.CheckIfFileExist(_markingPaths.GetProductsFileName(), ref strDest, ref strSrc, _markingPaths.GetGeneratedFolder())) return false;

            LoadDataBV(_markingPaths.GetProductsFileName(), dataGridView, dataGridFilter, ref _hNumTable, _markingPaths, strOrderID, _GetShopPrefix());
            parent.UpdateCountBV(dataGridView);
            return true;
        }

        public void LoadDataBV(string strFileName, DataGridView dgv, GridViewExtensions.DataGridFilterExtender dgFE, ref Dictionary<int, int> hNumTableSPO,
            MarkingPaths markingPaths = null, string strOrderID = "", string strShopPrefix = "") {
            _LoadDataBV(strFileName, dgv, dgFE, ref hNumTableSPO, markingPaths, strOrderID, strShopPrefix);
        }
        private void _LoadDataBV(string strFileName, DataGridView dgv, GridViewExtensions.DataGridFilterExtender dgFE, ref Dictionary<int, int> hNumTableSPO,
            MarkingPaths markingPaths = null, string strOrderID = "", string strShopPrefix = "") {
            int nIsIndexMDXNestand = -1;
            int nCodeOut = -1;

            //show only art
            HashSet<string> hShowOnlyArt = [];
            string strDestPathBARCODES = "";
            string strSrcPathBARCODES = "";

            if (null == markingPaths) {
                if (!DbfWrapper.CheckIfFileExist("BvShowOnlyArt.txt", ref strDestPathBARCODES, ref strSrcPathBARCODES, @"\BV\patern\")) { return; }
                string[] aStr = File.ReadAllLines(strDestPathBARCODES);
                foreach (string str in aStr)
                    hShowOnlyArt.Add(str.Trim());
            }
            //
            string strDestPathSoputka = "";
            string strSrcPathSoputka = "";

            string strDestPathOrdersKM = "";
            string strSrcPathOrdersKM = "";

            if (null != markingPaths) {
                strDestPathSoputka = markingPaths.GetDestPath();
                strDestPathOrdersKM = markingPaths.GetDestPathOrdersKM();
                strSrcPathOrdersKM = markingPaths.GetSrcPathOrdersKM();
            } else {
                if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, @"\BV\generated\")) { return; }
                if (!DbfWrapper.CheckIfFileExist("BVOrdersKM.dbf", ref strDestPathOrdersKM, ref strSrcPathOrdersKM, @"\BV\")) {  return; }
            }

            string strDestPathArticul = "";
            string strSrcPathArticul = "";
            if (!DbfWrapper.CheckIfFileExist("Articul.dbf", ref strDestPathArticul, ref strSrcPathArticul, @"\Bv.2\Mail\Ref\"))
                return;

            Dictionary<string, string> dicArticulSostav = [];
            int NCARD = 3;
            int SL = 4;
            DataTable dtTableArticul = Dbf.LoadDbfWithAddColumns(strDestPathArticul, out nIsIndexMDXNestand, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableArticul.Rows.Count; i++) {
                if (System.DBNull.Value == dtTableArticul.Rows[i][NCARD] || System.DBNull.Value == dtTableArticul.Rows[i][SL])
                    continue;
                dicArticulSostav.Add(dtTableArticul.Rows[i][NCARD].ToString(), (string)dtTableArticul.Rows[i][SL]);
            }

            if (!dicArticulSostav.ContainsKey("940016")) dicArticulSostav.Add("940016", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("940020")) dicArticulSostav.Add("940020", "50% ШЕРСТЬ,50% П Э");

            if (!dicArticulSostav.ContainsKey("9494")) dicArticulSostav.Add("9494", "65% ШЕРСТЬ,35% П Э");
            if (!dicArticulSostav.ContainsKey("940004")) dicArticulSostav.Add("940004", "55% ШЕРСТЬ,45% ЛЕН");
            if (!dicArticulSostav.ContainsKey("940012")) dicArticulSostav.Add("940012", "55% ШЕРСТЬ,45% ЛЕН");

            if (!dicArticulSostav.ContainsKey("940001")) dicArticulSostav.Add("940001", "50% ШЕРСТЬ,47% П Э,3% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("940003")) dicArticulSostav.Add("940003", "70% ШЕРСТЬ,27% П Э,3% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("940005")) dicArticulSostav.Add("940005", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("940013")) dicArticulSostav.Add("940013", "70% ШЕРСТЬ,28% П Э,2% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("940014")) dicArticulSostav.Add("940014", "50% ШЕРСТЬ,47% П Э,3% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("940015")) dicArticulSostav.Add("940015", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("940017")) dicArticulSostav.Add("940017", "50% ШЕРСТЬ,47% П Э,3% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("940018")) dicArticulSostav.Add("940018", "50% ШЕРСТЬ,47% П Э,3% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("940019")) dicArticulSostav.Add("940019", "80% ШЕРСТЬ,8% П Э,2% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("940021")) dicArticulSostav.Add("940021", "70% ШЕРСТЬ,30% П Э");
            if (!dicArticulSostav.ContainsKey("940022")) dicArticulSostav.Add("940022", "70% ШЕРСТЬ,30% П Э");
            if (!dicArticulSostav.ContainsKey("970000")) dicArticulSostav.Add("970000", "50% ШЕРСТЬ,50% П Э");

            if (!dicArticulSostav.ContainsKey("9468")) dicArticulSostav.Add("9468", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9469")) dicArticulSostav.Add("9469", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9470")) dicArticulSostav.Add("9470", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9496")) dicArticulSostav.Add("9496", "44% ШЕРСТЬ,53% П Э,3% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("9497")) dicArticulSostav.Add("9497", "44% ШЕРСТЬ,53% П Э,3% ЛАЙКРА");
            if (!dicArticulSostav.ContainsKey("9499")) dicArticulSostav.Add("9499", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9450")) dicArticulSostav.Add("9450", "50% ШЕРСТЬ,50% П Э");

            if (!dicArticulSostav.ContainsKey("6738")) dicArticulSostav.Add("6738", "100% ШЕРСТЬ");
            if (!dicArticulSostav.ContainsKey("9491")) dicArticulSostav.Add("9491", "70% ШЕРСТЬ,30% П Э");
            if (!dicArticulSostav.ContainsKey("9415")) dicArticulSostav.Add("9415", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9492")) dicArticulSostav.Add("9492", "50% ШЕРСТЬ,48% П Э,2% ЛАЙКРА");

            if (!dicArticulSostav.ContainsKey("9420")) dicArticulSostav.Add("9420", "70% ШЕРСТЬ,30% П Э");
            if (!dicArticulSostav.ContainsKey("9424")) dicArticulSostav.Add("9424", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9444")) dicArticulSostav.Add("9444", "70% ШЕРСТЬ,30% П Э");
            if (!dicArticulSostav.ContainsKey("9463")) dicArticulSostav.Add("9463", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9480")) dicArticulSostav.Add("9480", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9481")) dicArticulSostav.Add("9481", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9473")) dicArticulSostav.Add("9473", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9455")) dicArticulSostav.Add("9455", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("9458")) dicArticulSostav.Add("9458", "50% ШЕРСТЬ,50% П Э");

            if (!dicArticulSostav.ContainsKey("207201")) dicArticulSostav.Add("207201", "50% ШЕРСТЬ,50% П Э");
            if (!dicArticulSostav.ContainsKey("207200")) dicArticulSostav.Add("207200", "70% ШЕРСТЬ,30% П Э");
            if (!dicArticulSostav.ContainsKey("580012")) dicArticulSostav.Add("580012", "50% ШЕРСТЬ,50% П Э");


            //01
            Dictionary<string, int> dicGtinBarcodeShop_Count = [];
            int GTIN = 3;
            int STATUS = 9;
            int BARCODE3 = 10;
            string strDestPathKM = "";
            string strSrcPathKM = "";

            if (null != markingPaths)
                strDestPathKM = markingPaths.GetDestPathKM();
            else {
                if (!DbfWrapper.CheckIfFileExist("BVKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\BV\")) { return; }
            }
            DataTable dtTableSoputkaKM = Dbf.LoadDbfWithAddColumns(strDestPathKM, out nIsIndexMDXNestand, ref nCodeOut, "DEL", "0");
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
            if (!DbfWrapper.GetGtinsBv(ref dicNomenclGtin, markingPaths))
                return;

            dgFE.DataGridView = null;

            Dictionary<string, KMCOUNT> hGtin_Barcode_CountKM = [];

            Dictionary<string, string> hUseColumns = [];
            GetUseColumnsBV(ref hUseColumns);//переделать

            //_dtBvGtinOrdersMkLoaded = _GetLastWriteTime(strSrcPathOrdersKM);

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
                dtTableOrdersKM = Dbf.LoadDbfWithAddColumns(strDestPathOrdersKM, out _, ref nCodeOut, "DEL", "0");
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
                //DateTime dtStart = DateTime.Now.AddDays(-200);
                //string strDateStart = (dtStart.Year % 1000).ToString() + dtStart.Month.ToString("00") + dtStart.Day.ToString("00") + dtStart.Hour.ToString("00") + dtStart.Minute.ToString("00");
                //dtTableSPO = Dbf.LoadDbfWithAddColumns(strDestPathSPO, out nIsIndexMDXNestand, ref nCodeOut, "DT_KORR", ">#" + strDateStart, hUseColumns, false, false);

                bw.ReportProgress(100);
            };
            DlgReportProgress dlg = new(bw, "Извлечение данных");
            dlg.ShowDialog();

            //this.Cursor = Cursors.WaitCursor;
            int ART = 7;

            for (int i = dtTableSPO.Rows.Count - 1; i >= 0; i--) {
                if (0 == hShowOnlyArt.Count || strFileName == "BvRestShopsToStikers_SGP.dbf")
                    break;
                if (System.DBNull.Value == dtTableSPO.Rows[i][ART]) {
                    dtTableSPO.Rows.Remove(dtTableSPO.Rows[i]);
                    continue;
                }
                string strArt = ((string)dtTableSPO.Rows[i][ART]).Trim();
                if (!hShowOnlyArt.Contains(strArt))
                    dtTableSPO.Rows.Remove(dtTableSPO.Rows[i]);
            }


            DataView dv = dtTableSPO.DefaultView;
            //dv.Sort = "Перед.СПО desc, № desc";
            dgv.DataSource = dv.ToTable();

            dgFE.DataGridView = dgv;////

            int RAZ = 8;
            int CCLOTH = 16;
            int GTIN2 = 23;
            int KOL_KM = 24;
            int KOL_PRN = 25;
            int KOL_TOSHOP = 26;
            int BARCODE2 = 27;
            int CCLOTH2 = 30;
            hNumTableSPO = [];
            for (int i = 0; i < dgv.Rows.Count; i++) {
                hNumTableSPO.Add(1 + i, (int)dgv.Rows[i].Cells[0].Value);
                dgv.Rows[i].Cells[0].Value = 1 + i;

                if (System.DBNull.Value != dgv.Rows[i].Cells[ART].Value) {
                    string strNcard = ((string)dgv.Rows[i].Cells[ART].Value).Trim();
                    string strArt = dgv.Rows[i].Cells[ART].Value.ToString().Trim();

                    //if (_hUsedMishaArticuled.Contains(strArt))
                    //    MessageBox.Show("Ошибка, отображаем артикул с которым работает Миша: " + strArt);

                    if (System.DBNull.Value != dgv.Rows[i].Cells[CCLOTH2].Value)
                        dgv.Rows[i].Cells[CCLOTH].Value = dgv.Rows[i].Cells[CCLOTH2].Value;
                    else {
                        if (dicArticulSostav.ContainsKey(strNcard))
                            dgv.Rows[i].Cells[CCLOTH].Value = dicArticulSostav[strNcard];
                        else
                            dgv.Rows[i].Cells[CCLOTH].Value = "?????";
                    }

                }
                if (System.DBNull.Value != dgv.Rows[i].Cells[RAZ].Value) {
                    string strRazm = ((string)dgv.Rows[i].Cells[RAZ].Value).Trim();
                    string[] parms = strRazm.Split('-');

                    if (4 == parms.Length) {                        
                        dgv.Rows[i].Cells[RAZ].Value = "(" + parms[0] + "-" + parms[1] + ")" + "-" + parms[2] + "-" + parms[3];
                        continue;
                    }
                    if (3 != parms.Length)
                        continue;
                    int nRost = Convert.ToInt32(parms[0]);                    
                    string strNweRazm = "(" + nRost.ToString() + "-" + (nRost + 6).ToString() + ")" + "-" + parms[1] + "-" + parms[2];
                }

                //   if (System.DBNull.Value == dgv.Rows[i].Cells[GTIN2].Value || "0" == dgv.Rows[i].Cells[GTIN2].Value.ToString()) {
                DataRow row = ((DataRowView)dgv.Rows[i].DataBoundItem).Row;
                string strNomencl = GetNomenclBV(row);
                if (dicNomenclGtin.ContainsKey(strNomencl))
                    dgv.Rows[i].Cells[GTIN2].Value = dicNomenclGtin[strNomencl];
                else
                    dgv.Rows[i].Cells[GTIN2].Value = "0";

                //   }
                if (System.DBNull.Value != dgv.Rows[i].Cells[GTIN2].Value) {
                    string strGTIN_Barcode = dgv.Rows[i].Cells[GTIN2].Value.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString();
                    if (hGtin_Barcode_CountKM.ContainsKey(strGTIN_Barcode)) {
                        dgv.Rows[i].Cells[KOL_KM].Value = hGtin_Barcode_CountKM[strGTIN_Barcode].nKOL_KM_ALL;
                        dgv.Rows[i].Cells[KOL_PRN].Value = hGtin_Barcode_CountKM[strGTIN_Barcode].nKOL_KM_PRN;
                    }
                }
                if (System.DBNull.Value != dgv.Rows[i].Cells[GTIN2].Value) {
                    long lGTIN = (long)dgv.Rows[i].Cells[GTIN2].Value;

                    string strShopPrefixLocal = strShopPrefix;
                    //if ("" == strShopPrefixLocal)
                    //    strShopPrefixLocal = _GetShopPrefix(PRODUCT.PR_BV);

                    string strKey = lGTIN.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString() + "_" + strShopPrefixLocal;
                    if (!dicGtinBarcodeShop_Count.ContainsKey(strKey))
                        continue;
                    dgv.Rows[i].Cells[KOL_TOSHOP].Value = dicGtinBarcodeShop_Count[strKey].ToString();
                }
            }            
            InidataGridViewSoputkaBv(ref dgv);
            //hGtin_CountKM
            //this.Cursor = Cursors.Default;
        }
        public static string GetNomenclBV(DataRow row) {
            int IZD = 3;
            int PRS = 5;
            int MOD = 6;
            int ART = 7;
            int RAZ = 8;
            int ART2 = 13;//Country
            int PATTERN = 14;
            int CCODE = 15;
            int CCLOTH = 16;

            string strPRS = "";
            if (System.DBNull.Value != row[PRS])
                strPRS = row[PRS].ToString().Trim();
            string strMod = row[MOD].ToString().Trim() + " " + strPRS;
            string strIzd = row[IZD].ToString();
            string strIzdName = RestItemBV.GetIZDName(strIzd);

            string strART = row[ART].ToString().Trim();
            string strRAZ = row[RAZ].ToString();
            string[] parms = strRAZ.Split('-');
            //if (4 == parms.Length)//
            //    strRAZ = "(" + parms[0] + "-" + parms[1] + ")" + "-" + parms[2] + "-" + parms[3];
            if (3 == parms.Length) {
                int nRost = Convert.ToInt32(parms[0]);
                strRAZ = "(" + nRost.ToString() + "-" + (nRost + 6).ToString() + ")" + "-" + parms[1] + "-" + parms[2];
            }
            string strPATTERN = row[PATTERN].ToString();
            string strCCLOTH = row[CCLOTH].ToString();
            string strCountry = "РОССИЯ";//row[ART2].ToString();

            string strART2 = "";
            if (System.DBNull.Value != row[ART2]) {
                if ("06" == strIzd)
                    strART2 = " " + row[ART2].ToString().Trim();
                else
                    strCountry = row[ART2].ToString().Trim().ToUpper();
            }
            string strCCODE = row[CCODE].ToString();

            string strRazmer = "";
            if ("" != strRAZ)
                strRazmer = " размер " + strRAZ;
            string strSostav = "";
            if ("" != GetSostav(strCCLOTH))
                strSostav = " состав " + GetSostav(strCCLOTH);
            string strCvet = "";
            if ("" != strCCODE)
                strCvet = " цвет " + strCCODE;
            string strRis = "";
            if ("" != strPATTERN)
                strRis = " рис." + strPATTERN;
            return strIzdName + " мод." + strMod + " арт." + strART + strART2 + strRazmer + strSostav + strCvet + strRis + " страна " + strCountry;
        }
        public static string GetSostav(string strSostav) {
            if ("|" == strSostav)
                return "";
            string[] a2Line = strSostav.Split('|');
            if (2 != a2Line.Length)
                return strSostav;
            string strPrefix = a2Line[0];
            if ("_" == strPrefix.Substring(strPrefix.Length - 1, 1))
                strPrefix = strPrefix.Substring(0, strPrefix.Length - 1);

            string strPostfix = a2Line[1];
            string[] aPostfix = strPostfix.Split('-');

            string[] aPrefix = strPrefix.Split('_');
            for (int i = 0; i < aPrefix.Length; i++)
                aPrefix[i] = _GetTextile(aPrefix[i]);

            if (aPrefix.Length > aPostfix.Length) {
                MessageBox.Show(null, "Ошибка входных данных:" + strSostav, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return strSostav;
            }
            string strOut = "";
            for (int i = 0; i < aPrefix.Length; i++) {
                if ("" == strOut)
                    strOut = aPrefix[i] + " " + aPostfix[i] + "%";
                else
                    strOut = strOut + "," + aPrefix[i] + " " + aPostfix[i] + "%";

                if (i == aPrefix.Length - 1) {
                    for (int j = aPrefix.Length; j < aPostfix.Length; j++)
                        strOut = strOut + " " + aPostfix[j] + "%";
                }
            }
            return strOut;// +"       #" + strSostav;
        }
        
        public static void InidataGridViewSoputkaBv(ref DataGridView dataGridView) {
            dataGridView.Columns[0].Visible = false;//№"
            dataGridView.Columns[1].Visible = false;//DEL
            dataGridView.Columns[2].Visible = false;//INO
            dataGridView.Columns[4].Visible = false;//KOD_P
            dataGridView.Columns[9].Visible = false;//SRT
            dataGridView.Columns[11].Visible = false;//CJ
            dataGridView.Columns[12].Visible = false;//SM
            //dataGridView.Columns[13].Visible = false;//ART2

            dataGridView.Columns[17].Visible = false;//COTHER
            dataGridView.Columns[18].Visible = false;//CSEASON

            dataGridView.Columns[19].Visible = false;//ONETWO
            //dataGridView.Columns[20].Visible = false;//FLABEL
            dataGridView.Columns[21].Visible = false;//CJ2
            dataGridView.Columns[22].Visible = false;//CJ2

            dataGridView.Columns[27].Visible = false;//BARCODE
            if(dataGridView.Columns.Count > 29)
                dataGridView.Columns[30].Visible = false;//CCLOTH2

            dataGridView.Columns[3].Width = 50;//IZD           
            dataGridView.Columns[10].Width = 50;//KOL
            dataGridView.Columns[11].Width = 50;//CJ
            dataGridView.Columns[12].Width = 50;//SM
            dataGridView.Columns[13].Width = 50;//ART2
            dataGridView.Columns[14].Width = 50;//PATTERN
            dataGridView.Columns[15].Width = 50;//CCODE
            dataGridView.Columns[16].Width = 50;//CCLOTH
            dataGridView.Columns[17].Width = 50;//COTHER
            dataGridView.Columns[18].Width = 50;//CSEASON
            dataGridView.Columns[19].Width = 50;//ONETWO
            dataGridView.Columns[20].Width = 50;//FLABEL
            dataGridView.Columns[21].Width = 50;//CJ2
            dataGridView.Columns[22].Width = 50;//CR
            dataGridView.Columns[23].Width = 50;//GTIN
            dataGridView.Columns[24].Width = 50;//KOL_KM
            dataGridView.Columns[25].Width = 50;//KOL_PRN
            dataGridView.Columns[26].Width = 50;//KOL_TOSHOP
            dataGridView.Columns[27].Width = 50;//BARCODE
           
            DataGridViewCellStyle style = new() {
                Font = new Font(dataGridView.Font, FontStyle.Bold)
            };
            dataGridView.DefaultCellStyle = style;
            //dataGridView.Columns[2].DisplayIndex = 1;//
        }
        public static void GetUseColumnsBV(ref Dictionary<string, string> hUseColumns) {
            hUseColumns.Add("№", "№");//0
            hUseColumns.Add("DEL", "DEL");//1
            hUseColumns.Add("INO", "INO");//2
            hUseColumns.Add("IZD", "Код изделия");//3 //IZD
            hUseColumns.Add("KOD_P", "KOD_P");//4
            hUseColumns.Add("PRS", "Прейскурант");//5 //PRS
            hUseColumns.Add("MOD", "Модель");//6 "Наим. Изделия"
            hUseColumns.Add("ART", "Артикул");//7
            hUseColumns.Add("RAZ", "Размер");//8
            hUseColumns.Add("SRT", "SRT");//9
            hUseColumns.Add("KOL", "Количество");//10
            hUseColumns.Add("CJ", "CJ");//11
            hUseColumns.Add("SM", "SM");//12
            hUseColumns.Add("ART2", "Производитель");//13
            hUseColumns.Add("PATTERN", "Рисунок");//14
            hUseColumns.Add("CCODE", "Цвет");//15
            hUseColumns.Add("CCLOTH", "Состав");//16
            hUseColumns.Add("COTHER", "Поставщик");//17
            hUseColumns.Add("CSEASON", "CSEASON");//18             
            hUseColumns.Add("ONETWO", "ONETWO");//19
            hUseColumns.Add("FLABEL", "Большевички Штрихкод");//20                       
            hUseColumns.Add("CJ2", "CJ2");//21
            hUseColumns.Add("CR", "CR");//22                          
            hUseColumns.Add("GTIN", "GTIN (Штрихкод)");//23
            hUseColumns.Add("KOL_KM", "Всего КМ запрошено в ЧЗ");//24
            hUseColumns.Add("KOL_PRN", "Получено КМ из ЧЗ");//25
            hUseColumns.Add("KOL_TOSHOP", "Распечатано КМ для магазинов");//26
            hUseColumns.Add("BARCODE", "Штрихкод");//27
            hUseColumns.Add("IZDNAME", "Изделие");//28
            hUseColumns.Add("TMARK", "Торговая марка");//29
            hUseColumns.Add("CCLOTH2", "Состав пользовательский");//30
                                                       
            
        }
        protected bool _PrintRow(PrintParms pp, DataRow row, string strDestPathKM, string strDestPathPaternSP,
     ref int nPrintedAll, List<KM> listKM = null, bool bLastRow = false, bool bPrintOnFile = false, bool bFlabelToBarcode = false) {
            return _PrintRowBV(row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, pp, listKM, bLastRow, bPrintOnFile, bFlabelToBarcode);
        }

        protected bool _PrintRowBV(DataRow row, string strDestPathKM, string strDestPathPaternSP, ref int nPrintedAll,
PrintParms pp, List<KM> listKM = null, bool bLastRow = false, bool bPrintOnFile = false, bool bFlabelToBarcode = false) {
            if (null == pp) {
                pp = new() { bMultySize = true, strDate = "03 2024", strPrefix = "" };
            }

            if (null == _markingPaths)
                return false;

            int STATUS = 9;
            int KM_KM = 5;
            int N = 0;

            int BARCODE = 27;
            int MOD = 28;
            int PRS = 5;
            int MOD2 = 6;
            int ART = 7;
            int RAZ = 8;
            int SRT = 9;
            int KOL = 10;
            int ART2 = 13;
            int PATTERN = 14;
            int CCODE = 15;
            int CCLOTH = 16;
            int CSEASON = 18;
            int FLABEL = 20;
            int GTIN = 23;

            int nCodeOut = -1;

            string strBARCODE = row[BARCODE].ToString();
            string strMOD = row[MOD].ToString();

            //strMOD = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С БРЮКАМИ", "КОСТЮМ ЖЕНСКИЙ БР.");
            //strMOD = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С ЮБКОЙ", "КОСТЮМ ЖЕНСКИЙ ЮБ.");

            string strART = row[ART].ToString();
            string strART_second = "";

            //string strModel = row[MOD2].ToString() + "" + row[PRS].ToString();
            //if (product == .PR_VETEX_PRODUCTION || product == .PR_VETEX_IMPORT)

            string strModel = row[MOD2].ToString().Trim() + "  " + row[PRS].ToString().Trim();

            int nPosG = strART.ToUpper().LastIndexOf("ЖЕН");
            if (strART.Length > 17 && -1 == nPosG) {
                strART_second = strART.Substring(17);
                strART = strART.Substring(0, 17);
            }
            if (strART.Length > 17 && -1 != nPosG) {
                int nPosPr = strART.LastIndexOf(" ");
                string strPostfix = "";
                if (-1 != nPosPr) {
                    strPostfix = strART.Substring(nPosPr);
                    strART = strART.Substring(0, nPosPr);
                    strART_second = strPostfix.Trim();
                }
                if (strART.Length > 17) {
                    strART_second = strART.Substring(17) + strPostfix;
                    strART = strART.Substring(0, 17);
                }
            }
            strART_second = strART_second.Trim();
            string strRAZ = row[RAZ].ToString();// +"56(170-112-120)";
                                                //strRAZ = "(176-182)-100-88";
            if (pp.bMultySize) {
                if (-1 == strRAZ.LastIndexOf('(')) {
                    string[] parms = strRAZ.Split('-');
                    if (4 == parms.Length)
                        strRAZ = "(" + parms[0] + "-" + parms[1] + ")" + "-" + parms[2] + "-" + parms[3];
                    if (3 == parms.Length) {
                        int nRost = Convert.ToInt32(parms[0]);
                        strRAZ = "(" + nRost.ToString() + "-" + (nRost + 6).ToString() + ")" + "-" + parms[1] + "-" + parms[2];
                    }
                }
            }
            string strSRT = "";
            if (System.DBNull.Value != row[SRT])
                strSRT = row[SRT].ToString();

            string strGTIN = row[GTIN].ToString();
            string strART2 = row[ART2].ToString();
            string strFlabel = row[FLABEL].ToString().Trim();

            //if (product == .PR_VETEX_PRODUCTION) {
            if ("" == strART2)
                strART2 = "Россия";
            strART2 = strART2.Replace("РОССИЯ", "Россия");
        //}
            //if (product == .PR_VETEX_REMAINS)
            //    strART2 = "Россия";

            string strCCODE = row[CCODE].ToString();//цвет
            string strCCLOTH = row[CCLOTH].ToString();//состав

            string[] parmsCL = strCCLOTH.Split('|');
            string strCCLOTH1 = strCCLOTH;
            //string strCCLOTH2 = "";
            if (2 == parmsCL.Length) {
                strCCLOTH1 = parmsCL[0];
                //strCCLOTH2 = parmsCL[1];
            }


            string strCSEASON = row[CSEASON].ToString();
            string strPATTERN = row[PATTERN].ToString();//рисунок                               
            int nKol = (int)row[KOL];
            if (null != listKM)
                nKol = listKM.Count;//repeat

            try {
                //List<KM> listKM = new();
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

                //strPrefix = "401";
                //strPrefix = "414";
                //strPrefix = "300";
                //strPrefix = "421";
                //strPrefix = "057";
                //strPrefix = "401";
                //strPrefix = "432";
                //strPrefix = "300";
                //strPrefix = "310";
                //strPrefix = "433";
                //strPrefix = "260";
                string strMOD1 = "";
                string strMOD2 = "";

                if ("КОСТЮМ МУЖСКОЙ" == strMOD) {
                    if (strART == "9725" || strART == "202032" || strART == "202038" || strART == "204706" || strART == "204711" || strART == "204750" ||
                        strART == "204784" || strART == "205308") {
                        strMOD1 = "КОСТЮМ МУЖСКОЙ";
                        strMOD2 = "SPORT STYLE";
                    } else {
                        strMOD1 = strMOD.Replace("КОСТЮМ МУЖСКОЙ", "КОСТЮМ");
                        strMOD2 = strMOD.Replace("КОСТЮМ МУЖСКОЙ", "МУЖСКОЙ");
                    }
                } else if ("КОМПЛЕКТ МУЖСКОЙ" == strMOD) {
                    strMOD1 = strMOD.Replace("КОМПЛЕКТ МУЖСКОЙ", "КОМПЛЕКТ");
                    strMOD2 = strMOD.Replace("КОМПЛЕКТ МУЖСКОЙ", "МУЖСКОЙ");
                    strART = strART + "," + strART2;
                } else if ("ЖИЛЕТ МУЖСКОЙ" == strMOD) {
                    strMOD1 = strMOD.Replace("ЖИЛЕТ МУЖСКОЙ", "ЖИЛЕТ");
                    strMOD2 = strMOD.Replace("ЖИЛЕТ МУЖСКОЙ", "МУЖСКОЙ");
                } else if ("ПИДЖАК МУЖСКОЙ" == strMOD) {
                    strMOD1 = strMOD.Replace("ПИДЖАК МУЖСКОЙ", "ПИДЖАК");
                    strMOD2 = strMOD.Replace("ПИДЖАК МУЖСКОЙ", "МУЖСКОЙ");
                } else if ("БРЮКИ МУЖСКИЕ" == strMOD) {
                    strMOD1 = strMOD.Replace("БРЮКИ МУЖСКИЕ", "БРЮКИ");
                    strMOD2 = strMOD.Replace("БРЮКИ МУЖСКИЕ", "МУЖСКИЕ");
                } else if ("ЮБКА ЖЕНСКАЯ" == strMOD) {
                    strMOD1 = strMOD.Replace("ЮБКА ЖЕНСКАЯ", "ЮБКА");
                    strMOD2 = strMOD.Replace("ЮБКА ЖЕНСКАЯ", "ЖЕНСКАЯ");
                } else if ("КОСТЮМ ЖЕНСКИЙ ЮБКА" == strMOD) {
                    strMOD1 = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ ЮБ.", "КОСТЮМ");
                    strMOD2 = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ ЮБ.", "ЖЕНСКИЙ ЮБ.");
                } else if ("БРЮКИ ЖЕНСКИЕ" == strMOD) {
                    strMOD1 = strMOD.Replace("БРЮКИ ЖЕНСКИЕ", "БРЮКИ");
                    strMOD2 = strMOD.Replace("БРЮКИ ЖЕНСКИЕ", "ЖЕНСКИЕ");
                } else if ("ЖАКЕТ ЖЕНСКИЙ" == strMOD) {
                    strMOD1 = strMOD.Replace("ЖАКЕТ ЖЕНСКИЙ", "ЖАКЕТ");
                    strMOD2 = strMOD.Replace("ЖАКЕТ ЖЕНСКИЙ", "ЖЕНСКИЙ");
                } else if ("ЖИЛЕТ ЖЕНСКИЙ" == strMOD) {
                    strMOD1 = strMOD.Replace("ЖИЛЕТ ЖЕНСКИЙ", "ЖИЛЕТ");
                    strMOD2 = strMOD.Replace("ЖИЛЕТ ЖЕНСКИЙ", "ЖЕНСКИЙ");
                } else if ("КОСТЮМ ЖЕНСКИЙ С БРЮКАМИ" == strMOD) {
                    strMOD1 = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С БРЮКАМИ", "КОСТЮМ ЖЕНСКИЙ");
                    strMOD2 = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С БРЮКАМИ", "С БРЮКАМИ");
                } else if ("КОСТЮМ ЖЕНСКИЙ С ЮБКОЙ" == strMOD) {
                    strMOD1 = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С ЮБКОЙ", "КОСТЮМ ЖЕНСКИЙ");
                    strMOD2 = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С ЮБКОЙ", "С ЮБКОЙ");
                } else if ("ПЛАТЬЕ ЖЕНСКОЕ" == strMOD) {
                    strMOD1 = strMOD.Replace("ПЛАТЬЕ ЖЕНСКОЕ", "ПЛАТЬЕ");
                    strMOD2 = strMOD.Replace("ПЛАТЬЕ ЖЕНСКОЕ", "ЖЕНСКОЕ");
                }
                if (strMOD1.Length > 15 || strMOD2.Length > 15)
                    MessageBox.Show("Внимание длина названия более 15:" + strMOD);


                for (int z = 0; z < nKol; z++) {
                    string[] lines = File.ReadAllLines(strDestPathPaternSP, GetStickerEncoding());
                    for (int i = 0; i < lines.Length; i++) {
                        lines[i] = lines[i].Replace("#01", listKM[z].N.ToString("00000"));
                        lines[i] = lines[i].Replace("#02", strPrefix);
                        //lines[i] = lines[i].Replace("#03", txt03.Text);

                        if (bFlabelToBarcode || 13 == strFlabel.Length)
                            lines[i] = lines[i].Replace("#04", strFlabel);
                        else if(EAN13_TYPE.GTIN == _GetEan13Type())
                            lines[i] = lines[i].Replace("#04", strGTIN);
                        else
                            lines[i] = lines[i].Replace("#04", strBARCODE);

                        lines[i] = lines[i].Replace("#05", strMOD1.PadLeft(16));
                        if (strMOD1 != strMOD2)
                            lines[i] = lines[i].Replace("#32", strMOD2.PadLeft(16));


                        lines[i] = lines[i].Replace("#06", strART);
                        lines[i] = lines[i].Replace("#07", strART_second);
                        lines[i] = lines[i].Replace("#08", _CorrectRazm(strRAZ).PadLeft(15));
                        lines[i] = lines[i].Replace("#09", strART2);
                        //lines[i] = lines[i].Replace("#10", txt10.Text);
                        lines[i] = lines[i].Replace("#11", strCCODE);
                        //lines[i] = lines[i].Replace("#12", txt12.Text);
                        lines[i] = lines[i].Replace("#13", "");
                       
                        lines[i] = lines[i].Replace("#14", _markingPaths.GetCompanyName());

                        lines[i] = lines[i].Replace("#15", "Россия, 107078");
                        lines[i] = lines[i].Replace("#16", "Москва, Каланчевская 17");
                        //lines[i] = lines[i].Replace("#17", txt17.Text);
                        //lines[i] = lines[i].Replace("#18", txt18.Text);
                        lines[i] = lines[i].Replace("#19", listKM[z].strKN);

                        //if (85 != listKM[z].strKN.Length || 29 != (int)listKM[z].strKN[31] || 29 != (int)listKM[z].strKN[38] || "91" != listKM[z].strKN.Substring(32, 2) || "92" != listKM[z].strKN.Substring(39, 2))
                        if (85 != listKM[z].strKN.Length && "" != listKM[z].strKN) {
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
                        }
                        lines[i] = lines[i].Replace("#20", strCCLOTH1);
                        //lines[i] = lines[i].Replace("#28", strCCLOTH2);
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
                            strSiluet = "Клас.";
                        if (-1 != strCSEASON.LastIndexOf("0202"))
                            strSiluet = "Слим";
                        lines[i] = lines[i].Replace("#26", strSiluet);

                        string strRukava = "";
                        if (-1 != strCSEASON.LastIndexOf("0412"))
                            strRukava = "КР";
                        if (-1 != strCSEASON.LastIndexOf("0411"))
                            strRukava = "ДЛ";

                        lines[i] = lines[i].Replace("#27", strRukava);

                        lines[i] = lines[i].Replace("#29", strModel);

                        if (strSRT.Length > 1 && "0" == strSRT.Substring(0, 1))
                            strSRT = strSRT.Substring(1);

                        lines[i] = lines[i].Replace("#30", strSRT);

                        lines[i] = lines[i].Replace("#31", pp.strDate);//"10 2023");

                    }
                    if (nPrintedAll % 30 == 0 && 0 != nPrintedAll) {
                        DialogResult rez = MessageBox.Show(null, "Напечатали: " + nPrintedAll.ToString() + ", стикеров, продолжить?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (rez == DialogResult.No) {
                            Trace.WriteLine("Прервали печать");
                            return false;
                        }
                        if (!bPrintOnFile)
                            CheckStikers();
                    }
                    File.WriteAllLines(strNewFileName, lines, GetStickerEncoding()); //Encoding.GetEncoding(866));
                    SendTextFileToPrinter(strNewFileName);
                    //Trace.WriteLine("Напечатали стикер");
                    nPrintedAll++;
                }
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (bLastRow) {
                if (listKM.Count != 1 || "" != listKM[0].strKN)
                    CheckStikers();
                //else
                //печатаем без кода маркировки
            }

            return true;
        }
        private string _CorrectRazmAddNull(string strRAZ) {
            string[] parms = strRAZ.Split('-');
            if (parms.Length == 3) {
                if (2 == parms[0].Length)
                    parms[0] = "0" + parms[0];
                if (2 == parms[1].Length)
                    parms[1] = "0" + parms[1];
                if (2 == parms[2].Length)
                    parms[2] = "0" + parms[2];
                return parms[0] + "-" + parms[1] + "-" + parms[2];
            }
            return strRAZ;
        }
        public override void ImportInFlabel(string strFileName, string strOrderID) {

            if (null == _markingPaths) return;

            string strDestPath = _markingPaths.GetDestPath();
            string strSrcPath = _markingPaths.GetSrcPath();

        //    if (!CheckIfFileExist("ImportToStikers_003.dbf", ref strDestPath, ref strSrcPath, @"\ooo\vetex\generated\")) { this.Cursor = Cursors.Default; return; }

            DateTime dtSrcFileBegin = Win32.GetLastWriteTime(strSrcPath);
            try {
                int nIsIndexMDXNestand = -1;
                int nCodeOut = -1;
                DataTable dtTableDestination = Dbf.LoadDbfWithAddColumns(strDestPath, out nIsIndexMDXNestand, ref nCodeOut, "INO", strOrderID, null,false,false);
                DataTable dtTableWithBarcode = Dbf.LoadDbfWithAddColumns(strFileName, out nIsIndexMDXNestand, ref nCodeOut);
                if (dtTableDestination.Rows.Count != dtTableWithBarcode.Rows.Count) {
                    MessageBox.Show(null, "В таблицах разное число строк", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int BARCODE = 4;
                int PRS = 7;
                int MOD = 8;
                int ART = 9;
                int RAZ = 10;
                Dictionary<string, string> dicNomenclBarcodeBV = [];
                for (int i = 0; i < dtTableWithBarcode.Rows.Count; i++) {
                    string strBARCODE = (string)dtTableWithBarcode.Rows[i][BARCODE];
                    string strPRS = dtTableWithBarcode.Rows[i][PRS].ToString().Trim();
                    string strMOD = dtTableWithBarcode.Rows[i][MOD].ToString().Trim();
                    string strART = dtTableWithBarcode.Rows[i][ART].ToString().Trim();
                    string strRAZ = dtTableWithBarcode.Rows[i][RAZ].ToString().Trim();
                    string strKey = strART + " " + strRAZ + " " + strMOD + " " + strPRS;
                    dicNomenclBarcodeBV.Add(strKey, strBARCODE);
                }
                if (dtTableWithBarcode.Rows.Count != dicNomenclBarcodeBV.Count) {
                    MessageBox.Show(null, "В таблице Большевички задвоение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                PRS = 5;
                MOD = 6;
                ART = 7;
                RAZ = 8;
                for (int i = 0; i < dtTableDestination.Rows.Count; i++) {
                    string strPRS = dtTableDestination.Rows[i][PRS].ToString().Trim();
                    string strMOD = dtTableDestination.Rows[i][MOD].ToString().Trim();
                    string strART = dtTableDestination.Rows[i][ART].ToString().Trim();
                    string strRAZ = dtTableDestination.Rows[i][RAZ].ToString().Trim();
                    if (strRAZ.Length != 11)
                        strRAZ = _CorrectRazmAddNull(strRAZ);
                    string strKey = strART + " " + strRAZ + " " + strMOD + " " + strPRS;
                    //strBARCODE = 
                    if (!dicNomenclBarcodeBV.ContainsKey(strKey)) {
                        MessageBox.Show(null, "Есть несоответствие номенклатур", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                int N = 0;
                for (int i = 0; i < dtTableDestination.Rows.Count; i++) {
                    int nID = (int)dtTableDestination.Rows[i][N];
                    string strPRS = dtTableDestination.Rows[i][PRS].ToString().Trim();
                    string strMOD = dtTableDestination.Rows[i][MOD].ToString().Trim();
                    string strART = dtTableDestination.Rows[i][ART].ToString().Trim();
                    string strRAZ = dtTableDestination.Rows[i][RAZ].ToString().Trim();
                    if (strRAZ.Length != 11)
                        strRAZ = _CorrectRazmAddNull(strRAZ);
                    string strKey = strART + " " + strRAZ + " " + strMOD + " " + strPRS;
                    string strBARCODE = dicNomenclBarcodeBV[strKey];

                    DateTime dtOpenedFile = Win32.GetLastWriteTime(strDestPath);
                    if (!Dbf.SetValue(strDestPath, "№", nID.ToString(), "FLABEL", strBARCODE, dtOpenedFile)) {
                        Console.Beep();
                        MessageBox.Show("Ошибка записи в файл: " + strDestPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Trace.WriteLine("Ошибка записи в файл: " + strDestPath);
                        return;
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try {
                DateTime dtSrcFileEnd = Win32.GetLastWriteTime(strSrcPath);
                if (dtSrcFileBegin != dtSrcFileEnd) {
                    MessageBox.Show(null, "файл : " + strSrcPath + ", был изменен сторонней программой, обновится сейчас таблица, повторите ваши действия", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                File.Copy(strDestPath, strSrcPath, true);
            } catch (Exception ex) {
                MessageBox.Show(null, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(null, "Импорт прошел успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
                
        public override void Print(PrintParms pp, int nStcker = -1,
    string strBarcodePrint = "", string strCodeSpm = "", bool bFlabelToBarcode = false, bool bWithoutKM = false) {
            if (null == _markingPaths)
                return;

            string strDestPathKM = _markingPaths.GetDestPathKM();
            if ("" == strCodeSpm.Trim())
                strCodeSpm = _GetShopPrefix();

            string strDestPathPaternSP = "";
            string strSrcPathPaternSP = "";
            if (!DbfWrapper.CheckIfFileExist(GetZplName(), ref strDestPathPaternSP, ref strSrcPathPaternSP, @"\1c\Matrix\patern\")) return;

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

                string strDestPathSoputka = _markingPaths.GetDestPath();
                //string strSrcPathSoputka = _markingPaths.GetSrcPath();
                //string strFileName = _markingPaths.GetProductsFileName() ;

                DataTable dtTableSoputka = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "BARCODE", strBarcode);
                if (0 == dtTableSoputka.Rows.Count) {
                    MessageBox.Show(null, "Нет штрихкода: " + strBarcode + " В файле: " + strDestPathSoputka, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                List<KM> listKM = [];
                listKM.Add(new KM(nStcker, strKM));

                int GTIN = 23;
                dtTableSoputka.Rows[0][GTIN] = strGTIN;

                _PrintRow(pp, dtTableSoputka.Rows[0], strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, false, false, bFlabelToBarcode);
            } else if (strBarcodePrint != "") {
                //string strColumnNameBarcode = "BARCODE";
                //if (product == .PR_VETEX_IMPORT_SOPUTKA)
                //    strColumnNameBarcode = "GTIN";

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
                        string strPrefixFileWithData = _markingPaths.GetProductsFileName().Split('_')[0] + "_";
                        string strFileName = strPrefixFileWithData + strCodeSpm + ".dbf";
                        string strRootFolder = _markingPaths.GetGeneratedFolder();
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
                        _PrintRow(pp, dtTableSoputka.Rows[0], strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, false, false, bFlabelToBarcode);                      
                        }
                    } catch (Exception) { return; }
                    } 
                else {
                bool bLastRow = false;
                int nPos = 0;
                try { 
                    foreach (DataGridViewRow gvRow in _dataGridView.SelectedRows) {
                        nPos++;
                        if (nPos == _dataGridView.SelectedRows.Count)
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

    }
}
