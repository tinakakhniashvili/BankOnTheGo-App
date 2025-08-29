namespace BankOnTheGo.Shared.Models;

public class ServiceResult<T>
{
    private ServiceResult(bool success, T? data, string? error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public bool Success { get; }
    public string? Error { get; }
    public T? Data { get; }

    public static ServiceResult<T> Ok(T data)
    {
        return new ServiceResult<T>(true, data, null);
    }

    public static ServiceResult<T> Fail(string error)
    {
        return new ServiceResult<T>(false, default, error);
    }
}