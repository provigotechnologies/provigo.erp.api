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
             Guid branchId,
             Guid tenantId);

        Task<ApiResponse<List<OrderResponseDto>>> GetOrderByIdAsync(
            int orderId,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> UpdateOrderAsync(
            int orderId,
            OrderUpdateDto dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(
            OrderCreateDto dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> RemoveOrderAsync(
            int orderId,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> UpdatePaymentAsync(
            int orderId,
            decimal paidAmount,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> UpdateRefundAsync(
            int orderId,
            decimal refundAmount,
            Guid branchId,
            Guid tenantId);
    }
}