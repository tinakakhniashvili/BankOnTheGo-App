using BankOnTheGo.Data;
using BankOnTheGo.Models;

namespace BankOnTheGo.Repository
{
    public class UserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }
        public void CreateUser(RegisterModel userRegisterData)
        {
            var userData = new UserModel
            {
                FirstName = userRegisterData.FirstName,
                LastName = userRegisterData.LastName,
                ID_Number = userRegisterData.ID_Number,
                HashedPassword = userRegisterData.Password
            };

            _context.Add(userData);
        }

        public UserModel FindUserById(int userId)
        {
            return _context.Users.Where(u => u.Id == userId).FirstOrDefault();
        }
    }
}
