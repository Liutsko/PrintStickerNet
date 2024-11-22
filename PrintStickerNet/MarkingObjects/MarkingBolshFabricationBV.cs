using System.Text;

namespace PrintSticker.MarkingObjects {
    internal class MarkingBolshFabricationBV : MarkingBV {

        public MarkingBolshFabricationBV() : base() {
            _strPrefixEan13 = "27";
            _strSettingsID = "04";
            _MARKINGTYPES = MARKINGTYPES.FABR_BV;
        }
        protected override string _GetShopPrefix() { return "500"; }
        public override string GetZplName() { return "sticker_BV_01.zpl"; } //printer S4M
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderFab.txt"; }
        public override Encoding GetStickerEncoding() { return Encoding.GetEncoding(866); }
        protected override EAN13_TYPE _GetEan13Type() { return EAN13_TYPE.INTERNAL; }

        protected override bool IsChangeBarcode() { return true; }

    }
}
