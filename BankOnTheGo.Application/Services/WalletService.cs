// using System.Security.Cryptography;
// using System.Text;
// using System.Text.Json;
// using BankOnTheGo.Application.Interfaces;
// using BankOnTheGo.Application.Interfaces.Ledger;
// using BankOnTheGo.Application.Interfaces.Repositories;
// using BankOnTheGo.Domain.DTOs;
// using BankOnTheGo.Domain.DTOs.Ledger;
// using BankOnTheGo.Domain.Models;
//
// namespace BankOnTheGo.Application.Services;
//
// public sealed class WalletService : IWalletService
// {
//     private readonly ILedgerService _ledger;              
//     private readonly IWalletRepository _walletRepo;       
//     private readonly ILedgerRepository _ledgerRepo;
//     private readonly IPaymentLinkRepository _paymentLinks;
//     private readonly IIdempotencyStore _idempotency;      
//
//     public WalletService(
//         ILedgerService ledger,
//         IWalletRepository walletRepo,
//         ILedgerRepository ledgerRepo,
//         IPaymentLinkRepository paymentLinks,
//         IIdempotencyStore idempotency)
//     {
//         _ledger = ledger;
//         _walletRepo = walletRepo;
//         _ledgerRepo = ledgerRepo;
//         _paymentLinks = paymentLinks;
//         _idempotency = idempotency;
//     }
//
//     public async Task<WalletDto> CreateAsync(string userId, WalletRequestDto request, CancellationToken ct)
//     {
//         if (string.IsNullOrWhiteSpace(userId))
//             throw new ArgumentException("User id is required.", nameof(userId));
//         if (request is null) throw new ArgumentNullException(nameof(request));
//         if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
//             throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(request.Currency));
//
//         var currency = request.Currency.ToUpperInvariant();
//
//         var existing = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency, ct);
//         if (existing is not null)
//             throw new InvalidOperationException("Wallet already exists for this user and currency.");
//
//         var created = await _walletRepo.CreateAsync(userId, currency, ct);
//         return MapWallet(created, 0);
//     }
//
//     public async Task<IReadOnlyList<WalletDto>> GetMineAsync(string userId, CancellationToken ct)
//     {
//         if (string.IsNullOrWhiteSpace(userId))
//             throw new ArgumentException("User id is required.", nameof(userId));
//
//         var wallets = await _walletRepo.GetAllByUserAsync(userId, ct);
//         if (wallets.Count == 0)
//             throw new InvalidOperationException("Wallet not found.");
//
//         var result = new List<WalletDto>(wallets.Count);
//         foreach (var w in wallets)
//         {
//             var balance = await _walletRepo.GetBalanceMinorAsync(w.Id, ct);
//             result.Add(MapWallet(w, balance));
//         }
//
//         return result;
//     }
//
//     public async Task<WalletDto> GetAsync(string userId, string currency, CancellationToken ct)
//     {
//         if (string.IsNullOrWhiteSpace(userId))
//             throw new ArgumentException("User id is required.", nameof(userId));
//         if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
//             throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
//
//         var wallet = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency.ToUpperInvariant(), ct)
//                      ?? throw new InvalidOperationException("Wallet not found.");
//
//         var balance = await _walletRepo.GetBalanceMinorAsync(wallet.Id, ct);
//         return MapWallet(wallet, balance);
//     }
//
//     public async Task<TransactionDto> TopUpAsync(string userId, AddTransactionRequestDto request, CancellationToken ct)
//     {
//         if (string.IsNullOrWhiteSpace(userId))
//             throw new ArgumentException("User id is required.", nameof(userId));
//         if (request is null) throw new ArgumentNullException(nameof(request));
//         if (request.AmountMinor <= 0)
//             throw new ArgumentException("Amount must be greater than zero.", nameof(request.AmountMinor));
//         if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
//             throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(request.Currency));
//
//         var currency = request.Currency.ToUpperInvariant();
//
//         var wallet = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency, ct)
//                      ?? throw new InvalidOperationException("Wallet not found.");
//
//         if (wallet.Status == WalletStatus.Locked)
//             throw new InvalidOperationException("Wallet is locked.");
//
//         var entry = new LedgerEntry
//         {
//             WalletId = wallet.Id,
//             AmountMinor = request.AmountMinor,
//             Currency = wallet.Currency,
//             Type = LedgerEntryType.TopUp,
//             Note = request.Note,
//             CreatedAtUtc = DateTime.UtcNow
//         };
//
//         await _walletRepo.AddLedgerAsync(entry, ct);
//         return MapTransaction(entry);
//     }
//
//     public async Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(
//         string userId, string? currency, DateTime? from, DateTime? to, CancellationToken ct)
//     {
//         if (string.IsNullOrWhiteSpace(userId))
//             throw new ArgumentException("User id is required.", nameof(userId));
//
//         Wallet wallet;
//         if (!string.IsNullOrWhiteSpace(currency))
//         {
//             wallet = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency!.ToUpperInvariant(), ct)
//                      ?? throw new InvalidOperationException("Wallet not found.");
//         }
//         else
//         {
//             var wallets = await _walletRepo.GetAllByUserAsync(userId, ct);
//             if (wallets.Count == 0) throw new InvalidOperationException("Wallet not found.");
//             if (wallets.Count > 1) throw new ArgumentException("Currency is required when multiple wallets exist.");
//             wallet = wallets[0];
//         }
//
//         var fromUtc = from?.ToUniversalTime();
//         var toUtc = to?.ToUniversalTime();
//
//         var entries = await _walletRepo.GetLedgerAsync(wallet.Id, fromUtc, toUtc, ct);
//         return entries.OrderByDescending(e => e.CreatedAtUtc).Select(MapTransaction).ToList();
//     }
//
//     public async Task<TransactionDto> TransferAsync(Guid fromUserId, CreateTransferRequestDto request, CancellationToken ct)
// {
//     if (request is null) throw new ArgumentNullException(nameof(request));
//     EnsurePositive(request.Amount);
//
//     if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
//     {
//         var hash = Hash(request);
//         var (exists, codeStatus, resp) = await _idempotency.TryGetAsync(fromUserId, request.IdempotencyKey!, hash, ct);
//         if (exists && resp is not null && codeStatus is >= 200 and < 300) return JsonSerializer.Deserialize<TransactionDto>(resp)!;
//         if (exists && codeStatus is >= 400) throw new InvalidOperationException("Duplicate idempotency key with different outcome.");
//     }
//
//     var fromAccount = await _walletRepo.GetAccountAsync(fromUserId, request.Currency, ct) 
//                       ?? throw new InvalidOperationException("Source wallet not found.");
//     var toAccount   = await _walletRepo.GetAccountAsync(request.ToUserId, request.Currency, ct) 
//                       ?? throw new InvalidOperationException("Destination wallet not found.");
//
//     var txnId = Guid.NewGuid();
//     var amount = new MoneyDto(request.Amount.Amount, request.Currency);
//
//
//     var postReq = new PostJournalEntryRequestDto
//     {
//         TransactionId = txnId,
//         Lines = new List<JournalLineDto>
//         {
//             new(toAccount.Id, EntryDirection.Debit,  amount),
//             new(fromAccount.Id, EntryDirection.Credit, amount)
//         }
//     };
//
//     await _ledger.PostJournalEntryAsync(postReq, ct);
//     var txn = await _ledgerRepo.GetTransactionAsync(fromUserId, txnId, ct) 
//               ?? throw new InvalidOperationException("Transaction not found.");
//
//     var dto = new TransactionDto(
//         txn.Id,
//         txn.Type.ToString(),            
//         (long)amount.Amount,                  
//         request.Currency,
//         txn.CreatedAt.UtcDateTime,
//         request.Memo
//     );
//
//
//     if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
//     {
//         var respBody = JsonSerializer.Serialize(dto);
//         await _idempotency.SaveAsync(fromUserId, request.IdempotencyKey!, Hash(request), 201, respBody, ct);
//     }
//
//     return dto;
// }
//
//
//
//     public async Task<TransactionDto> PayoutAsync(Guid userId, CreatePayoutRequestDto request, CancellationToken ct)
// {
//     if (request is null) throw new ArgumentNullException(nameof(request));
//     EnsurePositive(request.Amount);
//
//     if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
//     {
//         var hash = Hash(request);
//         var (exists, codeStatus, resp) = await _idempotency.TryGetAsync(userId, request.IdempotencyKey!, hash, ct);
//         if (exists && resp is not null && codeStatus is >= 200 and < 300) return JsonSerializer.Deserialize<TransactionDto>(resp)!;
//         if (exists && codeStatus is >= 400) throw new InvalidOperationException("Duplicate idempotency key with different outcome.");
//     }
//
//     var userAccount = await _walletRepo.GetAccountAsync(userId, request.Currency, ct) ?? throw new InvalidOperationException("User wallet not found.");
//     var settlement = await _ledgerRepo.GetSystemSettlementAccountAsync(request.Currency, ct) ?? throw new InvalidOperationException("Settlement account not configured.");
//
//     var txnId = Guid.NewGuid();
//     var postReq = new PostJournalEntryRequestDto
//     {
//         TransactionId = txnId,
//         Lines =
//         [
//             new JournalLineDto { AccountId = settlement.Id, Amount = new MoneyDto { Amount = request.Amount.Amount, Currency = request.Currency }, Side = "DR" },
//             new JournalLineDto { AccountId = userAccount.Id, Amount = new MoneyDto { Amount = request.Amount.Amount, Currency = request.Currency }, Side = "CR" }
//         ]
//     };
//
//     await _ledger.PostJournalEntryAsync(postReq, ct);
//     var txn = await _ledgerRepo.GetTransactionAsync(userId, txnId, ct) ?? throw new InvalidOperationException("Transaction not found.");
//
//     var dto = new TransactionDto
//     {
//         Id = txn.Id,
//         Amount = new MoneyDto { Amount = request.Amount.Amount, Currency = request.Currency },
//         Currency = request.Currency,
//         CreatedAt = txn.CreatedAt,
//         Memo = $"Payout to {request.Destination}",
//         State = txn.State.ToString()
//     };
//
//     if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
//     {
//         var respBody = JsonSerializer.Serialize(dto);
//         await _idempotency.SaveAsync(userId, request.IdempotencyKey!, Hash(request), 201, respBody, ct);
//     }
//
//     return dto;
// }
//
//
//     public async Task<TransactionDto?> GetTransactionAsync(Guid userId, Guid transactionId, CancellationToken ct)
//     {
//         var txn = await _ledgerRepo.GetTransactionAsync(userId, transactionId, ct);
//         if (txn is null) return null;
//
//         return new TransactionDto
//         {
//             Id = txn.Id,
//             Amount = new MoneyDto { Amount = txn.Amount.Amount, Currency = txn.Amount.Currency },
//             Currency = txn.Amount.Currency,
//             CreatedAt = txn.CreatedAt,
//             Memo = txn.Memo,
//             State = txn.State.ToString()
//         };
//     }
//
//     public async Task<PaymentLinkDto> CreatePaymentLinkAsync(Guid userId, CreatePaymentLinkRequestDto request, CancellationToken ct)
//     {
//         if (request is null) throw new ArgumentNullException(nameof(request));
//         EnsurePositive(request.Amount);
//
//         var link = new PaymentLink
//         {
//             Id = Guid.NewGuid(),
//             OwnerUserId = userId,
//             Code = GenerateCode(10),
//             Amount = new Money { Amount = request.Amount.Amount, Currency = request.Currency },
//             Currency = request.Currency,
//             Memo = request.Memo,
//             CreatedAt = DateTimeOffset.UtcNow,
//             ExpiresAt = request.ExpiresAt,
//             Status = "Active"
//         };
//
//         await _paymentLinks.AddAsync(link, ct);
//
//         return new PaymentLinkDto(
//             link.Id,
//             link.Code,
//             new MoneyDto { Amount = link.Amount.Amount, Currency = link.Currency },
//             link.Currency,
//             link.Memo,
//             link.CreatedAt,
//             link.ExpiresAt,
//             link.Status);
//     }
//
//     private static string GenerateCode(int length)
//     {
//         const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
//         Span<char> buf = stackalloc char[length];
//         var rnd = RandomNumberGenerator.Create();
//         var bytes = new byte[length];
//         rnd.GetBytes(bytes);
//         for (int i = 0; i < length; i++)
//             buf[i] = alphabet[bytes[i] % alphabet.Length];
//         return new string(buf);
//     }
//
//     public async Task<PaymentLinkDto?> GetPaymentLinkAsync(Guid userId, string code, CancellationToken ct)
//     {
//         if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Invalid code.", nameof(code));
//
//         var link = await _paymentLinks.GetByCodeAsync(code, ct);
//         if (link is null) return null;
//
//         var isOwner = link.OwnerUserId == userId;
//         var memo = isOwner ? link.Memo : link.Memo;
//
//         if (link.ExpiresAt is not null && link.ExpiresAt < DateTimeOffset.UtcNow && link.Status == "Active")
//         {
//             link.Status = "Expired";
//             await _paymentLinks.UpdateAsync(link, ct);
//         }
//
//         return new PaymentLinkDto(
//             link.Id,
//             link.Code,
//             new MoneyDto { Amount = link.Amount.Amount, Currency = link.Currency },
//             link.Currency,
//             memo,
//             link.CreatedAt,
//             link.ExpiresAt,
//             link.Status);
//     }
//
//     public async Task<TransactionDto> PayPaymentLinkAsync(Guid payerUserId, string code, string? idempotencyKey, CancellationToken ct)
// {
//     if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Invalid code.", nameof(code));
//
//     if (!string.IsNullOrWhiteSpace(idempotencyKey))
//     {
//         var idemPayload = new { payerUserId, code };
//         var idemHash = Hash(idemPayload);
//         var (exists, codeStatus, resp) = await _idempotency.TryGetAsync(payerUserId, idempotencyKey!, idemHash, ct);
//         if (exists && resp is not null && codeStatus is >= 200 and < 300) return JsonSerializer.Deserialize<TransactionDto>(resp)!;
//         if (exists && codeStatus is >= 400) throw new InvalidOperationException("Duplicate idempotency key with different outcome.");
//     }
//
//     var link = await _paymentLinks.GetByCodeAsync(code, ct) ?? throw new InvalidOperationException("Payment link not found.");
//     if (link.ExpiresAt is not null && link.ExpiresAt < DateTimeOffset.UtcNow) { if (link.Status == "Active") { link.Status = "Expired"; await _paymentLinks.UpdateAsync(link, ct); } throw new InvalidOperationException("Payment link expired."); }
//     if (!string.Equals(link.Status, "Active", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Payment link is not active.");
//     if (link.OwnerUserId == payerUserId) throw new InvalidOperationException("Cannot pay your own payment link.");
//
//     var payerAccount = await _walletRepo.GetAccountAsync(payerUserId, link.Currency, ct) ?? throw new InvalidOperationException("Payer wallet not found.");
//     var receiverAccount = await _walletRepo.GetAccountAsync(link.OwnerUserId, link.Currency, ct) ?? throw new InvalidOperationException("Receiver wallet not found.");
//
//     var amountDto = new MoneyDto { Amount = link.Amount.Amount, Currency = link.Currency };
//     EnsurePositive(amountDto);
//
//     var txnId = Guid.NewGuid();
//     var postReq = new PostJournalEntryRequestDto
//     {
//         TransactionId = txnId,
//         Lines =
//         [
//             new JournalLineDto { AccountId = receiverAccount.Id, Amount = amountDto, Side = "DR" },
//             new JournalLineDto { AccountId = payerAccount.Id,    Amount = amountDto, Side = "CR" }
//         ]
//     };
//
//     await _ledger.PostJournalEntryAsync(postReq, ct);
//     var txn = await _ledgerRepo.GetTransactionAsync(payerUserId, txnId, ct) ?? throw new InvalidOperationException("Transaction not found.");
//
//     link.Status = "Paid";
//     await _paymentLinks.UpdateAsync(link, ct);
//
//     var dto = new TransactionDto
//     {
//         Id = txn.Id,
//         Amount = amountDto,
//         Currency = link.Currency,
//         CreatedAt = txn.CreatedAt,
//         Memo = link.Memo,
//         State = txn.State.ToString()
//     };
//
//     if (!string.IsNullOrWhiteSpace(idempotencyKey))
//     {
//         var idemPayload = new { payerUserId, code };
//         var idemHash = Hash(idemPayload);
//         var respBody = JsonSerializer.Serialize(dto);
//         await _idempotency.SaveAsync(payerUserId, idempotencyKey!, idemHash, 201, respBody, ct);
//     }
//
//     return dto;
// }
//
//
//     private static WalletDto MapWallet(Wallet w, long balanceMinor)
//     {
//         return new WalletDto(
//             w.Id,
//             w.Currency,
//             w.Status.ToString(),
//             balanceMinor
//         );
//     }
//
//     private static TransactionDto MapTransaction(LedgerEntry e)
//     {
//         return new TransactionDto(
//             e.Id,
//             e.Type.ToString(),
//             e.AmountMinor,
//             e.Currency,
//             e.CreatedAtUtc,
//             e.Note
//         );
//     }
//
//     private static string Hash(object obj)
//     {
//         var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { IgnoreNullValues = true });
//         return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
//     }
//
//     private static void EnsureCurrency(string expected, string actual)
//     {
//         if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
//             throw new ArgumentException("Currency mismatch.");
//     }
//
//     private static void EnsurePositive(MoneyDto amount)
//     {
//         if (amount.Amount <= 0) throw new ArgumentException("Amount must be positive.");
//     }
// }

