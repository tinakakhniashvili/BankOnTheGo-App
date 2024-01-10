using BankOnTheGo.Data;
using BankOnTheGo.Dto;
using BankOnTheGo.IRepository;
using BankOnTheGo.Models;
using Microsoft.AspNetCore.Mvc;
using BankOnTheGo.Helper;
using MimeKit;
using MimeKit.Text;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace BankOnTheGo.Controllers
{
    [Route("api[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IWalletRepository _walletRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IUserRepository userRepository,  IPasswordHasher passwordHasher,
                              IWalletRepository walletRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _walletRepository = walletRepository;
            _configuration = configuration;
        }

        [HttpPost("/Auth/Login/")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult Login([FromBody] LoginDto login)
        {
            LoginRequestResponse response = new LoginRequestResponse();

            if (!ModelState.IsValid)
            {
                return BadRequest(response);
            }

            var isEmailValid = _userRepository.UserEmailExists(login.Email);


            if (!isEmailValid)
            {
                response.Message = "The email doesn't match any existing accounts";
                return BadRequest(response);
            }
            else
            {
                if(!_userRepository.VerifyPassword(login.Email, login.Password))
                {
                    response.Message = "Incorrect password";
                }
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
