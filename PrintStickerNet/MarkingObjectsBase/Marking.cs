using DbfLib;
using System.Data;
using System.Diagnostics;
using System.Text;
using FairMarkLib;
using System.Runtime.InteropServices;
using PrintSticker.MarkingObjectsBase;
using System.ComponentModel;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым
#pragma warning disable CA2249 //Используйте "string.Contains" вместо "string.IndexOf"


namespace PrintSticker {
    public class COLUMNS_PRODUCTS {
        public int BARCODE = 0;// = 3;
        public int KOL = 0;// = 11;
        public int GTIN = 0;// = 27;
        public int KOL_KM = 0;// = 28;
        public int IZDNAME = 0;// = 7;
    }

    public class Marking() {
        protected static readonly Composition _composition = new();

        protected Dictionary<int, int> _hNumTable = [];
        protected DataGridView _dataGridView = null;
        protected MarkingPaths _markingPaths = null;
        protected MarkingOrders _markingOrders = null;

        protected string _strPrefixEan13 = "";
        protected string _strSettingsID = "";
        protected MARKINGTYPES _MARKINGTYPES;

        protected DateTime _dtOpenedFile = new(1929, 1, 1);
        protected DateTime _dtGtinOrdersMkLoaded = new(1929, 1, 1);

        protected bool _IsWool(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("шерсть") || -1 != strSostav.LastIndexOf("мохер"))
                return true;
            return false;
        }
        protected bool _IsLen(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("лен"))
                return true;
            return false;
        }
        protected bool _IsShelk(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("шелк"))
                return true;
            return false;
        }
        protected bool _IsHlopok(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("хлопок"))
                return true;
            return false;
        }
        public static bool _IsSintetik(string strSostav) {
            if (-1 != strSostav.ToLower().LastIndexOf("полиэстер") || -1 != strSostav.ToLower().LastIndexOf("п э") || -1 != strSostav.LastIndexOf("вискоза"))
                return true;
            if (-1 != strSostav.ToLower().LastIndexOf("пан"))
                return true;
            return false;
        }
        protected static string _GetTextile(string strValue) {
            string[] parms = strValue.Split('+');
            string strOut = "";
            for (int i = 0; i < parms.Length; i++) {
                if ("" == strOut)
                    strOut = _composition.GetMaterial(parms[i]);
                else
                    strOut = strOut + "+" + _composition.GetMaterial(parms[i]);
            }
            return strOut;
        }
        
        protected string _GetTypeProduct(string strTypeProduct) {

            int nCodeOut = -1;
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_GetProductFileName(), ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return "";
            int NAME = 2;
            int NAMEFULL = 3;
            strTypeProduct = strTypeProduct.ToUpper();
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTable.Rows.Count; i++) {
                if (strTypeProduct == dtTable.Rows[i][NAME].ToString().ToUpper())
                    return dtTable.Rows[i][NAMEFULL].ToString();
            }
            MessageBox.Show($"Ошибка, для продукта: {strTypeProduct} не найдено в справочнике полное название (вид изделия)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return strTypeProduct;
        }
        protected string _GetSostav(string strSostav) {
            if ("|" == strSostav)
                return "";
            string[] a2Line = strSostav.Split('|');
            if (2 != a2Line.Length)
                return strSostav;
            string strPrefix = a2Line[0];
            if ("_" == strPrefix.Substring(strPrefix.Length - 1, 1))
                strPrefix = strPrefix.Substring(0, strPrefix.Length - 1);

            string strPostfix = a2Line[1];
            string[] aPostfix = strPostfix.Split('-');

            string[] aPrefix = strPrefix.Split('_');
            for (int i = 0; i < aPrefix.Length; i++)
                aPrefix[i] = _GetTextile(aPrefix[i]);

            if (aPrefix.Length > aPostfix.Length) {
                MessageBox.Show(null, "Ошибка входных данных:" + strSostav, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return strSostav;
            }
            string strOut = "";
            for (int i = 0; i < aPrefix.Length; i++) {
                if ("" == strOut)
                    strOut = aPrefix[i] + " " + aPostfix[i] + "%";
                else
                    strOut = strOut + "," + aPrefix[i] + " " + aPostfix[i] + "%";

                if (i == aPrefix.Length - 1) {
                    for (int j = aPrefix.Length; j < aPostfix.Length; j++)
                        strOut = strOut + " " + aPostfix[j] + "%";
                }
            }
            return strOut;// +"       #" + strSostav;
        }
        protected virtual string _GetCountryFileName() {
            MessageBox.Show("Ошибка, метод _GetCountryFileName не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "";
        }
        protected virtual string _GetProductFileName() {
            MessageBox.Show("Ошибка, метод _GetProductFileName не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "";
        }

        protected string _GetCountry(string strCountry) {
            int nCodeOut = -1;
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist(_GetCountryFileName(), ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return "";
            int NAME = 2;
            int NAMEFULL = 3;
            strCountry = strCountry.ToUpper();
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTable.Rows.Count; i++) {
                if (strCountry == dtTable.Rows[i][NAME].ToString().ToUpper())
                    return dtTable.Rows[i][NAMEFULL].ToString();
            }
            MessageBox.Show($"Ошибка, для страны: {strCountry} не найдено в справочнике полное название", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return strCountry;                       
        }

        public string GetTnvedFromBD(string strTypeProduct, string strSostav, string strMod) {
            string strSex = "М";
            if(-1 != strMod.LastIndexOf("ЖЕН") || -1 != strTypeProduct.LastIndexOf("ЖЕН"))
                strSex = "Ж";
            int nCodeOut = -1;

            string[] parms1 = strTypeProduct.Split(' ');
            strTypeProduct = parms1[0];

            Composition compos = new();
            Dictionary<string, string> dicShortFullCompos = compos.GetDicMaterials();
            string strDestPath = "";
            string strSrcPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbTnved.dbf", ref strDestPath, ref strSrcPath, "\\ooo\\"))
                return "";

            int TNVED = 3;
            int SEX = 4;
            int COMPOS = 5;
            string strTNVED = "";
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "PRODUCT", strTypeProduct.ToUpper(), null, false, false);
            for (int i = 0; i < dtTable.Rows.Count; i++) {
                if (strSex != dtTable.Rows[i][SEX].ToString())
                    continue;

                string strCompos = dtTable.Rows[i][COMPOS].ToString();
                if("" == strTNVED && "" == strCompos)
                    strTNVED = dtTable.Rows[i][TNVED].ToString();

                if ("" != strCompos) {
                    string[] parms = strCompos.Split(',');
                    foreach (string strShort in parms) {
                        if (dicShortFullCompos.ContainsKey(strShort)) {
                            string strFullName = dicShortFullCompos[strShort].ToLower();
                            if(-1 != strSostav.ToLower().LastIndexOf(strFullName))
                                return dtTable.Rows[i][TNVED].ToString();
                        }
                    }
               }
            }
            return strTNVED;
        }
        protected string _GetTNVED(string strTypeProduct, string strSostav, string strMod) {
            string strTnved = GetTnvedFromBD(strTypeProduct, strSostav, strMod);
            if ("" != strTnved)
                return strTnved;
            MessageBox.Show(null, $"В справочнике ТНВЕД нет данных для {strTypeProduct} , {strSostav} , {strMod}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);            

            if (-1 != strTypeProduct.IndexOf("СОРОЧКА")) {
                if (_IsWool(strSostav)) return "6205908001"; //<6205908001> Рубашки мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6205200000";//<6205200000> Рубашки мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6205300000";//<6205300000> Рубашки мужские или для мальчиков, из химических нитей
                if (_IsLen(strSostav)) return "6205901000";//<6205901000> Рубашки мужские или для мальчиков, из льняных волокон или волокна рами
                return "6205908009";//<6205908009> Рубашки мужские или для мальчиков, из прочих текстильных материалов              
            }


            bool bBrukiWomen = false;
            if (-1 != strTypeProduct.IndexOf("БРЮКИ") && -1 != strTypeProduct.IndexOf("ЖЕН"))
                bBrukiWomen = true;

            if (-1 != strTypeProduct.IndexOf("БРЮКИ ЖЕН") || bBrukiWomen) {
                //<6204691800> Прочие брюки и бриджи женские или для девочек, из искусственных нитей
                if (_IsWool(strSostav)) return "6204611000"; //<6204611000> Брюки и бриджи женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6204623900";//<6204623900> Прочие брюки и бриджи женские или для девочек, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6204631800";//<6204631800> Прочие брюки и бриджи женские или для девочек, из синтетических нитей
                return "6204699000";//<6204699000> Брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты женские или для девочек,из прочих текстильных материалов
            }

            if (-1 != strTypeProduct.IndexOf("БРЮКИ")) {
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos) {
                    //<6204691800> Прочие брюки и бриджи женские или для девочек, из искусственных нитей
                    if (_IsWool(strSostav)) return "6204611000"; //<6204611000> Брюки и бриджи женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                    if (_IsHlopok(strSostav)) return "6204623900";//<6204623900> Прочие брюки и бриджи женские или для девочек, из хлопчатобумажной пряжи
                    if (_IsSintetik(strSostav)) return "6204631800";//<6204631800> Прочие брюки и бриджи женские или для девочек, из синтетических нитей
                    return "6204699000";//<6204699000> Брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты женские или для девочек,из прочих текстильных материалов
                }
                //<6203491900> Прочие брюки и бриджи мужские или для мальчиков, из искусственных нитей
                if (_IsWool(strSostav)) return "6203411000"; //<6203411000> Брюки и бриджи мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6203423500";//<6203423500> Прочие брюки и бриджи мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav))
                    return "6203431900";//<6203431900> Прочие брюки и бриджи мужские или для мальчиков, из синтетических нитей
                return "6203499000";//<6203499000> Брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты, мужские или для мальчиков, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("КОСТЮМ ЖЕН")) {
                //<6204191000> Костюмы женские или для девочек, из искусственных нитей
                if (_IsWool(strSostav)) return "6204110000";//<6204110000> Костюмы женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6204120000";//<6204120000> Костюмы женские или для девочек, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6204130000";//<6204130000> Костюмы женские или для девочек, из синтетических нитей
                return "6204199000";//<6204199000> Костюмы женские или для девочек, из прочих текстильных материалов
            }

            if (-1 != strTypeProduct.IndexOf("КОСТЮМ") || -1 != strTypeProduct.IndexOf("КОМПЛЕКТ")) {
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos) {
                    //<6204191000> Костюмы женские или для девочек, из искусственных нитей
                    if (_IsWool(strSostav)) return "6204110000";//<6204110000> Костюмы женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                    if (_IsHlopok(strSostav)) return "6204120000";//<6204120000> Костюмы женские или для девочек, из хлопчатобумажной пряжи
                    if (_IsSintetik(strSostav)) return "6204130000";//<6204130000> Костюмы женские или для девочек, из синтетических нитей
                    return "6204199000";//<6204199000> Костюмы женские или для девочек, из прочих текстильных материалов 
                }

                //<6203193000> Костюмы мужские или для мальчиков, из искусственных нитей
                if (_IsSintetik(strSostav))
                    return "6203120000";//<6203120000> Костюмы мужские или для мальчиков, из синтетических нитей

                if (_IsWool(strSostav)) return "6203110000";//<6203110000> Костюмы мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6203191000";//<6203191000> Костюмы мужские или для мальчиков, из хлопчатобумажной пряжи
                return "6203199000";//<6203199000> Костюмы мужские или для мальчиков, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("ПИДЖАК")) {
                //<6203391900> Прочие пиджаки и блайзеры мужские или для мальчиков, из искусственных нитей
                if (_IsWool(strSostav)) return "6203310000";//<6203310000> Пиджаки и блайзеры мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6203329000";//<6203329000> Прочие пиджаки и блайзеры мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6203339000";//<6203339000> Прочие пиджаки и блайзеры мужские или для мальчиков, из синтетических нитей
                return "6203399000";//<6203399000> Пиджаки и блайзеры мужские или для мальчиков из прочих текстильных материалов                
            }
            if (-1 != strTypeProduct.IndexOf("ДЖИНСЫ")) {
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos)
                    return "6204623100";//<6204623100> Прочие брюки и бриджи женские или для девочек, из денима, или джинсовой ткани
                return "6203423100";//<6203423100> Брюки и бриджи мужские или для мальчиков, из денима, или джинсовой ткани
            }
            if (-1 != strTypeProduct.IndexOf("ТОП")) {
                if (_IsWool(strSostav)) return "6109902000";//6109902000 Майки, фуфайки с рукавами и прочие нательные фуфайки трикотажные машинного или ручного вязания из шерстяной пряжи или пряжи из тонкого волоса животных или из химических нитей
                if (_IsHlopok(strSostav)) return "6109100000";//6109100000 Майки, фуфайки с рукавами и прочие нательные фуфайки трикотажные, из хлопчатобумажной пряжи, машинного или ручного вязания
                return "6109909000"; //6109909000 Майки, фуфайки с рукавами и прочие нательные фуфайки трикотажные, из прочих текстильных материалов, машинного или ручного вязания
            }

            if (-1 != strTypeProduct.IndexOf("ПОЛО")) {
                if (_IsHlopok(strSostav)) return "6105100000";//<6105100000> Рубашки трикотажные, мужские или для мальчиков, из хлопчатобумажной пряжи, машинного или ручного вязания
                if (_IsWool(strSostav)) return "6105901000";//<6105901000> Рубашки трикотажные, мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных, машинного или ручного вязания
                if (_IsSintetik(strSostav)) return "6105201000";//<6105201000> Рубашки трикотажные, мужские или для мальчиков, из химических синтетических нитей, машинного или ручного вязания

                //< 6105209000 > Рубашки трикотажные, мужские или для мальчиков, из химических искусственных нитей, машинного или ручного вязания
                return "6105909000";//<6105909000> Рубашки трикотажные, мужские или для мальчиков, из прочих текстильных материалов, машинного или ручного вязания
            }


            if (-1 != strTypeProduct.IndexOf("ФУТБОЛКА")) return "6109000000";
            if (-1 != strTypeProduct.IndexOf("ХУДИ")) return "6110000000";

            if (-1 != strTypeProduct.IndexOf("ПЛАТЬЕ") || -1 != strTypeProduct.IndexOf("САРАФАН")) {
                //6204440000 Платья женские или для девочек из искусственных нитей
                if (_IsWool(strSostav)) return "6204410000";//6204410000 Платья женские или для девочек из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav)) return "6204420000";//6204420000 Платья женские или для девочек из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav)) return "6204430000";//6204430000 Платья женские или для девочек из синтетических нитей
                if (_IsShelk(strSostav)) return "6204491000";//6204491000 Платья женские или для девочек из шелковых нитей или пряжи из шелковых отходов
                return "6204499000";//6204499000 Платья женские или для девочек из прочих текстильных материалов

            }

            if (-1 != strTypeProduct.IndexOf("БЛУЗКА") || -1 != strTypeProduct.IndexOf("БЛУЗА") ||
                -1 != strTypeProduct.IndexOf("БЛУЗА-ТОП") || -1 != strTypeProduct.IndexOf("ТУНИКА")) {
                if (_IsWool(strSostav)) return "6206200000";
                if (_IsHlopok(strSostav)) return "6206300000";
                if (_IsSintetik(strSostav)) return "6206400000";
                if (_IsLen(strSostav)) return "6206901000";
                if (_IsShelk(strSostav)) return "6206100000";
                return "6206909000";
            }
            if (-1 != strTypeProduct.IndexOf("КАРДИГАН") || -1 != strTypeProduct.IndexOf("ВОДОЛАЗКА") || -1 != strTypeProduct.IndexOf("СВИТШОТ")) {
                //if (_IsWool(strSostav))
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos)
                    return "6110119000"; //<6110119000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для женщин или девочек, из шерстяной пряжи, машинного или ручного вязания
                return "6110113000"; //<6110113000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для мужчин или мальчиков, из шерстяной пряжи, машинного или ручного вязания
            }

                if (-1 != strTypeProduct.IndexOf("ДЖЕМПЕР") || -1 != strTypeProduct.IndexOf("СВИТЕР")) {
                if (_IsSintetik(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110309900"; //<6110309900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для женщин или девочек, машинного или ручного вязания
                    return "6110309100"; //<6110309100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для мужчин или мальчиков, машинного или ручного вязания
                }
                if (_IsHlopok(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110209900"; //<6110209900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для женщин или девочек, машинного или ручного вязания
                    return "6110209100"; //<6110209100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для мужчин или мальчиков, машинного или ручного вязания
                }
                if (_IsLen(strSostav))
                    return "6110901000"; //<6110901000> Свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из льняных волокон или волокна рами, машинного или ручного вязания
                return "6110909000";//<6110909000> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из прочих текстильных материалов, машинного или ручного вязания
            }
            if (-1 != strTypeProduct.IndexOf("ЖИЛЕТ")) {
                if (_IsWool(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110119000"; //<6110119000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для женщин или девочек, из шерстяной пряжи, машинного или ручного вязания
                    return "6110113000"; //<6110113000> Прочие кардиганы, жилеты и аналогичные изделия трикотажные, для мужчин или мальчиков, из шерстяной пряжи, машинного или ручного вязания
                }
                if (_IsHlopok(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110209900"; //<6110209900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для женщин или девочек, машинного или ручного вязания
                    return "6110209100"; //<6110209100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из хлопчатобумажной пряжи, для мужчин или мальчиков, машинного или ручного вязания
                }
                if (_IsSintetik(strSostav)) {
                    int nPos = strMod.LastIndexOf("ЖЕН");
                    if (-1 != nPos)
                        return "6110309100"; //<6110309100> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для мужчин или мальчиков, машинного или ручного вязания
                    return "6110309900"; //<6110309900> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из химических нитей, для женщин или девочек, машинного или ручного вязания
                }
                if (_IsLen(strSostav))
                    return "6110901000";//<6110901000> Свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из льняных волокон или волокна рами, машинного или ручного вязания
                return "6110909000";//<6110909000> Прочие свитеры, пуловеры, джемперы, жилеты и аналогичные изделия трикотажные, из прочих текстильных материалов, машинного или ручного вязания
            }
            if (-1 != strTypeProduct.IndexOf("ЖАКЕТ")) {
                if (_IsSintetik(strSostav))
                    return "6204339000"; //<6204339000> Жакеты и блайзеры женские или для девочек, из синтетических нитей
                if (_IsWool(strSostav))
                    return "6204310000"; //<6204310000> Жакеты и блайзеры женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6204329000";//<6204329000> Прочие жакеты и блайзеры женские или для девочек, из хлопчатобумажной пряжи
                return "6204399000";//<6204399000> Прочие жакеты и блайзеры женские или для девочек, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("РУБАШКА")) {
                int nPos = strMod.LastIndexOf("ЖЕН");
                if (-1 != nPos) {
                    if (_IsWool(strSostav)) return "6206200000";
                    if (_IsHlopok(strSostav)) return "6206300000";
                    if (_IsSintetik(strSostav)) return "6206400000";
                    if (_IsLen(strSostav)) return "6206901000";
                    if (_IsShelk(strSostav)) return "6206100000";
                    return "6206909000";
                }

                if (_IsHlopok(strSostav))
                    return "6205200000";//<6205200000> Рубашки мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsWool(strSostav))
                    return "6205908001";//<6205908001> Рубашки  из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsSintetik(strSostav))
                    return "6205300000"; //<6205300000> Рубашки  из химических нитей
                if (_IsLen(strSostav))
                    return "6205901000"; //<6205901000> Рубашки  из льняных волокон или волокна рами
                return "6205908009";//<6205908009> Рубашки  из прочих текстильных материалов                  
            }
            if (-1 != strTypeProduct.IndexOf("ГАЛСТУК")) {
                if (_IsSintetik(strSostav))
                    return "6215200000"; //<6215200000> Галстуки, галстуки-бабочки и шейные платки, из химических нитей
                if (_IsShelk(strSostav))
                    return "6215100000";//<6215100000> Галстуки, галстуки-бабочки и шейные платки, из шелковых нитей или пряжи из шелковых отходов
                return "6215900000"; //<6215900000> Галстуки, галстуки-бабочки и шейные платки, из прочих текстильных материалов
            }
            if (-1 != strTypeProduct.IndexOf("ШОРТЫ")) {
                if (_IsHlopok(strSostav))
                    return "6203429000";//<6203429000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav))
                    return "6203439000"; //<6203439000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из синтетических нитей
                //<6203419000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из шерстяной пряжи или пряжи из тонкого волоса животных
                return "6203495000"; //<6203495000> Прочие брюки, комбинезоны с нагрудниками и лямками, бриджи и шорты мужские или для мальчиков, из искусственных нитей
            }
            if (-1 != strTypeProduct.IndexOf("ЮБКА")) {
                //<6204591000> Юбки и юбки-брюки женские или для девочек, из искусственных нитей
                if (_IsWool(strSostav))
                    return "6204510000";//<6204510000> Юбки и юбки-брюки женские или для девочек, из шерстяной пряжи или пряжи из тонкого волоса животных
                if (_IsHlopok(strSostav))
                    return "6204520000";//<6204520000> Юбки и юбки-брюки женские или для девочек, из хлопчатобумажной пряжи
                if (_IsSintetik(strSostav))
                    return "6204530000";//<6204530000> Юбки и юбки-брюки женские или для девочек, из синтетических нитей                
                return "6204599000";//<6204599000> Юбки и юбки-брюки женские или для девочек, из прочих текстильных материалов               
            }
            MessageBox.Show(null, "Ошибка, неизвестный вида изделия:" + strTypeProduct, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return strTypeProduct;
        }

        protected virtual bool _InitPaths() {
            MessageBox.Show("Ошибка, метод _InitPaths не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }
        protected virtual string _GetFileNameCreateOrderFM() {
            MessageBox.Show("Ошибка, метод _GetFileNameCreateOrderFM не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "";
        }

        public virtual string GetZplName() {
            MessageBox.Show("Ошибка, метод GetZplName не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return ""; 
        }
        public virtual bool ShowPage(_Form1 parent, DataGridView dataGridView, GridViewExtensions.DataGridFilterExtender dataGridFilter, string strOrderID = "-1") {
            MessageBox.Show("Ошибка, метод ShowPage не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }
        protected virtual bool _EditProduct(ref DataTable dt) {
            MessageBox.Show("Ошибка, метод EditProduct не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }

        public virtual bool AddNewProduct(Form parent, string strOrderID) {
            MessageBox.Show("Ошибка, метод AddNewProduct не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }
        public virtual bool CheckStikers() {
            DlgCheckStikers dlg = new(_GetShopPrefix(), _GetEan13Type(), null, _markingPaths, _MARKINGTYPES);
            if (DialogResult.Abort == dlg.ShowDialog())
                return false;
            return true;
        }

        public virtual void Print(PrintParms pp, int nStcker = -1,
    string strBarcodePrint = "", string strCodeSpm = "", bool bFlabelToBarcode = false, bool bWithoutKM = false) {
            MessageBox.Show("Ошибка, метод Print не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        public virtual void ExportToRaskroyny(DataGridView dataGridView) {
            MessageBox.Show("Ошибка, метод ExportToRaskroyny не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        public virtual void ImportInFlabel(string strFileName, string strOrderID) {
            MessageBox.Show("Ошибка, метод ImportInFlabel не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        public virtual bool ExportToExcel(_Form1 parent) {
            MessageBox.Show("Ошибка, метод ExportToExcel не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }

        protected void  _InitOrders() {
            if (null == _markingPaths) {
                MessageBox.Show("Ошибка входных параметров в _InitOrders", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (null != _markingOrders) return;
            _markingOrders = new(_markingPaths, _GetShopPrefix());
        }
        public virtual string GetSettingsID() {
            MessageBox.Show("Ошибка, метод GetSettingsID не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return _strSettingsID;
        }
        protected virtual COLUMNS_PRODUCTS _GetCollumnsPRD() {
            MessageBox.Show("Ошибка, метод _InitCollumns не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
        protected virtual string GetNomencl(DataRow row) {
            MessageBox.Show("Ошибка, метод GetNomencl не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "";
        }
        protected virtual string _GetShopPrefix() {
            MessageBox.Show("Ошибка, метод _GetShopPrefix не должен вызываться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return ""; 
        }
        protected virtual EAN13_TYPE _GetEan13Type() {
            return EAN13_TYPE.GTIN;
        }
       
        public bool ShowDescription(IWin32Window parent, string strName, out string strOut) {
            strOut = "";
            _InitOrders();
            if (null == _markingOrders) {
                MessageBox.Show("Ошибка входных параметров в ShowDescription", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return _markingOrders.ShowDescription(parent,strName, out strOut);
        }
        public bool AddOrder(string strName, out string strID) {
            _InitOrders();
            if (null == _markingOrders) {
                MessageBox.Show("Ошибка входных параметров в AddOrder", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                strID = "";
                return false;
            }
            return _markingOrders.AddOrder(strName, out strID);
        }
        public bool ReturnHidenOrders(IWin32Window parent) {
            _InitOrders();
            if (null == _markingOrders) {
                MessageBox.Show("Ошибка входных параметров в ReturnHidenOrders", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return _markingOrders.ReturnHidenOrders(parent);
        }
        public bool HideOrder(string strName) {
            _InitOrders();
            if (null == _markingOrders) {
                MessageBox.Show("Ошибка входных параметров в HideOrder", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return _markingOrders.HideOrder(strName);
        }

        protected void _GetNomenclWithGtin(ref HashSet<string> hNomenclWithGtin) {
            if (null == _markingPaths) {
                MessageBox.Show("Ошибка входных параметров в _GetNomenclWithGtin", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string strDestSoputkaGtin = "";
            string strSrcSoputkaGtin = "";
            if (!DbfWrapper.CheckIfFileExist(_markingPaths.GetGtinFileName(), ref strDestSoputkaGtin, ref strSrcSoputkaGtin, _markingPaths.GetMainFolder())) { return; }

            int nCodeOut = -1;
            DataTable dtTableSoputkaGtin = Dbf.LoadDbfWithAddColumns(strDestSoputkaGtin, out _, ref nCodeOut);
            int NOMENCL = 4;
            for (int i = 0; i < dtTableSoputkaGtin.Rows.Count; i++)
                hNomenclWithGtin.Add((string)dtTableSoputkaGtin.Rows[i][NOMENCL]);
        }

        public bool FillTree(TreeView tv) {
            if (!_InitPaths()) return false;
            if (null == _markingPaths) {
                MessageBox.Show("Ошибка входных параметров в FillTree", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            _markingPaths.FillTree(tv, _GetShopPrefix());
            return true;
        }
        public bool AddEAN13() {
            _InitPaths();
            if (null == _markingPaths || null == _dataGridView || null == _hNumTable || 
                0 == _hNumTable.Count || "" == _strPrefixEan13) {
                MessageBox.Show("Ошибка входных параметров в _AddEAN13", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; 
            }
            COLUMNS_PRODUCTS cp = _GetCollumnsPRD();
            int BARCODE = cp.BARCODE;
            string strDestPath = _markingPaths.GetDestPath();
            string strSrcPath = _markingPaths.GetSrcPath();

            int N = 0;
            try {
                foreach (DataGridViewRow gvRow in _dataGridView.Rows) {
                    DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                    if (System.DBNull.Value != row[BARCODE])
                        continue;
                    int nNUM = Convert.ToInt32(row[N]);
                    string strNumInDB = _hNumTable[nNUM].ToString();

                    DateTime dtOpenedFile = Win32.GetLastWriteTime(strDestPath);
                    string strNewBarcode = _strPrefixEan13 + _hNumTable[nNUM].ToString("0000000000");
                    string strPostfix = "";
                    if (!EAN13.GetPostfix(strNewBarcode, ref strPostfix))
                        continue;
                    strNewBarcode += strPostfix;
                    if (!Dbf.SetValue(strDestPath, "№", strNumInDB, "BARCODE", strNewBarcode, dtOpenedFile)) {
                        Console.Beep();
                        MessageBox.Show("Ошибка записи в файл: " + strDestPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Trace.WriteLine("Ошибка записи в файл: " + strDestPath);
                        return false;
                    }
                }

            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try {
                File.Copy(strDestPath, strSrcPath, true);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }


        public bool EditSelProducts() {
            if (0 == _dataGridView.SelectedRows.Count)
                return false;
            if (null == _markingPaths || null == _dataGridView) {
                MessageBox.Show("Ошибка входных параметров в EditSelProducts", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            string strDestPath = "";
            string strSrcPath= "";
            if (!DbfWrapper.CheckIfFileExist(_markingPaths.GetProductsFileName(), ref strDestPath, ref strSrcPath, _markingPaths.GetGeneratedFolder())) return false;

            COLUMNS_PRODUCTS cp = _GetCollumnsPRD();
            string strGTIN = "";

            List<string> listRows = [];
            foreach (DataGridViewRow row in _dataGridView.SelectedRows) {
                listRows.Add(_hNumTable[(int)row.Cells[0].Value].ToString());

                if (System.DBNull.Value != row.Cells[cp.GTIN].Value) 
                    strGTIN = row.Cells[cp.GTIN].Value.ToString();
                break;
            }
            int nCodeOut = -1;
            DataTable dtTableRow = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "№", listRows[0]);
            if (0 == dtTableRow.Rows.Count)
                return false;

            dtTableRow.Rows[0][cp.GTIN] = strGTIN;

            if (!_EditProduct(ref dtTableRow))
                return false;
            int nNewRowOut = -1;
            GenLogic.CopyToArhive(strSrcPath);
            int N = 0;
            if (!Dbf.SaveOneRow(strSrcPath, dtTableRow, N, ref nNewRowOut)) {                
                MessageBox.Show(null, "Ошибка сохранения в файл " + strSrcPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        public bool HideSelProducts() {
            if (null == _markingPaths || null == _dataGridView || 0 == _dataGridView.SelectedRows.Count) {
                MessageBox.Show("Ошибка входных параметров в HideSelProducts", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            DialogResult rez = MessageBox.Show("Вы действительно хотите пометить строки на удаление", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rez != DialogResult.Yes)
                return false;
            string strSrcPath = _markingPaths.GetSrcPath();

            GenLogic.CopyToArhive(strSrcPath);
            try {
                List<int> listRows = [];
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                    listRows.Add(_hNumTable[(int)row.Cells[0].Value]);
                listRows.Sort();
                byte nDEL = 42;
                if (!Dbf.SetDelRowBytes(strSrcPath, listRows, nDEL, ref _dtOpenedFile))
                    return false;
            } catch (Exception ex) {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            } finally {
            }
            return true;
        }
        protected bool _AddCreatedOrdersKMToDB(string strDestSoputkaOrdersMK, string strSrcSoputkaOrdersMK, Dictionary<string, string> dicGtin_OrderID_Received) {
            int nCodeOut = -1;
            DataTable dtTableSoputkaOrdersMK = Dbf.LoadDbfWithAddColumns(strDestSoputkaOrdersMK, out _, ref nCodeOut, "№", "1");
            dtTableSoputkaOrdersMK.Rows.Clear();

            int GTIN2 = 3;
            int ORDER_MARK = 4;
            int DATECREATE = 8;
            int BARCODE = 9;
            DateTime dt = DateTime.Now;
            List<string> lMsg = [];
            foreach (var item in dicGtin_OrderID_Received) {
                string[] parms = item.Key.Split('_');
                string strGTIN = parms[0];
                string strBARCODE = parms[1];
                System.Data.DataRow rowAdd = dtTableSoputkaOrdersMK.NewRow();
                rowAdd[GTIN2] = strGTIN;
                rowAdd[BARCODE] = strBARCODE;
                rowAdd[ORDER_MARK] = item.Value;
                rowAdd[DATECREATE] = dt;
                dtTableSoputkaOrdersMK.Rows.Add(rowAdd);
                lMsg.Add("ERROR, данные не попавшие в SoputkaOrdersKM.dbf, GTIN:" + strGTIN + " ORDER_MARK: " + item.Value + " DATECREATE:" + dt.ToString("dd.MM.yyyy"));
            }

            if (dtTableSoputkaOrdersMK.Rows.Count == 0) {
                MessageBox.Show(null, "Ошибка: не должно быть 0 элементов в dtTableSoputkaOrdersMK", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!DbfWrapper.AddDoDB(null, strDestSoputkaOrdersMK, strSrcSoputkaOrdersMK, dtTableSoputkaOrdersMK)) {
                foreach (string str in lMsg)
                    Trace.WriteLine(str);
                return false;
            }
            return true;
        }
        protected static string _GetCurDir() {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }
        protected static DateTime _GetLastWriteTime(string strPath) {
            while (true) {
                DateTime dt = Win32.GetLastWriteTime(strPath);
                if (1929 != dt.Year)
                    return dt;
                Trace.WriteLine("Sleep: 1000 ms");
                System.Threading.Thread.Sleep(1000);
            }
        }

        protected string _GetPrinterName() {
            if (null == _markingPaths) {
                MessageBox.Show("Ошибка входных параметров в _GetPrinterName", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            return _markingPaths.GetPrinterName();

            //return "Zebra S4M (203 dpi) - ZPL";
        }
        public virtual Encoding GetStickerEncoding() {
            return Encoding.GetEncoding(866);
        }

        public bool SendTextFileToPrinter(string szFileName) {
            string printerName = _GetPrinterName();// "Zebra S4M (203 dpi) - ZPL";

     //          printerName = "Zebra ZT230 (203 dpi)";
    //          szFileName = @"c:\0\5\pr5!!!!!!!!!.zpl";
           
            byte[] bytes = File.ReadAllBytes(szFileName);
            //IntPtr pUnmanagedBytes = new(0);
            int nLength = bytes.Length;
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);

            //_ZPL_TO_PDF(szFileName);

            return RawPrinterHelper.SendBytesToPrinter(printerName, pUnmanagedBytes, nLength);
            //return RawPrinterHelper.SendStringToPrinter(printerName, sb.ToString());
        }

        protected bool _GetNotProcessedGTIN(string strDestPathOrdersKM, string strSrcPathOrdersKM, ref DateTime dtBvGtinOrdersMkLoaded,
           ref HashSet<long> hsNotProcessedGTIN, ref DataGridView dataGridView) {
            DateTime dt = _GetLastWriteTime(strSrcPathOrdersKM);
            if (dtBvGtinOrdersMkLoaded != dt) {
                MessageBox.Show(null, "Данные изменились, Необходимо обновить таблицу с товаром", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            int nCodeOut = -1;

            int GTIN = 3;
            int STATUS = 5;
            DataTable dtTableOrdersKM = Dbf.LoadDbfWithAddColumns(strDestPathOrdersKM, out _, ref nCodeOut, "DEL", "0");
            for (int i = 0; i < dtTableOrdersKM.Rows.Count; i++) {
                if (System.DBNull.Value != dtTableOrdersKM.Rows[i][STATUS])
                    continue;
                if (System.DBNull.Value == dtTableOrdersKM.Rows[i][GTIN])
                    continue;
                hsNotProcessedGTIN.Add((long)dtTableOrdersKM.Rows[i][GTIN]);
            }
            if (hsNotProcessedGTIN.Count == dataGridView.SelectedRows.Count) {
                string strBody1 = "Не могу получить коды маркировки, на выбранные GTIN: еще не обработаны предыдущие запросы";
                MessageBox.Show(null, strBody1, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }
       
        protected string _CreateOrderFairMark(string strGTIN = "", int nCount = 1, string strBARCODE = "BARCODE", bool bKomplekt = false) {
            string strFile = _GetCurDir() + "\\requests\\" + _GetFileNameCreateOrderFM();//"CreateOrderM.txt";
            if (!File.Exists(strFile)) {
                MessageBox.Show("нет файла: " + strFile);
                return "";
            }
            if ("" == _strSettingsID) {
                MessageBox.Show("Ошибка входных параметров в _CreateOrderFairMark", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            string strRequest = File.ReadAllText(strFile).Replace("\r\n", " ");
            strRequest = strRequest.Replace("22222", nCount.ToString());
            strRequest = strRequest.Replace("33333", strBARCODE);

            if ("" != strGTIN)
                strRequest = strRequest.Replace("11111", strGTIN);

            if (bKomplekt)
                strRequest = strRequest.Replace("UNIT", "BUNDLE");

            FairMark fm = new(_strSettingsID);
            CreateOrderRespons resp = fm.CreateOrder(strRequest);
            if ("" == resp.orderId) {
                if (null != resp.err && resp.err.fieldErrors.Count > 0)
                    MessageBox.Show("Ошибка: " + resp.err.fieldErrors[0].fieldError + ", при запросе: " + strRequest, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            return resp.orderId;
        }

        protected bool _CreateOrdersInFairMar(out Dictionary<string, string> dicGtin_OrderID_Received, out string strInfoOut) {
            dicGtin_OrderID_Received = [];
            strInfoOut = "";
            string strDestPathOrdersKM = _markingPaths.GetDestPathOrdersKM();
            string strSrcPathOrdersKM = _markingPaths.GetSrcPathOrdersKM();
            HashSet<long> hsNotProcessedGTIN = [];
            if (!_GetNotProcessedGTIN(strDestPathOrdersKM, strSrcPathOrdersKM, ref _dtGtinOrdersMkLoaded, ref hsNotProcessedGTIN, ref _dataGridView))
                return false;

            COLUMNS_PRODUCTS cp = _GetCollumnsPRD();
            int BARCODE = cp.BARCODE;
            int KOL = cp.KOL;
            int GTIN = cp.GTIN;
            int KOL_KM = cp.KOL_KM;
            int IZDNAME = cp.IZDNAME;

            int nErrors = 0;
            int nOK = 0;
            int nCreatedBefore = 0;
            int nNotProcessed = 0;
            foreach (DataGridViewRow gvRow in _dataGridView.SelectedRows) {
                DataRow row = ((DataRowView)gvRow.DataBoundItem).Row;
                long lGTIN = (long)row[GTIN];
                string strGtin0 = lGTIN.ToString();
                string strGtin1 = lGTIN.ToString() + "_" + row[BARCODE].ToString();
                string strGtin2 = "0" + strGtin0;
                string strBARCODE = "SP BARCODE: " + row[BARCODE].ToString();

                int nCol_KM = 0;
                if (System.DBNull.Value != row[KOL_KM]) nCol_KM = (int)row[KOL_KM];

                int nCount = (int)row[KOL] - nCol_KM;
                nCount = (int)row[KOL];     
                if (nCount <= 0) {
                    Trace.WriteLine("Для GTIN:" + strGtin1 + ", уже ранее были заказаны QR коды");
                    nCreatedBefore++;
                    continue;
                }
                if (hsNotProcessedGTIN.Contains(lGTIN)) {
                    nNotProcessed++;
                    continue;
                }
                bool bKomplekt = false;
                string strIzdName = (string)row[IZDNAME];
                int nPos = strIzdName.ToLower().LastIndexOf("костюм");
                if (-1 != nPos)
                    bKomplekt = true;
                string strCreateID = _CreateOrderFairMark(strGtin2, nCount, strBARCODE, bKomplekt);
                if ("" == strCreateID) {
                    nErrors++;
                    Trace.WriteLine("Не создан заказ на Коды Маркировки на: " + nCount.ToString() + " штук, для GTIN:" + strGtin1);
                    continue;
                }
                Trace.WriteLine("Создан заказ на Коды Маркировки: " + strCreateID + " на: " + nCount.ToString() + " штук, для GTIN:" + strGtin1);
                nOK++;
                dicGtin_OrderID_Received.Add(strGtin1, strCreateID);
            }
            if (nCreatedBefore == _dataGridView.SelectedRows.Count) {
                MessageBox.Show(null, "Для выбранных GTIN, QR коды уже были заказаны", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (0 == dicGtin_OrderID_Received.Count) {
                if (1 == _dataGridView.SelectedRows.Count)
                    MessageBox.Show(null, "Ошибка: номер заказа для маркиовки не получен, повторите ваши действия чуть позже", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(null, "Ошибка: номера заказов для маркиовки не получены, повторите ваши действия чуть позже", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            strInfoOut = "Создано успешно: " + nOK.ToString() + " заказов на Коды Маркировки,\n не создано: заказов: " + nErrors.ToString() + ",\n уже было создано ранее заказов: " + nCreatedBefore.ToString() + ", \n еще не обработано заказов: " + nNotProcessed.ToString();
            return true;
        }

        protected void _GetSelProducts(ref HashSet<string> hProducts) {
            if (null == _dataGridView) {
                MessageBox.Show("Ошибка входных параметров в _GetSelProducts", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (DataGridViewRow gvRow in _dataGridView.SelectedRows) {
                hProducts.Add(GetNomencl(((DataRowView)gvRow.DataBoundItem).Row));
            }
        }
        public bool GetKM(_Form1 parent) {
            if (null == _markingPaths || null == _dataGridView || 0 == _dataGridView.SelectedRows.Count) return false;
            if (null == parent) return false;

            if (!_GetSelProducts()) return false;

            if (!_CreateOrdersInFairMar(out Dictionary<string, string> dicGtin_OrderID_Received, out string strInfoOut))
                return false;

            if (!_AddCreatedOrdersKMToDB(_markingPaths.GetDestPathOrdersKM(), _markingPaths.GetSrcPathOrdersKM(), dicGtin_OrderID_Received))
                return false;
            Trace.WriteLine(strInfoOut);
            MessageBox.Show(parent, strInfoOut, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        public void ExportToExcelKM(HashSet<string> hsBarcodes, int nLenKM) {
            if (0 == hsBarcodes.Count)
                return;
            ExcelWrapper.ExportToExcelKM(null, _markingPaths.GetDestPathKM(), hsBarcodes, nLenKM);
        }
        public void ExportToExcelNomenkl(List<(string name, string barcode)> listNomenkl) {
            if (0 == listNomenkl.Count)
                return;
            ExcelWrapper.ExportToExcelNomenkl(listNomenkl);
        }
        public void ExportTo1СNomenkl(List<(string name, string barcode)> listNomenkl) {            
            if (0 == listNomenkl.Count)
                return;
            if (null == _markingPaths) {
                MessageBox.Show("Ошибка входных параметров в ExportTo1СNomenkl", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DlgInputBox dlgInp = new(false);
            dlgInp.SetCaption("Введите");
            dlgInp.SetFildName("Код копируемой номенклатуры");
            if (DialogResult.OK != dlgInp.ShowDialog())
                return;
            bool bRet = false;

            BackgroundWorker bw = new() {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            int nCount = 0;
            bw.DoWork += delegate (object sender2, DoWorkEventArgs e2) {
                bw.ReportProgress(0);
                string connectionStringTESTBD = $"Srvr='{_markingPaths.Get1cIP()}';ref='{_markingPaths.Get1cBase()}';usr='{_markingPaths.Get1cUser()}';pwd='{_markingPaths.Get1cPsw()}'";

                Com1C.ConnectTo1C(connectionStringTESTBD);
                if (!Com1C.IsConnected) {
                    bRet = false;
                    return;
                }
                bRet = true;
                foreach ((string name, string barcode) in listNomenkl) {
                    if (!Com1C.CreateCopyNomenkl(dlgInp.GetString(), name, barcode))//"КА-00210091"
                        bRet = false;
                    nCount++;                    
                    bw.ReportProgress((int)(nCount * 100 / listNomenkl.Count));

                    if (bw.CancellationPending) {
                        bRet = false;
                        break;
                    }
                    if(!bRet)
                        break;
                }
                bw.ReportProgress(100);
            };            
            DlgReportProgress dlg = new(bw, "Импорт в 1С");
            dlg.ShowDialog();

            if(bRet)
                MessageBox.Show(null, "Импорт произведен успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(null, "Импорт произведен не успешно", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }

        protected bool _GetSelProducts() {
            if (null == _dataGridView) {
                MessageBox.Show("Ошибка входных параметров в _GetSelProducts", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (_dataGridView.SelectedRows.Count == 0)
                return false;
            HashSet<string> hProducts = [];
            foreach (DataGridViewRow gvRow in _dataGridView.SelectedRows)
                hProducts.Add(GetNomencl(((DataRowView)gvRow.DataBoundItem).Row));

            HashSet<string> hNomenclWithGtin = [];
            _GetNomenclWithGtin(ref hNomenclWithGtin);

            int nCount1 = 0;
            List<string> listProductsToQR = [];
            foreach (string strProd in hProducts) {
                if (!hNomenclWithGtin.Contains(strProd)) {
                    nCount1++;
                    continue;
                }
                listProductsToQR.Add(strProd);
            }
            if (0 == listProductsToQR.Count) {
                //MessageBox.Show(this, "Ошибка, Нет выбранных элементов с GTIN для получения QR кодов (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show(null, "Ошибка, для выбранных элементов отсутствуют GTIN (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (nCount1 > 0) {
                MessageBox.Show(null, "Ошибка, для :" + nCount1 + " выбранных элементов отсутствуют GTIN (предварительно нужно получить GTIN)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool ImportGtinFromExcel(_Form1 paren, string strFileName) {
            if (null == _markingPaths || null == paren) {
                MessageBox.Show("Ошибка входных параметров в ImportGtinFromExcel", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string strDestSoputkaGtin = "";
            string strSrcSoputkaGtin = "";
            if (!DbfWrapper.CheckIfFileExist(_markingPaths.GetGtinFileName(), ref strDestSoputkaGtin, ref strSrcSoputkaGtin, _markingPaths.GetMainFolder())) { return false; }

            string strCompanyName = "";
            //if (rbV_IMPORT_SOPUTKA.Checked)
            //    strCompanyName = "ООО 'ВЕТЕКС'";
            if (!DbfWrapper.ImportGtinFromExcel(paren, strFileName, strDestSoputkaGtin, strSrcSoputkaGtin, strCompanyName))//"ООО '____'"
                return false;
            return true;
        }


    }
}
