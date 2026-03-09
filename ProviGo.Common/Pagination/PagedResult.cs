using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProviGo.Common.Pagination
{
    public class PagedResult<T>
    {
        [JsonPropertyName("data")]
        public List<T> Items { get; set; } = [];
        public PaginationMeta? Pagination { get; set; }
    }
}
