using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker {
    public partial class DlgImage : Form {
        private readonly int _nImage = 0;
        public DlgImage() {
            InitializeComponent();
        }
        public DlgImage(int nImage) {
            _nImage = nImage;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            Close();
        }

        private void DlgImage_Load(object sender, EventArgs e) {
            if (0 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.InfoOMSID;
            if (1 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.InfoOMSCONN;
            if (2 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.InfoSERT;
            if (3 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.Info1;
            if (4 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.Info2_8;
            if (5 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.Info12;
            if (6 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.Info9;
            if (7 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.Info10;
            if (8 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.Info11;
            if (9 == _nImage) pictureBox1.Image = PrintStickerNet.Properties.Resources.Info31;
        }
    }
}
