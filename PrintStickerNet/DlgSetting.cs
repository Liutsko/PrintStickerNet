using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using DbfLib;
using PrintSticker.MarkingObjectsBase;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgSetting : Form {
        public const int OMSID = 5;
        public const int OMSCONN = 6;
        public const int SERT = 7;
        public const int NAME = 9;
        public const int MainFolder = 14;
        public const int OrdersKMFN = 15;
        public const int KMFN = 16;
        public const int GtinFN = 17;
        public const int OrdersFN = 18;
        public const int ProdSPFN = 19;
        public const int ProdBVFN = 20;
        public const int ProdSPIMPO = 21;
        public const int ProdBVIMPO = 22;
        public const int ProdBVFABR = 23;
        public const int PRINTER = 24;
        public const int c1IP = 25;
        public const int c1Base = 26;
        public const int c1User = 27;
        public const int c1Psw = 28;
        public const int ProdSPFABR = 29;
        public const int DECLAR = 30;
        public const int DECLARDT = 31;
        public const int GS1PATH = 32;
        public const int GS1LOGIN = 33;
        public const int GS1PSW = 34;
              
        private readonly  string _strID;
        private readonly _Form1 _parent;
        private DateTime _dtOpenedFile = new (1929,1,1);
        private readonly static string _strSettingPath = @"C:\Po_BOLSHEVICHKA\PrintStickerServer\setting.dbf";
        private readonly Marking _marking;
        
        public DlgSetting(_Form1 parent, Marking marking) {
            InitializeComponent();
            _parent = parent;
            _marking = marking;

            _strID = marking?.GetSettingsID();
        }

        public static string GetSettingPath() { return _strSettingPath; }

        private bool _CheckValuesInDB(out DataTable dtTableSetting, out string strFileSettingPath) {
            if (null == _strID) {
                dtTableSetting = null;
                strFileSettingPath = "";
                return false;
            }

            strFileSettingPath = GetSettingPath();
            if (!File.Exists(strFileSettingPath)) {
                MessageBox.Show(_parent, "Отсутствует файл:" + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dtTableSetting = null;
                return false;
            }
            int nCodeOut = -1;
            dtTableSetting = Dbf.LoadDbfWithAddColumns(strFileSettingPath, out _, ref nCodeOut, "DOC", _strID);
            if (1 != dtTableSetting.Rows.Count) {
                MessageBox.Show(_parent, "Несколько записей в файле с номером:" + _strID, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private void DlgSetting_Load(object sender, EventArgs e) {
            txtSettingPath.Text = GetSettingPath();
            txtID.Text = _strID;
            if (!_CheckValuesInDB(out DataTable dtTableSetting, out string strFileSettingPath)) {
                Close();
                return;
            }
            _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);            

            if (System.DBNull.Value != dtTableSetting.Rows[0][NAME])
                txtCompanyName.Text = dtTableSetting.Rows[0][NAME].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][OMSID])
                txtOMSID.Text = dtTableSetting.Rows[0][OMSID].ToString();
            txtOMSID.Tag = txtOMSID.Text;

            if (System.DBNull.Value != dtTableSetting.Rows[0][OMSCONN])
                txtOMSCONN.Text = dtTableSetting.Rows[0][OMSCONN].ToString();
            txtOMSCONN.Tag = txtOMSCONN.Text;

            if (System.DBNull.Value != dtTableSetting.Rows[0][SERT])
                txtSERT.Text = dtTableSetting.Rows[0][SERT].ToString();
            txtSERT.Tag = txtSERT.Text;

            if (System.DBNull.Value != dtTableSetting.Rows[0][MainFolder])
                txtRootFolder.Text = dtTableSetting.Rows[0][MainFolder].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][OrdersKMFN])
                txtOrdersKNFile.Text = dtTableSetting.Rows[0][OrdersKMFN].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][KMFN])
                txtKMFile.Text = dtTableSetting.Rows[0][KMFN].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][GtinFN])
                txtGTINFile.Text = dtTableSetting.Rows[0][GtinFN].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][OrdersFN])
                txtOrdersFile.Text = dtTableSetting.Rows[0][OrdersFN].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][ProdSPFN])
                txtRestSPFile.Text = dtTableSetting.Rows[0][ProdSPFN].ToString();

            if (System.DBNull.Value != dtTableSetting.Rows[0][ProdBVFN])
                txtRestBVFile.Text = dtTableSetting.Rows[0][ProdBVFN].ToString();

            txtRootDisk.Text = DbfWrapper.GetRootDisk();
            txtZPL.Text = _marking.GetZplName();

            if (System.DBNull.Value != dtTableSetting.Rows[0][DECLAR])
                txtDECLAR.Text = dtTableSetting.Rows[0][DECLAR].ToString().Trim();
            txtDECLAR.Tag = txtDECLAR.Text;

            if (System.DBNull.Value != dtTableSetting.Rows[0][DECLARDT])
                txtDECLARDT.Text = dtTableSetting.Rows[0][DECLARDT].ToString().Trim();
            txtDECLARDT.Tag = txtDECLARDT.Text;

            if (System.DBNull.Value != dtTableSetting.Rows[0][GS1PATH])
                txtGS1PATH.Text = dtTableSetting.Rows[0][GS1PATH].ToString().Trim();
            txtGS1PATH.Tag = txtGS1PATH.Text;

            if (System.DBNull.Value != dtTableSetting.Rows[0][GS1LOGIN])
                txtGS1LOGIN.Text = dtTableSetting.Rows[0][GS1LOGIN].ToString().Trim();
            txtGS1LOGIN.Tag = txtGS1LOGIN.Text;

            if (System.DBNull.Value != dtTableSetting.Rows[0][GS1PSW])
                txtGS1PSW.Text = dtTableSetting.Rows[0][GS1PSW].ToString().Trim();
            txtGS1PSW.Tag = txtGS1PSW.Text;

            _Resize();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        private void btSave_Click(object sender, EventArgs e) {
            if (!_CheckValuesInDB(out _, out string strFileSettingPath)) {
                Close();
                return;
            }
            if (txtOMSID.Tag.ToString() != txtOMSID.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "OMSID", txtOMSID.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            if (txtOMSCONN.Tag.ToString() != txtOMSCONN.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "OMSCONN", txtOMSCONN.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            if (txtSERT.Tag.ToString() != txtSERT.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "SERT", txtSERT.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            //
            if (txtDECLAR.Tag.ToString() != txtDECLAR.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "DECLAR", txtDECLAR.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            if (txtDECLARDT.Tag.ToString() != txtDECLARDT.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "DECLARDT", txtDECLARDT.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            if (txtGS1PATH.Tag.ToString() != txtGS1PATH.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "GS1PATH", txtGS1PATH.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            if (txtGS1LOGIN.Tag.ToString() != txtGS1LOGIN.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "GS1LOGIN", txtGS1LOGIN.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            if (txtGS1PSW.Tag.ToString() != txtGS1PSW.Text) {
                if (!Dbf.SetValue(strFileSettingPath, "DOC", _strID, "GS1PSW", txtGS1PSW.Text, _dtOpenedFile)) {
                    MessageBox.Show("Ошибка записи в файл: " + strFileSettingPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strFileSettingPath);
                    return;
                }
                _dtOpenedFile = Win32.GetLastWriteTime(strFileSettingPath);
            }
            Close();
        }

        private void timerStart_Tick(object sender, EventArgs e) {
            btCancel.Focus();
            timerStart.Stop();
        }

        private void btInfo1_Click(object sender, EventArgs e) {
            DlgImage dlg = new(0);
            dlg.ShowDialog(this);
        }

        private void btInfo2_Click(object sender, EventArgs e) {
            DlgImage dlg = new(1);
            dlg.ShowDialog(this);
        }

        private void btInfo3_Click(object sender, EventArgs e) {
            DlgImage dlg = new(2);
            dlg.ShowDialog(this);
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void btOpenZPL_Click(object sender, EventArgs e) {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_marking.GetZplName(), ref strDestPath, ref strSrcPath, @"\1c\Matrix\patern\")) return;

            DlgShowZpl dlg = new(strSrcPath, _marking.GetStickerEncoding());
            dlg.ShowDialog(this);
        }
        private void btOpenDbTnved_Click(object sender, EventArgs e) {
            DlgTnved dlg = new(_parent);
            dlg.ShowDialog(this);
        }
    }
}
