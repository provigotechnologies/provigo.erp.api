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


        //  Create Customers
        public async Task<ApiResponse<CustomerResponseDto>> CreateCustomerAsync(CustomerCreateDto dto, Guid branchId, Guid tenantId)
        {
            try
            {

                // 🔒 Duplicate email check
                var emailExists = await _db.Customers
                .AnyAsync(i => i.TenantId == tenantId
               && i.BranchId == branchId
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
                    BranchId = branchId,
                    CustomerName = dto.FullName,
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
                    FullName = customer.CustomerName,
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


        //  Get All Customers
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


        //  Get Customer By Id
        public async Task<ApiResponse<CustomerResponseDto>> GetCustomerByIdAsync(
        int customerId,
        bool includeInactive,
        Guid branchId,
        Guid tenantId)
        {
            try
            {
                var customer = await _db.Customers
                    .AsNoTracking()
                    .Where(c =>
                        c.CustomerId == customerId &&
                        c.BranchId == branchId &&
                        c.TenantId == tenantId)
                    .Where(c => includeInactive || c.IsActive)
                    .Select(c => new CustomerResponseDto
                    {
                        CustomerId = c.CustomerId,
                        FullName = c.CustomerName,
                        Phone = c.Phone,
                        Email = c.Email,
                        IsActive = c.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (customer == null)
                    return ApiResponseFactory.Failure<CustomerResponseDto>(
                        "Customer not found");

                return ApiResponseFactory.Success(
                    customer,
                    "Customer fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<CustomerResponseDto>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }


        //  Update Customer 
        public async Task<ApiResponse<string>> UpdateCustomerAsync(
        int customerId,
        CustomerUpdateDto dto,
        Guid branchId,
        Guid tenantId)
        {
            try
            {
                var exists = await _db.Customers.AnyAsync(c =>
                    c.TenantId == tenantId &&
                    c.BranchId == branchId &&
                    c.CustomerId != customerId && c.Email == dto.Email);

                if (exists)
                    return ApiResponseFactory.Failure<string>("Customer already exists");

                int affectedRows = await _db.Customers
                .Where(i => i.CustomerId == customerId && i.TenantId == tenantId && i.BranchId == branchId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.CustomerName, dto.FullName)
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


        //  Remove Customer 
        public async Task<ApiResponse<string>> RemoveCustomerAsync(
            int customerId,
            Guid branchId, 
            Guid tenantId)
        {
            try
            {
                var customer = await _db.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId
                                && c.BranchId == branchId && c.TenantId == tenantId);

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