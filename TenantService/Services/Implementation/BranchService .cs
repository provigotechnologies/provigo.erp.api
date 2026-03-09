using ProviGo.Common.Data;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using TenantService.DTOs;
using TenantService.Services.Interface;
using ProviGo.Common.Exceptions;

namespace TenantService.Services.Implementation
{
    public class BranchService(
     TenantDbContext db,
     IGenericRepository<Branch> repo) : IBranchService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Branch> _repo = repo;

        public async Task<ApiResponse<BranchResponseDto>> CreateBranchAsync(BranchCreateDto dto, Guid tenantId)
        {
            try
            {
                // 🔒 Duplicate Name check
                var exists = await _db.Branches
                    .AnyAsync(b => b.TenantId == tenantId && b.BranchName == dto.BranchName);

                if (exists)
                    return ApiResponseFactory.Failure<BranchResponseDto>("Branch name already exists for this tenant");

                var branch = new Branch
                {
                    TenantId = tenantId,
                    BranchName = dto.BranchName,
                    BranchCode = dto.BranchCode,
                    GSTIN = dto.GSTIN,
                    Address = dto.Address,
                    State = dto.State,
                    StateCode = dto.StateCode,
                    Country = dto.Country,
                    CountryCode = dto.CountryCode,
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
                    State = branch.State,
                    StateCode = branch.StateCode,
                    Country = branch.Country,
                    CountryCode = branch.CountryCode,
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
                return ApiResponseFactory.Failure<BranchResponseDto>(
                    ex.InnerException?.Message ?? ex.Message
                );
            }
        }


        public async Task<ApiResponse<BranchResponseDto>> GetBranchByIdAsync(Guid branchId, Guid tenantId)
        {
            var branch = await _db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.TenantId == tenantId);

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
                State = branch.State,
                StateCode = branch.StateCode,
                Country = branch.Country,
                CountryCode = branch.CountryCode,
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
                        .SetProperty(b => b.BranchCode, dto.BranchCode)
                        .SetProperty(b => b.GSTIN, dto.GSTIN)
                        .SetProperty(b => b.Address, dto.Address)
                        .SetProperty(b => b.IsActive, dto.IsActive)
                        .SetProperty(b => b.State, dto.State)
                        .SetProperty(b => b.StateCode, dto.StateCode)
                        .SetProperty(b => b.Country, dto.Country)
                        .SetProperty(b => b.CountryCode, dto.CountryCode)
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
