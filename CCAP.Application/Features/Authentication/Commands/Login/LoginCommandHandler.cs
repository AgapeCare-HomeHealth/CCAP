using MediatR;
using CCAP.Application.Abstractions.Identity;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Authentication.DTOs;

namespace CCAP.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwt;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtService jwt)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive ||
            !_passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            return new LoginResultDto(false, "Invalid email or password.", null, null, null, null, null);
        }

        var token = _jwt.GenerateToken(user);

        return new LoginResultDto(
            true,
            "Login successful.",
            token,
            _jwt.GetExpirationUtc(),
            user.UserId,
            $"{user.FirstName} {user.LastName}",
            user.Role.RoleName);
    }
}
