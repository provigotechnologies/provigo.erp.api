namespace SettingsManagement.DTOs
{
    public class BaseAccessDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int SettingId { get; set; }
        public bool View { get; set; }
        public bool Add { get; set; }
        public bool Modify { get; set; }
        public bool Delete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
