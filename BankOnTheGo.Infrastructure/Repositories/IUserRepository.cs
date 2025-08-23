using BankOnTheGo.API.Models;

namespace BankOnTheGo.Infrastructure.Repositories;

    public interface IUserRepository
    {
        bool CreateUser(UserModel userRegisterData);
        UserModel FindUserById(int userId);
        bool Save();
        bool UserIDExists(string userID);
        bool UserEmailExists(string email);
        UserModel FindUserByEmail(string email);
        bool VerifyPassword(string email, string password);
    }

