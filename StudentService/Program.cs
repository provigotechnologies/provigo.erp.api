using InstituteService.Data;
using CustomerService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using CustomerService.Services.Extensions;
using CustomerService.Services.Implementation;
using Microsoft.OpenApi.Models;
using Provigo.Common.Exceptions.Middleware;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using CustomerService.DTOs;

var builder = WebApplication.CreateBuilder(args);

// 🔹 DbContext
builder.Services.AddDbContext<InstituteDbContext>(options =>
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
builder.Services.AddTransient<ICustomerService, CustomerService.Services.Implementation.CustomerService>();
// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Customer Service API",
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

// ==================== Customer APIs ====================
// Create Customer
app.MapPost("/api/customers",
async (CustomerDto dto, ICustomerService service) =>
{
    var res = await service.CreateCustomerAsync(dto);
    return res.Success ? Results.Ok(res) : Results.BadRequest(res);
});

// List Customer
app.MapGet("/api/customers",
async ([AsParameters] PaginationRequest request,
bool includeInactive,
ICustomerService service) =>
{
    var res = await service.GetCustomersAsync(request, includeInactive);
    return Results.Ok(res);
});

// Update Customer
app.MapPut("/api/customers/{id:int}",
async (int id, CustomerDto dto, ICustomerService service) =>
{
    var res = await service.UpdateCustomerAsync(id, dto);
    return res.Success ? Results.Ok(res) : Results.BadRequest(res);
});

// Customer Status
app.MapPatch("/api/customers/{id:int}/status",
async (int id, CustomerStatusUpdateDto dto, ICustomerService service) =>
{
    var res = await service.UpdateStatusAsync(id, dto.IsActive);
    return res.Success ? Results.Ok(res) : Results.BadRequest(res);
});

//Delete Customer
app.MapDelete("/Customers/{id:int}", async (int id, string action, ICustomerService customerService) =>
{
    var response = await customerService.RemoveCustomerAsync(id, action);
    if (!response.Success)
        return Results.BadRequest(response);

    return Results.Ok(response);
});



// ==================== COURSE APIs ====================
// Create Course
app.MapPost("/api/Products", async (ProductCreateDto dto, InstituteDbContext db) =>
{
    var exists = await db.Products.AnyAsync(p =>
        p.InstituteId == dto.InstituteId &&
        p.ProductName == dto.ProductName);

    if (exists)
        return Results.BadRequest("Course name already exists");

    var product = new Product
    {
        InstituteId = dto.InstituteId,
        ProductName = dto.ProductName,
        TotalFee = dto.TotalFee,
        IsActive = true
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Ok("Course added successfully");
});


// List Courses
app.MapGet("/api/products", async (InstituteDbContext db) =>
{
    var Products = await db.Products
        //.Where(c => c.IsActive)
        .Select(c => new ProductResponseDto
        {
            ProductId = c.ProductId,
            InstituteId = c.InstituteId,
            ProductName = c.ProductName,
            TotalFee = c.TotalFee,
            IsActive = c.IsActive
        })
        .ToListAsync();

    return Results.Ok(Products);
});

// Update Course
app.MapPut("/api/products/{id:int}", async (int id, ProductUpdateDto dto, InstituteDbContext db) =>
{
    var Product = await db.Products.FirstOrDefaultAsync(c => c.ProductId == id);
    if (Product == null) return Results.NotFound("Course not found");

    Product.ProductName = dto.ProductName;
    Product.TotalFee = dto.TotalFee;
    Product.IsActive = dto.IsActive;

    await db.SaveChangesAsync();
    return Results.Ok("Course updated successfully");
});

// Delete Course
app.MapDelete("/api/products/{id:int}", async (int id, InstituteDbContext db) =>
{
    var product = await db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
    if (product == null)
        return Results.NotFound("Course not found");

    product.IsActive = false;
    await db.SaveChangesAsync();

    return Results.Ok("Course deleted successfully");
});


// ==================== BATCH APIs ====================
// Create Batch 
app.MapPost("/api/batches", async (BatchCreateDto dto, InstituteDbContext db) =>
{
    var exists = await db.Batches.AnyAsync(b =>
        b.InstituteId == dto.InstituteId &&
        b.BatchName == dto.BatchName);

    if (exists)
        return Results.BadRequest("Batch name already exists");

    var batch = new Batch
    {
        InstituteId = dto.InstituteId,
        ProductId = dto.ProductId,
        BatchName = dto.BatchName,
        TrainerId = dto.TrainerId,
        IsActive = true
    };

    db.Batches.Add(batch);
    await db.SaveChangesAsync();

    return Results.Ok("Batch added successfully");
});


// List Batch 
app.MapGet("/api/batches", async (InstituteDbContext db) =>
{
    var batches = await db.Batches
        .Where(b => b.IsActive)
        .Select(b => new BatchResponseDto
        {
            BatchId = b.BatchId,
            InstituteId = b.InstituteId,
            ProductId = b.ProductId,
            BatchName = b.BatchName,
            TrainerId = b.TrainerId,
            IsActive = b.IsActive
        })
        .ToListAsync();

    return Results.Ok(batches);
});

// Update Batch 
app.MapPut("/api/batches/{id:int}", async (int id, BatchUpdateDto dto, InstituteDbContext db) =>
{
    var batch = await db.Batches.FirstOrDefaultAsync(b => b.BatchId == id);
    if (batch == null) return Results.NotFound("Batch not found");

    batch.BatchName = dto.BatchName;
    batch.TrainerId = dto.TrainerId;
    batch.IsActive = dto.IsActive;

    await db.SaveChangesAsync();
    return Results.Ok("Batch updated successfully");
});

// Delete Batch
app.MapDelete("/api/batches/{id:int}", async (int id, InstituteDbContext db) =>
{
    var batch = await db.Batches.FirstOrDefaultAsync(b => b.BatchId == id);
    if (batch == null)
        return Results.NotFound("Batch not found");

    batch.IsActive = false;
    await db.SaveChangesAsync();

    return Results.Ok("Batch deleted successfully");
});


// ==================== STUDENT-BATCH APIs ====================
// Assign student to batch
app.MapPost("/api/Customerbatches", async (CustomerBatchCreateDto dto, InstituteDbContext db) =>
{
    // Check if Student exists
    var Customer = await db.Customers.FirstOrDefaultAsync(s => s.CustomerId == dto.CustomerId && s.IsActive);
    if (Customer == null) return Results.NotFound("Student not found");

    // Check if Batch exists
    var batch = await db.Batches.FirstOrDefaultAsync(b => b.BatchId == dto.BatchId && b.IsActive);
    if (batch == null) return Results.NotFound("Batch not found");

    // Check if already assigned
    var exists = await db.CustomerBatches
        .AnyAsync(sb => sb.CustomerId == dto.CustomerId && sb.BatchId == dto.BatchId);
    if (exists) return Results.BadRequest("Student already assigned to this batch");

    var mapping = new CustomerBatchMapping
    {
        CustomerId = dto.CustomerId,
        BatchId = dto.BatchId
    };

    db.CustomerBatches.Add(mapping);
    await db.SaveChangesAsync();

    return Results.Created($"/api/Customerbatches/{mapping.Id}", new
    {
        mapping.Id,
        CustomerId = mapping.CustomerId,
        BatchId = mapping.BatchId
    });
});

// List student-batch mappings
app.MapGet("/api/Customerbatches", async (InstituteDbContext db) =>
{
    var mappings = await db.CustomerBatches
        .Include(sb => sb.Customer)
        .Include(sb => sb.Batch)
        .Select(sb => new CustomerBatchResponseDto
        {
            Id = sb.Id,
            CustomerId = sb.CustomerId,
            CustomerName = sb.Customer != null ? sb.Customer.FullName : "",
            BatchId = sb.BatchId,
            BatchName = sb.Batch != null ? sb.Batch.BatchName : ""
        })
        .ToListAsync();

    return Results.Ok(mappings);
});


app.Run();