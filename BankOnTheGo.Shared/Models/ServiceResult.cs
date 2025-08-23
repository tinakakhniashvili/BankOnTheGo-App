namespace BankOnTheGo.Shared.Models;

public class ServiceResult<T>
{
    public bool Success { get; }
    public string? Error { get; }
    public T? Data { get; }

    private ServiceResult(bool success, T? data, string? error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public static ServiceResult<T> Ok(T data) => new(true, data, null);
    public static ServiceResult<T> Fail(string error) => new(false, default, error);
}