using System.ComponentModel;
using System.Data;
using System.Drawing.Printing;
using System.Text;
using System.Diagnostics;

using DbfLib;
using FairMarkLib;
using System.Runtime.InteropServices;
using PrintSticker.MarkingObjects;
using PrintSticker.MarkingObjectsBase;


#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым
#pragma warning disable CA2249 //Используйте "string.Contains" вместо "string.IndexOf"
#pragma warning disable CA1864 //Чтобы избежать двойного поиска, вызовите "TryAdd" вместо вызова "Add"
#pragma warning disable IDE0057 //Substring можно упростить
#pragma warning disable CA1854 //Предпочитать вызов TryGetValue доступу к индексатору словаря

namespace PrintSticker {
    public partial class _Form1 : Form {
        private readonly bool bPRINT_USER_SOPUTKA = false;

        private const string _strMTM_PageName = "Большевичка МТМ";
        private const string _strWT_PageName = "Веспер трейдинг";
        private const string _strVetex_PageName = "Ветекс";
        private const string _strBol_PageName = "Большевичка";
        private const string _strSP_PageName = "Сопутка";        
        //private const string _strSettingID_VETEX = "01";
        private const string _strSettingID_BV = "04";
        private readonly string _strFileSettingPath = @"C:\Po_BOLSHEVICHKA\PrintStickerServer\setting.dbf";

        private enum DOCSTATUS {
            READ_EAN13,
            CHECK_EAN13,
            CHECK_QR
        }        
        private MarkingBolshFabricationSP _markingBolFabrSP = null;
        private MarkingBolshFabricationBV _markingBolFabrBV = null;
        private MarkingWtRestBV _markingWTRestBV = null;
        private MarkingWtRestSP _markingWTRestSP = null;
        private MarkingMtmRestSP _markingMtmRestSP = null;
        private MarkingVetexRestBV _markingVetexRestBV = null;

        private MarkingVetexImportSP _markingVetexImpSP = null;
        private MarkingVetexImportBV _markingVetexImpBV = null;
        private MarkingVetexFabricationBV _markingVetexFabrBV = null;

        private MarkingMtFabricationBV _markingMtmFabrBV = null;
        private MarkingMtImportBV _markingMtmImpBV = null;
        private MarkingMtImportSP _markingMtmImpSP = null;
        private MarkingWtFabricationBV _markingWtFabrBV = null;
        private MarkingWtImportBV _markingWtImpBV = null;
        private MarkingWtImportSP _markingWtImpSP = null;

        private Dictionary<int, int> _hNumTableSoputka = [];
        private Dictionary<int, int> _hNumTableBV = [];
        private readonly Dictionary<int, int> _hNumTableVetex = [];

        private DateTime _dtOpenedFileBV = new(1929, 1, 1);
        private DateTime _dtOpenedFileSoputka = new(1929, 1, 1);
        private DateTime _dtOpenedFileVetex = new(1929, 1, 1);

        private string _strOpenedFileBV = "";
        private string _strOpenedFileSoputka = "";
        private const string strPageSticker01 = "Стикер 01";
        private const string strPageSoputka = "Сопутка";
        private bool _bLoadSoputka = false;
        private const string strPageBV = "Большевичка";

        private bool _bLoadBV = false;

        private readonly static object _lockerLogFile = new();

        private string _strDestPathPaternPage1 = "";
        private string _strSrcPathPaternPage1 = "";
        private DateTime _dtSoputkaGtinOrdersMkLoaded = new(1929, 1, 1);
        private DateTime _dtBvGtinOrdersMkLoaded = new(1929, 1, 1);

        Dictionary<string, ProductInCUT> _dicProduct_ProductInCUT = [];

        public _Form1() {
            InitializeComponent();
        }               
        public static bool SendTextFileToPrinter(string szFileName) {
            string printerName = "Zebra S4M (203 dpi) - ZPL";

            byte[] bytes = File.ReadAllBytes(szFileName);
            int nLength = bytes.Length;
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            return RawPrinterHelper.SendBytesToPrinter(printerName, pUnmanagedBytes, nLength);
        }

        private void button2_Click(object sender, EventArgs e) {
           
            string s = "^XA^LH30,30\n^FO20,10^ADN,90,50^AD^FDHello World^FS\n^XZ";
            PrintDialog pd = new() {
                PrinterSettings = new PrinterSettings()
            };
            if (DialogResult.OK == pd.ShowDialog(this))
                RawPrinterHelper.SendStringToPrinter(pd.PrinterSettings.PrinterName, s);
        }

        private void button1_Click(object sender, EventArgs e) {
            SendTextFileToPrinter("c:\\5\\9\\7649.zpl");
        }       
        private void _GoToAllFiles(string strPath, ref List<string> listPathes) {
            if (File.Exists(strPath)) {
                string[] paths0 = File.ReadAllLines(strPath);
                foreach (string strVal in paths0)
                    listPathes.Add(strVal);
                return;
            }

            if (strPath.Length > 248)
                return;

            string[] files = Directory.GetFiles(strPath, "*.dbf");
            foreach (string file in files)
                listPathes.Add(file);

            string[] paths = Directory.GetDirectories(strPath);
            foreach (string strFolder in paths)
                _GoToAllFiles(strFolder, ref listPathes);
        }      
        private HashSet<string> _GetShowedClients() {
            string strFileSettingPath = DlgSetting.GetSettingPath();
            if (!File.Exists(strFileSettingPath)) {
                MessageBox.Show(this, "Отсутствует файл:" + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return [];
            }
            int nCodeOut = -1;
            DataTable dtTableSetting = Dbf.LoadDbfWithAddColumns(strFileSettingPath, out _, ref nCodeOut, "VISIBLE", "1");

            HashSet<string> hs = [];
            int DOC = 2;
            for (int i = 0; i < dtTableSetting.Rows.Count; i++)
                hs.Add(dtTableSetting.Rows[i][DOC].ToString());            
          
            return hs;
        }
        void _GetKMRaskroynyi(string strPath, ref List<string> list) {
            string[] files = Directory.GetFiles(strPath, "*.zpl");
            foreach (string strPathFile in files) {
                string[] lines = File.ReadAllLines(strPathFile, Encoding.GetEncoding(866));
                foreach (string line in lines) {
                    if (line.Length < 83)
                        continue;
                    string strFind = "^FD\\7E1";
                    int pos = line.LastIndexOf(strFind);
                    if (-1 != pos) {
                        string strKM = line.Substring(pos + strFind.Length, 85);
                        list.Add(strKM + "     " + strPathFile);
                    }
                }

            }
            string[] dirs = Directory.GetDirectories(strPath);
            foreach (string strPathDir in dirs)
                _GetKMRaskroynyi(strPathDir, ref list);

        }                     
        private void Form1_Load(object sender, EventArgs e) {
            Trace.Close();
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TraceWraper());
            panel1.Height = 50;

            _markingBolFabrSP = new();
            _markingBolFabrBV = new();
            _markingWTRestBV = new();
            _markingWTRestSP = new();

            _markingMtmRestSP = new();

            _markingVetexRestBV = new();
            _markingVetexImpSP = new();
            _markingVetexImpBV = new();
            _markingVetexFabrBV = new();

            _markingMtmImpSP = new();
            _markingMtmImpBV = new();
            _markingMtmFabrBV = new();

            _markingWtImpSP = new();
            _markingWtImpBV = new();
            _markingWtFabrBV = new();

            HashSet<string> hs = _GetShowedClients();
            tabControl1.TabPages.RemoveAt(5);//Api
            if(!hs.Contains("03")) tabControl1.TabPages.RemoveAt(4);
            if (!hs.Contains("02")) tabControl1.TabPages.RemoveAt(3);
            if (!hs.Contains("01")) tabControl1.TabPages.RemoveAt(2);
            if (!hs.Contains("04")) {
                tabControl1.TabPages.RemoveAt(1);
                tabControl1.TabPages.RemoveAt(0);
            }
           
            this.Cursor = Cursors.Default;
        }
        private void Form1_Shown(object sender, EventArgs e) {
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
        public static void WriteLog(string strText) {
            try {
                string strDir = _GetCurDir() + "\\logs\\";
                Directory.CreateDirectory(strDir);
                string strFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                string strTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                List<string> list = [];
                list.Add(strTime + " " + strText);

                lock (_lockerLogFile)
                    File.AppendAllLines(strDir + strFileName, list);
            } catch (Exception) {
            }
        }

        private void _ShowPageBV() {
            //dataGridViewBV
            tvBV.Nodes.Clear();
            btSettingBV.Enabled = false;

            string strDestPathSoputka = "";
            string strSrcPathSoputka = "";

            string strFileName = "";
            if (rb053_BV.Checked) strFileName = "BvRestShopsToStikers_053.dbf";
            if (rb057_BV.Checked) strFileName = "BvRestShopsToStikers_057.dbf";
            if (rb300_BV.Checked) strFileName = "BvRestShopsToStikers_300.dbf";
            if (rb260_BV.Checked) strFileName = "BvRestShopsToStikers_260.dbf";
            if (rb310_BV.Checked) strFileName = "BvRestShopsToStikers_310.dbf";
            if (rb370_BV.Checked) strFileName = "BvRestShopsToStikers_370.dbf";
            if (rb375_BV.Checked) strFileName = "BvRestShopsToStikers_375.dbf";
            if (rb397_BV.Checked) strFileName = "BvRestShopsToStikers_397.dbf";
            if (rb401_BV.Checked) strFileName = "BvRestShopsToStikers_401.dbf";
            if (rb414_BV.Checked) strFileName = "BvRestShopsToStikers_414.dbf";
            if (rb416_BV.Checked) strFileName = "BvRestShopsToStikers_416.dbf";
            if (rb421_BV.Checked) strFileName = "BvRestShopsToStikers_421.dbf";
            if (rb432_BV.Checked) strFileName = "BvRestShopsToStikers_432.dbf";
            if (rb433_BV.Checked) strFileName = "BvRestShopsToStikers_433.dbf";

            if (rbSGP_BV.Checked) strFileName = "BvRestShopsToStikers_SGP.dbf";
            if (rbAll_BV.Checked) strFileName = "BvRestShopsToStikers.dbf";

            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, @"\Bv\generated\")) {
                dataGridViewBV.DataSource = null;
                _UpdateCount(dataGridViewBV);
                this.Cursor = Cursors.Default;
                return;
            }
            _hNumTableBV = [];

            _dtOpenedFileBV = Win32.GetLastWriteTime(strSrcPathSoputka);
            _strOpenedFileBV = strFileName;

            _LoadDataBV(strFileName, dataGridViewBV, dataGridFilterBV, ref _hNumTableBV);
            _UpdateCountBV();
        }
      
        private string _GetFileNameVetexStikers() {
            string strFileName = "";
            if (rbV_SHOP.Checked) strFileName = "RestShopsToStikers_001.dbf";
            if (rbV_Fabrication.Checked) strFileName = "FabricationToStikers_002.dbf";
            if (rbV_IMPORT.Checked) strFileName = "ImportToStikers_003.dbf";
            if (rbV_IMPORT_SOPUTKA.Checked) strFileName = "ImportToStikersSoputka_004.dbf";
            return strFileName;
        }       
        private void _ShowPageSoputka() {
            string strDestPathSoputka = "";
            string strSrcPathSoputka = "";

            string strFileName = "";
            if (rb053.Checked) strFileName = "SoputkaRestShopsToStikers_053.dbf";
            if (rb057.Checked) strFileName = "SoputkaRestShopsToStikers_057.dbf";
            if (rb300.Checked) strFileName = "SoputkaRestShopsToStikers_300.dbf";
            if (rb260.Checked) strFileName = "SoputkaRestShopsToStikers_260.dbf";
            if (rb310.Checked) strFileName = "SoputkaRestShopsToStikers_310.dbf";
            if (rb370.Checked) strFileName = "SoputkaRestShopsToStikers_370.dbf";
            if (rb375.Checked) strFileName = "SoputkaRestShopsToStikers_375.dbf";
            if (rb397.Checked) strFileName = "SoputkaRestShopsToStikers_397.dbf";
            if (rb401.Checked) strFileName = "SoputkaRestShopsToStikers_401.dbf";
            if (rb414.Checked) strFileName = "SoputkaRestShopsToStikers_414.dbf";
            if (rb416.Checked) strFileName = "SoputkaRestShopsToStikers_416.dbf";
            if (rb421.Checked) strFileName = "SoputkaRestShopsToStikers_421.dbf";
            if (rb432.Checked) strFileName = "SoputkaRestShopsToStikers_432.dbf";
            if (rb433.Checked) strFileName = "SoputkaRestShopsToStikers_433.dbf";

            if (rbSGP.Checked) strFileName = "SoputkaRestShopsToStikers_SGP.dbf";
            if (rbALL.Checked) strFileName = "SoputkaRestShopsToStikers.dbf";


            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, @"\Soputka\generated\")) {
                dataGridViewSoputka.DataSource = null;
                _UpdateCount(dataGridViewSoputka);
                this.Cursor = Cursors.Default; return;
            }
            _hNumTableSoputka = [];

            _dtOpenedFileSoputka = Win32.GetLastWriteTime(strSrcPathSoputka);
            _strOpenedFileSoputka = strFileName;

            _LoadDataSoputka(strFileName, dataGridViewSoputka, dataGridFilterSoputka, ref _hNumTableSoputka);
            _UpdateCount();
        }
                       
        private void _ShovPage1() {
            if (!DbfWrapper.CheckIfFileExist("sticker_P_01.zpl", ref _strDestPathPaternPage1, ref _strSrcPathPaternPage1, @"\1c\Matrix\patern\"))
                return;
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            if (strPageSticker01 == ((TabControl)(sender)).SelectedTab.Text)
                _ShovPage1();

            if (strPageSoputka == ((TabControl)(sender)).SelectedTab.Text) {
                _UpdateCount();
                if (_bLoadSoputka)
                    return;
                //Trace.WriteLine("Страница Сопутка открыта");
                _ShowPageSoputka();
                _bLoadSoputka = true;
            }
            if (strPageBV == ((TabControl)(sender)).SelectedTab.Text) {
                _UpdateCountBV();
                if (_bLoadBV)
                    return;
                //Trace.WriteLine("Страница Сопутка открыта");
                _ShowPageBV();
                _bLoadBV = true;
            }
        }     
        private void CollectRestSP() {
            DlgImportProductsSP dlgImp = new();
            dlgImp.ShowDialog(this);

            this.Cursor = Cursors.WaitCursor;

            BackgroundWorker bw = new() {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };
            bool bRet = false;
            bw.DoWork += delegate (object sender2, DoWorkEventArgs e2) {
                bw.ReportProgress(5);
                if (!GetRemains("SoputkaRestShopsToStikers.dbf", "ALL")) return;
                bw.ReportProgress(10);
                if (!GetRemains("SoputkaRestShopsToStikers_053.dbf", "053")) return;
                bw.ReportProgress(15);
                if (!GetRemains("SoputkaRestShopsToStikers_057.dbf", "057")) return;
                bw.ReportProgress(20);
                if (!GetRemains("SoputkaRestShopsToStikers_300.dbf", "300")) return;
                bw.ReportProgress(25);
                if (!GetRemains("SoputkaRestShopsToStikers_260.dbf", "260")) return;
                bw.ReportProgress(30);
                if (!GetRemains("SoputkaRestShopsToStikers_310.dbf", "310")) return;
                bw.ReportProgress(35);
                if (!GetRemains("SoputkaRestShopsToStikers_370.dbf", "370")) return;
                bw.ReportProgress(40);
                if (!GetRemains("SoputkaRestShopsToStikers_375.dbf", "375")) return;
                bw.ReportProgress(50);
                if (!GetRemains("SoputkaRestShopsToStikers_397.dbf", "397")) return;
                bw.ReportProgress(60);
                if (!GetRemains("SoputkaRestShopsToStikers_401.dbf", "401")) return;
                bw.ReportProgress(70);
                if (!GetRemains("SoputkaRestShopsToStikers_414.dbf", "414")) return;
                bw.ReportProgress(75);
                if (!GetRemains("SoputkaRestShopsToStikers_416.dbf", "416")) return;
                bw.ReportProgress(80);
                if (!GetRemains("SoputkaRestShopsToStikers_421.dbf", "421")) return;
                bw.ReportProgress(90);
                if (!GetRemains("SoputkaRestShopsToStikers_432.dbf", "432")) return;
                bw.ReportProgress(95);
                if (!GetRemains("SoputkaRestShopsToStikers_433.dbf", "433")) return;
                bw.ReportProgress(100);
                bRet = true;
            };

            DlgReportProgress dlg = new(bw, "Импорт Сопутки (остатки из магазинов)");
            dlg.ShowDialog();
            this.Cursor = Cursors.Default;
            if (bRet)
                MessageBox.Show(this, "Импорт прошел Успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }
        private void btSoputka_Click(object sender, EventArgs e) {
            CollectRestSP();
        }

        public bool GetGtinsVetex(ref Dictionary<string, long> dicNomenclGtin) {
            int nCodeOut = -1;

            string strDestPathGTIN = "";
            string strSrcPathGTIN = "";
            if (!DbfWrapper.CheckIfFileExist("VetexGtin.dbf", ref strDestPathGTIN, ref strSrcPathGTIN, @"\ooo\vetex\")) { this.Cursor = Cursors.Default; return false; }

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
        private bool GetRemainsBV(string strResultFileName, string strWorkWithShopNum) {
            
            int nCodeOut = -1;

            string strDestPathRez = "";
            string strSrcPathRez = "";
            if (!DbfWrapper.CheckIfFileExist("BvRestShopsToStikers.dbf", ref strDestPathRez, ref strSrcPathRez, @"\BV\patern\")) { this.Cursor = Cursors.Default; return false; }

            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);
            if (0 != dtTableRez.Rows.Count) {
                Trace.WriteLine("Ошибка, в шаблоне есть данные, в файле: " + strSrcPathRez);
                return false;
            }
            //GTIN   
            Dictionary<string, long> dicNomenclGtin = [];
            if (!DbfWrapper.GetGtinsBv(ref dicNomenclGtin))
                return false;
            //_GTIN
            string strDestPathGen = "";
            string strSrcPathGen = "";
            if (!DbfWrapper.CheckIfFileExist(strResultFileName, ref strDestPathGen, ref strSrcPathGen, @"\BV\generated\")) { this.Cursor = Cursors.Default; return false; }

            string strFileName = "Wf22.dbf";

            string strDestPathSpm01 = "";
            string strSrcPathSpm01 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm01, ref strSrcPathSpm01, @"\BV\Shops\s053\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm02 = "";
            string strSrcPathSpm02 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm02, ref strSrcPathSpm02, @"\BV\Shops\s057\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm03 = "";
            string strSrcPathSpm03 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm03, ref strSrcPathSpm03, @"\BV\Shops\s300\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm04 = "";
            string strSrcPathSpm04 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm04, ref strSrcPathSpm04, @"\BV\Shops\s260\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm05 = "";
            string strSrcPathSpm05 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm05, ref strSrcPathSpm05, @"\BV\Shops\s310\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm06 = "";
            string strSrcPathSpm06 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm06, ref strSrcPathSpm06, @"\BV\Shops\s370\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm07 = "";
            string strSrcPathSpm07 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm07, ref strSrcPathSpm07, @"\BV\Shops\s375\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm08 = "";
            string strSrcPathSpm08 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm08, ref strSrcPathSpm08, @"\BV\Shops\s397\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm09 = "";
            string strSrcPathSpm09 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm09, ref strSrcPathSpm09, @"\BV\Shops\s401\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm10 = "";
            string strSrcPathSpm10 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm10, ref strSrcPathSpm10, @"\BV\Shops\s414\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm11 = "";
            string strSrcPathSpm11 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm11, ref strSrcPathSpm11, @"\BV\Shops\s416\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm12 = "";
            string strSrcPathSpm12 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm12, ref strSrcPathSpm12, @"\BV\Shops\s421\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm13 = "";
            string strSrcPathSpm13 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm13, ref strSrcPathSpm13, @"\BV\Shops\s432\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm14 = "";
            string strSrcPathSpm14 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm14, ref strSrcPathSpm14, @"\BV\Shops\s433\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strD1 = File.GetLastWriteTime(strDestPathSpm01).ToString("yyyy-MM-dd");
            string strD2 = File.GetLastWriteTime(strDestPathSpm02).ToString("yyyy-MM-dd");
            string strD3 = File.GetLastWriteTime(strDestPathSpm03).ToString("yyyy-MM-dd");
            string strD4 = File.GetLastWriteTime(strDestPathSpm04).ToString("yyyy-MM-dd");
            string strD5 = File.GetLastWriteTime(strDestPathSpm05).ToString("yyyy-MM-dd");
            string strD6 = File.GetLastWriteTime(strDestPathSpm06).ToString("yyyy-MM-dd");
            string strD7 = File.GetLastWriteTime(strDestPathSpm07).ToString("yyyy-MM-dd");
            string strD8 = File.GetLastWriteTime(strDestPathSpm08).ToString("yyyy-MM-dd");
            string strD9 = File.GetLastWriteTime(strDestPathSpm09).ToString("yyyy-MM-dd");
            string str10 = File.GetLastWriteTime(strDestPathSpm10).ToString("yyyy-MM-dd");
            string str11 = File.GetLastWriteTime(strDestPathSpm11).ToString("yyyy-MM-dd");
            string str12 = File.GetLastWriteTime(strDestPathSpm12).ToString("yyyy-MM-dd");
            string str13 = File.GetLastWriteTime(strDestPathSpm13).ToString("yyyy-MM-dd");
            string str14 = File.GetLastWriteTime(strDestPathSpm14).ToString("yyyy-MM-dd");

            HashSet<string> hs = [];
            hs.Add(strD1);
            hs.Add(strD2);
            hs.Add(strD3);
            hs.Add(strD4);
            hs.Add(strD5);
            hs.Add(strD6);
            hs.Add(strD7);
            hs.Add(strD8);
            hs.Add(strD9);
            hs.Add(str10);
            hs.Add(str11);
            hs.Add(str12);
            hs.Add(str13);
            hs.Add(str14);
            //hs.Add(str15);
            if (1 != hs.Count) {
                Trace.WriteLine("Ошибка: даты файлов с остатками отличаются");
                return false;
            }

            DataTable dtTableSpm01 = Dbf.LoadDbfWithAddColumns(strDestPathSpm01, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm02 = Dbf.LoadDbfWithAddColumns(strDestPathSpm02, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm03 = Dbf.LoadDbfWithAddColumns(strDestPathSpm03, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm04 = Dbf.LoadDbfWithAddColumns(strDestPathSpm04, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm05 = Dbf.LoadDbfWithAddColumns(strDestPathSpm05, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm06 = Dbf.LoadDbfWithAddColumns(strDestPathSpm06, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm07 = Dbf.LoadDbfWithAddColumns(strDestPathSpm07, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm08 = Dbf.LoadDbfWithAddColumns(strDestPathSpm08, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm09 = Dbf.LoadDbfWithAddColumns(strDestPathSpm09, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm10 = Dbf.LoadDbfWithAddColumns(strDestPathSpm10, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm11 = Dbf.LoadDbfWithAddColumns(strDestPathSpm11, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm12 = Dbf.LoadDbfWithAddColumns(strDestPathSpm12, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm13 = Dbf.LoadDbfWithAddColumns(strDestPathSpm13, out _, ref nCodeOut, "DEL", "0", null, false, false);
            DataTable dtTableSpm14 = Dbf.LoadDbfWithAddColumns(strDestPathSpm14, out _, ref nCodeOut, "DEL", "0", null, false, false);

            //ssgp
            Dictionary<string, RestItemBV> dicBARCODE_RestItem = [];
            if (!_GetDataFromTableBV(ref dtTableSpm01, ref dicBARCODE_RestItem, "053", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm02, ref dicBARCODE_RestItem, "057", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm03, ref dicBARCODE_RestItem, "300", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm04, ref dicBARCODE_RestItem, "260", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm05, ref dicBARCODE_RestItem, "310", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm06, ref dicBARCODE_RestItem, "370", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm07, ref dicBARCODE_RestItem, "375", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm08, ref dicBARCODE_RestItem, "397", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm09, ref dicBARCODE_RestItem, "401", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm10, ref dicBARCODE_RestItem, "414", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm11, ref dicBARCODE_RestItem, "416", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm12, ref dicBARCODE_RestItem, "421", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm13, ref dicBARCODE_RestItem, "432", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTableBV(ref dtTableSpm14, ref dicBARCODE_RestItem, "433", strWorkWithShopNum)) { return false; }

            dtTableRez.Rows.Clear();

            //int nPos = 0;
            int nCount = 0;
            foreach (var item in dicBARCODE_RestItem) {
                RestItemBV ri = item.Value;
                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
                rowAdd[RESTCOLIDBV.BARCODE] = ri.GetBARCODE();
                rowAdd[RESTCOLIDBV.IZD] = ri.GetIZD();
                rowAdd[RESTCOLIDBV.PRS] = ri.GetPRS();
                rowAdd[RESTCOLIDBV.MOD] = ri.GetMOD();
                rowAdd[RESTCOLIDBV.ART] = ri.GetART().Trim();
                rowAdd[RESTCOLIDBV.RAZ] = ri.GetRAZ();
                rowAdd[RESTCOLIDBV.KOL] = ri.GetKOL();
                nCount += ri.GetKOL();
                rowAdd[RESTCOLIDBV.CJ] = ri.GetCJ();
                rowAdd[RESTCOLIDBV.ART2] = ri.GetART2();
                rowAdd[RESTCOLIDBV.PATTERN] = ri.GetPATTERN();
                rowAdd[RESTCOLIDBV.CCODE] = ri.GetCCODE();
                rowAdd[RESTCOLIDBV.CCLOTH] = ri.GetCCLOTH();
                rowAdd[RESTCOLIDBV.COTHER] = ri.GetCOTHER();
                rowAdd[RESTCOLIDBV.CSEASON] = ri.GetCSEASON();
                rowAdd[RESTCOLIDBV.CJ2] = ri.GetCJ2();
                rowAdd[RESTCOLIDBV.CR] = ri.GetCR();
                rowAdd[RESTCOLIDBV.SRT] = ri.GetSRT();

                string strNomencl = MarkingBV.GetNomenclBV(rowAdd);
                if (dicNomenclGtin.ContainsKey(strNomencl))
                    rowAdd[RESTCOLIDBV.GTIN] = dicNomenclGtin[strNomencl];

                rowAdd[RESTCOLIDBV.KOL_KM] = 0;
                rowAdd[RESTCOLIDBV.KOL_PRN] = 0;
                rowAdd[RESTCOLIDBV.IZDNAME] = ri.GetIZDName();

                if ("2142052397382" == ri.GetBARCODE()) {
                    rowAdd[RESTCOLIDBV.RAZ] = "170-088-076";
                }
                dtTableRez.Rows.Add(rowAdd);
            }
            if (!Dbf.AddTable(strDestPathRez, dtTableRez)) {
                Trace.WriteLine("Ошибка добавления остатков в файл: " + strDestPathRez);
                return false;
            }
            try {
                File.Copy(strDestPathRez, strSrcPathGen, true);
            } catch (Exception ex) {
                Trace.WriteLine("Ошибка: " + ex.Message);
                return false;
            }
            return true;
        }

        private bool GetRemains(string strResultFileName, string strWorkWithShopNum) {
            int nCodeOut = -1;

            string strDestPathRez = "";
            string strSrcPathRez = "";
            if (!DbfWrapper.CheckIfFileExist("SoputkaRestShopsToStikers.dbf", ref strDestPathRez, ref strSrcPathRez, @"\Soputka\patern\")) { this.Cursor = Cursors.Default; return false; }

            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);
            if (0 != dtTableRez.Rows.Count) {
                Trace.WriteLine("Ошибка, в шаблоне есть данные, в файле: " + strSrcPathRez);
                return false;
            }
            //GTIN   
            Dictionary<string, long> dicNomenclGtin = [];
            if (!DbfWrapper.GetGtinsSoputka(ref dicNomenclGtin))
                return false;
            //_GTIN
            string strDestPathGen = "";
            string strSrcPathGen = "";
            if (!DbfWrapper.CheckIfFileExist(strResultFileName, ref strDestPathGen, ref strSrcPathGen, @"\Soputka\generated\")) { this.Cursor = Cursors.Default; return false; }

            string strFileName = "Wf22.dbf";

            string strDestPathSpm01 = "";
            string strSrcPathSpm01 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm01, ref strSrcPathSpm01, @"\Soputka\Shops\s053\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm02 = "";
            string strSrcPathSpm02 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm02, ref strSrcPathSpm02, @"\Soputka\Shops\s057\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm03 = "";
            string strSrcPathSpm03 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm03, ref strSrcPathSpm03, @"\Soputka\Shops\s300\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm04 = "";
            string strSrcPathSpm04 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm04, ref strSrcPathSpm04, @"\Soputka\Shops\s260\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm05 = "";
            string strSrcPathSpm05 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm05, ref strSrcPathSpm05, @"\Soputka\Shops\s310\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm06 = "";
            string strSrcPathSpm06 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm06, ref strSrcPathSpm06, @"\Soputka\Shops\s370\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm07 = "";
            string strSrcPathSpm07 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm07, ref strSrcPathSpm07, @"\Soputka\Shops\s375\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm08 = "";
            string strSrcPathSpm08 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm08, ref strSrcPathSpm08, @"\Soputka\Shops\s397\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm09 = "";
            string strSrcPathSpm09 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm09, ref strSrcPathSpm09, @"\Soputka\Shops\s401\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm10 = "";
            string strSrcPathSpm10 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm10, ref strSrcPathSpm10, @"\Soputka\Shops\s414\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm11 = "";
            string strSrcPathSpm11 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm11, ref strSrcPathSpm11, @"\Soputka\Shops\s416\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm12 = "";
            string strSrcPathSpm12 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm12, ref strSrcPathSpm12, @"\Soputka\Shops\s421\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm13 = "";
            string strSrcPathSpm13 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm13, ref strSrcPathSpm13, @"\Soputka\Shops\s432\Datarm\")) { this.Cursor = Cursors.Default; return false; }

            string strDestPathSpm14 = "";
            string strSrcPathSpm14 = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSpm14, ref strSrcPathSpm14, @"\Soputka\Shops\s433\Datarm\")) { this.Cursor = Cursors.Default; return false; }
            
            //
            string strD1 = File.GetLastWriteTime(strDestPathSpm01).ToString("yyyy-MM-dd");
            string strD2 = File.GetLastWriteTime(strDestPathSpm02).ToString("yyyy-MM-dd");
            string strD3 = File.GetLastWriteTime(strDestPathSpm03).ToString("yyyy-MM-dd");
            string strD4 = File.GetLastWriteTime(strDestPathSpm04).ToString("yyyy-MM-dd");
            string strD5 = File.GetLastWriteTime(strDestPathSpm05).ToString("yyyy-MM-dd");
            string strD6 = File.GetLastWriteTime(strDestPathSpm06).ToString("yyyy-MM-dd");
            string strD7 = File.GetLastWriteTime(strDestPathSpm07).ToString("yyyy-MM-dd");
            string strD8 = File.GetLastWriteTime(strDestPathSpm08).ToString("yyyy-MM-dd");
            string strD9 = File.GetLastWriteTime(strDestPathSpm09).ToString("yyyy-MM-dd");
            string str10 = File.GetLastWriteTime(strDestPathSpm10).ToString("yyyy-MM-dd");
            string str11 = File.GetLastWriteTime(strDestPathSpm11).ToString("yyyy-MM-dd");
            string str12 = File.GetLastWriteTime(strDestPathSpm12).ToString("yyyy-MM-dd");
            string str13 = File.GetLastWriteTime(strDestPathSpm13).ToString("yyyy-MM-dd");
            string str14 = File.GetLastWriteTime(strDestPathSpm14).ToString("yyyy-MM-dd");
            //string str15 = File.GetLastWriteTime(strDestPathSpm15).ToString("yyyy-MM-dd");
            HashSet<string> hs = [];
            hs.Add(strD1);
            hs.Add(strD2);
            hs.Add(strD3);
            hs.Add(strD4);
            hs.Add(strD5);
            hs.Add(strD6);
            hs.Add(strD7);
            hs.Add(strD8);
            hs.Add(strD9);
            hs.Add(str10);
            hs.Add(str11);
            hs.Add(str12);
            hs.Add(str13);
            hs.Add(str14);
            //hs.Add(str15);
            if (1 != hs.Count) {
                Trace.WriteLine("Ошибка: даты файлов с остатками отличаются");
                MessageBox.Show(null,"Ошибка: даты файлов с остатками отличаются", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return false;
            }
            string strColumn = "COTHER";//"COTHER"
            string strColumnValue = "30";//"30"

            DataTable dtTableSpm01 = Dbf.LoadDbfWithAddColumns(strDestPathSpm01, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm02 = Dbf.LoadDbfWithAddColumns(strDestPathSpm02, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm03 = Dbf.LoadDbfWithAddColumns(strDestPathSpm03, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm04 = Dbf.LoadDbfWithAddColumns(strDestPathSpm04, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm05 = Dbf.LoadDbfWithAddColumns(strDestPathSpm05, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm06 = Dbf.LoadDbfWithAddColumns(strDestPathSpm06, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm07 = Dbf.LoadDbfWithAddColumns(strDestPathSpm07, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm08 = Dbf.LoadDbfWithAddColumns(strDestPathSpm08, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm09 = Dbf.LoadDbfWithAddColumns(strDestPathSpm09, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm10 = Dbf.LoadDbfWithAddColumns(strDestPathSpm10, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm11 = Dbf.LoadDbfWithAddColumns(strDestPathSpm11, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm12 = Dbf.LoadDbfWithAddColumns(strDestPathSpm12, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm13 = Dbf.LoadDbfWithAddColumns(strDestPathSpm13, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            DataTable dtTableSpm14 = Dbf.LoadDbfWithAddColumns(strDestPathSpm14, out _, ref nCodeOut, strColumn, strColumnValue, null, false, false);
            //ssgp
            Dictionary<string, RestItem> dicBARCODE_RestItem = [];
            if (!_GetDataFromTable(ref dtTableSpm01, ref dicBARCODE_RestItem, "053", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm02, ref dicBARCODE_RestItem, "057", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm03, ref dicBARCODE_RestItem, "300", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm04, ref dicBARCODE_RestItem, "260", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm05, ref dicBARCODE_RestItem, "310", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm06, ref dicBARCODE_RestItem, "370", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm07, ref dicBARCODE_RestItem, "375", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm08, ref dicBARCODE_RestItem, "397", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm09, ref dicBARCODE_RestItem, "401", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm10, ref dicBARCODE_RestItem, "414", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm11, ref dicBARCODE_RestItem, "416", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm12, ref dicBARCODE_RestItem, "421", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm13, ref dicBARCODE_RestItem, "432", strWorkWithShopNum)) { return false; }
            if (!_GetDataFromTable(ref dtTableSpm14, ref dicBARCODE_RestItem, "433", strWorkWithShopNum)) { return false; }

            dtTableRez.Rows.Clear();
            //int nPos = 0;
            int nCount = 0;
            foreach (var item in dicBARCODE_RestItem) {
                //nPos++;
                RestItem ri = item.Value;
                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
                rowAdd[RESTCOLID.BARCODE] = ri.GetBARCODE();
                rowAdd[RESTCOLID.IZD] = ri.GetIZD();
                rowAdd[RESTCOLID.MOD] = ri.GetMOD();

                rowAdd[RESTCOLID.ART] = ri.GetART();
                rowAdd[RESTCOLID.RAZ] = ri.GetRAZ();
                rowAdd[RESTCOLID.KOL] = ri.GetKOL();
                nCount += ri.GetKOL();
                rowAdd[RESTCOLID.CJ] = ri.GetCJ();
                rowAdd[RESTCOLID.ART2] = ri.GetART2();
                rowAdd[RESTCOLID.PATTERN] = ri.GetPATTERN();
                rowAdd[RESTCOLID.CCODE] = ri.GetCCODE();
                rowAdd[RESTCOLID.CCLOTH] = ri.GetCCLOTH();
                rowAdd[RESTCOLID.COTHER] = ri.GetCOTHER();
                rowAdd[RESTCOLID.CSEASON] = ri.GetCSEASON();
                rowAdd[RESTCOLID.CJ2] = ri.GetCJ2();
                rowAdd[RESTCOLID.CR] = ri.GetCR();

                if ("БРЮКИ" == ri.GetMOD() && "|" == ri.GetCCLOTH() && "" == ri.GetCCODE()) {
                    string str1 = ri.GetART();
                    string str2 = ri.GetRAZ();
                    int nCol3 = ri.GetKOL();
                    int nPos = str1.LastIndexOf("5-421/09");
                    int nPos2 = str1.LastIndexOf("6-490/09");
                    int nPos3 = str1.LastIndexOf("6-469/08");
                    int nPos4 = str1.LastIndexOf("5-417/41");
                    int nPos5 = str1.LastIndexOf("6-490/38");

                    if (str1 == "WEGENER ETON 6-500/09" || str1 == "WEGENER BOSTON 6-568/09" || str1 == "WEGENER BOSTON 6-568/24" ||
                        str1 == "WEGENER BOSTON 6-568/38" || str1 == "WEGENER ETON 6-500/38" || str1 == "WEGENER BOSTON 6-568/19" ||
                        str1 == "WEGENER ETON 6-500/28" || str1 == "WEGENER ETON 6-500/19")
                        rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|98-2";
                    else if (nPos != -1) {
                        rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|96-4";
                    } else if (nPos2 != -1 || -1 != nPos5) {
                        rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|99-1";
                    } else if (nPos3 != -1 || -1 != nPos4) {
                        rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|98-2";
                    } else if (str1 == "WEGENER ETON 6-651/18" || str1 == "WEGENER ETON 6-651/09" || str1 == "WEGENER ETON 6-561/18" ||
                          str1 == "WEGENER DOUGLAS 5-601/08" || str1 == "WEGENER BOSTON 6-573/08" || str1 == "WEGENER ETON 6-561/09") {
                        rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|99-1";
                    } else if (str1 == "WEGENER ETON 5-521/09" || str1 == "WEGENER ETON 5-516/09" || str1 == "WEGENER ETON 5-516/19" ||
                          str1 == "WEGENER ETON 5-516/41" || str1 == "WEGENER ETON 5-516/41*" || str1 == "WEGENER BOSTON 5-527/31**" ||
                          str1 == "WEGENER ETON 5-516/06") {
                        rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|97-3";
                    } else {
                        rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|97-3";
                    }


                    if (str1 == "WEGENER BOSTON 6-568/24" || str1 == "WEGENER ETON 5-516/41" || str1 == "WEGENER ETON 5-516/41*" || -1 != nPos4 || str1 == "WEGENER BOSTON 5-527/31**")
                        rowAdd[RESTCOLID.CCODE] = "270";
                    else if (str1 == "WEGENER ETON 6-500/28")
                        rowAdd[RESTCOLID.CCODE] = "510";
                    else if (str1 == "WEGENER BOSTON 6-568/38" || str1 == "WEGENER ETON 6-500/38" || -1 != nPos5)
                        rowAdd[RESTCOLID.CCODE] = "290";
                    else if (str1 == "WEGENER DOUGLAS 5-601/08" || str1 == "WEGENER BOSTON 6-573/08" || str1 == "WEGENER ETON 5-516/06" || -1 != nPos3)
                        rowAdd[RESTCOLID.CCODE] = "210";
                    else if (str1 == "WEGENER ETON 6-651/18" || str1 == "WEGENER BOSTON 6-568/19" || str1 == "WEGENER ETON 5-516/19" || str1 == "WEGENER ETON 6-561/18" || str1 == "WEGENER ETON 6-500/19")
                        rowAdd[RESTCOLID.CCODE] = "440";//
                    else
                        rowAdd[RESTCOLID.CCODE] = "110";

                }

                if ("2922405432416" == ri.GetBARCODE())
                    rowAdd[RESTCOLID.GTIN] = "4640287256521";

                string strNomencl = GetNomencl(rowAdd);
                if (dicNomenclGtin.ContainsKey(strNomencl))
                    rowAdd[RESTCOLID.GTIN] = dicNomenclGtin[strNomencl];

                rowAdd[RESTCOLID.KOL_KM] = 0;
                rowAdd[RESTCOLID.KOL_PRN] = 0;

                dtTableRez.Rows.Add(rowAdd);
            }
            if (!Dbf.AddTable(strDestPathRez, dtTableRez)) {
                Trace.WriteLine("Ошибка добавления остатков в файл: " + strDestPathRez);
                return false;
            }
            try {
                File.Copy(strDestPathRez, strSrcPathGen, true);
            } catch (Exception ex) {
                Trace.WriteLine("Ошибка: " + ex.Message);
                return false;
            }
            return true;
        }

        readonly HashSet<string> hsNotAdd = [];

        private bool _GetDataFromTableBV(ref DataTable dtTableSpm, ref Dictionary<string, RestItemBV> dicBARCODE_RestItem, string spmCode, string strWorkWithShopNum) {
            if (strWorkWithShopNum != "ALL" && spmCode != strWorkWithShopNum)
                return true;

            if (!RestItemBV.CheckColumnsInTable(ref dtTableSpm))
                return false;

            string strBARCODE;
            for (int i = 0; i < dtTableSpm.Rows.Count; i++) {
                RestItemBV ri = new(dtTableSpm.Rows[i]);
                if (ri.GetKOL() <= 0)
                    continue;
                string strProduct = ri.GetProduct();
                strBARCODE = ri.GetBARCODE();

                if (_dicProduct_ProductInCUT.ContainsKey(strProduct)) {
                    strBARCODE = _dicProduct_ProductInCUT[strProduct].GetBARCODE();
                    ri.SetBARCODE(strBARCODE);
                }
                
                if (!dicBARCODE_RestItem.ContainsKey(strBARCODE)) {
                    ri.listSpm.Add(spmCode);
                    dicBARCODE_RestItem.Add(strBARCODE, ri);
                } else {
                    ((RestItemBV)dicBARCODE_RestItem[strBARCODE]).listSpm.Add(spmCode);
                    if (!((RestItemBV)dicBARCODE_RestItem[strBARCODE]).Add(ref ri)) {
                        Trace.WriteLine("Ошибка, разные данные у одного и того же стрихкода: " + strBARCODE);
                        MessageBox.Show("Ошибка, разные данные у одного и того же стрихкода: " + strBARCODE, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //return false;
                    }
                }
            }
            return true;
        }

        private bool _GetDataFromTable(ref DataTable dtTableSpm, ref Dictionary<string, RestItem> dicBARCODE_RestItem, string spmCode, string strWorkWithShopNum) {
            if (strWorkWithShopNum != "ALL" && spmCode != strWorkWithShopNum)
                return true;
            if (!RestItem.CheckColumnsInTable(ref dtTableSpm))
                return false;

            ProductForImportSP prImport = new();

            string strBARCODE;
            for (int i = 0; i < dtTableSpm.Rows.Count; i++) {
                RestItem ri = new(dtTableSpm.Rows[i]);
                strBARCODE = ri.GetBARCODE();
                if (ri.GetKOL() <= 0)
                    continue;
                if (!prImport.Contains(ri.GetMOD())) {
                    hsNotAdd.Add(ri.GetMOD());
                    continue;
                }
                if (!dicBARCODE_RestItem.ContainsKey(strBARCODE)) {
                    ri.listSpm.Add(spmCode);
                    dicBARCODE_RestItem.Add(strBARCODE, ri);
                } else {
                    ((RestItem)dicBARCODE_RestItem[strBARCODE]).listSpm.Add(spmCode);
                    if (!((RestItem)dicBARCODE_RestItem[strBARCODE]).Add(ref ri)) {
                        Trace.WriteLine("Ошибка, разные данные у одного и того же стрихкода: " + strBARCODE);
                        MessageBox.Show("Ошибка, разные данные у одного и того же стрихкода: " + strBARCODE, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return true;
        }
        
        private static bool _IsInt(string strValur) {
            try {
                int tt = Convert.ToInt32(strValur);
            } catch (Exception) {
                return false;
            }
            return true;
        }
        private static bool _IsLong(string strValur) {
            try {
                long tt = Convert.ToInt64(strValur);
            } catch (Exception) {
                return false;
            }
            return true;
        }

        private string _GetCvetBV(string strCvet) { //DBCOLOR.DBF
            if (!_IsInt(strCvet))
                return strCvet;
            int nColor = Convert.ToInt32(strCvet);

            if (rbV_IMPORT_SOPUTKA.Checked) {
                if (270 == nColor) return "светло-бежевый";
                if (240 == nColor) return "бежевый";
                if (320 == nColor) return "темно-бежевый";
            }

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
            return "";
        }
        private string _GetCvet(string strCvet) {//DBCOLOR.DBF
            if (!_IsInt(strCvet))
                return strCvet;
            int nColor = Convert.ToInt32(strCvet);

            ColorSP colorSP = new();
            return colorSP.GetNameColor(nColor);           
        }        
        
        private string _GetCountry(string strCountry) {
            strCountry = strCountry.ToUpper();
            if ("РОССИЯ" == strCountry) return "<RU> РОССИЙСКАЯ ФЕДЕРАЦИЯ";
            if ("БАНГЛАДЕШ" == strCountry) return "<BD> НАРОДНАЯ РЕСПУБЛИКА БАНГЛАДЕШ";
            if ("БЕЛАРУСЬ" == strCountry) return "<BY> РЕСПУБЛИКА БЕЛАРУСЬ";
            if ("КНР" == strCountry || "КИТАЙ" == strCountry) return "<CN> КИТАЙСКАЯ НАРОДНАЯ РЕСПУБЛИКА";
            if ("ИТАЛИЯ" == strCountry) return "<IT> ИТАЛЬЯНСКАЯ РЕСПУБЛИКА";
            if ("РУМЫНИЯ" == strCountry) return "<RO> " + strCountry;
            if ("ТУРЦИЯ" == strCountry) return "<TR> ТУРЕЦКАЯ РЕСПУБЛИКА";
            if ("ГЕРМАНИЯ" == strCountry) return "<DE> ФЕДЕРАТИВНАЯ РЕСПУБЛИКА ГЕРМАНИЯ";
            if ("ВЬЕТНАМ" == strCountry) return "<VN> СОЦИАЛИСТИЧЕСКАЯ РЕСПУБЛИКА ВЬЕТНАМ";
            if ("ВЕЛИКОБРИТАНИЯ" == strCountry) return "<GB> " + strCountry;
            if ("ИНДИЯ" == strCountry) return "<IN> РЕСПУБЛИКА ИНДИЯ";
            if ("ЛАТВИЯ" == strCountry) return "<LV> ЛАТВИЙСКАЯ РЕСПУБЛИКА";
            if ("МОЛДОВА" == strCountry) return "<MD> РЕСПУБЛИКА МОЛДОВА";
            if ("ПОРТУГАЛИЯ" == strCountry) return "<PT> ПОРТУГАЛЬСКАЯ РЕСПУБЛИКА";
            if ("ПЕРУ" == strCountry) return "<PE> РЕСПУБЛИКА ПЕРУ";
            if ("ИНДОНЕЗИЯ" == strCountry) return "<ID> РЕСПУБЛИКА ИНДОНЕЗИЯ";
            if ("СЛОВАКИЯ" == strCountry) return "<SK> СЛОВАЦКАЯ РЕСПУБЛИКА";
            if ("ФИНЛЯНДИЯ" == strCountry) return "<FI> ФИНЛЯНДСКАЯ РЕСПУБЛИКА";

            if ("УЗБЕКИСТАН" == strCountry) return "<UZ> УЗБЕКИСТАН";
            return strCountry;
        }      
        private string _GetTNVED(string strTypeProduct, string strSostav, string strMod) {
            Marking marking = new();

            string strTnved = marking.GetTnvedFromBD(strTypeProduct, strSostav, strMod);
            if ("" != strTnved)
                return strTnved;
            MessageBox.Show(this, $"В справочнике ТНВЕД нет данных для {strTypeProduct} , {strSostav} , {strMod}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);          

            if (-1 != strTypeProduct.IndexOf("СОРОЧКА")) {
                if (_IsWool(strSostav)) return "6205908001"; //<6205908001> Рубашки мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6205200000";//<6205200000> Рубашки мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6205300000";//<6205300000> Рубашки мужские или для мальчиков, из химических нитей
                if (_IsLen(strSostav)) return "6205901000";//<6205901000> Рубашки мужские или для мальчиков, из льняных волокон или волокна рами
                return "6205908009";//<6205908009> Рубашки мужские или для мальчиков, из прочих текстильных материалов              
            }

            bool bBrukiWomen = false;
            if (-1 != strTypeProduct.IndexOf("БРЮКИ") && -1 != strTypeProduct.IndexOf("ЖЕН"))
                bBrukiWomen = true;

            if (-1 != strTypeProduct.IndexOf("БРЮКИ ЖЕН") || bBrukiWomen) {
                //<6204691800> Прочие брюки и бриджи женские или для девочек, из искусственных нитей
                if (_IsWool(strSostav)) return "6204611000"; //<6204611000> Брюки и бриджи женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6204623900";//<6204623900> Прочие брюки и бриджи женские или для девочек, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6204631800";//<6204631800> Прочие брюки и бриджи женские или для девочек, из синтетических нитей
                return "6204699000";//<6204699000> Брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты женские или для девочек,из прочих текстильных материалов
            }

            if (-1 != strTypeProduct.IndexOf("БРЮКИ")) {
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos) {
                    //<6204691800> Прочие брюки и бриджи женские или для девочек, из искусственных нитей
                    if (_IsWool(strSostav)) return "6204611000"; //<6204611000> Брюки и бриджи женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                    if (_IsHlopok(strSostav)) return "6204623900";//<6204623900> Прочие брюки и бриджи женские или для девочек, из хлопчатобумажной пряжи
                    if (_IsSintetik(strSostav)) return "6204631800";//<6204631800> Прочие брюки и бриджи женские или для девочек, из синтетических нитей
                    return "6204699000";//<6204699000> Брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты женские или для девочек,из прочих текстильных материалов
                }
                //<6203491900> Прочие брюки и бриджи мужские или для мальчиков, из искусственных нитей
                if (_IsWool(strSostav)) return "6203411000"; //<6203411000> Брюки и бриджи мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6203423500";//<6203423500> Прочие брюки и бриджи мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav))
                    return "6203431900";//<6203431900> Прочие брюки и бриджи мужские или для мальчиков, из синтетических нитей
                return "6203499000";//<6203499000> Брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты, мужские или для мальчиков, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("КОСТЮМ ЖЕН")) {
                //<6204191000> Костюмы женские или для девочек, из искусственных нитей
                if (_IsWool(strSostav)) return "6204110000";//<6204110000> Костюмы женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6204120000";//<6204120000> Костюмы женские или для девочек, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6204130000";//<6204130000> Костюмы женские или для девочек, из синтетических нитей
                return "6204199000";//<6204199000> Костюмы женские или для девочек, из прочих текстильных материалов
            }

            if (-1 != strTypeProduct.IndexOf("КОСТЮМ") || -1 != strTypeProduct.IndexOf("КОМПЛЕКТ")) {
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos) {
                    //<6204191000> Костюмы женские или для девочек, из искусственных нитей
                    if (_IsWool(strSostav)) return "6204110000";//<6204110000> Костюмы женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                    if (_IsHlopok(strSostav)) return "6204120000";//<6204120000> Костюмы женские или для девочек, из хлопчатобумажной пряжи
                    if (_IsSintetik(strSostav)) return "6204130000";//<6204130000> Костюмы женские или для девочек, из синтетических нитей
                    return "6204199000";//<6204199000> Костюмы женские или для девочек, из прочих текстильных материалов 
                }

                //<6203193000> Костюмы мужские или для мальчиков, из искусственных нитей
                if (_IsSintetik(strSostav))
                    return "6203120000";//<6203120000> Костюмы мужские или для мальчиков, из синтетических нитей

                if (_IsWool(strSostav)) return "6203110000";//<6203110000> Костюмы мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6203191000";//<6203191000> Костюмы мужские или для мальчиков, из хлопчатобумажной пряжи
                return "6203199000";//<6203199000> Костюмы мужские или для мальчиков, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("ПИДЖАК")) {
                //<6203391900> Прочие пиджаки и блайзеры мужские или для мальчиков, из искусственных нитей
                if (_IsWool(strSostav)) return "6203310000";//<6203310000> Пиджаки и блайзеры мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6203329000";//<6203329000> Прочие пиджаки и блайзеры мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6203339000";//<6203339000> Прочие пиджаки и блайзеры мужские или для мальчиков, из синтетических нитей
                return "6203399000";//<6203399000> Пиджаки и блайзеры мужские или для мальчиков из прочих текстильных материалов                
            }

            if (-1 != strTypeProduct.IndexOf("ДЖИНСЫ")) {
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos)
                    return "6204623100";//<6204623100> Прочие брюки и бриджи женские или для девочек, из денима, или джинсовой ткани
                return "6203423100";//<6203423100> Брюки и бриджи мужские или для мальчиков, из денима, или джинсовой ткани
            }
            if (-1 != strTypeProduct.IndexOf("ТОП")) {
                if (_IsWool(strSostav)) return "6109902000";//6109902000 Майки, фуфайки с рукавами и прочие нательные фуфайки трикотажные машинного или ручного вязания из шерстяной пряжи или пряжи из тонкого волоса животных или из химических нитей
                if (_IsHlopok(strSostav)) return "6109100000";//6109100000 Майки, фуфайки с рукавами и прочие нательные фуфайки трикотажные, из хлопчатобумажной пряжи, машинного или ручного вязания
                return "6109909000";
            }
            //6109909000 Майки, фуфайки с рукавами и прочие нательные фуфайки трикотажные, из прочих текстильных материалов, машинного или ручного вязания
            if (-1 != strTypeProduct.IndexOf("ПОЛО")) {
                if (_IsHlopok(strSostav)) return "6105100000";//<6105100000> Рубашки трикотажные, мужские или для мальчиков, из хлопчатобумажной пряжи, машинного или ручного вязания
                if (_IsWool(strSostav)) return "6105901000";//<6105901000> Рубашки трикотажные, мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных, машинного или ручного вязания
                if (_IsSintetik(strSostav)) return "6105201000";//<6105201000> Рубашки трикотажные, мужские или для мальчиков, из химических синтетических нитей, машинного или ручного вязания

                //< 6105209000 > Рубашки трикотажные, мужские или для мальчиков, из химических искусственных нитей, машинного или ручного вязания
                return "6105909000";//<6105909000> Рубашки трикотажные, мужские или для мальчиков, из прочих текстильных материалов, машинного или ручного вязания
            }

            if (-1 != strTypeProduct.IndexOf("ФУТБОЛКА")) return "6109000000";
            if (-1 != strTypeProduct.IndexOf("ХУДИ")) return "6110000000";

            if (-1 != strTypeProduct.IndexOf("ПЛАТЬЕ") || -1 != strTypeProduct.IndexOf("САРАФАН")) {
                //6204440000 Платья женские или для девочек из искусственных нитей
                if (_IsWool(strSostav)) return "6204410000";//6204410000 Платья женские или для девочек из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6204420000";//6204420000 Платья женские или для девочек из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6204430000";//6204430000 Платья женские или для девочек из синтетических нитей
                if (_IsShelk(strSostav)) return "6204491000";//6204491000 Платья женские или для девочек из шелковых нитей или пряжи из шелковых отходов
                return "6204499000";//6204499000 Платья женские или для девочек из прочих текстильных материалов

            }

            if (-1 != strTypeProduct.IndexOf("БЛУЗКА") || -1 != strTypeProduct.IndexOf("БЛУЗА") ||
                -1 != strTypeProduct.IndexOf("БЛУЗА-ТОП") || -1 != strTypeProduct.IndexOf("ТУНИКА")) {
                if (_IsWool(strSostav)) return "6206200000";
                if (_IsHlopok(strSostav)) return "6206300000";
                if (_IsSintetik(strSostav)) return "6206400000";
                if (_IsLen(strSostav)) return "6206901000";
                if (_IsShelk(strSostav)) return "6206100000";
                return "6206909000";
            }
            if (-1 != strTypeProduct.IndexOf("КАРДИГАН") || -1 != strTypeProduct.IndexOf("ВОДОЛАЗКА") || -1 != strTypeProduct.IndexOf("СВИТШОТ")) {
                //if (_IsWool(strSostav))
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos)
                    return "6110119000"; //<6110119000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для женщин или девочек, из шерстяной пряжи, машинного или ручного вязания
                return "6110113000"; //<6110113000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для мужчин или мальчиков, из шерстяной пряжи, машинного или ручного вязания
            }

            if (-1 != strTypeProduct.IndexOf("ДЖЕМПЕР") || -1 != strTypeProduct.IndexOf("СВИТЕР")) {
                if (_IsSintetik(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110309900"; //<6110309900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для женщин или девочек, машинного или ручного вязания
                    return "6110309100"; //<6110309100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для мужчин или мальчиков, машинного или ручного вязания
                }
                if (_IsHlopok(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110209900"; //<6110209900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для женщин или девочек, машинного или ручного вязания
                    return "6110209100"; //<6110209100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для мужчин или мальчиков, машинного или ручного вязания
                }
                if (_IsLen(strSostav))
                    return "6110901000"; //<6110901000> Свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из льняных волокон или волокна рами, машинного или ручного вязания
                return "6110909000";//<6110909000> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из прочих текстильных материалов, машинного или ручного вязания
            }
            if (-1 != strTypeProduct.IndexOf("ЖИЛЕТ")) {
                if (_IsWool(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110119000"; //<6110119000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для женщин или девочек, из шерстяной пряжи, машинного или ручного вязания
                    return "6110113000"; //<6110113000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для мужчин или мальчиков, из шерстяной пряжи, машинного или ручного вязания
                }
                if (_IsHlopok(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110209900"; //<6110209900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для женщин или девочек, машинного или ручного вязания
                    return "6110209100"; //<6110209100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для мужчин или мальчиков, машинного или ручного вязания
                }
                if (_IsSintetik(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110309100"; //<6110309100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для мужчин или мальчиков, машинного или ручного вязания
                    return "6110309900"; //<6110309900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для женщин или девочек, машинного или ручного вязания
                }
                if (_IsLen(strSostav))
                    return "6110901000";//<6110901000> Свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из льняных волокон или волокна рами, машинного или ручного вязания
                return "6110909000";//<6110909000> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из прочих текстильных материалов, машинного или ручного вязания
            }
            if (-1 != strTypeProduct.IndexOf("ЖАКЕТ")) {
                if (_IsSintetik(strSostav))
                    return "6204339000"; //<6204339000> Жакеты и блайзеры женские или для девочек, из синтетических нитей
                if (_IsWool(strSostav))
                    return "6204310000"; //<6204310000> Жакеты и блайзеры женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6204329000";//<6204329000> Прочие жакеты и блайзеры женские или для девочек, из хлопчатобумажной пряжи
                return "6204399000";//<6204399000> Прочие жакеты и блайзеры женские или для девочек, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("РУБАШКА")) {
                if (_IsHlopok(strSostav))
                    return "6205200000";//<6205200000> Рубашки мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsWool(strSostav))
                    return "6205908001";//<6205908001> Рубашки  из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsSintetik(strSostav))
                    return "6205300000"; //<6205300000> Рубашки  из химических нитей
                if (_IsLen(strSostav))
                    return "6205901000"; //<6205901000> Рубашки  из льняных волокон или волокна рами
                return "6205908009";//<6205908009> Рубашки  из прочих текстильных материалов                  
            }
            if (-1 != strTypeProduct.IndexOf("ГАЛСТУК")) {
                if (_IsSintetik(strSostav))
                    return "6215200000"; //<6215200000> Галстуки, галстуки-бабочки и шейные платки, из химических нитей
                if (_IsShelk(strSostav))
                    return "6215100000";//<6215100000> Галстуки, галстуки-бабочки и шейные платки, из шелковых нитей или пряжи из шелковых отходов
                return "6215900000"; //<6215900000> Галстуки, галстуки-бабочки и шейные платки, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("ШОРТЫ")) {
                if (_IsHlopok(strSostav))
                    return "6203429000";//<6203429000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav))
                    return "6203439000"; //<6203439000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из синтетических нитей
                //<6203419000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                return "6203495000"; //<6203495000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из искусственных нитей
            }
            if (-1 != strTypeProduct.IndexOf("ЮБКА")) {
                //<6204591000> Юбки и юбки-брюки женские или для девочек, из искусственных нитей
                if (_IsWool(strSostav))
                    return "6204510000";//<6204510000> Юбки и юбки-брюки женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6204520000";//<6204520000> Юбки и юбки-брюки женские или для девочек, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav))
                    return "6204530000";//<6204530000> Юбки и юбки-брюки женские или для девочек, из синтетических нитей                
                return "6204599000";//<6204599000> Юбки и юбки-брюки женские или для девочек, из прочих текстильных материалов               
            }
            MessageBox.Show(this, "Ошибка, неизвестный вида изделия:" + strTypeProduct, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return strTypeProduct;
        }
        private bool _IsWool(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("шерсть") || -1 != strSostav.LastIndexOf("мохер"))
                return true;
            return false;
        }
        private bool _IsLen(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("лен"))
                return true;
            return false;
        }
        private bool _IsShelk(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("шелк"))
                return true;
            return false;
        }
        private bool _IsHlopok(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("хлопок"))
                return true;
            return false;
        }
        private bool _IsSintetik(string strSostav) {
            return Marking._IsSintetik(strSostav);
        }



        private string _GetTypeProduct(string strTypeProduct) {
            if (-1 != strTypeProduct.IndexOf("БЛУЗА-ТОП")) return "<99> БЛУЗА-ТОП";
            if (-1 != strTypeProduct.IndexOf("БЛУЗА")) return "<11> БЛУЗА";
            if (-1 != strTypeProduct.IndexOf("БЛУЗКА")) return "<20> БЛУЗКА";
            if (-1 != strTypeProduct.IndexOf("БРЮКИ")) return "<33> БРЮКИ";
            if (-1 != strTypeProduct.IndexOf("ВОДОЛАЗКА")) return "<131> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ГАЛСТУК")) return "<100> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ДЖЕМПЕР")) return "<12> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ДЖИНСЫ")) return "<33> БРЮКИ";
            if (-1 != strTypeProduct.IndexOf("ЖАКЕТ")) return "<2> ЖАКЕТ";
            if (-1 != strTypeProduct.IndexOf("ЖИЛЕТ")) return "<3> ЖИЛЕТ";
            if (-1 != strTypeProduct.IndexOf("КАРДИГАН")) return "<112> КАРДИГАН";
            if (-1 != strTypeProduct.IndexOf("КОСТЮМ")) return "<42> КОСТЮМ";
            if (-1 != strTypeProduct.IndexOf("КОМПЛЕКТ МУЖСКОЙ")) return "<42> КОСТЮМ";
            if (-1 != strTypeProduct.IndexOf("ПИДЖАК")) return "<8> ПИДЖАК";
            if (-1 != strTypeProduct.IndexOf("ПЛАСТРОН")) return "<99> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ПЛАТЬЕ") || -1 != strTypeProduct.IndexOf("САРАФАН")) return "<18> ПЛАТЬЕ";
            if (-1 != strTypeProduct.IndexOf("РУБАШКА")) return "<22> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("САРАФАН")) return "<99> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("СВИТЕР")) return "<13> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("СВИТШОТ")) return "<146> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("СОРОЧКА")) return "<22> РУБАШКА";
            if (-1 != strTypeProduct.IndexOf("ТОП")) return "<151> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ТУНИКА")) return "<122> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ФУТБОЛКА")) return "<123> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ХУДИ")) return "<121> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ШОРТЫ")) return "<34> " + strTypeProduct;
            if (-1 != strTypeProduct.IndexOf("ЮБКА")) return "<36> ЮБКА";
            MessageBox.Show(this, "Ошибка, неизвестный вида изделия:" + strTypeProduct, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return strTypeProduct;
        }

        private void menuExportToExcel_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            _ExportToExcel();
            this.Cursor = Cursors.Default;
        }
       
        private void _GetNomenclWithGtinBV(ref HashSet<string> hNomenclWithGtin) {
            string strDestSoputkaGtin = "";
            string strSrcSoputkaGtin = "";
            if (!DbfWrapper.CheckIfFileExist("BvGtin.dbf", ref strDestSoputkaGtin, ref strSrcSoputkaGtin, @"\BV\")) { this.Cursor = Cursors.Default; return; }

            int nCodeOut = -1;
            DataTable dtTableSoputkaGtin = Dbf.LoadDbfWithAddColumns(strDestSoputkaGtin, out _, ref nCodeOut);

            int NOMENCL = 4;
            for (int i = 0; i < dtTableSoputkaGtin.Rows.Count; i++)
                hNomenclWithGtin.Add((string)dtTableSoputkaGtin.Rows[i][NOMENCL]);
        }

        private void _GetNomenclWithGtin(ref HashSet<string> hNomenclWithGtin) {
            string strDestSoputkaGtin = "";
            string strSrcSoputkaGtin = "";
            if (!DbfWrapper.CheckIfFileExist("SoputkaGtin.dbf", ref strDestSoputkaGtin, ref strSrcSoputkaGtin, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }

            int nCodeOut = -1;
            DataTable dtTableSoputkaGtin = Dbf.LoadDbfWithAddColumns(strDestSoputkaGtin, out _, ref nCodeOut);

            int NOMENCL = 4;
            for (int i = 0; i < dtTableSoputkaGtin.Rows.Count; i++)
                hNomenclWithGtin.Add((string)dtTableSoputkaGtin.Rows[i][NOMENCL]);
        }             
        private bool _CheckSelItemsBV() {
            if (dataGridViewBV.SelectedRows.Count == 0)
                return false;
            HashSet<string> hProducts = [];
            _GetSelProductsBV(ref hProducts);

            HashSet<string> hNomenclWithGtin = [];
            _GetNomenclWithGtinBV(ref hNomenclWithGtin);

            int nCount1 = 0;
            List<string> listProductsToQR = [];
            foreach (string strProd in hProducts) {
                if (!hNomenclWithGtin.Contains(strProd)) {
                    nCount1++;
                    continue;
                }
                listProductsToQR.Add(strProd);
            }
            if (0 == listProductsToQR.Count) {
                //MessageBox.Show(this, "Ошибка, Нет выбранных элементов с GTIN для получения QR кодов (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show(this, "Ошибка, для выбранных элементов отсутствуют GTIN (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (nCount1 > 0) {
                MessageBox.Show(this, "Ошибка, для :" + nCount1 + " выбранных элементов отсутствуют GTIN (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private bool _CheckSelItems() {
            if (dataGridViewSoputka.SelectedRows.Count == 0)
                return false;
            HashSet<string> hProducts = [];
            _GetSelProducts(ref hProducts);

            HashSet<string> hNomenclWithGtin = [];
            _GetNomenclWithGtin(ref hNomenclWithGtin);

            int nCount1 = 0;
            List<string> listProductsToQR = [];
            foreach (string strProd in hProducts) {
                if (!hNomenclWithGtin.Contains(strProd)) {
                    nCount1++;
                    continue;
                }
                listProductsToQR.Add(strProd);
            }
            if (0 == listProductsToQR.Count) {
                //MessageBox.Show(this, "Ошибка, Нет выбранных элементов с GTIN для получения QR кодов (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show(this, "Ошибка, для выбранных элементов отсутствуют GTIN (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (nCount1 > 0) {
                MessageBox.Show(this, "Ошибка, для :" + nCount1 + " выбранных элементов отсутствуют GTIN (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }       
        private bool _GetNotProcessedGTIN_BV(string strDestPathOrdersKM, string strSrcPathOrdersKM, ref DateTime dtBvGtinOrdersMkLoaded, ref HashSet<long> hsNotProcessedGTIN) {
            DateTime dt = _GetLastWriteTime(strSrcPathOrdersKM);
            if (dtBvGtinOrdersMkLoaded != dt) {
                MessageBox.Show(this, "Данные изменились, Необходимо обновить таблицу с товаром", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            int nCodeOut = -1;

            int GTIN = 3;
            int STATUS = 5;
            DataTable dtTableOrdersKM = Dbf.LoadDbfWithAddColumns(strDestPathOrdersKM, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableOrdersKM.Rows.Count; i++) {
                if (System.DBNull.Value != dtTableOrdersKM.Rows[i][STATUS])
                    continue;
                if (System.DBNull.Value == dtTableOrdersKM.Rows[i][GTIN])
                    continue;
                hsNotProcessedGTIN.Add((long)dtTableOrdersKM.Rows[i][GTIN]);
            }
            if (hsNotProcessedGTIN.Count == dataGridViewBV.SelectedRows.Count) {
                string strBody1 = "Не могу получить коды маркировки, на выбранные GTIN: еще не обработаны предыдущие запросы";
                MessageBox.Show(this, strBody1, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }
        private void _LoadDataSoputka(string strFileName, DataGridView dgv, GridViewExtensions.DataGridFilterExtender dgFE, ref Dictionary<int, int> hNumTableSPO,
           PRODUCT product = PRODUCT.PR_SOPUTKA, MarkingPaths markingPaths = null, string strOrderID = "", string strShopPrefix = "") {
            string strDestPathSoputka = "";
            string strSrcPathSoputka = "";

            if (null == markingPaths) {
                if (PRODUCT.PR_SOPUTKA == product) {
                    if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, @"\Soputka\generated\")) { this.Cursor = Cursors.Default; return; }
                }
            }
            string strDestPathOrdersKM = "";
            string strSrcPathOrdersKM = "";
            if (null == markingPaths) {
                if (PRODUCT.PR_SOPUTKA == product) {
                    if (!DbfWrapper.CheckIfFileExist("SoputkaOrdersKM.dbf", ref strDestPathOrdersKM, ref strSrcPathOrdersKM, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }
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
                    if (!DbfWrapper.CheckIfFileExist("SoputkaKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }
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
            MarkingSP.GetUseColumns(ref hUseColumns);//переделать          

            _dtSoputkaGtinOrdersMkLoaded = _GetLastWriteTime(strSrcPathOrdersKM);

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
                    string strKey = lGTIN.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString() + "_" + _GetShopPrefix(PRODUCT.PR_SOPUTKA);
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
            MarkingSP.InidataGridViewSoputka(ref dgv);
            //hGtin_CountKM
            //this.Cursor = Cursors.Default;
        }
        private bool _GetNotProcessedGTIN(string strDestPathOrdersKM, string strSrcPathOrdersKM, ref HashSet<long> hsNotProcessedGTIN) {
            //_dtSoputkaGtinOrdersMkLoaded
            DateTime dt = _GetLastWriteTime(strSrcPathOrdersKM);
            if (_dtSoputkaGtinOrdersMkLoaded != dt) {
                MessageBox.Show(this, "Данные изменились, Необходимо обновить таблицу с товаром", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            int nCodeOut = -1;

            int GTIN = 3;
            int STATUS = 5;
            DataTable dtTableOrdersKM = Dbf.LoadDbfWithAddColumns(strDestPathOrdersKM, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableOrdersKM.Rows.Count; i++) {
                if (System.DBNull.Value != dtTableOrdersKM.Rows[i][STATUS])
                    continue;
                if (System.DBNull.Value == dtTableOrdersKM.Rows[i][GTIN])
                    continue;
                hsNotProcessedGTIN.Add((long)dtTableOrdersKM.Rows[i][GTIN]);
            }
            if (hsNotProcessedGTIN.Count == dataGridViewSoputka.SelectedRows.Count) {
                string strBody1 = "Не могу получить коды маркировки, на выбранные GTIN: еще не обработаны предыдущие запросы";
                MessageBox.Show(this, strBody1, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }        
        private bool _CreateOrdersInFairMarkBV(out Dictionary<string, string> dicGtin_OrderID_Received, out string strInfoOut) {
            dicGtin_OrderID_Received = [];
            strInfoOut = "";

            string strDestPathOrdersKM = "";
            string strSrcPathOrdersKM = "";
            if (!DbfWrapper.CheckIfFileExist("BvOrdersKM.dbf", ref strDestPathOrdersKM, ref strSrcPathOrdersKM, @"\BV\")) { this.Cursor = Cursors.Default; return false; }

            HashSet<long> hsNotProcessedGTIN = [];
            if (!_GetNotProcessedGTIN_BV(strDestPathOrdersKM, strSrcPathOrdersKM, ref _dtBvGtinOrdersMkLoaded, ref hsNotProcessedGTIN))
                return false;

            int BARCODE = 27;
            int KOL = 10;
            int GTIN = 23;
            int KOL_KM = 24;
            int IZDNAME = 28;
            int nErrors = 0;
            int nOK = 0;
            int nCreatedBefore = 0;
            int nNotProcessed = 0;
            foreach (DataGridViewRow gvRow in dataGridViewBV.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                long lGTIN = (long)row[GTIN];
                string strGtin0 = lGTIN.ToString();
                string strGtin1 = lGTIN.ToString() + "_" + row[BARCODE].ToString();
                string strGtin2 = "0" + strGtin0;
                string strBARCODE = "SP BARCODE: " + row[BARCODE].ToString();
                int nCount = (int)row[KOL] - (int)row[KOL_KM];
                nCount = (int)row[KOL];          
                if (nCount <= 0) {
                    Trace.WriteLine("Для GTIN:" + strGtin1 + ", уже ранее были заказаны QR коды");
                    nCreatedBefore++;
                    continue;
                }
                if (hsNotProcessedGTIN.Contains(lGTIN)) {
                    nNotProcessed++;
                    continue;
                }
                bool bKomplekt = false;
                string strIzdName = (string)row[IZDNAME];
                int nPos = strIzdName.ToLower().LastIndexOf("костюм");
                int nPos2 = strIzdName.ToLower().LastIndexOf("комплект");
                if (-1 != nPos || -1 != nPos2)
                    bKomplekt = true;


                string strCreateID = _CreateOrder(strGtin2, nCount, strBARCODE, bKomplekt);
                if ("" == strCreateID) {
                    nErrors++;
                    Trace.WriteLine("Не создан заказ на Коды Маркировки на: " + nCount.ToString() + " штук, для GTIN:" + strGtin1);
                    continue;
                }
                Trace.WriteLine("Создан заказ на Коды Маркировки: " + strCreateID + " на: " + nCount.ToString() + " штук, для GTIN:" + strGtin1);
                nOK++;
                dicGtin_OrderID_Received.Add(strGtin1, strCreateID);
            }
            if (nCreatedBefore == dataGridViewBV.SelectedRows.Count) {
                MessageBox.Show(this, "Для выбранных GTIN, QR коды уже были заказаны", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (0 == dicGtin_OrderID_Received.Count) {
                if (1 == dataGridViewBV.SelectedRows.Count)
                    MessageBox.Show(this, "Ошибка: номер заказа для маркиовки не получен, повторите ваши действия чуть позже", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "Ошибка: номера заказов для маркиовки не получены, повторите ваши действия чуть позже", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            strInfoOut = "Создано успешно: " + nOK.ToString() + " заказов на Коды Маркировки,\n не создано: заказов: " + nErrors.ToString() + ",\n уже было создано ранее заказов: " + nCreatedBefore.ToString() + ", \n еще не обработано заказов: " + nNotProcessed.ToString();
            return true;
        }

        private bool _CreateOrdersInFairMark(out Dictionary<string, string> dicGtin_OrderID_Received, out string strInfoOut) {
            dicGtin_OrderID_Received = [];
            strInfoOut = "";

            string strDestPathOrdersKM = "";
            string strSrcPathOrdersKM = "";
            if (!DbfWrapper.CheckIfFileExist("SoputkaOrdersKM.dbf", ref strDestPathOrdersKM, ref strSrcPathOrdersKM, @"\Soputka\")) { this.Cursor = Cursors.Default; return false; }

            HashSet<long> hsNotProcessedGTIN = [];
            if (!_GetNotProcessedGTIN(strDestPathOrdersKM, strSrcPathOrdersKM, ref hsNotProcessedGTIN))
                return false;

            int BARCODE = 3;
            int KOL = 11;
            int GTIN = 27;
            int KOL_KM = 28;
            int IZDNAME = 7;
            int nErrors = 0;
            int nOK = 0;
            int nCreatedBefore = 0;
            int nNotProcessed = 0;
            foreach (DataGridViewRow gvRow in dataGridViewSoputka.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                long lGTIN = (long)row[GTIN];
                string strGtin0 = lGTIN.ToString();
                string strGtin1 = lGTIN.ToString() + "_" + row[BARCODE].ToString();
                string strGtin2 = "0" + strGtin0;
                string strBARCODE = "SP BARCODE: " + row[BARCODE].ToString();
                int nCount = (int)row[KOL] - (int)row[KOL_KM];
                nCount = (int)row[KOL];         
                if (nCount <= 0) {
                    Trace.WriteLine("Для GTIN:" + strGtin1 + ", уже ранее были заказаны QR коды");
                    nCreatedBefore++;
                    continue;
                }
                if (hsNotProcessedGTIN.Contains(lGTIN)) {
                    nNotProcessed++;
                    continue;
                }
                bool bKomplekt = false;
                string strIzdName = (string)row[IZDNAME];
                int nPos = strIzdName.ToLower().LastIndexOf("костюм");
                if (-1 != nPos)
                    bKomplekt = true;

                string strCreateID = _CreateOrder(strGtin2, nCount, strBARCODE, bKomplekt);
                if ("" == strCreateID) {
                    nErrors++;
                    Trace.WriteLine("Не создан заказ на Коды Маркировки на: " + nCount.ToString() + " штук, для GTIN:" + strGtin1);
                    continue;
                }
                Trace.WriteLine("Создан заказ на Коды Маркировки: " + strCreateID + " на: " + nCount.ToString() + " штук, для GTIN:" + strGtin1);
                nOK++;
                dicGtin_OrderID_Received.Add(strGtin1, strCreateID);
            }
            if (nCreatedBefore == dataGridViewSoputka.SelectedRows.Count) {
                MessageBox.Show(this, "Для выбранных GTIN, QR коды уже были заказаны", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (0 == dicGtin_OrderID_Received.Count) {
                if (1 == dataGridViewSoputka.SelectedRows.Count)
                    MessageBox.Show(this, "Ошибка: номер заказа для маркиовки не получен, повторите ваши действия чуть позже", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "Ошибка: номера заказов для маркиовки не получены, повторите ваши действия чуть позже", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            strInfoOut = "Создано успешно: " + nOK.ToString() + " заказов на Коды Маркировки,\n не создано: заказов: " + nErrors.ToString() + ",\n уже было создано ранее заказов: " + nCreatedBefore.ToString() + ", \n еще не обработано заказов: " + nNotProcessed.ToString();
            return true;
        }
        private bool _AddCreatedOrdersToDB(string strDestSoputkaOrdersMK, string strSrcSoputkaOrdersMK, Dictionary<string, string> dicGtin_OrderID_Received) {
            int nCodeOut = -1;
            DataTable dtTableSoputkaOrdersMK = Dbf.LoadDbfWithAddColumns(strDestSoputkaOrdersMK, out _, ref nCodeOut, "№", "1");
            dtTableSoputkaOrdersMK.Rows.Clear();

            int GTIN2 = 3;
            int ORDER_MARK = 4;
            int DATECREATE = 8;
            int BARCODE = 9;
            DateTime dt = DateTime.Now;
            List<string> lMsg = [];
            foreach (var item in dicGtin_OrderID_Received) {
                string[] parms = item.Key.Split('_');
                string strGTIN = parms[0];
                string strBARCODE = parms[1];
                System.Data.DataRow rowAdd = dtTableSoputkaOrdersMK.NewRow();
                rowAdd[GTIN2] = strGTIN;
                rowAdd[BARCODE] = strBARCODE;
                rowAdd[ORDER_MARK] = item.Value;
                rowAdd[DATECREATE] = dt;
                dtTableSoputkaOrdersMK.Rows.Add(rowAdd);
                lMsg.Add("ERROR, данные не попавшие в SoputkaOrdersKM.dbf, GTIN:" + strGTIN + " ORDER_MARK: " + item.Value + " DATECREATE:" + dt.ToString("dd.MM.yyyy"));
            }

            if (dtTableSoputkaOrdersMK.Rows.Count == 0) {
                MessageBox.Show(this, "Ошибка: не должно быть 0 элементов в dtTableSoputkaOrdersMK", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Dbf.AddTable(strDestSoputkaOrdersMK, dtTableSoputkaOrdersMK)) {
                foreach (string str in lMsg)
                    Trace.WriteLine(str);
                MessageBox.Show(this, "Ошибка не удалось добавить в базу данных новые GTIN и номера заказов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestSoputkaOrdersMK, strSrcSoputkaOrdersMK, true);
            } catch (Exception ex) {
                foreach (string str in lMsg)
                    Trace.WriteLine(str);
                MessageBox.Show(this, "Ошибка " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private string _UpdateRazmer(string strTypeProduct, string strRazmer) {
            int nPos1 = strRazmer.IndexOf('(');
            int nPos2 = strRazmer.IndexOf(')');
            int nPos3 = strRazmer.IndexOf('/');
            int nPos4 = strRazmer.IndexOf('-');

            if ("БРЮКИ" == strTypeProduct && -1 != nPos1)
                return strRazmer.Substring(0, nPos1);

            if ("БРЮКИ" == strTypeProduct && -1 != nPos4)
                return strRazmer[..nPos4];

            if ("БРЮКИ" == strTypeProduct && -1 != nPos3)
                return strRazmer[..nPos3];

            if (-1 != nPos1 && -1 != nPos2 && nPos2 > nPos1) {
                return strRazmer.Substring(nPos1 + 1, nPos2 - nPos1 - 1);
            }
            return strRazmer;
        }
        private bool IsFileSetingsExist() {
            string strFileSettingPath = @"C:\Po_BOLSHEVICHKA\PrintStickerServer\setting.dbf";
            if (!File.Exists(strFileSettingPath)) {
                MessageBox.Show(this, "Отсутствует файл:" + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }        
        private void _GetQRCodeBV() {
            if (!IsFileSetingsExist())
                return;

            if (!_CheckSelItemsBV())
                return;

            //get dtTableSoputkaGtinOrdersMK for ADD items
            string strDestBVGtinOrdersMK = "";
            string strSrcBVGtinOrdersMK = "";
            if (!DbfWrapper.CheckIfFileExist("BvOrdersKM.dbf", ref strDestBVGtinOrdersMK, ref strSrcBVGtinOrdersMK, @"\BV\")) { this.Cursor = Cursors.Default; return; }

            //Dictionary<string, string> dicGtin_OrderID_Received;
            //string strInfoOut = "";
            if (!_CreateOrdersInFairMarkBV(out Dictionary<string, string> dicGtin_OrderID_Received, out string strInfoOut))
                return;

            if (!_AddCreatedOrdersToDB(strDestBVGtinOrdersMK, strSrcBVGtinOrdersMK, dicGtin_OrderID_Received))
                return;

            Trace.WriteLine(strInfoOut);
            MessageBox.Show(this, strInfoOut, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        private void _GetQRCode() {
            if (!IsFileSetingsExist())
                return;

            if (!_CheckSelItems())
                return;
            //get dtTableSoputkaGtinOrdersMK for ADD items
            string strDestSoputkaGtinOrdersMK = "";
            string strSrcSoputkaGtinOrdersMK = "";
            if (!DbfWrapper.CheckIfFileExist("SoputkaOrdersKM.dbf", ref strDestSoputkaGtinOrdersMK, ref strSrcSoputkaGtinOrdersMK, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }

            //Dictionary<string, string> dicGtin_OrderID_Received;
            //string strInfoOut = "";
            if (!_CreateOrdersInFairMark(out Dictionary<string, string> dicGtin_OrderID_Received, out string strInfoOut))
                return;

            if (!_AddCreatedOrdersToDB(strDestSoputkaGtinOrdersMK, strSrcSoputkaGtinOrdersMK, dicGtin_OrderID_Received))
                return;

            Trace.WriteLine(strInfoOut);
            MessageBox.Show(this, strInfoOut, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }       
        public string GetNomencl(DataRow row) {
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
            if ("" != MarkingBV.GetSostav(strCCLOTH))
                strSostav = " состав " + MarkingBV.GetSostav(strCCLOTH);
            string strCvet = "";
            if ("" != strCCODE)
                strCvet = " цвет " + strCCODE;
            string strRis = "";
            if ("" != strPATTERN)
                strRis = " рис." + strPATTERN;
            return strMod + " арт." + strART + strRazmer + strSostav + strCvet + strRis + " страна " + strART2;
        }       
        private void _GetSelProductsBV(ref HashSet<string> hProducts) {
            foreach (DataGridViewRow gvRow in dataGridViewBV.SelectedRows)
                hProducts.Add(MarkingBV.GetNomenclBV(((DataRowView)gvRow.DataBoundItem).Row));
        }
        private void _GetSelProducts(ref HashSet<string> hProducts) {
            foreach (DataGridViewRow gvRow in dataGridViewSoputka.SelectedRows)
                hProducts.Add(GetNomencl(((DataRowView)gvRow.DataBoundItem).Row));
        }        
        private void _ExportToExcelBV() {
            if (dataGridViewBV.SelectedRows.Count == 0)
                return;
            string strDestPathToGTIN = "";
            string strSrcPathToGTIN = "";
            if (!DbfWrapper.CheckIfFileExist("toGTIN.xlsx", ref strDestPathToGTIN, ref strSrcPathToGTIN, @"\Soputka\patern\")) { this.Cursor = Cursors.Default; return; }

            HashSet<string> hProducts = [];
            _GetSelProductsBV(ref hProducts);

            HashSet<string> hNomenclWithGtin = [];
            _GetNomenclWithGtinBV(ref hNomenclWithGtin);

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

                ToExcelItem tei = new() {
                    strColumn5_ProductName = strProdict
                };

                int nPos5 = strProdict.Length;

                int nPosANext = nPos0;
                if (-1 == nPosANext) nPosANext = nPos1;
                if (-1 == nPosANext) nPosANext = nPos2;
                if (-1 == nPosANext) nPosANext = nPos3;
                if (-1 == nPosANext) nPosANext = nPos31;
                if (-1 == nPosANext) nPosANext = nPos4;
                if (-1 == nPosANext) nPosANext = nPos5;

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

                tei.strColumn19_Sostav = MarkingBV.GetSostav(strSostav);

                string strTypeProduct = strProdict.Substring(0, nPos0);

                string strMod1 = strProdict.Substring(nPosA + " мод.".Length, nPosANext - nPosA - " мод.".Length);
                string strMod2 = strProdict.Substring(nPos0 + " арт.".Length, nPos0Next - nPos0 - " арт.".Length);
                tei.strColumn17_Mod = strMod1 + " " + strMod2;

                tei.strColumn7_Country = _GetCountry(strCountry);

                tei.strColumn8_INN = "7708029923";
                tei.strColumn9_IZGOTOVITEL = "АО 'Большевичка'";
                tei.strColumn10_SortProduct = _GetTypeProduct(strTypeProduct);
                tei.strColumn12_TNVED = _GetTNVED(strTypeProduct, strSostav, tei.strColumn17_Mod);
                tei.strColumn14_Razmer = strRazmer;

                int nPos6 = strRazmer.LastIndexOf(')');
                if (-1 != nPos6)
                    tei.strColumn15_Rost = strRazmer.Substring(1, nPos6 - 1);

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
                MessageBox.Show(this, "Для всех выбранных элементов уже ранее были получены GTIN", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ExcelExport excExp = new();
            if (!excExp.Open(strDestPathToGTIN, out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(this, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!excExp.ExportData(xlsWorkSheet, ref listToExcelItems)) {
                MessageBox.Show(this, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(this, "Успешно сгенерирован файл:" + strDestPathToGTIN, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void _ExportToExcel() {
            if (dataGridViewSoputka.SelectedRows.Count == 0)
                return;
            string strDestPathToGTIN = "";
            string strSrcPathToGTIN = "";
            if (!DbfWrapper.CheckIfFileExist("toGTIN.xlsx", ref strDestPathToGTIN, ref strSrcPathToGTIN, @"\Soputka\patern\")) { this.Cursor = Cursors.Default; return; }

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
                    //product = PRODUCT.PR_SOPUTKA,
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

                tei.strColumn19_Sostav = MarkingBV.GetSostav(strSostav);

                string strTypeProduct = strProdict[..nPos0];

                tei.strColumn17_Mod = strProdict.Substring(nPos0 + " арт.".Length, nPos0Next - nPos0 - " арт.".Length);

                tei.strColumn7_Country = _GetCountry(strCountry);
                tei.strColumn10_SortProduct = _GetTypeProduct(strTypeProduct);
                tei.strColumn12_TNVED = _GetTNVED(strTypeProduct, strSostav, tei.strColumn17_Mod);
                tei.strColumn14_Razmer = _UpdateRazmer(strTypeProduct, strRazmer);
                tei.strColumn16_Cvet = _GetCvet(strCvet);

                listToExcelItems.Add(tei);
            }
            if (0 == listToExcelItems.Count) {
                MessageBox.Show(this, "Для всех выбранных элементов уже ранее были получены GTIN", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ExcelExport excExp = new();
            if (!excExp.Open(strDestPathToGTIN, out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(this, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!excExp.ExportData(xlsWorkSheet, ref listToExcelItems)) {
                MessageBox.Show(this, "Произошла ошибка при открытии файла:" + strDestPathToGTIN, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(this, "Успешно сгенерирован файл:" + strDestPathToGTIN, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void contextMenuGTIN_Opening(object sender, CancelEventArgs e) {
            menuLoadByBarcodelSP.Visible = false;
            if (rbSGP.Checked)
                menuLoadByBarcodelSP.Visible = true;


            menuMarkDelSoputka.Visible = false;

            if (rbALL.Checked) {
                menuExportToExcel.Visible = true;
                menuImportGTIN.Visible = true;
                menuGetQRCode.Visible = true;
                toolStripMenuItem1.Visible = true;
                toolStripMenuItem2.Visible = true;
                toolStripMenuItem3.Visible = true;

                menuPrint.Visible = false;
                menuCheckStikers.Visible = false;
                toolStripMenuItem5.Visible = false;

                menuImportGTIN.Enabled = true;
                menuPrint.Enabled = false;
                menuCheckStikers.Enabled = false;
            } else if (rbSGP.Checked) {
                menuExportToExcel.Visible = true;
                menuImportGTIN.Visible = true;
                menuGetQRCode.Visible = true;
                toolStripMenuItem1.Visible = true;
                toolStripMenuItem2.Visible = true;
                toolStripMenuItem3.Visible = true;

                menuPrint.Visible = true;
                menuCheckStikers.Visible = true;
                toolStripMenuItem5.Visible = true;
                menuMarkDelSoputka.Visible = true;
            } else {
                menuExportToExcel.Visible = false;
                menuImportGTIN.Visible = false;
                menuGetQRCode.Visible = true;
                toolStripMenuItem1.Visible = false;
                toolStripMenuItem2.Visible = false;
                toolStripMenuItem3.Visible = false;

                menuPrint.Visible = true;
                menuCheckStikers.Visible = true;
                toolStripMenuItem5.Visible = true;
            }
            _HideMenuPrintUser();
            if (0 == dataGridViewSoputka.SelectedRows.Count)
                menuMarkDelSoputka.Enabled = false;
            else
                menuMarkDelSoputka.Enabled = true;

            if (0 == dataGridViewSoputka.SelectedRows.Count) {
                menuExportToExcel.Enabled = false;
                menuGetQRCode.Enabled = false;
                menuPrint.Enabled = false;
                return;
            }
            menuExportToExcel.Enabled = true;
            menuGetQRCode.Enabled = true;
            menuPrint.Enabled = true;

            menuCheckStikers.Enabled = true;
            _HideMenuPrintUser();

        }
        private void _HideMenuPrintUser() {
            if (bPRINT_USER_SOPUTKA) {
                menuExportToExcel.Visible = false;
                toolStripMenuItem1.Visible = false;
                menuImportGTIN.Visible = false;
                toolStripMenuItem2.Visible = false;
                menuGetQRCode.Visible = false;
                toolStripMenuItem3.Visible = false;
                menuPrintRepeat.Visible = false;
                menuPrintRepeatSelected.Visible = false;
            }
        }
        private bool _ImportGtinFromExcel(string strDestSoputkaGtin, string strSrcSoputkaGtin, string strCompanyNmae) {
            openFileDlg.CheckFileExists = true;
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "Эксель файлы(*.xlsx)|*.xlsx";

            if (openFileDlg.ShowDialog() != DialogResult.OK)
                return false;
            string strFileName = openFileDlg.FileName;
            ExcelExport excExp = new();
            if (!excExp.Open(strFileName, out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(this, "Произошла ошибка при открытии файла:" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!excExp.GetData(xlsWorkSheet, strCompanyNmae, out List<FromExcelItem> listFromExcelItems)) {
                MessageBox.Show(this, "Произошла ошибка при получении данных из файла:" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (listFromExcelItems.Count == 0) {
                MessageBox.Show(this, "в Excel не найдено GTIN", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show(this, "ОШИБКА, GTIN: " + item.llColumn2_GTIN + ", существует в файле: " + strSrcSoputkaGtin + ", но номенклатуры отличаются в файле и в EXCEL", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(this, "Новых GTIN не найдено (GTIN из Excel уже есть в БД: " + strSrcSoputkaGtin + ")", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (!Dbf.AddTable(strDestSoputkaGtin, dtTableSoputkaGtin)) {
                MessageBox.Show(this, "Ошибка не удалось добавить в базу данных новые GTIN", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestSoputkaGtin, strSrcSoputkaGtin, true);
                MessageBox.Show(this, "Добавлено: " + dtTableSoputkaGtin.Rows.Count + " кодов GTIN в файл: " + strSrcSoputkaGtin, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                MessageBox.Show(this, "Ошибка " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private void menuImportGTIN_Click(object sender, EventArgs e) {
            string strDestSoputkaGtin = "";
            string strSrcSoputkaGtin = "";
            if (!DbfWrapper.CheckIfFileExist("SoputkaGtin.dbf", ref strDestSoputkaGtin, ref strSrcSoputkaGtin, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }

            if (!_ImportGtinFromExcel(strDestSoputkaGtin, strSrcSoputkaGtin, ""))
                return;
            _ShowPageSoputka();
        }

        private void menuGetQRCode_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            _GetQRCode();
            this.Cursor = Cursors.Default;
        }
        private void button4_Click(object sender, EventArgs e) {
            _ShowPageSoputka();
        }
        private void btGetToken_Click(object sender, EventArgs e) {
        }       
        private string _CreateOrder(string strGTIN = "", int nCount = 1, string strBARCODE = "BARCODE", bool bKomplekt = false,
            string strCompanyID = _strSettingID_BV, bool bSandbox = false) {
            string strFile = _GetCurDir() + "\\requests\\CreateOrderM.txt";

            if(bSandbox)
                strFile = _GetCurDir() + "\\requests\\CreateOrderFab.txt";

            if (!File.Exists(strFile)) {
                MessageBox.Show("нет файла: " + strFile);
                return "";
            }
            string strRequest = File.ReadAllText(strFile).Replace("\r\n", " ");
            strRequest = strRequest.Replace("22222", nCount.ToString());
            strRequest = strRequest.Replace("33333", strBARCODE);
            if ("" != strGTIN)
                strRequest = strRequest.Replace("11111", strGTIN);
            if (bKomplekt)
                strRequest = strRequest.Replace("UNIT", "BUNDLE");


            FairMark fm = new(strCompanyID, bSandbox);
            CreateOrderRespons resp = fm.CreateOrder(strRequest);
            if ("" == resp.orderId) {
                if (null != resp.err && resp.err.fieldErrors.Count > 0)
                    MessageBox.Show("Ошибка: " + resp.err.fieldErrors[0].fieldError + ", при запросе: " + strRequest, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            return resp.orderId;
        }
        private bool _RegisterPO(string strAdress, string strName) {
            txtAns1.Text = "";
            string strFile = _GetCurDir() + "\\requests\\RegisterPO.txt";
            if (!File.Exists(strFile)) {
                MessageBox.Show("нет файла: " + strFile);
                return false;
            }
            string strRequest = File.ReadAllText(strFile).Replace("\r\n", " ");
            strRequest = strRequest.Replace("11111", strAdress);
            strRequest = strRequest.Replace("22222", strName);

            FairMark fm = new("05", true);//номер клиенрта из setting.dbf , 05 - для ПЕСОЧНИЦЫ
            RegisterRespons resp = fm.RegisterPO(strRequest);
            if ("SUCCESS" != resp.status) {
                if (null != resp.err && resp.err.fieldErrors.Count > 0) {
                    txtAns1.Text = "Ошибка: " + resp.err.fieldErrors[0].fieldError + ", при запросе: " + strRequest;
                    MessageBox.Show("Ошибка: " + resp.err.fieldErrors[0].fieldError + ", при запросе: " + strRequest, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (null != resp.err && resp.err.globalErrors.Count > 0) {
                    txtAns1.Text = "Ошибка: " + resp.err.globalErrors[0].error + ", при запросе: " + strRequest;
                    MessageBox.Show("Ошибка: " + resp.err.globalErrors[0].error + ", при запросе: " + strRequest, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            txtAns1.Text = "ПО зарегистрировано успешно \n:" + resp.strRespons;
            return true;
        }

        private void btCreateOrder_Click(object sender, EventArgs e) {
            txtToken.Text = _CreateOrder();
        }

        private void btGetInfoFromOrder_Click(object sender, EventArgs e) {
        }
        private void menuCreateOrderOnMarkirovka_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            _CreateOrderOnMarkirovka();
            this.Cursor = Cursors.Default;
        }
        private void _CreateOrderOnMarkirovka() {
            if (dataGridViewSoputka.SelectedRows.Count == 0)
                return;
            HashSet<string> hProducts = [];
            _GetSelProducts(ref hProducts);

            HashSet<string> hNomenclWithGtin = [];
            _GetNomenclWithGtin(ref hNomenclWithGtin);

            int nCount1 = 0;
            List<string> listProductsToQR = [];
            foreach (string strProd in hProducts) {
                if (!hNomenclWithGtin.Contains(strProd)) {
                    nCount1++;
                    continue;
                }
                listProductsToQR.Add(strProd);
            }
            if (0 == listProductsToQR.Count) {
                MessageBox.Show(this, "Ошибка, Нет выбранных элементов с GTIN для создания заказа на маркировку(предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (nCount1 > 0) {
                MessageBox.Show(this, "Ошибка, для :" + nCount1 + " выбранных элементов отсутствуют GTIN (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            return;
        }
        private void menuUpdate_Click(object sender, EventArgs e) {
            _ShowPageSoputka();
        }
        private void menuPrint_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            DlgSelRazmer dlg = new(true);
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            _Print(dlg.GetPrintParms(), PRODUCT.PR_SOPUTKA);
            this.Cursor = Cursors.Default;
        }        
        private bool _PrintRow(PrintParms pp, PRODUCT product, DataRow row, string strDestPathKM, string strDestPathPaternSP,
            ref int nPrintedAll, List<KM> listKM = null, bool bLastRow = false, bool bPrintOnFile = false) {
            if (product == PRODUCT.PR_SOPUTKA)
                return _PrintRowSoputka(pp, product, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, bLastRow);
            return _PrintRowBV(product, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, pp, listKM, bLastRow, bPrintOnFile);
        }


        private string _CorrectRazmAddNull(string strRAZ) {
            string[] parms = strRAZ.Split('-');
            //string str2 = "";
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

        private string _CorrectRazm(string strRAZ) {
            string[] parms = strRAZ.Split('-');
            string str2;
            if (parms.Length > 2) {
                string str1 = parms[parms.Length - 1];
                if (str1.Length > 2 && "0" == str1[..1]) {
                    str2 = str1.Substring(1);
                    strRAZ = strRAZ.Replace(str1, str2);
                }
                str1 = parms[parms.Length - 2];
                if (str1.Length > 2 && "0" == str1[..1]) {
                    str2 = str1.Substring(1);
                    strRAZ = strRAZ.Replace(str1, str2);
                }
            }
            return strRAZ;
        }
        private bool _PrintRowBV(PRODUCT product, DataRow row, string strDestPathKM, string strDestPathPaternSP, ref int nPrintedAll,
            PrintParms pp, List<KM> listKM = null, bool bLastRow = false, bool bPrintOnFile = false) {
            //, bool bFlabelToBarcode = false
            if (null == pp) {
                pp = new() { bMultySize = true, strDate = "03 2024", strPrefix = "" };
            }

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
            //int FLABEL = 20;
            int GTIN = 23;

            int nCodeOut = -1;

            string strBARCODE = row[BARCODE].ToString();
            string strMOD = row[MOD].ToString();

            //strMOD = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С БРЮКАМИ", "КОСТЮМ ЖЕНСКИЙ БР.");
            //strMOD = strMOD.Replace("КОСТЮМ ЖЕНСКИЙ С ЮБКОЙ", "КОСТЮМ ЖЕНСКИЙ ЮБ.");

            string strART = row[ART].ToString();
            string strART_second = "";

            string strModel = row[MOD2].ToString() + "" + row[PRS].ToString();
           
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
                    strART = strART[..nPosPr];
                    strART_second = strPostfix.Trim();
                }
                if (strART.Length > 17) {
                    strART_second = strART.Substring(17) + strPostfix;
                    strART = strART[..17];
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

                string strPrefix = _GetShopPrefix(product);
                if ("" != pp.strPrefix)
                    strPrefix = pp.strPrefix;
                
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
                    string[] lines = File.ReadAllLines(strDestPathPaternSP, Encoding.GetEncoding(866));
                    for (int i = 0; i < lines.Length; i++) {
                        lines[i] = lines[i].Replace("#01", listKM[z].N.ToString("00000"));
                        lines[i] = lines[i].Replace("#02", strPrefix);
                        //lines[i] = lines[i].Replace("#03", txt03.Text);
                        
                         lines[i] = lines[i].Replace("#04", strBARCODE);


                        lines[i] = lines[i].Replace("#05", strMOD1.PadLeft(18));
                        if (strMOD1 != strMOD2)
                            lines[i] = lines[i].Replace("#32", strMOD2.PadLeft(18));


                        lines[i] = lines[i].Replace("#06", strART);
                        lines[i] = lines[i].Replace("#07", strART_second);
                        lines[i] = lines[i].Replace("#08", _CorrectRazm(strRAZ).PadLeft(15));
                        lines[i] = lines[i].Replace("#09", strART2);
                        //lines[i] = lines[i].Replace("#10", txt10.Text);
                        lines[i] = lines[i].Replace("#11", strCCODE);
                        //lines[i] = lines[i].Replace("#12", txt12.Text);
                        lines[i] = lines[i].Replace("#13", "");

                        if (product == PRODUCT.PR_BV)
                            lines[i] = lines[i].Replace("#14", "АО \"Большевичка\"");
                        //else if (product == PRODUCT.PR_VETEX_PRODUCTION || product == PRODUCT.PR_VETEX_IMPORT || product == PRODUCT.PR_VETEX_REMAINS)
                        //    lines[i] = lines[i].Replace("#14", "ООО \"Ветекс\"");

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
                                string strValue1 = strKN[..nPos1];
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

                        if (product == PRODUCT.PR_BV)
                            lines[i] = lines[i].Replace("#31", pp.strDate); //"03 2024");
                    }
                    if (nPrintedAll % 30 == 0 && 0 != nPrintedAll) {
                        DialogResult rez = MessageBox.Show(this, "Напечатали: " + nPrintedAll.ToString() + ", стикеров, продолжить?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (rez == DialogResult.No) {
                            Trace.WriteLine("Прервали печать");
                            return false;
                        }
                        if (!bPrintOnFile)
                            _CheckStikers(product);
                    }
                    File.WriteAllLines(strNewFileName, lines, Encoding.GetEncoding(866));
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
                    _CheckStikers(product);
                //else
                //печатаем без кода маркировки
            }

            return true;
        }
        //bool bSoputks
        private bool _PrintRowSoputka(PrintParms pp, PRODUCT product, DataRow row, string strDestPathKM, 
            string strDestPathPaternSP, ref int nPrintedAll, List<KM> listKM = null, bool bLastRow = false) {
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
            //
            int STATUS = 9;
            int KM_KM = 5;
            int N = 0;
            if (product != PRODUCT.PR_SOPUTKA) {
                BARCODE = 27;
                MOD = 28;
                ART = 7;
                RAZ = 8;
                KOL = 10;
                ART2 = 13;
                PATTERN = 14;
                CCODE = 15;
                CCLOTH = 16;
                CSEASON = 18;
                GTIN = 23;
            }
            int nCodeOut = -1;

            string strBARCODE = row[BARCODE].ToString();
            string strMOD = row[MOD].ToString();
            string strART = row[ART].ToString();
            string strART_second = "";
            int nLenLine = 17;//18
            int nPosG = strART.ToUpper().LastIndexOf("ЖЕН");
            if (strART.Length > nLenLine && -1 == nPosG) {
                strART_second = strART.Substring(nLenLine);
                strART = strART.Substring(0, nLenLine);
            }
            if (strART.Length > nLenLine && -1 != nPosG) {
                int nPosPr = strART.LastIndexOf(" ");
                string strPostfix = "";
                if (-1 != nPosPr) {
                    strPostfix = strART.Substring(nPosPr);
                    strART = strART.Substring(0, nPosPr);
                    strART_second = strPostfix.Trim();
                }
                if (strART.Length > nLenLine) {
                    strART_second = strART.Substring(nLenLine) + strPostfix;
                    strART = strART.Substring(0, nLenLine);
                }
            }
            strART_second = strART_second.Trim();
            string strRAZ = row[RAZ].ToString();// +"56(170-112-120)";
            if (product != PRODUCT.PR_SOPUTKA && -1 == strRAZ.LastIndexOf('(')) {
                string[] parms = strRAZ.Split('-');
                if (4 == parms.Length)
                    strRAZ = "(" + parms[0] + "-" + parms[1] + ")" + "-" + parms[2] + "-" + parms[3];
                if (3 == parms.Length) {
                    int nRost = Convert.ToInt32(parms[0]);
                    strRAZ = "(" + nRost.ToString() + "-" + (nRost + 6).ToString() + ")" + "-" + parms[1] + "-" + parms[2];
                }
            }
            string strGTIN = row[GTIN].ToString();
            string strART2 = row[ART2].ToString();                                   
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
            int nKol;
            if (null != listKM)
                nKol = listKM.Count;//repeat
            else
                nKol = (int)row[KOL];
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

                string strPrefix = _GetShopPrefix(product);
                if (null != pp && "" != pp.strPrefix)
                    strPrefix = pp.strPrefix;

                for (int z = 0; z < nKol; z++) {
                    string[] lines = File.ReadAllLines(strDestPathPaternSP, Encoding.GetEncoding(866));
                    for (int i = 0; i < lines.Length; i++) {
                        lines[i] = lines[i].Replace("#01", listKM[z].N.ToString("00000"));
                        lines[i] = lines[i].Replace("#02", strPrefix);
                       
                        lines[i] = lines[i].Replace("#04", strBARCODE);

                        lines[i] = lines[i].Replace("#05", strMOD.PadLeft(16));
                        lines[i] = lines[i].Replace("#06", strART);
                        lines[i] = lines[i].Replace("#07", strART_second);
                        //strRAZ = "188-194 50";

                        lines[i] = lines[i].Replace("#08", strRAZ);
                        lines[i] = lines[i].Replace("#09", strART2);
                        //lines[i] = lines[i].Replace("#10", txt10.Text);
                        lines[i] = lines[i].Replace("#11", strCCODE);
                        //lines[i] = lines[i].Replace("#12", txt12.Text);
                        lines[i] = lines[i].Replace("#13", "");
                        //if (product == PRODUCT.PR_VETEX_IMPORT_SOPUTKA)
                        //    lines[i] = lines[i].Replace("#14", "ООО \"Ветекс\"");
                        //else if (product == PRODUCT.PR_WT_REMAINS_SOPUTKA)
                        //    lines[i] = lines[i].Replace("#14", "ООО \"Веспер Трейдинг\"");
                        //else
                            lines[i] = lines[i].Replace("#14", "АО \"Большевичка\"");

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
                    }
                    if (nPrintedAll % 30 == 0 && 0 != nPrintedAll) {
                        DialogResult rez = MessageBox.Show(this, "Напечатали: " + nPrintedAll.ToString() + ", стикеров, продолжить?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (rez == DialogResult.No) {
                            Trace.WriteLine("Прервали печать");
                            return false;
                        }
                        _CheckStikers(product);
                    }
                    File.WriteAllLines(strNewFileName, lines, Encoding.GetEncoding(866));
                    SendTextFileToPrinter(strNewFileName);
                    //Trace.WriteLine("Напечатали стикер");
                    nPrintedAll++;
                }
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (bLastRow)
                _CheckStikers(product);

            return true;
        }

        //bool bSoputks
        private void _PrintOnFile(PRODUCT product) {
            string strDestPathKM = "";
            string strSrcPathKM = "";

            string strDestPathPaternSP = "";
            string strSrcPathPaternSP = "";
            
            DataGridView dgv = null;
            if (product == PRODUCT.PR_SOPUTKA) {
                if (!DbfWrapper.CheckIfFileExist("sticker_SP_01.zpl", ref strDestPathPaternSP, ref strSrcPathPaternSP, @"\1c\Matrix\patern\")) return;
                dgv = dataGridViewSoputka;
            } else if (product == PRODUCT.PR_BV) {
                if (!DbfWrapper.CheckIfFileExist("sticker_BV_01.zpl", ref strDestPathPaternSP, ref strSrcPathPaternSP, @"\1c\Matrix\patern\")) return;
                dgv = dataGridViewBV;
            } 
            openFileDlg.CheckFileExists = true;
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "txt файлы c штрихкодами(*.txt)|*.txt";

            if (openFileDlg.ShowDialog() != DialogResult.OK)
                return;
            this.Cursor = Cursors.WaitCursor;

            string strPathWithBARCODES = openFileDlg.FileName;
            string[] lineBARCODES = File.ReadAllLines(strPathWithBARCODES);
            if (0 == lineBARCODES.Length)
                return;
            for (int i = 0; i < lineBARCODES.Length; i++) {
                string strVal = lineBARCODES[i];
                if (strVal.Length > 2 && strVal[0] == '[' && strVal[strVal.Length - 1] == ']')
                    lineBARCODES[i] = strVal.Substring(1, strVal.Length - 2);
            }

            DlgSelRazmer dlg = new();
            dlg.ShowDialog();

            int nPrintedAll = 0;
            int BARCODE = 27;
            int KOL = 10;
            for (int i = lineBARCODES.Length - 1; i >= 0; i--) {
                string strBARCODE_1 = lineBARCODES[i];
                foreach (DataGridViewRow gvRow in dgv.SelectedRows) {
                    DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                    row[KOL] = 1;
                    string strBARCODE_2 = row[BARCODE].ToString();
                    if (strBARCODE_1 == strBARCODE_2) {
                        if (product == PRODUCT.PR_SOPUTKA) {
                            if (!DbfWrapper.CheckIfFileExist("SoputkaKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }
                        } else if (product == PRODUCT.PR_BV) {
                            if (!DbfWrapper.CheckIfFileExist("BvKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\BV\")) { this.Cursor = Cursors.Default; return; }
                        } 
                        if (!_PrintRow(dlg.GetPrintParms(), product, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, null, false, true))
                            return;
                        _CheckStikers(PRODUCT.PR_BV);
                        break;
                    }
                }
            }
        }

        //bSoputks
        private void _Print(PrintParms pp, PRODUCT product, int nStcker = -1,
            string strBarcodePrint = "", string strCodeSpm = "", bool bWithoutKM = false) {
            string strDestPathKM = "";
            string strSrcPathKM = "";

            string strDestPathPaternSP = "";
            string strSrcPathPaternSP = "";

            string strRootFolder = "";
            string strPrefixFileWithData = "";

            DataGridView dgv = null;
            if (product == PRODUCT.PR_SOPUTKA) {
                if (!DbfWrapper.CheckIfFileExist("SoputkaKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }
                if (!DbfWrapper.CheckIfFileExist("sticker_SP_01.zpl", ref strDestPathPaternSP, ref strSrcPathPaternSP, @"\1c\Matrix\patern\")) return;
                strRootFolder = @"\Soputka\generated\";
                strPrefixFileWithData = "SoputkaRestShopsToStikers_";
                dgv = dataGridViewSoputka;
            } else if (product == PRODUCT.PR_BV) {
                if (!DbfWrapper.CheckIfFileExist("BvKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\BV\")) { this.Cursor = Cursors.Default; return; }
                if (!DbfWrapper.CheckIfFileExist("sticker_BV_01.zpl", ref strDestPathPaternSP, ref strSrcPathPaternSP, @"\1c\Matrix\patern\")) return;
                strRootFolder = @"\BV\generated\";
                strPrefixFileWithData = "BvRestShopsToStikers_";
                dgv = dataGridViewBV;
            }            

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
                //string strFileName = strPrefixFileWithData + strStatus + ".dbf";
                string strFileName = "SoputkaRestShopsToStikers.dbf";

                if (product == PRODUCT.PR_BV)
                    strFileName = "BvRestShopsToStikers.dbf";

                if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, strRootFolder)) { this.Cursor = Cursors.Default; return; }

                DataTable dtTableSoputka = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "BARCODE", strBarcode);
                if (0 == dtTableSoputka.Rows.Count) {
                    strFileName = "SoputkaRestShopsToStikers_SGP.dbf";
                    if (product == PRODUCT.PR_BV)
                        strFileName = "BvRestShopsToStikers_SGP.dbf";

                    if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, strRootFolder)) { this.Cursor = Cursors.Default; return; }
                    dtTableSoputka = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "BARCODE", strBarcode);

                    if (0 == dtTableSoputka.Rows.Count) {
                        if (product == PRODUCT.PR_SOPUTKA) {
                            string strDestPathBar = "";
                            string strSrcPathBar = "";
                            if (!DbfWrapper.CheckIfFileExist("Bar.dbf", ref strDestPathBar, ref strSrcPathBar, @"\Soputka\Bc\SERVER\BAR\")) { this.Cursor = Cursors.Default; return; }
                            DataTable dtTableBar = Dbf.LoadDbfWithAddColumns(strDestPathBar, out _, ref nCodeOut, "BARCODE", strBarcode);
                            if (0 == dtTableBar.Rows.Count) {
                                MessageBox.Show(this, "Нет штрихкода: " + strBarcode + " В файле: " + strDestPathBar, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            Dictionary<string, int> dicGTIN_COUNT = [];
                            dicGTIN_COUNT.Add(strBarcode, 1);
                            dtTableSoputka = _tableBarToSoputkaRest(dtTableBar, dicGTIN_COUNT);
                        } else {
                            MessageBox.Show(this, "Нет штрихкода: " + strBarcode + " В файле: " + strDestPathSoputka, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                List<KM> listKM = [];
                listKM.Add(new KM(nStcker, strKM));

                int GTIN = 23;
                dtTableSoputka.Rows[0][GTIN] = strGTIN;

                _PrintRow(pp, product, dtTableSoputka.Rows[0], strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, false, false);
            } else if (strBarcodePrint != "") {

                int GTIN0 = 3;
                DataTable dtTableSoputkaKM0 = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "BARCODE", strBarcodePrint);
                if (0 == dtTableSoputkaKM0.Rows.Count)
                    return;
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
                    string strFileName = strPrefixFileWithData + strCodeSpm + ".dbf";
                    if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, strRootFolder)) { this.Cursor = Cursors.Default; return; }

                    DataTable dtTableSoputka = Dbf.LoadDbfWithAddColumns(strDestPathSoputka, out _, ref nCodeOut, "BARCODE", strBarcodePrint);
                    if (1 != dtTableSoputka.Rows.Count) {
                        MessageBox.Show(this, "не равно 1 количество Штрихкода: " + strBarcodePrint + " В файле: " + strDestPathSoputka, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    List<KM> listKM = [];
                    listKM.Add(new KM((int)row[N], strKM));
                    int GTIN = 23;
                    string strGTIN = row[GTIN0].ToString();
                    dtTableSoputka.Rows[0][GTIN] = strGTIN;
                    _PrintRow(pp, product, dtTableSoputka.Rows[0], strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, false, false);                    
                }
            } else {
                bool bLastRow = false;
                int nPos = 0;
                foreach (DataGridViewRow gvRow in dgv.SelectedRows) {
                    nPos++;
                    if (nPos == dgv.SelectedRows.Count)
                        bLastRow = true;
                    DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                    if (bWithoutKM) {
                        List<KM> listKM = [];
                        listKM.Add(new KM(0, ""));
                        if (!_PrintRow(pp, product, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, listKM, bLastRow))
                            return;
                    } else {
                        if (!_PrintRow(pp, product, row, strDestPathKM, strDestPathPaternSP, ref nPrintedAll, null, bLastRow))
                            return;
                    }
                }
            }
        }
        private void btRegPO_Click(object sender, EventArgs e) {
            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Регистрация ПО [1]");
            this.Cursor = Cursors.WaitCursor;
            try {
                _RegisterPO(txtAdress.Text, txtNamePO.Text);
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Cursor = Cursors.Default;
        }

        private void label24_Click(object sender, EventArgs e) {
            txtRegKey.Visible = !txtRegKey.Visible;
        }

        private void btGetToken_Click_1(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;

            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Получение токена [4]");

            try {
                txtToken.Text = "";
                string strCompanyID = "05";
                FairMark fm = new(strCompanyID, true);               
                AuthorizationRespons rfm = fm._AuthorizationRequest();
                if (null == rfm) {
                    txtToken.Text = "Ошибка: Запрос на авторизацию вернул NULL";
                    return;
                }
                if ("" != rfm.strError) {
                    txtToken.Text = "Ошибка: Запрос на авторизацию вернул:" + rfm.strError;
                    return;
                }
                txtToken.Text = "Авторизация прошла успешно:\r\n" + rfm.strContent + "\r\n";

                CryptoPro cryptoPro = new(fm.GetSert());
                string strSinged = cryptoPro.SingAttachedSignature(rfm.data);
                Trace.WriteLine(" ");
                Trace.WriteLine(" ");

                string strToken = Task.Run(() => FairMark._GetToken(rfm.uuid, strSinged)).Result;
                if ("" == strToken) {
                    Trace.WriteLine("Ошибка: токен не получен");
                    return;
                }
                Trace.WriteLine("OK: Получен токен: " + strToken);

                if ("" != strToken) {
                    txtToken.Text += "Токун получен успешно:\r\n" + strToken;

                    if (!File.Exists(_strFileSettingPath)) {
                        Trace.WriteLine("Ошибка, Отсутствует файл:" + _strFileSettingPath);
                        return;
                    }
                    DateTime dtOpenedFile = Win32.GetLastWriteTime(_strFileSettingPath);
                    if (!Dbf.SetValue(_strFileSettingPath, "DOC", strCompanyID, "LASTTOKEN", strToken, dtOpenedFile)) {
                        Trace.WriteLine("Ошибка записи в файл: " + _strFileSettingPath);
                    }

                }
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Cursor = Cursors.Default;
        }

        private void btPing_Click(object sender, EventArgs e) {
            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Проверка доступности СУЗ [5]");

            this.Cursor = Cursors.WaitCursor;
            try {
                //IniSettings ist = new();
                //string strToken = ist.GetValue(INITPARAMS.LASTTOKEN).ToLower();

                int nCodeOut = -1;
                string strCompanyID = "05";
                DataTable dtTableSetting = Dbf.LoadDbfWithAddColumns(_strFileSettingPath, out _, ref nCodeOut, "DOC", strCompanyID);
                if (1 != dtTableSetting.Rows.Count) {
                    Trace.WriteLine("Ошибка: Несколько записей в файле с номером:" + strCompanyID);
                    return;
                }

                txtPing.Text = "";
                FairMark fm = new(strCompanyID, true);               
                int LASTTOKEN = 4;
                string strToken = "";
                if (System.DBNull.Value != dtTableSetting.Rows[0][LASTTOKEN])
                    strToken = dtTableSetting.Rows[0][LASTTOKEN].ToString();


                PingRespons pr = fm.Ping(strToken);
                if ("" != pr.strError)
                    txtPing.Text += "Пинг токена: " + strToken + " не прошел:\r\n" + pr.strError;
                else
                    txtPing.Text += "Успешно прошел Пинг токена " + strToken + " :\r\n" + pr.strContent;
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Cursor = Cursors.Default;
        }
        private void btCreateOrder_Click_1(object sender, EventArgs e) {
            if (!_IsInt(txtCount1.Text)) {
                MessageBox.Show(this, "Ошибка: а поле Кол-во должно быть число", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Convert.ToInt32(txtCount1.Text) < 1) {
                MessageBox.Show(this, "Ошибка: а поле Кол-во должно быть больше 0", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Convert.ToInt32(txtCount1.Text) > 150000) {
                MessageBox.Show(this, "Ошибка: а поле Кол-во должно быть не более 150 000", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Создание заказа на эмиссию КМ [6]");

            try {
                string strCompanyID = "05";
                bool bSandbox = true;
                string strCreateID = _CreateOrder(txtGTIN1.Text, Convert.ToInt32(txtCount1.Text), "BARCODE", false, strCompanyID, bSandbox);
                if ("" == strCreateID) {
                    txtOrderCreate.Text = "Не создан заказ на Коды Маркировки на: " + Convert.ToInt32(txtCount1.Text) + " штук, для GTIN:" + txtGTIN1.Text;
                    return;
                }
                txtOrderCreate.Text = "Создан заказ на Коды Маркировки: \r\n" + strCreateID + "\r\nна: " + Convert.ToInt32(txtCount1.Text) + " штук, для GTIN:" + txtGTIN1.Text;
                txtOrderID.Text = strCreateID;
                txtOrderID2.Text = strCreateID;
                txtOrderID3.Text = strCreateID;
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btGetInfoFromOrder_Click_1(object sender, EventArgs e) {
            txtOrderInfo.Text = "";
            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Получить инфо из заказа [7]");

            if ("" == txtOrderID.Text) {
                MessageBox.Show("Ошибка, в поле: " + label20.Text + " , должен быть номер заказа");
                return;
            }
            try {
                string strCompanyID = "05";
                FairMark fm = new(strCompanyID, true);
                List<GetOrderInfoRespons> loi = fm.GetOrderInfo(txtOrderID.Text);
                if (null == loi) {
                    txtToken.Text = "Ошибка: ResponsFM1 is NULL";
                    return;
                }
                txtOrderInfo.Text = "Получено элементов:" + loi.Count + "\r\n";
                foreach (GetOrderInfoRespons item in loi)
                    txtOrderInfo.Text += "bufferStatus:" + item.bufferStatus + " gtin: " + item.gtin + " totalCodes: " + item.totalCodes + " leftInBuffer: " + item.leftInBuffer + "\r\n";
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btGetKM_Click_1(object sender, EventArgs e) {
            txtOrderInfo2.Text = "";

            if ("" == txtOrderID2.Text) {
                MessageBox.Show("Ошибка, в поле: " + label21.Text + " , должен быть номер заказа");
                return;
            }
            if ("" == txtCount.Text) {
                MessageBox.Show("Ошибка, в поле: " + label22.Text + " , должно быть кол-во заказов");
                return;
            }
            if ("" == txtGTIN.Text) {
                MessageBox.Show("Ошибка, в поле: " + label23.Text + " , должен быть GTIN");
                return;
            }
            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Получение эмитированных КМ [8]");

            try {
                string strCompanyID = "05";
                FairMark fm = new(strCompanyID, true);
                GeyOrderKM resp = fm.GetKM(txtOrderID2.Text, txtGTIN.Text, Convert.ToInt32(txtCount.Text));
                if (null == resp) {
                    txtOrderInfo2.Text = "Ошибка: ResponsFM1 is NULL";
                    return;
                }
                txtOrderInfo2.Text = "Полученые КМ:\n";
                foreach (string strVal in resp.codes)
                    txtOrderInfo2.Text += strVal + "\n";

                if (0 == resp.codes.Count && resp.err.globalErrors.Count > 0)
                    txtOrderInfo2.Text += "Ощибка:" + resp.err.globalErrors[0].error;
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btCloseOrder_Click(object sender, EventArgs e) {
            txtCloseOrder.Text = "";
            if ("" == txtOrderID3.Text) {
                MessageBox.Show("Ошибка, в поле: " + label34.Text + " , должен быть номер заказа");
                return;
            }
            this.Cursor = Cursors.WaitCursor;

            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Закрыть заказ на КМ [11]");

            try {
                string strCompanyID = "05";
                FairMark fm = new(strCompanyID, true);
                CloseOrderRespons resp = fm.CloseOrder(txtOrderID3.Text);
                if (null == resp) {
                    this.Cursor = Cursors.Default;
                    txtCloseOrder.Text = "Ошибка: CloseOrderRespons is NULL";
                    return;
                }
                if (resp.err.globalErrors.Count > 0)
                    txtCloseOrder.Text = "Ощибка:" + resp.err.globalErrors[0].error;
                else
                    txtCloseOrder.Text = "Успешно закрыт заказ: " + txtOrderID3.Text;
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Cursor = Cursors.Default;
        }

        private string _GetShopPrefix(PRODUCT product) {
            if (product == PRODUCT.PR_SOPUTKA) {
                if (rbALL.Checked) return rbALL.Text[..3];
                if (rb053.Checked) return rb053.Text[..3];
                if (rb057.Checked) return rb057.Text[..3];
                if (rb300.Checked) return rb300.Text[..3];
                if (rb260.Checked) return rb260.Text[..3];
                if (rb310.Checked) return rb310.Text[..3];
                if (rb370.Checked) return rb370.Text[..3];
                if (rb375.Checked) return rb375.Text[..3];
                if (rb397.Checked) return rb397.Text[..3];
                if (rb401.Checked) return rb401.Text[..3];
                if (rb414.Checked) return rb414.Text[..3];
                if (rb416.Checked) return rb416.Text[..3];
                if (rb421.Checked) return rb421.Text[..3];
                if (rb432.Checked) return rb432.Text[..3];
                if (rb433.Checked) return rb433.Text[..3];
                if (rbSGP.Checked) return rbSGP.Text[..3];
            } else if (product == PRODUCT.PR_BV) {
                if (rbAll_BV.Checked) return rbAll_BV.Text[..3];
                if (rb053_BV.Checked) return rb053_BV.Text[..3];
                if (rb057_BV.Checked) return rb057_BV.Text[..3];
                if (rb300_BV.Checked) return rb300_BV.Text[..3];
                if (rb260_BV.Checked) return rb260_BV.Text[..3];
                if (rb310_BV.Checked) return rb310_BV.Text[..3];
                if (rb370_BV.Checked) return rb370_BV.Text[..3];
                if (rb375_BV.Checked) return rb375_BV.Text[..3];
                if (rb397_BV.Checked) return rb397_BV.Text[..3];
                if (rb401_BV.Checked) return rb401_BV.Text[..3];
                if (rb414_BV.Checked) return rb414_BV.Text[..3];
                if (rb416_BV.Checked) return rb416_BV.Text[..3];
                if (rb421_BV.Checked) return rb421_BV.Text[..3];
                if (rb432_BV.Checked) return rb432_BV.Text[..3];
                if (rb433_BV.Checked) return rb433_BV.Text[..3];
                if (rbSGP_BV.Checked) return rbSGP_BV.Text[..3];
            } 
            return "000";
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
                if (!DbfWrapper.CheckIfFileExist("BvShowOnlyArt.txt", ref strDestPathBARCODES, ref strSrcPathBARCODES, @"\BV\patern\")) { this.Cursor = Cursors.Default; return; }
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
                if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathSoputka, ref strSrcPathSoputka, @"\BV\generated\")) { this.Cursor = Cursors.Default; return; }
                if (!DbfWrapper.CheckIfFileExist("BVOrdersKM.dbf", ref strDestPathOrdersKM, ref strSrcPathOrdersKM, @"\BV\")) { this.Cursor = Cursors.Default; return; }
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
                if (!DbfWrapper.CheckIfFileExist("BVKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\BV\")) { this.Cursor = Cursors.Default; return; }
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
            MarkingBV.GetUseColumnsBV(ref hUseColumns);//переделать

            _dtBvGtinOrdersMkLoaded = _GetLastWriteTime(strSrcPathOrdersKM);

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
            hNumTableSPO = [];
            for (int i = 0; i < dgv.Rows.Count; i++) {
                hNumTableSPO.Add(1 + i, (int)dgv.Rows[i].Cells[0].Value);
                dgv.Rows[i].Cells[0].Value = 1 + i;

                if (System.DBNull.Value != dgv.Rows[i].Cells[ART].Value) {
                    string strNcard = ((string)dgv.Rows[i].Cells[ART].Value).Trim();
                    string strArt = dgv.Rows[i].Cells[ART].Value.ToString().Trim();

                    //if (_hUsedMishaArticuled.Contains(strArt))
                    //    MessageBox.Show("Ошибка, отображаем артикул с которым работает Миша: " + strArt);

                    if (dicArticulSostav.ContainsKey(strNcard))
                        dgv.Rows[i].Cells[CCLOTH].Value = dicArticulSostav[strNcard];
                    else
                        dgv.Rows[i].Cells[CCLOTH].Value = "?????";
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
                string strNomencl = MarkingBV.GetNomenclBV(row);
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
                    if ("" == strShopPrefixLocal)
                        strShopPrefixLocal = _GetShopPrefix(PRODUCT.PR_BV);

                    string strKey = lGTIN.ToString() + "_" + dgv.Rows[i].Cells[BARCODE2].Value.ToString() + "_" + strShopPrefixLocal;
                    if (!dicGtinBarcodeShop_Count.ContainsKey(strKey))
                        continue;
                    dgv.Rows[i].Cells[KOL_TOSHOP].Value = dicGtinBarcodeShop_Count[strKey].ToString();
                }
            }
            MarkingBV.InidataGridViewSoputkaBv(ref dgv);
        }
        private void _CheckStikers(PRODUCT product0) {
            DlgCheckStikers dlg = new(_GetShopPrefix(product0), null, product0, null, MARKINGTYPES.REST_BV);
            dlg.ShowDialog();
        }
        private void menuCheckStikers_Click(object sender, EventArgs e) {
            _CheckStikers(PRODUCT.PR_SOPUTKA);
        }
        public void UpdateCountBV(DataGridView dgv) {
            _UpdateCountBV(dgv);
        }
        private void _UpdateCountBV(DataGridView dgv = null) {
            int nSumKol = 0;
            int nSumToShop = 0;
            int KOL = 10;
            int KOL_TOSHOP = 26;
            if (null == dgv)
                dgv = dataGridViewBV;
            foreach (DataGridViewRow row in dgv.Rows) {
                if (System.DBNull.Value != row.Cells[KOL].Value)
                    nSumKol += (int)row.Cells[KOL].Value;
                if (System.DBNull.Value != row.Cells[KOL_TOSHOP].Value)
                    nSumToShop += (int)row.Cells[KOL_TOSHOP].Value;
            }
            tsInfo.Text = "Номенклатуры (Cтрок): " + dgv.RowCount.ToString() + ", Количество: " + nSumKol.ToString() + ", Передано в магазин: " + nSumToShop.ToString();
        }
        public void UpdateCount(DataGridView dgv = null) {
            _UpdateCount(dgv);
        }
        private void _UpdateCount(DataGridView dgv = null) {
            int nSumKol = 0;
            int nSumToShop = 0;
            int KOL = 11;
            int KOL_TOSHOP = 30;
            if (null == dgv)
                dgv = dataGridViewSoputka;
            if (dgv.Columns.Count > 30 && "Код изделия" == dgv.Columns[3].Name) {
                KOL = 10;
                KOL_TOSHOP = 26;
            }

            foreach (DataGridViewRow row in dgv.Rows) {
                if (System.DBNull.Value != row.Cells[KOL].Value)
                    nSumKol += (int)row.Cells[KOL].Value;
                if (System.DBNull.Value != row.Cells[KOL_TOSHOP].Value)
                    nSumToShop += (int)row.Cells[KOL_TOSHOP].Value;
            }
            tsInfo.Text = "Номенклатуры (Cтрок): " + dgv.RowCount.ToString() + ", Количество: " + nSumKol.ToString() + ", Передано в магазин: " + nSumToShop.ToString();
        }
        private void _UpdateCountVetex() {
            int nSumKol = 0;
            int nSumToShop = 0;
            int KOL = 10;
            int KOL_TOSHOP = 26;
            if (dataGridViewVetex.ColumnCount >= 31) {
                KOL = 11;
                KOL_TOSHOP = 30;
            }
            foreach (DataGridViewRow row in dataGridViewVetex.Rows) {
                if (System.DBNull.Value != row.Cells[KOL].Value)
                    nSumKol += (int)row.Cells[KOL].Value;
                if (System.DBNull.Value != row.Cells[KOL_TOSHOP].Value)
                    nSumToShop += (int)row.Cells[KOL_TOSHOP].Value;
            }
            tsInfo.Text = "Номенклатуры (Cтрок): " + dataGridViewVetex.RowCount.ToString() + ", Количество: " + nSumKol.ToString() + ", Передано в магазин: " + nSumToShop.ToString();
        }

        private void dataGridFilterSoputka_AfterFiltersChanged(object sender, EventArgs e) {
            _UpdateCount();
        }
        private void CheckSpWithBar() {
            if (0 == dataGridViewSoputka.Rows.Count) {
                MessageBox.Show(this, "Ошибка, в DataGridView нет данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int nCodeOut = -1;
            string strDestPathBar = "";
            string strSrcPathBar = "";
            //199\ps\Soputka\Bc\SERVER\BAR\Bar.dbf
            if (!DbfWrapper.CheckIfFileExist("Bar.dbf", ref strDestPathBar, ref strSrcPathBar, @"\Soputka\Bc\SERVER\BAR\")) { this.Cursor = Cursors.Default; return; }

            DataTable dtTableBar = Dbf.LoadDbfWithAddColumns(strDestPathBar, out _, ref nCodeOut, "COTHER", "30");
            if (0 == dtTableBar.Rows.Count) {
                Trace.WriteLine("Ошибка, нет данных в файле: " + strSrcPathBar);
                MessageBox.Show(this, "Ошибка, нет данных в файле: " + strSrcPathBar, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nError = 0;

            int BARCODE = 3;
            int ART = 8;
            int RAZ = 9;
            int nCount = 0;
            foreach (DataGridViewRow row in dataGridViewSoputka.Rows) {
                nCount++;
                string strBarcode = (string)row.Cells[BARCODE].Value;
                string strArt = (string)row.Cells[ART].Value;
                string strRaz = "";
                if (System.DBNull.Value != row.Cells[RAZ].Value)
                    strRaz = (string)row.Cells[RAZ].Value;

                DataRow[] rows = dtTableBar.Select("BARCODE = '" + strBarcode + "'");
                if (0 == rows.Length) {
                    Trace.WriteLine("Ошибка, BARCODE:" + strBarcode + "равно 0");
                    nError++;
                    continue;
                }
                if (rows.Length != 1) {
                    Trace.WriteLine("Ошибка, BARCODE:" + strBarcode + "не равно 1");
                    nError++;
                    continue;
                }
                if (rows[0][7].ToString() != strArt) {
                    Trace.WriteLine("Ошибка, BARCODE:" + strBarcode + "не равны ART");
                    nError++;
                    continue;
                }
                if (rows[0][8].ToString() != strRaz) {
                    Trace.WriteLine("Ошибка, BARCODE:" + strBarcode + "не равно RAZ");
                    nError++;
                    continue;
                }
            }
            if (nError > 0)
                MessageBox.Show(this, "Ошибка, смотри логи", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show(this, "Проверка прошла успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void button5_Click(object sender, EventArgs e) {
            CheckSpWithBar();
        }
        private void _RunPrintStickerServer(string strSettingID) {
            Win32.ShellExecute(IntPtr.Zero, "open", "PrintStickerServer.exe", strSettingID, "C:\\Po_BOLSHEVICHKA\\PrintStickerServer\\", ShowCommands.SW_SHOWNORMAL);
        }
        private void btTmpPrintSticker_Click(object sender, EventArgs e) {
            _RunPrintStickerServer(_strSettingID_BV);
        }
        private void rbALL_CheckedChanged(object sender, EventArgs e) {
            if (rbALL.Checked) _ShowPageSoputka();
        }
        private void rb053_CheckedChanged(object sender, EventArgs e) {
            if (rb053.Checked) _ShowPageSoputka();
        }
        private void rb057_CheckedChanged(object sender, EventArgs e) {
            if (rb057.Checked) _ShowPageSoputka();
        }
        private void rb300_CheckedChanged(object sender, EventArgs e) {
            if (rb300.Checked) _ShowPageSoputka();
        }
        private void rb260_CheckedChanged(object sender, EventArgs e) {
            if (rb260.Checked) _ShowPageSoputka();
        }
        private void rb310_CheckedChanged(object sender, EventArgs e) {
            if (rb310.Checked) _ShowPageSoputka();
        }
        private void rb370_CheckedChanged(object sender, EventArgs e) {
            if (rb370.Checked) _ShowPageSoputka();
        }
        private void rb375_CheckedChanged(object sender, EventArgs e) {
            if (rb375.Checked) _ShowPageSoputka();
        }
        private void rb397_CheckedChanged(object sender, EventArgs e) {
            if (rb397.Checked) _ShowPageSoputka();
        }
        private void rb401_CheckedChanged(object sender, EventArgs e) {
            if (rb401.Checked) _ShowPageSoputka();
        }
        private void rb414_CheckedChanged(object sender, EventArgs e) {
            if (rb414.Checked) _ShowPageSoputka();
        }
        private void rb416_CheckedChanged(object sender, EventArgs e) {
            if (rb416.Checked) _ShowPageSoputka();
        }
        private void rb421_CheckedChanged(object sender, EventArgs e) {
            if (rb421.Checked) _ShowPageSoputka();
        }
        private void rb432_CheckedChanged(object sender, EventArgs e) {
            if (rb432.Checked) _ShowPageSoputka();
        }
        private void rb433_CheckedChanged(object sender, EventArgs e) {
            if (rb433.Checked) _ShowPageSoputka();
        }
        private void rbSGP_CheckedChanged(object sender, EventArgs e) {
            if (rbSGP.Checked) _ShowPageSoputka();
        }

        private bool _CopyDataToFileBV(Dictionary<string, ProductInCUT> hDicBarcodes_ProductInCUT, Dictionary<string, int> dicGTIN_COUNT, string strFileNameDestination) {
            if (null == hDicBarcodes_ProductInCUT || dicGTIN_COUNT.Count == 0)
                return false;

            //GTIN   
            Dictionary<string, long> dicNomenclGtin = [];
            if (!DbfWrapper.GetGtinsBv(ref dicNomenclGtin))
                return false;
            //_GTIN

            int nCodeOut = -1;

            string strDestPathRez = "";
            string strSrcPathRez = "";
            if (!DbfWrapper.CheckIfFileExist("BvRestShopsToStikers.dbf", ref strDestPathRez, ref strSrcPathRez, @"\BV\patern\")) { this.Cursor = Cursors.Default; return false; }

            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);
            if (0 != dtTableRez.Rows.Count) {
                MessageBox.Show("Ошибка, в шаблоне есть данные, в файле: " + strSrcPathRez, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //"SoputkaRestShopsToStikers_SGP.dbf"
            string strDestPathGen = "";
            string strSrcPathGen = "";
            if (!DbfWrapper.CheckIfFileExist(strFileNameDestination, ref strDestPathGen, ref strSrcPathGen, @"\BV\generated\")) { this.Cursor = Cursors.Default; return false; }

            dtTableRez.Rows.Clear();

            foreach (var item in dicGTIN_COUNT) {
                string strBarcode = item.Key;
                int nCount = item.Value;

                ProductInCUT prCut = hDicBarcodes_ProductInCUT[strBarcode];

                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
                rowAdd[RESTCOLIDBV.BARCODE] = prCut.GetBARCODE();
                rowAdd[RESTCOLIDBV.IZD] = prCut.GetIZD();
                rowAdd[RESTCOLIDBV.PRS] = prCut.GetPRS();
                rowAdd[RESTCOLIDBV.MOD] = prCut.GetMOD();
                rowAdd[RESTCOLIDBV.ART] = prCut.GetART().Trim();
                rowAdd[RESTCOLIDBV.RAZ] = prCut.GetRAZ();
                rowAdd[RESTCOLIDBV.KOL] = nCount;

                rowAdd[RESTCOLIDBV.CJ] = 0;
                rowAdd[RESTCOLIDBV.ART2] = prCut.GetART2().Trim();

                rowAdd[RESTCOLIDBV.PATTERN] = prCut.GetPATTERN();
                rowAdd[RESTCOLIDBV.CCODE] = prCut.GetCCODE();
                rowAdd[RESTCOLIDBV.CCLOTH] = prCut.GetCCLOTH();
                rowAdd[RESTCOLIDBV.COTHER] = prCut.GetCOTHER();
                rowAdd[RESTCOLIDBV.CSEASON] = prCut.GetCSEASON();

                rowAdd[RESTCOLIDBV.CJ2] = 0;
                rowAdd[RESTCOLIDBV.CR] = 0;

                rowAdd[RESTCOLIDBV.SRT] = prCut.GetSRT();

                string strNomencl = MarkingBV.GetNomenclBV(rowAdd);
                if (dicNomenclGtin.ContainsKey(strNomencl))
                    rowAdd[RESTCOLIDBV.GTIN] = dicNomenclGtin[strNomencl];

                rowAdd[RESTCOLIDBV.KOL_KM] = 0;
                rowAdd[RESTCOLIDBV.KOL_PRN] = 0;
                rowAdd[RESTCOLIDBV.IZDNAME] = RestItemBV.GetIZDName(prCut.GetIZD());

                dtTableRez.Rows.Add(rowAdd);
            }
            if (!Dbf.AddTable(strDestPathGen, dtTableRez)) {
                MessageBox.Show("Ошибка добавления в файл: " + strDestPathRez, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestPathGen, strSrcPathGen, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        DataTable _tableBarToSoputkaRest(DataTable dtTableBar, Dictionary<string, int> dicGTIN_COUNT) {

            //GTIN   
            Dictionary<string, long> dicNomenclGtin = [];
            if (!DbfWrapper.GetGtinsSoputka(ref dicNomenclGtin))
                return null;
            //_GTIN

            int nCodeOut = -1;

            string strDestPathRez = "";
            string strSrcPathRez = "";
            if (!DbfWrapper.CheckIfFileExist("SoputkaRestShopsToStikers.dbf", ref strDestPathRez, ref strSrcPathRez, @"\Soputka\patern\")) { this.Cursor = Cursors.Default; return null; }

            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);
            if (0 != dtTableRez.Rows.Count) {
                MessageBox.Show("Ошибка, в шаблоне есть данные, в файле: " + strSrcPathRez, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
           
            dtTableRez.Rows.Clear();

            int BARCODE_BAR = 2;
            int IZD_BAR = 3;
            int MOD_BAR = 6;
            int ART_BAR = 7;
            int RAZ_BAR = 8;
            int CJ_BAR = 10;
            int ART2_BAR = 11;
            int PATTERN_BAR = 12;//
            int CCODE_BAR = 13;
            int CCLOTH_BAR = 14;
            int COTHER_BAR = 15;
            int CSEASON_BAR = 16;
            int CR_BAR = 17;

            foreach (var item in dicGTIN_COUNT) {
                string strBarcode = item.Key;
                int nCount = item.Value;
                DataRow[] rows = dtTableBar.Select("BARCODE = '" + strBarcode + "'");
                if (rows.Length != 1) {
                    MessageBox.Show("Ошибка, BARCODE:" + strBarcode + "не равно 1", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
                rowAdd[RESTCOLID.BARCODE] = rows[0][BARCODE_BAR];
                rowAdd[RESTCOLID.IZD] = rows[0][IZD_BAR];
                rowAdd[RESTCOLID.MOD] = rows[0][MOD_BAR];
                rowAdd[RESTCOLID.ART] = rows[0][ART_BAR];
                rowAdd[RESTCOLID.RAZ] = rows[0][RAZ_BAR];
                rowAdd[RESTCOLID.KOL] = nCount;
                rowAdd[RESTCOLID.CJ] = rows[0][CJ_BAR];
                rowAdd[RESTCOLID.ART2] = rows[0][ART2_BAR];
                rowAdd[RESTCOLID.PATTERN] = rows[0][PATTERN_BAR];
                rowAdd[RESTCOLID.CCODE] = rows[0][CCODE_BAR];
                rowAdd[RESTCOLID.CCLOTH] = rows[0][CCLOTH_BAR];
                rowAdd[RESTCOLID.COTHER] = rows[0][COTHER_BAR];
                rowAdd[RESTCOLID.CSEASON] = rows[0][CSEASON_BAR];
                rowAdd[RESTCOLID.CJ2] = 0;
                rowAdd[RESTCOLID.CR] = rows[0][CR_BAR];


                //rowAdd[RESTCOLID.CCODE] = "210";
                //rowAdd[RESTCOLID.CCLOTH] = "состав"

                if ("2925191255450" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "210";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|96-4";
                }
                if ("2925133390270" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "210";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|96-4";
                }
                if ("2925191120277" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "270";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|96-4";
                }
                //
                if ("2925191088683" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "270";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|97-3";
                }
                if ("2925137337257" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "270";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|97-3";
                }
                if ("2925154334543" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "110";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|97-3";
                }
                if ("2925154948849" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "210";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|97-3";
                }

                if ("2925131984105" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "210";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|99-1";
                }
                if ("2925131744549" == rows[0][BARCODE_BAR].ToString()) {
                    rowAdd[RESTCOLID.CCODE] = "210";
                    rowAdd[RESTCOLID.CCLOTH] = "ХБ_ЭЛ_|99-1";
                }

                string strNomencl = GetNomencl(rowAdd);
                if (dicNomenclGtin.ContainsKey(strNomencl))
                    rowAdd[RESTCOLID.GTIN] = dicNomenclGtin[strNomencl];

                rowAdd[RESTCOLID.KOL_KM] = 0;
                rowAdd[RESTCOLID.KOL_PRN] = 0;

                dtTableRez.Rows.Add(rowAdd);
            }

            return dtTableRez;
        }
        private bool _CopyDataToFile(DataTable dtTableBar, Dictionary<string, int> dicGTIN_COUNT, string strFileNameDestination) {
            if (null == dtTableBar || dicGTIN_COUNT.Count == 0)
                return false;

            DataTable dtTableRez = _tableBarToSoputkaRest(dtTableBar, dicGTIN_COUNT);

            //"SoputkaRestShopsToStikers_SGP.dbf"
            string strDestPathGen = "";
            string strSrcPathGen = "";
            if (!DbfWrapper.CheckIfFileExist(strFileNameDestination, ref strDestPathGen, ref strSrcPathGen, @"\Soputka\generated\")) { this.Cursor = Cursors.Default; return false; }


            if (!Dbf.AddTable(strDestPathGen, dtTableRez)) {
                MessageBox.Show("Ошибка добавления в файл: " + strDestPathGen, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestPathGen, strSrcPathGen, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void _AddBarcodesToRestShopsBV(string strPathWithBARCODES, string strFileNameDestination) {
            string[] lineBARCODES = File.ReadAllLines(strPathWithBARCODES);
            if (0 == lineBARCODES.Length)
                return;
            for (int i = 0; i < lineBARCODES.Length; i++) {
                string strVal = lineBARCODES[i];
                if (strVal.Length > 2 && strVal[0] == '[' && strVal[strVal.Length - 1] == ']')
                    lineBARCODES[i] = strVal.Substring(1, strVal.Length - 2);
            }

            foreach (string strVal in lineBARCODES) {
                if (13 != strVal.Length) {
                    MessageBox.Show("Ошибка, в файле " + strPathWithBARCODES + ", есть строка с длиной не равной 13", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!_IsLong(strVal)) {
                    MessageBox.Show("Ошибка, в файле " + strPathWithBARCODES + ", есть строки в которой есть не только цифры", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            _dicProduct_ProductInCUT.Clear();
            Dictionary<string, ProductInCUT> hDicBarcodes_ProductInCUT = [];
            if (!_GetDataWithBarcode(ref _dicProduct_ProductInCUT, ref hDicBarcodes_ProductInCUT, false))
                return;
            
            Dictionary<string, int> dicBARCODES_COUNT = [];
            foreach (string strVal in lineBARCODES) {
                if (!hDicBarcodes_ProductInCUT.ContainsKey(strVal)) {
                    MessageBox.Show("Ошибка, в файле " + strPathWithBARCODES + ", BARCODE: " + strVal + " который не найден в хранилище", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!dicBARCODES_COUNT.ContainsKey(strVal))
                    dicBARCODES_COUNT.Add(strVal, 1);
                else
                    dicBARCODES_COUNT[strVal]++;
            }

            if (!_CopyDataToFileBV(hDicBarcodes_ProductInCUT, dicBARCODES_COUNT, strFileNameDestination))
                return;
        }
        private void _AddBarcodesToRestShops(string strPathWithBARCODES, string strFileNameDestination) {
            string[] lineGTIN = File.ReadAllLines(strPathWithBARCODES);
            if (0 == lineGTIN.Length)
                return;
            for (int i = 0; i < lineGTIN.Length; i++) {
                string strVal = lineGTIN[i];
                if (strVal.Length > 2 && strVal[0] == '[' && strVal[strVal.Length - 1] == ']')
                    lineGTIN[i] = strVal.Substring(1, strVal.Length - 2);
            }

            foreach (string strVal in lineGTIN) {
                if (13 != strVal.Length) {
                    MessageBox.Show("Ошибка, в файле " + strPathWithBARCODES + ", есть строка с длиной не равной 13", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!_IsLong(strVal)) {
                    MessageBox.Show("Ошибка, в файле " + strPathWithBARCODES + ", есть строки в которой есть не только цифры", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            int nCodeOut = -1;
            string strDestPathBar = "";
            string strSrcPathBar = "";
            if (!DbfWrapper.CheckIfFileExist("Bar.dbf", ref strDestPathBar, ref strSrcPathBar, @"\Soputka\Bc\SERVER\BAR\")) { this.Cursor = Cursors.Default; return; }

            DataTable dtTableBar = Dbf.LoadDbfWithAddColumns(strDestPathBar, out _, ref nCodeOut, "COTHER", "30");
            if (0 == dtTableBar.Rows.Count) {
                MessageBox.Show("Ошибка, в файле " + strDestPathBar + ", нет данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            HashSet<string> hsBARCODE = [];
            int BARCODE = 2;
            for (int i = 0; i < dtTableBar.Rows.Count; i++) {
                if (System.DBNull.Value == dtTableBar.Rows[i][BARCODE])
                    continue;
                hsBARCODE.Add(dtTableBar.Rows[i][BARCODE].ToString());
            }
            Dictionary<string, int> dicGTIN_COUNT = [];
            foreach (string strVal in lineGTIN) {
                if (!hsBARCODE.Contains(strVal)) {
                    MessageBox.Show("Ошибка, в файле " + strDestPathBar + ", BARCODE: " + strVal + " которого нет в файле: " + strDestPathBar, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!dicGTIN_COUNT.ContainsKey(strVal))
                    dicGTIN_COUNT.Add(strVal, 1);
                else
                    dicGTIN_COUNT[strVal]++;
            }
            if (!_CopyDataToFile(dtTableBar, dicGTIN_COUNT, strFileNameDestination))
                return;
        }

        private void LoadDataToSGP() {
            openFileDlg.CheckFileExists = true;
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "txt файлы c штрихкодами(*.txt)|*.txt";

            if (openFileDlg.ShowDialog() != DialogResult.OK)
                return;
            //Cursor curBefore = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string strPathWithBARCODES = openFileDlg.FileName;
            _AddBarcodesToRestShops(strPathWithBARCODES, "SoputkaRestShopsToStikers_SGP.dbf");

            MessageBox.Show(this, "Импорт прошел Успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Cursor = Cursors.Default;
        }
        private void btFromSGP_Click(object sender, EventArgs e) {
            LoadDataToSGP();
        }

        private void btOborot_Click(object sender, EventArgs e) {
            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Ввод в оборот");

            int nCodeOut = -1;
            int KM = 5;
            string strDestPathKM = "";
            string strSrcPathKM = "";
            if (!DbfWrapper.CheckIfFileExist("SoputkaKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\Soputka\")) { this.Cursor = Cursors.Default; return; }

            string strKN = "";
            DataTable dtTableSoputkaKM = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableSoputkaKM.Rows.Count; i++) {
                if (System.DBNull.Value == dtTableSoputkaKM.Rows[i][KM])
                    continue;
                strKN = (string)dtTableSoputkaKM.Rows[i][KM];

                int nPos1 = strKN.IndexOf("91");
                //int nPos2 = strKN.IndexOf("91");
                string strValue1 = strKN[..nPos1];
                string strValue2 = strKN.Substring(nPos1, 6);
                string strValue3 = strKN.Substring(nPos1 + 6);
                //strValue3 = strValue3;
                strKN = strValue1 + ((char)29).ToString() + strValue2 + ((char)29).ToString() + strValue3;
                //strKN = strKN;

            }
            string strFile = _GetCurDir() + "\\requests\\Utilisation.txt";
            if (!File.Exists(strFile)) {
                MessageBox.Show("нет файла: " + strFile);
                return;
            }
            string strRequest = File.ReadAllText(strFile).Replace("\r\n", " ");
            strRequest = strRequest.Replace("11111", strKN);
            try {
                FairMark fm = new("05", true);//Песочница
                CloseOrderRespons resp = fm.InOborot(strRequest);
                if (null == resp) {
                    txtOrderInfo2.Text = "Ошибка: InOborot is NULL";
                    return;
                }               
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btGetInfoCatalog_Click(object sender, EventArgs e) {
            Trace.WriteLine(" ");
            Trace.WriteLine(" ");
            Trace.WriteLine("Информация из национального каталога");
            try {
                FairMark fm = new("05", true); ////Песочница
                PingRespons resp = fm.GetInfoNationalCatalog("");
                if (null == resp) {
                    txtOrderInfo2.Text = "Ошибка: InOborot is NULL";
                    return;
                }
            } catch (Exception ex) {
                this.Cursor = Cursors.Default;
                MessageBox.Show(this, "Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuPrintRepeat_Click(object sender, EventArgs e) {
            DlgInputBox dlg = new();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            DlgSelRazmer dlg2 = new(true);
            if (DialogResult.OK != dlg2.ShowDialog(this))
                return;

            _Print(dlg2.GetPrintParms(), PRODUCT.PR_SOPUTKA, dlg.GetNumber());
        }

        private void rbAll_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbAll_BV.Checked) _ShowPageBV();
        }
        private void rb053_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb053_BV.Checked) _ShowPageBV();
        }
        private void rb057_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb057_BV.Checked) _ShowPageBV();
        }
        private void rb300_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb300_BV.Checked) _ShowPageBV();
        }
        private void rb260_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb260_BV.Checked) _ShowPageBV();
        }
        private void rb310_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb310_BV.Checked) _ShowPageBV();
        }
        private void rb370_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb370_BV.Checked) _ShowPageBV();
        }
        private void rb375_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb375_BV.Checked) _ShowPageBV();
        }
        private void rb397_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb397_BV.Checked) _ShowPageBV();
        }
        private void rb401_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb401_BV.Checked) _ShowPageBV();
        }
        private void rb414_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb414_BV.Checked) _ShowPageBV();
        }
        private void rb416_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb416_BV.Checked) _ShowPageBV();
        }
        private void rb421_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb421_BV.Checked) _ShowPageBV();
        }
        private void rb432_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb432_BV.Checked) _ShowPageBV();
        }
        private void rb433_BV_CheckedChanged(object sender, EventArgs e) {
            if (rb433_BV.Checked) _ShowPageBV();
        }
        private void rbSGP_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbSGP_BV.Checked) _ShowPageBV();
        }
        private bool _GetDataWithBarcode(ref Dictionary<string, ProductInCUT> dic, ref Dictionary<string, ProductInCUT> hDicBarcodes_ProductInCUT, bool bUseExceptedBarcodes = true) {

            DlgIzdelie dlg = new();
            dlg.ShowDialog();
            string strIzdID = dlg.GetSelIzdID();
            //strIzdID = "11";
            string strFilePrefix = "Prx";
            string strFindBarcodeFolder = @"\BV.2\PS\DATAPRX\";
            if (!_GetDataWithBarcode0(strIzdID, ref dic, ref hDicBarcodes_ProductInCUT, strFilePrefix, strFindBarcodeFolder, bUseExceptedBarcodes))
                return false;

            strFilePrefix = "Cut";
            strFindBarcodeFolder = @"\BV.2\PS\DATACUT\";
            if (!_GetDataWithBarcode0(strIzdID, ref dic, ref hDicBarcodes_ProductInCUT, strFilePrefix, strFindBarcodeFolder, bUseExceptedBarcodes))
                return false;

            //return true;
            strFilePrefix = "RM";
            strFindBarcodeFolder = @"\BV.2\PS\DATARM\";
            if (!_GetDataWithBarcode0(strIzdID, ref dic, ref hDicBarcodes_ProductInCUT, strFilePrefix, strFindBarcodeFolder, bUseExceptedBarcodes))
                return false;

            //type 3
            strFilePrefix = "Prx";
            strFindBarcodeFolder = @"\bv\shops\s310\dataprx\";
            if (!_GetDataWithBarcode0(strIzdID, ref dic, ref hDicBarcodes_ProductInCUT, strFilePrefix, strFindBarcodeFolder, bUseExceptedBarcodes))
                return false;

            //type 2
            strFilePrefix = "wf221c.dbf";
            strFindBarcodeFolder = @"\bv\shops\s310\datarm\";
            if (!_GetDataWithBarcode0(strIzdID, ref dic, ref hDicBarcodes_ProductInCUT, strFilePrefix, strFindBarcodeFolder, bUseExceptedBarcodes))
                return false;

            strFilePrefix = "rm1c0324.dbf";
            strFindBarcodeFolder = @"\bv\shops\s310\datarm\";
            if (!_GetDataWithBarcode0(strIzdID, ref dic, ref hDicBarcodes_ProductInCUT, strFilePrefix, strFindBarcodeFolder, bUseExceptedBarcodes))
                return false;

            strFilePrefix = "291021.dbf";
            strFindBarcodeFolder = @"\bv\shops\s310\datarm\";
            if (!_GetDataWithBarcode0(strIzdID, ref dic, ref hDicBarcodes_ProductInCUT, strFilePrefix, strFindBarcodeFolder, bUseExceptedBarcodes))
                return false;            
            return true;
        }
        private bool _GetDataWithBarcode0(string strIzdID, ref Dictionary<string, ProductInCUT> dic, ref Dictionary<string, ProductInCUT> hDicBarcodes_ProductInCUT,
            string strFilePrefix, string strFindBarcodeFolder, bool bUseExceptedBarcodes) {
            HashSet<string> hSetBarcodesErr = [];
            string strDestPathBARCODES = "";
            string strSrcPathBARCODES = "";
            if (!DbfWrapper.CheckIfFileExist("BvNotValidBARCODE.txt", ref strDestPathBARCODES, ref strSrcPathBARCODES, @"\BV\patern\")) { this.Cursor = Cursors.Default; return false; }

            if (bUseExceptedBarcodes) {
                string[] aStr = File.ReadAllLines(strDestPathBARCODES);
                foreach (string str in aStr)
                    hSetBarcodesErr.Add(str);
            }
            int nERR = 0;

            DateTime dtStart = DateTime.Now;
            //dtStart = dtStart.AddMonths(-156);

            while (dtStart.Year > 2013) {
                DateTime dtWork = dtStart;
                dtStart = dtStart.AddMonths(-1);
                //string strFileName = "Cut" + dtWork.Month.ToString("00") + (dtWork.Year % 100).ToString("00") + ".dbf";//Cut0124.dbf
                string strFileName = strFilePrefix + dtWork.Month.ToString("00") + (dtWork.Year % 100).ToString("00") + ".dbf";//Cut0124.dbf
                if (-1 != strFilePrefix.LastIndexOf(".dbf"))
                    strFileName = strFilePrefix;

                int nCodeOut = -1;
                string strDestPathRez = "";
                string strSrcPathRez = "";
                //if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathRez, ref strSrcPathRez, @"\BV.2\PS\DATACUT\")) { this.Cursor = Cursors.Default; continue; }
                if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathRez, ref strSrcPathRez, strFindBarcodeFolder)) { this.Cursor = Cursors.Default; continue; }


                DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);                
                int nTypeSourceFile;
                if (ProductInCUT.CheckColumnsInTable(ref dtTableRez))
                    nTypeSourceFile = 1;
                else if (ProductInCUT.CheckColumnsInTableType2(ref dtTableRez))
                    nTypeSourceFile = 2;
                else if (ProductInCUT.CheckColumnsInTableType3(ref dtTableRez))
                    nTypeSourceFile = 3;
                else {
                    MessageBox.Show("Неправильный формат файла: " + strDestPathRez, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }


                for (int i = 0; i < dtTableRez.Rows.Count; i++) {
                    ProductInCUT pr = new(dtTableRez.Rows[i], nTypeSourceFile);
                    //pr.SetFileName(strFileName);

                    if (13 != pr.GetBARCODE().Length)
                        continue;

                    if (hSetBarcodesErr.Contains(pr.GetBARCODE()))
                        continue;

                    string strProd = pr.GetProduct();
                    if (!dic.ContainsKey(strProd))
                        dic.Add(strProd, pr);

                    //работаем тольо с костюмами СГП
                    string strIZD = strIzdID;//"02";
                    if (pr.GetIZD() == strIZD && !hDicBarcodes_ProductInCUT.ContainsKey(pr.GetBARCODE()))
                        hDicBarcodes_ProductInCUT.Add(pr.GetBARCODE(), pr);
                    else {
                        if (pr.GetIZD() == strIZD)
                            nERR++;
                    }
                }
            }
            return true;
        }
        private void _CollectRestBV() {
            this.Cursor = Cursors.WaitCursor;
            _dicProduct_ProductInCUT.Clear();
            Dictionary<string, ProductInCUT> hDicBarcodes_ProductInCUT = [];
            if (!_GetDataWithBarcode(ref _dicProduct_ProductInCUT, ref hDicBarcodes_ProductInCUT))
                return;

            if (!GetRemainsBV("BVRestShopsToStikers.dbf", "ALL")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_053.dbf", "053")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_057.dbf", "057")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_300.dbf", "300")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_260.dbf", "260")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_310.dbf", "310")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_370.dbf", "370")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_375.dbf", "375")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_397.dbf", "397")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_401.dbf", "401")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_414.dbf", "414")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_416.dbf", "416")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_421.dbf", "421")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_432.dbf", "432")) return;
            if (!GetRemainsBV("BVRestShopsToStikers_433.dbf", "433")) return;
            
            MessageBox.Show(this, "Импорт прошел Успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Cursor = Cursors.Default;
        }
        private void btBvFill_Click(object sender, EventArgs e) {
            _CollectRestBV();
        }
        private void tabPage2_Click(object sender, EventArgs e) {
        }
        private void dataGridFilterBV_AfterFiltersChanged(object sender, EventArgs e) {
            _UpdateCountBV();
        }

        private void menuPrintRepeatSelected_Click(object sender, EventArgs e) {
            int BARCODE = 3;
            foreach (DataGridViewRow gvRow in dataGridViewSoputka.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                string strBarcode = row[BARCODE].ToString();
                string strCodeSpm = _GetShopPrefix(PRODUCT.PR_SOPUTKA);
                _Print(null, PRODUCT.PR_SOPUTKA, -1, strBarcode, strCodeSpm);
            }
        }
        private void menuExportToExcelBV_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            _ExportToExcelBV();
            this.Cursor = Cursors.Default;
        }
        private void menuImportGTIN_BV_Click(object sender, EventArgs e) {
            string strDestSoputkaGtin = "";
            string strSrcSoputkaGtin = "";
            if (!DbfWrapper.CheckIfFileExist("BvGtin.dbf", ref strDestSoputkaGtin, ref strSrcSoputkaGtin, @"\BV\")) { this.Cursor = Cursors.Default; return; }

            if (!_ImportGtinFromExcel(strDestSoputkaGtin, strSrcSoputkaGtin, "АО 'Большевичка'"))
                return;
            _ShowPageBV();
        }
        private void menuUpdateBV_Click(object sender, EventArgs e) {
            _ShowPageBV();
        }
        private void menuGetQRCodeBV_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            _GetQRCodeBV();
            this.Cursor = Cursors.Default;
        }
        private void menuPrintBV_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;

            DlgSelRazmer dlg = new();
            dlg.ShowDialog();

            _Print(dlg.GetPrintParms(), PRODUCT.PR_BV);
            this.Cursor = Cursors.Default;
        }
        private void menuPrintRepeatBV_Click(object sender, EventArgs e) {
            DlgInputBox dlg = new();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            DlgSelRazmer dlg2 = new();
            dlg2.ShowDialog();

            _Print(dlg2.GetPrintParms(), PRODUCT.PR_BV, dlg.GetNumber());
        }

        private void menuPrintRepeatSelectedBV_Click(object sender, EventArgs e) {
            DlgSelRazmer dlg = new();
            dlg.ShowDialog();

            int BARCODE = 27;
            foreach (DataGridViewRow gvRow in dataGridViewBV.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                string strBarcode = row[BARCODE].ToString();
                string strCodeSpm = _GetShopPrefix(PRODUCT.PR_BV);
                _Print(dlg.GetPrintParms(), PRODUCT.PR_BV, -1, strBarcode, strCodeSpm);
            }
        }

        private void menuCheckStikersBV_Click(object sender, EventArgs e) {
            _CheckStikers(PRODUCT.PR_BV);
        }

        private void btTmpPrintSticker2_Click(object sender, EventArgs e) {
            _RunPrintStickerServer(_strSettingID_BV);
        }

        private void _LoadByBarcodeBV() {
            openFileDlg.CheckFileExists = true;
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "txt файлы c штрихкодами(*.txt)|*.txt";

            if (openFileDlg.ShowDialog() != DialogResult.OK)
                return;
            //Cursor curBefore = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string strPathWithBARCODES = openFileDlg.FileName;
            _AddBarcodesToRestShopsBV(strPathWithBARCODES, "BvRestShopsToStikers_SGP.dbf");

            _ShowPageBV();

            MessageBox.Show(this, "Импорт прошел Успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Cursor = Cursors.Default;

        }
        private void btFromSGP_BV_Click(object sender, EventArgs e) {
            _LoadByBarcodeBV();
        }

        private void menuPrintBV_OnFile_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            _PrintOnFile(PRODUCT.PR_BV);
            this.Cursor = Cursors.Default;
        }
        private void _PrintOneZpl() {            
            CustomPrintPreviewDialog dlg = new(1);
            dlg.PrintPreviewControl.Zoom = 1;
            dlg.Size = new Size(800, 1200);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            //dlg.SetAutoScrollMargin
            ((Form)dlg).WindowState = FormWindowState.Maximized;
            dlg.Document = printDocument1;

            //dlg.Document.PrinterSettings.Copies = 5;

            int PreferredZoomValue = 150;
            dlg.PrintPreviewControl.Zoom = PreferredZoomValue / 100f;
            dlg.Icon = this.Icon;
            //_showPrintMsg = true;

            //dialog1.Document.PrinterSettings = printDialog.PrinterSettings;
            if (DialogResult.OK != dlg.ShowDialog())
                return;
        }
        private void menuPrintBV_ONE_ZPL_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            _PrintOneZpl();
            this.Cursor = Cursors.Default;
        }

        int _nPrintPagePrint = 1;
        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e) {
            float left = 5;
            float top = 5;

            int nCount = 2;

            for (int i = _nPrintPagePrint; i <= nCount; i++) {
                e.Graphics.DrawRectangle(Pens.Black, new Rectangle((int)left, (int)top, 155, 375));
                e.Graphics.DrawString(_nPrintPagePrint.ToString() + "9876543219876543219876543221_00001111", lbPrint2.Font, new SolidBrush(lbPrint2.ForeColor), left + 5, top + 5);

                if (_nPrintPagePrint != nCount) {
                    e.HasMorePages = true;
                    _nPrintPagePrint++;
                    return;
                }
            }

            e.HasMorePages = false;
            _nPrintPagePrint = 1;

            return;
        }
        private void rbV_SHOP_CheckedChanged(object sender, EventArgs e) {
            //dataGridViewVetex.ContextMenuStrip = contextMenuVetex;
            if (rbV_SHOP.Checked) _FillTree();
        }
        private void rbV_IMPORT_CheckedChanged(object sender, EventArgs e) {
            //dataGridViewVetex.ContextMenuStrip = contextMenuVetex;
            if (rbV_IMPORT.Checked) _FillTree();
        }
        private void rbV_Fabrication_CheckedChanged(object sender, EventArgs e) {
            //dataGridViewVetex.ContextMenuStrip = contextMenuVetex;
            if (rbV_Fabrication.Checked) _FillTree();
        }
        private void toolStripMenuItem14_Click(object sender, EventArgs e) {
            _FillTree();
        }
        private bool _GetPostfix(string strNewBarcode, ref string strPostfix) {
            if (12 != strNewBarcode.Length)
                return false;
            int n1 = Convert.ToInt32(strNewBarcode[1]) - 48;
            int n3 = Convert.ToInt32(strNewBarcode[3]) - 48;
            int n5 = Convert.ToInt32(strNewBarcode[5]) - 48;
            int n7 = Convert.ToInt32(strNewBarcode[7]) - 48;
            int n9 = Convert.ToInt32(strNewBarcode[9]) - 48;
            int n11 = Convert.ToInt32(strNewBarcode[11]) - 48;

            int n0 = Convert.ToInt32(strNewBarcode[0]) - 48;
            int n2 = Convert.ToInt32(strNewBarcode[2]) - 48;
            int n4 = Convert.ToInt32(strNewBarcode[4]) - 48;
            int n6 = Convert.ToInt32(strNewBarcode[6]) - 48;
            int n8 = Convert.ToInt32(strNewBarcode[8]) - 48;
            int n10 = Convert.ToInt32(strNewBarcode[10]) - 48;

            int Rez1 = 3 * (n1 + n3 + n5 + n7 + n9 + n11);
            int Rez2 = n0 + n2 + n4 + n6 + n8 + n10;
            int Rez3 = Rez1 + Rez2;
            int Rest = Rez3 % 10;
            if (0 == Rest)
                strPostfix = "0";
            else
                strPostfix = (10 - Rest).ToString();

            return true;
        }

        private void _MakeAndFillBarcodes() {
            int BARCODE = 27;
            string strPrefixEan13 = "20";
            string strFileName = _GetFileNameVetexStikers();
            if (rbV_Fabrication.Checked) {
                strPrefixEan13 = "21";
            } else if (rbV_IMPORT.Checked) {
                strPrefixEan13 = "22";
            } else if (rbV_IMPORT_SOPUTKA.Checked) {
                strPrefixEan13 = "23";
                BARCODE = 3;
            }

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, @"\ooo\vetex\generated\")) { this.Cursor = Cursors.Default; return; }
            int N = 0;
            try {
                foreach (DataGridViewRow gvRow in dataGridViewVetex.Rows) {
                    DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                    if (System.DBNull.Value != row[BARCODE])
                        continue;
                    int nNUM = Convert.ToInt32(row[N]);
                    string strNumInDB = _hNumTableVetex[nNUM].ToString();

                    DateTime dtOpenedFile = Win32.GetLastWriteTime(strDestPath);
                    string strNewBarcode = strPrefixEan13 + _hNumTableVetex[nNUM].ToString("0000000000");
                    string strPostfix = "";
                    if (!_GetPostfix(strNewBarcode, ref strPostfix))
                        continue;
                    strNewBarcode += strPostfix;
                    if (!Dbf.SetValue(strDestPath, "№", strNumInDB, "BARCODE", strNewBarcode, dtOpenedFile)) {
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
                File.Copy(strDestPath, strSrcPath, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _FillTree();
        }
        private void menuMakeEAN13Vetex_Click(object sender, EventArgs e) {
            //_MakeAndFillBarcodes();
        }
        private void toolStripMenuItem6_Click(object sender, EventArgs e) {
            string strDestSoputkaGtin = "";
            string strSrcSoputkaGtin = "";
            if (!DbfWrapper.CheckIfFileExist("VetexGtin.dbf", ref strDestSoputkaGtin, ref strSrcSoputkaGtin, @"\ooo\vetex\")) { this.Cursor = Cursors.Default; return; }

            string strCompanyName = "";
            if (rbV_IMPORT_SOPUTKA.Checked || rbV_Fabrication.Checked)
                strCompanyName = "ООО 'ВЕТЕКС'";

            if (!_ImportGtinFromExcel(strDestSoputkaGtin, strSrcSoputkaGtin, strCompanyName))//"ООО 'ВЕТЕКС'"
                return;
            _FillTree();
        }
        private void menuExportToExcelVetex_Click(object sender, EventArgs e) {
        }
        private void menuGetQRCodeVetex_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            //_GetQRCodeVetex();
            this.Cursor = Cursors.Default;
        }

        private void menuPrintVetex_Click(object sender, EventArgs e) {

        }

        private void menuPrintRepeatSelectedVetex_Click(object sender, EventArgs e) {
            DlgSelRazmer dlg2 = new();
            dlg2.ShowDialog();

            int BARCODE = 27;
            foreach (DataGridViewRow gvRow in dataGridViewVetex.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                string strBarcode = row[BARCODE].ToString();                
                if (rbV_IMPORT_SOPUTKA.Checked) {
                    BARCODE = 3;
                    strBarcode = row[BARCODE].ToString();
                }
            }
        }

        private void menuMarkDel_Click(object sender, EventArgs e) {
            if (0 == dataGridViewBV.SelectedRows.Count)
                return;
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return;

            Cursor curBefore = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_strOpenedFileBV, ref strDestPath, ref strSrcPath, @"\BV\generated\")) { this.Cursor = Cursors.Default; return; }

            GenLogic.CopyToArhive(strSrcPath);

            try {
                List<int> listRows = [];
                foreach (DataGridViewRow row in dataGridViewBV.SelectedRows)
                    listRows.Add(_hNumTableBV[(int)row.Cells[0].Value]);
                listRows.Sort();
                byte nDEL = 42;
                if (!Dbf.SetDelRowBytes(strSrcPath, listRows, nDEL, ref _dtOpenedFileBV)) {
                    this.Cursor = curBefore;
                    return;
                }
            } catch (Exception ex) {
                this.Cursor = curBefore;
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            } finally {
                this.Cursor = curBefore;
            }
            _ShowPageBV();
        }

        private void contextMenuBV_Opening(object sender, CancelEventArgs e) {
            menuLoadByBarcodelBV.Visible = false;
            toolStripMenuItem7.Visible = false;
            menuMarkDel.Visible = false;

            if (rbSGP_BV.Checked) {
                menuLoadByBarcodelBV.Visible = true;
                toolStripMenuItem7.Visible = true;
                menuMarkDel.Visible = true;
            }

            if (0 == dataGridViewBV.SelectedRows.Count)
                menuMarkDel.Enabled = false;
            else
                menuMarkDel.Enabled = true;
        }

        private void menuMarkDelSoputka_Click(object sender, EventArgs e) {
            if (0 == dataGridViewSoputka.SelectedRows.Count)
                return;
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return;

            Cursor curBefore = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_strOpenedFileSoputka, ref strDestPath, ref strSrcPath, @"\Soputka\generated\")) { this.Cursor = Cursors.Default; return; }

            GenLogic.CopyToArhive(strSrcPath);

            try {
                List<int> listRows = [];
                foreach (DataGridViewRow row in dataGridViewSoputka.SelectedRows)
                    listRows.Add(_hNumTableSoputka[(int)row.Cells[0].Value]);
                listRows.Sort();
                byte nDEL = 42;
                if (!Dbf.SetDelRowBytes(strSrcPath, listRows, nDEL, ref _dtOpenedFileSoputka)) {
                    this.Cursor = curBefore;
                    return;
                }
            } catch (Exception ex) {
                this.Cursor = curBefore;
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            } finally {
                this.Cursor = curBefore;
            }
            _ShowPageSoputka();
        }

        private void menuLoadByBarcodelBV_Click(object sender, EventArgs e) {
            _LoadByBarcodeBV();
        }

        private void menuCheckStikersVetex_Click(object sender, EventArgs e) {
        }

        private void rbV_IMPORT_SOPUTKA_CheckedChanged(object sender, EventArgs e) {
            //dataGridViewVetex.ContextMenuStrip = contextMenuVetex;
            if (rbV_IMPORT_SOPUTKA.Checked) _FillTree();
        }

        private void _ExportToRaskroyny() {
            if (dataGridViewVetex.SelectedRows.Count == 0)
                return;            

            DataTable dtblResult = new();
            dtblResult.Columns.Add("NLIST");
            dtblResult.Columns.Add("CJ");
            dtblResult.Columns.Add("PATTERN");
            dtblResult.Columns.Add("CCODE");
            dtblResult.Columns.Add("CCLOTH");
            dtblResult.Columns.Add("COTHER");
            dtblResult.Columns.Add("CSEASON");
            dtblResult.Columns.Add("IZD");
            dtblResult.Columns.Add("MOD");

            dtblResult.Columns.Add("PRS");
            dtblResult.Columns.Add("RAZ");
            dtblResult.Columns.Add("ART");
            dtblResult.Columns.Add("ART2");
            dtblResult.Columns.Add("LABEL");
            dtblResult.Columns.Add("BRAKET");
            dtblResult.Columns.Add("FLOOR");
            dtblResult.Columns.Add("NOTE");
            dtblResult.Columns.Add("VC");

            MessageBox.Show(this, "Нужно ввести  : закупочную цену, номер торговой марки, код состава ткани, доп признаки, сезон", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            foreach (DataGridViewRow row in dataGridViewVetex.SelectedRows) {
                //int nCol = Convert.ToInt32(row.Cells[KOL].Value);
                int nCol = 1;
                for (int i = 1; i <= nCol; i++) {

                    string strArt = row.Cells[ART].Value.ToString();
                    int nPrice = 0;
                    if ("207201" == strArt) nPrice = 10665;
                    if ("207200" == strArt) nPrice = 11685;

                    string strRazmer = row.Cells[RAZ].Value.ToString();
                    if (strRazmer.Length != 11)
                        strRazmer = _CorrectRazmAddNull(strRazmer);

                    var r = dtblResult.NewRow();
                    r["NLIST"] = "";
                    r["CJ"] = nPrice;//закупочная цена
                    r["PATTERN"] = row.Cells[PATTERN].Value;
                    r["CCODE"] = row.Cells[CCODE].Value;
                    r["CCLOTH"] = "1";//1 - ПШ, код состава ткани \\192.168.0.199\ps\BV.2\MAIL\REF\CLOTH.DBF
                    r["COTHER"] = "00";//доп признаки \\192.168.0.199\ps\BV.2\MAIL\REF\OTHER.DBF
                    r["CSEASON"] = "М";//сезон М - Меж., Л - Лето, З - Зима \\192.168.0.199\ps\BV.2\MAIL\REF\SEASON.DBF
                    r["IZD"] = row.Cells[IZD].Value;
                    r["MOD"] = row.Cells[MOD].Value;
                    r["PRS"] = " " + row.Cells[PRS].Value;
                    r["RAZ"] = strRazmer;
                    r["ART"] = row.Cells[ART].Value.ToString().PadLeft(15);
                    r["ART2"] = "";
                    r["LABEL"] = "01";
                    r["BRAKET"] = "38";//код торговой марки TRADEMAR.DBF  \\192.168.0.199\ps\BV.2\MAIL\REF\TRADEMAR.DBF
                    r["FLOOR"] = "";
                    r["NOTE"] = "";
                    r["VC"] = 0;
                    dtblResult.Rows.Add(r);
                }
            }
            string strDestPath = "";
            string strSrcPath = "";
            DbfWrapper.CheckIfFileExist("F300000.dbf", ref strDestPath, ref strSrcPath, @"\ooo\vetex\patern\");
            if (dtblResult.Rows.Count > 0) {
                if (!Dbf.SaveAddTableToDbfAndChangeColumnSize(strDestPath, dtblResult, 1, 10))
                    return;
                MessageBox.Show(this, "Экспорт прошел успешно, путь к экспортируемому файлу : " + strDestPath, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //File.Copy(strDestPath, strSrcPath, true);
            }
        }
        private void menuExportToRaskroyny_Click(object sender, EventArgs e) {
            if (!rbV_IMPORT.Checked)
                return;
            _ExportToRaskroyny();
        }

        private void rbWT_REM_SOPUTKA_CheckedChanged(object sender, EventArgs e) {
            if (!rbWT_REM_SOPUTKA.Checked) return;

            btSettingWT.Enabled = true;
             _FillTree();
        }

        private void rbWT_EMPTY_CheckedChanged(object sender, EventArgs e) {
            if (rbWT_EMPTY.Checked) _FillTree();;
        }
        private void menuMarkDelVetex_Click(object sender, EventArgs e) {
            if (0 == dataGridViewVetex.SelectedRows.Count)
                return;
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return;

            Cursor curBefore = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string strFileStikers = _GetFileNameVetexStikers();
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileStikers, ref strDestPath, ref strSrcPath, @"\ooo\vetex\generated\")) { this.Cursor = Cursors.Default; return; }

            GenLogic.CopyToArhive(strSrcPath);

            try {
                List<int> listRows = [];
                foreach (DataGridViewRow row in dataGridViewVetex.SelectedRows)
                    listRows.Add(_hNumTableVetex[(int)row.Cells[0].Value]);
                listRows.Sort();
                byte nDEL = 42;
                if (!Dbf.SetDelRowBytes(strSrcPath, listRows, nDEL, ref _dtOpenedFileVetex)) {
                    this.Cursor = curBefore;
                    return;
                }
            } catch (Exception ex) {
                this.Cursor = curBefore;
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            } finally {
                this.Cursor = curBefore;
            }
            _FillTree();
        }
        private void rbMTM_REM_SOPUTKA_CheckedChanged(object sender, EventArgs e) {
            if (!rbMTM_REM_SOPUTKA.Checked) return;

            btSettingsMTM.Enabled = true;

            _FillTree();
        }
        private void rbMTM_EMPTY_CheckedChanged(object sender, EventArgs e) {
            _FillTree();
        }
        private void menuImportBARCODE_Vetex_Click(object sender, EventArgs e) {
            if (!rbV_IMPORT.Checked)
                return;

            openFileDlg.CheckFileExists = true;
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "dbf файлы(*.dbf)|*.dbf";

            if (openFileDlg.ShowDialog() != DialogResult.OK)
                return;
            string strFileName = openFileDlg.FileName;

            string strDestPath = "";
            string strSrcPath = "";

            if (!DbfWrapper.CheckIfFileExist("ImportToStikers_003.dbf", ref strDestPath, ref strSrcPath, @"\ooo\vetex\generated\")) { this.Cursor = Cursors.Default; return; }

            DateTime dtSrcFileBegin = Win32.GetLastWriteTime(strSrcPath);
            try {
                int nIsIndexMDXNestand = -1;
                int nCodeOut = -1;
                DataTable dtTableDestination = Dbf.LoadDbfWithAddColumns(strDestPath, out nIsIndexMDXNestand, ref nCodeOut, "DEL", "0");

                //
                DataTable dtTableWithBarcode = Dbf.LoadDbfWithAddColumns(strFileName, out nIsIndexMDXNestand, ref nCodeOut);
                if (dtTableDestination.Rows.Count != dtTableWithBarcode.Rows.Count) {
                    MessageBox.Show(this, "В таблицах разное число строк", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show(this, "В таблице Большевички задвоение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show(this, "Есть несоответствие номенклатур", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("файл : " + strSrcPath + ", был изменен сторонней программой, обновится сейчас таблица, повторите ваши действия", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _FillTree();
                    return;
                }
                File.Copy(strDestPath, strSrcPath, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(this, "Импорт прошел успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _FillTree();
        }
        private void menuPrintRepeatOnNumberVetex_Click(object sender, EventArgs e) {           
        }
        private void menuPrintRepeatOnNumberVetexLabel_Click(object sender, EventArgs e) {
            DlgInputBox dlg = new();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            DlgSelRazmer dlg2 = new();
            dlg2.ShowDialog();
        }

        private void dataGridFilterVetex_AfterFiltersChanged(object sender, EventArgs e) {
            _UpdateCountVetex();
        }

        private void dataGridFilterWesperTrading_AfterFiltersChanged(object sender, EventArgs e) {
            _UpdateCount(dataGridViewWesperTrading);
        }

        private void menuLoadByRazkomplektBV_Click(object sender, EventArgs e) {
            DlgInputNum dlg = new();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            string strNumber = dlg.GetNumber().ToString();

            //string strPath = @"\\192.168.0.199\ps\1c\MATRIX\PROIZV\0" + strNumber + @"0\0" + strNumber + @"0.tmp";
            string strPath = @"x:\\1c\MATRIX\PROIZV\0" + strNumber + @"0\0" + strNumber + @"0.tmp";
            if (!File.Exists(strPath)) {
                MessageBox.Show(this, "Отсутствует путь: " + strPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Dictionary<string, ProductInCUT> hDicBarcodes_ProductInCUT = [];
            Dictionary<string, int> dicBARCODES_COUNT = [];
            int BARCODE = 4;
            int KOL = 12;
            int nCodeOut = -1;
            DataTable dtTableSrc = Dbf.LoadDbfWithAddColumns(strPath, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableSrc.Rows.Count; i++) {
                if (System.DBNull.Value == dtTableSrc.Rows[i][BARCODE] || System.DBNull.Value == dtTableSrc.Rows[i][KOL])
                    continue;
                string strBarcode = dtTableSrc.Rows[i][BARCODE].ToString();
                dicBARCODES_COUNT.Add(strBarcode, (int)dtTableSrc.Rows[i][KOL]);

                ProductInCUT pr = new(dtTableSrc.Rows[i], 3);
                hDicBarcodes_ProductInCUT.Add(strBarcode, pr);
            }
            if (!_CopyDataToFileBV(hDicBarcodes_ProductInCUT, dicBARCODES_COUNT, "BvRestShopsToStikers_SGP.dbf"))
                return;
            _ShowPageBV();
        }

        private void menuPrintWithoutKmBV_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            DlgSelRazmer dlg = new();
            dlg.ShowDialog();
            _Print(dlg.GetPrintParms(), PRODUCT.PR_BV, -1, "", "", true);
            this.Cursor = Cursors.Default;
        }
        private void menuAddToTableVetex_Click(object sender, EventArgs e) {
            if (!rbV_SHOP.Checked && !rbV_Fabrication.Checked)
                return;
            DlgAddToTableBV dlg = new();
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            dlg.GetSelItems(out List<RestItemBV> listItems);
            if (0 == listItems.Count) return;

            string strDestPathRez = "";
            string strSrcPathRez = "";

            string strFileName = "";
            if (rbV_SHOP.Checked)
                strFileName = "RestShopsToStikers_001.dbf";
            if (rbV_Fabrication.Checked)
                strFileName = "FabricationToStikers_002.dbf";

            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathRez, ref strSrcPathRez, @"\ooo\vetex\generated\")) { this.Cursor = Cursors.Default; return; }

            int nCodeOut = -1;
            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);

            dtTableRez.Rows.Clear();

            foreach (RestItemBV item in listItems) {
                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
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

                rowAdd[RESTCOLIDBV.IZDNAME] = RestItemBV.GetIZDName(item.GetIZD());

                dtTableRez.Rows.Add(rowAdd);
            }
            if (!Dbf.AddTable(strDestPathRez, dtTableRez)) {
                MessageBox.Show("Ошибка добавления в файл: " + strDestPathRez, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try {
                File.Copy(strDestPathRez, strSrcPathRez, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _FillTree();
            _MakeAndFillBarcodes();
        }
        private void dataGridFilterMtm_AfterFiltersChanged(object sender, EventArgs e) {
            _UpdateCount(dataGridViewMTM);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e) {
            _ShowPageSoputka();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            _ShowPageBV();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            _FillTree();
        }
        private void btSettingSoputka_Click(object sender, EventArgs e) {
            DlgSetting dlg = new(this, _GetCurMarkingObject());
            dlg.ShowDialog(this);

        }
        private void btSettingVetex_Click(object sender, EventArgs e) {
            DlgSetting dlg = new(this, _GetCurMarkingObject());
            dlg.ShowDialog(this);
        }

        private void btSettingsMTM_Click(object sender, EventArgs e) {
            DlgSetting dlg = new(this, _GetCurMarkingObject());
            dlg.ShowDialog(this);
        }

        private void btSettingWT_Click(object sender, EventArgs e) {
            DlgSetting dlg = new(this, _GetCurMarkingObject());
            dlg.ShowDialog(this);
        }

        private void btSettingBV_Click(object sender, EventArgs e) {
            if(!rb_FABR_BV.Checked && rb_FABR_SP.Checked) {
                btSettingBV.Enabled = false;
                return;
            }
            DlgSetting dlg = new(this, _GetCurMarkingObject());
            dlg.ShowDialog(this);
        }

        private void tv_MouseUp(TreeView tv, MouseEventArgs e) {
            if ((e.Button == MouseButtons.Left))
                return;
            tv.SelectedNode = tv.GetNodeAt(e.X, e.Y);
            if (null == tv.SelectedNode)
                return;
            if (MouseButtons.Right == e.Button) {
                if (tv.SelectedNode.Text.Length < 19) {
                    contextMenuTreeViewOrders.Show(this, new Point(e.X + 6, e.Y + tv.Top + 7 + tabControl1.ItemSize.Height));
                    return;
                } else {
                    contextMenuTreeViewItems.Show(this, new Point(e.X + 6, e.Y + tv.Top + 7 + tabControl1.ItemSize.Height));
                    return;
                }
            }
        }
        private void tvMTM_MouseUp(object sender, MouseEventArgs e) {
            tv_MouseUp(tvMTM, e);
        }

        private void _InsertToTV(TreeView tv, string strName, string strID) {
            TreeNode tnL = tv.Nodes[0].Nodes.Insert(0, strName, strName, 1, 2);
            tnL.Tag = strID;
            tv.Nodes[0].Expand();
        }

        private bool _IsBolFabrBV() {
            if (_strBol_PageName == tabControl1.SelectedTab.Text && rb_FABR_BV.Checked) return true;
            return false;
        }
        private bool _IsBolFabrSP() {
            if (_strBol_PageName == tabControl1.SelectedTab.Text && rb_FABR_SP.Checked) return true;
            return false;
        }

        private bool _IsWtRemSP() {
            if (_strWT_PageName == tabControl1.SelectedTab.Text && rbWT_REM_SOPUTKA.Checked) return true;
            return false;
        }
        private bool _IsWtRemBV() {
            if (_strWT_PageName == tabControl1.SelectedTab.Text && rbWT_REM_BV.Checked) return true;
            return false;
        }
        private bool _IsWtImpSP() {
            if (_strWT_PageName == tabControl1.SelectedTab.Text && rbWt_IMP_SP.Checked) return true;
            return false;
        }
        private bool _IsWtImpBV() {
            if (_strWT_PageName == tabControl1.SelectedTab.Text && rbWt_IMP_BV.Checked) return true;
            return false;
        }
        private bool _IsWtFabrBV() {
            if (_strWT_PageName == tabControl1.SelectedTab.Text && rbWt_FABR_BV.Checked) return true;
            return false;
        }

        private bool _IsVetexRemBV() {
            if (_strVetex_PageName == tabControl1.SelectedTab.Text && rbV_REM_BV.Checked) return true;
            return false;
        }
        private bool _IsVetexImpSP() {
            if (_strVetex_PageName == tabControl1.SelectedTab.Text && rbV_IMP_SP.Checked) return true;
            return false;
        }
        private bool _IsVetexImpBV() {
            if (_strVetex_PageName == tabControl1.SelectedTab.Text && rbV_IMP_BV.Checked) return true;
            return false;
        }
        private bool _IsVetexFabrBV() {
            if (_strVetex_PageName == tabControl1.SelectedTab.Text && rbV_FABR_BV.Checked) return true;
            return false;
        }

        private bool _IsMtmRemSP() {
            if (_strMTM_PageName == tabControl1.SelectedTab.Text && rbMTM_REM_SOPUTKA.Checked) return true;
            return false;
        }
        private bool _IsMtmImpSP() {
            if (_strMTM_PageName == tabControl1.SelectedTab.Text && rbMTM_IMP_SP.Checked) return true;
            return false;
        }
        private bool _IsMtmImpBV() {
            if (_strMTM_PageName == tabControl1.SelectedTab.Text && rbMTM_IMP_BV.Checked) return true;
            return false;
        }
        private bool _IsMtmFabrBV() {
            if (_strMTM_PageName == tabControl1.SelectedTab.Text && rbMTM_FABR_BV.Checked) return true;
            return false;
        }


        private void tvMTM_DoubleClick(object sender, EventArgs e) {
            _ShowDescription();
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e) {
            if (!rbWT_REM_BV.Checked) return;

            btSettingWT.Enabled = true;
            _FillTree();
        }
        private void tvWT_MouseUp(object sender, MouseEventArgs e) {
            tv_MouseUp(tvWT, e);
        }
        private void tvWT_DoubleClick(object sender, EventArgs e) {
            _ShowDescription();
        }
        private void _GetSelBARCODEVetex(ref HashSet<string> hsBarcodes) {
            int BARCODE = 27;
            if (rbV_IMPORT_SOPUTKA.Checked)
                BARCODE = 3;
            foreach (DataGridViewRow gvRow in dataGridViewVetex.SelectedRows)
                hsBarcodes.Add(((DataRowView)gvRow.DataBoundItem).Row[BARCODE].ToString());
        }
        private void _ExportToExcelKMVetex() {
            if (dataGridViewVetex.SelectedRows.Count == 0)
                return;
            string strDestPathKM = "";
            string strSrcPathKM = "";
            if (!DbfWrapper.CheckIfFileExist("VetexKM.dbf", ref strDestPathKM, ref strSrcPathKM, @"\ooo\vetex\")) { return; }

            HashSet<string> hsBarcodes = [];
            _GetSelBARCODEVetex(ref hsBarcodes);
            if (0 == hsBarcodes.Count)
                return;
            List<string> listKN = [];
            int KM = 5;
            int BARCODE = 10;
            int nCodeOut = -1;
            DataTable dtTableKM = Dbf.LoadDbfWithAddColumns(strDestPathKM, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableKM.Rows.Count; i++) {
                string strBarcode = dtTableKM.Rows[i][BARCODE].ToString();
                if (!hsBarcodes.Contains(strBarcode))
                    continue;
                listKN.Add(dtTableKM.Rows[i][KM].ToString());
            }
            ExcelExport excExp = new();
            if (!excExp.Create("KM", out Microsoft.Office.Interop.Excel.Worksheet xlsWorkSheet)) {
                MessageBox.Show(this, "Произошла ошибка при экспорте", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            System.GC.Collect();
            if (!excExp.ExportKM(xlsWorkSheet, ref listKN)) {
                MessageBox.Show(this, "Произошла ошибка при экспорте", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(this, "Экспорт прошел успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void menuExportToExcelKMVetex_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            //_ExportToExcelVetex();
            _ExportToExcelKMVetex();
            this.Cursor = Cursors.Default;
        }
        private void tvV_MouseUp(object sender, MouseEventArgs e) {
            tv_MouseUp(tvV, e);
        }

        private void tvV_DoubleClick(object sender, EventArgs e) {
            _ShowDescription();
        }
        private TreeNode _GetCurSelNode() {
            TreeNode tn = _GetCurSelNode0();
            if (tn != null && tn.Text.Length < 10)
                return null;
            return tn;
        }
        ////////////////////////////////////
        ////////////////////////////////////
        ////////////////////////////////////
        private GridViewExtensions.DataGridFilterExtender _GetCurDataGridFilterExtender() {
            if (_IsBolFabrBV() || _IsBolFabrSP()) return dataGridFilterBV;
            if (_IsVetexRemBV() || _IsVetexImpSP() || _IsVetexImpBV() || _IsVetexFabrBV()) return dataGridFilterVetex;
            if (_IsMtmRemSP() || _IsMtmImpSP() || _IsMtmImpBV() || _IsMtmFabrBV()) return dataGridFilterMtm;
            if (_IsWtRemBV() || _IsWtRemSP() || _IsWtImpSP() || _IsWtImpBV() || _IsWtFabrBV()) return dataGridFilterWesperTrading;
            return null;
        }
        private DataGridView _GetCurDataGridView() {
            if (_IsBolFabrBV() || _IsBolFabrSP()) return dataGridViewBV;
            if (_IsVetexRemBV() || _IsVetexImpSP() || _IsVetexImpBV() || _IsVetexFabrBV()) return dataGridViewVetex;
            if (_IsMtmRemSP() || _IsMtmImpSP() || _IsMtmImpBV() || _IsMtmFabrBV()) return dataGridViewMTM;
            if (_IsWtRemBV() || _IsWtRemSP() || _IsWtImpSP() || _IsWtImpBV() || _IsWtFabrBV()) return dataGridViewWesperTrading;
            return null;
        }
        private TreeNode _GetCurSelNode0() {
            if (_IsBolFabrBV() || _IsBolFabrSP()) return tvBV.SelectedNode;
            if (_IsVetexRemBV() || _IsVetexImpSP() || _IsVetexImpBV() || _IsVetexFabrBV()) return tvV.SelectedNode;
            if (_IsMtmRemSP() || _IsMtmImpSP() || _IsMtmImpBV() || _IsMtmFabrBV()) return tvMTM.SelectedNode;
            if (_IsWtRemBV() || _IsWtRemSP() || _IsWtImpSP() || _IsWtImpBV() || _IsWtFabrBV()) return tvWT.SelectedNode;
            return null;
        }
        private TreeView _GetCurTreeView() {
            if (_IsBolFabrBV() || _IsBolFabrSP()) return tvBV;
            if (_IsVetexRemBV() || _IsVetexImpSP() || _IsVetexImpBV() || _IsVetexFabrBV()) return tvV;
            if (_IsMtmRemSP() || _IsMtmImpSP() || _IsMtmImpBV() || _IsMtmFabrBV()) return tvMTM;
            if (_IsWtRemBV() || _IsWtRemSP() || _IsWtImpSP() || _IsWtImpBV() || _IsWtFabrBV()) return tvWT;
            return null;
        }
        private void _FillCurTree() {            
            if (_IsBolFabrBV() || _IsBolFabrSP()) { _FillTree(); return; }
            if (_IsVetexRemBV() || _IsVetexImpSP() || _IsVetexImpBV() || _IsVetexFabrBV()) { _FillTree(); return; }
            if (_IsMtmRemSP() || _IsMtmImpSP() || _IsMtmImpBV() || _IsMtmFabrBV()) { _FillTree(); return; }
            if (_IsWtRemBV() || _IsWtRemSP() || _IsWtImpSP() || _IsWtImpBV() || _IsWtFabrBV()) { _FillTree(); return; }
        }
        private Marking _GetCurMarkingObject() {
            if (_IsMtmRemSP()) return _markingMtmRestSP;            
            if (_IsVetexRemBV()) return _markingVetexRestBV;
            if (_IsVetexImpSP()) return _markingVetexImpSP;
            if (_IsVetexImpBV()) return _markingVetexImpBV;
            if (_IsVetexFabrBV()) return _markingVetexFabrBV;
            if (_IsWtRemBV()) return _markingWTRestBV;
            if (_IsWtRemSP()) return _markingWTRestSP;
            if (_IsBolFabrBV()) return _markingBolFabrBV;
            if (_IsBolFabrSP()) return _markingBolFabrSP;
            //
            if (_IsMtmImpSP()) return _markingMtmImpSP;
            if (_IsMtmImpBV()) return _markingMtmImpBV;
            if (_IsMtmFabrBV()) return _markingMtmFabrBV;

            if (_IsWtImpSP()) return _markingWtImpSP;
            if (_IsWtImpBV()) return _markingWtImpBV;
            if (_IsWtFabrBV()) return _markingWtFabrBV;

            return null;
        }
        private bool _IsBV() {
            if (_IsVetexRemBV() || _IsVetexImpBV() || _IsWtRemBV() || 
                _IsVetexFabrBV() || _IsBolFabrBV() ||
                _IsMtmImpBV() || _IsMtmFabrBV() ||
                _IsWtImpBV() || _IsWtFabrBV() ) return true;                          
            return false;
        }

        ////////////////////////////////////
        ////////////////////////////////////
        ////////////////////////////////////
        private void contextMenuWT_and_MTM_Opening(object sender, CancelEventArgs e) {
            TreeNode tn = _GetCurSelNode();
            bool bEnabled = true;
            if (null == tn)
                bEnabled = false;

            menuGetQRCodeFromFM.Enabled = false;
            menuPrintRepeatOnNumber.Enabled = false;
            menuPrintRepeatOnNumberLabel.Enabled = false;
            Marking mr = _GetCurMarkingObject();
            if (null != mr) {
                menuGetQRCodeFromFM.Enabled = true;
                menuPrintRepeatOnNumber.Enabled = true;
                menuPrintRepeatOnNumberLabel.Enabled = true;
            }

            menuAddToTableWesperTrading.Enabled = bEnabled;
            menuExportToExcelWT.Enabled = bEnabled;
            menuExportToExcelKM.Enabled = bEnabled;
            menuImportGTIN_WT.Enabled = bEnabled;
            menuGetQRCodeWesperTrading.Enabled = bEnabled;
            menuPrintWesperTrading.Enabled = bEnabled;
            menuPrintRepeatSelectedWesperTrading.Enabled = bEnabled;
            menuCheckStikersWesperTrading.Enabled = bEnabled;
            crhsnToolStripMenuItem.Enabled = bEnabled;
            menuUpdateWT.Enabled = bEnabled;
            menuMakeEAN13WesperTrading.Enabled = bEnabled;
            menuPrintRepeatSelectedLabel.Enabled = bEnabled;
            menuExportToRaskroynyCeh.Enabled = bEnabled;
            menuEdit.Enabled = bEnabled;
            menuExportToExcelNomenkl.Enabled = bEnabled;
            //menuGetQRCodeFromFM.Enabled = bEnabled;
            menuExportTo1СNomenkl.Enabled = bEnabled;
            DataGridView dgv = _GetCurDataGridView();
            if (null == dgv || 0 == dgv.SelectedRows.Count) {
                menuExportToExcelWT.Enabled = false;
                menuExportToExcelKM.Enabled = false;
                menuGetQRCodeWesperTrading.Enabled = false;
                menuPrintWesperTrading.Enabled = false;
                menuPrintRepeatSelectedWesperTrading.Enabled = false;
                menuMakeEAN13WesperTrading.Enabled = false;
                crhsnToolStripMenuItem.Enabled = false;
                menuPrintRepeatSelectedLabel.Enabled = false;
                menuExportToRaskroynyCeh.Enabled = false;
                menuEdit.Enabled = false;
                menuExportToExcelNomenkl.Enabled = false;
                menuExportTo1СNomenkl.Enabled = false;
            }
            toolStripMenuItem19.Enabled = false;
        }
        private void tvWT_AfterSelect(object sender, TreeViewEventArgs e) {
            AfterSelect(e);
        }
        private void tvMTM_AfterSelect(object sender, TreeViewEventArgs e) {
            AfterSelect(e);
        }

        private void menuUpdateWT_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            if (int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID))
                mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }
        private void menuAddToTableWesperTrading_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            if (!int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID)) return;
            if (!mr.AddNewProduct(this, nOrderID.ToString())) return;
            mr.ShowPage(this, dataGridViewMTM, dataGridFilterMtm, nOrderID.ToString());
            if (!mr.AddEAN13()) return;
            mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }
        private void menuExportToExcelWT_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            this.Cursor = Cursors.WaitCursor;
            mr.ExportToExcel(this);
            this.Cursor = Cursors.Default;
        }
        private void menuImportGTIN_WT_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            openFileDlg.CheckFileExists = true;
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "Эксель файлы(*.xlsx)|*.xlsx";

            if (openFileDlg.ShowDialog() != DialogResult.OK)
                return;
            if (!mr.ImportGtinFromExcel(this, openFileDlg.FileName)) return;
            if (int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID))
                mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }
        private void crhsnToolStripMenuItem_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            if (!mr.HideSelProducts()) return;
            if (!int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID)) return;
            mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }
        private void menuGetQRCodeWesperTrading_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            this.Cursor = Cursors.WaitCursor;
            mr.GetKM(this);
            this.Cursor = Cursors.Default;
            if (int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID))
                mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }

        private void menuPrintRepeatSelectedWesperTrading_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            int BARCODE;
            DlgSelRazmer dlg;
            if (_IsBV()) {
                BARCODE = 27;
                dlg = new();
            } else {
                BARCODE = 3;
                dlg = new(true);
            }
            dlg.ShowDialog();
            foreach (DataGridViewRow gvRow in dgv.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                mr.Print(dlg.GetPrintParms(), -1, row[BARCODE].ToString());
            }
        }
        private void menuMakeEAN13WesperTrading_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            if (!mr.AddEAN13()) return;
            if (int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID))
                mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }
        private void menuCheckStikersWesperTrading_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            if (!mr.CheckStikers())
                return;
            if (int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID))
                mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }

        private void menuPrintWesperTrading_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();

            DlgSelRazmer dlg;
            if (_IsBV())
                dlg = new();
            else
                dlg = new(true);
            dlg.ShowDialog();
            mr.Print(dlg.GetPrintParms());
            if (int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID))
                mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }
        private void menuHideOrder_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            TreeView tv = _GetCurTreeView();
            if (!mr.HideOrder(tv.SelectedNode.Text)) return;
            tv.SelectedNode.Remove();
        }
        private void _ShowDescription() {
            Marking mr = _GetCurMarkingObject();
            TreeView tv = _GetCurTreeView();
            if (!mr.ShowDescription(this, tv.SelectedNode.Text, out string strNewDescr))
                return;

            tv.SelectedNode.Text = tv.SelectedNode.Text[..19] + " (" + strNewDescr.Trim() + ")";
        }
        private void menuDescrTV_Click(object sender, EventArgs e) {
            _ShowDescription();
        }
        private void menuAddOrder_Click(object sender, EventArgs e) {
            string strName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            Marking mr = _GetCurMarkingObject();
            TreeView tv = _GetCurTreeView();

            if (!mr.AddOrder(strName, out string strID)) return;
            _InsertToTV(tv, strName, strID);
        }
        private void menuUpdateTreeView_Click(object sender, EventArgs e) {
            _FillCurTree();
        }

        private void menuReturnHidenTV_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            if (!mr.ReturnHidenOrders(this)) return;
            _FillCurTree();
        }
        private void menuExportToExcelKM_Click(object sender, EventArgs e) {

            DlgSelectLenghtKM dlg = new();
            if (DialogResult.OK != dlg.ShowDialog())
                return;
            
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            int BARCODE = 3;
            if (_IsBV())
                BARCODE = 27;
            HashSet<string> hsBarcodes = [];
            foreach (DataGridViewRow gvRow in dgv.SelectedRows)
                hsBarcodes.Add(((DataRowView)gvRow.DataBoundItem).Row[BARCODE].ToString());
            mr.ExportToExcelKM(hsBarcodes, dlg.GetSelLength());
        }

        private void menuPrintRepeatOnNumber_Click(object sender, EventArgs e) {
            DlgInputBox dlg = new();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            DlgSelRazmer dlg2;
            if (_IsBV())
                dlg2 = new();
            else
                dlg2 = new(true);
            dlg2.ShowDialog();

            Marking mr = _GetCurMarkingObject();
            mr.Print(dlg2.GetPrintParms(), dlg.GetNumber());
        }
        private void menuPrintRepeatSelectedVetexLabel_Click(object sender, EventArgs e) {
            if (!rbV_IMPORT.Checked)
                return;
            int BARCODE = 27;
            int FLABEL = 20;
            foreach (DataGridViewRow gvRow in dataGridViewVetex.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                string strBarcode = row[BARCODE].ToString();
                if (strBarcode.Length != 13)
                    continue;

                string strFlabel = row[FLABEL].ToString().Trim();
                if (strFlabel.Length != 13)
                    continue;                
            }
        }
        private void menuPrintRepeatOnNumberLabel_Click(object sender, EventArgs e) {
            DlgInputBox dlg = new();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            DlgSelRazmer dlg2;
            if (_IsBV())
                dlg2 = new();
            else
                dlg2 = new(true);
            dlg2.ShowDialog();

            Marking mr = _GetCurMarkingObject();
            mr.Print(dlg2.GetPrintParms(), dlg.GetNumber(), "", "", true);
        }

        private void menuPrintRepeatSelectedLabel_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            DlgSelRazmer dlg2;
            if (_IsBV())
                dlg2 = new();
            else
                dlg2 = new(true);
            dlg2.ShowDialog();

            int BARCODE = 27;
            int FLABEL = 20;
            foreach (DataGridViewRow gvRow in dgv.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                string strBarcode = row[BARCODE].ToString();
                if (strBarcode.Length != 13)
                    continue;

                string strFlabel = row[FLABEL].ToString().Trim();
                if (strFlabel.Length != 13)
                    continue;
                mr.Print(dlg2.GetPrintParms(), -1, strBarcode, "", true);
            }
        }
        private void AfterSelect(TreeViewEventArgs e) {
            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();

            _UpdateCount(dgv);
            Marking mr = _GetCurMarkingObject();
            if (int.TryParse(e.Node.Tag?.ToString(), out int ino)) {
                if (mr.ShowPage(this, dgv, dgFilter, ino.ToString()))
                    return;
            } else {
                DataTable dt = (DataTable)dgv.DataSource;
                dt?.Rows.Clear();
            }
        }
        private void tvV_AfterSelect(object sender, TreeViewEventArgs e) {
            AfterSelect(e);           
        }
        private void _FillTree() {
            Marking mr = _GetCurMarkingObject();
            TreeView tv = _GetCurTreeView();
            DataGridView dgv = _GetCurDataGridView();
            DataTable dt = (DataTable)dgv.DataSource;
            dt?.Rows.Clear();
            mr?.FillTree(tv);            
        }
        private void menuImportBARCODE_InFLABEL_Click(object sender, EventArgs e) {
            openFileDlg.CheckFileExists = true;
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "dbf файлы(*.dbf)|*.dbf";

            if (openFileDlg.ShowDialog() != DialogResult.OK)
                return;
            Marking mr = _GetCurMarkingObject();

            TreeView tv = _GetCurTreeView();
            if (!int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID)) return;//
            mr.ImportInFlabel(openFileDlg.FileName, nOrderID.ToString());
            mr.FillTree(tv);
        }
        private void menuAddToTableBV_Click(object sender, EventArgs e) {
            if (!rbSGP_BV.Checked )
                return;
            DlgAddToTableBV dlg = new();
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
            dlg.GetSelItems(out List<RestItemBV> listItems);
            if (0 == listItems.Count) return;

            string strDestPathRez = "";
            string strSrcPathRez = "";

            string strFileName = "BvRestShopsToStikers_SGP.dbf";          

            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPathRez, ref strSrcPathRez, @"\bv\generated\")) { this.Cursor = Cursors.Default; return; }

            int nCodeOut = -1;
            DataTable dtTableRez = Dbf.LoadDbfWithAddColumns(strDestPathRez, out _, ref nCodeOut);

            dtTableRez.Rows.Clear();

            foreach (RestItemBV item in listItems) {
                System.Data.DataRow rowAdd = dtTableRez.NewRow();
                rowAdd[0] = 0;
                rowAdd[1] = 0;
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

                
                rowAdd[RESTCOLIDBV.BARCODE] = item.GetBARCODE();
                rowAdd[RESTCOLIDBV.IZDNAME] = RestItemBV.GetIZDName(item.GetIZD());

                dtTableRez.Rows.Add(rowAdd);
            }
            if (!Dbf.AddTable(strDestPathRez, dtTableRez)) {
                MessageBox.Show("Ошибка добавления в файл: " + strDestPathRez, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try {
                File.Copy(strDestPathRez, strSrcPathRez, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _ShowPageBV();
        }
        private void menuExportToRaskroynyCeh_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            mr.ExportToRaskroyny(dgv);
        }
        private void menuGetQRCodeFromFM_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            _RunPrintStickerServer(mr.GetSettingsID());
        }
        private void rbV_REM_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbV_REM_BV.Checked) {
                rbV_FABR_BV.Checked = false;
                rbV_IMP_BV.Checked = false;
                rbV_IMP_SP.Checked = false;
            }

            if (!rbV_REM_BV.Checked) return;

            btSettingVetex.Enabled = true;

            dataGridViewVetex.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbV_FABR_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbV_FABR_BV.Checked) rbV_REM_BV.Checked = false;

            if (!rbV_FABR_BV.Checked) return;
            btSettingVetex.Enabled = true;
            dataGridViewVetex.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbV_IMP_SP_CheckedChanged(object sender, EventArgs e) {
            if (rbV_IMP_SP.Checked) rbV_REM_BV.Checked = false;

            if (!rbV_IMP_SP.Checked) return;
            btSettingVetex.Enabled = true;
            dataGridViewVetex.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbV_IMP_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbV_IMP_BV.Checked) rbV_REM_BV.Checked = false;

            if (!rbV_IMP_BV.Checked) return;
            btSettingVetex.Enabled = true;
            dataGridViewVetex.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void btBvFillAll_Click(object sender, EventArgs e) {
            _CollectRestBV();
        }
        private void button4_Click_1(object sender, EventArgs e) {
            CheckSpWithBar();
        }
        private void button6_Click(object sender, EventArgs e) {
            LoadDataToSGP();
        }
        private void button7_Click(object sender, EventArgs e) {
            CollectRestSP();
        }
        private void tvBV_DoubleClick(object sender, EventArgs e) {
            _ShowDescription();
        }
        private void tvBV_AfterSelect(object sender, TreeViewEventArgs e) {
            AfterSelect(e);
        }
        private void tvBV_MouseUp(object sender, MouseEventArgs e) {
            tv_MouseUp(tvBV, e);
        }
        private void rb_FABR_BV_CheckedChanged(object sender, EventArgs e) {
            if (!rb_FABR_BV.Checked) return;
            btSettingBV.Enabled = true;

            dataGridViewBV.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void _Edit() {
            Marking mr = _GetCurMarkingObject();
            if (null == mr)
                return;
            if (!mr.EditSelProducts()) return;

            DataGridView dgv = _GetCurDataGridView();
            GridViewExtensions.DataGridFilterExtender dgFilter = _GetCurDataGridFilterExtender();
            TreeView tv = _GetCurTreeView();
            if (!int.TryParse(tv.SelectedNode.Tag?.ToString(), out int nOrderID)) return;
            mr.ShowPage(this, dgv, dgFilter, nOrderID.ToString());
        }
        private void menuEdit_Click(object sender, EventArgs e) {
            _Edit();
        }
        private void dataGridViewMTM_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
        private void dataGridViewWesperTrading_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
        private void dataGridViewVetex_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
        private void dataGridViewBV_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
        private void dataGridViewSoputka_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
        private void _GetNomenklSP(DataGridView dgv, out List<(string name, string barcode)> listNomenkl) {
            listNomenkl = [];
            ColorSP colorSP = new();            
            foreach (DataGridViewRow gvRow in dgv.SelectedRows) {                              
                string strColor = ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLID.CCODE].ToString().Trim();
                if(int.TryParse(strColor, out int nColor))
                    strColor = ", цвет " + colorSP.GetNameColor(nColor);

                string Gtin = ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLID.GTIN].ToString().Trim();
                string Name = ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLID.MOD].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLID.ART].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLID.ART2].ToString().Trim();
                //Name += " ";
                Name += strColor;
                //Name += " ";
                Name += ", р." + ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLID.RAZ].ToString().Trim();
                listNomenkl.Add((Name, Gtin));
            }
        }
        private void _GetNomenklBV(DataGridView dgv, out List<(string name, string barcode)> listNomenkl) {
            listNomenkl = [];
            foreach (DataGridViewRow gvRow in dgv.SelectedRows) {
                string Gtin = ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.GTIN].ToString().Trim();
                string Name = ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.IZDNAME].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.PRS].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.MOD].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.ART].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.PATTERN].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.CCODE].ToString().Trim();
                Name += " ";
                Name += ((DataRowView)gvRow.DataBoundItem).Row[RESTCOLIDBV.RAZ].ToString().Trim();
                listNomenkl.Add((Name, Gtin));
            }            
        }
        private void menuExportToExcelNomenkl_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            List<(string name, string barcode)> listNomenkl;
            if (_IsBV())
                _GetNomenklBV(dgv, out listNomenkl);
            else
                _GetNomenklSP(dgv, out listNomenkl);
            mr.ExportToExcelNomenkl(listNomenkl);
        }

        private void menuExportTo1СNomenkl_Click(object sender, EventArgs e) {
            Marking mr = _GetCurMarkingObject();
            DataGridView dgv = _GetCurDataGridView();
            List<(string name, string barcode)> listNomenkl;
            if (_IsBV())
                _GetNomenklBV(dgv, out listNomenkl);
            else
                _GetNomenklSP(dgv, out listNomenkl);

            DlgSample dlg = new();
            dlg.To1СNomenkl(listNomenkl);
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;

            mr.ExportTo1СNomenkl(listNomenkl);
        }

        private void menuCheckSoputkaWithBar_Click(object sender, EventArgs e) {
            CheckSpWithBar();
        }

        private void menuGetQRCodeFromFMSoputka_Click(object sender, EventArgs e) {
            _RunPrintStickerServer(_strSettingID_BV);
        }

        private void menuGetQRCodeFromFMBV_Click(object sender, EventArgs e) {
            _RunPrintStickerServer(_strSettingID_BV);
        }

        private void menuLoadByBarcodelSP_Click(object sender, EventArgs e) {
            LoadDataToSGP();
        }

        private void rb_FABR_SP_CheckedChanged(object sender, EventArgs e) {
            if (!rb_FABR_SP.Checked) return;
            btSettingSoputka.Enabled = true;

            dataGridViewSoputka.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }

        private void rb_FABR_SP_CheckedChanged_1(object sender, EventArgs e) {
            if (!rb_FABR_SP.Checked) return;
            btSettingBV.Enabled = true;

            dataGridViewBV.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }

        private void btInfo1_Click(object sender, EventArgs e) {
            DlgImage dlg = new(3);
            dlg.ShowDialog(this);
        }

        private void btInfo2_8_Click(object sender, EventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void btInfo12_Click(object sender, EventArgs e) {
            DlgImage dlg = new(5);
            dlg.ShowDialog(this);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(3);
            dlg.ShowDialog(this);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(5);
            dlg.ShowDialog(this);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(4);
            dlg.ShowDialog(this);
        }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(6);
            dlg.ShowDialog(this);
        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(7);
            dlg.ShowDialog(this);

        }

        private void linkLabel12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(8);
            dlg.ShowDialog(this);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            panel7.Visible = cbRestsMTM.Checked;
            if (!cbRestsMTM.Checked) {
                TreeView tv = _GetCurTreeView();
                DataGridView dgv = _GetCurDataGridView();
                if (null != dgv) {
                    DataTable dt = (DataTable)dgv.DataSource;
                    dt?.Rows.Clear();
                }
                tv?.Nodes.Clear();
                rbMTM_REM_SOPUTKA.Checked = false;
            }
        }

        private void cbRests_CheckedChanged(object sender, EventArgs e) {
            panel8.Visible = cbRestsWT.Checked;
            if (!cbRestsWT.Checked) {
                TreeView tv = _GetCurTreeView();
                DataGridView dgv = _GetCurDataGridView();
                if (null != dgv) {
                    DataTable dt = (DataTable)dgv.DataSource;
                    dt?.Rows.Clear();
                }
                tv?.Nodes.Clear();
                rbWT_REM_SOPUTKA.Checked = false;
                rbWT_REM_BV.Checked = false;
            }
        }

        private void dataGridViewMTM_CellContentClick(object sender, DataGridViewCellEventArgs e) {

        }
        private void cbRestsVetex_CheckedChanged(object sender, EventArgs e) {
            panel11.Visible = cbRestsVetex.Checked;
            if (!cbRestsVetex.Checked) {
                TreeView tv = _GetCurTreeView();
                DataGridView dgv = _GetCurDataGridView();
                if (null != dgv) {
                    DataTable dt = (DataTable)dgv.DataSource;
                    dt?.Rows.Clear();
                }
                tv?.Nodes.Clear();
                rbV_REM_BV.Checked = false;

                rbV_FABR_BV.Checked = false;
                rbV_IMP_BV.Checked = false;
                rbV_IMP_SP.Checked = false;
            }
        }
        private void linkLabel13_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            DlgImage dlg = new(9);
            dlg.ShowDialog(this);
        }

        private void cbRestsSP_CheckedChanged(object sender, EventArgs e) {
            bool bChecked = cbRestsSP.Checked;
            rbALL.Visible = bChecked;
            rb053.Visible = bChecked;
            rb057.Visible = bChecked;
            rb300.Visible = bChecked;
            rb260.Visible = bChecked;
            rb310.Visible = bChecked;
            rb370.Visible = bChecked;
            rb375.Visible = bChecked;
            rb397.Visible = bChecked;
            rb401.Visible = bChecked;
            rb414.Visible = bChecked;
            rb416.Visible = bChecked;
            rb421.Visible = bChecked;
            rb432.Visible = bChecked;
            rb433.Visible = bChecked;
            label38.Visible = bChecked;
            rbSGP.Visible = bChecked;
            btRestSP.Visible = bChecked;

            if (!bChecked) {
                DataTable dt = (DataTable)dataGridViewSoputka.DataSource;
                dt?.Rows.Clear();

                rbALL.Checked = bChecked;
                rb053.Checked = bChecked;
                rb057.Checked = bChecked;
                rb300.Checked = bChecked;
                rb260.Checked = bChecked;
                rb310.Checked = bChecked;
                rb370.Checked = bChecked;
                rb375.Checked = bChecked;
                rb397.Checked = bChecked;
                rb401.Checked = bChecked;
                rb414.Checked = bChecked;
                rb416.Checked = bChecked;
                rb421.Checked = bChecked;
                rb432.Checked = bChecked;
                rb433.Checked = bChecked;
                rbSGP.Checked = bChecked;
            }
        }

        private void cbRestsBV_CheckedChanged(object sender, EventArgs e) {

            bool bChecked = cbRestsBV.Checked;
            btBvFillAll.Visible = bChecked;

            if (bChecked)
                panel1.Height = 400;
            else {
                panel1.Height = 50;
                DataTable dt = (DataTable)dataGridViewBV.DataSource;
                dt?.Rows.Clear();

                rbAll_BV.Checked = bChecked;
                rb053_BV.Checked = bChecked;
                rb057_BV.Checked = bChecked;
                rb300_BV.Checked = bChecked;
                rb260_BV.Checked = bChecked;
                rb310_BV.Checked = bChecked;
                rb370_BV.Checked = bChecked;
                rb375_BV.Checked = bChecked;
                rb397_BV.Checked = bChecked;
                rb401_BV.Checked = bChecked;
                rb414_BV.Checked = bChecked;
                rb416_BV.Checked = bChecked;
                rb421_BV.Checked = bChecked;
                rb432_BV.Checked = bChecked;
                rb433_BV.Checked = bChecked;
                rbSGP_BV.Checked = bChecked;
            }
        }

        private void rbWt_FABR_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbWt_FABR_BV.Checked) {
                rbWT_REM_SOPUTKA.Checked = false;
                rbWT_REM_BV.Checked = false;
            }
            if (!rbWt_FABR_BV.Checked) return;
            btSettingWT.Enabled = true;
            dataGridViewWesperTrading.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbWt_IMP_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbWt_IMP_BV.Checked) {
                rbWT_REM_SOPUTKA.Checked = false;
                rbWT_REM_BV.Checked = false;
            }
            if (!rbWt_IMP_BV.Checked) return;
            btSettingWT.Enabled = true;
            dataGridViewWesperTrading.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbWt_IMP_SP_CheckedChanged(object sender, EventArgs e) {
            if (rbWt_IMP_SP.Checked) {
                rbWT_REM_SOPUTKA.Checked = false;
                rbWT_REM_BV.Checked = false;
            }
            if (!rbWt_IMP_SP.Checked) return;
            btSettingWT.Enabled = true;
            dataGridViewWesperTrading.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbMTM_FABR_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbMTM_FABR_BV.Checked) rbMTM_REM_SOPUTKA.Checked = false;
            
            if (!rbMTM_FABR_BV.Checked) return;
            btSettingsMTM.Enabled = true;
            dataGridViewMTM.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbMTM_IMP_BV_CheckedChanged(object sender, EventArgs e) {
            if (rbMTM_IMP_BV.Checked) rbMTM_REM_SOPUTKA.Checked = false;

            if (!rbMTM_IMP_BV.Checked) return;
            btSettingsMTM.Enabled = true;
            dataGridViewMTM.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
        private void rbMTM_IMP_SP_CheckedChanged(object sender, EventArgs e) {
            if (rbMTM_IMP_SP.Checked) rbMTM_REM_SOPUTKA.Checked = false;

            if (!rbMTM_IMP_SP.Checked) return;
            btSettingsMTM.Enabled = true;
            dataGridViewMTM.ContextMenuStrip = contextMenu_ForAll;
            _FillTree();
        }
    }
    public class KM(int nPod, string strKNIn) {
        public int N = nPod;
        public string strKN = strKNIn;
    }        
    public class PrintParms {
        public bool bMultySize = true;
        public string strDate = "";
        public string strPrefix = "";
    }
}
