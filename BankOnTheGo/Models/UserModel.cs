using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ID_Number { get; set; }
        public string HashedPassword { get; set; }
        public string MyProperty { get; set; }
    }
}