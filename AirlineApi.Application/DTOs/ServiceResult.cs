namespace AirlineApi.Application.DTOs;

// Small helper so services can return either a success payload
// or a list of validation/business errors, similar to how the PHP
// scripts built up an $errors[] array before redirecting back.
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
