using System.Text;

namespace PrintSticker {
    internal class MarkingWtRestBV : MarkingBV {

        public MarkingWtRestBV() : base() {
            _strPrefixEan13 = "26";
            _strSettingsID = "02";
            _MARKINGTYPES = MARKINGTYPES.REST_BV;
        }
        protected override string _GetShopPrefix() { return "011"; }
        public override string GetZplName() { return "sticker_001_011_REMAINS_BV.zpl"; }//printer ZT230
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderM.txt"; }
    }
}
