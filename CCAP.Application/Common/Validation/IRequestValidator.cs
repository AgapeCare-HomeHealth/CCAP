namespace CCAP.Application.Common.Validation;

public interface IRequestValidator<in TRequest>
{
    void Validate(TRequest request);
}