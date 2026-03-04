using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using PricingService.DTOs;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace PricingService.Services.Interface
{
    public interface IPricingService
    {
        // Discount
        Task<ApiResponse<List<Discount>>> GetDiscountsAsync(PaginationRequest request, bool includeInactive, Guid tenantId);

        Task<ApiResponse<DiscountResponseDto>> CreateDiscountAsync(DiscountCreateDto dto, Guid tenantId);

        Task<ApiResponse<string>> UpdateDiscountAsync(int discountId, DiscountUpdateDto dto, Guid tenantId);

        Task<ApiResponse<string>> RemoveDiscountAsync(int discountId, Guid tenantId);

        // Charge
        Task<ApiResponse<List<Charge>>> GetChargesAsync(PaginationRequest request, bool includeInactive, Guid tenantId);

        Task<ApiResponse<ChargeResponseDto>> CreateChargeAsync(ChargeCreateDto dto, Guid tenantId);

        Task<ApiResponse<string>> UpdateChargeAsync(int chargeId, ChargeUpdateDto dto, Guid tenantId);

        Task<ApiResponse<string>> RemoveChargeAsync(int chargeId, Guid tenantId);

        // Tax
        Task<ApiResponse<List<Tax>>> GetTaxesAsync(PaginationRequest request, bool includeInactive, Guid tenantId);

        Task<ApiResponse<TaxResponseDto>> CreateTaxAsync(TaxCreateDto dto, Guid tenantId);

        Task<ApiResponse<string>> UpdateTaxAsync(int taxId, TaxUpdateDto dto, Guid tenantId);

        Task<ApiResponse<string>> RemoveTaxAsync(int taxId, Guid tenantId);
    }
}