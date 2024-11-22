using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым
namespace PrintSticker {
    public partial class DlgInputNum : Form {
        private long _nNumber = -1;

        private long _nMin = 1;
        private long _nMax = 99999999;
        private string _strCaption = "";
        private string _strText = "";

        public DlgInputNum() {
            InitializeComponent();
        }
        public void SetMinMax(long nMin, long nMAx) {
            _nMin = nMin;
            _nMax = nMAx;
        }
        public void SetText(string strText) {
            _strText = strText;
        }

        public void SetCaption(string strText) {
            _strCaption = strText;
        }      
        public long GetNumber() {
            return _nNumber;
        }
        private static bool _IsLong(string strValur) {
            try {
                long tt = Convert.ToInt64(strValur);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }
        private void _OK() {
            if (!_IsLong(txtNumber.Text)) {
                MessageBox.Show($"Введите число в интервала от {_nMin} до {_nMax} в поле: " + label1.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            long nNumber = Convert.ToInt64(txtNumber.Text);
            if (nNumber < _nMin || nNumber > _nMax) {
                MessageBox.Show($"Введите число в интервала от {_nMin} до {_nMax} в поле: " + label1.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _nNumber = nNumber;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
        private void btOK_Click(object sender, EventArgs e) {
            _OK();
        }

        private void DlgInputNum_Load(object sender, EventArgs e) {
            if ("" != _strCaption) Text = _strCaption;
            if ("" != _strText) label1.Text = _strText;
           
            timerFocus.Enabled = true;
            timerFocus.Interval = 200;
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
