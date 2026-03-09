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
        TenantProvider tenantProvider,
        BranchAccessService branchAccess) : IProductService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Product> _repo = repo;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly BranchAccessService _branchAccess = branchAccess;


        // CREATE PRODUCT
        public async Task<ApiResponse<ProductResponseDto>> CreateProductAsync(
            ProductCreateDto dto)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (!allowedBranches.Contains(dto.BranchId))
                {
                    return ApiResponseFactory.Failure<ProductResponseDto>(
                        "You don't have access to this branch");
                }

                var exists = await _db.Products
                    .AnyAsync(p =>
                        p.BranchId == dto.BranchId &&
                        p.ProductName == dto.ProductName);

                if (exists)
                {
                    return ApiResponseFactory.Failure<ProductResponseDto>(
                        "This product already exists for this branch");
                }

                var product = new Product
                {
                    TenantId = _tenantProvider.TenantId,
                    BranchId = dto.BranchId,
                    ProductName = dto.ProductName,
                    TotalFee = dto.TotalFee,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                var responseDto = new ProductResponseDto
                {
                    ProductId = product.ProductId,
                    TenantId = product.TenantId,
                    BranchId = product.BranchId,
                    ProductName = product.ProductName,
                    TotalFee = product.TotalFee,
                    IsActive = product.IsActive
                };

                return ApiResponseFactory.Success(
                    responseDto,
                    "Product created successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<ProductResponseDto>(
                    "Database error occurred");
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

                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    p => includeInactive || p.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Products fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Product>>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }


        // UPDATE PRODUCT
        public async Task<ApiResponse<string>> UpdateProductAsync(
            int productId,
            ProductUpdateDto dto)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var exists = await _db.Products.AnyAsync(p =>
                    allowedBranches.Contains(p.BranchId) &&
                    p.ProductName == dto.ProductName &&
                    p.ProductId != productId);

                if (exists)
                    return ApiResponseFactory.Failure<string>(
                        "Product already exists");

                int affectedRows = await _db.Products
                    .Where(p =>
                        p.ProductId == productId &&
                        allowedBranches.Contains(p.BranchId))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.ProductName, dto.ProductName)
                        .SetProperty(p => p.TotalFee, dto.TotalFee)
                        .SetProperty(p => p.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>(
                        "Product not found");

                return ApiResponseFactory.Success(
                    "Product updated successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred");
            }
        }


        // DELETE PRODUCT
        public async Task<ApiResponse<string>> RemoveProductAsync(
            int productId)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var product = await _db.Products
                    .FirstOrDefaultAsync(p =>
                        p.ProductId == productId &&
                        allowedBranches.Contains(p.BranchId));

                if (product == null)
                {
                    return ApiResponseFactory.Failure<string>(
                        "Product not found");
                }

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