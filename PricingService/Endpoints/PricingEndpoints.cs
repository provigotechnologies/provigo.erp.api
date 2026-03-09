using PricingService.DTOs;
using PricingService.Services;
using PricingService.Services.Interface;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;

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
                IPricingService discountService) =>
            {
                var response = await discountService.GetDiscountsAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/discounts", async (
                DiscountCreateDto dto,
                IPricingService discountService) =>
            {
                var response = await discountService.CreateDiscountAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/discounts/{id:int}", async (
                int id,
                DiscountUpdateDto dto,
                IPricingService discountService) =>
            {
                var response = await discountService.UpdateDiscountAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/discounts/{id:int}", async (
                int id,
                IPricingService discountService) =>
            {
                var response = await discountService.RemoveDiscountAsync(id);
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
                IPricingService chargeService) =>
            {
                var response = await chargeService.GetChargesAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/charges", async (
                ChargeCreateDto dto,
                IPricingService chargeService) =>
            {
                var response = await chargeService.CreateChargeAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/charges/{id:int}", async (
                int id,
                ChargeUpdateDto dto,
                IPricingService chargeService) =>
            {
                var response = await chargeService.UpdateChargeAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/charges/{id:int}", async (
                int id,
                IPricingService discountService) =>
            {
                var response = await discountService.RemoveChargeAsync(id);
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
                IPricingService taxService) =>
            {
                var response = await taxService.GetTaxesAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/taxes", async (
                TaxCreateDto dto,
                IPricingService chargeService) =>
            {
                var response = await chargeService.CreateTaxAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/taxes/{id:int}", async (
                int id,
                TaxUpdateDto dto,
                IPricingService chargeService) =>
            {
                var response = await chargeService.UpdateTaxAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/taxes/{id:int}", async (
                int id,
                IPricingService taxService) =>
            {
                var response = await taxService.RemoveTaxAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

        }

    }
}
