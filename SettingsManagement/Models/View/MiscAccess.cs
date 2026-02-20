namespace SettingsManagement.Models.View
{
    public class MiscAccess
    {
        public int Id { get; set; }                      // Primary key
        public Guid UserId { get; set; }                 // ✅ Matches route/user ID (GUID)
        public int MiscSettingId { get; set; }      // ✅ Link to InventorySetting
        public bool View { get; set; }
        public bool Add { get; set; }
        public bool Modify { get; set; }
        public bool Delete { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
