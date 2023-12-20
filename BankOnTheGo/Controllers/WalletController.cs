using BankOnTheGo.IRepository;
using BankOnTheGo.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.Controllers
{
    [Route("api[controller]")]
    [ApiController]
    public class WalletController : Controller
    {
        private readonly IWalletRepository _walletRepository;
        public WalletController(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }


        [HttpGet("{walletId}")]
        [ProducesResponseType(200, Type = typeof(WalletModel))]
        [ProducesResponseType(400)]
        public IActionResult GetUserByWalletId(int walletId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_walletRepository.WalletExists(walletId))
            {
                return NotFound();
            }
            else
            {
                _walletRepository.GetByUserId(walletId);
            }

            return Ok();
        }

    }
}
