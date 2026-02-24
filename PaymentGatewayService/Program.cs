using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Data;
using PaymentGatewayService.DTOs;
using PaymentGatewayService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(conn, ServerVersion.AutoDetect(conn)); // Pomelo MySQL
});

builder.Services.AddScoped<PaymentService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
        .SetIsOriginAllowed(x => _ = true)
        .AllowAnyHeader()
        .AllowAnyMethod();
        // in production replace AllowAnyOrigin with specific origin
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();   // Applies migration if pending
}
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "My API V1");
    //c.SwaggerEndpoint("swagger/v1/swagger.json", "My API V1");

    c.RoutePrefix = "swagger";
});
app.UseHttpsRedirection();
app.UseCors("AllowAll"); 

    app.MapPost("/payment/create-order", async ([FromBody]CreateOrderRequest req, PaymentService service) =>
{
    try
    {
        if (req.Amount <= 0) return Results.BadRequest(new { message = "Invalid amount" });

        var resp = await service.CreateOrderAsync(req);
        return Results.Ok(resp);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
    
});

app.MapPost("/payment/verify", async (VerifyPaymentRequest dto, PaymentService service) =>
{
    var ok = await service.VerifyAndSavePaymentAsync(dto);
    if (ok) return Results.Ok(new { status = "success" });
    return Results.BadRequest(new { status = "failed" });
});

// optional webhook endpoint (verify using signature header)
app.MapPost("/api/payment/webhook", async (HttpRequest request, IConfiguration config, AppDbContext db) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var signature = request.Headers["X-Razorpay-Signature"].FirstOrDefault();
    var secret = config["Razorpay:KeySecret"];

    // Verify signature: HMAC_SHA256(body, secret) base64? Razorpay webhook uses HMAC-SHA256 with secret and returns signature (base64). But check docs.
    // Implement as needed, here just a placeholder:
    // var isValid = YourWebhookVerification(body, signature, secret);

    // if valid, parse and handle event
    return Results.Ok();
});

app.MapGet("/", () => "API is Running");

app.Run();
