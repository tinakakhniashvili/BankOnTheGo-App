using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BankOnTheGo.Dto;
using BankOnTheGo.IRepository;
using BankOnTheGo.Helper;
using BankOnTheGo.Models;

namespace BankOnTheGo.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public RegisterDto Input { get; set; }

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterModel(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = new UserModel
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                ID_Number = Input.ID_Number,
                Email = Input.Email,
                HashedPassword = _passwordHasher.Hash(Input.Password) // <-- hash here
            };

            _userRepository.CreateUser(user);

            return RedirectToPage("/Auth/Login");
        }

    }
}