using BankOnTheGo.Data;
using BankOnTheGo.Dto;
using BankOnTheGo.IRepository;
using BankOnTheGo.Repository;
using BankOnTheGo.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.Controllers
{
    [Route("api[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly DataContext _context;

        public AuthController(IUserRepository userRepository, DataContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult Login([FromBody] LoginDto login)
        {

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

            RegisterModel registerModel = new RegisterModel(register.FirstName, register.LastName, register.ID_Number, register.Password);

            var user = _userRepository.CreateUser(registerModel);

            if (!user)
            {
                return BadRequest("Failed to register user");
            }


            return Ok("Successfully registered");
        }
    }
}
