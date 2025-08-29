using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.Application.Services.Auth;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IEmailService _emailService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel input)
    {
        var name = input.Username?.Trim();
        if (string.IsNullOrEmpty(name))
            return ServiceResult<AuthResponse>.Fail("Invalid credentials.");

        var user = await _userManager.FindByNameAsync(name);
        if (user is null)
            return ServiceResult<AuthResponse>.Fail("Invalid credentials.");

        if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            return ServiceResult<AuthResponse>.Fail("Email not confirmed.");

        var pwd = await _signInManager.CheckPasswordSignInAsync(user, input.Password, true);
        if (pwd.IsLockedOut)
            return ServiceResult<AuthResponse>.Fail("Account locked. Try again later.");
        if (!pwd.Succeeded)
            return ServiceResult<AuthResponse>.Fail("Invalid credentials.");

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        await _emailService.SendEmail(new Message(
            new[] { user.Email! },
            "Your login code",
            $"Your one-time code is: {code}"
        ));

        return ServiceResult<AuthResponse>.Ok(new AuthResponse(
            user.Email!,
            null!,
            null!,
            "Two-factor required. Code sent to email."
        ));
    }
}