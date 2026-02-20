using ProviGo.Common.Pagination;

namespace ProviGo.Common.Response
{
    public static class ApiResponseFactory
    {
        // NON-PAGINATED SUCCESS
        public static ApiResponse<T> Success<T>(
            T data,
            string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Errors = null
            };
        }

        // PAGINATED SUCCESS
        public static ApiResponse<List<T>> PagedSuccess<T>(
            PagedResult<T> pagedResult,
            string message = "Success")
        {
            return new ApiResponse<List<T>>
            {
                Success = true,
                Message = message,
                Data = [.. pagedResult.Items],
                Pagination = pagedResult.Pagination,
                Errors = null
            };
        }

        // FAILURE
        public static ApiResponse<T> Failure<T>(
            string message,
            List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors
            };
        }
    }
    }
