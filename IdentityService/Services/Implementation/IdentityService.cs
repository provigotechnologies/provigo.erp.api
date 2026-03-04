using BCrypt.Net;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Services.Interface;
using IdentityService.Utils;
using Microsoft.EntityFrameworkCore;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace IdentityService.Services.Implementation
{
    public class IdentityService(
        TenantDbContext db,
        IGenericRepository<User> repo,
        TokenService tokenService) : IIdentityService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<User> _repo = repo;
        private readonly TokenService _tokenService = tokenService;


        public async Task<ApiResponse<UserResponse>> RegisterAsync(
            UserCreateRequest dto,
            Guid branchId,
            Guid tenantId)
        {
            try
            {
                // 🔒 Duplicate email check (Tenant wise)
                var emailExists = await _db.Users
                    .AnyAsync(u => u.Email == dto.Email
                                && u.BranchId == branchId
                                && u.TenantId == tenantId);

                if (emailExists)
                {
                    return ApiResponseFactory.Failure<UserResponse>(
                        "This email is already registered."
                    );
                }

                // 🔒 Role validation
                var roleExists = await _db.UserRoles
                    .AnyAsync(r => r.Id == dto.RoleId);

                if (!roleExists)
                {
                    return ApiResponseFactory.Failure<UserResponse>(
                        "Invalid role selected."
                    );
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    TenantId = tenantId,
                    BranchId = branchId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = dto.RoleId,
                    UserCategory = dto.UserCategory,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };

                _db.Users.Add(user);

                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<UserResponse>(
                        "Insert failed"
                    );

                // Response DTO
                var response = new UserResponse
                {
                    Id = user.UserId,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return ApiResponseFactory.Success(
                    response,
                    "User registered successfully"
                );
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<UserResponse>(
                    "Database error occurred"
                );
            }
        }

        public async Task<ApiResponse<object>> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
     .Include(u => u.UserRole)
     .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return ApiResponseFactory.Failure<object>("Invalid credentials");

            if (!user.IsActive)
                return ApiResponseFactory.Failure<object>("User inactive");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return ApiResponseFactory.Failure<object>("Invalid credentials");

            var token = _tokenService.Create(user);

            _db.UsersLogs.Add(new UsersLog
            {
                UserId = user.UserId,
                EventMessage = $"User {user.Email} logged in",
                EventTime = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success<object>(new { token });
        }

        public async Task<ApiResponse<List<User>>> GetUsersAsync(
            PaginationRequest request,
            bool includeInactive,
            Guid branchId,
            Guid tenantId)
        {
            try
            {
                var query = _db.Users
                    .AsNoTracking()
                    .Where(u => u.TenantId == tenantId && u.BranchId == branchId);

                var pagedResult = await _repo.GetPagedAsync(
                    query,
                    request,
                    i => includeInactive || i.IsActive
                );

                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Users fetched successfully"
                );
            }
            catch (Exception)
            {
                return ApiResponseFactory.Failure<List<User>>(
                    "Database error occurred"
                );
            }
        }


        public async Task<ApiResponse<string>> UpdateUserAsync(
            Guid id,
            UserUpdateRequest dto,
            Guid branchId,
            Guid tenantId)
        {
            try
            {
                int affectedRows = await _db.Users
                    .Where(u => u.UserId == id && u.TenantId == tenantId && u.BranchId == branchId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(u => u.FirstName, dto.FirstName)
                        .SetProperty(u => u.LastName, dto.LastName)
                        .SetProperty(u => u.PhoneNumber, dto.PhoneNumber)
                        .SetProperty(u => u.RoleId, dto.RoleId)
                        .SetProperty(u => u.IsActive, dto.IsActive)
                        .SetProperty(u => u.LastUpdatedAt, DateTime.UtcNow)
                    );

                if (affectedRows == 0)
                    throw new NotFoundException("User not found");

                return ApiResponseFactory.Success(
                    "User updated successfully"
                );
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred"
                );
            }
        }

        public async Task<ApiResponse<string>> DeleteUserAsync(Guid id, Guid branchId, Guid tenantId)
        {
            try
            {
                var user = await _db.Users
                    .FirstOrDefaultAsync(u => u.UserId == id && u.TenantId == tenantId && u.BranchId == branchId);

                if (user == null)
                    return ApiResponseFactory.Failure<string>("User not found");

                _db.Users.Remove(user);
                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                    return ApiResponseFactory.Failure<string>("Delete failed");

                return ApiResponseFactory.Success("User deleted successfully");
            }
            catch (Exception)
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }

        public async Task<ApiResponse<List<object>>> GetRolesAsync()
        {
            var roles = await _db.UserRoles
                .Select(r => new { r.Id, r.RoleName })
                .ToListAsync();

            return ApiResponseFactory.Success<List<object>>(roles.Cast<object>().ToList());
        }

        public async Task<ApiResponse<List<object>>> GetLogsAsync()
        {
            var logs = await _db.UsersLogs
                .OrderByDescending(l => l.EventTime)
                .Select(l => new { l.EventMessage, l.EventTime })
                .ToListAsync();

            return ApiResponseFactory.Success<List<object>>(logs.Cast<object>().ToList());
        }

        public async Task<ApiResponse<string>> LogoutAsync(LogoutRequest dto)
        {
            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure<string>("User not found");

            _db.UsersLogs.Add(new UsersLog
            {
                UserId = user.UserId,
                EventMessage = $"User {user.Email} logged out",
                EventTime = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Logout logged successfully");
        }
    }
}
