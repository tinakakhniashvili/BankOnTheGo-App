using System.ComponentModel.DataAnnotations;
using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.SignUp;
using BankOnTheGo.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IAuthService _authService;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest(new Response { Status = "Error", Message = "Role is required" });

            var result = await _authService.RegisterAsync(registerUser, role);
            if (!result.Success)
                return BadRequest(new Response { Status = "Error", Message = result.Error });
            
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _authService.SendConfirmationEmailAsync(registerUser.Email, result.Data, baseUrl);

            return StatusCode(201, new Response
            {
                Status = "Success",
                Message = $"User created & email sent to {registerUser.Email}",
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var result = await _authService.ConfirmEmailAsync(token, email);
            return result.Success
                ? Ok(new Response { Status = "Success", Message = "Email confirmed successfully." , IsSuccess = true })
                : BadRequest(new Response { Status = "Error", Message = result.Error });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var result = await _authService.LoginAsync(loginModel);
            return result.Success
                ? Ok(result.Data)
                : Unauthorized(new Response { Status = "Error", Message = result.Error });
        }

        [HttpPost("login-2fa")]
        public async Task<IActionResult> LoginTwoFactor(string code, string username)
        {
            var signInResult = await _signInManager.TwoFactorSignInAsync("Email", code, false, false);
            if (!signInResult.Succeeded)
                return Unauthorized(new Response { Status = "Error", Message = "Invalid or expired OTP code" });

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new Response { Status = "Error", Message = "User not found" });

            var jwtResult = await _authService.GenerateJwtForUserAsync(user);
            return Ok(jwtResult);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.ForgotPasswordAsync(email, baseUrl);
            return result.Success
                ? Ok(new Response { Status = "Success", Message = result.Data!.Message })
                : BadRequest(new Response { Status = "Error", Message = result.Error });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([Required] string email, [Required] string token, [Required] string newPassword)
        {
            var result = await _authService.ResetPasswordAsync(email, token, newPassword);
            return result.Success
                ? Ok(new Response { Status = "Success", Message = result.Data!.Message })
                : BadRequest(new Response { Status = "Error", Message = result.Error });
        }

        [HttpGet("reset-password")]
        [AllowAnonymous]
        public IActionResult GetResetPasswordModel(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return Ok(new { model });
        }
    }
}
