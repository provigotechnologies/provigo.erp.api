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
     bool includeInactive);

        Task<ApiResponse<string>> UpdateOrderAsync(int orderId, OrderUpdateDto dto);

        Task<ApiResponse<OrderDto>> CreateOrderAsync(OrderCreateDto dto);

        Task<ApiResponse<string>> RemoveOrderAsync(int orderId);
    }
}
