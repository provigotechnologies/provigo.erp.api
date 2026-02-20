using IdentityService.Data;
using AttendanceService.DTOs;
using AttendanceService.Services.Extensions;
using AttendanceService.Services.Implementation;
using AttendanceService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Provigo.Common.Exceptions.Middleware;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddTransient<IAttendanceService, AttendanceService.Services.Implementation.AttendanceService>();
// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Attendance Service API",
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


// ==================== ATTENDANCE APIs ====================
// Create Attendance

app.MapPost("/api/attendance",
async (AttendanceDto dto, IAttendanceService attendanceService) =>
{
    var res = await attendanceService.CreateAttendanceAsync(dto);
    return res.Success ? Results.Ok(res) : Results.BadRequest(res);
});

// List Attendance
app.MapGet("/api/attendance",
async ([AsParameters] PaginationRequest request,
bool includeInactive,
IAttendanceService attendanceService) =>
{
    var res = await attendanceService.GetAttendanceAsync(request, includeInactive);
    return Results.Ok(res);
});


app.Run();
