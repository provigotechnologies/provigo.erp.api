using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProviGo.Common.Pagination;

namespace ProviGo.Common.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }

        // Reference only (no logic)
        public PaginationMeta? Pagination { get; init; }

        public List<string>? Errors { get; init; }
        public ApiMeta Meta { get; init; } = new();
    }
}
