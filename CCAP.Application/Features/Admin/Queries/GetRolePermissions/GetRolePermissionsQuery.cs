using MediatR;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetRolePermissions;

public sealed record GetRolePermissionsQuery(Guid RoleId) : IRequest<RoleDetailsDto>;
