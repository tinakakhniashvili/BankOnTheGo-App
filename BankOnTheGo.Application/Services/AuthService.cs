using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankOnTheGo.Service.Models;
using Microsoft.AspNetCore.Identity;
using BankOnTheGo.Service.Models.Authentication.Login;
using BankOnTheGo.Service.Models.Authentication.Responses;
using BankOnTheGo.Service.Models.Authentication.SignUp;
using BankOnTheGo.Service.Services.Interfaces;

namespace BankOnTheGo.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RoleManager<IdentityRole> _roleManager;


    public AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailService emailService, IJwtTokenService jwtTokenService, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _jwtTokenService=jwtTokenService;
        _roleManager = roleManager;
    }
    
    public async Task<ServiceResult<string>> RegisterAsync(RegisterUser registerUser, string role)
    {
        var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
        if (userExist != null)
            return ServiceResult<string>.Fail("User already exists.");

        IdentityUser user = new()
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

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return ServiceResult<ConfirmEmailResponse>.Fail("Email confirmation failed.");

        return ServiceResult<ConfirmEmailResponse>.Ok(new ConfirmEmailResponse(user.Email, true));
    }


    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel loginModel)
    {
        var user = await _userManager.FindByNameAsync(loginModel.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
        {
            return ServiceResult<AuthResponse>.Fail("Invalid username or password");
        }
        
        await _signInManager.SignOutAsync();
        await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);
        
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        if (user.TwoFactorEnabled)
        {
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            var message = new Message(new string[] { user.Email! }, "OTP Confirmation", token);
            _emailService.SendEmail(message);

            return ServiceResult<AuthResponse>.Ok(new AuthResponse(
                Email: user.Email!,
                Token: null,
                Message: $"We have sent an OTP to your email {user.Email}"
            ));
        }
        
        var jwtToken = _jwtTokenService.GetToken(authClaims);

        return ServiceResult<AuthResponse>.Ok(new AuthResponse(
            Email: user.Email!,
            Token: new JwtSecurityTokenHandler().WriteToken(jwtToken),
            Message: null
        ));
    }
    


    public async Task<ServiceResult<ResetPasswordResponse>> ForgotPasswordAsync(string email, string baseUrl)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return ServiceResult<ResetPasswordResponse>.Fail("User does not exist.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var forgotPasswordLink = $"{baseUrl}/Auth/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email!)}";

        var message = new Message(new string[] { user.Email! }, "Reset Password", $"Click this link to reset your password: {forgotPasswordLink}");
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


    public Task<ServiceResult<AuthResponse>> LoginWithOtpAsync(string code, string username)
    {
        throw new NotImplementedException();
    }
}