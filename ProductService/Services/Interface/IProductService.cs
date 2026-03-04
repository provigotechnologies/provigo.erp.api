using ProductService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace ProductService.Services.Interface
{
    public interface IProductService
    {

        Task<ApiResponse<ProductResponseDto>> GetProductByIdAsync(
            int productId,
            bool includeInactive,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<List<Product>>> GetProductsAsync(
            PaginationRequest request, 
            bool includeInactive,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<ProductResponseDto>> CreateProductAsync(
            ProductCreateDto dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> UpdateProductAsync(
            int productId, 
            ProductUpdateDto dto, 
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> RemoveProductAsync(
            int productId, 
            Guid branchId,
            Guid tenantId);
    }
}
