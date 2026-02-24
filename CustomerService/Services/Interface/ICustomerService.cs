using CustomerService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace CustomerService.Services.Interface
{
    public interface ICustomerService
    {
        Task<ApiResponse<List<Customer>>> GetCustomersAsync(PaginationRequest request, bool includeInactive, Guid tenantId, Guid branchId);

        Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerCreateDto dto, Guid tenantId);

        Task<ApiResponse<string>> UpdateCustomerAsync(int customerId, CustomerUpdateDto dto, Guid tenantId);

        Task<ApiResponse<string>> RemoveCustomerAsync(int customerId, Guid tenantId);
    }
}
