using System;

namespace MasterService.Models
{
    public class GroupMaster
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? HSNCode { get; set; }

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
