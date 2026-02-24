using OrderService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace OrderService.Services.Interface
{
    public interface IOrderService
    {
        Task<ApiResponse<List<OrderDto>>> GetOrdersAsync(
     PaginationRequest request,
     bool includeInactive, Guid tenantId);

        Task<ApiResponse<List<OrderDto>>> GetOrderByIdAsync(int orderId, Guid tenantId);

        Task<ApiResponse<string>> UpdateOrderAsync(int orderId, OrderUpdateDto dto, Guid tenantId);

        Task<ApiResponse<OrderDto>> CreateOrderAsync(OrderCreateDto dto, Guid tenantId);

        Task<ApiResponse<string>> RemoveOrderAsync(int orderId, Guid tenantId);

        Task<ApiResponse<string>> UpdatePaymentAsync(int orderId, decimal paidAmount, Guid tenantId);

        Task<ApiResponse<string>> UpdateRefundAsync(int orderId, decimal refundAmount, Guid tenantId);
    }
}
