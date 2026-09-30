namespace CCAP.Domain.Entities;

public sealed class Discipline
{
    private Discipline() { }

    public Discipline(string code, string name, string? description = null)
    {
        DisciplineId = Guid.NewGuid();
        Code = code.Trim();
        Name = name.Trim();
        Description = description?.Trim();
    }

    public Guid DisciplineId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<ApplicationUser> Users { get; private set; } = new List<ApplicationUser>();
}
