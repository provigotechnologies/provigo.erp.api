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
     IGenericRepository<Customer> repo) : ICustomerService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Customer> _repo = repo;

        public async Task<ApiResponse<CustomerResponseDto>> CreateCustomerAsync(CompanyCreateDto dto, Guid tenantId)
        {
            try
            {

                // 🔒 Duplicate email check
                var emailExists = await _db.Customers
                    .AnyAsync(i => i.TenantId == tenantId
            && i.Email == dto.Email);

                if (emailExists)
                {
                    return ApiResponseFactory.Failure<CustomerResponseDto>(
                        "This email is already registered."
                    );
                }

                var customer = new Customer
                {
                    TenantId = tenantId,
                    BranchId = dto.BranchId,
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
                    return ApiResponseFactory.Failure<CustomerResponseDto>("Insert failed");

                // Return DTO with generated ID
                var responseDto = new CustomerResponseDto
                {
                    CustomerId = customer.CustomerId,
                    TenantId = customer.TenantId,
                    BranchId = customer.BranchId,
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

                return ApiResponseFactory.Failure<CustomerResponseDto>("Database error occurred");

            }

        }

        public async Task<ApiResponse<List<Customer>>> GetCustomersAsync(
         PaginationRequest request,
         bool includeInactive,
         Guid branchId,
         Guid tenantId)
        {
            try
            {
                var query = _db.Customers
                    .Where(p =>
                        p.TenantId == tenantId &&
                        p.BranchId == branchId)
                    .AsNoTracking();

                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Branches fetched successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Customer>>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }


        public async Task<ApiResponse<string>> UpdateCustomerAsync(
        int customerId,
        CustomerUpdateDto dto,
        Guid tenantId)
        {
            try
            {
                var exists = await _db.Customers.AnyAsync(p =>
                    p.TenantId == tenantId &&
                    p.CustomerId != customerId);

                if (exists)
                    return ApiResponseFactory.Failure<string>("Customer already exists");

                int affectedRows = await _db.Customers
                .Where(i => i.CustomerId == customerId && i.TenantId == tenantId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.FullName, dto.FullName)
                    .SetProperty(i => i.Email, dto.Email)
                    .SetProperty(i => i.Phone, dto.Phone)
                    .SetProperty(i => i.Address, dto.Address)
                    .SetProperty(i => i.JoinDate, dto.JoinDate)
                    .SetProperty(i => i.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Customer not found");

                return ApiResponseFactory.Success("Customer updated successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }

        public async Task<ApiResponse<string>> RemoveCustomerAsync(int customerId, Guid tenantId)
        {
            try
            {
                var customer = await _db.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId
                                           && c.TenantId == tenantId);

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