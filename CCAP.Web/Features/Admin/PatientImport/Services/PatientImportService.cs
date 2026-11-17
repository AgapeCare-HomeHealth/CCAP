using System.Net.Http.Headers;
using System.Net.Http.Json;
using CCAP.Web.Features.Admin.PatientImport.Models;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Admin.PatientImport.Services;

public sealed class PatientImportService
{
    private readonly CcapApiClient _api;
    public PatientImportService(CcapApiClient api) => _api = api;

    public async Task<PatientImportBatchDto> PreviewAsync(byte[] bytes, string fileName, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(file, "file", fileName);
        using var response = await _api.PostMultipartAsync("api/patient-import/preview", content, ct);
        await _api.EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<PatientImportBatchDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("The import preview response was empty.");
    }

    public async Task<PatientImportBatchDto> ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _api.PostAsJsonAsync($"api/patient-import/{id}/confirm", new { }, ct);
        await _api.EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<PatientImportBatchDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("The import queue response was empty.");
    }

    public Task<PatientImportBatchDto?> GetAsync(Guid id, CancellationToken ct = default)
        => _api.GetFromJsonAsync<PatientImportBatchDto>($"api/patient-import/{id}", ct);

    public async Task<List<PatientImportRowDto>> GetLogsAsync(Guid id, CancellationToken ct = default)
        => await _api.GetFromJsonAsync<List<PatientImportRowDto>>($"api/patient-import/{id}/logs", ct) ?? [];

    public async Task<List<PatientImportBatchDto>> HistoryAsync(CancellationToken ct = default)
        => await _api.GetFromJsonAsync<List<PatientImportBatchDto>>("api/patient-import", ct) ?? [];

    public async Task<(byte[] Bytes, string FileName)> DownloadErrorsAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _api.GetAsync($"api/patient-import/{id}/errors", ct);
        await _api.EnsureSuccessAsync(response, ct);
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        return (bytes, $"PatientImport_{id:N}_errors.csv");
    }
}
