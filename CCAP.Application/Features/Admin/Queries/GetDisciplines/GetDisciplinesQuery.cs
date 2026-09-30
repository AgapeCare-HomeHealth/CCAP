using MediatR;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetDisciplines;

public sealed record GetDisciplinesQuery : IRequest<List<DisciplineDto>>;
