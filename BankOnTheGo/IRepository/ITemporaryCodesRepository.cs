using System;
namespace BankOnTheGo.IRepository
{
	public interface ITemporaryCodesRepository
	{
        int GetTemporaryCode(string email);
        void ResetPassword(string email, string password);
    }
}

