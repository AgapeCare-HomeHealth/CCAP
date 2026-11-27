using System.Net.Http.Headers;
using System.Net.Http.Json;
using CCAP.Web.Features.Admin.CalendarImport.Models;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Admin.CalendarImport.Services;

public sealed class CalendarImportService
{
    private readonly CcapApiClient _api;
    public CalendarImportService(CcapApiClient api) => _api = api;

    public async Task<List<CalendarImportCareCoordinatorDto>> GetCareCoordinatorsAsync(CancellationToken cancellationToken = default) =>
        await _api.GetFromJsonAsync<List<CalendarImportCareCoordinatorDto>>("api/calendar-import/care-coordinators", cancellationToken) ?? [];

    public async Task<CalendarImportBatchDto> PreviewAsync(byte[] bytes, string fileName, Guid careCoordinatorId, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(careCoordinatorId.ToString()), "careCoordinatorId");
        using var response = await _api.PostMultipartAsync("api/calendar-import/preview", content, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CalendarImportBatchDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The calendar import preview response was empty.");
    }

    public async Task<CalendarImportBatchDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _api.PostAsJsonAsync($"api/calendar-import/{id}/confirm", new { }, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CalendarImportBatchDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The calendar import queue response was empty.");
    }

    public Task<CalendarImportBatchDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _api.GetFromJsonAsync<CalendarImportBatchDto>($"api/calendar-import/{id}", cancellationToken);

    public async Task<List<CalendarImportRowDto>> GetLogsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _api.GetFromJsonAsync<List<CalendarImportRowDto>>($"api/calendar-import/{id}/logs", cancellationToken) ?? [];

    public async Task<List<CalendarImportBatchDto>> HistoryAsync(CancellationToken cancellationToken = default) =>
        await _api.GetFromJsonAsync<List<CalendarImportBatchDto>>("api/calendar-import", cancellationToken) ?? [];

    public async Task<(byte[] Bytes, string FileName)> DownloadErrorsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _api.GetAsync($"api/calendar-import/{id}/errors", cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return (bytes, $"CalendarImport_{id:N}_errors.csv");
    }
}
