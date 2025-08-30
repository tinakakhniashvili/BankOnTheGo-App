using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Application.Interfaces.Wallet;

namespace BankOnTheGo.Application.Services.Wallet;

public sealed class IdempotencyExecutor : IIdempotencyExecutor
{
    private readonly IIdempotencyStore _store;

    public IdempotencyExecutor(IIdempotencyStore store)
    {
        _store = store;
    }

    public async Task<T> ExecuteAsync<T>(Guid userId, string? key, object requestForHash, Func<Task<T>> action,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return await action();

        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestForHash))));
        var (exists, code, resp) = await _store.TryGetAsync(userId, key!, hash, ct);
        if (exists && resp is not null && code is >= 200 and < 300) return JsonSerializer.Deserialize<T>(resp)!;
        if (exists && code >= 400)
            throw new InvalidOperationException("Duplicate idempotency key with different outcome.");

        var result = await action();
        await _store.SaveAsync(userId, key!, hash, 201, JsonSerializer.Serialize(result), ct);
        return result;
    }
}