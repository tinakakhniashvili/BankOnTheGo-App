using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankOnTheGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.Login;
using User.Management.Service.Models.Authentication.SignUp;
using User.Management.Service.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BankOnTheGo.Controllers
{
    [Route("api[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IUserManagement _user;
        

        public AuthController(IConfiguration configuration, UserManager<IdentityUser> userManager,  RoleManager<IdentityRole> roleManager, IEmailService emailService, SignInManager<IdentityUser> signInManager, IUserManagement user)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _signInManager = signInManager;
            _user = user;
        }


        [HttpPost("/Auth/Register/")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            var tokenResponse = await _user.CreateUserWithTokenAsync(registerUser);
            if (tokenResponse.IsSuccess)
            {
                await _user.AssignRoleToUserAsync(registerUser.Roles, tokenResponse.Response.User);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { tokenResponse.Response.Token, email = registerUser.Email}, Request.Scheme);
                var message = new Message(new string[] { registerUser.Email! }, "Confrmation email link", confirmationLink!);
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = "Confrmation email link" });
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Message = tokenResponse.Message, IsSuccess = false });
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new Response {Status = "Success", Message = "Email verified successfully!"});
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "This user does not exist." });
        }

      [HttpPost("/Auth/Login/")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var userRoles = await _userManager.GetRolesAsync(user);

                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                if (user.TwoFactorEnabled)
                {
                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    
                    var message = new Message(new string[] { user.Email! }, "OTP Confirmation",token);
                    _emailService.SendEmail(message);
                    
                    return StatusCode(StatusCodes.Status200OK,
                        new Response { Status = "Success", Message = $"We have sent an OTP to your email {user.Email}" });
                }
                
                var jwtToken = GetToken(authClaims);

                return Ok(
                    new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    });
            }
            return Unauthorized();
        } 
        
        [HttpPost("/Auth/Login-2FA/")]
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
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var jwtToken = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                expiration = jwtToken.ValidTo
            });
        }

        [HttpPost("/Forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var forgotPasswordLink = Url.Action(nameof(ResetPassword), "Auth", new {token, email = user.Email}, Request.Scheme);
                var message = new Message(new string[] { user.Email! }, "Forgot password link",forgotPasswordLink);
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"Password changed request is sent on email {user.Email}. Open your email and verify the link." });
            }
            
            return StatusCode(StatusCodes.Status400BadRequest, new Response {Status = "Error", Message = $"Couldn't send link to email, please try again."});
        }
        
        [HttpPost]
        [AllowAnonymous]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);

            if (user == null)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = "Could not find user with the provided email." }
                );
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(
                user,
                resetPassword.Token,
                resetPassword.Password
            );

            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return BadRequest(ModelState);
            }

            return Ok(new Response { Status = "Success", Message = "Password has been changed" });
        }

        [HttpGet("aleko/reset-password")]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var model = new ResetPassword{ Token = token, Email = email };
            return Ok(new { model });
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256));
            
            return token;
        }
    }
}
