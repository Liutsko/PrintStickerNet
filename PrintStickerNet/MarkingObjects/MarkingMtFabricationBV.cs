using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSticker.MarkingObjects {
    internal class MarkingMtFabricationBV : MarkingBV {
        public MarkingMtFabricationBV() : base() {
            _strPrefixEan13 = "32";
            _strSettingsID = "03";
            _MARKINGTYPES = MARKINGTYPES.FABR_BV;
        }
        protected override string _GetShopPrefix() { return "021"; }
        public override string GetZplName() { return "sticker_002_PRODUCTION_BV.zpl"; }//printer ZT230
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderFab.txt"; }
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
    }
}
