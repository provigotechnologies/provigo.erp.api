using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PurchaseService.Models;

namespace MasterService.Models
{
    public class ProductMaster
    {
        [Key]
        public int ProductId { get; set; }

        // Foreign Keys
        public int GroupId { get; set; }
        public int? BrandId { get; set; }
        public int UnitId { get; set; }
        public int UnitSettingId { get; set; }
        public int ProductTypeId { get; set; }
        public int? LocationId { get; set; }
        public int? SupplierId { get; set; }
        public int? KitchenId { get; set; }

        // Basic Info
        public string? DesignNo { get; set; }
        public string? Colour { get; set; }
        public string? Size { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string PrintName { get; set; } = string.Empty;

        // Pricing
        public decimal? PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal? MinServiceCharge { get; set; }
        public decimal? MRP { get; set; }

        // Stock Info
        public int? OpeningStock { get; set; }
        public decimal? OpeningStockValue { get; set; }

        // Tax & Discount
        public int? HSN_SAC_Code { get; set; }
        public decimal? SaleDiscount { get; set; }
        public int? LowLevelLimit { get; set; }

        // Warranty
        public int? WarrantyPeriod { get; set; }
        public string WarrantyUnit { get; set; } = "Years";

        // Identifier
        public string IdentifierType { get; set; } = "SerialNo";
        public string? IdentifierValue { get; set; }

        // Description
        public string? ProductDescription { get; set; }

        // Flags
        public bool PrintDescription { get; set; } = false;
        public bool OneClickSale { get; set; } = false;
        public bool EnableTracking { get; set; } = false;
        public bool PrintSerialNo { get; set; } = false;
        public bool NotForSale { get; set; } = false;

        // Status & Audit
        public string ProductStatus { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("GroupId")]
        public GroupMaster Group { get; set; } = default!;

        [ForeignKey("BrandId")]
        public BrandMaster? Brand { get; set; }

        [ForeignKey("UnitId")]
        public UnitMaster Unit { get; set; } = default!;

        [ForeignKey("UnitSettingId")]
        public UnitSetting UnitSetting { get; set; } = default!;

        [ForeignKey("ProductTypeId")]
        public ProductType ProductType { get; set; } = default!;

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        [ForeignKey("KitchenId")]
        public KitchenMaster? Kitchen { get; set; }

        [ForeignKey("LocationId")]
        public LocationMaster? Location { get; set; }
    }
}
