using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.Models
{
    public class UserModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ID_Number { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [Key]
        public int Id { get; set; }
    }
}
