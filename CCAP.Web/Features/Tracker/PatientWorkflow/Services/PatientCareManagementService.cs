using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Services;

public sealed class PatientCareManagementService
{
    private readonly CcapApiClient _api;

    public PatientCareManagementService(
        CcapApiClient api)
    {
        _api = api;
    }

    public async Task<PatientCareProfileDto?> GetAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<PatientCareProfileDto>(
            $"api/patients/{patientId}/care-management",
            cancellationToken);
    }
    public async Task<List<ServiceTypeDto>> GetServiceTypesAsync(CancellationToken cancellationToken = default)
        => await _api.GetFromJsonAsync<List<ServiceTypeDto>>("api/patients/service-types", cancellationToken) ?? [];

    public async Task AddCommunicationAsync(Guid patientId, string contactType, string method, string subject, string notes, string? outcome, CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync($"api/patients/{patientId}/call-notes", new { PatientId = patientId, ContactType = contactType, Method = method, Subject = subject, Notes = notes, Outcome = outcome }, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task AddCareLogAsync(Guid patientId, string logType, string item, decimal? quantity, string? unit, string? notes, CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync($"api/patients/{patientId}/care-logs", new { PatientId = patientId, LogType = logType, Item = item, Quantity = quantity, Unit = unit, Notes = notes }, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task AddServiceOrderAsync(Guid patientId, Guid serviceTypeId, string? frequency, string? duration, bool primary, CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync($"api/patients/{patientId}/service-orders", new { PatientId = patientId, ServiceTypeId = serviceTypeId, Frequency = frequency, Duration = duration, IsPrimaryDiscipline = primary }, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CompleteTaskAsync(Guid taskId,CancellationToken cancellationToken=default)
    { var response=await _api.PostAsJsonAsync($"api/patients/tasks/{taskId}/complete",new { },cancellationToken); await _api.EnsureSuccessAsync(response, cancellationToken); }

}