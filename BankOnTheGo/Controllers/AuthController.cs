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
        private readonly IPasswordHasher _passwordHasher;
        private readonly IWalletRepository _walletRepository;

        public AuthController(IUserRepository userRepository, DataContext context, IPasswordHasher passwordHasher, IWalletRepository walletRepository)
        {
            _userRepository = userRepository;
            _context = context;
            _passwordHasher = passwordHasher;
            _walletRepository = walletRepository;
        }

        [HttpPost("/Auth/Login/")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult Login([FromBody] LoginDto login)
        {
            LoginRequestResponse response = new LoginRequestResponse();
            response.Success = true;
            response.Message = string.Empty;

            if (!_userRepository.UserEmailExists(login.Email))
            {
                response.Success = false;
                response.Message = "The email doesn't match any existing accounts";
            }
            else
            {
                if(!_userRepository.VerifyPassword(login.Email, login.Password))
                {
                    response.Success = false;
                    response.Message = "Incorrect password";
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(response);
            }

            return Ok(response);
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

            WalletModel walletModel = new WalletModel(userModel.Id, 0, 1);

            var wallet = _walletRepository.CreateWallet(walletModel);

            if (wallet == null)
            {
                return BadRequest("Failed to register wallet");
            }

            return Ok("Successfully registered");
        }
    }
}
