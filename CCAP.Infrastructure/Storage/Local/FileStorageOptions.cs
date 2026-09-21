namespace CCAP.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>MetadataOnly, Local, or AzureBlob.</summary>
    public string Provider { get; set; } = "MetadataOnly";

    public string LocalRootPath { get; set; } = "App_Data/Files";
}
