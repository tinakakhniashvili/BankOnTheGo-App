namespace BankOnTheGo.Application.Interfaces.Wallet;

public interface IIdempotencyExecutor
{
    Task<T> ExecuteAsync<T>(Guid userId, string? key, object requestForHash, Func<Task<T>> action,
        CancellationToken ct);
}