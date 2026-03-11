using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Response;
using ProviGo.Common.Services;
using TenantService.DTOs;
using TenantService.Services.Interface;

namespace TenantService.Services.Implementation
{
    public class BranchService(
    TenantDbContext db,
    IGenericRepository<Branch> repo,
    TenantProvider tenantProvider,
    CurrentUserService currentUser) : IBranchService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Branch> _repo = repo;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly CurrentUserService _currentUser = currentUser;

        public async Task<ApiResponse<BranchResponseDto>> CreateBranchAsync(BranchCreateDto dto)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                // 🔒 Duplicate Name check
                var exists = await _db.Branches
                    .AnyAsync(b => b.BranchName == dto.BranchName);

                if (exists)
                    return ApiResponseFactory.Failure<BranchResponseDto>("Branch name already exists for this tenant");

                var branch = new Branch
                {
                    TenantId = _tenantProvider.TenantId,
                    BranchName = dto.BranchName,
                    BranchCode = dto.BranchCode,
                    GSTIN = dto.GSTIN,
                    CountryId = dto.CountryId,
                    StateId = dto.StateId,
                    Address = dto.Address,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Branches.Add(branch);
                int affectedRows = await _db.SaveChangesAsync();
                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<BranchResponseDto>("Insert failed");

                // Return DTO with timestamps
                var responseDto = new BranchResponseDto
                {
                    BranchId = branch.BranchId,
                    TenantId = branch.TenantId,
                    BranchName = branch.BranchName,
                    BranchCode = branch.BranchCode,
                    GSTIN = branch.GSTIN,
                    CountryId = branch.CountryId,
                    StateId = branch.StateId,
                    Address = branch.Address,
                    IsActive = branch.IsActive
                };

                return ApiResponseFactory.Success(
                    responseDto,
                    "Branch created successfully"
                );
            }
            catch (DbUpdateException ex)
            {
                return ApiResponseFactory.Failure<BranchResponseDto>(
                    ex.InnerException?.Message ?? ex.Message
                );
            }
        }


        public async Task<ApiResponse<BranchResponseDto>> GetBranchByIdAsync(Guid branchId)
        {
            var branch = await _db.Branches
                .Include(b => b.Country)
                .Include(b => b.State)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BranchId == branchId);

            if (branch == null)
                return ApiResponseFactory.Failure<BranchResponseDto>("Branch not found for this tenant");

            return ApiResponseFactory.Success(new BranchResponseDto
            {
                BranchId = branch.BranchId,
                TenantId = branch.TenantId,
                BranchName = branch.BranchName,
                BranchCode = branch.BranchCode,
                GSTIN = branch.GSTIN,
                Address = branch.Address,
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt,
                UpdatedAt = branch.UpdatedAt
            });
        }


        public async Task<ApiResponse<List<Branch>>> GetBranchesAsync(
           PaginationRequest request,
           bool includeInactive)
        {
            try
            {
                var query = _db.Branches
                    .Include(b => b.Country)
                    .Include(b => b.State)
                    .AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Branches fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<Branch>>(ex.Message, ["Database error occurred"]);

            }
        }


        public async Task<ApiResponse<string>> UpdateBranchAsync(Guid branchId, BranchUpdateDto dto)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                int affectedRows = await _db.Branches
                    .Where(b => b.BranchId == branchId && b.TenantId == _tenantProvider.TenantId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(b => b.BranchName, dto.BranchName)
                        .SetProperty(b => b.BranchCode, dto.BranchCode)
                        .SetProperty(b => b.GSTIN, dto.GSTIN)
                        .SetProperty(b => b.CountryId, dto.CountryId)
                        .SetProperty(b => b.StateId, dto.StateId)
                        .SetProperty(b => b.Address, dto.Address)
                        .SetProperty(b => b.IsActive, dto.IsActive)
                        .SetProperty(b => b.UpdatedAt, DateTime.UtcNow)
                    );

                if (affectedRows == 0)
                {
                    throw new NotFoundException("Branch not found");
                }

                return ApiResponseFactory.Success("Branch updated successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }


        public async Task<ApiResponse<string>> RemoveBranchAsync(Guid branchId)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                var branch = await _db.Branches
                    .FirstOrDefaultAsync(b => b.BranchId == branchId && b.TenantId == _tenantProvider.TenantId);

                if (branch == null)
                {
                    return ApiResponseFactory.Failure<string>("Branch not found");
                }

                _db.Branches.Remove(branch);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Branch {branchId} deleted successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message }
                );
            }
        }


        public async Task<List<StateDropdownDto>> GetStateDropdownAsync()
        {
            return await (from s in _db.States
                          join c in _db.Countries
                          on s.CountryId equals c.CountryId
                          select new StateDropdownDto
                          {
                              StateId = s.StateId,
                              StateName = s.StateName,
                              StateCode = s.StateCode,
                              CountryName = c.CountryName,
                              CountryCode = c.CountryCode
                          })
                          .OrderBy(x => x.StateName)
                          .ToListAsync();
        }

    }
}
