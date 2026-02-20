using System;

namespace MasterService.Models
{
    public class BrandMaster
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
