using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Services;
using ProductService.Services.Implementation;
using ProductService.Services.Interface;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using System.Security.Claims;

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
              [FromServices] IProductService productService) =>
            {
                var response = await productService
                    .GetProductsAsync(request, includeInactive);

                return Results.Ok(response);
            });

            app.MapPost("/api/products", async (
                ProductCreateDto dto,
                [FromServices] IProductService productService) =>
            {
                var response = await productService.CreateProductAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/products/{id:int}", async (
                int id,
                ProductUpdateDto dto,
                [FromServices] IProductService productService) =>
            {
                var response = await productService.UpdateProductAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/products/{id:int}", async (
                int id,
                [FromServices] IProductService productService) =>
            {

                var response = await productService.RemoveProductAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });



        }
    }
}