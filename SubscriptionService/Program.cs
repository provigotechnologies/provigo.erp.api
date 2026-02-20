using IdentityService.Data;
using SubscriptionService.DTOs;
using ProviGo.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Subscription Service API",
        Version = "v1"
    });
});

var app = builder.Build();

// 🔹 Middlewares
app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// ==================== PLANs APIs ====================
// Available Plans
app.MapPost("/api/plans", async (Plan plan, TenantDbContext db) =>
{
    var exists = await db.Plans.AnyAsync(p => p.PlanName == plan.PlanName && p.IsActive);
    if (exists)
        return Results.BadRequest("Plan name already exists");

    plan.IsActive = true;
    db.Plans.Add(plan);
    await db.SaveChangesAsync();

    return Results.Ok(plan);
});

// Available Plans
app.MapGet("/api/plans", async (TenantDbContext db) =>
{
    return Results.Ok(await db.Plans
        .Where(p => p.IsActive)
        .ToListAsync());
});

// Update Plans
app.MapPut("/api/plans/{id:int}", async (int id, Plan dto, TenantDbContext db) =>
{
    var plan = await db.Plans.FindAsync(id);
    if (plan == null) return Results.NotFound("Plan not found");

    plan.PlanName = dto.PlanName;
    plan.Price = dto.Price;
    plan.DurationDays = dto.DurationDays;
    plan.MaxStudents = dto.MaxStudents;
    plan.IsActive = dto.IsActive;

    await db.SaveChangesAsync();
    return Results.Ok("Plan updated successfully");
});


// Delete Plans
app.MapDelete("/api/plans/{id:int}", async (int id, TenantDbContext db) =>
{
    var plan = await db.Plans.FindAsync(id);
    if (plan == null)
        return Results.NotFound();

    plan.IsActive = false;
    await db.SaveChangesAsync();

    return Results.Ok("Plan deactivated");
});


// ==================== SUBSCRIPTION APIs ====================
// Subscribe Plan
app.MapPost("/api/subscription/subscribe", async (SubscribePlanDto dto, TenantDbContext db) =>
{
    var plan = await db.Plans.FindAsync(dto.PlanId);
    if (plan == null)
        return Results.BadRequest("Invalid Plan");

    // deactivate old subscriptions
    var activeSubs = await db.Subscriptions
        .Where(s => s.TenantId == dto.TenantDetailsId && s.IsActive)
        .ToListAsync();

    foreach (var sub in activeSubs)
        sub.IsActive = false;

    var subscription = new Subscription
    {
        TenantId = dto.TenantDetailsId,
        PlanId = dto.PlanId,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
        IsActive = true
    };

    db.Subscriptions.Add(subscription);
    await db.SaveChangesAsync();

    return Results.Ok("Subscription activated");
});


// Subscription Status
/*app.MapGet("/api/subscription/status", async (int instituteId, InstituteDbContext db) =>
{
    var subscription = await db.Set<Subscription>()
        .OrderByDescending(s => s.EndDate)
        .FirstOrDefaultAsync(s => s.InstituteId == instituteId);

    if (subscription == null)
        return Results.NotFound();

    var remaining = (subscription.EndDate - DateTime.UtcNow).Days;

    return Results.Ok(new SubscriptionStatusDto
    {
        PlanName = "Active Plan",
        EndDate = subscription.EndDate,
        IsActive = subscription.IsActive && remaining >= 0,
        RemainingDays = remaining
    });
});*/

const int GRACE_DAYS = 5;

app.MapGet("/api/subscription/status", async (Guid TenantDetailsId, TenantDbContext db) =>
{
    var subscription = await db.Subscriptions
        .Include(s => s.Plan)
        .OrderByDescending(s => s.EndDate)
        .FirstOrDefaultAsync(s => s.TenantId == TenantDetailsId);

    if (subscription == null)
        return Results.NotFound();

    var remaining = (subscription.EndDate - DateTime.UtcNow).Days;

    return Results.Ok(new SubscriptionStatusDto
    {
        PlanName = subscription.Plan.PlanName,
        EndDate = subscription.EndDate,
        RemainingDays = remaining,
        IsActive = remaining >= -GRACE_DAYS
    });
});

// Subscription Update
app.MapPut("/api/subscription/{id:int}", async (int id, Subscription dto, TenantDbContext db) =>
{
    var sub = await db.Subscriptions.FindAsync(id);
    if (sub == null) return Results.NotFound("Subscription not found");

    sub.PlanId = dto.PlanId;
    sub.StartDate = dto.StartDate;
    sub.EndDate = dto.EndDate;
    sub.IsActive = dto.IsActive;

    await db.SaveChangesAsync();
    return Results.Ok("Subscription updated successfully");
});


// Delete Subscription
app.MapDelete("/api/subscription/{id:int}", async (int id, TenantDbContext db) =>
{
    var sub = await db.Subscriptions.FindAsync(id);
    if (sub == null)
        return Results.NotFound();

    sub.IsActive = false;
    await db.SaveChangesAsync();

    return Results.Ok("Subscription cancelled");
});


// ==================== LICENSE APIs ====================
// Activate License
string GenerateUniqueLicenseKey(TenantDbContext db)
{
    string key;
    do
    {
        key = $"LIC-{Guid.NewGuid().ToString("N")[..16].ToUpper()}";
    }
    while (db.Licenses.Any(l => l.LicenseKey == key));

    return key;
}


app.MapPost("/api/license/activate", async (LicenseActivateDto dto, TenantDbContext db) =>
{
    var existing = await db.Licenses
        .FirstOrDefaultAsync(l => l.TenantId == dto.TenantDetailsId && l.IsActive);

    if (existing != null)
        return Results.BadRequest("License already active for this institute");

    var licenseKey = GenerateUniqueLicenseKey(db);

    var license = new License
    {
        TenantId = dto.TenantDetailsId,
        LicenseKey = licenseKey,
        ActivatedOn = DateTime.UtcNow,
        SupportExpiry = DateTime.UtcNow.AddYears(1),
        IsActive = true
    };

    db.Licenses.Add(license);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        Message = "License activated successfully",
        LicenseKey = licenseKey,
        SupportExpiry = license.SupportExpiry
    });
});


// Validate License
app.MapGet("/api/license/validate", async (Guid TenantDetailsId, TenantDbContext db) =>
{
    var license = await db.Licenses
        .FirstOrDefaultAsync(l => l.TenantId == TenantDetailsId && l.IsActive);

    if (license == null)
        return Results.Ok(new LicenseStatusDto { IsValid = false });

    return Results.Ok(new LicenseStatusDto
    {
        IsValid = license.SupportExpiry >= DateTime.UtcNow,
        SupportExpiry = license.SupportExpiry
    });
});

// Update License
app.MapPut("/api/license/{id:int}", async (int id, License dto, TenantDbContext db) =>
{
    var license = await db.Licenses.FindAsync(id);
    if (license == null) return Results.NotFound("License not found");

    license.LicenseKey = dto.LicenseKey;
    license.SupportExpiry = dto.SupportExpiry;
    license.IsActive = dto.IsActive;

    await db.SaveChangesAsync();
    return Results.Ok("License updated successfully");
});


// Delete License
app.MapDelete("/api/license/{id:int}", async (int id, TenantDbContext db) =>
{
    var license = await db.Licenses.FindAsync(id);
    if (license == null)
        return Results.NotFound();

    license.IsActive = false;
    await db.SaveChangesAsync();

    return Results.Ok("License deactivated");
});

app.Run();
