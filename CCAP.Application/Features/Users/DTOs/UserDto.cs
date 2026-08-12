namespace CCAP.Application.Features.Users.DTOs;

public sealed record UserDto(
    Guid UserId,
    string EmployeeNo,
    string FirstName,
    string LastName,
    string Email,
    string? MobileNo,
    bool IsActive,
    Guid RoleId,
    Guid? DisciplineId,
    string Role,
    string Discipline,
    DateTime? LastLoginAt);
