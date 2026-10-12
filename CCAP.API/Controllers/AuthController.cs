using CCAP.Application.Features.Authentication.Commands.Login;
using CCAP.API.Contracts.Authentication;
using CCAP.Application.Abstractions.Identity;
using CCAP.Application.Abstractions.Persistence;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender; private readonly IUserRepository _users; private readonly IRoleRepository _roles; private readonly IJwtService _jwt;
    public AuthController(ISender sender, IUserRepository users, IRoleRepository roles, IJwtService jwt) { _sender=sender; _users=users; _roles=roles; _jwt=jwt; }

    [Authorize(Roles = "Administrator")]
    [HttpPost("switch-role")]
    public async Task<IActionResult> SwitchRole(SwitchRoleRequest request, CancellationToken cancellationToken)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var userId)) return Unauthorized();
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive) return Unauthorized();
        var role = (await _roles.GetAllAsync(cancellationToken)).FirstOrDefault(x => string.Equals(x.RoleName, request.RoleName, StringComparison.OrdinalIgnoreCase));
        if (role is null) return NotFound(new { message = "Role not found." });
        var token = _jwt.GenerateToken(user, role);
        return Ok(new { success = true, token, expiration = _jwt.GetExpirationUtc(), role = role.RoleName });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
