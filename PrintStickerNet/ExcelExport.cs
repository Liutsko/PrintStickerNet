using Microsoft.Office.Interop.Excel;

using Range = Microsoft.Office.Interop.Excel.Range;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым
#pragma warning disable CA2249 //Используйте "string.Contains" вместо "string.IndexOf"

namespace PrintSticker
{
    public class ExcelExport
    {
        private string _strPath = "";
        private Microsoft.Office.Interop.Excel.Application _xlsApp = null;
        private Microsoft.Office.Interop.Excel.Workbook _xlsWorkBook = null;

        public bool Create(string title, out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet) {
            bool bRezOK = true;
            Microsoft.Office.Interop.Excel.Application xlsApp = null;
            xlsWorkSheet = null;
            try { xlsApp = new Microsoft.Office.Interop.Excel.Application(); } catch (Exception) {
                MessageBox.Show(null, "Не могу создать Excel документ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                bRezOK = false;
                return false;
            }
            try {
                Microsoft.Office.Interop.Excel.Workbook xlsWorkBook = xlsApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                xlsWorkSheet = (Worksheet)xlsWorkBook.Worksheets.get_Item(1);
                if (title.Length > 31)
                    title = title[..31];

                xlsWorkSheet.Name = title;
                xlsApp.Visible = true;
            } catch (Exception) {
                bRezOK = false;
            }
            return bRezOK;
        }

        public bool Open(string strPath, out Worksheet xlsWorkSheet)
        {
            _strPath = strPath;
            bool bRezOK = true;
            xlsWorkSheet = null;
            try
            { _xlsApp = new Microsoft.Office.Interop.Excel.Application(); }
            catch (Exception) {
                MessageBox.Show(null, "Не могу создать Excel документ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                bRezOK = false;
                return false;
            }
            try {
                _xlsWorkBook = _xlsApp.Workbooks.Open(strPath);
                xlsWorkSheet = (Worksheet)_xlsWorkBook.Worksheets.get_Item(1);
                //xlsApp.Visible = true;
                
            }
            catch (Exception) {
                bRezOK = false;
            }            
            return bRezOK;
        }
        public bool GetData(Worksheet xlsWorkSheet, string strCompanyNmae,  out List<FromExcelItem> listFromExcelItems) {
            listFromExcelItems = [];
            try {
                int y = 6;
                while (true) {
                    y++;
                    //((Range)xlsWorkSheet.Cells[y, 5]).Value;
                    if (null == ((Range)xlsWorkSheet.Cells[y, 5]).Value)
                        break;
                    string strValueCol5 = ((Range)xlsWorkSheet.Cells[y, 5]).Value2.ToString();
                    if (null == strValueCol5)
                        break;

                    string strCN = ((Range)xlsWorkSheet.Cells[y, 9]).Value2.ToString();
                    if (strCN != strCompanyNmae)
                        continue;

                    string strValueCol30 = ((Range)xlsWorkSheet.Cells[y, 30]).Value2.ToString();
                    if ("OK" != strValueCol30)
                        continue;

                    string strValueCol2 = ((Range)xlsWorkSheet.Cells[y, 2]).Value2.ToString();
                    string strValueCol12 = ((Range)xlsWorkSheet.Cells[y, 12]).Value2.ToString();
                    FromExcelItem fei = new() {
                        llColumn2_GTIN = Convert.ToInt64(strValueCol2),
                        llColumn12_TNVED = Convert.ToInt64(strValueCol12),
                        strColumn5_ProductName = strValueCol5
                };
                    listFromExcelItems.Add(fei);                    
                }
                _xlsWorkBook.Close();
            }
            catch (Exception ex) {
                MessageBox.Show(null, "Ошибка в Импорт из Excel:" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
            
        }
        private bool _IsInt(string strValur) {
            try {
                int tt = Convert.ToInt32(strValur);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }
        private bool _IsDouble(string strValur) {
            try {
                double tt = Convert.ToDouble(strValur);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }

        private string _GetRazmerType(ToExcelItem item) {            
            string[] parms = item.strColumn14_Razmer.Split('-');
            if (-1 != item.strColumn10_SortProduct.IndexOf("КОСТЮМ") ||
                -1 != item.strColumn10_SortProduct.IndexOf("БРЮКИ") ||
                -1 != item.strColumn10_SortProduct.IndexOf("ПИДЖАК")) {
                if (3 == parms.Length || 4 == parms.Length)
                    return "<1764000024> РОСТ-ОГ-ОТ";
                if (2 == parms.Length) {
                    item.strColumn14_Razmer = parms[1];
                    return "<1764000001> РОСТ";
                }
            }
            if (-1 != item.strColumn10_SortProduct.IndexOf("РУБАШКА")) //СОРОЧКА
            {
                if (1 == parms.Length) {
                    item.strColumn14_Razmer = parms[0];

                    if (!_IsInt(item.strColumn14_Razmer))
                        return "<1764000037> МЕЖДУНАРОДНЫЙ";
                    MessageBox.Show(null, "Размер <1764000002> ОШ в Честном Знаке не проходил раньше", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return "<1764000002> ОШ";
                }
                if (2 == parms.Length) {
                    string[] parms2 = parms[1].Split(' ');
                    if (2 == parms2.Length)
                        item.strColumn14_Razmer = parms[0] + "-" + parms2[1];
                }

                return "<1764000016> РОСТ-ОШ";
            }
            
            if (-1 != item.strColumn10_SortProduct.IndexOf("ЮБКА") ) {
                if(3 == parms.Length)
                    return "<1764000026> РОСТ-ОТ-ОБ";
                if (2 == parms.Length) {
                    item.strColumn14_Razmer = parms[1];
                    return "<1764000001> РОСТ";
                }
            }

            if (-1 != item.strColumn10_SortProduct.IndexOf("БРЮКИ")) {
                if (_IsInt(item.strColumn14_Razmer)) {
                    int nCount = Convert.ToInt32(item.strColumn14_Razmer);
                    if (nCount > 70 || nCount < 35)
                        return "<1764000034> ЕВРОПА";
                    else
                        return "<1764000029> РОССИЯ";
                }
                if (_IsDouble(item.strColumn14_Razmer.Replace('.',',')))
                    return "<1764000034> ЕВРОПА";                    
            }


            if (-1 != item.strColumn10_SortProduct.IndexOf("ДЖЕМПЕР") || -1 != item.strColumn10_SortProduct.IndexOf("ЖАКЕТ") ||
                -1 != item.strColumn10_SortProduct.IndexOf("ЖИЛЕТ") || -1 != item.strColumn10_SortProduct.IndexOf("СВИТЕР") ||
                -1 != item.strColumn10_SortProduct.IndexOf("БЛУЗА") || -1 != item.strColumn10_SortProduct.IndexOf("БЛУЗКА") ||
                -1 != item.strColumn10_SortProduct.IndexOf("ПЛАТЬЕ") || -1 != item.strColumn10_SortProduct.IndexOf("ТУНИКА") ||
                -1 != item.strColumn10_SortProduct.IndexOf("ПОЛО"))
            {
                if(3 == parms.Length)
                    return "<1764000025> РОСТ-ОГ-ОБ";
                if (2 == parms.Length)
                    return "<1764000029> РОССИЯ";
                if(!_IsInt(item.strColumn14_Razmer))
                    return "<1764000037> МЕЖДУНАРОДНЫЙ";

                return "<1764000029> РОССИЯ";
            }
            if (-1 != item.strColumn10_SortProduct.IndexOf("ГАЛСТУК")) {
                item.strColumn14_Razmer = "155-7";
                item.strColumn14_Razmer = "155-9";
                return "<1764000042> ДЛИНА-ШИРИНА";
            }
            if (!_IsInt(item.strColumn14_Razmer))
                return "<1764000037> МЕЖДУНАРОДНЫЙ";            
            return "<1764000029> РОССИЯ";
        }
        
        public bool ExportNomenkl(Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet, ref List<(string name, string barcode)> listNomenkl) {
            try {
                int y = 1;
                ((Microsoft.Office.Interop.Excel.Range)xlsWorkSheet.Columns[1, Type.Missing]).ColumnWidth = 80;
                ((Microsoft.Office.Interop.Excel.Range)xlsWorkSheet.Columns[2, Type.Missing]).ColumnWidth = 20;
                foreach (var (name, barcode) in listNomenkl) {
                    ((Range)xlsWorkSheet.Cells[y, 1]).Value2 = name;
                    ((Range)xlsWorkSheet.Cells[y, 2]).Value2 = barcode;
                    ((Range)xlsWorkSheet.Cells[y, 2]).NumberFormat = "# ?/?";
                    y++;
                }
            } catch (Exception) {
                return false;
            }
            return true;
        }

        public bool ExportKM(Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet, ref List<string> listKN) {
            try {
                int y = 1;
                foreach (string strKM in listKN) {
                    ((Range)xlsWorkSheet.Cells[y, 1]).Value2 = strKM;
                    y++;
                }
            } catch (Exception) {
                return false;
            }
            return true;
        }
        public bool ExportData(Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet, ref List<ToExcelItem> listToExcelItems) {
            try {
                int y = 7;
                string strDate = DateTime.Now.ToString("dd.MM.yyyy");
                foreach (ToExcelItem item in listToExcelItems) {
                    ((Range)xlsWorkSheet.Cells[y, 4]).Value2 = strDate;
                    ((Range)xlsWorkSheet.Cells[y, 5]).Value2 = item.strColumn5_ProductName;

                    if ("" == item.strColumn6_Country.Trim())
                        item.strColumn6_Country = "ОТСУТСТВУЕТ";
                    ((Range)xlsWorkSheet.Cells[y, 6]).Value2 = item.strColumn6_Country;
                    //((Range)xlsWorkSheet.Cells[y, 6]).Value2 = "ОТСУТСТВУЕТ";
                    ((Range)xlsWorkSheet.Cells[y, 7]).Value2 = item.strColumn7_Country;                                   
                    ((Range)xlsWorkSheet.Cells[y, 8]).Value2 = item.strColumn8_INN;
                    ((Range)xlsWorkSheet.Cells[y, 9]).Value2 = item.strColumn9_IZGOTOVITEL;
                    ((Range)xlsWorkSheet.Cells[y, 10]).Value2 = item.strColumn10_SortProduct;

                    ((Range)xlsWorkSheet.Cells[y, 12]).Value2 = item.strColumn12_TNVED;
                    ((Range)xlsWorkSheet.Cells[y, 13]).Value2 = _GetRazmerType(item);
                    ((Range)xlsWorkSheet.Cells[y, 14]).Value2 = item.strColumn14_Razmer;
                    ((Range)xlsWorkSheet.Cells[y, 15]).Value2 = item.strColumn15_Rost;
                    ((Range)xlsWorkSheet.Cells[y, 16]).Value2 = item.strColumn16_Cvet;
                    ((Range)xlsWorkSheet.Cells[y, 17]).Value2 = item.strColumn17_Mod;

                    int nPos = item.strColumn5_ProductName.LastIndexOf("ЖЕН");
                    int nPos2 = item.strColumn10_SortProduct.LastIndexOf("ЮБКА");
                    int nPos3 = item.strColumn10_SortProduct.LastIndexOf("ПЛАТЬЕ");
                    if (-1 != nPos || -1 != nPos2 || -1 != nPos3)
                        ((Microsoft.Office.Interop.Excel.Range)xlsWorkSheet.Cells[y, 18]).Value2 = "<1200000002> ЖЕНСКИЙ";
                    else
                        ((Range)xlsWorkSheet.Cells[y, 18]).Value2 = "<1200000001> МУЖСКОЙ";                  

                    ((Range)xlsWorkSheet.Cells[y, 19]).Value2 = item.strColumn19_Sostav;

                    ((Range)xlsWorkSheet.Cells[y, 20]).Value2 = "НЕТ";
                    ((Range)xlsWorkSheet.Cells[y, 21]).Value2 = "ДА";
                    ((Range)xlsWorkSheet.Cells[y, 22]).Value2 = "НЕТ";
                    ((Range)xlsWorkSheet.Cells[y, 23]).Value2 = "НЕТ";
                    ((Range)xlsWorkSheet.Cells[y, 24]).Value2 = "НЕТ";

                    ((Range)xlsWorkSheet.Cells[y, 25]).Value2 = "НЕТ";

                    int nPos10 = item.strColumn5_ProductName.LastIndexOf("КОСТЮМ");
                    int nPos10_2 = item.strColumn5_ProductName.LastIndexOf("КОМПЛЕКТ");
                    if (-1 != nPos10 || -1 != nPos10_2) {
                        item.strColumn26_Komplect = "Да";
                        item.strColumn27_CountInKomplect = "2";
                        item.strColumn28_DescriptionKomplect = "ПИДЖАК (1шт.); БРЮКИ (1шт.)";
                    }

                    ((Range)xlsWorkSheet.Cells[y, 27]).Value2 = item.strColumn26_Komplect;
                    ((Range)xlsWorkSheet.Cells[y, 28]).Value2 = item.strColumn27_CountInKomplect;
                    ((Range)xlsWorkSheet.Cells[y, 29]).Value2 = item.strColumn28_DescriptionKomplect;

                    y++;
                }
                _xlsWorkBook.Save();
                _xlsWorkBook.Close();                
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
