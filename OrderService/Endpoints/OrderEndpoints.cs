using IdentityService.Data;
using OrderService.DTOs;
using OrderService.Services.Interface;
using ProviGo.Common.Pagination;

namespace OrderService.Endpoints
{
    public class OrderEndpoints
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/api/orders", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                IOrderService orderService) =>
            {
                var response = await orderService.GetOrdersAsync(request, includeInactive);
                return Results.Ok(response);
            });

            app.MapPost("/api/orders", async (
                OrderCreateDto dto,
                IOrderService orderService) =>
            {
                var response = await orderService.CreateOrderAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapPut("/api/orders/{id:int}", async (
                int id,
                OrderUpdateDto dto,
                IOrderService orderService) =>
            {
                var response = await orderService.UpdateOrderAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapDelete("/api/orders/{id:int}", async (
                int id,
                IOrderService orderService) =>
            {
                var response = await orderService.RemoveOrderAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });
        }
    }
}
