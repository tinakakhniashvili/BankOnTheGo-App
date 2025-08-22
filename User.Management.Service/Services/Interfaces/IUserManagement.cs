using Microsoft.AspNetCore.Identity;
using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.Login;
using User.Management.Service.Models.Authentication.SignUp;
using User.Management.Service.Models.Authentication.User;

namespace User.Management.Service.Services.Interfaces;

public interface IUserManagement
{
    Task<ApiResponse<object>> LoginAsync(LoginModel loginModel);
    Task<ApiResponse<object>> LoginWithOtpAsync(string username, string code);
    Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser);
    Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, IdentityUser user);
    Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsyn(LoginModel loginModel);
}