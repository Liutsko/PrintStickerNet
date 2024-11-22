using System;
using System.Windows.Forms;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым


namespace PrintSticker
{
    public partial class DlgIzdelie : Form
    {
        private string _strIzdID = "";
        public DlgIzdelie()
        {
            InitializeComponent();
        }
        public string GetSelIzdID()
        {
            return _strIzdID;
        }
        private string _GetIzdID() { 
            if (rb00.Checked) return rb00.Text.Substring(0,2);
            if (rb01.Checked) return rb01.Text.Substring(0, 2);
            if (rb02.Checked) return rb02.Text.Substring(0, 2);
            if (rb03.Checked) return rb03.Text.Substring(0, 2);
            if (rb04.Checked) return rb04.Text.Substring(0, 2);
            if (rb06.Checked) return rb06.Text.Substring(0, 2);
            if (rb10.Checked) return rb10.Text.Substring(0, 2);
            if (rb11.Checked) return rb11.Text.Substring(0, 2);
            if (rb16.Checked) return rb16.Text.Substring(0, 2);
            if (rb36.Checked) return rb36.Text.Substring(0, 2);
            if (rb46.Checked) return rb46.Text.Substring(0, 2);
            if (rb56.Checked) return rb56.Text.Substring(0, 2);
            if (rb54.Checked) return rb54.Text.Substring(0, 2);
            if (rb67.Checked) return rb67.Text.Substring(0, 2);
            if (rb76.Checked) return rb76.Text.Substring(0, 2);
            if (rb77.Checked) return rb77.Text.Substring(0, 2);
            if (rb12.Checked) return rb12.Text.Substring(0, 2);
            if (rb55.Checked) return rb55.Text.Substring(0, 2);
            if (rb78.Checked) return rb78.Text.Substring(0, 2);

            if (rb05.Checked) return rb05.Text.Substring(0, 2);
            if (rb07.Checked) return rb07.Text.Substring(0, 2);
            if (rb21.Checked) return rb21.Text.Substring(0, 2);
            if (rb20.Checked) return rb20.Text.Substring(0, 2);
            if (rb22.Checked) return rb22.Text.Substring(0, 2);
            if (rb14.Checked) return rb14.Text.Substring(0, 2);
            if (rb24.Checked) return rb24.Text.Substring(0, 2);
            if (rb34.Checked) return rb34.Text.Substring(0, 2);
            if (rb25.Checked) return rb25.Text.Substring(0, 2);
            return "";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            _strIzdID = _GetIzdID();
        }

        private void DlgIzdelie_FormClosing(object sender, FormClosingEventArgs e)
        {
            _strIzdID = _GetIzdID();
        }
    }
}
