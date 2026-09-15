namespace CCAP.Domain.Entities;

public sealed class LookupOption
{
    private LookupOption() { }

    public Guid LookupOptionId { get; private set; }
    public string LookupType { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    public LookupOption(string lookupType, string code, string displayName, int sortOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(lookupType)) throw new ArgumentException("Lookup type is required.", nameof(lookupType));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));

        LookupOptionId = Guid.NewGuid();
        LookupType = lookupType.Trim();
        Code = code.Trim();
        DisplayName = displayName.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    public void Update(string code, string displayName, int sortOrder, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        Code = code.Trim();
        DisplayName = displayName.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}
