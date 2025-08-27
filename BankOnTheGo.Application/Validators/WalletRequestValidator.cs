using BankOnTheGo.Domain.DTOs;
using FluentValidation;

namespace BankOnTheGo.Application.Validators;

public class WalletRequestValidator  : AbstractValidator<WalletRequestDto>
{
    public WalletRequestValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Must(c => c.All(char.IsLetter))
            .WithMessage("Currency must be a 3-letter ISO code.");
    }
}