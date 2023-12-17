using BankOnTheGo.Models;

namespace BankOnTheGo.IRepository
{
    public interface IUserRepository
    {
        public bool CreateUser(RegisterModel userRegisterData);
        public UserModel FindUserById(int userId);
        public bool Save();
        public bool UserIDExists(int userID);
        public bool UserEmailExists(string email);
    }
}
