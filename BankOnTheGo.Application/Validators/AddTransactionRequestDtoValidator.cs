using FluentValidation;
using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Validators;

public sealed class AddTransactionRequestDtoValidator : AbstractValidator<AddTransactionRequestDto>
{
    public AddTransactionRequestDtoValidator()
    {
        RuleFor(x => x.AmountMinor).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
