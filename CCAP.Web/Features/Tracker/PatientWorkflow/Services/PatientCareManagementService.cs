using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Services;

/// <summary>
/// Patient-profile care-management operations. In MockData mode these are
/// stored in the in-memory MockDataStore. The method boundaries are intentionally
/// API-shaped so the implementation can later be replaced with HTTP calls
/// without changing the Razor components.
/// </summary>
public sealed class PatientCareManagementService
{
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public PatientCareManagementService(MockDataStore mock, MockDataOptions options)
    {
        _mock = mock;
        _options = options;
    }

    public Task<PatientCareProfileDto> GetAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            throw new NotSupportedException("Patient care-management API endpoints are not enabled yet. Set MockData:Enabled=true while these endpoints are being implemented.");

        return Task.FromResult(_mock.GetPatientCareProfile(patientId));
    }

    public Task SaveFaxAsync(FaxInformationDto fax, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.SaveFax(fax);
        return Task.CompletedTask;
    }

    public Task AddNotificationAsync(PatientNotificationDto notification, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.AddNotification(notification);
        return Task.CompletedTask;
    }

    public Task MarkNotificationReadAsync(Guid patientId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.MarkNotificationRead(patientId, notificationId);
        return Task.CompletedTask;
    }

    public Task DeleteNotificationAsync(Guid patientId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.DeleteNotification(patientId, notificationId);
        return Task.CompletedTask;
    }

    public Task AddNoteAsync(PatientNoteDto note, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.AddPatientNote(note);
        return Task.CompletedTask;
    }

    public Task ToggleNoteResolvedAsync(Guid patientId, Guid noteId, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.TogglePatientNoteResolved(patientId, noteId);
        return Task.CompletedTask;
    }

    public Task DeleteNoteAsync(Guid patientId, Guid noteId, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.DeletePatientNote(patientId, noteId);
        return Task.CompletedTask;
    }

    public Task AddLabOrderAsync(LabOrderDto order, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.AddLabOrder(order);
        return Task.CompletedTask;
    }

    public Task UpdateLabOrderStatusAsync(Guid patientId, Guid labOrderId, string status, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.UpdateLabOrderStatus(patientId, labOrderId, status);
        return Task.CompletedTask;
    }

    public Task AddWoundSupplyAsync(WoundSupplyDto supply, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.AddWoundSupply(supply);
        return Task.CompletedTask;
    }

    public Task UpdateWoundSupplyStatusAsync(Guid patientId, Guid supplyId, string status, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.UpdateWoundSupplyStatus(patientId, supplyId, status);
        return Task.CompletedTask;
    }

    public Task AddFoleyChangeAsync(FoleyChangeDto change, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.AddFoleyChange(change);
        return Task.CompletedTask;
    }

    public Task AddOrderAlertAsync(OrderAlertDto alert, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.AddOrderAlert(alert);
        return Task.CompletedTask;
    }

    public Task MarkOrderSignedAsync(Guid patientId, Guid orderAlertId, CancellationToken cancellationToken = default)
    {
        EnsureMock();
        _mock.MarkOrderSigned(patientId, orderAlertId);
        return Task.CompletedTask;
    }

    private void EnsureMock()
    {
        if (!_options.Enabled)
            throw new NotSupportedException("This operation is currently available only in MockData mode.");
    }
}
