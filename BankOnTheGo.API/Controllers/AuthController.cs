using System.ComponentModel.DataAnnotations;
using System.Net;
using BankOnTheGo.API.Helpers;
using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.SignUp;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Domain.Models;
using BankOnTheGo.Infrastructure.Data;
using BankOnTheGo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthFacade _authFacade;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        IAuthFacade authFacade,
        ApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _authFacade = authFacade;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _userManager = userManager;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUser registerUser, [FromQuery] string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest(new Response { Status = "Error", Message = "Role is required", IsSuccess = false });

        var result = await _authFacade.RegisterAsync(registerUser, role);
        if (!result.Success)
            return this.HandleResult(result);

        var token = result.Data;
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedEmail = WebUtility.UrlEncode(registerUser.Email);
        var confirmUrl =
            $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?token={encodedToken}&email={encodedEmail}";

        var message = new Message(
            new[] { registerUser.Email },
            "Confirm your email",
            $"Please confirm your account by clicking this link: {confirmUrl}"
        );

        await _emailService.SendEmail(message);

        return StatusCode(201, new Response
        {
            Status = "Success",
            Message = $"User created & email sent to {registerUser.Email}",
            IsSuccess = true
        });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
    {
        var result = await _authFacade.ConfirmEmailAsync(token, email);
        return result.Success
            ? Ok(new Response { Status = "Success", Message = "Email confirmed successfully.", IsSuccess = true })
            : this.HandleResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var result = await _authFacade.LoginAsync(loginModel);
        return result.Success
            ? Ok(result.Data)
            : Unauthorized(new Response { Status = "Error", Message = result.Error, IsSuccess = false });
    }

    [HttpPost("login-2fa")]
    public async Task<IActionResult> LoginTwoFactor([FromQuery] string code, [FromQuery] string username)
    {
        var user = await _userManager.FindByNameAsync(username)
                   ?? await _userManager.FindByEmailAsync(username);
        if (user == null)
            return NotFound(new Response { Status = "Error", Message = "User not found", IsSuccess = false });

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", code);
        if (!isValid)
            return Unauthorized(new Response
                { Status = "Error", Message = "Invalid or expired OTP code", IsSuccess = false });

        var tokens =
            await _jwtTokenService.GenerateTokensAsync(user, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new
        {
            user.Email,
            tokens.AccessToken,
            RefreshToken = tokens.RefreshToken.Token
        });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([Required] string email)
    {
        var result = await _authFacade.SendResetAsync(email);
        return this.HandleResult(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([Required] string email, [Required] string token,
        [Required] string newPassword)
    {
        var result = await _authFacade.ResetAsync(email, token, newPassword);
        return this.HandleResult(result);
    }

    [HttpGet("reset-password")]
    [AllowAnonymous]
    public IActionResult GetResetPasswordModel([FromQuery] string token, [FromQuery] string email)
    {
        return Ok(new { model = new ResetPassword { Token = token, Email = email } });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || !tokenEntity.IsActive)
            return Unauthorized(new Response
                { Status = "Error", Message = "Invalid or expired refresh token", IsSuccess = false });

        var newTokens =
            await _jwtTokenService.RefreshTokenAsync(tokenEntity, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new { newTokens.AccessToken, RefreshToken = newTokens.RefreshToken.Token });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (tokenEntity == null)
            return NotFound(new Response { Status = "Error", Message = "Token not found", IsSuccess = false });

        tokenEntity.Revoke(HttpContext.Connection.RemoteIpAddress?.ToString());
        await _context.SaveChangesAsync();

        return Ok(new Response { Status = "Success", Message = "Logged out successfully", IsSuccess = true });
    }
}