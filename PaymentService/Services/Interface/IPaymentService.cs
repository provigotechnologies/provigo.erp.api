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
            GetPaymentsAsync(PaginationRequest request, bool includeInactive, Guid branchId, Guid tenantId);

        Task<ApiResponse<List<OrderPaymentHistoryDto>>>
            GetOrderPaymentHistoryAsync(int orderId, Guid branchId, Guid tenantId);

        Task<PaymentTransactionResponseCreateDto>
            CreateOnlinePaymentAsync(PaymentTransactionCreateDto request, Guid branchId, Guid tenantId);

        Task<bool>
            VerifyOnlinePaymentAsync(VerifyPaymentTransactionRequestDto dto, Guid branchId, Guid tenantId);

        Task<ApiResponse<PaymentResponseDto>>
            CreateOfflinePaymentAsync(PaymentCreateDto dto, Guid branchId, Guid tenantId);

        Task<ApiResponse<RefundResponseDto>>
            CreateOnlineRefundAsync(RefundCreateDto dto, Guid branchId, Guid tenantId);

        Task<bool>
            VerifyOnlineRefundAsync(string gatewayRefundId, Guid branchId, Guid tenantId);

        Task<ApiResponse<RefundResponseDto>>
            CreateOfflineRefundAsync(RefundCreateDto dto, Guid branchId, Guid tenantId);
    }
}

