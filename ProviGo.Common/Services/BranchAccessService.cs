using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;

namespace ProviGo.Common.Services;

public class BranchAccessService
{
    private readonly TenantDbContext _db;
    private readonly CurrentUserService _currentUser;

    public BranchAccessService(
        TenantDbContext db,
        CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<Guid>> GetAllowedBranchesAsync()
    {
        if (_currentUser.Role == "SuperAdmin")
        {
            return await _db.Branches
                .Where(b => b.TenantId == _currentUser.TenantId)
                .Select(b => b.BranchId)
                .ToListAsync();
        }

        return await _db.UserBranches
            .Where(x => x.UserId == _currentUser.UserId)
            .Select(x => x.BranchId)
            .ToListAsync();
    }
}