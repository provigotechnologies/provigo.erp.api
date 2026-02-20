using IdentityService.Data;
using IdentityService.Middleware;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductService.Endpoints;
using ProductService.Middleware;
using ProductService.Services;
using ProductService.Services.Extensions;
using ProductService.Services.Implementation;
using ProductService.Services.Interface;
using ProviGo.Common.Pagination;

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

builder.Services.AddScoped<IProductService,
    ProductService.Services.Implementation.ProductService>();

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

app.UseMiddleware<ProductMiddleware>();

app.UseStaticFiles();

ProductEndpoints.Map(app);

app.Run();
