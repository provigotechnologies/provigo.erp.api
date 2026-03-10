using BCrypt.Net;
using IdentityService.DTOs;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Response;
using ProviGo.Common.Services;

namespace IdentityService.Services.Implementation
{
    public class IdentityService(
        TenantDbContext db,
        IGenericRepository<User> repo,
        TenantProvider tenantProvider,
        TokenService tokenService,
        CurrentUserService currentUser) : IIdentityService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<User> _repo = repo;
        private readonly TokenService _tokenService = tokenService;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly CurrentUserService _currentUser = currentUser;

        // ---------------- REGISTER ----------------
        public async Task<ApiResponse<UserResponse>> RegisterAsync(UserCreateRequest dto, List<Guid> branchIds)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;

                // Email duplicate check
                var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.TenantId == tenantId);
                if (emailExists)
                    return ApiResponseFactory.Failure<UserResponse>("Email already registered.");

                // Validate role
                var role = await _db.UserRoles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
                if (role == null)
                    return ApiResponseFactory.Failure<UserResponse>("Invalid role.");

                // Determine branches based on role
                List<Guid> assignedBranches = role.RoleName switch
                {
                    "SuperAdmin" => new List<Guid>(), // no branches assigned
                    "Admin" when branchIds != null && branchIds.Any() => branchIds,
                    "Admin" => throw new Exception("Admin must be assigned at least one branch"),
                    _ when branchIds != null && branchIds.Count == 1 => branchIds,
                    _ => throw new Exception("User must be assigned exactly one branch")
                };

                // Create user
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    TenantId = tenantId,
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

                await _db.Users.AddAsync(user);

                // Assign branches 
                if (assignedBranches.Any() && role.RoleName != "SuperAdmin")
                {
                    // Only branches that exist in tenant
                    var validBranches = await _db.Branches
                        .Where(b => b.TenantId == tenantId && assignedBranches.Contains(b.BranchId))
                        .Select(b => b.BranchId)
                        .ToListAsync();

                    var newBranches = validBranches
                        .Select(b => new UserBranch
                        {
                            UserId = user.UserId,
                            BranchId = b,
                            IsActive = true
                        });

                    if (newBranches.Any())
                        await _db.UserBranches.AddRangeAsync(newBranches);
                }

                await _db.SaveChangesAsync();

                // Response
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

                return ApiResponseFactory.Success(response, "User registered successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<UserResponse>("Registration failed", new List<string> { ex.Message });
            }
        }


        // ---------------- LOGIN ----------------
        public async Task<ApiResponse<object>> LoginAsync(LoginDto dto)
        {
            var tenantId = _tenantProvider.TenantId;

            var user = await _db.Users
             .Include(u => u.UserRole)
             .FirstOrDefaultAsync(u => u.Email == dto.Email && u.TenantId == tenantId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return ApiResponseFactory.Failure<object>("Invalid credentials");

            if (!user.IsActive)
                return ApiResponseFactory.Failure<object>("User inactive");

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

        // ---------------- GET USERS ----------------
        public async Task<ApiResponse<List<User>>> GetUsersAsync(PaginationRequest request, bool includeInactive)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;
                var currentUserId = _currentUser.UserId;

                var currentUser = await _db.Users
                    .Include(u => u.UserRole)
                    .Include(u => u.UserBranches)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                    return ApiResponseFactory.Failure<List<User>>("Current user not found");

                IQueryable<User> query = _db.Users.AsNoTracking()
                    .Where(u => u.TenantId == tenantId);

                // Role-based filtering
                if (currentUser.UserRole.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminBranchIds = currentUser.UserBranches.Select(ub => ub.BranchId).ToList();
                    query = query.Where(u => u.UserBranches.Any(ub => adminBranchIds.Contains(ub.BranchId)));
                }
                else if (!currentUser.UserRole.RoleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    var userBranchId = currentUser.UserBranches.FirstOrDefault()?.BranchId;
                    query = query.Where(u => u.UserBranches.Any(ub => ub.BranchId == userBranchId));
                }

                if (!includeInactive)
                    query = query.Where(u => u.IsActive);

                var pagedResult = await _repo.GetPagedAsync(query, request);
                return ApiResponseFactory.PagedSuccess(pagedResult, "Users fetched success+fully");
            }
            catch
            {
                return ApiResponseFactory.Failure<List<User>>("Database error occurred");
            }   
        }

        // ---------------- UPDATE USER ----------------
        public async Task<ApiResponse<string>> UpdateUserAsync(Guid userId, UserUpdateRequest dto)
        { 
            try
            {
                var tenantId = _tenantProvider.TenantId;

                // Update basic user info
                var query = _db.Users.Where(u => u.UserId == userId && u.TenantId == tenantId);

                int affectedRows = await query.ExecuteUpdateAsync(u => u
                    .SetProperty(u => u.FirstName, dto.FirstName)
                    .SetProperty(u => u.LastName, dto.LastName)
                    .SetProperty(u => u.PhoneNumber, dto.PhoneNumber)
                    .SetProperty(u => u.RoleId, dto.RoleId)
                    .SetProperty(u => u.IsActive, dto.IsActive)
                    .SetProperty(u => u.LastUpdatedAt, DateTime.UtcNow)
                );

                if (affectedRows == 0)
                    throw new NotFoundException("User not found");

                // Determine branches based on role
                var role = await _db.UserRoles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
                if (role == null)
                    return ApiResponseFactory.Failure<string>("Invalid role");

                List<Guid> assignedBranches;

                if (role.RoleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    assignedBranches = new List<Guid>(); // no branches for SuperAdmin
                }
                else if (role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    if (dto.BranchIds == null || !dto.BranchIds.Any())
                        return ApiResponseFactory.Failure<string>("Admin must have at least one branch");

                    assignedBranches = dto.BranchIds;
                }
                else
                {
                    if (dto.BranchIds == null || dto.BranchIds.Count != 1)
                        return ApiResponseFactory.Failure<string>("User must be assigned exactly one branch");

                    assignedBranches = dto.BranchIds;
                }

                // Fetch existing branches
                var existingBranches = await _db.UserBranches.Where(ub => ub.UserId == userId).ToListAsync();

                // Remove unassigned branches
                var removeBranches = existingBranches.Where(ub => !assignedBranches.Contains(ub.BranchId)).ToList();
                _db.UserBranches.RemoveRange(removeBranches);

                // Add only missing branches (validate against tenant branches)
                if (assignedBranches.Any() && role.RoleName != "SuperAdmin")
                {
                    var validBranches = await _db.Branches
                        .Where(b => b.TenantId == tenantId && assignedBranches.Contains(b.BranchId))
                        .Select(b => b.BranchId)
                        .ToListAsync();

                    var existingIds = existingBranches.Select(ub => ub.BranchId).ToList();

                    var newBranches = validBranches
                        .Where(b => !existingIds.Contains(b))
                        .Select(b => new UserBranch
                        {
                            UserId = userId,
                            BranchId = b,
                            IsActive = true
                        });

                    if (newBranches.Any())
                        await _db.UserBranches.AddRangeAsync(newBranches);
                }

                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success("User updated successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }


        // ---------------- DELETE USER ----------------
        public async Task<ApiResponse<string>> DeleteUserAsync(Guid userId)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;
                var user = await _db.Users.Include(u => u.UserBranches).FirstOrDefaultAsync(
                    u => u.UserId == userId && u.TenantId == tenantId);

                if (user == null)
                    return ApiResponseFactory.Failure<string>("User not found");

                _db.UserBranches.RemoveRange(user.UserBranches);
                _db.Users.Remove(user);

                await _db.SaveChangesAsync();
                return ApiResponseFactory.Success("User deleted successfully");
            }
            catch
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }

        // ---------------- GET ROLES ----------------
        public async Task<ApiResponse<List<object>>> GetRolesAsync()
        {
            var roles = await _db.UserRoles.Select(r => new { r.Id, r.RoleName }).ToListAsync();
            return ApiResponseFactory.Success<List<object>>(roles.Cast<object>().ToList());
        }

        // ---------------- GET LOGS ---------------- 
        public async Task<ApiResponse<List<object>>> GetLogsAsync()
        {
            var logs = await _db.UsersLogs.OrderByDescending(l => l.EventTime)
                .Select(l => new { l.EventMessage, l.EventTime }).ToListAsync();
            return ApiResponseFactory.Success<List<object>>(logs.Cast<object>().ToList());
        }

        // ---------------- LOGOUT ----------------
        public async Task<ApiResponse<string>> LogoutAsync(LogoutRequest dto)
        {
            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null)
                return ApiResponseFactory.Failure<string>("User not found");

            _db.UsersLogs.Add(new UsersLog { UserId = user.UserId, EventMessage = $"User {user.Email} logged out", EventTime = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Logout logged successfully");
        }


    }
}