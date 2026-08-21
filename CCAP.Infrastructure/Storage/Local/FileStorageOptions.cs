namespace CCAP.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Provider { get; set; } = "Local";

    public string LocalRootPath { get; set; } = "App_Data/Files";
}