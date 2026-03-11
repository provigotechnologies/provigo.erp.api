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
using ProviGo.Common.Constants;

namespace IdentityService.Services.Implementation
{
    public class IdentityService(
        TenantDbContext db,
        IGenericRepository<User> repo,
        TenantProvider tenantProvider,
        TokenService tokenService,
        CurrentUserService currentUser,
        BranchAccessService branchAccess) : IIdentityService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<User> _repo = repo;
        private readonly TokenService _tokenService = tokenService;
        private readonly TenantProvider _tenantProvider = tenantProvider;
        private readonly CurrentUserService _currentUser = currentUser;
        private readonly BranchAccessService _branchAccess = branchAccess;

        // ---------------- REGISTER ----------------
        public async Task<ApiResponse<UserResponse>> RegisterAsync(UserCreateRequest dto, List<Guid> branchIds)
        {
            try
            {
                _currentUser.EnsureWriteAccess();

                // Email duplicate check
                var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
                if (emailExists)
                    return ApiResponseFactory.Failure<UserResponse>("Email already registered.");

                // Validate role
                var role = await _db.UserRoles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
                if (role == null)
                    return ApiResponseFactory.Failure<UserResponse>("Invalid role.");

                // Determine branches based on role
                List<Guid> assignedBranches = role.RoleName switch
                {
                    Roles.SuperAdmin => new List<Guid>(),
                    Roles.Admin when branchIds != null && branchIds.Any() => branchIds,
                    Roles.Admin => throw new Exception("Admin must be assigned at least one branch"),
                    _ when branchIds != null && branchIds.Count == 1 => branchIds,
                    _ => throw new Exception("User must be assigned exactly one branch")
                };

                // 🔐 Admin branch restriction
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (_currentUser.IsAdmin && assignedBranches.Any())
                {
                    if (!assignedBranches.All(b => allowedBranches.Contains(b)))
                        return ApiResponseFactory.Failure<UserResponse>(
                            "You can only assign users to your branches");
                }

                // Validate branch exists in tenant
                if (assignedBranches.Any())
                {
                    var validBranches = await _db.Branches
                        .Where(b => assignedBranches.Contains(b.BranchId))
                        .Select(b => b.BranchId)
                        .ToListAsync();

                    if (validBranches.Count != assignedBranches.Count)
                        return ApiResponseFactory.Failure<UserResponse>("Invalid branch selected");
                }

                // Create user
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    TenantId = _tenantProvider.TenantId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = dto.RoleId,
                    UserCategory = dto.UserCategory,
                    IsActive = dto.IsActive,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
                };

                await _db.Users.AddAsync(user);

                // Assign branches 
                if (assignedBranches.Any())
                {
                    var branches = assignedBranches.Select(b => new UserBranch
                    {
                        UserId = user.UserId,
                        BranchId = b,
                        IsActive = true
                    });

                    await _db.UserBranches.AddRangeAsync(branches);
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
                    UserCategory = user.UserCategory,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedOn
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
            var user = await _db.Users
              .AsNoTracking()
              .Include(u => u.UserRole)
              .FirstOrDefaultAsync(u => u.Email == dto.Email);

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
        public async Task<ApiResponse<List<UserResponse>>> GetUsersAsync(PaginationRequest request, bool includeInactive)
        {
            try
            {
                var currentUserId = _currentUser.UserId;

                var currentUser = await _db.Users
                    .AsNoTracking()
                    .Include(u => u.UserRole)
                    .Include(u => u.UserBranches)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                    return ApiResponseFactory.Failure<List<UserResponse>>("Current user not found");

                var roleName = currentUser.UserRole.RoleName;

                IQueryable<User> query = _db.Users
                    .Include(u => u.UserRole)
                    .Include(u => u.UserBranches)
                    .AsNoTracking();

                // Admin filter
                if (roleName.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
                {
                    var adminBranchIds = currentUser.UserBranches
                        .Select(ub => ub.BranchId)
                        .ToList();

                    query = query.Where(u =>
                        u.UserBranches.Any(ub => adminBranchIds.Contains(ub.BranchId)));
                }

                // User filter
                else if (roleName.Equals(Roles.User, StringComparison.OrdinalIgnoreCase))
                {
                    var userBranchId = currentUser.UserBranches
                        .Select(ub => ub.BranchId)
                        .FirstOrDefault();

                    query = query.Where(u =>
                        u.UserBranches.Any(ub => ub.BranchId == userBranchId));
                }

                if (!includeInactive)
                    query = query.Where(u => u.IsActive);

                var pagedResult = await _repo.GetPagedAsync(query, request);

                var tenantBranches = await _db.Branches
                    .Select(b => b.BranchId)
                    .ToListAsync();

                // Entity → DTO
                var users = pagedResult.Items.Select(u => new UserResponse
                {
                    Id = u.UserId,
                    Email = u.Email,
                    FullName = $"{u.FirstName} {u.LastName}",
                    PhoneNumber = u.PhoneNumber,
                    UserCategory = u.UserCategory,
                    RoleId = u.RoleId,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedOn,

                    BranchIds = u.UserRole.RoleName == Roles.SuperAdmin
                        ? tenantBranches
                        : u.UserBranches.Select(b => b.BranchId).ToList()

                }).ToList();

                return ApiResponseFactory.Success(users, "Users fetched successfully");
            }
            catch
            {
                return ApiResponseFactory.Failure<List<UserResponse>>("Database error occurred");
            }
        }


        // ---------------- UPDATE USER ----------------
        public async Task<ApiResponse<string>> UpdateUserAsync(Guid userId, UserUpdateRequest dto)
        { 
            try
            {
                _currentUser.EnsureWriteAccess();

                var tenantId = _tenantProvider.TenantId;

                // Update basic user info
                var query = _db.Users.Where(u => u.UserId == userId && u.TenantId == tenantId);

                int affectedRows = await query.ExecuteUpdateAsync(u => u
                    .SetProperty(u => u.FirstName, dto.FirstName)
                    .SetProperty(u => u.LastName, dto.LastName)
                    .SetProperty(u => u.PhoneNumber, dto.PhoneNumber)
                    .SetProperty(u => u.RoleId, dto.RoleId)
                    .SetProperty(u => u.UserCategory, dto.UserCategory)
                    .SetProperty(u => u.IsActive, dto.IsActive)
                    .SetProperty(u => u.UpdatedOn, DateTime.UtcNow)
                );

                if (affectedRows == 0)
                    throw new NotFoundException("User not found");

                // Determine branches based on role
                var role = await _db.UserRoles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
                if (role == null)
                    return ApiResponseFactory.Failure<string>("Invalid role");

                List<Guid> assignedBranches;

                if (role.RoleName.Equals(Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase))
                {
                    assignedBranches = new List<Guid>(); // no branches for SuperAdmin
                }
                else if (role.RoleName.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
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

                // 🔐 Admin branch restriction
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (_currentUser.IsAdmin && assignedBranches.Any())
                {
                    if (!assignedBranches.All(b => allowedBranches.Contains(b)))
                        return ApiResponseFactory.Failure<string>(
                            "You can only assign users to your branches");
                }

                // Fetch existing branches
                var existingBranches = await _db.UserBranches.Where(ub => ub.UserId == userId).ToListAsync();

                // Remove unassigned branches
                var removeBranches = existingBranches.Where(ub => !assignedBranches.Contains(ub.BranchId)).ToList();
                _db.UserBranches.RemoveRange(removeBranches);

                // Add only missing branches  
                if (assignedBranches.Any() && role.RoleName != Roles.SuperAdmin)
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
                _currentUser.EnsureWriteAccess();

                var user = await _db.Users.Include(u => u.UserBranches).FirstOrDefaultAsync(
                    u => u.UserId == userId && u.TenantId == _tenantProvider.TenantId);

                if (user == null)
                    return ApiResponseFactory.Failure<string>("User not found");
                
                // 🔐 Admin branch restriction
                var allowedBranches = await _branchAccess.GetAllowedBranchesAsync();

                if (_currentUser.IsAdmin)
                {
                    var userBranchIds = user.UserBranches.Select(x => x.BranchId).ToList();

                    if (!userBranchIds.All(b => allowedBranches.Contains(b)))
                        return ApiResponseFactory.Failure<string>(
                            "You can only delete users from your branches");
                }

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
            var logs = await _db.UsersLogs
                .AsNoTracking()
                .OrderByDescending(l => l.EventTime)
                .Select(l => new { l.EventMessage, l.EventTime }).ToListAsync();

            return ApiResponseFactory.Success<List<object>>(logs.Cast<object>().ToList());
        }

        // ---------------- LOGOUT ----------------
        public async Task<ApiResponse<string>> LogoutAsync(LogoutRequest dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                return ApiResponseFactory.Failure<string>("User not authenticated");

            _db.UsersLogs.Add(new UsersLog
            {
                UserId = userId,
                EventMessage = "User logged out",
                EventTime = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Logout logged successfully");
        }


    }
}