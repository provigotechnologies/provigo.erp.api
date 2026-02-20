using InstituteService.Data;
using InstituteService.DTOs;
using InstituteService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace InstituteService.Services.Implementation
{
    public class InstituteService(
        InstituteDbContext db,
        IGenericRepository<TenantDetails> repo) : IInstituteService
    {
        private readonly InstituteDbContext _db = db;
        private readonly IGenericRepository<TenantDetails> _repo = repo;

        public async Task<ApiResponse<InstituteResponseDto>> CreateInstituteAsync(
         InstituteDto dto)
        {
            try
            {

                // 🔒 Duplicate email check
                var emailExists = await _db.Institutes
                    .AnyAsync(i => i.Email == dto.Email);

                if (emailExists)
                {
                    return ApiResponseFactory.Failure<InstituteResponseDto>(
                        "This email is already registered."
                    );
                }

                var institute = new TenantDetails
                {
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    LogoUrl = dto.LogoUrl,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                _db.Institutes.Add(institute);
                int affectedRows = await _db.SaveChangesAsync();
                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<InstituteResponseDto>("Insert failed");

                // Return DTO with generated ID
                var responseDto = new InstituteResponseDto
                {
                    TenantId = institute.TenantId,
                    Name = institute.Name,
                    Phone = institute.Phone,
                    Email = institute.Email,
                    Address = institute.Address,
                    LogoUrl = institute.LogoUrl,
                    IsActive = institute.IsActive
                };

                return ApiResponseFactory.Success(
                    responseDto,
                    "Institute created successfully"
                );
            }
            catch (DbUpdateException ex)
            {

                return ApiResponseFactory.Failure<InstituteResponseDto>("Database error occurred");

            }

        }

        public async Task<ApiResponse<List<TenantDetails>>> GetInstitutesAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.Institutes.AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Institutes fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<TenantDetails>>(ex.Message, ["Database error occurred"]);

            }
        }

        /*        public async Task<ApiResponse<string>> RemoveInstituteAsync(int instituteId, string action)
                {
                    try
                    {
                        var institute = await db.Institutes.FindAsync(instituteId);
                        if (institute == null)
                        {
                            return ApiResponseFactory.Failure<string>("Institute not found");
                        }
                        db.Institutes.Remove(institute);
                        int affectedRows = await db.SaveChangesAsync();
                        if (affectedRows == 0)
                        {
                            return ApiResponseFactory.Failure<string>("Delete/Active/Inactive failed");
                        }
                        return ApiResponseFactory.Success(
                            $"{action} for instituteId {instituteId} performed successfully");
                    }
                    catch (Exception ex)
                    {

                        return ApiResponseFactory.Failure<string>(ex.Message, ["Database error occurred"]);

                    }
                }*/


        public async Task<ApiResponse<string>> RemoveInstituteAsync(Guid tenantId)
        {
            try
            {
                var institute = await _db.Institutes.FindAsync(tenantId);

                if (institute == null)
                {
                    return ApiResponseFactory.Failure<string>("Institute not found");
                }

                _db.Institutes.Remove(institute);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Institute {tenantId} deleted successfully"
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


        public async Task<ApiResponse<string>> UpdateInstituteAsync(Guid tenantId, InstituteDto dto)
        {
            try
            {
                int affectedRows = await _db.Institutes
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
                    throw new NotFoundException("Institute not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Institute updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }
    }
}
