using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым


namespace PrintSticker {
    public partial class DlgShowZpl : Form {

        private readonly string _strSrcZplPath = "";
        private readonly Encoding _encodingZpl;
        private bool _btCancel = false;
        public DlgShowZpl(string strSrcZplPath, Encoding encodingZpl) {
            InitializeComponent();

            _encodingZpl = encodingZpl;
            if (File.Exists(strSrcZplPath))
                _strSrcZplPath = strSrcZplPath;
        }

        private void DlgShowZpl_Load(object sender, EventArgs e) {
            _Resize();
            try {
                Text = Path.GetFileName(_strSrcZplPath);
                richTextBox1.Text = File.ReadAllText(_strSrcZplPath, _encodingZpl);
                _ShowColors();
            } catch (Exception ex) {
                MessageBox.Show(this, "Ошибка " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void _ShowColors() { 
            for(int i = 0; i < richTextBox1.Text.Length - 3; i++) {
                richTextBox1.Select(i, 3);
                if ("^FO" == richTextBox1.SelectedText) {
                    richTextBox1.SelectionColor = System.Drawing.Color.Brown;
                    richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                    i += 3;
                    //X
                    for (int j = i; j < richTextBox1.Text.Length; j++) {
                        richTextBox1.Select(j, 1);
                        if ("," == richTextBox1.SelectedText) {
                            richTextBox1.Select(i, j-i);
                            richTextBox1.SelectionColor = System.Drawing.Color.Blue;
                            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                            i = j;
                            break;
                        }
                    }
                    //Y
                    int nStartY = i + 1;
                    for (int j = nStartY; j < richTextBox1.Text.Length; j++) {
                        richTextBox1.Select(j, 1);
                        if (!int.TryParse(richTextBox1.SelectedText, out _)) {
                            richTextBox1.Select(nStartY, j - nStartY);
                            richTextBox1.SelectionColor = System.Drawing.Color.Green;
                            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                            i = j;
                            break;
                        }
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

        private void DlgShowZpl_FormClosing(object sender, FormClosingEventArgs e) {
            if (_btCancel)
                return;

            try {
                string strFromFile = File.ReadAllText(_strSrcZplPath, _encodingZpl);
                string strRTB = richTextBox1.Text.ToString().Replace("\n", "\r\n");
                if (strFromFile == strRTB)
                    return;
                if (DialogResult.Yes != MessageBox.Show("Сохранить изменения?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    return;

            } catch (Exception ex) {
                MessageBox.Show(this, "Ошибка " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _Save();
        }
        private void _Save() {
            try {
                string strRTB = richTextBox1.Text.ToString().Replace("\n", "\r\n");
                GenLogic.CopyToArhive(_strSrcZplPath);
                File.WriteAllText(_strSrcZplPath, strRTB, _encodingZpl);
                MessageBox.Show("Стикер сохранен успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                MessageBox.Show(this, "Ошибка " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        private void btOK_Click(object sender, EventArgs e) {
            _Save();
            Close();
        }

        private void _ChangeTextWithColor(Color colorFind) {
            DlgInputNum dlg = new();
            dlg.SetText("Число");
            dlg.SetCaption("Введите число на которое нужно изменить");
            dlg.SetMinMax(-100, 100);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            int nMove = Convert.ToInt32(dlg.GetNumber().ToString());

            //richTextBox1.SelectedText.Replace()
            List<(int start, int length, string strAdd)> listRemove = [];

            for (int i = richTextBox1.Text.Length - 1; i >= 0; i--) {
                richTextBox1.Select(i, 1);
                if (colorFind == richTextBox1.SelectionColor) {
                    int nFinishBlue = i;
                    int nStartBlue = nFinishBlue + 100;
                    for (int j = i - 1; j >= 0; j--) {
                        richTextBox1.Select(j, 1);
                        if (colorFind != richTextBox1.SelectionColor) {
                            nStartBlue = j + 1;
                            break;
                        }
                    }
                    if (nStartBlue <= nFinishBlue) {
                        richTextBox1.Select(nStartBlue, nFinishBlue - nStartBlue + 1);
                        if (int.TryParse(richTextBox1.SelectedText.Trim(), out int nNumber)) {
                            nNumber += nMove;
                            if (nNumber < 0)
                                nNumber = 0;
                            listRemove.Add((richTextBox1.SelectionStart, richTextBox1.SelectionLength, nNumber.ToString()));
                        }
                        i = nStartBlue;
                    }

                }
            }
            foreach ((int start, int length, string strAdd) in listRemove) {
                richTextBox1.Text = richTextBox1.Text.Remove(start, length);
                richTextBox1.Text = richTextBox1.Text.Insert(start, strAdd);
            }
            _ShowColors();
        }
        private void menuBlueChange_Click(object sender, EventArgs e) {
            _ChangeTextWithColor(Color.Blue);
        }

        private void menuGreenChange_Click(object sender, EventArgs e) {
            _ChangeTextWithColor(Color.Green);
        }

        private void btCancel_Click(object sender, EventArgs e) {
            _btCancel = true;
        }

        private void menuUpdateColor_Click(object sender, EventArgs e) {
            _ShowColors();
        }
    }
}
