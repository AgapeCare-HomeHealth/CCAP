using CCAP.Web.Features.Authentication.Services;
using Microsoft.AspNetCore.Components.Forms;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Services;

public sealed class PatientWorkflowService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public PatientWorkflowService(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<PatientWorkflowDto?> GetAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return _mock.GetPatientWorkflow(patientId);

        return await _api.GetFromJsonAsync<PatientWorkflowDto>(
            $"api/patients/{patientId}/workflow", cancellationToken);
    }
    public async Task<List<PatientAuditLogDto>> GetAuditLogAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled) return [];
        return await _api.GetFromJsonAsync<List<PatientAuditLogDto>>($"api/patients/{patientId}/audit-log", cancellationToken) ?? [];
    }

    public async Task CompleteCareAsync(
        Guid patientId,
        string finalStatus, string? transferDestination = null, DateOnly? outcomeDate = null, string? transferReason = null, string? dischargeFeedback = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            $"api/patients/{patientId}/complete-care",
            new { PatientId = patientId, FinalStatus = finalStatus, TransferDestination = transferDestination, OutcomeDate = outcomeDate, TransferReason = transferReason, DischargeFeedback = dischargeFeedback },
            cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task ArchivePatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            $"api/patients/{patientId}/archive",
            new { },
            cancellationToken);

        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateHeaderAsync(PatientEditModel model, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            _mock.UpdatePatient(model);
            return;
        }

        var response = await _api.PutAsJsonAsync(
            $"api/patients/{model.PatientId}",
            new
            {
                model.MRN,
                model.FirstName,
                model.MiddleName,
                model.LastName,
                model.SocDate
            },
            cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CompleteInsuranceVerificationAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            $"api/patients/{patientId}/insurance/verify",
            new { },
            cancellationToken);

        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateInsuranceAsync(
        Guid patientId,
        InsuranceEditModel model,
        CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            throw new InvalidOperationException(
                "Insurance updates require the real API.");
        }

        var response = await _api.PutAsJsonAsync(
            $"api/patients/{patientId}/insurance",
            model,
            cancellationToken);

        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CompleteComplianceAsync(
    Guid patientId,
    string requirementCode,
    CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            $"api/patients/{patientId}/compliance/{Uri.EscapeDataString(requirementCode)}",
            new { },
            cancellationToken);

        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task ScheduleSocAsync(
    Guid patientId,
    DateOnly socDate,
    CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            $"api/patients/{patientId}/soc/schedule",
            new
            {
                SocDate = socDate
            },
            cancellationToken);

        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<PreAuthDocumentDto?> GetPreAuthDocumentAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return null;

        return await _api.GetFromJsonAsync<PreAuthDocumentDto>(
            $"api/patients/{patientId}/compliance/pre-auth/document",
            cancellationToken);
    }

    public async Task UploadPreAuthDocumentAsync(
        Guid patientId,
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            throw new InvalidOperationException("Document uploads require the real API.");

        await using var stream = file.OpenReadStream(10 * 1024 * 1024);
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(stream);

        if (!string.IsNullOrWhiteSpace(file.ContentType))
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, "file", file.Name);

        var response = await _api.PostMultipartAsync(
            $"api/patients/{patientId}/compliance/pre-auth/document",
            content,
            cancellationToken);

        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<(byte[] Content, string ContentType)?> GetPreAuthDocumentFileAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return null;

        using var response = await _api.GetAsync(
            $"api/patients/{patientId}/compliance/pre-auth/document/file",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await _api.EnsureSuccessAsync(response, cancellationToken);

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        return (bytes, contentType);
    }

    public async Task UpdateWorkflowDetailsAsync(Guid patientId, WorkflowDetailsDto model, CancellationToken cancellationToken = default)
    {
        var response = await _api.PutAsJsonAsync($"api/patients/{patientId}/workflow-details", model, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

}
