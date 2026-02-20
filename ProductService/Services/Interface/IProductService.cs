using ProductService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace ProductService.Services.Interface
{
    public interface IProductService
    {
        Task<ApiResponse<List<Product>>> GetProductsAsync(PaginationRequest request, bool includeInactive);

        Task<ApiResponse<ProductDto>> CreateProductAsync(ProductCreateDto dto);

        Task<ApiResponse<string>> UpdateProductAsync(int productId, ProductUpdateDto dto);

        Task<ApiResponse<string>> RemoveProductAsync(int productId);
    }
}
