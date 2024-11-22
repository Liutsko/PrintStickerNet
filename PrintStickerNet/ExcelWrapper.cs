using DbfLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace PrintSticker {
    internal class ExcelWrapper {      
        public static void ExportToExcelNomenkl(List<(string name, string barcode)> listNomenkl) {
            ExcelExport excExp = new();
            if (!excExp.Create("Номенклатура", out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(null, "Произошла ошибка при экспорте", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            System.GC.Collect();
            if (!excExp.ExportNomenkl(xlsWorkSheet, ref listNomenkl)) {
                MessageBox.Show(null, "Произошла ошибка при экспорте", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(null, "Экспорт прошел успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;

        }
        public static void ExportToExcelKM(Form parent, string strDestPathKM, HashSet<string> hsBarcodes, int nLenKM) {
            List<string> listKN = [];
            int KM = 5;
            int BARCODE = 10;
            int nCodeOut = -1;
            DataTable dtTableKM = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableKM.Rows.Count; i++) {
                string strBarcode = dtTableKM.Rows[i][BARCODE].ToString();
                if (!hsBarcodes.Contains(strBarcode))
                    continue;
                if(nLenKM > 0 && nLenKM <= 83)
                    listKN.Add(dtTableKM.Rows[i][KM].ToString()[..nLenKM]);
                else
                    listKN.Add(dtTableKM.Rows[i][KM].ToString());
            }
            ExcelExport excExp = new();
            if (!excExp.Create("KM", out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(parent, "Произошла ошибка при экспорте", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            System.GC.Collect();
            if (!excExp.ExportKM(xlsWorkSheet, ref listKN)) {
                MessageBox.Show(parent, "Произошла ошибка при экспорте", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(parent, "Экспорт прошел успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
    }
}
