namespace CCAP.Infrastructure.Storage.OneDrive;

public sealed class OneDriveOptions
{
    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string DriveId { get; set; } = string.Empty;

    public string RootFolder { get; set; } = "CCAP/Referrals";
}