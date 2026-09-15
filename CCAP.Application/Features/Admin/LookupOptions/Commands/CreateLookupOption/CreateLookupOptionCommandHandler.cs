using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Admin.LookupOptions.Commands.CreateLookupOption;

public sealed class CreateLookupOptionCommandHandler : IRequestHandler<CreateLookupOptionCommand, LookupOptionDto>
{
    private readonly ILookupOptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    public CreateLookupOptionCommandHandler(ILookupOptionRepository repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }

    public async Task<LookupOptionDto> Handle(CreateLookupOptionCommand request, CancellationToken cancellationToken)
    {
        var requestedType = request.LookupType.Trim();
        var type = await _repository.GetCanonicalLookupTypeAsync(requestedType, cancellationToken) ?? requestedType;
        var code = request.Code.Trim();
        var name = request.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Lookup type, code, and display name are required.");
        if (await _repository.ExistsAsync(type, code, null, cancellationToken)) throw new InvalidOperationException("An option with this code already exists in this lookup.");
        var entity = new LookupOption(type, code, name, request.SortOrder, request.IsActive);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new(entity.LookupOptionId, entity.LookupType, entity.Code, entity.DisplayName, entity.SortOrder, entity.IsActive);
    }
}
