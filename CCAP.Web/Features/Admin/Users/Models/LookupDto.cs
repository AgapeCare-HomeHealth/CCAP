namespace CCAP.Web.Features.Admin.Users.Models;

public sealed class LookupDto
{
    public Guid DisciplineId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid Id => DisciplineId;
}
