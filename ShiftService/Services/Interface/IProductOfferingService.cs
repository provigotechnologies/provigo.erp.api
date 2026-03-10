using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using ShiftService.DTOs;

namespace ShiftService.Services.Interface
{
    public interface IProductOfferingService
    {
        Task<ApiResponse<string>> CreateProductOfferingAsync(ProductOfferingCreateDto dto);

        Task<ApiResponse<List<ProductOffering>>> GetOfferingsAsync(
            PaginationRequest request,
            bool includeInactive);

        Task<ApiResponse<string>> UpdateProductOfferingAsync(
            int offeringId,
            ProductOfferingUpdateDto dto);

        Task<ApiResponse<string>> DeleteProductOfferingAsync(int offeringId);
    }
}