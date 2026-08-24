using MediatR;

namespace CCAP.Application.Common.Validation;

public sealed class RequestValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IRequestValidator<TRequest>>
        _validators;

    public RequestValidationBehavior(
        IEnumerable<IRequestValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        foreach (var validator in _validators)
        {
            validator.Validate(request);
        }

        return await next();
    }
}