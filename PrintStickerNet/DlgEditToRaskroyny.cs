using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgEditToRaskroyny : Form {
        private DataGridView _dgb = null;
        DataTable _dTable = null;
        DataTable _dtTableTM = null;
        DataTable _dtTableCloth = null;
        DataTable _dtTableSeason = null;
        DataTable _dtTableOther = null;

        readonly int CJ = 3;
        readonly int CCLOTH = 6;
        readonly int COTHER = 7;
        readonly int CSEASON = 8;
        readonly int BRAKET = 16;
        private readonly Dictionary<int, int> _hNumTable = [];

        public DlgEditToRaskroyny() {
            InitializeComponent();
        }
        public void SetTableTM(DataTable dtTable) {
            _dtTableTM = dtTable;
        }
        public void SetTableCloth(DataTable dtTable) {
            _dtTableCloth = dtTable;
        }

        public void SetTableSeason(DataTable dtTable) {
            _dtTableSeason = dtTable;
        }
        public void SetTableOther(DataTable dtTable) {
            _dtTableOther = dtTable;
        }

        public void SetEditDGV(ref DataTable dTable, ref DataGridView dgb) {
            _dgb = dgb;
            _dTable = dTable;

            int i = -1;
            int N = 0;
            foreach (DataRow row in _dTable.Rows) {
                i++;
                _hNumTable.Add((int)row[N], i);
            }
        }
        private void _InitSeason() {
            string strSeason = "";
            foreach (DataGridViewRow row in _dgb.SelectedRows) {
                strSeason = row.Cells[CSEASON].Value.ToString();
                break;
            }
            List<string> list = [];
            list.Add("");
            int NSEASON = 2;
            int CSEASON_2 = 3;
            for (int i = 0; i < _dtTableSeason.Rows.Count; i++) {
                string strName = _dtTableSeason.Rows[i][NSEASON].ToString();
                string strCode = _dtTableSeason.Rows[i][CSEASON_2].ToString();
                list.Add($"{strName}({strCode})");
            }
            list.Sort();

            int nSelPos = 0;
            int nI = -1;
            foreach (string strItem in list) {
                nI++;
                cbSeason.Items.Add(strItem);
                if (-1 != strItem.LastIndexOf($"({strSeason})"))
                    nSelPos = nI;
            }
            if (list.Count > 0)
                cbSeason.SelectedIndex = nSelPos;
        }
        private void _InitOther() {
            string strOther = "";
            foreach (DataGridViewRow row in _dgb.SelectedRows) {
                strOther = row.Cells[COTHER].Value.ToString();
                break;
            }
            List<string> list = [];
            list.Add("");
            int NOTHER = 2;
            int COTHER_2 = 3;
            for (int i = 0; i < _dtTableOther.Rows.Count; i++) {
                string strName = _dtTableOther.Rows[i][NOTHER].ToString();
                string strCode = _dtTableOther.Rows[i][COTHER_2].ToString();
                list.Add($"{strName}({strCode})");
            }
            list.Sort();

            int nSelPos = 0;
            int nI = -1;
            foreach (string strItem in list) {
                nI++;
                cbOther.Items.Add(strItem);
                if (-1 != strItem.LastIndexOf($"({strOther})"))
                    nSelPos = nI;
            }
            if (list.Count > 0)
                cbOther.SelectedIndex = nSelPos;
        }
        private void _InitCloth() {
            string strCloth = "";
            foreach (DataGridViewRow row in _dgb.SelectedRows) {
                strCloth = row.Cells[CCLOTH].Value.ToString();
                break;
            }
            List<string> list = [];
            list.Add("");
            int NCLOTH = 2;
            int CCLOTH_2 = 3;
            for (int i = 0; i < _dtTableCloth.Rows.Count; i++) {
                string strName = _dtTableCloth.Rows[i][NCLOTH].ToString();
                string strCode = _dtTableCloth.Rows[i][CCLOTH_2].ToString();
                list.Add($"{strName}({strCode})");
            }
            list.Sort();

            int nSelPos = 0;
            int nI = -1;
            foreach (string strItem in list) {
                nI++;
                cbCLOTH.Items.Add(strItem);
                if (-1 != strItem.LastIndexOf($"({strCloth})"))
                    nSelPos = nI;
            }
            if (list.Count > 0)
                cbCLOTH.SelectedIndex = nSelPos;
        }
        private void _InitTM() {
            string strTM = "";
            foreach (DataGridViewRow row in _dgb.SelectedRows) {
                strTM = row.Cells[BRAKET].Value.ToString();
                break;
            }
            List<string> list = [];
            list.Add("");
            int TM = 2;
            int NAME = 3;
            for (int i = 0; i < _dtTableTM.Rows.Count; i++) {
                string strName = _dtTableTM.Rows[i][NAME].ToString();
                string strTM2 = _dtTableTM.Rows[i][TM].ToString();
                list.Add($"{strName}({strTM2})");
            }
            list.Sort();

            int nSelPos = 0;
            int nI = -1;
            foreach (string strItem in list) {
                nI++;
                cbTM.Items.Add(strItem);
                if (-1 != strItem.LastIndexOf($"({strTM})"))
                    nSelPos = nI;
            }
            if (list.Count > 0)
                cbTM.SelectedIndex = nSelPos;
        }
        private void DlgEditToRaskroyny_Load(object sender, EventArgs e) {
            foreach (DataGridViewRow row in _dgb.SelectedRows) {
                txtCost.Text = row.Cells[CJ].Value.ToString();
                break;
            }

            _InitTM();
            _InitCloth();
            _InitSeason();
            _InitOther();
            _Resize();
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btOK.Left = this.ClientSize.Width - btOK.Width - 4;
        }
        private string _GetCodeSeason() {
            string strCode = "";
            int nPos1 = cbSeason.Text.LastIndexOf("(");
            int nPos2 = cbSeason.Text.LastIndexOf(")");
            if (nPos2 > nPos1 && nPos1 != -1)
                strCode = cbSeason.Text.Substring(nPos1 + 1, nPos2 - nPos1 - 1);
            return strCode;
        }
        private string _GetCodeCother() {
            string strCode = "";
            int nPos1 = cbOther.Text.LastIndexOf("(");
            int nPos2 = cbOther.Text.LastIndexOf(")");
            if (nPos2 > nPos1 && nPos1 != -1)
                strCode = cbOther.Text.Substring(nPos1 + 1, nPos2 - nPos1 - 1);
            return strCode;
        }

        private string _GetCodeCloth() {
            string strCode = "";
            int nPos1 = cbCLOTH.Text.LastIndexOf("(");
            int nPos2 = cbCLOTH.Text.LastIndexOf(")");
            if (nPos2 > nPos1 && nPos1 != -1)
                strCode = cbCLOTH.Text.Substring(nPos1 + 1, nPos2 - nPos1 - 1);
            return strCode;
        }

        private string _GetCodeTM() {
            string strCode = "";
            int nPos1 = cbTM.Text.LastIndexOf("(");
            int nPos2 = cbTM.Text.LastIndexOf(")");
            if (nPos2 > nPos1 && nPos1 != -1)
                strCode = cbTM.Text.Substring(nPos1 + 1, nPos2 - nPos1 - 1);
            return strCode;
        }
        private void btOK_Click(object sender, EventArgs e) {            
            int N = 0;
            
            foreach (DataGridViewRow row in _dgb.SelectedRows) {
                row.Cells[CJ].Value = txtCost.Text;
                row.Cells[BRAKET].Value = _GetCodeTM();
                row.Cells[CCLOTH].Value = _GetCodeCloth();
                row.Cells[CSEASON].Value = _GetCodeSeason();
                row.Cells[COTHER].Value = _GetCodeCother();

                int nSrc = (int)row.Cells[N].Value;
                _dTable.Rows[_hNumTable[nSrc]][CJ] = row.Cells[CJ].Value;
                _dTable.Rows[_hNumTable[nSrc]][BRAKET] = row.Cells[BRAKET].Value;
                _dTable.Rows[_hNumTable[nSrc]][CCLOTH] = row.Cells[CCLOTH].Value;
                _dTable.Rows[_hNumTable[nSrc]][CSEASON] = row.Cells[CSEASON].Value;
                _dTable.Rows[_hNumTable[nSrc]][COTHER] = row.Cells[COTHER].Value;
            }
        }

        private void label4_Click(object sender, EventArgs e) {

        }

        private void cbSeason_SelectedIndexChanged(object sender, EventArgs e) {

        }
    }
}
