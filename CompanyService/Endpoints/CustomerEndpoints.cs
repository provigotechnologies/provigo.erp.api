using IdentityService.Data;
using CustomerService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerService.DTOs;
using CustomerService.Services.Interface;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using System.Security.Claims;

namespace CustomerService.Endpoints
{
    public static class CustomerEndpoints
    {
        public static void Map(WebApplication app)
        {
            // ===========================
            // 🔹 Products
            // ===========================

            app.MapGet("/api/customers", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                Guid branchId,
                CustomerProvider customerProvider,
                 [FromServices] ICustomerService customerService) =>
            {
                var response = await customerService.GetCustomersAsync(request, includeInactive, branchId, customerProvider.TenantId);
                return Results.Ok(response);
            });

            app.MapPost("/api/customers", async (
                CompanyCreateDto dto,
                 [FromServices] ICustomerService customerService, CustomerProvider customerProvider) =>
            {
                var response = await customerService.CreateCustomerAsync(dto, customerProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/customers/{id:int}", async (
                int id,
                CustomerUpdateDto dto,
                CustomerProvider customerProvider,
                 [FromServices] ICustomerService customerService) =>
            {

                var response = await customerService.UpdateCustomerAsync(id, dto, customerProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/customers/{id:int}", async (
                int id,
                CustomerProvider customerProvider,
                [FromServices] ICustomerService customerService) =>
            {

                var response = await customerService.RemoveCustomerAsync(id, customerProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });



        }
    }
}
