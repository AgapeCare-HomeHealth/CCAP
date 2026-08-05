using CCAP.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace CCAP.Data.Security
{
    public class PasswordHasherService
    {
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public PasswordHasherService()
        {
            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public string HashPassword(ApplicationUser user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(ApplicationUser user, string hashedPassword, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                hashedPassword,
                password);

            return result == PasswordVerificationResult.Success
                || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
