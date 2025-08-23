namespace BankOnTheGo.Infrastructure.Repositories;

	public interface ITemporaryCodesRepository
	{
        int GetTemporaryCode(string email);
        void ResetPassword(string email, string password);
    }


