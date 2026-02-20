using FeeService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace FeeService.Services.Interface
{
    public interface IFeeService
    {
       // Task<ApiResponse<List<FeePayment>>> GetFeePaymentsAsync(
       // PaginationRequest request,
       //bool includeInactive);

        /*Task<ApiResponse<string>> UpdateFeePaymentAsync(
        int FeePaymentId, FeePaymentDto dto);*/
        Task<ApiResponse<FeePaymentResponseDto>> CreateFeePaymentAsync(FeePaymentDto dto);
        Task<ApiResponse<string>> RemoveFeePaymentAsync(int FeePaymentId);
    }
}
