using DbfLib;
using System;
using System.Collections.Generic;
using System.Data;


namespace PrintSticker.MarkingObjectsBase {
    internal class ColorBV() {

        private static List<string> _listColors = null;
        private static List<string> _listShortColors = null;
        private static string _strSrcPathColor = "";
        private static DateTime _dtLastEditFile = new(1929, 1, 1);//"DbColorBV.dbf"


        public string GetNameColorWithCode(int nColor) {
            List<string> list = GetListColor();
            for (int i = 0; i < list.Count; i++) {
                int nPos1 = list[i].LastIndexOf("(" + nColor.ToString() + ")");
                if (-1 != nPos1)
                    return list[i];
            }
            return nColor.ToString();
        }

        public int GetCodeColor(string strVal) {
            int nPos1 = strVal.IndexOf('(');
            int nPos2 = strVal.IndexOf(')');
            if (-1 == nPos1 || -1 == nPos2 || nPos1 >= nPos2) {
                if (int.TryParse(strVal, out int nColor))
                    return nColor;
                return -1;
            }
            return Convert.ToInt32(strVal.Substring(1 + nPos1, nPos2 - nPos1 - 1));
        }
        public string GetNameColor(int nColor) {
            List<string> list = GetListColor();
            for (int i = 0; i < list.Count; i++) {
                int nPos1 = list[i].LastIndexOf("(" + nColor.ToString() + ")");
                if (-1 != nPos1)
                    return list[i].Substring(0, nPos1);
            }
            return nColor.ToString();
        }
        public string GetNameShortColor(int nColor) {
            List<string> list = GetListColor();
            for (int i = 0; i < list.Count; i++) {
                int nPos1 = list[i].LastIndexOf("(" + nColor.ToString() + ")");
                if (-1 != nPos1) {
                    if ("" != _listShortColors[i])
                        return _listShortColors[i];
                    return list[i].Substring(0, nPos1);
                }
            }
            return nColor.ToString();
        }
        public List<string> GetListColor() {           
            if (null != _listColors) {
                DateTime dt = Win32.GetLastWriteTime(_strSrcPathColor);
                if (_dtLastEditFile != dt)
                    _LoadColors();
            }
            if (null == _listColors)
                _LoadColors();
            return _listColors;
        }
        private void _LoadColors() {
            string strDestPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbColorBV.dbf", ref strDestPath, ref _strSrcPathColor, "\\ooo\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");

            _dtLastEditFile = Win32.GetLastWriteTime(_strSrcPathColor);

            _listColors = [];
            _listShortColors = [];
            int NCOLOR = 2;
            int COLOR = 3;
            int SHORT = 4;
            string strShort;
            for (int i = 0; i < dtTable.Rows.Count; i++) {
                string strNum = (string)dtTable.Rows[i][NCOLOR];
                string strColor = (string)dtTable.Rows[i][COLOR];
                _listColors.Add(strColor + "(" + strNum + ")");

                strShort = "";
                if (System.DBNull.Value != dtTable.Rows[i][SHORT])
                    strShort = (string)dtTable.Rows[i][SHORT];
                _listShortColors.Add(strShort);
            }
        }
    }
}
