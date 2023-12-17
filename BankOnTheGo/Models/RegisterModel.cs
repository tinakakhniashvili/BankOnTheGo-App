using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.Models
{
    public class RegisterModel
    {
        public RegisterModel(string firstName, string lastName, int iD_Number, string password, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            ID_Number = iD_Number;
            Email = email;
            
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ID_Number { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}