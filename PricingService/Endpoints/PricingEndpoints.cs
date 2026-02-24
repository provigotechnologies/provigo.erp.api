using PricingService.DTOs;
using PricingService.Services.Interface;
using ProviGo.Common.Pagination;
using PricingService.Services;

namespace PricingService.Endpoints
{
    public class PricingEndpoints
    {
        public static void Map(WebApplication app)
        {
            // ===========================
            // 🔹 Discounts
            // ===========================

            app.MapGet("/api/discounts", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                PricingProvider pricingProvider,
                IPricingService discountService) =>
            {
                var response = await discountService.GetDiscountsAsync(request, includeInactive, pricingProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/discounts", async (
                DiscountCreateDto dto,
                PricingProvider pricingProvider,
                IPricingService discountService) =>
            {
                var response = await discountService.CreateDiscountAsync(dto, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/discounts/{id:int}", async (
                int id,
                DiscountUpdateDto dto,
                PricingProvider pricingProvider,
                IPricingService discountService) =>
            {
                var response = await discountService.UpdateDiscountAsync(id, dto, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/discounts/{id:int}", async (
                int id,
                PricingProvider pricingProvider,
                IPricingService discountService) =>
            {
                var response = await discountService.RemoveDiscountAsync(id, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


            // ===========================
            // 🔹 Charges
            // ===========================

            app.MapGet("/api/charges", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                PricingProvider pricingProvider,
                IPricingService chargeService) =>
            {
                var response = await chargeService.GetChargesAsync(request, includeInactive, pricingProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/charges", async (
                ChargeCreateDto dto,
                PricingProvider pricingProvider,
                IPricingService chargeService) =>
            {
                var response = await chargeService.CreateChargeAsync(dto, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/charges/{id:int}", async (
                int id,
                ChargeUpdateDto dto,
                PricingProvider pricingProvider,
                IPricingService chargeService) =>
            {
                var response = await chargeService.UpdateChargeAsync(id, dto, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/charges/{id:int}", async (
                int id,
                PricingProvider pricingProvider,
                IPricingService discountService) =>
            {
                var response = await discountService.RemoveChargeAsync(id, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


            // ===========================
            // 🔹 Taxes
            // ===========================

            app.MapGet("/api/taxes", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                PricingProvider pricingProvider,
                IPricingService taxService) =>
            {
                var response = await taxService.GetTaxesAsync(request, includeInactive, pricingProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/taxes", async (
                TaxCreateDto dto,
                PricingProvider pricingProvider,
                IPricingService chargeService) =>
            {
                var response = await chargeService.CreateTaxAsync(dto, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/taxes/{id:int}", async (
                int id,
                TaxUpdateDto dto,
                PricingProvider pricingProvider,
                IPricingService chargeService) =>
            {
                var response = await chargeService.UpdateTaxAsync(id, dto, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/taxes/{id:int}", async (
                int id,
                PricingProvider pricingProvider,
                IPricingService taxService) =>
            {
                var response = await taxService.RemoveTaxAsync(id, pricingProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

        }

    }
}
