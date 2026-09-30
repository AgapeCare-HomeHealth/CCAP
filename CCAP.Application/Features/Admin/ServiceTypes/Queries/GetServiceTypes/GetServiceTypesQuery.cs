using MediatR;
using CCAP.Application.Features.Admin.ServiceTypes;
namespace CCAP.Application.Features.Admin.ServiceTypes.Queries.GetServiceTypes;
public sealed record GetServiceTypesQuery(bool ActiveOnly = false) : IRequest<List<ServiceTypeAdminDto>>;
