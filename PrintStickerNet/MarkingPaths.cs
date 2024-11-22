using DbfLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Security.Cryptography;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public enum MARKINGTYPES {
        REST_SOPUTKA,
        REST_BV,
        IMPORT_SOPUTKA,
        IMPORT_BV,
        FABR_BV,
        FABR_SP
    };
    public class MarkingPaths {
        protected string _strProductsFileName = "";
        //protected string _strGeneratedFolder = "";
        protected string _strName = "";
        protected string _strMainFolder = "";
        protected string _strOrdersKMFileName = "";
        protected string _strKMFileName = "";
        protected string _strGtinFileName = "";
        protected string _strOrdersFileName = "";
        protected string _strPrinterName = "";

        protected bool _bAllFilesExists = false;

        protected string strDestPath = "";
        protected string strSrcPath = "";

        protected string strDestPathOrdersKM = "";
        protected string strSrcPathOrdersKM = "";

        protected string strDestPathKM = "";
        protected string strSrcPathKM = "";

        protected string strDestPathOrders = "";
        protected string strSrcPathOrders = "";

        protected string strDestPathGtin = "";
        protected string strSrcPathGtin = "";

        protected string _strSettingID = " ";

        protected string _str1cIP = "";
        protected string _str1cBase = "";
        protected string _str1cUser = "";
        protected string _str1cPsw = "";

        //private readonly  _Form1 _parent;
        private readonly  MARKINGTYPES _markTypes;
        public MarkingPaths(string strSettingID, MARKINGTYPES markTypes) {
            _markTypes = markTypes;
            //_parent = parent;
            _strSettingID = strSettingID;
            _InitPaths(null);
            //_strGeneratedFolder = @"\ooo\Mtm\generated\";
            _bAllFilesExists = UpdateLocalFiles();
        }

        public string GetSettingID() { return _strSettingID; }

        public string GetDestPath() { return strDestPath; }
        public string GetSrcPath() { return strSrcPath; }
        public string GetDestPathOrdersKM() { return strDestPathOrdersKM; }
        public string GetSrcPathOrdersKM() { return strSrcPathOrdersKM; }
        public string GetDestPathKM() { return strDestPathKM; }
        public string GetSrcPathKM() { return strSrcPathKM; }
        public string GetDestPathOrders() { return strDestPathOrders; }
        public string GetSrcPathOrders() { return strSrcPathOrders; }
        public string GetDestPathGtin() { return strDestPathGtin; }
        public string GetSrcPathGtin() { return strSrcPathGtin; }
        public string GetMainFolder() { return _strMainFolder; }
        public string GetCompanyName() { return _strName; }       
        public string GetProductsFileName() { return _strProductsFileName; }
        public string GetGtinFileName() { return _strGtinFileName; }
        public string GetGeneratedFolder() { return _strMainFolder + @"\generated\"; }
        public string GetOrdersFileName() { return _strOrdersFileName; }
        public string GetPrinterName() { return _strPrinterName; }       
        public bool AllFilesExists() { return _bAllFilesExists; }
        public string Get1cIP() { return _str1cIP; }
        public string Get1cBase() { return _str1cBase; }
        public string Get1cUser() { return _str1cUser; }
        public string Get1cPsw() { return _str1cPsw; }

        protected void _InitPaths(_Form1 parent) {
            string strFileSettingPath = DlgSetting.GetSettingPath();
            if (!File.Exists(strFileSettingPath)) {
                MessageBox.Show(parent, "Отсутствует файл:" + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nCodeOut = -1;
            DataTable dtTableSetting = Dbf.LoadDbfWithAddColumns(strFileSettingPath, out _, ref nCodeOut, "DOC", GetSettingID());
            if (1 != dtTableSetting.Rows.Count) {
                MessageBox.Show(parent, "Несколько записей в файле с номером:" + GetSettingID(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //_strMainFolder = @"\ooo\Mtm\";
            //_strOrdersKMFileName = "MtmOrdersKM.dbf";
            //_strKMFileName = "MtmKM.dbf";
            //_strGtinFileName = "MtmGtin.dbf";
            //_strProductsSPFileName = "RestShopsToStikersSoputks_020.dbf";
            //_strOrdersFileName = "Orders.dbf";

            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.NAME])
                _strName = dtTableSetting.Rows[0][DlgSetting.NAME].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.MainFolder])
                _strMainFolder = dtTableSetting.Rows[0][DlgSetting.MainFolder].ToString();
            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.OrdersKMFN])
                _strOrdersKMFileName = dtTableSetting.Rows[0][DlgSetting.OrdersKMFN].ToString();
            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.KMFN])
                _strKMFileName = dtTableSetting.Rows[0][DlgSetting.KMFN].ToString();
            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.GtinFN])
                _strGtinFileName = dtTableSetting.Rows[0][DlgSetting.GtinFN].ToString();
            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.OrdersFN])
                _strOrdersFileName = dtTableSetting.Rows[0][DlgSetting.OrdersFN].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.PRINTER])
                _strPrinterName = dtTableSetting.Rows[0][DlgSetting.PRINTER].ToString();
           
            if (_markTypes == MARKINGTYPES.REST_SOPUTKA) {
                if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.ProdSPFN])
                    _strProductsFileName = dtTableSetting.Rows[0][DlgSetting.ProdSPFN].ToString();
            }
            if (_markTypes == MARKINGTYPES.REST_BV) {
                if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.ProdBVFN])
                    _strProductsFileName = dtTableSetting.Rows[0][DlgSetting.ProdBVFN].ToString();
            }
            if (_markTypes == MARKINGTYPES.IMPORT_SOPUTKA) {
                if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.ProdSPIMPO])
                    _strProductsFileName = dtTableSetting.Rows[0][DlgSetting.ProdSPIMPO].ToString();
            }
            if (_markTypes == MARKINGTYPES.IMPORT_BV) {
                if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.ProdBVIMPO])
                    _strProductsFileName = dtTableSetting.Rows[0][DlgSetting.ProdBVIMPO].ToString();
            }
            if (_markTypes == MARKINGTYPES.FABR_BV) {
                if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.ProdBVFABR])
                    _strProductsFileName = dtTableSetting.Rows[0][DlgSetting.ProdBVFABR].ToString();
            }
            if (_markTypes == MARKINGTYPES.FABR_SP) {
                if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.ProdSPFABR])
                    _strProductsFileName = dtTableSetting.Rows[0][DlgSetting.ProdSPFABR].ToString();
            }

            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.c1IP])
                _str1cIP = dtTableSetting.Rows[0][DlgSetting.c1IP].ToString();
            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.c1Base])
                _str1cBase = dtTableSetting.Rows[0][DlgSetting.c1Base].ToString();
            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.c1User])
                _str1cUser = dtTableSetting.Rows[0][DlgSetting.c1User].ToString();
            if (System.DBNull.Value != dtTableSetting.Rows[0][DlgSetting.c1Psw])
                _str1cPsw = dtTableSetting.Rows[0][DlgSetting.c1Psw].ToString();
        }
        public bool UpdateLocalFiles() {
            if (!_UpdateLocalFiles()) {
                MessageBox.Show("Необходимо проверить наличие файлов прописанных в setting.dbf", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private bool _UpdateLocalFiles() {
            //if (null == _parent)
            //    return false;
            if (!DbfWrapper.CheckIfFileExist(_strProductsFileName, ref strDestPath, ref strSrcPath, GetGeneratedFolder()))
                return false;
            if (!DbfWrapper.CheckIfFileExist(_strOrdersKMFileName, ref strDestPathOrdersKM, ref strSrcPathOrdersKM, _strMainFolder))
                return false;
            if (!DbfWrapper.CheckIfFileExist(_strKMFileName, ref strDestPathKM, ref strSrcPathKM, _strMainFolder))
                return false;
            if (!DbfWrapper.CheckIfFileExist(_strOrdersFileName, ref strDestPathOrders, ref strSrcPathOrders, _strMainFolder))
                return false;
            if (!DbfWrapper.CheckIfFileExist(_strGtinFileName, ref strDestPathGtin, ref strSrcPathGtin, _strMainFolder))
                return false;
            
            return true;
        }
        public bool FillTree(TreeView tv, string strShopPrefix) {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(GetOrdersFileName(), ref strDestPath, ref strSrcPath, GetMainFolder()))
                return false;

            int nCodeOut = -1;
            DataTable dtTableOrders = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "VISIBLE", "1", null, false, false);
            int NAME = 3;
            int TYPE = 4;
            int DESCR = 5;
            int INO = 7;
            List<Tuple<string, string>> listNamesIno = [];//name, ino

            for (int i = 0; i < dtTableOrders.Rows.Count; i++) {
                if (dtTableOrders.Rows[i][TYPE].ToString() != strShopPrefix)
                    continue;
                string straName = dtTableOrders.Rows[i][NAME].ToString() + " (" + dtTableOrders.Rows[i][DESCR].ToString().Trim()  + ")";
                listNamesIno.Add(Tuple.Create(straName, dtTableOrders.Rows[i][INO].ToString()));
            }

            listNamesIno.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            //sort.Sort((a, b) => a.Item1.CompareTo(b.Item1));

            tv.Nodes.Clear();
            TreeNode tn = tv.Nodes.Add("Заявки", "Заявки", 0, 0);
            for (int i = 0; i < listNamesIno.Count; i++) {
                TreeNode tnL = tn.Nodes.Add(listNamesIno[i].Item1, listNamesIno[i].Item1, 1, 2);
                tnL.Tag = listNamesIno[i].Item2;
            }
            tn.Expand();
            return true;
        }

    }
}
