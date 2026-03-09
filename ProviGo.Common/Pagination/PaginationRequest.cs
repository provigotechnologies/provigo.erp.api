using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProviGo.Common.Pagination
{
    public class PaginationRequest
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; } = 1;
        [JsonPropertyName("pageSize")]

        public int PageSize { get; set; } = 10;
        [JsonPropertyName("sortBy")]

        public string? SortBy { get; set; }
        [JsonPropertyName("sortOrder")]

        public string? SortOrder { get; set; } // asc | desc
    }
}
