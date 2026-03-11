using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;

namespace ProviGo.Common.Services
{
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
            // SuperAdmin → All tenant branches
            if (_currentUser.IsSuperAdmin)
            {
                return await _db.Branches
                .Select(b => b.BranchId)
                .ToListAsync();
            }

            // Admin / User → Only assigned branches
            return await _db.UserBranches
                .Where(x => x.UserId == _currentUser.UserId)
                .Select(x => x.BranchId)
                .ToListAsync();
        }

        public async Task<bool> HasBranchAccess(Guid branchId)
        {
            if (_currentUser.IsSuperAdmin)
                return true;

            return await _db.UserBranches
                .AnyAsync(x => x.UserId == _currentUser.UserId && x.BranchId == branchId);
        }


    }
}