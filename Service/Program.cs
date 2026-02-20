using InstituteService.DTOs;
using InstituteService.Models;
using InstituteService.DTOs;
using Microsoft.EntityFrameworkCore;
using InstituteService.Data;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<InstituteDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                     ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddCors();
builder.Services.AddControllers();
var app = builder.Build();

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

// ➕ Create Institute
app.MapPost("/institutes", async (InstituteDto dto, InstituteDbContext db) =>
{
    var institute = new Institute
    {
        Name = dto.Name,
        Address = dto.Address,
        LogoPath = dto.LogoPath,
        CreatedAt = DateTime.UtcNow,
        LastUpdatedAt = DateTime.UtcNow
    };

    db.Institutes.Add(institute);
    await db.SaveChangesAsync();

    return Results.Ok(institute);
});

// 📋 Get All Institutes
app.MapGet("/institutes", async (InstituteDbContext db) =>
{
    var institutes = await db.Institutes
        .Select(i => new InstituteDto
        {
            Id = i.Id,
            Name = i.Name,
            Address = i.Address,
            LogoPath = i.LogoPath
        })
        .ToListAsync();

    return Results.Ok(institutes);
});

// ✏ Update Institute
app.MapPut("/institutes/{id:int}", async (int id, InstituteDto dto, InstituteDbContext db) =>
{
    var institute = await db.Institutes.FindAsync(id);
    if (institute == null) return Results.NotFound("Institute not found");

    institute.Name = dto.Name;
    institute.Address = dto.Address;
    institute.LogoPath = dto.LogoPath;
    institute.LastUpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(institute);
});

// ❌ Delete Institute
app.MapDelete("/institutes/{id:int}", async (int id, InstituteDbContext db) =>
{
    var institute = await db.Institutes.FindAsync(id);
    if (institute == null) return Results.NotFound("Institute not found");

    db.Institutes.Remove(institute);
    await db.SaveChangesAsync();
    return Results.Ok("Institute deleted successfully");
});

app.Run();
