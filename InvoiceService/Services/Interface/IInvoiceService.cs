using InvoiceService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace InvoiceService.Services.Interface
{
    public interface IInvoiceService
    {

        Task<ApiResponse<List<Invoice>>> GetInvoicesAsync(
            PaginationRequest request,
            bool includeInactive,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> CancelInvoiceAsync(
            int invoiceId,
            Guid branchId,
            Guid tenantId);

        // ✅ ADD THESE
        Task<ApiResponse<InvoiceResponseDto>> GenerateFromOrderAsync(
            int orderId,
            Guid branchId,
            Guid tenantId);

        Task<byte[]> GenerateInvoicePdfAsync(
            int invoiceId,
            Guid branchId,
            Guid tenantId,
            PaperSize paperSize = PaperSize.A4);
    }
}
