using MediatR;
using CCAP.Application.Features.Authentication.DTOs;

namespace CCAP.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResultDto>;
