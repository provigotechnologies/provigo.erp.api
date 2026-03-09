using ProviGo.Common.Data;
using Microsoft.EntityFrameworkCore;
using PricingService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace PricingService.Services.Interface
{
    public interface IPricingService
    {
        // Discount
        Task<ApiResponse<List<Discount>>> GetDiscountsAsync(PaginationRequest request, bool includeInactive);

        Task<ApiResponse<DiscountResponseDto>> CreateDiscountAsync(DiscountCreateDto dto);

        Task<ApiResponse<string>> UpdateDiscountAsync(int discountId, DiscountUpdateDto dto);

        Task<ApiResponse<string>> RemoveDiscountAsync(int discountId);

        // Charge
        Task<ApiResponse<List<Charge>>> GetChargesAsync(PaginationRequest request, bool includeInactive);

        Task<ApiResponse<ChargeResponseDto>> CreateChargeAsync(ChargeCreateDto dto);

        Task<ApiResponse<string>> UpdateChargeAsync(int chargeId, ChargeUpdateDto dto);

        Task<ApiResponse<string>> RemoveChargeAsync(int chargeId);

        // Tax
        Task<ApiResponse<List<Tax>>> GetTaxesAsync(PaginationRequest request, bool includeInactive);

        Task<ApiResponse<TaxResponseDto>> CreateTaxAsync(TaxCreateDto dto);

        Task<ApiResponse<string>> UpdateTaxAsync(int taxId, TaxUpdateDto dto);

        Task<ApiResponse<string>> RemoveTaxAsync(int taxId);
    }
}