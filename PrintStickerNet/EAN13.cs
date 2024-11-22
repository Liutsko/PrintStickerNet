using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSticker {
    internal class EAN13 {

        public static bool GetPostfix(string strNewBarcode, ref string strPostfix) {
            if (12 != strNewBarcode.Length)
                return false;
            int n1 = Convert.ToInt32(strNewBarcode[1]) - 48;
            int n3 = Convert.ToInt32(strNewBarcode[3]) - 48;
            int n5 = Convert.ToInt32(strNewBarcode[5]) - 48;
            int n7 = Convert.ToInt32(strNewBarcode[7]) - 48;
            int n9 = Convert.ToInt32(strNewBarcode[9]) - 48;
            int n11 = Convert.ToInt32(strNewBarcode[11]) - 48;

            int n0 = Convert.ToInt32(strNewBarcode[0]) - 48;
            int n2 = Convert.ToInt32(strNewBarcode[2]) - 48;
            int n4 = Convert.ToInt32(strNewBarcode[4]) - 48;
            int n6 = Convert.ToInt32(strNewBarcode[6]) - 48;
            int n8 = Convert.ToInt32(strNewBarcode[8]) - 48;
            int n10 = Convert.ToInt32(strNewBarcode[10]) - 48;

            int Rez1 = 3 * (n1 + n3 + n5 + n7 + n9 + n11);
            int Rez2 = n0 + n2 + n4 + n6 + n8 + n10;
            int Rez3 = Rez1 + Rez2;
            int Rest = Rez3 % 10;
            if (0 == Rest)
                strPostfix = "0";
            else
                strPostfix = (10 - Rest).ToString();

            return true;
        }

    }
}
