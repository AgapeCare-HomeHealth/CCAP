using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Identity;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    DateTime GetExpirationUtc();
}
