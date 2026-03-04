using IdentityService.Data;
using IdentityService.Services;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Services.Interface;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace ProductService.Services.Implementation
{
    public class ProductService(
    TenantDbContext db,
    IGenericRepository<Product> repo) : IProductService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Product> _repo = repo;

        public async Task<ApiResponse<ProductResponseDto>> CreateProductAsync(
            ProductCreateDto dto, 
            Guid branchId, 
            Guid tenantId)
        {
            try
            {

                var exists = await _db.Products
                    .AnyAsync(p => p.TenantId == tenantId && 
                    p.BranchId == branchId &&
                    p.ProductName == dto.ProductName);

                if (exists)
                {
                    return ApiResponseFactory.Failure<ProductResponseDto>(
                        "This product already exists for the selected institute."
                    );
                }

                // Create new product
                var product = new Product
                {
                    TenantId = tenantId,
                    BranchId = branchId,
                    ProductName = dto.ProductName,
                    TotalFee = dto.TotalFee,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                _db.Products.Add(product);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<ProductResponseDto>("Insert failed");

                var responseDto = new ProductResponseDto
                {
                    ProductId = product.ProductId,
                    TenantId = product.TenantId,
                    BranchId = product.BranchId,
                    ProductName = product.ProductName,
                    TotalFee = product.TotalFee,
                    IsActive = product.IsActive,
                };

                return ApiResponseFactory.Success(responseDto, "Product created successfully");
            }
            catch (DbUpdateException ex)
            {
                return ApiResponseFactory.Failure<ProductResponseDto>("Database error occurred");
            }

        }

        public async Task<ApiResponse<List<Product>>> GetProductsAsync(
         PaginationRequest request,
         bool includeInactive,
         Guid branchId,
         Guid tenantId)
        {
            try
            {
                var query = _db.Products
                    .Where(p =>
                        p.TenantId == tenantId &&
                        p.BranchId == branchId)
                    .AsNoTracking();

                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Products fetched successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Product>>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }


        public async Task<ApiResponse<ProductResponseDto>> GetProductByIdAsync(
       int productId,
       bool includeInactive,
       Guid branchId,
       Guid tenantId)
        {
            try
            {
                var product = await _db.Products
                    .AsNoTracking()
                    .Where(c =>
                        c.ProductId == productId &&
                        c.BranchId == branchId &&
                        c.TenantId == tenantId)
                    .Where(c => includeInactive || c.IsActive)
                    .Select(c => new ProductResponseDto
                    {
                        ProductId = c.ProductId,
                        ProductName = c.ProductName,
                        IsActive = c.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                    return ApiResponseFactory.Failure<ProductResponseDto>(
                        "Product not found");

                return ApiResponseFactory.Success(
                    product,
                    "Product fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<ProductResponseDto>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }



        public async Task<ApiResponse<string>> UpdateProductAsync(
        int productId,
        ProductUpdateDto dto,
        Guid branchId,
        Guid tenantId)
        {
            try
            {
                var exists = await _db.Products.AnyAsync(p =>
                    p.TenantId == tenantId &&
                    p.BranchId == branchId &&
                    p.ProductName == dto.ProductName &&
                    p.ProductId != productId);

                if (exists)
                    return ApiResponseFactory.Failure<string>("Product already exists");

                int affectedRows = await _db.Products
                    .Where(p => p.ProductId == productId &&
                                p.TenantId == tenantId &&
                                p.BranchId == branchId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(i => i.ProductName, dto.ProductName)
                        .SetProperty(i => i.TotalFee, dto.TotalFee)
                        .SetProperty(i => i.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Product not found");

                return ApiResponseFactory.Success("Product updated successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }


        public async Task<ApiResponse<string>> RemoveProductAsync(
            int productId,
            Guid branchId, 
            Guid tenantId)
        {
            try
            {
                var product = await _db.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId &&
                                 p.BranchId == branchId && 
                                 p.TenantId == tenantId);

                if (product == null)
                {
                    return ApiResponseFactory.Failure<string>("Product not found");
                }

                _db.Products.Remove(product);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Product {productId} deleted successfully"
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