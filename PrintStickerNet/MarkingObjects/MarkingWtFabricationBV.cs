using System.Text;
namespace PrintSticker.MarkingObjects {
    internal class MarkingWtFabricationBV : MarkingBV {
        public MarkingWtFabricationBV() : base() {
            _strPrefixEan13 = "30";
            _strSettingsID = "02";
            _MARKINGTYPES = MARKINGTYPES.FABR_BV;
        }
        protected override string _GetShopPrefix() { return "012"; }
        public override string GetZplName() { return "sticker_002_PRODUCTION_BV.zpl"; }//printer ZT230
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderFab.txt"; }
        public override Encoding GetStickerEncoding() { return Encoding.UTF8; }
    }
}
