using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Admin.LookupOptions.Commands.UpdateLookupOption;

public sealed class UpdateLookupOptionCommandHandler : IRequestHandler<UpdateLookupOptionCommand>
{
    private readonly ILookupOptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateLookupOptionCommandHandler(ILookupOptionRepository repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }
    public async Task Handle(UpdateLookupOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.LookupOptionId, cancellationToken) ?? throw new KeyNotFoundException("Lookup option not found.");
        var code = request.Code.Trim();
        if (await _repository.ExistsAsync(entity.LookupType, code, entity.LookupOptionId, cancellationToken)) throw new InvalidOperationException("An option with this code already exists in this lookup.");
        entity.Update(code, request.DisplayName, request.SortOrder, request.IsActive);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
