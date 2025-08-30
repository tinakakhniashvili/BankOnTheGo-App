using System.Collections.Concurrent;
using BankOnTheGo.Application.Interfaces.Repositories;

namespace BankOnTheGo.Infrastructure.Repositories;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<(Guid userId, string key, string hash), (int status, string response)>
        _store = new();

    public Task<(bool exists, int statusCode, string? response)> TryGetAsync(Guid userId, string key,
        string requestHash, CancellationToken ct)
    {
        var exists = _store.TryGetValue((userId, key, requestHash), out var val);
        return Task.FromResult((exists, exists ? val.status : 0, exists ? val.response : null));
    }

    public Task SaveAsync(Guid userId, string key, string requestHash, int statusCode, string responseBody,
        CancellationToken ct)
    {
        _store[(userId, key, requestHash)] = (statusCode, responseBody);
        return Task.CompletedTask;
    }
}