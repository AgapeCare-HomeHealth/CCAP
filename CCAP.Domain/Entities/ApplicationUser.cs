namespace CCAP.Domain.Entities;

public sealed class ApplicationUser
{
    private ApplicationUser() { }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid? DisciplineId { get; private set; }
    public string EmployeeNo { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? MobileNo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Role Role { get; private set; } = null!;
    public Discipline? Discipline { get; private set; }

    public ApplicationUser(
        string employeeNo,
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        Guid roleId,
        Guid? disciplineId,
        string? mobileNo = null)
    {
        UserId = Guid.NewGuid();
        EmployeeNo = employeeNo.Trim();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        RoleId = roleId;
        DisciplineId = disciplineId;
        MobileNo = mobileNo;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetPasswordHash(string hash) => PasswordHash = hash;
    public void Update(
        string employeeNo,
        string firstName,
        string lastName,
        string email,
        string? mobileNo,
        Guid roleId,
        Guid? disciplineId)
    {
        EmployeeNo = employeeNo.Trim();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        MobileNo = mobileNo;
        RoleId = roleId;
        DisciplineId = disciplineId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
