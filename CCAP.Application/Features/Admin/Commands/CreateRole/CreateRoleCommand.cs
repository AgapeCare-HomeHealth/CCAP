using MediatR;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Commands.CreateRole;

public sealed record CreateRoleCommand(string RoleName, string? Description, bool IsActive) : IRequest<RoleListItemDto>;
