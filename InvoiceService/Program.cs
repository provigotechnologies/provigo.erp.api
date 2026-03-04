using IdentityService.Data;
using InvoiceService.Endpoints;
using InvoiceService.Middleware;
using InvoiceService.Services;
using InvoiceService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProviGo.Common.Pagination;
using QuestPDF.Infrastructure;

// Builder
var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// 🔹 Memory Cache
builder.Services.AddMemoryCache();

// 🔹 Generic Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 🔹 Application Services
builder.Services.AddScoped<InvoiceProvider>();
builder.Services.AddScoped<IInvoiceService, InvoiceService.Services.InvoiceService>();

// 🔹 Master DB (Tenant Registry)
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Master"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("Master")
        )));

// 🔹 Tenant DB (Per Tenant Connection)
builder.Services.AddDbContext<TenantDbContext>((sp, options) =>
{
    var provider = sp.GetRequiredService<InvoiceProvider>();

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

// 🔹 Swagger (Tenant Header)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Invoice Service API",
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

// 🔹 Custom Tenant Middleware
app.UseMiddleware<InvoiceMiddleware>();

app.UseStaticFiles();

// 🔹 Map Endpoints
InvoiceEndpoints.Map(app);

app.Run();