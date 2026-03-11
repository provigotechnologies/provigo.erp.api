using OrderService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace OrderService.Services.Interface
{
    public interface IOrderService
    {
        Task<ApiResponse<List<OrderResponseDto>>> GetOrdersAsync(
             PaginationRequest request,
             bool includeInactive,
             Guid branchId);

        Task<ApiResponse<List<OrderResponseDto>>> GetOrderByIdAsync(
            int orderId,
            Guid branchId);

        Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(OrderCreateDto dto);

        Task<ApiResponse<string>> UpdateOrderAsync(
            int orderId,
            OrderUpdateDto dto,
            Guid branchId);

        Task<ApiResponse<string>> RemoveOrderAsync(
            int orderId,
            Guid branchId);

        Task<ApiResponse<string>> UpdatePaymentAsync(
            int orderId,
            decimal paidAmount,
            Guid branchId);

        Task<ApiResponse<string>> UpdateRefundAsync(
            int orderId,
            decimal refundAmount,
            Guid branchId);
    }
}