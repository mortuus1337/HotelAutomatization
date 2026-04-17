namespace Hotel.API.Middleware;

public class ApiErrorResponse
{
    public string Code { get; set; } = "internal_error";
    public string Message { get; set; } = "Internal server error.";
    public int Status { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, string[]>? Errors { get; set; }
}
