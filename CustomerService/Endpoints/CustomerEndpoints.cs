using CustomerService.DTOs;
using CustomerService.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using ProviGo.Common.Pagination;

namespace CustomerService.Endpoints
{
    public static class CustomerEndpoints
    {
        public static void Map(WebApplication app)
        {
            // GET CUSTOMERS
            app.MapGet("/api/customers", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                ICustomerService customerService) =>
            {
                var response = await customerService.GetCustomersAsync(request, includeInactive);
                return Results.Ok(response);
            });


            // CREATE CUSTOMER
            app.MapPost("/api/customers", async (
                CustomerCreateDto dto,
                ICustomerService customerService) =>
            {
                var response = await customerService.CreateCustomerAsync(dto);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


            // UPDATE CUSTOMER
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


            // DELETE CUSTOMER
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