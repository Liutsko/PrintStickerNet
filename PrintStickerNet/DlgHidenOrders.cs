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
    public partial class DlgHidenOrders : Form {

        private string _strCaption = "";
        public DlgHidenOrders() {
            InitializeComponent();
        }
        private void btCancel_Click(object sender, EventArgs e) {
            Close();
        }
        private void btOK_Click(object sender, EventArgs e) {
            Close();
        }
        public void SetCaption(string strCaption) {
            _strCaption = strCaption;
        }
        public void Add(List<string> list) {
            foreach (string strItem in list)
                checkedListBox1.Items.Add(strItem.Trim().Replace("\r\n", ","));                     
        }

        public void Sel(List<string> list) {
            for (int i = 0; i < checkedListBox1.Items.Count; i++) {
                if (list.Contains(checkedListBox1.Items[i]))
                    checkedListBox1.SetItemChecked(i, true);               
            }
        }
        

        public List<string> GetSelItems() {
            return checkedListBox1.CheckedItems.OfType<string>().ToList();
        }
        private void DlgHidenOrders_Load(object sender, EventArgs e) {
            if(0 == checkedListBox1.Items.Count)
                btOK.Enabled = false;

            if ("" != _strCaption) {
                Text = _strCaption;
                btOK.Text = "OK";
            }
            _Resize();
        }
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e) {            
            toolTip1.SetToolTip(checkedListBox1, checkedListBox1.SelectedItem.ToString());
        }
        private void _Resize() {
            btOK.Top = this.ClientSize.Height - btOK.Height - 9;
            btCancel.Top = this.ClientSize.Height - btCancel.Height - 9;

            btOK.Left = this.ClientSize.Width - 2 * btOK.Width - 8;
            btCancel.Left = this.ClientSize.Width - btCancel.Width - 4;

            checkedListBox1.IntegralHeight = false;
            checkedListBox1.Width = this.ClientSize.Width - 4;
            checkedListBox1.Height = this.ClientSize.Height - 2 * btOK.Height + 5;           
        }
        private void DlgHidenOrders_Resize(object sender, EventArgs e) {
            _Resize();
        }
        private void DlgHidenOrders_ResizeEnd(object sender, EventArgs e) {
            _Resize();
        }
    }
}
