using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Repositories;
using System.Threading.Tasks;

namespace ECommerce.Application.Helpers
{
    public class UserValidationHelper(IUserRepository userRepository)
    {
        public async Task EnsureEmailIsUniqueAsync(string email)
        {
            if (await userRepository.EmailExistsAsync(email))
                throw new ConflictException("Email is already registered");
        }

        public async Task EnsureUsernameIsUniqueAsync(string username)
        {
            if (await userRepository.UsernameExistsAsync(username))
                throw new ConflictException("Username is already taken");
        }

        public async Task EnsureEmailAndUsernameAreUniqueAsync(string email, string username)
        {
            await EnsureEmailIsUniqueAsync(email);
            await EnsureUsernameIsUniqueAsync(username);
        }
    }
}