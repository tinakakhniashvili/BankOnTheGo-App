using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.SignUp;
using Microsoft.AspNetCore.Identity;
using BankOnTheGo.Shared.Models;
using BankOnTheGo.Domain.Authentication.User;
using Microsoft.AspNetCore.Http;

namespace BankOnTheGo.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IJwtTokenService jwtTokenService,
        RoleManager<IdentityRole> roleManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServiceResult<string>> RegisterAsync(RegisterUser registerUser, string role)
    {
        var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
        if (userExist != null)
            return ServiceResult<string>.Fail("User already exists.");

        var user = new ApplicationUser
        {
            Email = registerUser.Email,
            UserName = registerUser.Username,
            SecurityStamp = Guid.NewGuid().ToString(),
            TwoFactorEnabled = true
        };

        if (!await _roleManager.RoleExistsAsync(role))
            return ServiceResult<string>.Fail("Role does not exist.");

        var result = await _userManager.CreateAsync(user, registerUser.Password);
        if (!result.Succeeded)
            return ServiceResult<string>.Fail("User creation failed.");

        await _userManager.AddToRoleAsync(user, role);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return ServiceResult<string>.Ok(token);
    }

    public async Task<ServiceResult<ConfirmEmailResponse>> ConfirmEmailAsync(string token, string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return ServiceResult<ConfirmEmailResponse>.Fail("User does not exist.");

        token = Uri.UnescapeDataString(token);

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ServiceResult<ConfirmEmailResponse>.Fail($"Email confirmation failed: {errors}");
        }

        return ServiceResult<ConfirmEmailResponse>.Ok(new ConfirmEmailResponse(user.Email, true));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel loginModel)
    {
        var user = await _userManager.FindByNameAsync(loginModel.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
            return ServiceResult<AuthResponse>.Fail("Invalid username or password");

        await _signInManager.SignOutAsync();
        await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);

        if (user.TwoFactorEnabled)
        {
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            var message = new Message(new[] { user.Email! }, "OTP Confirmation", token);
            _emailService.SendEmail(message);

            return ServiceResult<AuthResponse>.Ok(new AuthResponse(
                Email: user.Email!,
                Token: null,
                RefreshToken: null,
                Message: $"We have sent an OTP to your email {user.Email}"
            ));
        }
        
        var ipAddress = GetClientIp();
        var tokens = await _jwtTokenService.GenerateTokensAsync(user, ipAddress);

        
        return ServiceResult<AuthResponse>.Ok(new AuthResponse(
            Email: user.Email!,
            Token: tokens.AccessToken,
            RefreshToken: tokens.RefreshToken.Token,
            Message: null
        ));
    }


    public async Task<ServiceResult<ResetPasswordResponse>> ForgotPasswordAsync(string email, string baseUrl)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return ServiceResult<ResetPasswordResponse>.Fail("User does not exist.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var forgotPasswordLink = $"{baseUrl}/api/Auth/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email!)}";
        var message = new Message(new[] { user.Email! }, "Reset Password", $"Click this link to reset your password: {forgotPasswordLink}");
        _emailService.SendEmail(message);

        return ServiceResult<ResetPasswordResponse>.Ok(new ResetPasswordResponse(user.Email!, false, $"Password reset link sent to {user.Email}."));
    }

    public async Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return ServiceResult<ResetPasswordResponse>.Fail("User does not exist.");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return ServiceResult<ResetPasswordResponse>.Fail("Password reset failed. Token may be invalid or expired.");

        return ServiceResult<ResetPasswordResponse>.Ok(new ResetPasswordResponse(user.Email!, true, "Password has been reset successfully."));
    }

    public async Task SendConfirmationEmailAsync(string email, string token, string baseUrl)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found");

        var encodedToken = Uri.EscapeDataString(token);
        var confirmationLink = $"{baseUrl}/api/auth/confirm-email?token={encodedToken}&email={email}";

        var message = new Message(
            new[] { email },
            "Confirm your email",
            $"Hello {user.UserName}, please confirm your account by clicking this link: {confirmationLink}"
        );

        await _emailService.SendEmail(message);
    }

    public async Task<object> GenerateJwtForUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            authClaims.Add(new Claim(ClaimTypes.Role, role));

        var token = _jwtTokenService.GetToken(authClaims);

        return new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        };
    }
    
    // helper 
    
    private string GetClientIp()
    {
        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        
        if (string.IsNullOrEmpty(ip) && 
            _httpContextAccessor.HttpContext?.Request?.Headers != null)
        {
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                ip = forwarded.FirstOrDefault()?.Split(',')[0];
            }
            else if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                ip = realIp.FirstOrDefault();
            }
        }

        return string.IsNullOrEmpty(ip) ? "unknown" : ip;
    }
}
