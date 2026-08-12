using CCAP.API.Authorization;
using CCAP.Application.Features.Users.Commands.ActivateUser;
using CCAP.Application.Features.Users.Commands.CreateUser;
using CCAP.Application.Features.Users.Commands.DeactivateUser;
using CCAP.Application.Features.Users.Commands.DeleteUser;
using CCAP.Application.Features.Users.Commands.UpdateUser;
using CCAP.Application.Features.Users.Queries.GetUserById;
using CCAP.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;
    public UsersController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = PermissionPolicies.UsersView)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetUsersQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.UsersView)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.UsersManage)]
    public async Task<IActionResult> CreateUser(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = result.UserId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.UsersManage)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.UserId)
            return BadRequest("Route ID and command UserId do not match.");

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.UsersManage)]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Policy = PermissionPolicies.UsersManage)]
    public async Task<IActionResult> ActivateUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = PermissionPolicies.UsersManage)]
    public async Task<IActionResult> DeactivateUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateUserCommand(id), cancellationToken);
        return NoContent();
    }
}
