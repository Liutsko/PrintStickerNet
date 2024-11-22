using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;

namespace PrintSticker
{
    class CustomPrintPreviewDialog : System.Windows.Forms.PrintPreviewDialog
    {
        private int _nCopies = -1;
        private int _nType = -9999999;
        private bool _bPrinted = false;
        public CustomPrintPreviewDialog(int nCopies = 1, int nType = -9999999)
            : base()
        {
            _nCopies = nCopies;
            _nType = nType;
            if (this.Controls.ContainsKey("toolstrip1"))
            {
                ToolStrip tStrip1 = (ToolStrip)this.Controls["toolstrip1"];               

                ToolStripButton printButton = new ToolStripButton();
                printButton = (ToolStripButton)tStrip1.Items[0];

                tStrip1.Items.RemoveAt(0);
                ToolStripButton b = new ToolStripButton();
                b.ImageIndex = printButton.ImageIndex;
                b.Visible = true;
                tStrip1.Items.Insert(0, b);
                b.Click += new EventHandler(SaveDocument);

                //tStrip1.Items[0].Click += new EventHandler(SaveDocument);
            }
        }
        private void RemoveClickEvent(ToolStripButton b)
        {
            FieldInfo f1 = typeof(Control).GetField("EventClick",
                BindingFlags.Static | BindingFlags.NonPublic);

            object obj = f1.GetValue(b);
            PropertyInfo pi = b.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);

            EventHandlerList list = (EventHandlerList)pi.GetValue(b, null);
            list.RemoveHandler(obj, list[obj]);
        }


        public bool GetPrinted() { return _bPrinted; }

        protected void SaveDocument(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrinterSettings.Copies = (short)_nCopies;

            if (_nType != -1) // -1 it is PDO_MODE
            {
                if (DialogResult.OK != printDialog.ShowDialog())
                    return;
            }
            this.Document.PrinterSettings = printDialog.PrinterSettings;
            this.Document.Print();
            _bPrinted = true;
        }
    }
}
