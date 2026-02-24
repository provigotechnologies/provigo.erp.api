using PaymentService.DTOs;
using PaymentService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace PaymentService.Services.Interface
{
    public interface IPaymentService
    {
        Task<ApiResponse<List<Payment>>> 
            GetPaymentsAsync(PaginationRequest request, bool includeInactive, Guid tenantId);

        Task<ApiResponse<List<OrderPaymentHistoryDto>>>
            GetOrderPaymentHistoryAsync(int orderId, Guid tenantId);

        Task<PaymentTransactionResponseCreateDto>
            CreateOnlinePaymentAsync(PaymentTransactionRequestCreateDto request, Guid tenantId);

        Task<bool>
            VerifyOnlinePaymentAsync(VerifyPaymentTransactionRequestDto dto, Guid tenantId);

        Task<ApiResponse<PaymentDto>>
            CreateOfflinePaymentAsync(PaymentCreateDto dto, Guid tenantId);

        Task<ApiResponse<RefundDto>>
            CreateOnlineRefundAsync(RefundCreateDto dto, Guid tenantId);

        Task<bool>
            VerifyOnlineRefundAsync(string gatewayRefundId, Guid tenantId);

        Task<ApiResponse<RefundDto>>
            CreateOfflineRefundAsync(RefundCreateDto dto, Guid tenantId);
    }
}

