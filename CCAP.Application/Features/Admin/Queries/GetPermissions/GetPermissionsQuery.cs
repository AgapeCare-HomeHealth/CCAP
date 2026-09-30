using MediatR;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetPermissions;

public sealed record GetPermissionsQuery : IRequest<List<PermissionDto>>;
