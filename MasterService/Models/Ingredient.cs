using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterService.Models
{
    public class Ingredient
    {
        public int IngId { get; set; }
        public int RecipeId { get; set; }
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = string.Empty;

        [ForeignKey("ReceipeId")]
        public ReceipeMaster Receipe { get; set; } = default!;

        [ForeignKey("ProductId")]
        public ProductMaster Product { get; set; } = default!;

        [ForeignKey("UnitId")]
        public UnitMaster Unit { get; set; } = default!;
    }
}
