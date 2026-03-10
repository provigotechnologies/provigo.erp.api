using ProductService.DTOs;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace ProductService.Services.Interface
{
    public interface ITrainerProductService
    {
        Task<ApiResponse<string>> MapTrainerToProductAsync(
            TrainerProductCreateDto dto);

        Task<ApiResponse<List<TrainerProduct>>>GetTrainersByProductAsync(
            PaginationRequest request,
            bool includeInactive);

        Task<ApiResponse<List<int>>> GetProductsByTrainerAsync(
            Guid trainerId);

        Task<ApiResponse<string>> DeleteTrainerProductAsync(int id);
    }
}