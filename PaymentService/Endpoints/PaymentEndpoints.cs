using PaymentService.DTOs;
using PaymentService.Services.Interface;
using ProviGo.Common.Pagination;

namespace PaymentService.Endpoints
{
    public class PaymentEndpoints
    {
        public static void Map(WebApplication app)
        {
            // 🔹 Create payment
            app.MapGet("/api/payments", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                IPaymentService paymentService) =>
            {
                var response = await paymentService.GetPaymentsAsync(request, includeInactive);
                return Results.Ok(response);
            });

            // 🔹 Get payment by id
            app.MapGet("/api/payments/{id:int}", async (
                int id,
                IPaymentService paymentService) =>
            {
                var response = await paymentService.GetPaymentByIdAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            });

            // 🔹 Add transaction
            app.MapPost("/api/payments/{id:int}/transactions", async (
                int id,
                PaymentTransactionCreateDto dto,
                IPaymentService paymentService) =>
            {
                var response = await paymentService.AddTransactionAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Create refund
            app.MapPost("/api/payments/refund", async (
                RefundCreateDto dto,
                IPaymentService paymentService) =>
            {
                var response = await paymentService.CreateRefundAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Get payment
            app.MapPost("/api/payments", async (
                PaymentCreateDto dto,
                IPaymentService paymentService) =>
            {
                var response = await paymentService.CreatePaymentAsync(dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Update payment
            app.MapPut("/api/payments/{id:int}", async (
                int id,
                PaymentUpdateDto dto,
                IPaymentService paymentService) =>
            {
                var response = await paymentService.UpdatePaymentAsync(id, dto);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            // 🔹 Delete payment
            app.MapDelete("/api/payments/{id:int}", async (
                int id,
                IPaymentService paymentService) =>
            {
                var response = await paymentService.RemovePaymentAsync(id);
                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });
        }

    }
}
