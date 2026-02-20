using Microsoft.EntityFrameworkCore;
using SettingsManagement.Data;
using SettingsManagement.DTOs;
using SettingsManagement.Models.View;
using SettingsManagement.Services.Interface;

namespace SettingsManagement.Services.Implementation
{
    public class SettingService : ISettingService
    {
        private readonly SettingsDbContext _context;

        public SettingService(SettingsDbContext context)
        {
            _context = context;
        }

        public async Task<object?> GetSettingsByModuleAsync(string module)
        {
            return module.ToLower() switch
            {
                "sale" => await _context.SaleSetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                "purchase" => await _context.PurchaseSetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                "inventory" => await _context.InventorySetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                "staff" => await _context.StaffSetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                "account" => await _context.AccountSetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                "report" => await _context.ReportSetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                "master" => await _context.MasterSetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                "misc" => await _context.MiscSetting.Select(s => new { id = s.Id, name = s.SettingName }).ToListAsync(),
                _ => null
            };
        }

        public async Task<SettingAccess> GetUserAccessAsync(Guid userId)
        {
            var sale = await _context.SaleAccess.Where(x => x.UserId == userId).ToListAsync();
            var purchase = await _context.PurchaseAccess.Where(x => x.UserId == userId).ToListAsync();
            var inventory = await _context.InventoryAccess.Where(x => x.UserId == userId).ToListAsync();
            var staff = await _context.StaffAccess.Where(x => x.UserId == userId).ToListAsync();
            var account = await _context.AccountAccess.Where(x => x.UserId == userId).ToListAsync();
            var report = await _context.ReportAccess.Where(x => x.UserId == userId).ToListAsync();
            var master = await _context.MasterAccess.Where(x => x.UserId == userId).ToListAsync();
            var misc = await _context.MiscAccess.Where(x => x.UserId == userId).ToListAsync();

            return new SettingAccess
            {
                SaleAccess = sale.Select(ToDto).ToList(),
                PurchaseAccess = purchase.Select(ToDto).ToList(),
                InventoryAccess = inventory.Select(ToDto).ToList(),
                StaffAccess = staff.Select(ToDto).ToList(),
                AccountAccess = account.Select(ToDto).ToList(),
                ReportAccess = report.Select(ToDto).ToList(),
                MasterAccess = master.Select(ToDto).ToList(),
                MiscAccess = misc.Select(ToDto).ToList()
            };

            static AccessDto ToDto(dynamic x) => new AccessDto
            {
                Id = x.Id,
                UserId = x.UserId,
                SettingId = x.GetType().GetProperty("SaleSettingId")?.GetValue(x) ??
                            x.GetType().GetProperty("PurchaseSettingId")?.GetValue(x) ??
                            x.GetType().GetProperty("InventorySettingId")?.GetValue(x) ??
                            x.GetType().GetProperty("StaffSettingId")?.GetValue(x) ??
                            x.GetType().GetProperty("AccountSettingId")?.GetValue(x) ??
                            x.GetType().GetProperty("ReportSettingId")?.GetValue(x) ??
                            x.GetType().GetProperty("MasterSettingId")?.GetValue(x) ??
                            x.GetType().GetProperty("MiscSettingId")?.GetValue(x),
                View = x.View,
                Add = x.Add,
                Modify = x.Modify,
                Delete = x.Delete,
                CreatedAt = x.CreatedAt,
                LastUpdatedAt = x.LastUpdatedAt
            };
        }

        public async Task<bool> SaveUserAccessAsync(Guid userId, SettingAccess settingAccess)
        {
            try
            {
                // 1. SALE
                if (settingAccess.SaleAccess != null)
                {
                    var existing = await _context.SaleAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.SaleAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.SaleSettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.SaleAccess.Add(new Models.View.SaleAccess
                            {
                                UserId = dto.UserId,
                                SaleSettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                // 2. PURCHASE
                if (settingAccess.PurchaseAccess != null)
                {
                    var existing = await _context.PurchaseAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.PurchaseAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.PurchaseSettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.PurchaseAccess.Add(new Models.View.PurchaseAccess
                            {
                                UserId = dto.UserId,
                                PurchaseSettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                // 3. INVENTORY
                if (settingAccess.InventoryAccess != null)
                {
                    var existing = await _context.InventoryAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.InventoryAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.InventorySettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.InventoryAccess.Add(new Models.View.InventoryAccess
                            {
                                UserId = dto.UserId,
                                InventorySettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                // 4. STAFF
                if (settingAccess.StaffAccess != null)
                {
                    var existing = await _context.StaffAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.StaffAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.StaffSettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.StaffAccess.Add(new Models.View.StaffAccess
                            {
                                UserId = dto.UserId,
                                StaffSettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                // 5. ACCOUNT
                if (settingAccess.AccountAccess != null)
                {
                    var existing = await _context.AccountAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.AccountAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.AccountSettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.AccountAccess.Add(new Models.View.AccountAccess
                            {
                                UserId = dto.UserId,
                                AccountSettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                // 6. REPORT
                if (settingAccess.ReportAccess != null)
                {
                    var existing = await _context.ReportAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.ReportAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.ReportSettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.ReportAccess.Add(new Models.View.ReportAccess
                            {
                                UserId = dto.UserId,
                                ReportSettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                // 7. MASTER
                if (settingAccess.MasterAccess != null)
                {
                    var existing = await _context.MasterAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.MasterAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.MasterSettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.MasterAccess.Add(new Models.View.MasterAccess
                            {
                                UserId = dto.UserId,
                                MasterSettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                // 8. MISC
                if (settingAccess.MiscAccess != null)
                {
                    var existing = await _context.MiscAccess.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var dto in settingAccess.MiscAccess)
                    {
                        var match = existing.FirstOrDefault(x => x.MiscSettingId == dto.SettingId);
                        if (match != null)
                        {
                            match.View = dto.View;
                            match.Add = dto.Add;
                            match.Modify = dto.Modify;
                            match.Delete = dto.Delete;
                            match.LastUpdatedAt = dto.LastUpdatedAt;
                        }
                        else
                        {
                            _context.MiscAccess.Add(new Models.View.MiscAccess
                            {
                                UserId = dto.UserId,
                                MiscSettingId = dto.SettingId,
                                View = dto.View,
                                Add = dto.Add,
                                Modify = dto.Modify,
                                Delete = dto.Delete,
                                CreatedAt = dto.CreatedAt,
                                LastUpdatedAt = dto.LastUpdatedAt
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
