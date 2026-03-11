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
    public class ProductService(
     TenantDbContext db,
     IGenericRepository<Product> repo,
     BranchAccessService branchAccess,
     TenantProvider tenantProvider,
     CurrentUserService currentUser) : IProductService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Product> _repo = repo;
        private readonly BranchAccessService _branchAccess = branchAccess;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly CurrentUserService _currentUser = currentUser;


        // CREATE PRODUCT
        public async Task<ApiResponse<ProductResponseDto>> CreateProductAsync(ProductCreateDto dto)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                // Branch exists check
                var branchExists = await _db.Branches
                    .AnyAsync(b => b.BranchId == dto.BranchId);

                if (!branchExists)
                    return ApiResponseFactory.Failure<ProductResponseDto>(
                        "Invalid branch selected");

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (!allowedBranches.Contains(dto.BranchId))
                    return ApiResponseFactory.Failure<ProductResponseDto>(
                        "You don't have access to this branch");

                var exists = await _db.Products.AnyAsync(p =>
                    p.BranchId == dto.BranchId &&
                    p.ProductName == dto.ProductName);

                if (exists)
                    return ApiResponseFactory.Failure<ProductResponseDto>(
                        "Product already exists");

                var product = new Product
                {
                    TenantId = _tenantProvider.TenantId,
                    BranchId = dto.BranchId,
                    ProductName = dto.ProductName,
                    TotalFee = dto.TotalFee,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };

                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                var response = new ProductResponseDto
                {
                    ProductId = product.ProductId,
                    BranchId = product.BranchId,
                    ProductName = product.ProductName,
                    TotalFee = product.TotalFee,
                    IsActive = product.IsActive
                };

                return ApiResponseFactory.Success(
                    response,
                    "Product created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<ProductResponseDto>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        // GET PRODUCTS
        public async Task<ApiResponse<List<Product>>> GetProductsAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var query = _db.Products
                    .Where(p => allowedBranches.Contains(p.BranchId))
                    .AsNoTracking();

                var paged = await _repo.GetPagedAsync(
                    query,
                    request,
                    p => includeInactive || p.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    paged,
                    "Products fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Product>>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


        // UPDATE PRODUCT
        public async Task<ApiResponse<string>> UpdateProductAsync(
            int productId,
            ProductUpdateDto dto)
        {
            try
            {
                _currentUser.EnsureWriteAccess();
                var tenantId = _currentUser.TenantId;
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                // 1️⃣ Get product first
                var product = await _db.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.TenantId == tenantId);

                if (product == null)
                    return ApiResponseFactory.Failure<string>("Product not found");

                // 2️⃣ Branch access check
                if (!allowedBranches.Contains(product.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                // 3️⃣ Duplicate name check
                var exists = await _db.Products.AnyAsync(p =>
                    p.ProductId != productId &&
                    p.BranchId == product.BranchId &&
                    p.ProductName == dto.ProductName);

                if (exists)
                    return ApiResponseFactory.Failure<string>(
                        "Product already exists");

                // 4️⃣ Update
                int affectedRows = await _db.Products
                    .Where(p => p.ProductId == productId && p.TenantId == tenantId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.BranchId, dto.BranchId)
                        .SetProperty(p => p.ProductName, dto.ProductName)
                        .SetProperty(p => p.TotalFee, dto.TotalFee)
                        .SetProperty(p => p.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Update failed");

                return ApiResponseFactory.Success(
                    "Product updated successfully");
            }
            catch (Exception)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred");
            }
        }


        // DELETE PRODUCT
        public async Task<ApiResponse<string>> RemoveProductAsync(int productId)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var product = await _db.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.TenantId == _tenantProvider.TenantId);

                if (product == null)
                    return ApiResponseFactory.Failure<string>("Product not found");

                if (!allowedBranches.Contains(product.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(
                    $"Product {productId} deleted successfully");
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