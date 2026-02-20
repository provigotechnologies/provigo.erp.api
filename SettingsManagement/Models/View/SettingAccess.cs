using SettingsManagement.DTOs;

namespace SettingsManagement.Models.View
{
    public class SettingAccess
    {
        public List<AccessDto>? SaleAccess { get; set; }
        public List<AccessDto>? PurchaseAccess { get; set; }
        public List<AccessDto>? InventoryAccess { get; set; }
        public List<AccessDto>? StaffAccess { get; set; }
        public List<AccessDto>? AccountAccess { get; set; }
        public List<AccessDto>? ReportAccess { get; set; }
        public List<AccessDto>? MasterAccess { get; set; }
        public List<AccessDto>? MiscAccess { get; set; }
    }

}
