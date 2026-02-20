using CustomerService.DTOs;
using CustomerService.DTOs;
using CustomerService.Services.Interface;
using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace CustomerService.Services.Implementation
{
    public class CustomerService(
  TenantDbContext db,
  IGenericRepository<Customer> repo,
  IIdentityProvider identityProvider) : ICustomerService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Customer> _repo = repo;
        private readonly IIdentityProvider _identityProvider = identityProvider;
        private Guid TenantId => _identityProvider.TenantId;

        public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerCreateDto dto)
        {
            try
            {

                // 🔒 Duplicate email check
                var emailExists = await _db.Customers
                    .AnyAsync(i => i.TenantId == TenantId
            && i.Email == dto.Email);

                if (emailExists)
                {
                    return ApiResponseFactory.Failure<CustomerDto>(
                        "This email is already registered."
                    );
                }

                var customer = new Customer
                {
                    TenantId = TenantId,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    IsActive = true,
                    JoinDate = dto.JoinDate == default
                               ? DateTime.UtcNow
                               : dto.JoinDate
                };

                _db.Customers.Add(customer);
                int affectedRows = await _db.SaveChangesAsync();
                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<CustomerDto>("Insert failed");

                // Return DTO with generated ID
                var responseDto = new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    TenantId = customer.TenantId,
                    FullName = customer.FullName,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Address = customer.Address,
                    JoinDate = customer.JoinDate,
                    IsActive = customer.IsActive
                };

                return ApiResponseFactory.Success(
                    responseDto,
                    "Customer created successfully"
                );
            }
            catch (DbUpdateException ex)
            {

                return ApiResponseFactory.Failure<CustomerDto>("Database error occurred");

            }

        }

        public async Task<ApiResponse<List<Customer>>> GetCustomersAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.Customers
                    .Where(c => c.TenantId == TenantId)
                    .AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Customers fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<Customer>>(ex.Message, ["Database error occurred"]);

            }
        }

        public async Task<ApiResponse<string>> UpdateCustomerAsync(int customerId, CustomerUpdateDto dto)
        {
            try
            {
                int affectedRows = await _db.Customers
                .Where(i => i.CustomerId == customerId && i.TenantId == TenantId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.FullName, dto.FullName)
                    .SetProperty(i => i.Email, dto.Email)
                    .SetProperty(i => i.Phone, dto.Phone)
                    .SetProperty(i => i.Address, dto.Address)
                    .SetProperty(i => i.JoinDate, dto.JoinDate)
                    .SetProperty(i => i.IsActive, dto.IsActive)
                );
                if (affectedRows == 0)
                {
                    throw new NotFoundException("Customer not found");
                    //return ApiResponseFactory.Failure<string>("Institute not found");
                    // return ApiResponseFactory.Failure<string>("Update failed");
                }

                return ApiResponseFactory.Success("Customer updated successfully");
            }
            catch (DbUpdateException)
            {

                return ApiResponseFactory.Failure<string>("Database error occurred");

            }

        }

        public async Task<ApiResponse<string>> RemoveCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _db.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId
                                           && c.TenantId == TenantId);

                if (customer == null)
                {
                    return ApiResponseFactory.Failure<string>("Customer not found");
                }

                _db.Customers.Remove(customer);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Customer {customerId} deleted successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message }
                );
            }
        }



    }
}