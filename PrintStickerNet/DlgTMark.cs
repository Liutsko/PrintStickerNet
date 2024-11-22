using DbfLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgTMark : Form {
        private readonly string _strFileBdName = "DbTrademark.dbf";
        private readonly string _strNOTEXIST = "ОТСУТСТВУЕТ";
        private bool _bChangedTable = false;
        public DlgTMark() {
            InitializeComponent();
        }

        public bool IsChanged() {
            return _bChangedTable;
        }
        private void DlgTMark_Load(object sender, EventArgs e) {
            _ShowView();
        }

        private void _GetUseColumn(out Dictionary<string, string> hUseColumns) {
            hUseColumns = [];
            hUseColumns.Add("№", "№");//0
            hUseColumns.Add("DEL", "DEL");//1
            hUseColumns.Add("NAME", "Торговая марка");//2
        }
        private void _InidataGridView(ref DataGridView dataGridView) {
            dataGridView.Columns[0].Visible = false;//№"
            dataGridView.Columns[1].Visible = false;//DEL
            dataGridView.Columns[2].Width = 250;//
            DataGridViewCellStyle style = new() {
                Font = new Font(dataGridView.Font, FontStyle.Bold)
            };
            dataGridView.DefaultCellStyle = style;
        }

        public List<string> GetTM() {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_strFileBdName, ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return [];

            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");

            List<string> list = [];
            int NAME = 2;
            for (int i = 0; i < dtTable.Rows.Count; i++) {
                if (System.DBNull.Value != dtTable.Rows[i][NAME])
                    list.Add((string)dtTable.Rows[i][NAME]);
                else
                    list.Add("");
            }
            return list;
        }

        private void _ShowView() {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_strFileBdName, ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;
            _GetUseColumn(out Dictionary<string, string> hUseColumns);

            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0", hUseColumns);
            
            DataView dv = dtTable.DefaultView;
            //dv.Sort = "Перед.СПО desc, № desc";
            dataGridView.DataSource = dv.ToTable();
            dataGridFilter.DataGridView = dataGridView;
            _InidataGridView(ref dataGridView);
        }
        private void _FormClose() {
            dataGridView.DataSource = null;
            dataGridFilter.DataGridView = null;
        }

        private void btCancel_Click(object sender, EventArgs e) {
            _FormClose();
        }

        private void DlgTMark_FormClosing(object sender, FormClosingEventArgs e) {
            _FormClose();
        }

        private void menuUpdate_Click(object sender, EventArgs e) {
            _ShowView();
        }
        protected bool _Edit(ref DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count)
                return false;
            DlgInputBox dlg = new(false);
            dlg.SetCaption("Введите");
            dlg.SetFildName("Название Торговой марки");
            int NAME = 2;            
            dlg.SetString(dt.Rows[0][NAME].ToString());
            if (DialogResult.OK != dlg.ShowDialog())
                return false;
            string strVal = dlg.GetString().Trim();
            if (strVal.Length > 30)
                strVal = strVal.Substring(0, 30);

            if ("" == strVal || _strNOTEXIST.ToUpper() == strVal.ToUpper()) {
                MessageBox.Show("Такой элемент справочника уже добавлен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            dt.Rows[0][NAME] = strVal;
            return true;
        }
        private void _Edit() {
            if (0 == dataGridView.SelectedRows.Count)
                return;

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_strFileBdName, ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;

            List<string> listRows = [];
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
                listRows.Add(((int)row.Cells[0].Value).ToString());

            int nCodeOut = -1;
            DataTable dtTableRow = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "№", listRows[0]);
            if (0 == dtTableRow.Rows.Count)
                return;
            if (!_Edit(ref dtTableRow))
                return;
            int nNewRowOut = -1;
            GenLogic.CopyToArhive(strSrcPath);
            int N = 0;
            if (!Dbf.SaveOneRow(strSrcPath, dtTableRow, N, ref nNewRowOut)) {
                MessageBox.Show(this.Parent, "Ошибка сохранения в файл " + strSrcPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _bChangedTable = true;
            _ShowView();
        }
        private void menuHide_Click(object sender, EventArgs e) {
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return;
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_strFileBdName, ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;
            GenLogic.CopyToArhive(strSrcPath);
            try {
                List<int> listRows = [];
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    listRows.Add((int)row.Cells[0].Value);
                listRows.Sort();
                byte nDEL = 42;
                DateTime dtOpenedFile = Win32.GetLastWriteTime(strSrcPath);
                if (!Dbf.SetDelRowBytes(strSrcPath, listRows, nDEL, ref dtOpenedFile))
                    return;
                _bChangedTable = true;
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _ShowView();
        }

        private void menuAdd_Click(object sender, EventArgs e) {
            DlgInputBox dlg = new(false);
            dlg.SetCaption("Введите");
            dlg.SetFildName("Название Торговой марки");

            if (DialogResult.OK != dlg.ShowDialog())
                return;

            string name = dlg.GetString().Trim();
            if (name.Length > 30)
                name = name.Substring(0, 30);

            if ("" == name || _strNOTEXIST.ToUpper() == name.ToUpper()) {
                MessageBox.Show("Такой элемент справочника уже добавлен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_strFileBdName, ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "№", "1");
            dtTable.Rows.Clear();

            int NAME = 2;
            System.Data.DataRow rowAdd = dtTable.NewRow();
            rowAdd[NAME] = name;
            dtTable.Rows.Add(rowAdd);
            if (!DbfWrapper.AddDoDB(null, strDestPath, strSrcPath, dtTable))
                return;
            _bChangedTable = true;
            _ShowView();
        }

        private void menuEdit_Click(object sender, EventArgs e) {
            _Edit();
        }

        private void dataGridView_DoubleClick(object sender, EventArgs e) {
            int NAME = 2;
            foreach (DataGridViewRow row in dataGridView.SelectedRows) {
                if (DBNull.Value == row.Cells[NAME].Value || _strNOTEXIST.ToUpper() == row.Cells[NAME].Value.ToString().ToUpper())
                    return;
            }
            _Edit();
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e) {
            if (0 == dataGridView.SelectedRows.Count) {
                menuEdit.Enabled = false;
                menuHide.Enabled = false;
            } else {
                menuEdit.Enabled = false;
                menuHide.Enabled = false;

                int NAME = 2;
                foreach (DataGridViewRow row in dataGridView.SelectedRows) {
                    if (DBNull.Value == row.Cells[NAME].Value || _strNOTEXIST.ToUpper() == row.Cells[NAME].Value.ToString().ToUpper())
                        return;
                }
                menuEdit.Enabled = true;
                menuHide.Enabled = true;
            }
        }
    }
}
