using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSticker.MarkingObjects {
    internal class MarkingMtImportBV : MarkingBV {

        public MarkingMtImportBV() : base() {
            _strPrefixEan13 = "33";
            _strSettingsID = "03";
            _MARKINGTYPES = MARKINGTYPES.IMPORT_BV;
        }
        protected override string _GetShopPrefix() { return "022"; }
        public override string GetZplName() { return "sticker_003_IMPORT_BV.zpl"; }//printer ZT230
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderImport.txt"; }
    }
}
