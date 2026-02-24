using IdentityService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PricingService.Endpoints;
using PricingService.Middleware;
using PricingService.Services;
using PricingService.Services.Implementation;
using PricingService.Services.Interface;
using ProviGo.Common.Pagination;
using System.Text;

// Builder
var builder = WebApplication.CreateBuilder(args);

// 🔹 Application Services

builder.Services.AddScoped<IPricingService, PricingService.Services.Implementation.PricingService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<PricingProvider>();
builder.Services.AddMemoryCache();
builder.Services.AddCommonPagination();


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
    var provider = sp.GetRequiredService<PricingProvider>();

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

/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization();*/


// 🔹 Swagger (JWT + Tenant Header)

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pricing Service API",
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

// app.UseAuthentication();

app.UseMiddleware<PricingMiddleware>();  // After Auth, Before Authorization

// app.UseAuthorization();

app.UseStaticFiles();

PricingEndpoints.Map(app);

app.Run();