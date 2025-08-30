namespace BankOnTheGo.Application.Interfaces.Repositories;

public interface IIdempotencyStore
{
    Task<(bool exists, int statusCode, string? response)> TryGetAsync(Guid userId, string key, string requestHash,
        CancellationToken ct);

    Task SaveAsync(Guid userId, string key, string requestHash, int statusCode, string responseBody,
        CancellationToken ct);
}