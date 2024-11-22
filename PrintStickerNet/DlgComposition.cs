using DbfLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgComposition : Form {
        public DlgComposition() {
            InitializeComponent();
        }
        private void _InidataGridView(ref DataGridView dataGridView) {
            dataGridView.Columns[0].Visible = false;//№"
            dataGridView.Columns[1].Visible = false;//DEL

            dataGridView.Columns[2].Width = 250;//
            dataGridView.Columns[3].Width = 250;//

            DataGridViewCellStyle style = new() {
                Font = new Font(dataGridView.Font, FontStyle.Bold)
            };
            dataGridView.DefaultCellStyle = style;
        }
        private void _GetUseColumn(out Dictionary<string, string> hUseColumns) {
            hUseColumns = [];
            hUseColumns.Add("№", "№");//0
            hUseColumns.Add("DEL", "DEL");//1
            hUseColumns.Add("NAME", "Состав ткани");//2
            hUseColumns.Add("SHORT", "Сокращенное название состава ткани");//4
        }

        private void DlgComposition_Load(object sender, EventArgs e) {
            _ShowView();
        }
        private void _ShowView() {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbComposition.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;
            _GetUseColumn(out Dictionary<string, string> hUseColumns);

            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0", hUseColumns);
            
            int SHORT = 3;
            HashSet<string> hsNames = [];
            for (int i = 0; i < dtTable.Rows.Count; i++)
                hsNames.Add((string)dtTable.Rows[i][SHORT]);

            if (hsNames.Count != dtTable.Rows.Count)
                MessageBox.Show(this, "Внимание есть одинаковые сокращения", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            DataView dv = dtTable.DefaultView;
            //dv.Sort = "Перед.СПО desc, № desc";
            dataGridView.DataSource = dv.ToTable();
            dataGridFilter.DataGridView = dataGridView;
            _InidataGridView(ref dataGridView);
        }
        private void btCancel_Click(object sender, EventArgs e) {
            _FormClose();
        }
        private void contextMenu_Opening(object sender, CancelEventArgs e) {
            if (0 == dataGridView.SelectedRows.Count) {
                menuEdit.Enabled = false;
                menuHide.Enabled = false;
            } else {
                menuEdit.Enabled = true;
                menuHide.Enabled = true;
            }
        }
        protected bool _Edit(ref DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count)
                return false;
            DlgAddComposition dlg = new();
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
            if (!DbfWrapper.CheckIfFileExist("DbComposition.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
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
            _ShowView();
        }
        private void menuAdd_Click(object sender, EventArgs e) {
            DlgAddComposition dlg = new();
            if (DialogResult.OK != dlg.ShowDialog())
                return;
            var (name, shortName) = dlg.GetSelItems();

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbComposition.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "№", "1");
            dtTable.Rows.Clear();

            int NAME = 2;
            int SHORT = 3;
            System.Data.DataRow rowAdd = dtTable.NewRow();
            rowAdd[NAME] = name;
            rowAdd[SHORT] = shortName;

            dtTable.Rows.Add(rowAdd);
            if (!DbfWrapper.AddDoDB(null, strDestPath, strSrcPath, dtTable))
                return;
            _ShowView();
        }
        private void menuEdit_Click(object sender, EventArgs e) {
            _Edit();
        }
        private void menuHide_Click(object sender, EventArgs e) {
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return;
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbComposition.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
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
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _ShowView();
        }
        private void menuUpdate_Click(object sender, EventArgs e) {
            _ShowView();
        }
        private void _FormClose() {
            dataGridView.DataSource = null;
            dataGridFilter.DataGridView = null;
        }
        private void DlgComposition_FormClosing(object sender, FormClosingEventArgs e) {
            _FormClose();
        }
        private void dataGridView_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
    }
}
