namespace SettingsManagement.Models.Master
{
    public class MiscSetting
    {
        public int Id { get; set; } = default;

        public string SettingName { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
