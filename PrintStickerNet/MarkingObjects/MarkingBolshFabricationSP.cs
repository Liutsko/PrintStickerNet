using System.Text;

namespace PrintSticker.MarkingObjects {
    internal class MarkingBolshFabricationSP : MarkingSP {
        public MarkingBolshFabricationSP() : base() {
            _strPrefixEan13 = "29";
            _strSettingsID = "06";
            _MARKINGTYPES = MARKINGTYPES.FABR_SP;
        }
        protected override string _GetShopPrefix() { return "600"; }
        protected override string _GetFileNameCreateOrderFM() { return "CreateOrderFab.txt"; }
        
        public override string GetZplName() { return "sticker_SP_01.zpl"; } //printer S4M
        public override Encoding GetStickerEncoding() { return Encoding.GetEncoding(866); }

        protected override EAN13_TYPE _GetEan13Type() { return EAN13_TYPE.INTERNAL; }

    }
}
