using MediatR;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<List<RoleListItemDto>>;
