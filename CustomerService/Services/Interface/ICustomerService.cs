using IdentityService.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using CustomerService.DTOs;

namespace CustomerService.Services.Interface
{
    public interface ICustomerService
    {
        Task<ApiResponse<List<Customer>>> GetCustomersAsync(PaginationRequest request, bool includeInactive);

        Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerCreateDto dto);

        Task<ApiResponse<string>> UpdateCustomerAsync(int customerId, CustomerUpdateDto dto);

        Task<ApiResponse<string>> RemoveCustomerAsync(int customerId);
    }
}

