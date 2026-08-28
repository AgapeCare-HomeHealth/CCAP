using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Patients.DTOs;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatientCareManagement;

public sealed class GetPatientCareManagementQueryHandler
    : IRequestHandler<
        GetPatientCareManagementQuery,
        PatientCareProfileResponseDto?>
{
    private readonly IPatientRepository _patients;

    public GetPatientCareManagementQueryHandler(
        IPatientRepository patients)
    {
        _patients = patients;
    }

    public async Task<PatientCareProfileResponseDto?> Handle(
        GetPatientCareManagementQuery request,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(
            request.PatientId,
            cancellationToken);

        if (patient is null)
            return null;

        return new PatientCareProfileResponseDto
        {
            PatientId = patient.PatientId,

            // These sections do not have database entities yet.
            Fax = new FaxInformationResponseDto
            {
                PatientId = patient.PatientId
            },

            Notifications = [],

            Notes = [],

            LabOrders = [],

            WoundSupplies = [],

            FoleyChanges = [],

            OrderAlerts = []
        };
    }
}