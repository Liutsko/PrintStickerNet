using System;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker
{
    public partial class DlgInputBox : Form {
        private int _nNumber = -1;
        private string _strValue = "";
        private readonly bool _bInt = true;
        private string _strCaption = "";
        private string _strFildName = "";
        public DlgInputBox(bool bInt = true) {
            InitializeComponent();
            _bInt = bInt;
        }
        public int GetNumber() {
            return _nNumber;
        }
        public string GetString() {
            return _strValue;
        }
        public void SetString(string strValue) {
            _strValue = strValue;
        }



        public void SetCaption(string strText) {
            _strCaption = strText;
        }
        public void SetFildName(string strText) {
            _strFildName = strText;
        }

        private static bool _IsInt(string strValur) {
            try {
                int tt = Convert.ToInt32(strValur);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }
        private void _OK() {
            if (_bInt) {
                if (!_IsInt(txtNumber.Text)) {
                    MessageBox.Show("Введите число в интервала от 1 до 99999 в поле: " + label1.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int nNumber = Convert.ToInt32(txtNumber.Text);
                if (nNumber < 1 || nNumber > 99999) {
                    MessageBox.Show("Введите число в интервала от 1 до 99999 в поле: " + label1.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _nNumber = nNumber;
            } else
                _strValue = txtNumber.Text;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
        private void btOK_Click(object sender, EventArgs e) {
            _OK();
        }

        private void DlgInputBox_Load(object sender, EventArgs e) {
            if ("" != _strCaption) Text = _strCaption;
            if ("" != _strFildName) label1.Text = _strFildName;
            txtNumber.Text = _strValue;

            timerFocus.Enabled = true;
            timerFocus.Interval = 100;
            timerFocus.Start();            
        }

        private void timerFocus_Tick(object sender, EventArgs e) {
            timerFocus.Stop();
            timerFocus.Enabled = false;
            txtNumber.Focus();
        }

        private void txtNumber_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            _OK();
        }
    }
}
