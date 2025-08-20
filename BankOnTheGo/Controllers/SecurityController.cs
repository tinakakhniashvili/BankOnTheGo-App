using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using BankOnTheGo.IRepository;

namespace BankOnTheGo.Controllers
{
    [Route("api[controller]")]
    [ApiController]
    public class SecurityController : Controller
	{
        private readonly ITemporaryCodesRepository _temporaryCodesRepository;
        public SecurityController(ITemporaryCodesRepository temporaryCodesRepository)
        {
            _temporaryCodesRepository = temporaryCodesRepository;
        }
        [HttpPost("/Security/RequestPasswordChange/")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult RequestPasswordChange()
        {
         DotNetEnv.Env.Load();

        Random random = new Random();
        int code = random.Next(10000, 99999);


        var mailFrom = Environment.GetEnvironmentVariable("MAIL_FROM");
        var pass = Environment.GetEnvironmentVariable("MAIL_FROM_PASS");
            
        MailMessage message = new MailMessage();
        message.From = new MailAddress(mailFrom);
        message.Subject = "Test Subject";
        message.To.Add(new MailAddress("nika.nabakhteveli1@gmail.com"));
        message.Body = "<html><body> THIS IS BODY </body></html>";
        message.IsBodyHtml = true;

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
          Port = 587,
          Credentials = new NetworkCredential(mailFrom, pass),
          EnableSsl = true,
        };

         smtpClient.Send(message);

         return Ok();
        }

        [HttpPost("/Security/ResetPassword/")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult ResetPassword(int code, string email, string newPassword)
        {
            int tempCode = _temporaryCodesRepository.GetTemporaryCode(email);
            if (code != tempCode)
            {
                    return BadRequest();
            }
            else
            {
                _temporaryCodesRepository.ResetPassword(email, newPassword);
            }

            return Ok();
        }
    }
}

