using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Admin.LookupOptions.Commands.DeleteLookupOption;
public sealed class DeleteLookupOptionCommandHandler : IRequestHandler<DeleteLookupOptionCommand>
{
    private readonly ILookupOptionRepository _repository; private readonly IUnitOfWork _unitOfWork;
    public DeleteLookupOptionCommandHandler(ILookupOptionRepository repository, IUnitOfWork unitOfWork) { _repository=repository; _unitOfWork=unitOfWork; }
    public async Task Handle(DeleteLookupOptionCommand request, CancellationToken cancellationToken)
    {
        var entity=await _repository.GetByIdAsync(request.LookupOptionId,cancellationToken) ?? throw new KeyNotFoundException("Lookup option not found.");
        _repository.Remove(entity); await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
