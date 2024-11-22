using System.Text;

namespace PrintSticker.MarkingObjects {
    internal class MarkingVetexFabricationBV : MarkingBV {
        public MarkingVetexFabricationBV() : base() {
            _strPrefixEan13 = "21";
            _strSettingsID = "01";
            _MARKINGTYPES = MARKINGTYPES.FABR_BV;
        }
        protected override string _GetShopPrefix() { return "002"; }
        public override string GetZplName() { return "sticker_002_PRODUCTION_BV.zpl"; }//printer ZT230
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderFab.txt"; }
        public override Encoding GetStickerEncoding() { return Encoding.UTF8;}
    }
}
