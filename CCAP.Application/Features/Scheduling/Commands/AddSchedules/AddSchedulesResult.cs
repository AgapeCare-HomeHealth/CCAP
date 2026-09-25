namespace CCAP.Application.Features.Scheduling.Commands.AddSchedules;

public sealed class AddSchedulesResult
{
    public int Added { get; init; }
    public int SkippedDuplicates { get; init; }
    public IReadOnlyList<Guid> VisitIds { get; init; } = [];
}
