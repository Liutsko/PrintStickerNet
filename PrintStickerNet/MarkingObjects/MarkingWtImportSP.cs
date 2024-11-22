using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSticker.MarkingObjects {
    internal class MarkingWtImportSP : MarkingSP {

        public MarkingWtImportSP() : base() {
            _strPrefixEan13 = "31";
            _strSettingsID = "02";
            _MARKINGTYPES = MARKINGTYPES.IMPORT_SOPUTKA;
        }
        protected override string _GetShopPrefix() { return "014"; }
        public override string GetZplName() { return "sticker_004_IMPORT_SP.zpl"; }//printer ZT230
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderImport.txt"; }
    }
}
