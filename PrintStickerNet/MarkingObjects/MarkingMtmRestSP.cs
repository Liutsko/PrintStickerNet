using System;
using System.Text;


namespace PrintSticker
{
    class MarkingMtmRestSP : MarkingSP {      
        public MarkingMtmRestSP() : base() {
            _strPrefixEan13 = "25";
            _strSettingsID = "03";
            _MARKINGTYPES = MARKINGTYPES.REST_SOPUTKA;
        }
        protected override string _GetShopPrefix() { return "020"; }
        public override string GetZplName() { return "sticker_010_020_REMAINS_SP.zpl"; }//printer ZT230
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }

        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderM.txt"; }

        protected override string GetCvetSP(string strCvet) {  //DBCOLOR.DBF
            if (!int.TryParse(strCvet, out _))
                return strCvet;            
            return base.GetCvetSP(strCvet);
        }       
    }
}
