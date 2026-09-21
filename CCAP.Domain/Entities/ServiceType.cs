namespace CCAP.Domain.Entities;

public sealed class ServiceType
{
    private ServiceType() { }

    public static ServiceType Create(string code, string name, string icon, string cssClass)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        return new ServiceType
        {
            ServiceTypeId = Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            Icon = icon?.Trim() ?? string.Empty,
            CssClass = cssClass?.Trim() ?? string.Empty,
            IsActive = true
        };
    }

    public Guid ServiceTypeId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Icon { get; private set; } = string.Empty;
    public string CssClass { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public ICollection<PatientServiceOrder> PatientServiceOrders { get; private set; } = new List<PatientServiceOrder>();

    public void Update(string code, string name, string icon, string cssClass, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Code = code.Trim();
        Name = name.Trim();
        Icon = icon?.Trim() ?? string.Empty;
        CssClass = cssClass?.Trim() ?? string.Empty;
        IsActive = isActive;
    }
}
