using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using PricingService.DTOs;
using PricingService.Services.Interface;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace PricingService.Services.Implementation
{
    public class PricingService(
    TenantDbContext db,
    IGenericRepository<Discount> discountRepo,
    IGenericRepository<Charge> chargeRepo,
    IGenericRepository<Tax> taxRepo,
    IIdentityProvider identityProvider) : IPricingService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Discount> _discountRepo = discountRepo;
        private readonly IGenericRepository<Charge> _chargeRepo = chargeRepo;
        private readonly IGenericRepository<Tax> _taxRepo = taxRepo;
        private readonly IIdentityProvider _identityProvider = identityProvider;
        private Guid TenantId => _identityProvider.TenantId;

        // Discount
        public async Task<ApiResponse<DiscountDto>> CreateDiscountAsync(DiscountCreateDto dto)
        {
            try
            {

                var exists = await _db.Discounts
            .AnyAsync(d => d.TenantId == TenantId && d.Name == dto.Name);

                if (exists)
                    return ApiResponseFactory.Failure<DiscountDto>("Discount already exists");

                var discount = new Discount
                {
                    TenantId = TenantId,
                    Name = dto.Name,
                    Type = dto.Type,
                    Value = dto.Value,
                    IsActive = dto.IsActive
                };

                _db.Discounts.Add(discount);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<DiscountDto>("Insert failed");

                var responseDto = new DiscountDto
                {
                    DiscountId = discount.DiscountId,
                    TenantId = discount.TenantId,
                    Name = discount.Name,
                    Type = discount.Type,
                    Value = discount.Value,
                    IsActive = discount.IsActive
                };

                return ApiResponseFactory.Success(responseDto, "Discount created successfully");
            }
            catch (DbUpdateException ex)
            {

                return ApiResponseFactory.Failure<DiscountDto>("Database error occurred");

            }

        }

        public async Task<ApiResponse<List<Discount>>> GetDiscountsAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.Discounts
                    .Where(p => p.TenantId == TenantId)
                    .AsNoTracking();

                // Apply filter
                var pagedResult = await _discountRepo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Discounts fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<Discount>>(ex.Message, ["Database error occurred"]);

            }
        }

        public async Task<ApiResponse<string>> UpdateDiscountAsync(int discountId, DiscountUpdateDto dto)
        {
            try
            {
                int affectedRows = await _db.Discounts
                   .Where(p => p.DiscountId == discountId
                            && p.TenantId == TenantId)

                   .ExecuteUpdateAsync(s => s
                       .SetProperty(i => i.Name, dto.Name)
                       .SetProperty(i => i.Type, dto.Type)
                       .SetProperty(i => i.Value, dto.Value)
                       .SetProperty(i => i.IsActive, dto.IsActive)
                   );
                if (affectedRows == 0)
                {
                    throw new NotFoundException("Discount not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Discount updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }


        public async Task<ApiResponse<string>> RemoveDiscountAsync(int discountId)
        {
            try
            {
                var discount = await _db.Discounts
                    .FirstOrDefaultAsync(p => p.DiscountId == discountId
                                           && p.TenantId == TenantId);
                if (discount == null)
                {
                    return ApiResponseFactory.Failure<string>("Discount not found");
                }

                _db.Discounts.Remove(discount);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Discount {discountId} deleted successfully"
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


        // Charge
        public async Task<ApiResponse<ChargeDto>> CreateChargeAsync(ChargeCreateDto dto)
        {
            try
            {

                var exists = await _db.Charges
            .AnyAsync(d => d.TenantId == TenantId && d.Name == dto.Name);

                if (exists)
                    return ApiResponseFactory.Failure<ChargeDto>("Charge already exists");

                var charge = new Charge
                {
                    TenantId = TenantId,
                    Name = dto.Name,
                    ChargeType = dto.ChargeType,
                    Value = dto.Value,
                    IsActive = dto.IsActive
                };

                _db.Charges.Add(charge);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<ChargeDto>("Insert failed");

                var responseDto = new ChargeDto
                {
                    ChargeId = charge.ChargeId,
                    TenantId = charge.TenantId,
                    Name = charge.Name,
                    ChargeType = charge.ChargeType,
                    Value = charge.Value,
                    IsActive = charge.IsActive
                };

                return ApiResponseFactory.Success(responseDto, "Charge created successfully");
            }
            catch (DbUpdateException ex)
            {

                return ApiResponseFactory.Failure<ChargeDto>("Database error occurred");

            }

        }

        public async Task<ApiResponse<List<Charge>>> GetChargesAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.Charges
                    .Where(p => p.TenantId == TenantId)
                    .AsNoTracking();

                // Apply filter
                var pagedResult = await _chargeRepo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Charges fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<Charge>>(ex.Message, ["Database error occurred"]);

            }
        }

        public async Task<ApiResponse<string>> UpdateChargeAsync(int chargeId, ChargeUpdateDto dto)
        {
            try
            {
                int affectedRows = await _db.Charges
                   .Where(p => p.ChargeId == chargeId
                            && p.TenantId == TenantId)

                   .ExecuteUpdateAsync(s => s
                       .SetProperty(i => i.Name, dto.Name)
                       .SetProperty(i => i.ChargeType, dto.ChargeType)
                       .SetProperty(i => i.Value, dto.Value)
                       .SetProperty(i => i.IsActive, dto.IsActive)
                   );
                if (affectedRows == 0)
                {
                    throw new NotFoundException("Charge not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Charge updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }


        public async Task<ApiResponse<string>> RemoveChargeAsync(int chargeId)
        {
            try
            {
                var charge = await _db.Charges
                    .FirstOrDefaultAsync(p => p.ChargeId == chargeId
                                           && p.TenantId == TenantId);
                if (charge == null)
                {
                    return ApiResponseFactory.Failure<string>("Charge not found");
                }

                _db.Charges.Remove(charge);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Charge {chargeId} deleted successfully"
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


        // Tax
        public async Task<ApiResponse<TaxDto>> CreateTaxAsync(TaxCreateDto dto)
        {
            try
            {

                var exists = await _db.Taxes
            .AnyAsync(d => d.TenantId == TenantId && d.Name == dto.Name);

                if (exists)
                    return ApiResponseFactory.Failure<TaxDto>("Tax already exists");

                var tax = new Tax
                {
                    TenantId = TenantId,
                    Name = dto.Name,
                    Rate = dto.Rate,
                    IsActive = dto.IsActive
                };

                _db.Taxes.Add(tax);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<TaxDto>("Insert failed");

                var responseDto = new TaxDto
                {
                    TaxId = tax.TaxId,
                    TenantId = tax.TenantId,
                    Name = tax.Name,
                    Rate = tax.Rate,
                    IsActive = tax.IsActive
                };

                return ApiResponseFactory.Success(responseDto, "Tax created successfully");
            }
            catch (DbUpdateException ex)
            {

                return ApiResponseFactory.Failure<TaxDto>("Database error occurred");

            }

        }

        public async Task<ApiResponse<List<Tax>>> GetTaxesAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.Taxes
                    .Where(p => p.TenantId == TenantId)
                    .AsNoTracking();

                // Apply filter
                var pagedResult = await _taxRepo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Taxes fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<Tax>>(ex.Message, ["Database error occurred"]);

            }
        }

        public async Task<ApiResponse<string>> UpdateTaxAsync(int taxId, TaxUpdateDto dto)
        {
            try
            {
                int affectedRows = await _db.Taxes
                   .Where(p => p.TaxId == taxId
                            && p.TenantId == TenantId)

                   .ExecuteUpdateAsync(s => s
                       .SetProperty(i => i.Name, dto.Name)
                       .SetProperty(i => i.Rate, dto.Rate)
                       .SetProperty(i => i.IsActive, dto.IsActive)
                   );
                if (affectedRows == 0)
                {
                    throw new NotFoundException("Tax not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Tax updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }


        public async Task<ApiResponse<string>> RemoveTaxAsync(int taxId)
        {
            try
            {
                var tax = await _db.Taxes
                    .FirstOrDefaultAsync(p => p.TaxId == taxId
                                           && p.TenantId == TenantId);
                if (tax == null)
                {
                    return ApiResponseFactory.Failure<string>("Tax not found");
                }

                _db.Taxes.Remove(tax);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Tax {taxId} deleted successfully"
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