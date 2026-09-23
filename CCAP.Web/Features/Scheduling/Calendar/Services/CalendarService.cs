using Microsoft.AspNetCore.Components.Forms;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Scheduling.Calendar.Models;

namespace CCAP.Web.Features.Scheduling.Calendar.Services;

public sealed class CalendarService
{
    private readonly CcapApiClient _api;

    public CalendarService(CcapApiClient api) => _api = api;

    public async Task<List<CalendarVisitDto>> GetVisitsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var url = $"api/scheduling/calendar?startDate={Uri.EscapeDataString(startDate.ToString("O"))}&endDate={Uri.EscapeDataString(endDate.ToString("O"))}";
        return await _api.GetFromJsonAsync<List<CalendarVisitDto>>(url, cancellationToken) ?? [];
    }

    public async Task<List<ScheduleImportPreviewItem>> PreviewImportAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream(10 * 1024 * 1024, cancellationToken);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? "text/csv"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", file.Name);
        using var response = await _api.PostMultipartAsync("api/scheduling/import/preview", content, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<ScheduleImportPreviewItem>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<ScheduleImportResult> CommitImportAsync(IEnumerable<ScheduleImportPreviewItem> preview, CancellationToken cancellationToken = default)
    {
        var previewItems = preview.ToList();

        // Every parsed schedule can be persisted. Patient/clinician matching
        // is optional metadata, not a prerequisite for saving an external
        // schedule.
        var items = previewItems
            .Where(x => x.CanImport)
            .Select(x => new ScheduleImportCommitItem
            {
                PatientName = x.PatientName,
                PatientId = x.PatientId,
                ExistingVisitId = x.ExistingVisitId,
                ClinicianId = x.ClinicianId,
                ClinicianName = x.ClinicianName,
                HasWarning = x.HasWarning,
                ScheduledDate = x.ScheduledDate,
                VisitType = x.VisitType,
                Location = x.Location,
                Notes = x.Notes,
                OverwriteExisting = x.OverwriteExisting
            })
            .ToList();

        var skipped = previewItems.Count - items.Count;

        if (items.Count == 0)
            return new ScheduleImportResult { Skipped = skipped };
        using var response = await _api.PostAsJsonAsync("api/scheduling/import/commit", items, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ScheduleImportResult>(cancellationToken: cancellationToken) ?? new();
        result.Skipped = skipped;
        return result;
    }
}
