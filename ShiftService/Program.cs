using ShiftService.Endpoints;
using ShiftService.Services;
using ShiftService.Services.Implementation;
using ShiftService.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProviGo.Common.Data;
using ProviGo.Common.Middleware;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Services;
using System.Text;

// Builder
var builder = WebApplication.CreateBuilder(args);

// Tenant & User Services 
builder.Services.AddScoped<TenantProvider>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<BranchAccessService>();

// Generic Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Application Services
builder.Services.AddScoped<IShiftService, ShiftService.Services.Implementation.ShiftService>();
builder.Services.AddScoped<IProductOfferingService, ProductOfferingService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddCommonPagination();

// 🔹 Master DB 
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Master"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Master"))
    ));

// 🔹 Tenant DB 
builder.Services.AddDbContext<TenantDbContext>((sp, options) =>
{
    var provider = sp.GetRequiredService<TenantProvider>();
    if (string.IsNullOrEmpty(provider.ConnectionString))
        throw new Exception("Tenant connection string not configured.");

    options.UseMySql(
        provider.ConnectionString,
        ServerVersion.AutoDetect(provider.ConnectionString));
});

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 🔐 JWT Authentication
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
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("Auth failed: " + ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Console.WriteLine("Token validated for user: " +
                                  ctx.Principal?.Identity?.Name);
                // Log all claims for debugging
                foreach (var claim in ctx.Principal.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 🔹 Swagger (JWT + Tenant + Branch Headers)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shift Service API", Version = "v1" });

    // JWT
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
    };
    c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);

    // Tenant Header
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
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "TenantHeader" } }, Array.Empty<string>() },
    });
});

// 🔹 Build App
var app = builder.Build();

// 🔹 Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.UseStaticFiles();

// 🔹 Map Endpoints
ShiftEndpoints.Map(app);
ProductOfferingEndpoints.Map(app);
EnrollmentEndpoints.Map(app);

app.Run();