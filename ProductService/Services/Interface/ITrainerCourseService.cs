using ProductService.DTOs;
using ProviGo.Common.Response;

namespace ProductService.Services.Interface
{
    public interface ITrainerCourseService
    {
        Task<ApiResponse<string>> MapTrainerToCourseAsync(
            TrainerCourseCreateDto dto,
            Guid branchId,
            Guid tenantId);

        Task<ApiResponse<List<TrainerDropdownDto>>>
            GetTrainersByProductAsync(
                int productId,
                Guid branchId,
                Guid tenantId);
    }
}