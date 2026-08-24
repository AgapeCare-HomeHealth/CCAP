using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryValidator
    : IRequestValidator<GetUserByIdQuery>
{
    public void Validate(
        GetUserByIdQuery request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.");
        }
    }
}