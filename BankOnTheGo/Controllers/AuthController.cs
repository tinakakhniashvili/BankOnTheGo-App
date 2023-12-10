using BankOnTheGo.Data;
using BankOnTheGo.Dto;
using BankOnTheGo.IRepository;
using BankOnTheGo.Repository;
using BankOnTheGo.Models;
using Microsoft.AspNetCore.Mvc;
using BankOnTheGo.Helper;

namespace BankOnTheGo.Controllers
{
    [Route("api[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly DataContext _context;
        private readonly PasswordHasher _passwordHasher;

        public AuthController(IUserRepository userRepository, DataContext context, PasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("/Auth/Login/")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult Login([FromBody] LoginDto login)
        {
            if (!_userRepository.UserEmailExists(login.Email))
            {
                return BadRequest("The email  doesn't match any existing accounts");
            }
            else
            {
                if(!_userRepository.VerifyPassword(login.Email, login.Password))
                {
                    return BadRequest("Incorrect password");
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("/Auth/Register/")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult Register([FromBody] RegisterDto register)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var passwordHash = _passwordHasher.Hash(register.Password);
            UserModel userModel = new UserModel {
                FirstName=register.FirstName,
                LastName=register.LastName,
                ID_Number=register.ID_Number,
                HashedPassword= passwordHash,
                Email=register.Email };

            if (_userRepository.UserIDExists(register.ID_Number))
            {
                return BadRequest("The ID you selected is already in use");
            }

            if (_userRepository.UserEmailExists(register.Email))
            {
                return BadRequest("The email you selected is already in use");
            }

            var user = _userRepository.CreateUser(userModel);

            if (user==null)
            {
                return BadRequest("Failed to register user");
            }

            
            return Ok("Successfully registered");
        }
    }
}
