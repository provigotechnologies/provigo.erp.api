using InvoiceService.DTOs;
using InvoiceService.Services;
using InvoiceService.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using InvoiceService.Shared.Enums;


namespace InvoiceService.Endpoints
{
    public static class InvoiceEndpoints
    {
        public static void Map(WebApplication app)
        {
            app.MapPost("/api/invoices/generate/{orderId}", async (
            int orderId,
            Guid BranchId,
            InvoiceProvider provider,
            IInvoiceService service) =>
            {
                var response = await service.GenerateFromOrderAsync(
                    orderId, BranchId,
                    provider.TenantId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });


            app.MapGet("/api/invoices", async (
              [AsParameters] PaginationRequest request,
              bool includeInactive,
              Guid branchId,
              InvoiceProvider invoiceProvider,
              [FromServices] IInvoiceService invoiceService) =>
            {
                var response = await invoiceService
                    .GetInvoicesAsync(request, includeInactive, branchId, invoiceProvider.TenantId);

                return Results.Ok(response);
            });

            app.MapPut("/api/invoices/{invoiceId}/cancel", async (
            int invoiceId,
            Guid branchId,
            InvoiceProvider provider,
            IInvoiceService service) =>
            {
                var response = await service.CancelInvoiceAsync(
                    invoiceId,
                    branchId,
                    provider.TenantId);

                return response.Success
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            });

            app.MapGet("/api/invoices/{invoiceId}/pdf", async (
            int invoiceId,
            Guid branchId,
            InvoiceProvider provider,
            [FromQuery] string paperSize, // <-- added query param
            IInvoiceService service) =>
            {
                // Convert string to enum
                PaperSize size = paperSize?.ToUpper() switch
                {
                    "THERMAL58" => PaperSize.Thermal58,
                    "THERMAL80" => PaperSize.Thermal80,
                    "A5" => PaperSize.A5,
                    "LETTER" => PaperSize.Letter,
                    _ => PaperSize.A4 // default
                };

                var pdfBytes = await service.GenerateInvoicePdfAsync(
                    invoiceId,
                    branchId,
                    provider.TenantId,
                    size); // pass dynamic size

                return Results.File(
                    pdfBytes,
                    "application/pdf",
                    $"Invoice-{invoiceId}.pdf");
            });
        }


    }
}