namespace AirlineApi.Domain.Entities.API;

public class ApiResponse
{
    public int StatusCode { get; set; }

    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<object> Data { get; set; } = new();
}