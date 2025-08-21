using Microsoft.AspNetCore.Identity;
using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.Login;
using User.Management.Service.Models.Authentication.SignUp;
using User.Management.Service.Models.Authentication.User;

namespace User.Management.Service.Services;

public class UserManagement : IUserManagement
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public UserManagement( UserManager<IdentityUser> userManager,  RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }
    
    public async Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser)
    {
        var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
        if (userExist != null)
        {
            return new ApiResponse<CreateUserResponse> 
            { 
                IsSuccess = false, 
                StatusCode = 403, 
                Message = "User already exists" 
            };
        }
        
        IdentityUser user = new()
        {
            Email = registerUser.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerUser.Username,
            TwoFactorEnabled = true,
        };
    
        var result = await _userManager.CreateAsync(user, registerUser.Password);
        if (result.Succeeded)
        {
             var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return new ApiResponse<CreateUserResponse> 
            { 
                Response = new CreateUserResponse(){User = user, Token = token},
                IsSuccess = true, 
                StatusCode = 201, 
                Message = "User registered successfully." 
            };
        }
        else
        {
            return new ApiResponse<CreateUserResponse> 
            { 
                IsSuccess = false, 
                StatusCode = 500, 
                Message = "User failed to register." 
            };
        }
    }


    public async Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, IdentityUser user)
    {
        var assignedRole = new List<string>();
        foreach (var role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                     await _userManager.AddToRoleAsync(user, role);
                     assignedRole.Add(role);
                }
            }
        }

        return new ApiResponse<List<string>>
        {
            IsSuccess = true, StatusCode = 200, Message="Roles has been assigned",
            Response = assignedRole
        };
        
    }

    public async Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsyn(LoginModel loginModel)
    {
        var user = await _userManager.FindByNameAsync(loginModel.Username);
        if (user == null)
        {
            return new ApiResponse<LoginOtpResponse>
            {
                IsSuccess = false,
                StatusCode = 404,
                Message = "User does not exist"
            };
        }

        await _signInManager.SignOutAsync();
        var signInResult = await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);

        if (!signInResult.Succeeded)
        {
            return new ApiResponse<LoginOtpResponse>
            {
                IsSuccess = false,
                StatusCode = 401,
                Message = "Invalid username or password"
            };
        }

        if (!user.TwoFactorEnabled)
        {
            return new ApiResponse<LoginOtpResponse>
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Two-factor authentication is not enabled for this user."
            };
        }

        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

        return new ApiResponse<LoginOtpResponse>
        {
            Response = new LoginOtpResponse
            {
                User = user,
                Token = token,
                IsTwoFactorEnable = true
            },
            IsSuccess = true,
            StatusCode = 200,
            Message = $"OTP sent to the email {user.Email}"
        };
    }
}