using CustomerService.DTOs;
using CustomerService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using ProviGo.Common.Services;

namespace CustomerService.Services.Implementation
{
    public class CustomerService(
        TenantDbContext db,
        IGenericRepository<Customer> repo,
        BranchAccessService branchAccess) : ICustomerService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Customer> _repo = repo;
        private readonly BranchAccessService _branchAccess = branchAccess;

        // CREATE CUSTOMER
        public async Task<ApiResponse<CustomerResponseDto>> CreateCustomerAsync(CustomerCreateDto dto)
        {
            try
            {
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (!allowedBranches.Contains(dto.BranchId))
                    return ApiResponseFactory.Failure<CustomerResponseDto>(
                        "You don't have access to this branch");

                var emailExists = await _db.Customers
                    .AnyAsync(c =>
                        c.BranchId == dto.BranchId &&
                        c.Email == dto.Email);

                if (emailExists)
                    return ApiResponseFactory.Failure<CustomerResponseDto>(
                        "Customer email already exists");

                var customer = new Customer
                {
                    BranchId = dto.BranchId,
                    CustomerName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    JoinDate = dto.JoinDate == default
                        ? DateTime.UtcNow
                        : dto.JoinDate,
                    IsActive = true
                };

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();

                var response = new CustomerResponseDto
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.CustomerName,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Address = customer.Address,
                    JoinDate = customer.JoinDate,
                    IsActive = customer.IsActive
                };

                return ApiResponseFactory.Success(response, "Customer created successfully");
            }
            catch (Exception)
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
                    "Database error occurred",
                    new List<string> { ex.Message });
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

                var customer = await _db.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                    return ApiResponseFactory.Failure<string>("Customer not found");

                if (!allowedBranches.Contains(customer.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                var emailExists = await _db.Customers.AnyAsync(c =>
                    c.CustomerId != customerId &&
                    c.BranchId == customer.BranchId &&
                    c.Email == dto.Email);

                if (emailExists)
                    return ApiResponseFactory.Failure<string>(
                        "Customer email already exists");

                int affectedRows = await _db.Customers
                    .Where(c => c.CustomerId == customerId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.CustomerName, dto.FullName)
                        .SetProperty(c => c.Email, dto.Email)
                        .SetProperty(c => c.Phone, dto.Phone)
                        .SetProperty(c => c.Address, dto.Address)
                        .SetProperty(c => c.JoinDate, dto.JoinDate)
                        .SetProperty(c => c.IsActive, dto.IsActive)
                    );

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Update failed");

                return ApiResponseFactory.Success("Customer updated successfully");
            }
            catch (Exception)
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
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                    return ApiResponseFactory.Failure<string>("Customer not found");

                if (!allowedBranches.Contains(customer.BranchId))
                    return ApiResponseFactory.Failure<string>(
                        "You don't have access to this branch");

                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();

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