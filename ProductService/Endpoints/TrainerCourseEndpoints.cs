using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;
using ProductService.Services.Interface;
using ProviGo.Common.Pagination;

namespace ProductService.Endpoints
{
    public static class TrainerCourseEndpoints
    {
        public static void Map(WebApplication app)
        {
            // 🔹 Map Trainer to Course
            app.MapPost("/api/trainercourse/map", async (
                TrainerCourseCreateDto dto,
                Guid branchId,
                ProductProvider productProvider,
                [FromServices] ITrainerCourseService trainerCourseService) =>
            {
                var response = await trainerCourseService
                    .MapTrainerToCourseAsync(dto, branchId, productProvider.TenantId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Get Trainers by Product
            app.MapGet("/api/trainercourse/by-product/{productId}", async (
                int productId,
                Guid branchId,
                ProductProvider productProvider,
                [FromServices] ITrainerCourseService trainerCourseService) =>
            {
                var response = await trainerCourseService
                    .GetTrainersByProductAsync(productId, branchId, productProvider.TenantId);

                return Results.Ok(response);
            });
        }

    }
}
