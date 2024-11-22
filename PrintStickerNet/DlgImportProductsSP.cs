using DbfLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgImportProductsSP : Form {
        public DlgImportProductsSP() {
            InitializeComponent();
        }

        private void DlgImportProductsSP_Load(object sender, EventArgs e) {
            _ShowView();
        }
        private void _ShowView() {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbImportProductsSP.dbf", ref strDestPath, ref strSrcPath, "\\Soputka\\"))
                return;
            _GetUseColumn(out Dictionary<string, string> hUseColumns);

            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0", hUseColumns);
            DataView dv = dtTable.DefaultView;
            dataGridView.DataSource = dv.ToTable();
            dataGridFilter.DataGridView = dataGridView;
           
            _InidataGridView(ref dataGridView);
        }
        private void _InidataGridView(ref DataGridView dataGridView) {
            dataGridView.Columns[0].Visible = false;//№"
            dataGridView.Columns[1].Visible = false;//DEL

            dataGridView.Columns[2].Width = 220;//
            dataGridView.Columns[3].Width = 130;//

            DataGridViewCellStyle style = new() {
                Font = new Font(dataGridView.Font, FontStyle.Bold)
            };
            dataGridView.DefaultCellStyle = style;
        }
        private void _GetUseColumn(out Dictionary<string, string> hUseColumns) {
            hUseColumns = [];
            hUseColumns.Add("№", "№");//0
            hUseColumns.Add("DEL", "DEL");//1
            hUseColumns.Add("PRODUCT", "Наименование изделия");//2
            hUseColumns.Add("IMPORT", "Импортировать");//3
        }

        private void menuUpdate_Click(object sender, EventArgs e) {
            _ShowView();
        }
        private bool _Hide(string strFileName) {
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return false;
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, "\\Soputka\\"))
                return false;
            GenLogic.CopyToArhive(strSrcPath);
            try {
                List<int> listRows = [];
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    listRows.Add((int)row.Cells[0].Value);
                listRows.Sort();
                byte nDEL = 42;
                DateTime dtOpenedFile = Win32.GetLastWriteTime(strSrcPath);
                if (!Dbf.SetDelRowBytes(strSrcPath, listRows, nDEL, ref dtOpenedFile))
                    return false;
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private void menuHide_Click(object sender, EventArgs e) {
            if (!_Hide("DbImportProductsSP.dbf"))
                return;
            _ShowView();
        }
        protected bool _Edit(ref DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count)
                return false;
            DlgAddImportProduct dlg = new();
            dlg.SetEditDataTableRow(dt);
            if (DialogResult.OK != dlg.ShowDialog(this))
                return false;
            return true;
        }
        private void _Edit() {
            if (0 == dataGridView.SelectedRows.Count)
                return;

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbImportProductsSP.dbf", ref strDestPath, ref strSrcPath, "\\Soputka\\"))
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
                MessageBox.Show(this, "Ошибка сохранения в файл " + strSrcPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _ShowView();
        }
        private void menuEdit_Click(object sender, EventArgs e) {
            _Edit();
        }

        private void menuAdd_Click(object sender, EventArgs e) {
            DlgAddImportProduct dlg = new();
            if (DialogResult.OK != dlg.ShowDialog())
                return;
            var (product, import) = dlg.GetSelItems();
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbImportProductsSP.dbf", ref strDestPath, ref strSrcPath, "\\Soputka\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "№", "1");
            dtTable.Rows.Clear();

            int PRODUCT = 2;
            int IMPORT = 3;
            DataRow rowAdd = dtTable.NewRow();
            rowAdd[PRODUCT] = product;
            rowAdd[IMPORT] = import;

            dtTable.Rows.Add(rowAdd);
            if (!DbfWrapper.AddDoDB(null, strDestPath, strSrcPath, dtTable))
                return;

            _ShowView();
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e) {
            if (0 == dataGridView.SelectedRows.Count) {
                menuEdit.Enabled = false;
                menuImport.Enabled = false;
                menuNotImport.Enabled = false;
                menuHide.Enabled = false;

            } else {
                menuEdit.Enabled = true;
                menuImport.Enabled = true;
                menuNotImport.Enabled = true;
                menuHide.Enabled = true;
            }
        }

        private void dataGridView_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }

        private void _SetSelImport(string strImport) {
            if (0 == dataGridView.SelectedRows.Count)
                return;
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbImportProductsSP.dbf", ref strDestPath, ref strSrcPath, "\\Soputka\\"))
                return;
            GenLogic.CopyToArhive(strSrcPath);

            foreach (DataGridViewRow row in dataGridView.SelectedRows) {             
                string strN = row.Cells[0].Value.ToString();
                if (!Dbf.SetValue(strSrcPath, "№", strN, "IMPORT", strImport.PadRight(3), Win32.GetLastWriteTime(strSrcPath))) {
                    Console.Beep();
                    MessageBox.Show(this, "Ошибка записи в файл: " + strSrcPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Trace.WriteLine("Ошибка записи в файл: " + strSrcPath);
                    return;
                }
            }
            _ShowView();
        }
        private void menuImport_Click(object sender, EventArgs e) {
            _SetSelImport("ДА");
        }

        private void menuNotImport_Click(object sender, EventArgs e) {
            _SetSelImport("НЕТ");
        }

        private void btCancel_Click(object sender, EventArgs e) {

        }
    }
}
