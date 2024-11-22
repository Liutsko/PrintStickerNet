using DbfLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgTnved : Form {
        private readonly _Form1 _parent;
        public DlgTnved(_Form1 parent) {
            InitializeComponent();
            _parent = parent;
        }
        private void DlgTnved_Load(object sender, EventArgs e) {
            _ShowView();
            _Resize();
        }
        private void _Resize() {
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }

        private void _ShowView() {
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbTnved.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
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

            dataGridView.Columns[2].Width = 140;//
            dataGridView.Columns[3].Width = 130;//
            dataGridView.Columns[4].Width = 40;//
            dataGridView.Columns[5].Width = 390;//
            dataGridView.Columns[6].Width = 550;//

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
            hUseColumns.Add("TNVED", "ТНВЭД");//3
            hUseColumns.Add("SEX", "Пол");//4          
            hUseColumns.Add("COMPOS", "Состав");//5
            hUseColumns.Add("DESCR", "Комментарий");//6          
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
        private void menuUpdate_Click(object sender, EventArgs e) {
            _ShowView();
        }
        private bool _Hide(string strFileName) {
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return false;
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, "\\ooo\\"))
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
            if (!_Hide("DbTnved.dbf"))
                return;
            _ShowView();
        }

        protected bool _Edit(ref DataTable dt) {
            if (null == dt || 1 != dt.Rows.Count)
                return false;
            DlgAddTnved dlg = new();
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
            if (!DbfWrapper.CheckIfFileExist("DbTnved.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
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
                MessageBox.Show(_parent, "Ошибка сохранения в файл " + strSrcPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _ShowView();
        }
        private void menuEdit_Click(object sender, EventArgs e) {
            _Edit();
        }

        private void dataGridView_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
        private void menuAdd_Click(object sender, EventArgs e) {
            DlgAddTnved dlg = new();
            if (DialogResult.OK != dlg.ShowDialog())
                return;
            var (product, tnved, sex, compos, descr) = dlg.GetSelItems();
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbTnved.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "№", "1");
            dtTable.Rows.Clear();

            int PRODUCT = 2;
            int TNVED = 3;
            int SEX = 4;
            int COMPOS = 5;
            int DESCR = 6;
            DataRow rowAdd = dtTable.NewRow();
            rowAdd[PRODUCT] = product;
            rowAdd[TNVED] = tnved;
            rowAdd[SEX] = sex;
            rowAdd[COMPOS] = compos;
            rowAdd[DESCR] = descr;

            dtTable.Rows.Add(rowAdd);
            if (!DbfWrapper.AddDoDB(null, strDestPath, strSrcPath, dtTable))
                return;

            _ShowView();
        }

        private void menuCopyAndAddEnd_Click(object sender, EventArgs e) {
            if (0 == dataGridView.SelectedRows.Count)
                return;

            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbTnved.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "№", "1");
            dtTable.Rows.Clear();

            int PRODUCT = 2;
            int TNVED = 3;
            int SEX = 4;
            int COMPOS = 5;
            int DESCR = 6;
            for (int i = dataGridView.SelectedRows.Count - 1; i >= 0; i--) {
                DataGridViewRow row = dataGridView.SelectedRows[i];
                DataRow rowAdd = dtTable.NewRow();
                rowAdd[PRODUCT] = row.Cells[PRODUCT].Value.ToString();
                rowAdd[TNVED] = row.Cells[TNVED].Value.ToString();
                rowAdd[SEX] = row.Cells[SEX].Value.ToString();
                rowAdd[COMPOS] = row.Cells[COMPOS].Value.ToString();
                rowAdd[DESCR] = row.Cells[DESCR].Value.ToString();

                dtTable.Rows.Add(rowAdd);
            }
            if (!DbfWrapper.AddDoDB(null, strDestPath, strSrcPath, dtTable))
                return;
            _ShowView();
        }

        private void btCancel_Click(object sender, EventArgs e) {

        }
    }
}
