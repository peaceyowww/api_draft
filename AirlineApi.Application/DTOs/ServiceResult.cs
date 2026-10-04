namespace AirlineApi.Application.DTOs;


public class ServiceResult<T>
{
    public bool Success { get; init; }
    public List<string> Errors { get; init; } = new();
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ServiceResult<T> Fail(params string[] errors) =>
        new() { Success = false, Errors = errors.ToList() };

    public static ServiceResult<T> Fail(List<string> errors) =>
        new() { Success = false, Errors = errors };
}
