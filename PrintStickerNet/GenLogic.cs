using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace PrintSticker
{
    public enum PRODUCT : int {
        PR_BV = 0, //штрихкод на стикере это внутренний штрихкод        
        PR_SOPUTKA = 1 //штрихкод на стикере это внутренний штрихкод
    }

    public enum EAN13_TYPE : int {
        INTERNAL = 0, //штрихкод на стикере это внутренний штрихкод        
        GTIN = 9 //штрихкод на стикере GTIN
    }
    public class KMCOUNT {
        public int nKOL_KM_ALL = 0;
        public int nKOL_KM_PRN = 0;
        public KMCOUNT() { 
        }
        public KMCOUNT(int nCol1, int nCol2) {
            nKOL_KM_ALL = nCol1;
            nKOL_KM_PRN = nCol2;
        }
    }   
    public class GenLogic
    {
        public static string GetCurDir() {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }
        public static string CopyToArhive(string strSrc) {
            if (!File.Exists(strSrc))
                return "";
            try {
                string strFileName = Path.GetFileName(strSrc);// GetFileNameWithoutExtension(strSrc);
                string strCurDir = GetCurDir() + "\\tmpArhive\\" + strFileName + "\\";
                Directory.CreateDirectory(strCurDir);
                string strDest = strCurDir + Path.GetFileNameWithoutExtension(strSrc) + "_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + Path.GetExtension(strSrc);
                File.Copy(strSrc, strDest, true);
                //Trace.WriteLine("Копируем в архив перед изменением : " + strSrc + " to " + strDest);
                return strDest;
            }
            catch (Exception ex) {
                Trace.WriteLine("Ошибка в _CopyToArhive: " + ex.Message);
                return "";
            }
            //return "";
        }
    }
}
