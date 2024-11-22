using DbfLib;
using System;
using System.Collections.Generic;
using System.Data;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker.MarkingObjectsBase {
    public class Composition() {
        private static List<string> _listCompos = null;
        private static string _strSrcPathCompos = "";
        private static DateTime _dtLastEditFile = new(1929, 1, 1);//"DbComposition.dbf"
        private static Dictionary<string, string> _dicShortFullCompos = null;
         public Dictionary<string, string> GetDicMaterials() {
            if (null != _dicShortFullCompos) {
                DateTime dt = Win32.GetLastWriteTime(_strSrcPathCompos);
                if (_dtLastEditFile != dt)
                    _LoadCompos();
            }

            if (null == _dicShortFullCompos)
                _LoadCompos();

            return _dicShortFullCompos;
        }
         public  List<string> GetListMaterials() {            
            if (null != _listCompos) {
                DateTime dt = Win32.GetLastWriteTime(_strSrcPathCompos);
                if (_dtLastEditFile != dt)
                    _LoadCompos();
            }

            if (null == _listCompos)
                _LoadCompos();
            return _listCompos;
        }
        public string GetMaterial(string strShort) {
            List<string> list = GetListMaterials();
            for (int i = 0; i < list.Count; i++) {
                int nPos1 = list[i].LastIndexOf("(" + strShort + ")");
                if (-1 != nPos1)
                    return list[i].Substring(0, nPos1);
            }
            return strShort;            
        }
        public string GetCodeMaterial(string strVal) {
            int nPos1 = strVal.IndexOf('(');
            int nPos2 = strVal.IndexOf(')');
            if (-1 == nPos1 || -1 == nPos2 || nPos1 >= nPos2)
                return "";
            return strVal.Substring(1 + nPos1, nPos2 - nPos1 - 1);
        }
        private void _LoadCompos() {
            string strDestPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbComposition.dbf", ref strDestPath, ref _strSrcPathCompos, "\\ooo\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");

            _dtLastEditFile = Win32.GetLastWriteTime(_strSrcPathCompos);

            _listCompos = [];
            _listCompos.Add("");
            _dicShortFullCompos = [];
            int NAME = 2;
            int SHORT = 3;
            for (int i = 0; i < dtTable.Rows.Count; i++) {
                string strName = (string)dtTable.Rows[i][NAME];
                string strShort = (string)dtTable.Rows[i][SHORT];
                _listCompos.Add(strName + "(" + strShort + ")");
                if(!_dicShortFullCompos.ContainsKey(strShort))
                    _dicShortFullCompos.Add(strShort, strName);
            }
        }
    }
}
