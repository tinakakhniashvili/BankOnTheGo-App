using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.API.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string ID_Number { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string HashedPassword { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public string EmailConfirmationToken { get; set; }
    }
}