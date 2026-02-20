using SettingsManagement.Data;
using SettingsManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using SettingsManagement.Models.View;
using SettingsManagement.Services.Interface;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SettingsManagement.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register services before builder.Build()

// 1. DbContext
builder.Services.AddDbContext<SettingsDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// 2. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});


// 4. Swagger (✅ Move this before builder.Build())
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<ISettingService, SettingService>();

// ✅ Build the app
var app = builder.Build();

// 🚀 Use middlewares
app.UseCors("AllowAngularApp");
    app.UseSwagger();
    app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllers();

app.UseHttpsRedirection();

app.MapGet("/api/settings/{module}", async ( string module, ISettingService service) =>
{
    var result = await service.GetSettingsByModuleAsync(module);
    return result == null ? Results.BadRequest("Invalid module name") : Results.Ok(result);
});


app.MapPost("/api/settings/access/{userId:guid}", async (Guid userId, SettingAccess access, ISettingService service) =>
{
    if (access == null)
        return Results.BadRequest("Invalid data");

    var saved = await service.SaveUserAccessAsync(userId, access);
    return saved
        ? Results.Ok(new { Message = "Access saved successfully", StatusCode = 200, UserId = userId })
        : Results.StatusCode(500);
});

app.Run();

app.MapGet("/api/settings/access/{userId:guid}", async (Guid userId, ISettingService service) =>
{
    var result = await service.GetUserAccessAsync(userId);
    return Results.Ok(result);
});


app.Run();
