using DbfLib;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgToRaskroyny : Form {
        DataTable _dTable = null;
        DataTable _dtTableTM = null;
        DataTable _dtTableCloth = null;
        DataTable _dtTableSeason = null;
        DataTable _dtTableOther = null;

        public DlgToRaskroyny(ref DataTable dTable) {
            InitializeComponent();
            _dTable = dTable;
        }
        private void _GetTableTM() {
            string strFileName = "TRADEMAR.DBF";
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, @"\Bv.2\Mail\Ref\")) {
                MessageBox.Show(null, "отсутстсвует файл: " + strSrcPath + @"\" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nCodeOut = -1;
            _dtTableTM = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");
        }
        private void _GetTableCLOTH() {
            string strFileName = "CLOTH.DBF";
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, @"\Bv.2\Mail\Ref\")) {
                MessageBox.Show(null, "отсутстсвует файл: " + strSrcPath + @"\" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nCodeOut = -1;
            _dtTableCloth = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");
        }

        private void _GetTableSeason() {
            string strFileName = "SEASON.DBF";
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, @"\Bv.2\Mail\Ref\")) {
                MessageBox.Show(null, "отсутстсвует файл: " + strSrcPath + @"\" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nCodeOut = -1;
            _dtTableSeason = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");
        }

        private void _GetTableCother() {
            string strFileName = "OTHER.DBF";
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(strFileName, ref strDestPath, ref strSrcPath, @"\Bv.2\Mail\Ref\")) {
                MessageBox.Show(null, "отсутстсвует файл: " + strSrcPath + @"\" + strFileName, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nCodeOut = -1;
            _dtTableOther = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");
        }


        private void DlgToRaskroyny_Load(object sender, EventArgs e) {
            _Resize();
            _ShowView();

            _GetTableTM();
            _GetTableCLOTH();
            _GetTableSeason();
            _GetTableCother();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        private void _InidataGridView(ref DataGridView dataGridView) {
            dataGridView.Columns[0].Visible = false;//N
            dataGridView.Columns[1].Visible = false;//DEL

            dataGridView.Columns[4].Visible = false;//PATTERN
            dataGridView.Columns[5].Visible = false;//CCODE

            dataGridView.Columns[2].Visible = false;//NLIST
            dataGridView.Columns[14].Visible = false; //ART2
            dataGridView.Columns[15].Visible = false; //LABEL

            dataGridView.Columns[17].Visible = false; //FLOOR
            dataGridView.Columns[18].Visible = false; //NOTE
            dataGridView.Columns[19].Visible = false; //VC
            
            DataGridViewCellStyle style = new() {
                Font = new Font(dataGridView.Font, FontStyle.Bold)
            };
            dataGridView.DefaultCellStyle = style;
        }
        private void _ShowView() {
            //string strDestPath = "";
            //string strSrcPath = "";
            //if (!DbfWrapper.CheckIfFileExist("DbTnved.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
            //    return;
            //_GetUseColumn(out Dictionary<string, string> hUseColumns);

            //int nCodeOut = -1;
            //DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0", hUseColumns);

            DataView dv = _dTable.DefaultView;
            dataGridView.DataSource = dv.ToTable();
            dataGridFilter.DataGridView = dataGridView;
            _InidataGridView(ref dataGridView);
        }
        private void _Edit() {
            //if (1 != dataGridView.SelectedRows.Count)
            //    return;
            DlgEditToRaskroyny dlg = new();
            dlg.SetEditDGV(ref _dTable, ref dataGridView);
            dlg.SetTableTM(_dtTableTM);
            dlg.SetTableCloth(_dtTableCloth);
            dlg.SetTableSeason(_dtTableSeason);
            dlg.SetTableOther(_dtTableOther);         
            if (DialogResult.OK != dlg.ShowDialog(this))
                return;
        }

        private void menuEdit_Click(object sender, EventArgs e) {
            _Edit();
        }

        private void dataGridView_DoubleClick(object sender, EventArgs e) {
            _Edit();
        }
        private void contextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            if (0 == dataGridView.SelectedRows.Count) {
                menuEdit.Enabled = false;
            } else {
                menuEdit.Enabled = true;
            }
        }
    }
}
