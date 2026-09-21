using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Admin.ServiceTypes.Commands.DeleteServiceType;
public sealed class DeleteServiceTypeCommandHandler : IRequestHandler<DeleteServiceTypeCommand>
{
    private readonly IServiceTypeRepository _repository; private readonly IUnitOfWork _unitOfWork;
    public DeleteServiceTypeCommandHandler(IServiceTypeRepository repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }
    public async Task Handle(DeleteServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.ServiceTypeId, cancellationToken) ?? throw new KeyNotFoundException("Service type not found.");
        if (await _repository.HasOrdersAsync(entity.ServiceTypeId, cancellationToken))
            throw new InvalidOperationException("This service type is already used by patient orders. Deactivate it instead of deleting it.");
        _repository.Remove(entity); await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
