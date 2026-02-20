using IdentityService.Data;
using IdentityService.Middleware;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TenantService.Endpoints;
using TenantService.Middleware;
using TenantService.Services;
using TenantService.Services.Extensions;
using TenantService.Services.Implementation;
using TenantService.Services.Interface;
using ProviGo.Common.Pagination;
using TenantService.Endpoints;
using TenantService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// ✅ Identity Provider
builder.Services.AddScoped<IdentityProvider>();
builder.Services.AddScoped<IIdentityProvider>(sp => sp.GetRequiredService<IdentityProvider>());

// ✅ Master DB
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Master"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("Master")
        )
    ));

// ✅ Tenant DB
builder.Services.AddDbContext<TenantDbContext>();

builder.Services.AddScoped<ITenantService, TenantService.Services.Implementation.TenantService>();
builder.Services.AddScoped<IBranchService, TenantService.Services.Implementation.BranchService>();

builder.Services.AddScoped(typeof(IGenericRepository<>),
                           typeof(GenericRepository<>));

builder.Services.AddMemoryCache();
builder.Services.AddCommonPagination();

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

// 🔹 Swagger (Tenant Header)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product Service API",
        Version = "v1"
    });

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


// 🔹 Build
var app = builder.Build();


// 🔹 Middleware Order
app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<TenantMiddleware>();

app.UseStaticFiles();

TenantEndpoints.Map(app);

app.Run();
