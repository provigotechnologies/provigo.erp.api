using IdentityService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Pagination;
using CustomerService.DTOs;
using CustomerService.Services.Interface;

namespace CustomerService.Endpoints
{
    public static class CustomerEndpoints
    {
        public static void Map(WebApplication app)
        {
            // ===========================
            // 🔹 Customer
            // ===========================

            app.MapPost("/api/customers", async (
                CustomerCreateDto dto,
                ICustomerService customerService) =>
            {
                var response = await customerService.CreateCustomerAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapGet("/api/customers", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                ICustomerService customerService) =>
            {
                var response = await customerService.GetCustomersAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPut("/api/customers/{id:int}", async (
                int id,
                CustomerUpdateDto dto,
                ICustomerService customerService) =>
            {
                var response = await customerService.UpdateCustomerAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/customers/{id:int}", async (
                int id,
                ICustomerService customerService) =>
            {
                var response = await customerService.RemoveCustomerAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


         
        }
    }
}
