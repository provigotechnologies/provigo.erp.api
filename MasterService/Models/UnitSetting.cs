namespace MasterService.Models
{
    public class UnitSetting
    {
        public int UnitSettingId { get; set; }
        public string BaseUnit { get; set; } = string.Empty;
        public string SecondaryUnit { get; set; } = string.Empty;
        public decimal ConversionFactor { get; set; }
        public string PriceType { get; set; } = "Fixed";
        public decimal FixedPrice { get; set; }
        public decimal? MarginAmount { get; set; }
        public decimal? MarginPercent { get; set; }
        public decimal? MRP { get; set; }
        public decimal? MinSalePrice { get; set; }
        public int IsDefault { get; set; } = 0;
    }
}
