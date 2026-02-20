using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace SubscriptionService.Services.Extensions
{
    public static class ApiResultExtensions
    {
        // NON-PAGINATED
        public static IResult OkResponse<T>(
            this T data,
            string message = "Success")
        {
            return Results.Ok(
                ApiResponseFactory.Success(data, message)
            );
        }

        // PAGINATED
        public static IResult OkPagedResponse<T>(
            this PagedResult<T> pagedResult,
            string message = "Success")
        {
            return Results.Ok(
                ApiResponseFactory.PagedSuccess(pagedResult, message)
            );
        }

        public static IResult BadRequestResponse(
            string message,
            params string[] errors)
        {
            return Results.BadRequest(
                ApiResponseFactory.Failure<object>(
                    message,
                    [.. errors]
                )
            );
        }

        public static IResult NotFoundResponse(string message)
        {
            return Results.NotFound(
                ApiResponseFactory.Failure<object>(message)
            );
        }
    }
}
