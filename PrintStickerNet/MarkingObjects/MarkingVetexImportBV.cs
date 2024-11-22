using System.Text;

namespace PrintSticker.MarkingObjects {
    internal class MarkingVetexImportBV : MarkingBV {

        public MarkingVetexImportBV() : base() {
            _strPrefixEan13 = "22";
            _strSettingsID = "01";
            _MARKINGTYPES = MARKINGTYPES.IMPORT_BV;
        }
        protected override string _GetShopPrefix() { return "003"; }
        public override string GetZplName() { return "sticker_003_IMPORT_BV.zpl"; }//printer ZT230
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderImport.txt"; }
    }
}
