namespace ProviGo.Common.Response
{
    public sealed class ApiMeta
    {
        public string RequestId { get; init; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }

}
