using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Services.Interface;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace ProductService.Services.Implementation
{
    public class TrainerCourseService(
    TenantDbContext db,
    IGenericRepository<TrainerCourse> repo) : ITrainerCourseService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<TrainerCourse> _repo = repo;

        public async Task<ApiResponse<string>> MapTrainerToCourseAsync(
            TrainerCourseCreateDto dto,
            Guid branchId,
            Guid tenantId)
        {
            // 1️⃣ Validate Trainer
            var trainerExists = await _db.Users
                .AnyAsync(u => u.UserId == dto.TrainerId
                           && u.TenantId == tenantId);

            if (!trainerExists)
                return ApiResponseFactory.Failure<string>("Invalid trainer");

            // 2️⃣ Validate Product
            var productExists = await _db.Products
                .AnyAsync(p => p.ProductId == dto.ProductId
                           && p.TenantId == tenantId
                           && p.BranchId == branchId);

            if (!productExists)
                return ApiResponseFactory.Failure<string>("Invalid product");

            // 3️⃣ Prevent duplicate mapping
            var exists = await _db.TrainerCourses
                .AnyAsync(tc => tc.TrainerId == dto.TrainerId
                            && tc.ProductId == dto.ProductId
                            && tc.TenantId == tenantId
                            && tc.BranchId == branchId);

            if (exists)
                return ApiResponseFactory.Failure<string>("Mapping already exists");

            var mapping = new TrainerCourse
            {
                TenantId = tenantId,
                BranchId = branchId,
                TrainerId = dto.TrainerId,
                ProductId = dto.ProductId,
                IsActive = true
            };

            _db.TrainerCourses.Add(mapping);
            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Trainer mapped successfully");
        }


        public async Task<ApiResponse<List<TrainerDropdownDto>>>
     GetTrainersByProductAsync(
         int productId,
         Guid branchId,
         Guid tenantId)
        {
            try
            {
                var trainers = await _db.TrainerCourses
                    .Where(tc => tc.ProductId == productId
                              && tc.TenantId == tenantId
                              && tc.BranchId == branchId
                              && tc.IsActive)
                    .Select(tc => new TrainerDropdownDto
                    {
                        TrainerId = tc.Trainer.UserId,
                        TrainerName = tc.Trainer.FirstName + " " + tc.Trainer.LastName,
                        Email = tc.Trainer.Email,
                        PhoneNumber = tc.Trainer.PhoneNumber
                    })
                    .ToListAsync();

                return ApiResponseFactory.Success(trainers,
                    "Trainers fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<TrainerDropdownDto>>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }

    }
}