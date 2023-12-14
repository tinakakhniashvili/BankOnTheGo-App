using BankOnTheGo.Data;
using BankOnTheGo.Helper;
using BankOnTheGo.Models;

namespace BankOnTheGo.Dto.Repository
{
    public class UserRepository
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher _passwordHasher;
        public UserRepository(DataContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        public void CreateUser(RegisterModel userRegisterData)
        {
            var passwordHash = _passwordHasher.Hash(userRegisterData.Password);
            var userData = new UserModel
            {
                FirstName = userRegisterData.FirstName,
                LastName = userRegisterData.LastName,
                ID_Number = userRegisterData.ID_Number,
                HashedPassword = passwordHash
            };

            _context.Add(userData);
        }

        public UserModel FindUserById(int userId)
        {
            return _context.Users.Where(u => u.Id == userId).FirstOrDefault();
        }
    }
}
