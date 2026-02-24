using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using ProviGo.Common.Response;
using TenantService.DTOs;
using TenantService.DTOs;
using TenantService.Services.Interface;

namespace TenantService.Services.Implementation
{
    public class BranchService(
     TenantDbContext db,
     IGenericRepository<Branch> repo) : IBranchService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Branch> _repo = repo;

        public async Task<ApiResponse<BranchDto>> CreateBranchAsync(BranchCreateDto dto, Guid tenantId)
        {
            try
            {
                // 🔒 Duplicate Name check
                var exists = await _db.Branches
                    .AnyAsync(b => b.TenantId == tenantId && b.BranchName == dto.BranchName);

                if (exists)
                    return ApiResponseFactory.Failure<BranchDto>("Branch name already exists for this tenant");

                var branch = new Branch
                {
                    TenantId = tenantId,
                    BranchName = dto.BranchName,
                    Address = dto.Address,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Branches.Add(branch);
                int affectedRows = await _db.SaveChangesAsync();
                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<BranchDto>("Insert failed");

                // Return DTO with timestamps
                var responseDto = new BranchDto
                {
                    BranchId = branch.BranchId,
                    TenantId = branch.TenantId,
                    BranchName = branch.BranchName,
                    Address = branch.Address,
                    IsActive = branch.IsActive,
                    CreatedAt = branch.CreatedAt,
                    UpdatedAt = branch.UpdatedAt
                };

                return ApiResponseFactory.Success(
                    responseDto,
                    "Branch created successfully"
                );
            }
            catch (DbUpdateException ex)
            {
                return ApiResponseFactory.Failure<BranchDto>(
                    ex.InnerException?.Message ?? ex.Message
                );
            }
        }


        //public async Task<ApiResponse<BranchDto>> GetBranchByIdAsync(Guid branchId)
        //{
        //    var branch = await _db.Branches.AsNoTracking()
        //        .FirstOrDefaultAsync(b => b.BranchId == branchId);

        //    if (branch == null)
        //        return ApiResponseFactory.Failure<BranchDto>("Branch not found");

        //    return ApiResponseFactory.Success(new BranchDto
        //    {
        //        BranchId = branch.BranchId,
        //        TenantId = branch.TenantId,
        //        BranchName = branch.BranchName,
        //        Address = branch.Address,
        //        IsActive = branch.IsActive,
        //        CreatedAt = branch.CreatedAt,
        //        UpdatedAt = branch.UpdatedAt
        //    });
        //}

        public async Task<ApiResponse<BranchDto>> GetBranchByIdAsync(Guid branchId, Guid tenantId)
        {
            var branch = await _db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.TenantId == tenantId);

            if (branch == null)
                return ApiResponseFactory.Failure<BranchDto>("Branch not found for this tenant");

            return ApiResponseFactory.Success(new BranchDto
            {
                BranchId = branch.BranchId,
                TenantId = branch.TenantId,
                BranchName = branch.BranchName,
                Address = branch.Address,
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt,
                UpdatedAt = branch.UpdatedAt
            });
        }


        public async Task<ApiResponse<List<Branch>>> GetBranchesAsync(
           PaginationRequest request,
           bool includeInactive, Guid tenantId)
        {
            try
            {
                var query = _db.Branches
                    .Where(b => b.TenantId == tenantId)
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


        public async Task<ApiResponse<string>> UpdateBranchAsync(Guid branchId, BranchUpdateDto dto, Guid tenantId)
        {
            try
            {

                int affectedRows = await _db.Branches
                    .Where(b => b.BranchId == branchId && b.TenantId == tenantId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(b => b.BranchName, dto.BranchName)
                        .SetProperty(b => b.Address, dto.Address)
                        .SetProperty(b => b.IsActive, dto.IsActive)
                        .SetProperty(b => b.UpdatedAt, DateTime.UtcNow) // 🔹 update timestamp
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


        public async Task<ApiResponse<string>> RemoveBranchAsync(Guid branchId, Guid tenantId)
        {
            try
            {
                var branch = await _db.Branches
                    .FirstOrDefaultAsync(b => b.BranchId == branchId &&
                                              b.TenantId == tenantId);
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



    }
}
