using CCAP.Application.Abstractions.Scheduling;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Import;

public sealed record PreviewCalendarImportCommand(Guid UploadedByUserId, Guid CareCoordinatorId, string FileName, byte[] FileBytes)
    : IRequest<CalendarImportBatchDto>;
public sealed record QueueCalendarImportCommand(Guid BatchId) : IRequest<CalendarImportBatchDto>;
public sealed record GetCalendarImportQuery(Guid BatchId) : IRequest<CalendarImportBatchDto?>;
public sealed record GetCalendarImportHistoryQuery : IRequest<IReadOnlyList<CalendarImportBatchDto>>;
public sealed record GetCalendarImportLogsQuery(Guid BatchId) : IRequest<IReadOnlyList<CalendarImportRowDto>>;
public sealed record DownloadCalendarImportErrorsQuery(Guid BatchId) : IRequest<CalendarImportErrorFile?>;

public sealed class PreviewCalendarImportHandler(ICalendarImportApplicationService service)
    : IRequestHandler<PreviewCalendarImportCommand, CalendarImportBatchDto>
{
    public Task<CalendarImportBatchDto> Handle(PreviewCalendarImportCommand request, CancellationToken cancellationToken) =>
        service.PreviewAsync(request.UploadedByUserId, request.CareCoordinatorId, request.FileName, request.FileBytes, cancellationToken);
}

public sealed class QueueCalendarImportHandler(ICalendarImportApplicationService service)
    : IRequestHandler<QueueCalendarImportCommand, CalendarImportBatchDto>
{
    public Task<CalendarImportBatchDto> Handle(QueueCalendarImportCommand request, CancellationToken cancellationToken) =>
        service.QueueAsync(request.BatchId, cancellationToken);
}

public sealed class GetCalendarImportHandler(ICalendarImportApplicationService service)
    : IRequestHandler<GetCalendarImportQuery, CalendarImportBatchDto?>
{
    public Task<CalendarImportBatchDto?> Handle(GetCalendarImportQuery request, CancellationToken cancellationToken) =>
        service.GetAsync(request.BatchId, cancellationToken);
}

public sealed class GetCalendarImportHistoryHandler(ICalendarImportApplicationService service)
    : IRequestHandler<GetCalendarImportHistoryQuery, IReadOnlyList<CalendarImportBatchDto>>
{
    public Task<IReadOnlyList<CalendarImportBatchDto>> Handle(GetCalendarImportHistoryQuery request, CancellationToken cancellationToken) =>
        service.HistoryAsync(cancellationToken);
}

public sealed class GetCalendarImportLogsHandler(ICalendarImportApplicationService service)
    : IRequestHandler<GetCalendarImportLogsQuery, IReadOnlyList<CalendarImportRowDto>>
{
    public Task<IReadOnlyList<CalendarImportRowDto>> Handle(GetCalendarImportLogsQuery request, CancellationToken cancellationToken) =>
        service.LogsAsync(request.BatchId, cancellationToken);
}

public sealed class DownloadCalendarImportErrorsHandler(ICalendarImportApplicationService service)
    : IRequestHandler<DownloadCalendarImportErrorsQuery, CalendarImportErrorFile?>
{
    public Task<CalendarImportErrorFile?> Handle(DownloadCalendarImportErrorsQuery request, CancellationToken cancellationToken) =>
        service.DownloadErrorsAsync(request.BatchId, cancellationToken);
}
