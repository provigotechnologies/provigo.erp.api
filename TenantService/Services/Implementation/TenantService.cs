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
    public class TenantService(
    TenantDbContext db,
    IGenericRepository<TenantDetails> repo) : ITenantService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<TenantDetails> _repo = repo;

        public async Task<ApiResponse<TenantDto>> CreateTenantAsync(
         TenantCreateDto dto)
        {
            try
            {

                // 🔒 Duplicate email check
                var emailExists = await _db.TenantDetails
                    .AnyAsync(i => i.Email == dto.Email);

                if (emailExists)
                {
                    return ApiResponseFactory.Failure<TenantDto>(
                        "This email is already registered."
                    );
                }

                var tenantDetails = new TenantDetails
                {
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    LogoUrl = dto.LogoUrl,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                _db.TenantDetails.Add(tenantDetails);
                int affectedRows = await _db.SaveChangesAsync();
                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<TenantDto>("Insert failed");

                // Return DTO with generated ID
                var responseDto = new TenantDto
                {
                    TenantId = tenantDetails.TenantId,
                    Name = tenantDetails.Name,
                    Phone = tenantDetails.Phone,
                    Email = tenantDetails.Email,
                    Address = tenantDetails.Address,
                    IsActive = tenantDetails.IsActive
                };

                return ApiResponseFactory.Success(
                    responseDto,
                    "Tenant created successfully"
                );
            }
            catch (DbUpdateException ex)
            {

                return ApiResponseFactory.Failure<TenantDto>("Database error occurred");

            }

        }


        public async Task<ApiResponse<List<TenantDetails>>> GetTenantsAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.TenantDetails.AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Tenants fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<TenantDetails>>(ex.Message, ["Database error occurred"]);

            }
        }



        public async Task<ApiResponse<string>> UpdateTenantAsync(Guid tenantId, TenantUpdateDto dto)
        {
            try
            {
                int affectedRows = await _db.TenantDetails
                .Where(i => i.TenantId == tenantId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.Name, dto.Name)
                    .SetProperty(i => i.Email, dto.Email)
                    .SetProperty(i => i.Phone, dto.Phone)
                    .SetProperty(i => i.Address, dto.Address)
                    .SetProperty(i => i.LogoUrl, dto.LogoUrl)
                    .SetProperty(i => i.IsActive, dto.IsActive)
                );
                if (affectedRows == 0)
                {
                    throw new NotFoundException("Tenant not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Tenant updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }


        public async Task<ApiResponse<string>> RemoveTenantAsync(Guid tenantId)
        {
            try
            {
                var tenantDetails = await _db.TenantDetails.FindAsync(tenantId);

                if (tenantDetails == null)
                {
                    return ApiResponseFactory.Failure<string>("Tenant not found");
                }

                _db.TenantDetails.Remove(tenantDetails);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Tenant {tenantId} deleted successfully"
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
