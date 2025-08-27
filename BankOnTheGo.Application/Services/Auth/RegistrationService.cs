using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.SignUp;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.Application.Services.Auth
{
    public sealed class RegistrationService : IRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegistrationService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ServiceResult<string>> RegisterAsync(RegisterUser registerUser, string role)
        {
            if (await UserExists(registerUser.Email))
                return ServiceResult<string>.Fail("User already exists.");

            if (string.IsNullOrWhiteSpace(role))
                return ServiceResult<string>.Fail("Role is required.");

            if (!await _roleManager.RoleExistsAsync(role))
            {
                var newRole = new IdentityRole(role);
                var roleResult = await _roleManager.CreateAsync(newRole);
                if (!roleResult.Succeeded)
                    return ServiceResult<string>.Fail("Failed to create role: " +
                        string.Join("; ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
            }

            var user = new ApplicationUser
            {
                Email           = registerUser.Email,
                UserName        = registerUser.Username,
                SecurityStamp   = Guid.NewGuid().ToString(),
                TwoFactorEnabled= false,
                EmailConfirmed  = false
            };

            var create = await _userManager.CreateAsync(user, registerUser.Password);
            if (!create.Succeeded)
                return ServiceResult<string>.Fail("User creation failed: " +
                    string.Join("; ", create.Errors.Select(e => $"{e.Code}: {e.Description}")));

            var addRole = await _userManager.AddToRoleAsync(user, role);
            if (!addRole.Succeeded)
                return ServiceResult<string>.Fail("Failed to add role: " +
                    string.Join("; ", addRole.Errors.Select(e => $"{e.Code}: {e.Description}")));

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return ServiceResult<string>.Ok(token);
        }

        public async Task<ServiceResult<ConfirmEmailResponse>> ConfirmEmailAsync(string token, string email)
        {
            var user = await GetUserByEmail(email);
            if (user == null)
                return ServiceResult<ConfirmEmailResponse>.Fail("User does not exist.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded
                ? ServiceResult<ConfirmEmailResponse>.Ok(new ConfirmEmailResponse(user.Email!, true))
                : ServiceResult<ConfirmEmailResponse>.Fail("Email confirmation failed: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        private async Task<bool> UserExists(string email) =>
            await _userManager.FindByEmailAsync(email) is not null;

        private async Task<ApplicationUser?> GetUserByEmail(string email) =>
            await _userManager.FindByEmailAsync(email);
    }
}