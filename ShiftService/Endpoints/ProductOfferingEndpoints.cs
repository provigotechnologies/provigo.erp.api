using ProviGo.Common.Pagination;
using ShiftService.DTOs;
using ShiftService.Services.Interface;

namespace ShiftService.Endpoints
{
    public class ProductOfferingEndpoints
    {
        public static void Map(WebApplication app)
        {
            // 🔹 Create Product Offering
            app.MapPost("/api/product-offerings", async (
                ProductOfferingCreateDto dto,
                IProductOfferingService service) =>
            {
                var response = await service.CreateProductOfferingAsync(dto);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


            // 🔹 Get Offerings
            app.MapGet("/api/product-offerings", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                IProductOfferingService service) =>
            {
                var response = await service
                    .GetOfferingsAsync(request, includeInactive);

                return Results.Ok(response);
            });


            // 🔹 Update Offering
            app.MapPut("/api/product-offerings/{offeringId:int}", async (
                int offeringId,
                ProductOfferingUpdateDto dto,
                IProductOfferingService service) =>
            {
                var response = await service
                    .UpdateProductOfferingAsync(offeringId, dto);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


            // 🔹 Delete Offering
            app.MapDelete("/api/product-offerings/{offeringId:int}", async (
                int offeringId,
                IProductOfferingService service) =>
            {
                var response = await service
                    .DeleteProductOfferingAsync(offeringId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });
        }
    }
}