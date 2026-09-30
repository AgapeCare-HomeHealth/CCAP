namespace CCAP.Domain.Entities;

public sealed class Location
{
    private Location()
    {
    }

    public Guid LocationId { get; private set; }

    public string Name { get; private set; }
        = string.Empty;

    public bool IsActive { get; private set; }

    public bool IsDefault { get; private set; }

    public Location(
        string name,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Location name is required.",
                nameof(name));

        LocationId = Guid.NewGuid();

        Name = name.Trim();

        IsActive = true;

        IsDefault = isDefault;
    }

    public void SetDefault(bool value)
    {
        IsDefault = value;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}