using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterService.Models
{
    public class ReceipeMaster
    {
        public int RecipeId { get; set; }
        public int ProductId { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public ProductMaster Product { get; set; } = default!;
    }
}
