using SettingsManagement.Models.View;

namespace SettingsManagement.Services.Interface
{
    public interface ISettingService
    {
        Task<object?> GetSettingsByModuleAsync(string module);
        Task<bool> SaveUserAccessAsync(Guid userId, SettingAccess settingAccess);
        Task<SettingAccess> GetUserAccessAsync(Guid userId);
    }
}
