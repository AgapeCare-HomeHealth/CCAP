namespace CCAP.Infrastructure.Storage.AzureBlob;

public sealed class AzureBlobOptions
{
    public const string SectionName = "AzureBlob";

    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "ccap-files";
    public string RootFolder { get; set; } = "referrals";
}
