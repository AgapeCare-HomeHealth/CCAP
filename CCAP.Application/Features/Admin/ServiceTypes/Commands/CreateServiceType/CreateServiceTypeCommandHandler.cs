using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;
namespace CCAP.Application.Features.Admin.ServiceTypes.Commands.CreateServiceType;
public sealed class CreateServiceTypeCommandHandler : IRequestHandler<CreateServiceTypeCommand, ServiceTypeAdminDto>
{
    private readonly IServiceTypeRepository _repository; private readonly IUnitOfWork _unitOfWork;
    public CreateServiceTypeCommandHandler(IServiceTypeRepository repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }
    public async Task<ServiceTypeAdminDto> Handle(CreateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim(); var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Code and name are required.");
        if (await _repository.ExistsAsync(code, null, cancellationToken)) throw new InvalidOperationException("A service type with this code already exists.");
        var entity = ServiceType.Create(code, name, request.Icon, request.CssClass);
        entity.Update(code, name, request.Icon, request.CssClass, request.IsActive);
        await _repository.AddAsync(entity, cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new(entity.ServiceTypeId, entity.Code, entity.Name, entity.Icon, entity.CssClass, entity.IsActive);
    }
}
