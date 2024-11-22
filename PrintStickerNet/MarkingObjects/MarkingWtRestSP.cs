using System.Text;

namespace PrintSticker {
    internal class MarkingWtRestSP : MarkingSP {
        public MarkingWtRestSP() : base() {
            _strPrefixEan13 = "24";
            _strSettingsID = "02";
            _MARKINGTYPES = MARKINGTYPES.REST_SOPUTKA;
        }
        protected override string _GetShopPrefix() { return "010"; }
        public override string GetZplName() { return "sticker_010_020_REMAINS_SP.zpl"; }//printer ZT230

        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }

        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderM.txt"; }

    }
}
