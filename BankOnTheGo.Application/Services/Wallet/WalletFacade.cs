using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Application.Interfaces.Wallet;
using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Services.Wallet;

public sealed class WalletFacade : IWalletService
{
    private readonly IPaymentLinkService _links;
    private readonly IWalletManagementService _mgmt;
    private readonly IPayoutService _payout;
    private readonly ITransferService _xfer;

    public WalletFacade(IWalletManagementService mgmt, ITransferService xfer, IPayoutService payout,
        IPaymentLinkService links)
    {
        _mgmt = mgmt;
        _xfer = xfer;
        _payout = payout;
        _links = links;
    }

    public Task<WalletDto> CreateAsync(string userId, WalletRequestDto request, CancellationToken ct)
    {
        return _mgmt.CreateAsync(userId, request, ct);
    }

    public Task<IReadOnlyList<WalletDto>> GetMineAsync(string userId, CancellationToken ct)
    {
        return _mgmt.GetMineAsync(userId, ct);
    }

    public Task<WalletDto> GetAsync(string userId, string currency, CancellationToken ct)
    {
        return _mgmt.GetAsync(userId, currency, ct);
    }

    public Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(string userId, string? currency, DateTime? from,
        DateTime? to, CancellationToken ct)
    {
        return _mgmt.GetTransactionsAsync(userId, currency, from, to, ct);
    }

    public Task<TransactionDto> TopUpAsync(string userId, AddTransactionRequestDto request, CancellationToken ct)
    {
        return _mgmt.TopUpAsync(userId, request, ct);
    }

    public Task<TransactionDto?> GetTransactionAsync(Guid userId, Guid transactionId, CancellationToken ct)
    {
        return _mgmt.GetTransactionAsync(userId, transactionId, ct);
    }

    public Task<TransactionDto> TransferAsync(Guid fromUserId, CreateTransferRequestDto request, CancellationToken ct)
    {
        return _xfer.TransferAsync(fromUserId, request, ct);
    }

    public Task<TransactionDto> PayoutAsync(Guid userId, CreatePayoutRequestDto request, CancellationToken ct)
    {
        return _payout.PayoutAsync(userId, request, ct);
    }

    public Task<PaymentLinkDto> CreatePaymentLinkAsync(Guid userId, CreatePaymentLinkRequestDto request,
        CancellationToken ct)
    {
        return _links.CreateAsync(userId, request, ct);
    }

    public Task<PaymentLinkDto?> GetPaymentLinkAsync(Guid userId, string code, CancellationToken ct)
    {
        return _links.GetAsync(userId, code, ct);
    }

    public Task<TransactionDto> PayPaymentLinkAsync(Guid payerUserId, string code, string? idempotencyKey,
        CancellationToken ct)
    {
        return _links.PayAsync(payerUserId, code, idempotencyKey, ct);
    }
}