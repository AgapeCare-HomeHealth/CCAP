using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CCAP.Application.Abstractions.Identity;
using CCAP.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CCAP.Infrastructure.Identity;

public sealed class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> options) => _settings = options.Value;

    public DateTime GetExpirationUtc() =>
        DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes);

    public string GenerateToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.RoleName)
        }.Concat(
            user.Role.RolePermissions
                .Where(x => x.Permission is not null)
                .Select(x => new Claim("permission", x.Permission.PermissionCode))
        ).ToArray();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: GetExpirationUtc(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
