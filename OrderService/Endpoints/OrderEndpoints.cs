using IdentityService.Data;
using OrderService.DTOs;
using OrderService.Services.Interface;
using ProviGo.Common.Pagination;
using OrderService.Services;

namespace OrderService.Endpoints
{
    public class OrderEndpoints
    {
        public static void Map(WebApplication app)
        {
            // 🔹 Get single order by ID
            app.MapGet("/api/orders/{id:int}", async (
                int id,
                OrderProvider orderProvider,
                IOrderService orderService) =>
            {
                var response = await orderService.GetOrderByIdAsync(id, orderProvider.TenantId);
                return response != null ? Results.Ok(response) : Results.NotFound();
            });

            // 🔹 List Order
            app.MapGet("/api/orders", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                OrderProvider orderProvider,
                IOrderService orderService) =>
            {
                var response = await orderService.GetOrdersAsync(request, includeInactive, orderProvider.TenantId);
                return Results.Ok(response);
            });

            // 🔹 Create Order
            app.MapPost("/api/orders", async (
                OrderCreateDto dto,
                OrderProvider orderProvider,
                IOrderService orderService) =>
            {
                var response = await orderService.CreateOrderAsync(dto, orderProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Update Order
            app.MapPut("/api/orders/{id:int}", async (
                int id,
                OrderUpdateDto dto,
                OrderProvider orderProvider,
                IOrderService orderService) =>
            {
                var response = await orderService.UpdateOrderAsync(id, dto, orderProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Delete Order
            app.MapDelete("/api/orders/{id:int}", async (
                int id,
                OrderProvider orderProvider,
                IOrderService orderService) =>
            {
                var response = await orderService.RemoveOrderAsync(id, orderProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Update Payment
            app.MapPut("/api/orders/{orderId:int}/update-payment", async (
            int orderId,
            OrderPaymentUpdateDto dto,
            OrderProvider orderProvider,
            IOrderService orderService) =>
            {
                var response = await orderService.UpdatePaymentAsync(orderId, dto.PaidAmount, orderProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Update Refund
            app.MapPut("/api/orders/{orderId:int}/update-refund", async (
            int orderId,
            OrderRefundUpdateDto dto,
            OrderProvider orderProvider,
            IOrderService orderService) =>
            {
                var response = await orderService.UpdateRefundAsync(orderId, dto.RefundAmount, orderProvider.TenantId);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });
        }
    }
}
