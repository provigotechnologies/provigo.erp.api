using System;

namespace MasterService.Models
{
    public class UnitMaster
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
