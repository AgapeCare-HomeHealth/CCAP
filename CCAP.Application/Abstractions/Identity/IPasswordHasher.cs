using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Identity;

public interface IPasswordHasher
{
    string HashPassword(ApplicationUser user, string password);
    bool VerifyPassword(ApplicationUser user, string hashedPassword, string providedPassword);
}
