namespace Provigo.Common.Exceptions
{
    public class ApiErrorResponse
    {
        public string TraceId { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Path { get; set; }
    }
}
