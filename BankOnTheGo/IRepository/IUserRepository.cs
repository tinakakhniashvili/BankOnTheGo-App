﻿using BankOnTheGo.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.IRepository
{
    public interface IUserRepository
    {
        bool CreateUser(UserModel userRegisterData);
        UserModel FindUserById(int userId);
        bool Save();
        bool UserIDExists(int userID);
        bool UserEmailExists(string email);
        UserModel FindUserByEmail(string email);
        bool VerifyPassword(string email, string password);
    }
}
