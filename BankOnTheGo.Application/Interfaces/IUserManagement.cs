using BankOnTheGo.Domain.Authentication.User;
using Microsoft.AspNetCore.Identity;
using BankOnTheGo.Service.Models.Authentication.Login;
using BankOnTheGo.Service.Models.Authentication.SignUp;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Interfaces;

public interface IUserManagement
{
    Task<ApiResponse<object>> LoginAsync(LoginModel loginModel);
    Task<ApiResponse<object>> LoginWithOtpAsync(string username, string code);
    Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser);
    Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, IdentityUser user);
    Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsyn(LoginModel loginModel);
}