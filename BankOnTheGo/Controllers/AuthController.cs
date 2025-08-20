using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankOnTheGo.Models;
using BankOnTheGo.Models.Authentication.Login;
using Microsoft.AspNetCore.Mvc;
using BankOnTheGo.Models.Authentication.SignUp;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using User.Management.Service.Models;
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
        

        public AuthController(IConfiguration configuration, UserManager<IdentityUser> userManager,  RoleManager<IdentityRole> roleManager, IEmailService emailService, SignInManager<IdentityUser> signInManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _signInManager = signInManager;
        }


        [HttpPost("/Auth/Register/")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser, string role)
        {
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new Response { Status = "Error", Message = "User already exists!" });
            }
            
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username,
                TwoFactorEnabled = true,
            };

            if (await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password);

                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new Response { Status = "Error", Message = "User Failed to create" });
                }

                await _userManager.AddToRoleAsync(user, role);
                
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink =
                    Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = user.Email }, Request.Scheme);
                var message = new Message(new string[] { user.Email! }, "Confirmation email link", $"Please confirm your email by clicking this link: {confirmationLink}");
                _emailService.SendEmail(message);
                
                return StatusCode(StatusCodes.Status201Created,
                    new Response { Status = "Success", Message = $"User created & email send to {user.Email} successfully." });
            }
            else
            {
                   return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "This role does not exist." }); 
            }
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
