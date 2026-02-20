using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Middleware;
using IdentityService.Services;
using IdentityService.Services.Interface;
using IdentityService.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;  
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

// Builder
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IIdentityService, IdentityService.Services.Implementation.IdentityService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


// Master DB
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Master"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Master"))
    ));

// Institute DB 
builder.Services.AddDbContext<TenantDbContext>();


// ? CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ? JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

// Application Service
builder.Services.AddScoped<IdentityProvider>();
builder.Services.AddScoped<IIdentityProvider>(sp => sp.GetRequiredService<IdentityProvider>());
builder.Services.AddScoped<TokenService>();

builder.Services.AddMemoryCache();


// Swagger (JWT + Tenant Header)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProviGo API",
        Version = "v1"
    });

    // 🔐 JWT
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);

    // 🏢 Tenant Header
    c.AddSecurityDefinition("TenantHeader", new OpenApiSecurityScheme
    {
        Name = "X-Tenant-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter Tenant Id"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TenantHeader"
                }
            },
            Array.Empty<string>()
        }
    });
});


// Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Build App
var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();

app.UseMiddleware<IdentityMiddleware>(); // AFTER Auth, BEFORE Authorization

app.UseAuthorization();

app.UseStaticFiles();

IdentityEndpoints.Map(app);

app.Run();



//// ? Register
//app.MapPost("/register", async (UserCreateRequest dto, TenantDbContext db, TenantProvider tenantProvider) =>
//{
//    if (await db.Users.AnyAsync(u => u.Email == dto.Email))
//        return Results.BadRequest("Email already registered.");

//    var user = new User
//    {
//        UserId = Guid.NewGuid(),
//        TenantId = tenantProvider.TenantId,   // important
//        FirstName = dto.FirstName,
//        LastName = dto.LastName,
//        Email = dto.Email,
//        PhoneNumber = dto.PhoneNumber,
//        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
//        RoleId = dto.RoleId,
//        UserCategory = dto.UserCategory,   // ✅ SET THIS
//        IsActive = dto.IsActive,
//        CreatedAt = DateTime.UtcNow,
//        LastUpdatedAt = DateTime.UtcNow
//    };


//    db.Users.Add(user);
//    await db.SaveChangesAsync();

//    return Results.Ok("User registered.");
//});


//// UserRole
//app.MapGet("/roles", async (TenantDbContext db) =>
//{
//    var roles = await db.UserRoles
//        .Select(r => new {
//            r.Id,
//            r.RoleName
//        })
//        .ToListAsync();

//    return Results.Ok(roles);
//});


//// ? Login
//app.MapPost("/login", async (LoginDto dto, TenantDbContext db, IConfiguration config) =>
//{
//    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
//    if (user == null)
//        return Results.Unauthorized();

//    // 🔒 Must be BEFORE password check
//    if (!user.IsActive)
//        return Results.BadRequest("User is inactive. Please contact admin.");

//    if (dto.Password != user.PasswordHash)
//        return Results.Unauthorized();

//    var token = JwtHelper.GenerateToken(user, config);

//    var loginLog = new UsersLog
//    {
//        UserId = user.UserId,
//        EventMessage = $"User {user.Email} logged in successfully",
//        EventTime = DateTime.Now
//    };

//    db.UsersLogs.Add(loginLog);
//    await db.SaveChangesAsync();

//    return Results.Ok(new { token, user });
//});


//// ✅ GET all logs
//app.MapGet("/logs", async (TenantDbContext db) =>
//{
//    var logs = await db.UsersLogs
//        .OrderByDescending(l => l.EventTime)
//        .Select(l => new {
//            l.EventMessage,
//            l.EventTime
//        })
//        .ToListAsync();

//    return Results.Ok(logs);
//});

////Logout logs
//app.MapPost("/log-logout", async (HttpContext context, TenantDbContext db) =>
//{
//    try
//    {
//        using var reader = new StreamReader(context.Request.Body);
//        var body = await reader.ReadToEndAsync();

//        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//        var data = JsonSerializer.Deserialize<LogoutRequest>(body, options);

//        if (data == null || data.UserId == Guid.Empty)
//            return Results.BadRequest("Invalid data");

//        var user = await db.Users.FindAsync(data.UserId);
//        if (user == null)
//            return Results.NotFound("User not found");

//        var log = new UsersLog
//        {
//            UserId = user.UserId,
//            EventMessage = $"User {user.Email} logged out successfully",
//            EventTime = DateTime.Now
//        };

//        db.UsersLogs.Add(log);
//        await db.SaveChangesAsync();

//        return Results.Ok();
//    }
//    catch (Exception ex)
//    {
//        return Results.Problem("Exception occurred: " + ex.Message);
//    }
//});




////// ? Protected endpoint
////app.MapGet("/me", (ClaimsPrincipal user) =>
////{
////    var email = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
////    return Results.Ok(new { Email = email });
////}).RequireAuthorization();

//// ✅ Return active users
////app.MapGet("/users", async (IdentityDbContext db) =>
////{
////    var activeUsers = await db.Users
////        .Where(u => u.IsActive)
////        .Select(u => new { u.Id, u.FirstName })
////        .ToListAsync();

////    return Results.Ok(activeUsers);
////});

//app.MapGet("/users/email", async (TenantDbContext db) =>
//{
//    var users = await db.Users
//        .Select(u => new
//        {
//            u.UserId,
//            u.FirstName,
//            u.LastName,
//            u.Email,
//            u.RoleId,
//            u.PasswordHash,
//            u.PhoneNumber,
//            Status = u.IsActive ? true : false
//        })
//        .ToListAsync();

//    return Results.Ok(users);
//});

//// ✅ Update user by ID

//app.MapPut("/update/users/{id:guid}", async (Guid id, UserCreateRequest dto, TenantDbContext db, ClaimsPrincipal userClaims) =>
//{
//    // Get tenantId from JWT
//    var tenantId = Guid.Parse(userClaims.FindFirst("tenantId")!.Value);

//    var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id && u.TenantId == tenantId);
//    if (user == null)
//        return Results.NotFound("User not found.");

//    // Email uniqueness
//    if (await db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != id && u.TenantId == tenantId))
//        return Results.BadRequest("Email already in use.");

//    // Role exists
//    if (!await db.UserRoles.AnyAsync(r => r.Id == dto.RoleId))
//        return Results.BadRequest("Invalid role.");

//    user.FirstName = dto.FirstName;
//    user.LastName = dto.LastName;
//    user.Email = dto.Email;
//    user.PhoneNumber = dto.PhoneNumber;
//    user.RoleId = dto.RoleId;
//    user.UserCategory = dto.UserCategory;
//    user.IsActive = dto.IsActive;
//    user.LastUpdatedAt = DateTime.UtcNow;

//    if (!string.IsNullOrWhiteSpace(dto.Password))
//    {
//        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
//    }

//    await db.SaveChangesAsync();

//    return Results.Ok("User updated successfully.");
//}).RequireAuthorization();



//// ? Delete user by ID
//app.MapDelete("delete/users/{id:guid}", async (Guid id, TenantDbContext db) =>
//{
//    var user = await db.Users.FindAsync(id);

//    if (user == null)
//        return Results.NotFound("User not found.");

//    db.Users.Remove(user);
//    await db.SaveChangesAsync();

//    return Results.Ok("User deleted successfully.");
//});


////app.MapDelete("/delete/users/{id:guid}", async (Guid id, TenantDbContext db, ClaimsPrincipal userClaims) =>
////{
////    var tenantId = Guid.Parse(userClaims.FindFirst("tenantId")!.Value);

////    var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id && u.TenantId == tenantId);
////    if (user == null)
////        return Results.NotFound("User not found.");

////    db.Users.Remove(user);
////    await db.SaveChangesAsync();

////    return Results.Ok("User deleted successfully.");
////}).RequireAuthorization();





