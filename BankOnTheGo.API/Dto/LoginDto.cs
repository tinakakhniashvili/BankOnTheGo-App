using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.Dto
{
    public class LoginDto
    {
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        [Key]
        public int Id { get; set; }

    }
}
