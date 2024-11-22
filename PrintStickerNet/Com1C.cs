using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using V83;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace PrintSticker {
    public class Com1C {
        public static event Action<string> OnConnect;
        //public static event Action<string> Progress;
        public static event Action<string> OnError;

        public static bool Connecting { get; private set; }
        public static bool IsConnected { get; private set; }

        private static string _connectionString;
        private static string _connectedStr = "";
        private static COMConnector _con;
        private static dynamic _basa1C;
        private static string _error = "";

        public static string LastError {
            get { return _error; }
            set { _error = value; if (_error.Length > 0) { OnError?.Invoke(_error); } }
        }
        public static void ConnectTo1C(string connectionString) {
            _connectionString = connectionString;
            Connect();
        }       
        public static bool CreateCopyNomenkl(string strKodCopy, string strName, string strEAN13) {
            if (!IsConnected) {
                MessageBox.Show(null, $"Отсутствует подключение к 1С  {_connectionString}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {

                //ищем существует ли штрихкод
                dynamic zapros = _basa1C.NewObject("Запрос"); 
                zapros.Текст = @"ВЫБРАТЬ
                                    ШтрихкодыНоменклатуры.Номенклатура КАК Ном
                                    ИЗ
                                    РегистрСведений.ШтрихкодыНоменклатуры КАК ШтрихкодыНоменклатуры
                                    ГДЕ
                                    ШтрихкодыНоменклатуры.Штрихкод = &ШтрихКод";
                zapros.УстановитьПараметр("ШтрихКод", strEAN13);
                dynamic rez = zapros.Выполнить();

                if (!(bool)rez.Пустой()) {
                    dynamic viborka = rez.Выбрать();
                    int nCountNomenkl = viborka.Количество(); //скоре всего всегда будет 1 так как нельзя один штрихкод назначить разным номенклатурам
                    string srtNomName = "";
                    for (int i = 0; i < nCountNomenkl; i++) {
                        viborka.Следующий();
                        srtNomName = (string)viborka.Ном.Наименование;
                        if (srtNomName == strName)
                            return true;
                    }
                    MessageBox.Show(null, $"Штрихкод  {strEAN13} присутствует в другой номернкатуре: {srtNomName}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                //ищем номенклатуру с котрой делаем копию
                dynamic nomForCopy = _basa1C.Справочники.Номенклатура.НайтиПоКоду(strKodCopy);
                if ((bool)nomForCopy.Пустая()) {
                    MessageBox.Show(null, $"Отсутствует номенклатура с которой делаем копию, с кодом {strKodCopy}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                //делаем копию номенклатуры, или берем существующую
                dynamic nomenkl;
                dynamic nomExist = _basa1C.Справочники.Номенклатура.НайтиПоНаименованию(strName);            
                if ((bool)nomExist.Пустая()) {                
                    nomenkl = nomForCopy.Скопировать();
                    nomenkl.Наименование = strName;
                    nomenkl.НаименованиеПолное = nomenkl.Наименование;
                    nomenkl.Записать();
                }
                else
                    nomenkl = nomExist;
                dynamic refNomenkl = nomenkl.Ссылка;

                //добавляем штрихкорд с сылкой на номенклатуру           
                dynamic managerZap = _basa1C.РегистрыСведений.ШтрихкодыНоменклатуры.СоздатьМенеджерЗаписи();
                managerZap.Номенклатура = refNomenkl;
                managerZap.Штрихкод = strEAN13;
                managerZap.Записать();
            } catch (Exception ex) {
                MessageBox.Show(null, $"Ошибка:  {ex.Message}. Убедитесь что есть доступ к 1С и повторите экспорт в 1С ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private static void Connect() {
            if (IsConnected && _connectedStr == _connectionString) {
                return;
            }
            try {
                Connecting = true;
                IsConnected = false;
                _con = new COMConnector();

                _basa1C = _con.Connect(_connectionString);                
                IsConnected = true;
                _connectedStr = _connectionString;
                OnConnect?.Invoke("Соединение с сервером 1С установлено.");
            } catch (Exception ex) {
                IsConnected = false;
                OnConnect?.Invoke("Ошибка соединения с сервером 1С.");
                LastError = ex.Message;
            } finally {
                Connecting = false;
            }
        }
    }
}
