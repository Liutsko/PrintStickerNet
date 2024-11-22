using DbfLib;
using GridViewExtensions.GridFilters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым


namespace PrintSticker {
    public partial class DlgSelColor : Form {
        private int _nColor;
        private string _strColor;
        private readonly List<string> _listColors;
        public DlgSelColor(int nColor, List<string> list) {
            _nColor = nColor;
            _listColors = list;
            InitializeComponent();
        }        
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        
        private int _GetNumColor(int nColor) {
            //List<string> list = _GetListColor();
            for (int i = 0; i < _listColors.Count; i++) {
                int nPos1 = _listColors[i].LastIndexOf("(" + nColor.ToString() + ")");
                if (-1 != nPos1)
                    return i;
            }
            return 0;
        }     

        public int GetColorNum() {
            return _nColor;
        }
        public string GetColor() {
            return _strColor;
        }
        
        private int _GetCodeColor(string strVal) {
            int nPos1 = strVal.IndexOf('(');
            int nPos2 = strVal.IndexOf(')');
            if (-1 == nPos1 || -1 == nPos2 || nPos1 >= nPos2)
                return -1;
            return Convert.ToInt32(strVal.Substring(1 + nPos1, nPos2 - nPos1 - 1));
        }
        private void btOK_Click(object sender, EventArgs e) {
            _nColor = _GetCodeColor(cb1.Text);
            _strColor = cb1.Text;
        }
       
        private void DlgSelColor_Load(object sender, EventArgs e) {
            _listColors.Sort();
            for (int i = 0; i < _listColors.Count; i++)
                cb1.Items.Add(_listColors[i]);

            cb1.SelectedIndex = _GetNumColor(_nColor);
            _Resize();
        }
    }
}
