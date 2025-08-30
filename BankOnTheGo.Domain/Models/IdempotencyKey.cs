namespace BankOnTheGo.Domain.Models;

public class IdempotencyKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Key { get; set; } = default!;
    public string RequestHash { get; set; } = default!;
    public string ResponseBody { get; set; } = default!;
    public int StatusCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}