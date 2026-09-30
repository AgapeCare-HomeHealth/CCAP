using CCAP.Application.Abstractions.Identity;
using CCAP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CCAP.Infrastructure.Identity;

public sealed class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public string HashPassword(ApplicationUser user, string password) =>
        _hasher.HashPassword(user, password);

    public bool VerifyPassword(ApplicationUser user, string hashedPassword, string providedPassword) =>
        _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword)
        != PasswordVerificationResult.Failed;
}
