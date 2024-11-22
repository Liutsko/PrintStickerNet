using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSticker {
    internal class MarkingVetexRestBV : MarkingBV {

        public MarkingVetexRestBV() : base() {
            _strPrefixEan13 = "20";
            _strSettingsID = "01";
            _MARKINGTYPES = MARKINGTYPES.REST_BV;
        }
        protected override string _GetShopPrefix() { return "001"; }
        public override string GetZplName() { return "sticker_001_011_REMAINS_BV.zpl"; }//printer ZT230
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderM.txt"; }
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
    }

}
