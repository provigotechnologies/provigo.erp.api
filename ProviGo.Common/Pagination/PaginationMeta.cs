using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProviGo.Common.Pagination
{
    public class PaginationMeta
    {
        [JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;

    }
}
