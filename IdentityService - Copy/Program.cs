using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using IdentityService.DTOs;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ? Configure DbContext
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

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
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ? Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Enter JWT token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// ? Middlewares
app.UseCors("AllowAngularApp");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ? Register
app.MapPost("/register", async (UserCreateRequest dto, IdentityDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == dto.Email))
        return Results.BadRequest("Email already registered.");

    var user = new User
    {
        Id = Guid.NewGuid(),
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        PhoneNumber = dto.PhoneNumber,
        PasswordHash = dto.Password,//BCrypt.Net.BCrypt.HashPassword(dto.Password),
        RoleId = dto.RoleId,
        IsActive = dto.IsActive,
        CreatedAt = DateTime.UtcNow,
        LastUpdatedAt = DateTime.UtcNow
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("User registered.");
});

//userrole
app.MapGet("/roles", async (IdentityDbContext db) =>
{
    var roles = await db.UserRole
        .Select(r => new {
            r.Id,
            r.RoleName
        })
        .ToListAsync();

    return Results.Ok(roles);
});


// ? Login
app.MapPost("/login", async (LoginDto dto, IdentityDbContext db, IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user == null)
        return Results.Unauthorized();

    // 🔒 Must be BEFORE password check
    if (!user.IsActive)
        return Results.BadRequest("User is inactive. Please contact admin.");

    if (dto.Password != user.PasswordHash)
        return Results.Unauthorized();

    var token = JwtHelper.GenerateToken(user, config);

    var loginLog = new UsersLog
    {
        UserId = user.Id,
        EventMessage = $"User {user.Email} logged in successfully",
        EventTime = DateTime.Now
    };

    db.UsersLog.Add(loginLog);
    await db.SaveChangesAsync();

    return Results.Ok(new { token, user });
});


// ✅ GET all logs
app.MapGet("/logs", async (IdentityDbContext db) =>
{
    var logs = await db.UsersLog
        .OrderByDescending(l => l.EventTime)
        .Select(l => new {
            l.EventMessage,
            l.EventTime
        })
        .ToListAsync();

    return Results.Ok(logs);
});

//Logout logs
app.MapPost("/log-logout", async (HttpContext context, IdentityDbContext db) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<LogoutRequest>(body, options);

        if (data == null || data.UserId == Guid.Empty)
            return Results.BadRequest("Invalid data");

        var user = await db.Users.FindAsync(data.UserId);
        if (user == null)
            return Results.NotFound("User not found");

        var log = new UsersLog
        {
            UserId = user.Id,
            EventMessage = $"User {user.Email} logged out successfully",
            EventTime = DateTime.Now
        };

        db.UsersLog.Add(log);
        await db.SaveChangesAsync();

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Problem("Exception occurred: " + ex.Message);
    }
});




//// ? Protected endpoint
//app.MapGet("/me", (ClaimsPrincipal user) =>
//{
//    var email = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//    return Results.Ok(new { Email = email });
//}).RequireAuthorization();

// ✅ Return active users
//app.MapGet("/users", async (IdentityDbContext db) =>
//{
//    var activeUsers = await db.Users
//        .Where(u => u.IsActive)
//        .Select(u => new { u.Id, u.FirstName })
//        .ToListAsync();

//    return Results.Ok(activeUsers);
//});

app.MapGet("/users/email", async (IdentityDbContext db) =>
{
    var users = await db.Users
        .Select(u => new
        {
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.RoleId,
            u.PasswordHash,
            u.PhoneNumber,
            Status = u.IsActive ? true : false
        })
        .ToListAsync();

    return Results.Ok(users);
});

// ✅ Update user by ID
app.MapPut("update/users/{id:guid}", async (Guid id, UserCreateRequest dto, IdentityDbContext db) =>
{
    var user = await db.Users.FindAsync(id);

    if (user == null)
        return Results.NotFound("User not found.");

    user.FirstName = dto.FirstName;
    user.LastName = dto.LastName;
    user.Email = dto.Email;
    user.PhoneNumber = dto.PhoneNumber;
    user.RoleId = dto.RoleId;
    user.IsActive = dto.IsActive;
    user.LastUpdatedAt = DateTime.UtcNow;

    // Update password only if provided
    if (!string.IsNullOrWhiteSpace(dto.Password))
    {
        user.PasswordHash = dto.Password;
    }

    await db.SaveChangesAsync();

    return Results.Ok("User updated successfully.");
});


// ? Delete user by ID
app.MapDelete("delete/users/{id:guid}", async (Guid id, IdentityDbContext db) =>
{
    var user = await db.Users.FindAsync(id);

    if (user == null)
        return Results.NotFound("User not found.");

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok("User deleted successfully.");
});

app.Run();




