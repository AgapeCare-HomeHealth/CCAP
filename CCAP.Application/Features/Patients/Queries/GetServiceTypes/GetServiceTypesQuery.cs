using MediatR;
using CCAP.Application.Features.Patients.DTOs;

namespace CCAP.Application.Features.Patients.Queries.GetServiceTypes;

public sealed record GetServiceTypesQuery : IRequest<List<ServiceTypeDto>>;
