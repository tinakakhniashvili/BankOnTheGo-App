using BankOnTheGo.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.IRepository
{
    public interface IUserRepository
    {
        public bool CreateUser(RegisterModel userRegisterData);
        public UserModel FindUserById(int userId);
        public bool Save();
        public bool UserIDExists(int userID);
        public bool UserEmailExists(string email);
        public UserModel FindUserByEmail(string email);
        public bool VerifyPassword(string email, string password);
    }
}
