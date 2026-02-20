using IdentityService.Data;
using FeeService.DTOs;
using FeeService.Services.Extensions;
using FeeService.Services.Implementation;
using FeeService.Services.Interface;
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
//builder.Services.AddTransient<IFeeService, FeeService.Services.Implementation.FeeService>();
// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fee Service API",
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


// ==================== FEEs APIs ====================
// Create FeePayment
app.MapPost("/feePayment", async (FeePaymentDto dto, IFeeService feeService) =>
{
    var response = await feeService.CreateFeePaymentAsync(dto);
    if (!response.Success)
        return Results.BadRequest(response);

    return Results.Ok(response);


});

// GET All FeePayment
//app.MapGet("/feePayment", async ([AsParameters] PaginationRequest request, bool includeInactive, IFeeService feeService) =>
//{
//    var response = await feeService.GetFeePaymentsAsync(request, includeInactive);
//    return Results.Ok(response);
//});


////Delete Product
//app.MapDelete("/feePayment/{id:int}", async (int id, IFeeService feeService) =>
//{
//    var response = await feeService.RemoveFeePaymentAsync(id);

//    return response.Success
//        ? Results.Ok(response)
//        : Results.BadRequest(response);
//});



//// ==================== PAYMENTs APIs ====================
//// Create Payments
//app.MapPost("/api/payments/initiate", async (FeePaymentDto dto, InstituteDbContext db) =>
//{
//    var studentFee = await db.CustomerFees
//        .FirstOrDefaultAsync(f => f.CustomerFeeId == dto.CustomerFeeId && f.IsActive);

//    if (studentFee == null)
//        return Results.NotFound("Student fee record not found");

//    if (dto.Amount <= 0 || dto.Amount > studentFee.BalanceAmount)
//        return Results.BadRequest("Invalid payment amount");

//    // Save payment
//    var payment = new FeePayment
//    {
//        CustomerFeeId = dto.CustomerFeeId,
//        Amount = dto.Amount,
//        PaymentMode = dto.PaymentMode,
//        PaidOn = DateTime.Now
//    };

//    db.FeePayments.Add(payment);

//    // Update fee summary
//    studentFee.PaidAmount += dto.Amount;
//    studentFee.BalanceAmount -= dto.Amount;

//    await db.SaveChangesAsync();

//    return Results.Ok("Payment recorded successfully");
//});


//app.MapPost("/api/payments/verify", () =>
//{
//    return Results.Ok("Payment verified successfully (dummy)");
//});

//// List Payments
//app.MapGet("/api/payments/history", async (int CustomerFeeId, InstituteDbContext db) =>
//{
//    var payments = await db.FeePayments
//        .Where(p => p.CustomerFeeId == CustomerFeeId)
//        .Select(p => new FeePaymentResponseDto
//        {
//            FeePaymentId = p.FeePaymentId,
//            CustomerFeeId = p.CustomerFeeId,
//            Amount = p.Amount,
//            PaymentMode = p.PaymentMode,
//            PaidOn = p.PaidOn
//        })
//        .ToListAsync();

//    return Results.Ok(payments);
//});

app.Run();
