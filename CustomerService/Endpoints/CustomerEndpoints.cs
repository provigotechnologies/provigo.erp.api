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
            // 🔹 Customers
            // ===========================

            app.MapGet("/api/customers/{id:int}", async (
            int id,
            bool includeInactive,
            Guid branchId,
            CustomerProvider customerProvider,
            [FromServices] ICustomerService customerService) =>
            {
                var response = await customerService.GetCustomerByIdAsync(id, includeInactive, branchId, customerProvider.TenantId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            });

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
                CustomerCreateDto dto,
                Guid branchId,
                 [FromServices] ICustomerService customerService, 
                 CustomerProvider customerProvider) =>
            {
                var response = await customerService.CreateCustomerAsync(dto, branchId, customerProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/customers/{id:int}", async (
                int id,
                CustomerUpdateDto dto,
                Guid branchId,
                CustomerProvider customerProvider,
                 [FromServices] ICustomerService customerService) =>
            {

                var response = await customerService.UpdateCustomerAsync(id, dto, branchId, customerProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/customers/{id:int}", async (
                int id,
                Guid branchId,
                CustomerProvider customerProvider,
                [FromServices] ICustomerService customerService) =>
            {

                var response = await customerService.RemoveCustomerAsync(id, branchId, customerProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });



        }
    }
}
