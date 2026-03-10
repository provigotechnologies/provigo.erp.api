using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services.Interface;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;

namespace ProductService.Endpoints
{
    public static class TrainerProductEndpoints
    {
        public static void Map(WebApplication app)
        {
            // 🔹 Map Trainer to Product
            app.MapPost("/api/trainerproduct/map", async (
                TrainerProductCreateDto dto,
                [FromServices] ITrainerProductService trainerProductService) =>
            {
                var response = await trainerProductService
                    .MapTrainerToProductAsync(dto);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Get Trainers by Product
            app.MapGet("/api/trainerproduct/by-product/{productId}", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                [FromServices] ITrainerProductService trainerProductService) =>
            {
                var response = await trainerProductService
                    .GetTrainersByProductAsync(request, includeInactive);

                return Results.Ok(response);
            });

            // 🔹 Get Products by Trainer
            app.MapGet("/api/trainerproduct/by-trainer/{trainerId}", async (
                Guid trainerId,
                [FromServices] ITrainerProductService trainerProductService) =>
            {
                var response = await trainerProductService
                    .GetProductsByTrainerAsync(trainerId);

                return Results.Ok(response);
            });

            // 🔹 Delete Mapping
            app.MapDelete("/api/trainerproduct/{id}", async (
                int id,
                [FromServices] ITrainerProductService trainerProductService) =>
            {
                var response = await trainerProductService
                    .DeleteTrainerProductAsync(id);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });
        }
    }
}