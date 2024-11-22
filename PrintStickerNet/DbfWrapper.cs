using DbfLib;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Diagnostics;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    internal class DbfWrapper {
        //private static readonly string _strRootFolder = @"\\192.168.0.199\ps\";
        private static readonly string _strRootFolder = "x:\\";

        private static string _GetFolderOnType() {
            return "1c";
        }
        private static string _GetCurDir() {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }
        private static DateTime _GetLastWriteTime(string strPath) {
            while (true) {
                DateTime dt = Win32.GetLastWriteTime(strPath);
                if (1929 != dt.Year)
                    return dt;
                Trace.WriteLine("Sleep: 1000 ms");
                System.Threading.Thread.Sleep(1000);
            }
        }
        public static string GetRootDisk() {
            return _strRootFolder;
        }
        public static bool CheckIfFileExist(string strFileName, ref string strDestPath, ref string strSrcPath, string strFolderPath = "", bool bMakeArhive = false) {
            //Cursor curBefore = this.Cursor;
            if (!Directory.Exists(_strRootFolder)) {
                MessageBox.Show("Отсутствует путь: " + _strRootFolder + ", Зайдите через проводник и проверьте доступ к диску, папке", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            string strTypeFolder = strFolderPath;
            if ("" == strTypeFolder)
                strTypeFolder = _GetFolderOnType();
            strSrcPath = _strRootFolder + "\\" + strTypeFolder + "\\" + strFileName;
            string strCurDir = _GetCurDir() + "\\tmpDbf\\";
            strDestPath = strCurDir + "\\" + strTypeFolder + "\\" + strFileName;
            Directory.CreateDirectory(strCurDir + "\\" + strTypeFolder);

            if (!File.Exists(strSrcPath)) {
                //MessageBox.Show("Отсутствует файл: " + strSrcPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            if (!File.Exists(strDestPath)) {
                //MessageBox.Show("Отсутствует файл: " + strDestPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return false;
            }                      

            //lock (_lockerDbfFile) 
                {
                if (!File.Exists(strDestPath)) {
                    File.Copy(strSrcPath, strDestPath);
                    //WriteLog("File.Copy2 [первый раз] : " + strSrcPath + " to " + strDestPath);
                } else {
                    long diffInSeconds = (long)Math.Abs((_GetLastWriteTime(strSrcPath) - _GetLastWriteTime(strDestPath)).TotalSeconds);
                    if (diffInSeconds > 2) {
                        File.Copy(strSrcPath, strDestPath, true);
                        //WriteLog("File.Copy2 [diff time=" + diffInSeconds.ToString() + "] : " + strSrcPath + " to " + strDestPath);
                    } else {
                        long lengthSrc = new System.IO.FileInfo(strSrcPath).Length;
                        long lengthDest = new System.IO.FileInfo(strDestPath).Length;
                        if (lengthSrc != lengthDest) {
                            File.Copy(strSrcPath, strDestPath, true);
                            //WriteLog("File.Copy2 [diff size] : " + strSrcPath + " to " + strDestPath);
                        }
                    }
                }
                if (bMakeArhive)
                    GenLogic.CopyToArhive(strDestPath);
            }
            return true;
        }
        public static bool AddDoDB(Form parent, string strDestPath, string strSrcPath, DataTable dtTable) {
            if (null == dtTable || 0 == dtTable.Rows.Count)
                return false;
            if (!File.Exists(strDestPath) || !File.Exists(strSrcPath))
                return false;

            if (!Dbf.AddTable(strDestPath, dtTable)) {
                MessageBox.Show(parent, "Ошибка не удалось добавить в базу данных новые GTIN и номера заказов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestPath, strSrcPath, true);
            } catch (Exception ex) {
                MessageBox.Show(parent, "Ошибка " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        public static bool ImportGtinFromExcel(Form parent, string strFileName, string strDestSoputkaGtin, string strSrcSoputkaGtin, string strCompanyNmae) {
            ExcelExport excExp = new();
            if (!excExp.Open(strFileName, out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(parent, "Произошла ошибка при открытии файла:" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //List<FromExcelItem> listFromExcelItems;
            if (!excExp.GetData(xlsWorkSheet, strCompanyNmae, out List<FromExcelItem> listFromExcelItems)) {
                MessageBox.Show(parent, "Произошла ошибка при получении данных из файла:" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (listFromExcelItems.Count == 0) {
                MessageBox.Show(parent, "в Excel не найдено GTIN", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            int nCodeOut = -1;
            DataTable dtTableSoputkaGtin = Dbf.LoadDbfWithAddColumns(strDestSoputkaGtin, out _, ref nCodeOut);

            int GTIN = 3;
            int NOMENCL = 4;
            int TNVED = 5;
            Dictionary<long, string> dicGtinNomencl = [];
            for (int i = 0; i < dtTableSoputkaGtin.Rows.Count; i++) {
                long llGTIN = (long)dtTableSoputkaGtin.Rows[i][GTIN];
                string strNomencl = (string)dtTableSoputkaGtin.Rows[i][NOMENCL];
                dicGtinNomencl.Add(llGTIN, strNomencl);
            }
            //
            dtTableSoputkaGtin.Rows.Clear();
            //
            foreach (FromExcelItem item in listFromExcelItems) {
                if (dicGtinNomencl.ContainsKey(item.llColumn2_GTIN) && dicGtinNomencl[item.llColumn2_GTIN] == item.strColumn5_ProductName)
                    continue;
                if (dicGtinNomencl.ContainsKey(item.llColumn2_GTIN)) {
                    MessageBox.Show(parent, "ОШИБКА, GTIN: " + item.llColumn2_GTIN + ", существует в файле: " + strSrcSoputkaGtin + ", но номенклатуры отличаются в файле и в EXCEL", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                System.Data.DataRow rowAdd = dtTableSoputkaGtin.NewRow();
                //rowAdd[0] = 0;
                //rowAdd[1] = 0;
                rowAdd[GTIN] = item.llColumn2_GTIN;
                rowAdd[NOMENCL] = item.strColumn5_ProductName;
                rowAdd[TNVED] = item.llColumn12_TNVED;
                dtTableSoputkaGtin.Rows.Add(rowAdd);
            }
            if (dtTableSoputkaGtin.Rows.Count == 0) {
                MessageBox.Show(parent, "Новых GTIN не найдено (GTIN из Excel уже есть в БД: " + strSrcSoputkaGtin + ")", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (!Dbf.AddTable(strDestSoputkaGtin, dtTableSoputkaGtin)) {
                MessageBox.Show(parent, "Ошибка не удалось добавить в базу данных новые GTIN", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestSoputkaGtin, strSrcSoputkaGtin, true);
                MessageBox.Show(parent, "Добавлено: " + dtTableSoputkaGtin.Rows.Count + " кодов GTIN в файл: " + strSrcSoputkaGtin, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                MessageBox.Show(parent, "Ошибка " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public static bool GetGtinsBv(ref Dictionary<string, long> dicNomenclGtin, MarkingPaths markingPaths = null) {
            int nCodeOut = -1;

            string strDestPathGTIN = "";
            string strSrcPathGTIN = "";
            if (null == markingPaths) {
                if (!DbfWrapper.CheckIfFileExist("BvGtin.dbf", ref strDestPathGTIN, ref strSrcPathGTIN, @"\Bv\")) { return false; }
            } else
                strDestPathGTIN = markingPaths.GetDestPathGtin();

            DataTable dtTableGTIN = Dbf.LoadDbfWithAddColumns(strDestPathGTIN, out _, ref nCodeOut);
            if (0 == dtTableGTIN.Rows.Count) {
                Trace.WriteLine("Ошибка, нет GTIN, в файле: " + strDestPathGTIN);
                return false;
            }
            int GTIN = 3;
            int NOMENCL = 4;
            for (int i = 0; i < dtTableGTIN.Rows.Count; i++) {
                long llGTIN = (long)dtTableGTIN.Rows[i][GTIN];
                string strNomencl = (string)dtTableGTIN.Rows[i][NOMENCL];
                dicNomenclGtin.Add(strNomencl, llGTIN);
            }
            return true;
        }
        public static bool GetGtinsSoputka(ref Dictionary<string, long> dicNomenclGtin, MarkingPaths markingPaths = null) {
            int nCodeOut = -1;

            string strDestPathGTIN = "";
            string strSrcPathGTIN = "";

            if (null != markingPaths)
                strDestPathGTIN = markingPaths.GetDestPathGtin();
            else {
                if (!DbfWrapper.CheckIfFileExist("SoputkaGtin.dbf", ref strDestPathGTIN, ref strSrcPathGTIN, @"\Soputka\")) { return false; }
            }
            DataTable dtTableGTIN = Dbf.LoadDbfWithAddColumns(strDestPathGTIN, out _, ref nCodeOut);
            if (0 == dtTableGTIN.Rows.Count) {
                Trace.WriteLine("Ошибка, нет GTIN, в файле: " + strDestPathGTIN);
                return true;
            }
            int GTIN = 3;
            int NOMENCL = 4;
            for (int i = 0; i < dtTableGTIN.Rows.Count; i++) {
                long llGTIN = (long)dtTableGTIN.Rows[i][GTIN];
                string strNomencl = (string)dtTableGTIN.Rows[i][NOMENCL];
                dicNomenclGtin.Add(strNomencl, llGTIN);
            }
            return true;
        }

    }
}
