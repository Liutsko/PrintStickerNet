using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DbfLib;

#pragma warning disable IDE1006 //Нарушение правила именования: Префикс "_" является недопустимым

namespace PrintSticker
{
     public class ProductForImportSP {

        private static string _strSrcPath = "";
        private static List<string> _listMods = null;
        private static DateTime _dtLastEditFile = new(1929, 1, 1);//"DbImportProductsSP.dbf"
      
        private void _Load() {
            string strDestPath = "";
            if (!DbfWrapper.CheckIfFileExist("DbImportProductsSP.dbf", ref strDestPath, ref _strSrcPath, "\\Soputka\\"))
                return;
            int nCodeOut = -1;
            DataTable dtTable = Dbf.LoadDbfWithAddColumns(strDestPath, out _, ref nCodeOut, "IMPORT", "ДА", null, false,false);
            _dtLastEditFile = Win32.GetLastWriteTime(_strSrcPath);
            _listMods = [];
            int PRODUCT = 2;
            for (int i = 0; i < dtTable.Rows.Count; i++)
                _listMods.Add((string)dtTable.Rows[i][PRODUCT]);            
        }
        public bool Contains(string strItem) {
            if (null != _listMods) {
                DateTime dt = Win32.GetLastWriteTime(_strSrcPath);
                if (_dtLastEditFile != dt)
                    _Load();
            }
            if (null == _listMods)
                _Load();

            return _listMods.Contains(strItem);
         }
     }

     public struct PRODUCTINCUTCOLNAME {
         public const string BARCODE = "BARCODE";
         public const string IZD = "IZD";
         public const string PRS = "PRS";
         public const string MOD = "MOD";
         public const string ART = "ART";
         public const string ART2 = "ART2";
         public const string RAZ = "RAZ";
         public const string SRT = "SRT";
         public const string PATTERN = "PATTERN";
         public const string CCODE = "CCODE";
         public const string CCLOTH = "CCLOTH";
         public const string COTHER = "COTHER";
         public const string CSEASON = "CSEASON";
         public const string DAT1 = "DAT1";

     }
     public struct PRODUCTINCUTCOLID {
         public const int BARCODE = 3;
         public const int IZD = 4;
         public const int PRS = 6;
         public const int MOD = 7;
         public const int ART = 8;
         public const int RAZ = 9;
         public const int SRT = 10;
         public const int ART2 = 14;
         public const int PATTERN = 15;
         public const int CCODE = 16;
         public const int CCLOTH = 17;
         public const int COTHER = 18;
         public const int CSEASON = 19;
     }
     public struct PRODUCTINCUTCOLID_TYPE3 {
         public const int BARCODE = 3 + 1;
         public const int IZD = 4 + 1;
         public const int PRS = 6 + 1;
         public const int MOD = 7 + 1;
         public const int ART = 8 + 1;
         public const int RAZ = 9 + 1;
         public const int SRT = 10 + 1;
         public const int ART2 = 14 + 1;
         public const int PATTERN = 15 + 1;
         public const int CCODE = 16 + 1;
         public const int CCLOTH = 17 + 1;
         public const int COTHER = 18 + 1;
         public const int CSEASON = 19 + 1;
     }  
     public struct PRODUCTINCUTCOLID_TYPE2 {
         public const int BARCODE = 4;
         public const int IZD = 11;
         public const int PRS = 14;
         public const int MOD = 15;
         public const int ART = 16;
         public const int RAZ = 18;//
         public const int SRT = 19;
         public const int ART2 = 35;
         public const int PATTERN = 36;
         public const int CCODE = 37;
         public const int CCLOTH = 38;
         public const int COTHER = 39;
         public const int CSEASON = 40;
     }    

     public class ProductInCUT
     {
         private readonly string strBARCODE = "";
         private readonly string strIZD = "";
         private readonly string strPRS = "";
         private readonly string strMOD = "";
         private readonly string strART = "";
         private readonly string strART2 = "";
         private readonly string strRAZ = "";
         private readonly string strSRT = "";

         private readonly string strPATTERN = "";
         private readonly string strCCODE = "";
         private readonly string strCCLOTH = "";
         private readonly string strCOTHER = "";
         private readonly string strCSEASON = "";
         
         public string GetProduct() {
             return strIZD + "#" + strPRS + "#" + strMOD + "#" + strART + strART2 +
                 "#" + strRAZ + "#" + strSRT + "#" + strPATTERN + "#" + strCCODE + "#" + strCCLOTH + "#" + strCOTHER + "#" + strCSEASON;
         }
         public string GetBARCODE() { return strBARCODE; }

         public string GetIZD() { return strIZD; }
         public string GetPRS() { return strPRS; }
         public string GetMOD() { return strMOD; }
         public string GetART() { return strART; }
         public string GetART2() { return strART2; }
         public string GetRAZ() { return strRAZ; }
         public string GetSRT() { return strSRT; }

         public string GetPATTERN() { return strPATTERN; }
         public string GetCCODE() { return strCCODE; }
         public string GetCCLOTH() { return strCCLOTH; }
         public string GetCOTHER() { return strCOTHER; }
         public string GetCSEASON() { return strCSEASON; }

         public ProductInCUT(DataRow dr, int nTypeSourceFile)
         {
             strBARCODE = "";
             strIZD = "";
             strPRS = "";
             strMOD = "";
             strART = "";
             strART2 = "";
             strRAZ = "";
             strSRT = "";

             strPATTERN = "";
             strCCODE = "";
             strCCLOTH = "";
             strCOTHER = "";
             strCSEASON = "";
             //dtDAT1 = new(1929, 1, 1);

             int nBARCODE = PRODUCTINCUTCOLID.BARCODE;
             int nIZD = PRODUCTINCUTCOLID.IZD;
             int nPRS = PRODUCTINCUTCOLID.PRS;
             int nMOD = PRODUCTINCUTCOLID.MOD;
             int nART = PRODUCTINCUTCOLID.ART;
             int nART2 = PRODUCTINCUTCOLID.ART2;
             int nRAZ = PRODUCTINCUTCOLID.RAZ;
             int nSRT = PRODUCTINCUTCOLID.SRT;
             int nPATTERN = PRODUCTINCUTCOLID.PATTERN;

             int nCCODE = PRODUCTINCUTCOLID.CCODE;
             int nCCLOTH = PRODUCTINCUTCOLID.CCLOTH;
             int nCOTHER = PRODUCTINCUTCOLID.COTHER;
             int nCSEASON = PRODUCTINCUTCOLID.CSEASON;

             if (2 == nTypeSourceFile) {
                 nBARCODE = PRODUCTINCUTCOLID_TYPE2.BARCODE;
                 nIZD = PRODUCTINCUTCOLID_TYPE2.IZD;
                 nPRS = PRODUCTINCUTCOLID_TYPE2.PRS;
                 nMOD = PRODUCTINCUTCOLID_TYPE2.MOD;
                 nART = PRODUCTINCUTCOLID_TYPE2.ART;
                 nART2 = PRODUCTINCUTCOLID_TYPE2.ART2;
                 nRAZ = PRODUCTINCUTCOLID_TYPE2.RAZ;
                 nSRT = PRODUCTINCUTCOLID_TYPE2.SRT;
                 nPATTERN = PRODUCTINCUTCOLID_TYPE2.PATTERN;
                 nCCODE = PRODUCTINCUTCOLID_TYPE2.CCODE;
                 nCCLOTH = PRODUCTINCUTCOLID_TYPE2.CCLOTH;
                 nCOTHER = PRODUCTINCUTCOLID_TYPE2.COTHER;
                 nCSEASON = PRODUCTINCUTCOLID_TYPE2.CSEASON;
             }
             else if (3 == nTypeSourceFile) {
                 nBARCODE = PRODUCTINCUTCOLID_TYPE3.BARCODE;
                 nIZD = PRODUCTINCUTCOLID_TYPE3.IZD;
                 nPRS = PRODUCTINCUTCOLID_TYPE3.PRS;
                 nMOD = PRODUCTINCUTCOLID_TYPE3.MOD;
                 nART = PRODUCTINCUTCOLID_TYPE3.ART;
                 nART2 = PRODUCTINCUTCOLID_TYPE3.ART2;
                 nRAZ = PRODUCTINCUTCOLID_TYPE3.RAZ;
                 nSRT = PRODUCTINCUTCOLID_TYPE3.SRT;
                 nPATTERN = PRODUCTINCUTCOLID_TYPE3.PATTERN;
                 nCCODE = PRODUCTINCUTCOLID_TYPE3.CCODE;
                 nCCLOTH = PRODUCTINCUTCOLID_TYPE3.CCLOTH;
                 nCOTHER = PRODUCTINCUTCOLID_TYPE3.COTHER;
                 nCSEASON = PRODUCTINCUTCOLID_TYPE3.CSEASON;
             }             
             if (System.DBNull.Value != dr[nBARCODE]) {
                 strBARCODE = (string)dr[nBARCODE];
                 if ("2190881052620" == strBARCODE) strBARCODE = "2112207711967"; //2190881052620 и 2112207711967 синонимы                   
                 if ("2142226590090" == strBARCODE) strBARCODE = "2112226175085";
                 if ("2122740127408" == strBARCODE) strBARCODE = "2122740075198";
             }
             if (System.DBNull.Value != dr[nIZD]) strIZD = (string)dr[nIZD];
             if (System.DBNull.Value != dr[nPRS]) strPRS = (string)dr[nPRS];
             if (System.DBNull.Value != dr[nMOD]) strMOD = (string)dr[nMOD];
             if (System.DBNull.Value != dr[nART]) strART = (string)dr[nART];
             if (System.DBNull.Value != dr[nART2]) strART2 = (string)dr[nART2];
             if (System.DBNull.Value != dr[nRAZ]) strRAZ = (string)dr[nRAZ];
             if (System.DBNull.Value != dr[nSRT]) strSRT = (string)dr[nSRT];
             if (System.DBNull.Value != dr[nPATTERN]) strPATTERN = (string)dr[nPATTERN];
             if (System.DBNull.Value != dr[nCCODE]) strCCODE = (string)dr[nCCODE];
             if (System.DBNull.Value != dr[nCCLOTH]) strCCLOTH = (string)dr[nCCLOTH];
             if (System.DBNull.Value != dr[nCOTHER]) strCOTHER = (string)dr[nCOTHER];
             if (System.DBNull.Value != dr[nCSEASON]) strCSEASON = (string)dr[nCSEASON];
          }
         public static bool CheckColumnsInTable(ref DataTable dtTableSpm) {
             if (PRODUCTINCUTCOLNAME.BARCODE != dtTableSpm.Columns[PRODUCTINCUTCOLID.BARCODE].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.IZD != dtTableSpm.Columns[PRODUCTINCUTCOLID.IZD].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.PRS != dtTableSpm.Columns[PRODUCTINCUTCOLID.PRS].ColumnName) return false;

             if (PRODUCTINCUTCOLNAME.MOD != dtTableSpm.Columns[PRODUCTINCUTCOLID.MOD].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.ART != dtTableSpm.Columns[PRODUCTINCUTCOLID.ART].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.ART2 != dtTableSpm.Columns[PRODUCTINCUTCOLID.ART2].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.RAZ != dtTableSpm.Columns[PRODUCTINCUTCOLID.RAZ].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.SRT != dtTableSpm.Columns[PRODUCTINCUTCOLID.SRT].ColumnName) return false;

             if (PRODUCTINCUTCOLNAME.PATTERN != dtTableSpm.Columns[PRODUCTINCUTCOLID.PATTERN].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CCODE != dtTableSpm.Columns[PRODUCTINCUTCOLID.CCODE].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CCLOTH != dtTableSpm.Columns[PRODUCTINCUTCOLID.CCLOTH].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.COTHER != dtTableSpm.Columns[PRODUCTINCUTCOLID.COTHER].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CSEASON != dtTableSpm.Columns[PRODUCTINCUTCOLID.CSEASON].ColumnName) return false;
             //if (PRODUCTINCUTCOLNAME.DAT1 != dtTableSpm.Columns[PRODUCTINCUTCOLID.DAT1].ColumnName) return false;
             return true;
         }
         public static bool CheckColumnsInTableType2(ref DataTable dtTableSpm) {
             if (PRODUCTINCUTCOLNAME.BARCODE != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.BARCODE].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.IZD != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.IZD].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.PRS != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.PRS].ColumnName) return false;

             if (PRODUCTINCUTCOLNAME.MOD != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.MOD].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.ART != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.ART].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.ART2 != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.ART2].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.RAZ != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.RAZ].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.SRT != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.SRT].ColumnName) return false;

             if (PRODUCTINCUTCOLNAME.PATTERN != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.PATTERN].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CCODE != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.CCODE].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CCLOTH != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.CCLOTH].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.COTHER != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.COTHER].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CSEASON != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE2.CSEASON].ColumnName) return false;
             return true;
         }   
         public static bool CheckColumnsInTableType3(ref DataTable dtTableSpm) {
             if (PRODUCTINCUTCOLNAME.BARCODE != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.BARCODE].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.IZD != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.IZD].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.PRS != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.PRS].ColumnName) return false;

             if (PRODUCTINCUTCOLNAME.MOD != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.MOD].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.ART != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.ART].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.ART2 != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.ART2].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.RAZ != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.RAZ].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.SRT != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.SRT].ColumnName) return false;

             if (PRODUCTINCUTCOLNAME.PATTERN != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.PATTERN].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CCODE != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.CCODE].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CCLOTH != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.CCLOTH].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.COTHER != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.COTHER].ColumnName) return false;
             if (PRODUCTINCUTCOLNAME.CSEASON != dtTableSpm.Columns[PRODUCTINCUTCOLID_TYPE3.CSEASON].ColumnName) return false;
             return true;
         }
     }


     public struct RESTCOLNAMEBV {
         public const string BARCODE = "BARCODE";
         public const string IZD = "IZD";
         public const string PRS = "PRS";
         public const string MOD = "MOD";
         public const string ART = "ART";
         public const string RAZ = "RAZ";
         public const string SRT = "SRT";
         public const string KOL = "KOL";
         public const string CJ = "CJ";
         //public const string SM = "SM";
         public const string ART2 = "ART2";
         public const string PATTERN = "PATTERN";
         public const string CCODE = "CCODE";
         public const string CCLOTH = "CCLOTH";
         public const string COTHER = "COTHER";
         public const string CSEASON = "CSEASON";
         public const string CJ2 = "CJ2";
         public const string CR = "CR";
     }
     public struct RESTCOLIDBV {
         public const int IZD = 3;
         public const int KOD_P = 4;
         public const int PRS = 5;
         public const int MOD = 6;
         public const int ART = 7;
         public const int RAZ = 8;
         public const int SRT = 9;
         public const int KOL = 10;
         public const int CJ = 11;
         public const int SM = 12;
         public const int ART2 = 13;
         public const int PATTERN = 14;
         public const int CCODE = 15;
         public const int CCLOTH = 16;
         public const int COTHER = 17;
         public const int CSEASON = 18;
         public const int CJ2 = 21;
         public const int CR = 22;
         public const int TMARK = 29;
         public const int CCLOTH2 = 30;

        //--
        public const int IZD_SGP = 4;
         public const int PRS_SGP = 6;
         public const int MOD_SGP = 7;
         public const int ART_SGP = 8;
         public const int RAZ_SGP = 9;
         public const int SRT_SGP = 10;
         public const int KOL_SGP = 11;
         public const int CJ_SGP = 12;
         public const int ART2_SGP = 14;
         public const int PATTERN_SGP = 15;
         public const int CCODE_SGP = 16;
         public const int CCLOTH_SGP = 17;
         public const int COTHER_SGP = 18;
         public const int CSEASON_SGP = 19;
         public const int CJ2_SGP = 20;   
         //--

         public const int GTIN = 23;
         public const int KOL_KM = 24;
         public const int KOL_PRN = 25;
         public const int KOL_TOSHOP = 26;
         public const int BARCODE = 27;
         public const int IZDNAME = 28;
     }
     public class RestItemBV
     {
         public List<string> listSpm = [];

         private string strBARCODE = "";
         private string strIZD = "";
         private string strPRS = "";
         private string strMOD = "";
         private string strART = "";
         private string strRAZ = "";
         private string strSRT = "";
         private string strTMark = "";
         private string strCCLOTH2 = "";

         private int nKOL = 0;
         private readonly decimal dCJ = 0;
         //private decimal dSM = 0;
         private string strART2 = "";
         private string strPATTERN = "";
         private string strCCODE = "";
         private readonly string strCCLOTH = "";
         private readonly string strCOTHER = "";
         private readonly string strCSEASON = "";
         private readonly decimal dCJ2 = 0;
         private readonly decimal dCR = 0;

         public string GetProduct() {
             return strIZD + "#" + strPRS + "#" + strMOD + "#" + strART +
                 "#" + strRAZ + "#" + strSRT + "#" + strPATTERN + "#" + strCCODE + "#" + strCCLOTH + "#" + strCOTHER + "#" + strCSEASON;
         }
         public static bool CheckColumnsInTable(ref DataTable dtTableSpm) {
             if ("BARCODE" == dtTableSpm.Columns[3].ColumnName) {//SGP 
                 if (RESTCOLNAMEBV.IZD != dtTableSpm.Columns[RESTCOLIDBV.IZD_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.PRS != dtTableSpm.Columns[RESTCOLIDBV.PRS_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.MOD != dtTableSpm.Columns[RESTCOLIDBV.MOD_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.ART != dtTableSpm.Columns[RESTCOLIDBV.ART_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.RAZ != dtTableSpm.Columns[RESTCOLIDBV.RAZ_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.SRT != dtTableSpm.Columns[RESTCOLIDBV.SRT_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.KOL != dtTableSpm.Columns[RESTCOLIDBV.KOL_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.CJ != dtTableSpm.Columns[RESTCOLIDBV.CJ_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.ART2 != dtTableSpm.Columns[RESTCOLIDBV.ART2_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.PATTERN != dtTableSpm.Columns[RESTCOLIDBV.PATTERN_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.CCODE != dtTableSpm.Columns[RESTCOLIDBV.CCODE_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.CCLOTH != dtTableSpm.Columns[RESTCOLIDBV.CCLOTH_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.COTHER != dtTableSpm.Columns[RESTCOLIDBV.COTHER_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.CSEASON != dtTableSpm.Columns[RESTCOLIDBV.CSEASON_SGP].ColumnName) return false;
                 if (RESTCOLNAMEBV.CJ2 != dtTableSpm.Columns[RESTCOLIDBV.CJ2_SGP].ColumnName) return false;
                 return true;
             }
             if (RESTCOLNAMEBV.IZD != dtTableSpm.Columns[RESTCOLIDBV.IZD].ColumnName) return false;
             if (RESTCOLNAMEBV.PRS != dtTableSpm.Columns[RESTCOLIDBV.PRS].ColumnName) return false;
             if (RESTCOLNAMEBV.MOD != dtTableSpm.Columns[RESTCOLIDBV.MOD].ColumnName) return false;
             if (RESTCOLNAMEBV.ART != dtTableSpm.Columns[RESTCOLIDBV.ART].ColumnName) return false;
             if (RESTCOLNAMEBV.RAZ != dtTableSpm.Columns[RESTCOLIDBV.RAZ].ColumnName) return false;
             if (RESTCOLNAMEBV.SRT != dtTableSpm.Columns[RESTCOLIDBV.SRT].ColumnName) return false;        
             if (RESTCOLNAMEBV.KOL != dtTableSpm.Columns[RESTCOLIDBV.KOL].ColumnName) return false;
             if (RESTCOLNAMEBV.CJ != dtTableSpm.Columns[RESTCOLIDBV.CJ].ColumnName) return false;             
             if (RESTCOLNAMEBV.ART2 != dtTableSpm.Columns[RESTCOLIDBV.ART2].ColumnName) return false;
             if (RESTCOLNAMEBV.PATTERN != dtTableSpm.Columns[RESTCOLIDBV.PATTERN].ColumnName) return false;
             if (RESTCOLNAMEBV.CCODE != dtTableSpm.Columns[RESTCOLIDBV.CCODE].ColumnName) return false;
             if (RESTCOLNAMEBV.CCLOTH != dtTableSpm.Columns[RESTCOLIDBV.CCLOTH].ColumnName) return false;
             if (RESTCOLNAMEBV.COTHER != dtTableSpm.Columns[RESTCOLIDBV.COTHER].ColumnName) return false;
             if (RESTCOLNAMEBV.CSEASON != dtTableSpm.Columns[RESTCOLIDBV.CSEASON].ColumnName) return false;
             if (RESTCOLNAMEBV.CJ2 != dtTableSpm.Columns[RESTCOLIDBV.CJ2].ColumnName) return false;
             if (RESTCOLNAMEBV.CR != dtTableSpm.Columns[RESTCOLIDBV.CR].ColumnName) return false;
             return true;
         }
         public string GetIZDName() {
             return GetIZDName(strIZD);
         }
         public static string GetIZDName(string strIZD_In) {
             if ("00" == strIZD_In) return "ЖИЛЕТ МУЖСКОЙ";
             if ("01" == strIZD_In) return "КОСТЮМ МУЖСКОЙ";
             if ("02" == strIZD_In) return "БРЮКИ МУЖСКИЕ";
             if ("03" == strIZD_In) return "КУРТКА МУЖСКАЯ";
             if ("04" == strIZD_In) return "ПИДЖАК МУЖСКОЙ";
             if ("05" == strIZD_In) return "ЮБКА ЖЕНСКАЯ";
             if ("06" == strIZD_In) return "КОМПЛЕКТ МУЖСКОЙ";
             if ("07" == strIZD_In) return "КОСТЮМ ЖЕНСКИЙ С ЮБКОЙ";
             if ("20" == strIZD_In) return "ЖИЛЕТ ЖЕНСКИЙ";
             if ("22" == strIZD_In) return "БРЮКИ ЖЕНСКИЕ";
             if ("24" == strIZD_In) return "ЖАКЕТ ЖЕНСКИЙ";
             if ("25" == strIZD_In) return "ПЛАТЬЕ ЖЕНСКОЕ";
             if ("34" == strIZD_In) return "КОСТЮМ ЖЕНСКИЙ С БРЮКАМИ";
             if ("74" == strIZD_In) return "ПАЛЬТО ЖЕНСКОЕ";
             if ("76" == strIZD_In) return "УТЕПЛЕННОЕ ПАЛЬТО МУЖСКОЕ";
             if ("77" == strIZD_In) return "ПАЛЬТО МУЖСКОЕ";
             if ("78" == strIZD_In) return "ПОЛУПАЛЬТО";

             if ("11" == strIZD_In) return "КОСТЮМ МУЖСКОЙ";//СПОРТ СТИЛЬ

             return "???????";
         }

        public void SetBARCODE(string strBARCODEIn) { strBARCODE = strBARCODEIn; }
        public void SetTMark(string strTMarkIn) { strTMark = strTMarkIn; }
        public void SetCCLOTH2(string strCCLOTH2In) { strCCLOTH2 = strCCLOTH2In; }        
        public void SetIZD(string strVal) { strIZD = strVal; }
         public void SetPRS(string strVal) { strPRS = strVal; }
         public void SetMOD(string strVal) { strMOD = strVal; }
         public void SetART(string strVal) { strART = strVal; }
         public void SetART2(string strVal) { strART2 = strVal; }
         public void SetRAZ(string strVal) { strRAZ = strVal; }
         public void SetKOL(int nVal) { nKOL = nVal; }
         public void SetPATTERN(string strVal) { strPATTERN = strVal; }
         public void SetCCODE(string strVal) { strCCODE = strVal; }
         public void SetSRT(string strVal) { strSRT = strVal; }
               
         public string GetIZD() { return strIZD; }
         public string GetPRS() { return strPRS; }
         public string GetMOD() { return strMOD; }
         public string GetART() { return strART; }
         public string GetRAZ() { return strRAZ; }
         public int GetKOL() { return nKOL; }
         public decimal GetCJ() { return dCJ; }
         public string GetART2() { return strART2; }
         public string GetPATTERN() { return strPATTERN; }
         public string GetCCODE() { return strCCODE; }
         public string GetCCLOTH() { return strCCLOTH; }
         public string GetCOTHER() { return strCOTHER; }
         public string GetCSEASON() { return strCSEASON; }
         public decimal GetCJ2() { return dCJ2; }
         public decimal GetCR() { return dCR; }
         public string GetSRT() { return strSRT; }

        public string GetBARCODE_0() {
            return strBARCODE;
        }

        public string GetBARCODE() {
            if (13 == strBARCODE.Length)
                return strBARCODE;
            return "?" + Guid.NewGuid().ToString().Substring(0, 12);
        }
        public string GetTMark() {
            return strTMark;
        }
        public string GetCCLOTH2() {
            return this.strCCLOTH2;
        }
        
        public RestItemBV() {
         }
         public RestItemBV(DataRow dr) {
             strBARCODE = "";
             strIZD = "";
             strPRS = "";
             strMOD = "";
             strART = "";
             strRAZ = "";
             nKOL = 0;
             dCJ = 0;
             //dSM = 0;
             strART2 = "";
             strPATTERN = "";
             strCCODE = "";
             strCCLOTH = "";
             strCOTHER = "";
             strCSEASON = "";
             dCJ2 = 0;
             dCR = 0;            

             if (29 == dr.ItemArray.Length) {
                 if (System.DBNull.Value != dr[RESTCOLIDBV.IZD_SGP])
                     strIZD = (string)dr[RESTCOLIDBV.IZD_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.PRS_SGP])
                     strPRS = (string)dr[RESTCOLIDBV.PRS_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.MOD_SGP])
                     strMOD = (string)dr[RESTCOLIDBV.MOD_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.ART_SGP])
                     strART = (string)dr[RESTCOLIDBV.ART_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.RAZ_SGP])
                     strRAZ = (string)dr[RESTCOLIDBV.RAZ_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.SRT_SGP])
                     strSRT = (string)dr[RESTCOLIDBV.SRT_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.KOL_SGP])
                     nKOL = (int)dr[RESTCOLIDBV.KOL_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.CJ_SGP])
                     dCJ = (decimal)dr[RESTCOLIDBV.CJ_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.ART2_SGP])
                     strART2 = (string)dr[RESTCOLIDBV.ART2_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.PATTERN_SGP])
                     strPATTERN = (string)dr[RESTCOLIDBV.PATTERN_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.CCODE_SGP])
                     strCCODE = (string)dr[RESTCOLIDBV.CCODE_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.CCLOTH_SGP])
                     strCCLOTH = (string)dr[RESTCOLIDBV.CCLOTH_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.COTHER_SGP])
                     strCOTHER = (string)dr[RESTCOLIDBV.COTHER_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.CSEASON_SGP])
                     strCSEASON = (string)dr[RESTCOLIDBV.CSEASON_SGP];
                 if (System.DBNull.Value != dr[RESTCOLIDBV.CJ2_SGP])
                     dCJ2 = (decimal)dr[RESTCOLIDBV.CJ2_SGP];
                 return;
             }

             if (System.DBNull.Value != dr[RESTCOLIDBV.IZD])
                 strIZD = (string)dr[RESTCOLIDBV.IZD];
             if (System.DBNull.Value != dr[RESTCOLIDBV.PRS])
                 strPRS = (string)dr[RESTCOLIDBV.PRS];
             if (System.DBNull.Value != dr[RESTCOLIDBV.MOD])
                 strMOD = (string)dr[RESTCOLIDBV.MOD];
             if (System.DBNull.Value != dr[RESTCOLIDBV.ART])
                 strART = (string)dr[RESTCOLIDBV.ART];
             if (System.DBNull.Value != dr[RESTCOLIDBV.RAZ])
                 strRAZ = (string)dr[RESTCOLIDBV.RAZ];
             if (System.DBNull.Value != dr[RESTCOLIDBV.SRT])
                 strSRT = (string)dr[RESTCOLIDBV.SRT];            
             if (System.DBNull.Value != dr[RESTCOLIDBV.KOL])
                 nKOL = (int)dr[RESTCOLIDBV.KOL];
             if (System.DBNull.Value != dr[RESTCOLIDBV.CJ])
                 dCJ = (decimal)dr[RESTCOLIDBV.CJ];
             if (System.DBNull.Value != dr[RESTCOLIDBV.ART2])
                 strART2 = (string)dr[RESTCOLIDBV.ART2];
             if (System.DBNull.Value != dr[RESTCOLIDBV.PATTERN])
                 strPATTERN = (string)dr[RESTCOLIDBV.PATTERN];
             if (System.DBNull.Value != dr[RESTCOLIDBV.CCODE])
                 strCCODE = (string)dr[RESTCOLIDBV.CCODE];
             if (System.DBNull.Value != dr[RESTCOLIDBV.CCLOTH])
                 strCCLOTH = (string)dr[RESTCOLIDBV.CCLOTH];
             if (System.DBNull.Value != dr[RESTCOLIDBV.COTHER])
                 strCOTHER = (string)dr[RESTCOLIDBV.COTHER];
             if (System.DBNull.Value != dr[RESTCOLIDBV.CSEASON])
                 strCSEASON = (string)dr[RESTCOLIDBV.CSEASON];
             if (System.DBNull.Value != dr[RESTCOLIDBV.CJ2])
                 dCJ2 = (decimal)dr[RESTCOLIDBV.CJ2];
             if (System.DBNull.Value != dr[RESTCOLIDBV.CR])
                 dCR = (decimal)dr[RESTCOLIDBV.CR];
         }
         private bool _IsIdenticakWithout_KOL_AND_PRICE(ref RestItemBV ri) {
             if (strBARCODE != ri.strBARCODE) return false;
             if (strTMark != ri.strTMark) return false;
             if (strCCLOTH2 != ri.strCCLOTH2) return false;          
             if (strIZD != ri.strIZD) return false;
             if (strPRS != ri.strPRS) return false;
             if (strMOD != ri.strMOD) return false;
             if (strART != ri.strART) return false;
             if (strRAZ != ri.strRAZ) return false;
             //if (dCJ != ri.dCJ) return false;
             //if (dSM != ri.dSM) return false;
             if (strART2 != ri.strART2) return false;
             if (strPATTERN != ri.strPATTERN) return false;
             if (strCCODE != ri.strCCODE) return false;
             if (strCCLOTH != ri.strCCLOTH) return false;
             if (strCOTHER != ri.strCOTHER) return false;
             if (strCSEASON != ri.strCSEASON) return false;
             if (dCJ2 != ri.dCJ2) return false;
             //if (dCR != ri.dCR) return false;
             return true;
         }
         public bool Add(ref RestItemBV newRI) {
             if (!_IsIdenticakWithout_KOL_AND_PRICE(ref newRI))
                 return false;
             nKOL += newRI.nKOL;
             return true;
         }
     }
    public struct RESTCOLNAME {
        public const string BARCODE = "BARCODE";
        public const string IZD = "IZD";
        public const string MOD = "MOD";
        public const string ART = "ART";
        public const string RAZ = "RAZ";
        public const string KOL = "KOL";
        public const string CJ = "CJ";
        //public const string SM = "SM";
        public const string ART2 = "ART2";
        public const string PATTERN = "PATTERN";
        public const string CCODE = "CCODE";
        public const string CCLOTH = "CCLOTH";
        public const string COTHER = "COTHER";
        public const string CSEASON = "CSEASON";
        public const string CJ2 = "CJ2";
        public const string CR = "CR";
    }
    public struct RESTCOLID {
        public const int BARCODE = 3;
        public const int IZD = 4;
        public const int MOD = 7;
        public const int ART = 8;
        public const int RAZ = 9;
        public const int KOL = 11;
        public const int CJ = 12;
        public const int SM = 13;
        public const int ART2 = 14;
        public const int PATTERN = 15;
        public const int CCODE = 16;
        public const int CCLOTH = 17;
        public const int COTHER = 18;
        public const int CSEASON = 19;
        public const int CJ2 = 20;
        public const int CR = 26;
        public const int GTIN = 27;
        public const int KOL_KM = 28;
        public const int KOL_PRN = 29;
        public const int KOL_TOSHOP = 30;
    }

    public class RestItem {
        public List<string> listSpm =[];

        private readonly string strBARCODE = "";
        private string strIZD = "";
        private string strMOD = "";
        private string strART = "";
        private string strRAZ = "";
        private int nKOL = 0;
        private readonly decimal dCJ = 0;
        //private decimal dSM = 0;
        private string strART2 = "";
        private string strPATTERN = "";
        private string strCCODE = "";
        private string strCCLOTH = "";
        private readonly string strCOTHER = "";
        private string strCSEASON = "";
        private readonly decimal dCJ2 = 0;
        private readonly decimal dCR = 0;
     
        public static bool CheckColumnsInTable(ref DataTable dtTableSpm) {
            if (RESTCOLNAME.BARCODE != dtTableSpm.Columns[RESTCOLID.BARCODE].ColumnName) return false;
            if (RESTCOLNAME.IZD != dtTableSpm.Columns[RESTCOLID.IZD].ColumnName) return false;
            if (RESTCOLNAME.MOD != dtTableSpm.Columns[RESTCOLID.MOD].ColumnName) return false;
            if (RESTCOLNAME.ART != dtTableSpm.Columns[RESTCOLID.ART].ColumnName) return false;
            if (RESTCOLNAME.RAZ != dtTableSpm.Columns[RESTCOLID.RAZ].ColumnName) return false;
            if (RESTCOLNAME.KOL != dtTableSpm.Columns[RESTCOLID.KOL].ColumnName) return false;
            if (RESTCOLNAME.CJ != dtTableSpm.Columns[RESTCOLID.CJ].ColumnName) return false;
            //if (RESTCOLNAME.SM != dtTableSpm.Columns[RESTCOLID.SM].ColumnName) return false;
            if (RESTCOLNAME.ART2 != dtTableSpm.Columns[RESTCOLID.ART2].ColumnName) return false;
            if (RESTCOLNAME.PATTERN != dtTableSpm.Columns[RESTCOLID.PATTERN].ColumnName) return false;
            if (RESTCOLNAME.CCODE != dtTableSpm.Columns[RESTCOLID.CCODE].ColumnName) return false;
            if (RESTCOLNAME.CCLOTH != dtTableSpm.Columns[RESTCOLID.CCLOTH].ColumnName) return false;
            if (RESTCOLNAME.COTHER != dtTableSpm.Columns[RESTCOLID.COTHER].ColumnName) return false;
            if (RESTCOLNAME.CSEASON != dtTableSpm.Columns[RESTCOLID.CSEASON].ColumnName) return false;
            if (RESTCOLNAME.CJ2 != dtTableSpm.Columns[RESTCOLID.CJ2].ColumnName) return false;
            if (RESTCOLNAME.CR != dtTableSpm.Columns[RESTCOLID.CR].ColumnName) return false;
            return true;
        }

        public void SetIZD(string strVal) { strIZD = strVal; }
        public void SetMOD(string strVal) { strMOD = strVal; }
        public void SetART(string strVal) { strART = strVal; }
        public void SetRAZ(string strVal) { strRAZ = strVal; }
        public void SetKOL(int nVal) { nKOL = nVal; }
        public void SetART2(string strVal) { strART2 = strVal; }
        public void SetPATTERN(string strVal) { strPATTERN = strVal; }
        public void SetCCODE(string strVal) { strCCODE = strVal; }
        public void SetCCLOTH(string strVal) { strCCLOTH = strVal; }
        public void SetCSEASON(string strVal) { strCSEASON = strVal; }

        public string GetIzdByMOD() {
            if ("СОРОЧКА" == strMOD.ToUpper()) return "1";
            if ("ГАЛСТУК" == strMOD.ToUpper()) return "6";
            if ("ДЖЕМПЕР" == strMOD.ToUpper()) return "5";
            if ("ЖАКЕТ" == strMOD.ToUpper()) return "5";
            if ("РУБАШКА" == strMOD.ToUpper()) return "9";
            if ("ЖИЛЕТ" == strMOD.ToUpper()) return "21";
            if ("СВИТЕР" == strMOD.ToUpper()) return "37";
            if ("ПИДЖАК" == strMOD.ToUpper()) return "57";
            if ("КАРДИГАН" == strMOD.ToUpper()) return "71";
            if ("ПОЛО" == strMOD.ToUpper()) return "67";
            if ("ПАЛЬТО" == strMOD.ToUpper()) return "31";


            return "";
        }
        public string GetBARCODE() { return strBARCODE; }
        public string GetIZD() { return strIZD; }
        public string GetMOD() { return strMOD; }
        public string GetART() { return strART; }
        public string GetRAZ() { return strRAZ; }
        public int GetKOL() { return nKOL; }
        public decimal GetCJ() { return dCJ; }
        public string GetART2() { return strART2; }
        public string GetPATTERN() { return strPATTERN; }
        public string GetCCODE() { return strCCODE; }
        public string GetCCLOTH() { return strCCLOTH; }
        public string GetCOTHER() { return strCOTHER; }
        public string GetCSEASON() { return strCSEASON; }
        public decimal GetCJ2() { return dCJ2; }
        public decimal GetCR() { return dCR; }

        public RestItem() { 
        }
        public RestItem(DataRow dr) {
            strBARCODE = "";
            strIZD = "";
            strMOD = "";
            strART = "";
            strRAZ = "";
            nKOL = 0;
            dCJ = 0;
            //dSM = 0;
            strART2 = "";
            strPATTERN = "";
            strCCODE = "";
            strCCLOTH = "";
            strCOTHER = "";
            strCSEASON = "";
            dCJ2 = 0;
            dCR = 0;

            if (System.DBNull.Value != dr[RESTCOLID.BARCODE])
                strBARCODE = (string)dr[RESTCOLID.BARCODE];
            if (System.DBNull.Value != dr[RESTCOLID.IZD])
                strIZD = (string)dr[RESTCOLID.IZD];
            if (System.DBNull.Value != dr[RESTCOLID.MOD])
                strMOD = (string)dr[RESTCOLID.MOD];
            if (System.DBNull.Value != dr[RESTCOLID.ART])
                strART = (string)dr[RESTCOLID.ART];
            if (System.DBNull.Value != dr[RESTCOLID.RAZ])
                strRAZ = (string)dr[RESTCOLID.RAZ];
            if (System.DBNull.Value != dr[RESTCOLID.KOL])
                nKOL = (int)dr[RESTCOLID.KOL];
            if (System.DBNull.Value != dr[RESTCOLID.CJ])
                dCJ = (decimal)dr[RESTCOLID.CJ];
            //if (System.DBNull.Value != dr[RESTCOLID.SM])
            //    dSM = (decimal)dr[RESTCOLID.SM];
            if (System.DBNull.Value != dr[RESTCOLID.ART2])
                strART2 = (string)dr[RESTCOLID.ART2];
            if (System.DBNull.Value != dr[RESTCOLID.PATTERN])
                strPATTERN = (string)dr[RESTCOLID.PATTERN];
            if (System.DBNull.Value != dr[RESTCOLID.CCODE])
                strCCODE = (string)dr[RESTCOLID.CCODE];
            if (System.DBNull.Value != dr[RESTCOLID.CCLOTH])
                strCCLOTH = (string)dr[RESTCOLID.CCLOTH];
            if (System.DBNull.Value != dr[RESTCOLID.COTHER])
                strCOTHER = (string)dr[RESTCOLID.COTHER];
            if (System.DBNull.Value != dr[RESTCOLID.CSEASON])
                strCSEASON = (string)dr[RESTCOLID.CSEASON];
            if (System.DBNull.Value != dr[RESTCOLID.CJ2])
                dCJ2 = (decimal)dr[RESTCOLID.CJ2];
            if (System.DBNull.Value != dr[RESTCOLID.CR])
                dCR = (decimal)dr[RESTCOLID.CR];            
        }
        private bool _IsIdenticakWithout_KOL_AND_PRICE(ref RestItem ri) {
            if (strBARCODE != ri.strBARCODE) return false;
            if (strIZD != ri.strIZD) return false;
            if (strMOD != ri.strMOD) return false;
            if (strART != ri.strART) return false;
            if (strRAZ != ri.strRAZ) return false;
            //if (dCJ != ri.dCJ) return false;
            //if (dSM != ri.dSM) return false;
            if (strART2 != ri.strART2) return false;
            if (strPATTERN != ri.strPATTERN) return false;
            if (strCCODE != ri.strCCODE) return false;
            if (strCCLOTH != ri.strCCLOTH) return false;
            if (strCOTHER != ri.strCOTHER) return false;
            if (strCSEASON != ri.strCSEASON) return false;
            if (dCJ2 != ri.dCJ2) return false;
            if (dCR != ri.dCR) return false;
            return true;
        }       
        public bool Add(ref RestItem newRI) {
            if (!_IsIdenticakWithout_KOL_AND_PRICE(ref newRI))
                return false;
            nKOL += newRI.nKOL;
            return true;
        }
    }


    public class ToExcelItem {
        public string strColumn5_ProductName = "";
        public string strColumn6_Country = "ОТСУТСТВУЕТ";
        public string strColumn7_Country = "";
        public string strColumn8_INN = "";
        public string strColumn9_IZGOTOVITEL = "";
        public string strColumn10_SortProduct = "";
        public string strColumn12_TNVED = "";
        public string strColumn14_Razmer = "";
        public string strColumn15_Rost = "";
        public string strColumn16_Cvet = "";
        public string strColumn17_Mod = "";
        public string strColumn19_Sostav = "";

        public string strColumn26_Komplect = "Нет";
        public string strColumn27_CountInKomplect = "1";
        public string strColumn28_DescriptionKomplect = "";
    }
    public class FromExcelItem {
        public long llColumn2_GTIN = 0;
        public string strColumn5_ProductName = "";
        public long llColumn12_TNVED = 0;
    } 
}
