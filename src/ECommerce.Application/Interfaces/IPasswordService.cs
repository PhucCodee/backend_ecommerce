namespace ECommerce.Application.Interfaces
{
    public interface IPasswordService
    {
        (string hash, string salt) HashPassword(string password);
        bool VerifyPassword(string password, string hash, string salt);
        string HashToken(string token);
    }
}