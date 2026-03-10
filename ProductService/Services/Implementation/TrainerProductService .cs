using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Services.Interface;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Response;
using ProviGo.Common.Services;

namespace ProductService.Services.Implementation
{
    public class TrainerProductService(
     TenantDbContext db,
     TenantProvider tenantProvider,
     BranchAccessService branchAccess,
     IGenericRepository<TrainerProduct> repo) : ITrainerProductService
    {
        private readonly TenantDbContext _db = db;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly BranchAccessService _branchAccess = branchAccess;
        private readonly IGenericRepository<TrainerProduct> _repo = repo;

        public async Task<ApiResponse<string>> MapTrainerToProductAsync(
    TrainerProductCreateDto dto)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (!allowedBranches.Contains(dto.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                // Validate trainer
                var trainerExists = await _db.Users
                    .AnyAsync(u => u.UserId == dto.TrainerId &&
                                   u.TenantId == tenantId);

                if (!trainerExists)
                    return ApiResponseFactory.Failure<string>("Invalid trainer");

                // Validate products
                var validProducts = await _db.Products
                    .Where(p => dto.ProductIds.Contains(p.ProductId) &&
                                p.TenantId == tenantId &&
                                p.BranchId == dto.BranchId)
                    .Select(p => p.ProductId)
                    .ToListAsync();

                if (!validProducts.Any())
                    return ApiResponseFactory.Failure<string>("Invalid products");

                // Existing mappings
                var existingMappings = await _db.TrainerProducts
                    .Where(tc => tc.TrainerId == dto.TrainerId &&
                                 tc.BranchId == dto.BranchId &&
                                 tc.TenantId == tenantId)
                    .Select(tc => tc.ProductId)
                    .ToListAsync();

                var newProducts = validProducts
                    .Except(existingMappings)
                    .ToList();

                if (!newProducts.Any())
                    return ApiResponseFactory.Failure<string>("Mappings already exist");

                var mappings = newProducts.Select(productId => new TrainerProduct
                {
                    TenantId = tenantId,
                    BranchId = dto.BranchId,
                    TrainerId = dto.TrainerId,
                    ProductId = productId,
                    IsActive = true
                });

                await _db.TrainerProducts.AddRangeAsync(mappings);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success("Trainer mapped successfully");
            }
            catch
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }


        public async Task<ApiResponse<List<TrainerProduct>>> GetTrainersByProductAsync(
     PaginationRequest request,
     bool includeInactive)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var query = _db.TrainerProducts
                    .Include(tp => tp.Trainer)
                    .Include(tp => tp.Product)
                    .Where(c => allowedBranches.Contains(c.BranchId))
                    .AsNoTracking();

                var paged = await _repo.GetPagedAsync(
                    query,
                    request,
                    c => includeInactive || c.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    paged,
                    "Trainer Product fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<TrainerProduct>>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        public async Task<ApiResponse<List<int>>> GetProductsByTrainerAsync(Guid trainerId)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var products = await _db.TrainerProducts
                    .Where(x => x.TrainerId == trainerId &&
                                x.TenantId == tenantId &&
                                allowedBranches.Contains(x.BranchId) &&
                                x.IsActive)
                    .Select(x => x.ProductId)
                    .ToListAsync();

                return ApiResponseFactory.Success(products);
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<int>>(
                 ex.Message,
                 ["Database error occurred"]);
            }
        }


        public async Task<ApiResponse<string>> DeleteTrainerProductAsync(int id)
        {
            try
            {
                var mapping = await _db.TrainerProducts
                    .FirstOrDefaultAsync(x => x.TrainerProductId == id);

                if (mapping == null)
                    return ApiResponseFactory.Failure<string>("Mapping not found");

                _db.TrainerProducts.Remove(mapping);

                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Delete failed");

                return ApiResponseFactory.Success(
                    $"TrainerCourse {id} deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


    }
}