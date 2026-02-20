using System.Text.Json.Serialization;

namespace ProviGo.Common.Pagination
{
    public class PagedResult<T>
    {
        [JsonPropertyName("data")]
        public List<T> Items { get; set; } = [];
        public PaginationMeta? Pagination { get; set; }
    }
}
