using CCAP.Application.Features.Scheduling.Import;

namespace CCAP.Application.Abstractions.Scheduling;

public interface ICalendarImportApplicationService
{
    Task<CalendarImportBatchDto> PreviewAsync(Guid uploadedByUserId, Guid careCoordinatorId, string fileName, byte[] fileBytes, CancellationToken cancellationToken);
    Task<CalendarImportBatchDto> QueueAsync(Guid batchId, CancellationToken cancellationToken);
    Task<CalendarImportBatchDto?> GetAsync(Guid batchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CalendarImportBatchDto>> HistoryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CalendarImportRowDto>> LogsAsync(Guid batchId, CancellationToken cancellationToken);
    Task<CalendarImportErrorFile?> DownloadErrorsAsync(Guid batchId, CancellationToken cancellationToken);
}

public interface ICalendarImportProcessor
{
    Task<bool> ProcessNextBatchAsync(CancellationToken cancellationToken);
}
