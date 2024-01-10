using System;
using BankOnTheGo.Data;
using BankOnTheGo.IRepository;

namespace BankOnTheGo.Repository
{
    public class TemporaryCodesRepository : ITemporaryCodesRepository
    {
        private readonly DataContext _context;
        public TemporaryCodesRepository(DataContext context)
        {
            _context = context;
        }

        public int GetTemporaryCode(string email)
        {
            return _context.TemporaryCodes.Where(t => t.Email == email).FirstOrDefault().Code;
        }

        public void ResetPassword(string email, string password)
        {
            _context.TemporaryCodes.Where(t => t.Email == email).FirstOrDefault().NewPassword=password;
        }
    }
}

