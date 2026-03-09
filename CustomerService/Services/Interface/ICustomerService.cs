using CustomerService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace CustomerService.Services.Interface
{
    public interface ICustomerService
    {
        Task<ApiResponse<List<Customer>>> GetCustomersAsync(
            PaginationRequest request,
            bool includeInactive);

        Task<ApiResponse<CustomerResponseDto>> CreateCustomerAsync(
            CustomerCreateDto dto);

        Task<ApiResponse<string>> UpdateCustomerAsync(
            int customerId,
            CustomerUpdateDto dto);

        Task<ApiResponse<string>> RemoveCustomerAsync(
            int customerId);
    }
}