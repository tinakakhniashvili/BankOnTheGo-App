using BankOnTheGo.Data;
using BankOnTheGo.Helper;
using BankOnTheGo.IRepository;
using BankOnTheGo.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher _passwordHasher;
        public UserRepository(DataContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        public bool CreateUser(UserModel userRegisterData)
        {
            _context.Add(userRegisterData);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public UserModel FindUserById(int userId)
        {
            return _context.Users.Where(u => u.Id == userId).FirstOrDefault();
        }

        public bool UserIDExists(int inputUserID)
        {
            bool userIdExists = _context.Users.Any(u => u.ID_Number == inputUserID);
            return userIdExists;
        }

        public bool UserEmailExists(string email)
        {
            bool userEmailExists = _context.Users.Any(u => u.Email.Equals(email));
            return userEmailExists;
        }

        public UserModel FindUserByEmail(string email)
        {
            return _context.Users.Where(u => u.Email.Equals(email)).FirstOrDefault();
        }
        public bool VerifyPassword(string email, string password)
        {
            string hashedPassword = _passwordHasher.Hash(password);
            if(FindUserByEmail(email).HashedPassword == hashedPassword)
            {
                return true;
            }
            return false;
        }

    }
}
