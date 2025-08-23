using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankOnTheGo.API.Models;
using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Service.Models.Authentication.Login;
using BankOnTheGo.Service.Models.Authentication.SignUp;
using BankOnTheGo.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BankOnTheGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuthService _authService;
        

        public AuthController(UserManager<IdentityUser> userManager, IEmailService emailService, SignInManager<IdentityUser> signInManager, IJwtTokenService jwtTokenService, IAuthService authService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _authService = authService;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser, string role)
        {
            var result = await _authService.RegisterAsync(registerUser, role);
            if (!result.Success)
                return BadRequest(new { result.Error });
            
            var user = await _userManager.FindByEmailAsync(registerUser.Email);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth",
                new { token = result.Data, email = user.Email }, Request.Scheme);

            var message = new Message(new[] { user.Email }, "Confirm your email", $"Click here: {confirmationLink}");
            await _emailService.SendEmail(message);

            return StatusCode(StatusCodes.Status201Created,
                new { Status = "Success", Message = $"User created & email sent to {user.Email}" });
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var result = await _authService.ConfirmEmailAsync(token, email);

            return result.Success
                ? Ok(result.Data)
                : BadRequest(new { result.Error });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var result = await _authService.LoginAsync(loginModel);

            if (!result.Success)
                return Unauthorized(new
                {
                    result.Error
                });

            return Ok(result.Data);
        }


        [HttpPost("Login-2FA")]
          public async Task<IActionResult> LoginWithOtp(string code, string username)
        {
            var signInResult = await _signInManager.TwoFactorSignInAsync("Email", code, false, false);

            if (!signInResult.Succeeded)
            {
                return Unauthorized(new Response
                {
                    Status = "Error",
                    Message = "Invalid or expired OTP code."
                });
            }
            
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound(new Response
                {
                    Status = "Error",
                    Message = "User not found."
                });
            }
            
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
            
            var jwtToken = _jwtTokenService.GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                expiration = jwtToken.ValidTo
            });
        }

        [HttpPost("Forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.ForgotPasswordAsync(email, baseUrl);

            if (!result.Success)
                return BadRequest(new Response { Status = "Error", Message = result.Error });

            return Ok(new Response { Status = "Success", Message = result.Data!.Message });
        }

        [HttpPost("Reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([Required] string email, [Required] string token, [Required] string newPassword)
        {
            var result = await _authService.ResetPasswordAsync(email, token, newPassword);

            if (!result.Success)
                return BadRequest(new Response { Status = "Error", Message = result.Error });

            return Ok(new Response { Status = "Success", Message = result.Data!.Message });
        }


        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var model = new ResetPassword{ Token = token, Email = email };
            return Ok(new { model });
        }
    }
}
