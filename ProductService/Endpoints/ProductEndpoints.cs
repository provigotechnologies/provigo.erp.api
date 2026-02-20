using IdentityService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using ProductService.DTOs;
using ProductService.Services.Interface;

namespace ProductService.Endpoints
{
    public static class ProductEndpoints
    {
        public static void Map(WebApplication app)
        {
            // ===========================
            // 🔹 Products
            // ===========================

            app.MapGet("/api/products", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                 [FromServices] IProductService ProductService) =>
            {
                var response = await ProductService.GetProductsAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/products", async (
                ProductCreateDto dto,
                 [FromServices] IProductService ProductService) =>
            {
                var response = await ProductService.CreateProductAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/products/{id:int}", async (
                int id,
                ProductUpdateDto dto,
                 [FromServices] IProductService ProductService) =>
            {
                var response = await ProductService.UpdateProductAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/products/{id:int}", async (
                int id,
                [FromServices] IProductService ProductService) =>
            {
                var response = await ProductService.RemoveProductAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });



        }
    }
}
