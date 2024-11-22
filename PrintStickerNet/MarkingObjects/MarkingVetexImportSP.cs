using System.Text;

namespace PrintSticker.MarkingObjects {
    internal class MarkingVetexImportSP : MarkingSP {

        public MarkingVetexImportSP() : base() {
            _strPrefixEan13 = "23";
            _strSettingsID = "01";
            _MARKINGTYPES = MARKINGTYPES.IMPORT_SOPUTKA;
        }
        protected override string _GetShopPrefix() { return "004"; }
        public override string GetZplName() { return "sticker_004_IMPORT_SP.zpl"; }//printer ZT230
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderImport.txt"; }
    }


}
