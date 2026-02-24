using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services;
using PaymentService.Services.Interface;
using ProviGo.Common.Pagination;
using PaymentService.Utils;

namespace PaymentService.Endpoints
{
    public class PaymentEndpoints
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/api/payments", async (
                [AsParameters] PaginationRequest request,
                bool includeInactive,
                PaymentProvider paymentProvider,
                 [FromServices] IPaymentService paymentService) =>
            {
                var response = await paymentService.GetPaymentsAsync(request, includeInactive, paymentProvider.TenantId);
                return Results.Ok(response);
            });


            app.MapPost("/api/paymentTransactions",
                async ([FromBody] PaymentTransactionRequestCreateDto req,
                       PaymentProvider paymentProvider,
                       IPaymentService service) => {
                try
                {
                    if (req.Amount <= 0) return Results.BadRequest(new { message = "Invalid amount" });

                    var resp = await service.CreateOnlinePaymentAsync(req, paymentProvider.TenantId);
                               return Results.Ok(resp);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }

            });

            app.MapPost("/api/payments",
            async (PaymentCreateDto dto,
                   IPaymentService service,
                   PaymentProvider paymentProvider,
                   HttpContext context) =>
            {
                try
                {
                    if (dto.PaidAmount <= 0)
                        return Results.BadRequest(new { message = "Invalid amount" });

                    var result = await service.CreateOfflinePaymentAsync(dto, paymentProvider.TenantId);

                    if (!result.Success)
                        return Results.BadRequest(result);

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });


            app.MapPost("/api/paymentTransaction/verify",
    async (VerifyPaymentTransactionRequestDto dto,
           IPaymentService service,
           PaymentProvider paymentProvider) =>
    {
        var ok = await service.VerifyAndSavePaymentTransactionAsync(dto, paymentProvider.TenantId);

        if (ok) return Results.Ok(new { status = "success" });
        return Results.BadRequest(new { status = "failed" });
    });


            app.MapPost("/api/payments/refund",
            async (RefundCreateDto dto,
                   IPaymentService service,
                   PaymentProvider paymentProvider,
                   HttpContext context) =>
            {
                if (dto.RefundAmount <= 0)
                    return Results.BadRequest(new { message = "Invalid refund amount" });

                var result = await service.CreateRefundAsync(dto, paymentProvider.TenantId);

                if (!result.Success)
                    return Results.BadRequest(result);

                return Results.Ok(result);
            });


            app.MapPost("/api/paymentTransaction/generate-signature",
    (VerifyPaymentTransactionRequestDto dto, IConfiguration config) =>
    {
        var secret = config["Razorpay:KeySecret"];
        var data = $"{dto.razorpay_order_id}|{dto.razorpay_payment_id}";
        var signature = SignatureHelper.CreateSignature(data, secret ?? "");

        return Results.Ok(new { generatedSignature = signature });
    });

        }

    }
}
