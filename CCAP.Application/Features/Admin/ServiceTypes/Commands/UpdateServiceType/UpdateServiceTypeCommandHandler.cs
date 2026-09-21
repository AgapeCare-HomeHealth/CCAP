using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Admin.ServiceTypes.Commands.UpdateServiceType;
public sealed class UpdateServiceTypeCommandHandler : IRequestHandler<UpdateServiceTypeCommand>
{
    private readonly IServiceTypeRepository _repository; private readonly IUnitOfWork _unitOfWork;
    public UpdateServiceTypeCommandHandler(IServiceTypeRepository repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }
    public async Task Handle(UpdateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.ServiceTypeId, cancellationToken) ?? throw new KeyNotFoundException("Service type not found.");
        var code = request.Code.Trim();
        if (await _repository.ExistsAsync(code, entity.ServiceTypeId, cancellationToken)) throw new InvalidOperationException("A service type with this code already exists.");
        entity.Update(code, request.Name, request.Icon, request.CssClass, request.IsActive);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
