using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSticker.MarkingObjects {
    internal class MarkingMtImportSP : MarkingSP {

        public MarkingMtImportSP() : base() {
            _strPrefixEan13 = "34";
            _strSettingsID = "03";
            _MARKINGTYPES = MARKINGTYPES.IMPORT_SOPUTKA;
        }
        protected override string _GetShopPrefix() { return "023"; }
        public override string GetZplName() { return "sticker_004_IMPORT_SP.zpl"; }//printer ZT230
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderImport.txt"; }
    }
}
