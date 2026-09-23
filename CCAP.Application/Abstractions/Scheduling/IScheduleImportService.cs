using CCAP.Application.Features.Scheduling.Import;

namespace CCAP.Application.Abstractions.Scheduling;

public interface IScheduleImportService
{
    Task<IReadOnlyList<ScheduleImportPreviewItem>> PreviewAsync(Guid userId, Stream fileStream, string fileName, CancellationToken cancellationToken);
    Task<ScheduleImportResult> CommitAsync(Guid userId, IReadOnlyList<ScheduleImportCommitItem> items, CancellationToken cancellationToken);
}

public sealed class ScheduleImportResult
{
    public int Added { get; init; }
    public int Overwritten { get; init; }
    public int Kept { get; init; }
    public int Skipped { get; init; }
    public int Warnings { get; init; }
}
