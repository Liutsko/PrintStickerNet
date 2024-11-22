using System;
using System.Collections.Generic;
using System.Windows.Forms;
#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgSelSostav : Form {

        private string _strSostav = "";
        private readonly List<string> _listCompos;

        public DlgSelSostav(string strSostav, List<string> list) {
            _strSostav = strSostav;
            _listCompos = list;
            InitializeComponent();
        }
        
        private int _GetNumMaterial(string strMaterial) {
            //List<string> list = _GetListMaterials();
            for (int i = 0; i < _listCompos.Count; i++) {
                int nPos1 = _listCompos[i].LastIndexOf("(" + strMaterial + ")");
                if (-1 != nPos1)
                    return i;
            }
            return 0;
        }
        private void _InitItems() {
            string[] parms1 = _strSostav.Split('|');
            if (2 == parms1.Length) {
                string strMaterial = parms1[0];
                string strProcent = parms1[1];
                string[] parmsM = strMaterial.Split('+');
                string[] parmsP = strProcent.Split('-');
                if (parmsM.Length == parmsP.Length) {
                    if (1 == parmsM.Length) {
                        cb1.SelectedIndex = _GetNumMaterial(parmsM[0]);
                        txt1.Text = parmsP[0];
                    }
                    if (2 == parmsM.Length) {
                        cb1.SelectedIndex = _GetNumMaterial(parmsM[0]);
                        cb2.SelectedIndex = _GetNumMaterial(parmsM[1]);

                        txt1.Text = parmsP[0];
                        txt2.Text = parmsP[1];
                    }
                    if (3 == parmsM.Length) {
                        cb1.SelectedIndex = _GetNumMaterial(parmsM[0]);
                        cb2.SelectedIndex = _GetNumMaterial(parmsM[1]);
                        cb3.SelectedIndex = _GetNumMaterial(parmsM[2]);

                        txt1.Text = parmsP[0];
                        txt2.Text = parmsP[1];
                        txt3.Text = parmsP[2];
                    }
                }
            }
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;
        }
        private string _GetCodeMaterial(string strVal) {
            int nPos1 = strVal.IndexOf('(');
            int nPos2 = strVal.IndexOf(')');
            if (-1 == nPos1 || -1 == nPos2 || nPos1 >= nPos2)
                return "";
            return strVal.Substring(1 + nPos1, nPos2 - nPos1 - 1);
        }
        private int _GetProcent(string strVal) {
            if (!int.TryParse(strVal, out int nVal))
                return 0;
            if(nVal < 1 || nVal > 100)
                return 0;
            return nVal;
        }
        private void button1_Click(object sender, EventArgs e) {
            _strSostav = "";
            string strText1 = _GetCodeMaterial(cb1.Text);
            string strText2 = _GetCodeMaterial(cb2.Text);
            string strText3 = _GetCodeMaterial(cb3.Text);

            if ("" == strText1 && "" == strText2 && "" == strText3) {
                _strSostav = "";
                DialogResult = DialogResult.OK;
                Close();
                //MessageBox.Show(this, "Не выбран материал", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nPr1 = _GetProcent(txt1.Text);
            int nPr2 = _GetProcent(txt2.Text);
            int nPr3 = _GetProcent(txt3.Text);
            if(nPr1 + nPr2 + nPr3 != 100) {
                MessageBox.Show(this, "Состав не равен 100 %", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(strText1 != "" && 0 == nPr1 || strText1 == "" && 0 != nPr1) {
                MessageBox.Show(this, "Проверьте соответсвие материала и состава", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (strText2 != "" && 0 == nPr2 || strText2 == "" && 0 != nPr2) {
                MessageBox.Show(this, "Проверьте соответсвие материала и состава", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (strText3 != "" && 0 == nPr3 || strText3 == "" && 0 != nPr3) {
                MessageBox.Show(this, "Проверьте соответсвие материала и состава", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int nCount = 0;
            HashSet<string> hs = [];
            if (strText1 != "") { nCount++; hs.Add(strText1); }
            if (strText2 != "") { nCount++; hs.Add(strText2); }
            if (strText3 != "") { nCount++; hs.Add(strText3); }

            if (nCount != hs.Count) {
                MessageBox.Show(this, "Нельзя выбирать одинаковые материалы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (strText1 != "") _strSostav += strText1 + "+";
            if (strText2 != "") _strSostav += strText2 + "+";
            if (strText3 != "") _strSostav += strText3 + "+";
            _strSostav = _strSostav.Substring(0, _strSostav.Length - 1) + "|";

            if (0 != nPr1) _strSostav += nPr1.ToString() + "-";
            if (0 != nPr2) _strSostav += nPr2.ToString() + "-";
            if (0 != nPr3) _strSostav += nPr3.ToString() + "-";

            _strSostav = _strSostav.Substring(0, _strSostav.Length - 1);


            DialogResult = DialogResult.OK;
            Close();
        }
        public string GetSostav() {
            return _strSostav;
        }
        private void DlgSelSostav_Load(object sender, EventArgs e) {
            for (int i = 0; i < _listCompos.Count; i++) {
                cb1.Items.Add(_listCompos[i]);
                cb2.Items.Add(_listCompos[i]);
                cb3.Items.Add(_listCompos[i]);
            }
            _InitItems();
            _Resize();
        }
    }
}
