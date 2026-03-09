using CustomerService.DTOs;
using CustomerService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Response;
using ProviGo.Common.Services;

namespace CustomerService.Services.Implementation
{
    public class CustomerService(
        TenantDbContext db,
        IGenericRepository<Customer> repo,
        TenantProvider tenantProvider,
        BranchAccessService branchAccess) : ICustomerService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Customer> _repo = repo;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly BranchAccessService _branchAccess = branchAccess;


        // CREATE CUSTOMER
        public async Task<ApiResponse<CustomerResponseDto>> CreateCustomerAsync(CustomerCreateDto dto)
        {
            try 
            {
                    var tenantId = _tenantProvider.TenantId;

                    var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                    if (!allowedBranches.Contains(dto.BranchId))
                    {
                        return ApiResponseFactory.Failure<CustomerResponseDto>(
                            "You don't have access to this branch");
                    }

                    var emailExists = await _db.Customers
                        .AnyAsync(i =>
                            i.TenantId == tenantId &&
                            i.BranchId == dto.BranchId &&
                            i.Email == dto.Email);

                    if (emailExists)
                    {
                        return ApiResponseFactory.Failure<CustomerResponseDto>(
                            "This email is already registered.");
                    }

                    var customer = new Customer
                    {
                        TenantId = tenantId,
                        BranchId = dto.BranchId,
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
                    await _db.SaveChangesAsync();

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
                        "Customer created successfully");
                }
            catch (DbUpdateException ex)
            {
                return ApiResponseFactory.Failure<CustomerResponseDto>("Database error occurred");
            }
        }



        // GET CUSTOMERS
        public async Task<ApiResponse<List<Customer>>> GetCustomersAsync(
             PaginationRequest request,
             bool includeInactive)
        {

            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var query = _db.Customers
                    .Where(c => allowedBranches.Contains(c.BranchId))
                    .AsNoTracking();

                var paged = await _repo.GetPagedAsync(
                    query,
                    request,
                    c => includeInactive || c.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    paged,
                    "Customers fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Customer>>(
                    ex.Message,
                    ["Database error occurred"]);
            }  
        }


        // UPDATE CUSTOMER
        public async Task<ApiResponse<string>> UpdateCustomerAsync(
             int customerId,
             CustomerUpdateDto dto)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var exists = await _db.Customers.AnyAsync(c =>
                    allowedBranches.Contains(c.BranchId) &&
                    c.CustomerId != customerId &&
                    c.Email == dto.Email);

                if (exists)
                    return ApiResponseFactory.Failure<string>("Customer email already exists");

                int affectedRows = await _db.Customers
                    .Where(c =>
                        c.CustomerId == customerId &&
                        allowedBranches.Contains(c.BranchId))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.CustomerName, dto.FullName)
                        .SetProperty(c => c.Email, dto.Email)
                        .SetProperty(c => c.Phone, dto.Phone)
                        .SetProperty(c => c.Address, dto.Address)
                        .SetProperty(c => c.JoinDate, dto.JoinDate)
                        .SetProperty(c => c.IsActive, dto.IsActive)
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


        // DELETE CUSTOMER
        public async Task<ApiResponse<string>> RemoveCustomerAsync(int customerId)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                var customer = await _db.Customers
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == customerId &&
                    allowedBranches.Contains(c.BranchId));

                if (customer == null)
                    return ApiResponseFactory.Failure<string>("Customer not found");

                _db.Customers.Remove(customer);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Delete failed");

                return ApiResponseFactory.Success(
                    $"Customer {customerId} deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message });
            }
        }


    }
}




 
