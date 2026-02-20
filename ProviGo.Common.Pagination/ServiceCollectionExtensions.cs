using Microsoft.Extensions.DependencyInjection;

namespace ProviGo.Common.Pagination
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonPagination(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            return services;
        }
    }

}
