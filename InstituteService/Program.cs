using InstituteService.Data;
using InstituteService.DTOs;
using InstituteService.Services.Extensions;
using InstituteService.Services.Implementation;
using InstituteService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Provigo.Common.Exceptions.Middleware;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;

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
builder.Services.AddTransient<IInstituteService, InstituteService.Services.Implementation.InstituteService>();
// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Institute Service API",
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

//upload image
app.MapPost("/institutes/{id:int}/logo", async (
    Guid id,
    HttpRequest request,
    InstituteDbContext db) =>
{
    var institute = await db.Institutes
        .FirstOrDefaultAsync(i => i.TenantId == id);

    if (institute == null)
        return Results.NotFound("Institute not found");

    var file = request.Form.Files["logo"];
    if (file == null || file.Length == 0)
        return Results.BadRequest("No logo file uploaded");

    // 📁 Create upload folder
    var uploadPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "uploads",
        "institutes"
    );

    if (!Directory.Exists(uploadPath))
        Directory.CreateDirectory(uploadPath);

    // 🧾 File name
    var fileExt = Path.GetExtension(file.FileName);
    var fileName = $"institute_{id}{fileExt}";
    var filePath = Path.Combine(uploadPath, fileName);

    // 💾 Save file
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    // 🌐 Public URL
    var logoUrl = $"/uploads/institutes/{fileName}";

    // 🗄 Save URL to DB
    institute.LogoUrl = logoUrl;
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        logoUrl
    });
});

// Create Institute
app.MapPost("/institutes", async (InstituteDto dto, IInstituteService instituteService) =>
{
    var response = await instituteService.CreateInstituteAsync(dto);
    if (!response.Success)
        return Results.BadRequest(response);

    return Results.Ok(response);


});

// GET All Institutes
app.MapGet("/institutes", async ([AsParameters] PaginationRequest request, bool includeInactive, IInstituteService instituteService) =>
{
    var response = await instituteService.GetInstitutesAsync(request, includeInactive);
    return Results.Ok(response);
});

//Update Institute
app.MapPut("/institutes/{id:int}", async (Guid id, InstituteDto dto, IInstituteService instituteService) =>
{
    var response = await instituteService.UpdateInstituteAsync(id, dto);

    if (!response.Success)
        return Results.BadRequest(response);

    return Results.Ok(response);
});


//Delete institute
app.MapDelete("/institutes/{id:int}", async (Guid id, IInstituteService instituteService) =>
{
    var response = await instituteService.RemoveInstituteAsync(id);

    return response.Success
        ? Results.Ok(response)
        : Results.BadRequest(response);
});


app.Run();

/*//Delete institute
app.MapDelete("/institutes/{id:int}", async (int id, string action, IInstituteService instituteService) =>
{
    var response =  await instituteService.RemoveInstituteAsync(id, action);
    if (!response.Success)
        return Results.BadRequest(response);

    return Results.Ok(response);
});*/

