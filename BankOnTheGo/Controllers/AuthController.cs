using BankOnTheGo.Data;
using BankOnTheGo.Dto;
using BankOnTheGo.Dto.Repository;
using BankOnTheGo.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.Controllers
{
    [Route("api[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly DataContext _context;

        public AuthController(UserRepository userRepository, DataContext context)
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

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult Register([FromBody] RegisterDto register)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = _userRepository.CreateUser(register);

            if (user == null)
            {
                return BadRequest("Failed to register user");
            }


            return Ok("Successfully registered");
        }
    }
}
