using IdentityService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Services;
using ProductService.Services.Implementation;
using ProductService.Services.Interface;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
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

            app.MapGet("/api/products/{id:int}", async (
            int id,
            bool includeInactive,
            Guid branchId,
            ProductProvider productProvider,
            [FromServices] IProductService productService) =>
            {
                var response = await productService.GetProductByIdAsync(id, includeInactive, branchId, productProvider.TenantId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            });


            app.MapGet("/api/products", async (
              [AsParameters] PaginationRequest request,
              bool includeInactive,
              Guid branchId,   
              ProductProvider productProvider,
              [FromServices] IProductService productService) =>
            {
                var response = await productService
                    .GetProductsAsync(request, includeInactive, branchId, productProvider.TenantId);

                return Results.Ok(response);
            });

            app.MapPost("/api/products", async (
                ProductCreateDto dto,
                Guid branchId,
                [FromServices] IProductService productService,
                ProductProvider productProvider) =>
            {
                var response = await productService.CreateProductAsync(dto, branchId, productProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/products/{id:int}", async (
                int id,
                ProductUpdateDto dto,
                Guid branchId,
                ProductProvider productProvider,
                [FromServices] IProductService productService) =>
            {
                var response = await productService.UpdateProductAsync(id, dto, branchId, productProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/products/{id:int}", async (
                int id,
                Guid branchId,
                ProductProvider productProvider,
                [FromServices] IProductService productService) =>
            {

                var response = await productService.RemoveProductAsync(id, branchId, productProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });



        }
    }
}
