using CustomerService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace CustomerService.Services.Interface
{
    public interface ICustomerService
    {
        Task<ApiResponse<CustomerResponseDto>> GetCustomerByIdAsync(
            int customerId,
            bool includeInactive,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<List<Customer>>> GetCustomersAsync(
            PaginationRequest request,
            bool includeInactive,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<CustomerResponseDto>> CreateCustomerAsync(
            CustomerCreateDto dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> UpdateCustomerAsync(
            int customerId,
            CustomerUpdateDto dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<string>> RemoveCustomerAsync(
            int customerId,
            Guid branchId,
            Guid tenantId);
    }
}
