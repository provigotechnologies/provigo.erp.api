using IdentityService.Data;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Provigo.Common.Exceptions.Middleware;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ShiftService.DTOs;
using ShiftService.Endpoints;
using ShiftService.Services.Extensions;
using ShiftService.Services.Implementation;
using ShiftService.Services.Interface;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IIdentityProvider, IdentityProvider>();

builder.Services.AddTransient<IShiftService, ShiftService.Services.Implementation.ShiftService>();

// 🔹 DbContext
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

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
builder.Services.AddCommonPagination();

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shift Service API",
        Version = "v1"
    });
});

var app = builder.Build();

// 🔹 Middlewares
app.UseCors("AllowAngularApp");
//Handling all the exceptions globally
app.UseGlobalExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

ShiftEndpoints.Map(app);

app.Run();