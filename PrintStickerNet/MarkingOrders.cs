using DbfLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public class MarkingOrders(MarkingPaths markingPaths, string strOrderType) {

        private readonly MarkingPaths _markingPaths = markingPaths;
        private readonly string _strOrderType = strOrderType;
        
        public bool ShowDescription(IWin32Window parent, string strName, out string strOut) {
            if (strName.Length > 19)
                strName = strName.Substring(0, 19);
            strOut = "";
            if (19 != strName.Length)
                return false;

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_markingPaths.GetOrdersFileName(), ref strDestPath, ref strSrcPath, _markingPaths.GetMainFolder()))
                return false;

            int nCodeOut = -1;
            DataTable dtTableOrders = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "NAME", strName);
            if (0 == dtTableOrders.Rows.Count)
                return false;
            int DESCR = 5;
            string strDesc = "";
            if (System.DBNull.Value != dtTableOrders.Rows[0][DESCR])
                strDesc = dtTableOrders.Rows[0][DESCR].ToString();

            DlgDescription dlg = new();
            dlg.SetText(strDesc);
            if (dlg.ShowDialog(parent) != DialogResult.OK)
                return false;

            if (dlg.GetText() == strDesc)
                return false;
            strDesc = dlg.GetText();
            string strPath = _markingPaths.GetSrcPathOrders();
            if (!Dbf.SetValue(strPath, "NAME", strName, "DESCR", strDesc, Win32.GetLastWriteTime(strPath))) {
                Console.Beep();
                MessageBox.Show("Ошибка записи в файл: " + strPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Trace.WriteLine("Ошибка записи в файл: " + strPath);
                return false;
            }
            strOut = strDesc;
            return true;
        }

        public bool ReturnHidenOrders(IWin32Window parent) {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_markingPaths.GetOrdersFileName(), ref strDestPath, ref strSrcPath, _markingPaths.GetMainFolder()))
                return false;

            int nCodeOut = -1;
            DataTable dtTableOrders = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "VISIBLE", "0", null, false, false);
            int NAME = 3;
            int TYPE = 4;
            int DESCR = 5;
            List<string> listNames = [];
            for (int i = 0; i < dtTableOrders.Rows.Count; i++) {
                if (_strOrderType != dtTableOrders.Rows[i][TYPE].ToString())
                    continue;

                    string strDesc = "";
                if (System.DBNull.Value != dtTableOrders.Rows[i][DESCR])
                    strDesc = dtTableOrders.Rows[i][DESCR].ToString();

                if ("" != strDesc)
                    listNames.Add(dtTableOrders.Rows[i][NAME].ToString() + " (" + strDesc.Trim() + ")");
                else
                    listNames.Add(dtTableOrders.Rows[i][NAME].ToString());
            }
            listNames.Sort();
            //
            List<string> listHideItems = [];
            for (int i = listNames.Count - 1; i >= 0; i--)
                listHideItems.Add(listNames[i]);

            DlgHidenOrders dlg = new();
            dlg.Add(listHideItems);
            if (dlg.ShowDialog(parent) != DialogResult.OK)
                return false;
            listHideItems = dlg.GetSelItems();
            if (0 == listHideItems.Count)
                return false;
            foreach (string strVal in listHideItems) {
                string strName = strVal;
                int nPos = strVal.IndexOf('(');
                if (-1 != nPos)
                    strName = strVal.Substring(0, nPos).Trim();

                _SetVisibleOrder(strName, "1");
            }
            return true;
        }
        public bool HideOrder(string strName) {
            return _SetVisibleOrder(strName, "0");
        }
        private bool _SetVisibleOrder(string strName, string strVal) {
            if (strName.Length > 19)
                strName = strName.Substring(0, 19);

            string strPath = _markingPaths.GetSrcPathOrders();
            if (!Dbf.SetValue(strPath, "NAME", strName, "VISIBLE", strVal, Win32.GetLastWriteTime(strPath))) {
                Console.Beep();
                MessageBox.Show("Ошибка записи в файл: " + strPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Trace.WriteLine("Ошибка записи в файл: " + strPath);
                return false;
            }
            return true;
        }

        public bool AddOrder(string strName, out string strID) {
            if (!_AddOrderToDB(_markingPaths.GetDestPathOrders(), _markingPaths.GetSrcPathOrders(), strName, out strID))
                return false;
            return true;
        }
        private bool _AddOrderToDB(string strDest, string strSrc, string strName, out string strID) {
            int NAME = 3;
            int TYPE = 4;
            int VISIBLE = 6;
            int INO = 7;

            int nMax = 0;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDest, out _, ref nCodeOut);
            for (int i = 0; i < dtTable.Rows.Count; i++)
                nMax = Math.Max(nMax, Convert.ToInt32(dtTable.Rows[i][INO]));
            dtTable.Rows.Clear();

            DataRow rowAdd = dtTable.NewRow();
            rowAdd[NAME] = strName;
            rowAdd[TYPE] = _strOrderType;
            rowAdd[VISIBLE] = 1;
            strID = (1 + nMax).ToString();
            rowAdd[INO] = strID;

            dtTable.Rows.Add(rowAdd);

            if (!DbfWrapper.AddDoDB(null,strDest, strSrc, dtTable))
                return false;
            return true;
        }

    }
}
