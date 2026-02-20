using PaymentService.DTOs;
using PaymentService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace PaymentService.Services.Interface
{
    public interface IPaymentService
    {
        // 🔹 Get all payments 
        Task<ApiResponse<List<Payment>>> GetPaymentsAsync(
            PaginationRequest request,
            bool includeInactive);

        // 🔹 Get single payment with transactions
        Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(int paymentId);

        // 🔹 Create payment (Always Pending)
        Task<ApiResponse<PaymentDto>> CreatePaymentAsync(
            PaymentCreateDto dto);

        // 🔹 Add payment transaction 
        Task<ApiResponse<PaymentTransactionDto>> AddTransactionAsync(
            int paymentId,
            PaymentTransactionCreateDto dto);

        // 🔹 Update only TotalPayable 
        Task<ApiResponse<string>> UpdatePaymentAsync(
            int paymentId,
            PaymentUpdateDto dto);

        // 🔹 Delete payment
        Task<ApiResponse<string>> RemovePaymentAsync(int paymentId);

        // 🔹 Refund payment
        Task<ApiResponse<RefundDto>> CreateRefundAsync(RefundCreateDto dto);

    }
}

