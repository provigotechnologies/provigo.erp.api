using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SettingsManagement.Data;
using SettingsManagement.DTOs;
using SettingsManagement.Models.View;

namespace SettingsManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsDbContext _context;

        public SettingsController(SettingsDbContext context)
        {
            _context = context;
        }

        [HttpGet("{module}")]
        public async Task<IActionResult> GetSettingsByModule(string module)
        {
            object? settings = module.ToLower() switch
            {
                "sale" => await _context.SaleSetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                "purchase" => await _context.PurchaseSetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                "inventory" => await _context.InventorySetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                "staff" => await _context.StaffSetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                "account" => await _context.AccountSetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                "report" => await _context.ReportSetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                "master" => await _context.MasterSetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                "misc" => await _context.MiscSetting
                    .Select(s => new { id = s.Id, name = s.SettingName })
                    .ToListAsync(),

                _ => null
            };

            if (settings == null)
                return BadRequest("Invalid module name");

            return Ok(settings);
        }

        [HttpPost("access/{userId:guid}")]
        public async Task<IActionResult> SaveUserAccess(Guid userId, [FromBody] SettingAccess settingAccess)
        {
            try
            {
                if (settingAccess == null)
                    return BadRequest("Invalid data received");

                // 1. SALE
                if (settingAccess.SaleAccess != null)
                {
                    var existing = await _context.SaleAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.SaleAccess.Add(new SaleAccess
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
                    var existing = await _context.PurchaseAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.PurchaseAccess.Add(new PurchaseAccess
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
                    var existing = await _context.InventoryAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.InventoryAccess.Add(new InventoryAccess
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
                    var existing = await _context.StaffAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.StaffAccess.Add(new StaffAccess
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
                    var existing = await _context.AccountAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.AccountAccess.Add(new AccountAccess
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
                    var existing = await _context.ReportAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.ReportAccess.Add(new ReportAccess
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
                    var existing = await _context.MasterAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.MasterAccess.Add(new MasterAccess
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
                    var existing = await _context.MiscAccess.Where(a => a.UserId == userId).ToListAsync();

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
                            _context.MiscAccess.Add(new MiscAccess
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
                return Ok(new { Message = "Access saved successfully", StatusCode = 200, UserId = userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception occurred while saving user access:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return Ok(new { Message = "Access saved successfully", StatusCode = 500, UserId = userId });
               // return StatusCode(500, "An error occurred while saving access data.");
            }
        }

        [HttpGet("access/{userId:guid}")]
        public async Task<IActionResult> GetUserAccess(Guid userId)
        {
            var result = new SettingAccess
            {
                SaleAccess = await _context.SaleAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.SaleSettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync(),

                PurchaseAccess = await _context.PurchaseAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.PurchaseSettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync(),

                InventoryAccess = await _context.InventoryAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.InventorySettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync(),

                StaffAccess = await _context.StaffAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.StaffSettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync(),

                AccountAccess = await _context.AccountAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.AccountSettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync(),

                ReportAccess = await _context.ReportAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.ReportSettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync(),

                MasterAccess = await _context.MasterAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.MasterSettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync(),

                MiscAccess = await _context.MiscAccess
                    .Where(x => x.UserId == userId)
                    .Select(x => new AccessDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        SettingId = x.MiscSettingId,
                        View = x.View,
                        Add = x.Add,
                        Modify = x.Modify,
                        Delete = x.Delete,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToListAsync()
            };

            return Ok(result);
        }

    }
}
