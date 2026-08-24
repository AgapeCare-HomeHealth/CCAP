namespace CCAP.Application.Common.Validation;

public sealed class RequestValidationException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    public RequestValidationException(
        IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToArray();
    }
}