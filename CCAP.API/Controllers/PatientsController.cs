using CCAP.API.Authorization;
using CCAP.API.Contracts.Patients;
using CCAP.Application.Features.Patients.Commands.AddCallNote;
using CCAP.Application.Features.Patients.Commands.AddServiceOrder;
using CCAP.Application.Features.Patients.Commands.AddCareLog;
using CCAP.Application.Features.Patients.Commands.ArchivePatient;
using CCAP.Application.Features.Patients.Commands.CompleteCare;
using CCAP.Application.Features.Patients.Commands.CompleteInsuranceVerification;
using CCAP.Application.Features.Patients.Commands.CompleteSoc;
using CCAP.Application.Features.Patients.Commands.ScheduleSoc;
using CCAP.Application.Features.Patients.Commands.UpdateInsurance;
using CCAP.Application.Features.Patients.Commands.UpdatePatient;
using CCAP.Application.Features.Patients.Commands.UpdateWorkflowDetails;
using CCAP.Application.Features.Patients.Queries.GetPatientCareManagement;
using CCAP.Application.Features.Patients.Queries.GetPatientAuditLog;
using CCAP.Application.Features.Patients.Queries.GetPatients;
using CCAP.Application.Features.Patients.Queries.GetPatientWorkflow;
using CCAP.Application.Features.Patients.Queries.GetServiceTypes;
using CCAP.Application.Features.Patients.Commands.CompleteCompliance;
using CCAP.Application.Features.Patients.Commands.CompleteTask;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize]
public sealed class PatientsController : ControllerBase
{
    private readonly ISender _sender;

    public PatientsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetPatients(
        CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new GetPatientsQuery(),
            cancellationToken));

    [HttpGet("service-types")]
    [Authorize(Policy = PermissionPolicies.PatientsView)]
    public async Task<IActionResult> GetServiceTypes(
        CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new GetServiceTypesQuery(),
            cancellationToken));

    // NEW
    [HttpGet("{patientId:guid}/workflow")]
    [Authorize(Policy = PermissionPolicies.PatientsView)]
    public async Task<IActionResult> GetWorkflow(
    Guid patientId,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetPatientWorkflowQuery(patientId),
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }


    [HttpGet("{patientId:guid}/audit-log")]
    [Authorize(Policy = PermissionPolicies.PatientsView)]
    public async Task<IActionResult> GetAuditLog(Guid patientId, CancellationToken cancellationToken) => Ok(await _sender.Send(new GetPatientAuditLogQuery(patientId), cancellationToken));

    [HttpPost("{patientId:guid}/call-notes")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> AddCallNote(
        Guid patientId,
        AddCallNoteCommand command,
        CancellationToken cancellationToken)
    {
        if (patientId != command.PatientId)
            return BadRequest("Route ID and command PatientId do not match.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId)) return Unauthorized();

        var id = await _sender.Send(new AddCallNoteCommand(
            command.PatientId, currentUserId, command.ContactType, command.Method,
            command.Subject, command.Notes, command.Outcome), cancellationToken);

        return Ok(new { CallNoteId = id });
    }

    [HttpPost("{patientId:guid}/care-logs")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> AddCareLog(Guid patientId, AddCareLogCommand command, CancellationToken cancellationToken)
    {
        if (patientId != command.PatientId) return BadRequest("Route ID and command PatientId do not match.");
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId)) return Unauthorized();
        var id = await _sender.Send(new AddCareLogCommand(command.PatientId, command.LogType, command.Item, command.Quantity, command.Unit, command.Notes, currentUserId), cancellationToken);
        return Ok(new { PatientCareLogId = id });
    }

    [HttpPost("{patientId:guid}/service-orders")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> AddServiceOrder(Guid patientId, AddServiceOrderCommand command, CancellationToken cancellationToken)
    {
        if (patientId != command.PatientId) return BadRequest("Route ID and command PatientId do not match.");
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId)) return Unauthorized();
        var id = await _sender.Send(new AddServiceOrderCommand(command.PatientId, command.ServiceTypeId, command.Frequency, command.Duration, command.IsPrimaryDiscipline, currentUserId), cancellationToken);
        return Ok(new { PatientServiceOrderId = id });
    }

    [HttpPost("{patientId:guid}/complete-care")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> CompleteCare(
        Guid patientId,
        CompleteCareCommand command,
        CancellationToken cancellationToken)
    {
        if (patientId != command.PatientId)
            return BadRequest("Route ID and command PatientId do not match.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId))
            return Unauthorized();

        await _sender.Send(
            new CompleteCareCommand(
                command.PatientId,
                command.FinalStatus,
                currentUserId,
                command.OutcomeDate,
                command.TransferDestination,
                command.TransferReason,
                command.DischargeFeedback),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{patientId:guid}/insurance/verify")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> VerifyInsurance(
    Guid patientId,
    CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var currentUserId))
            return Unauthorized();

        await _sender.Send(
            new CompleteInsuranceVerificationCommand(
                patientId,
                currentUserId),
            cancellationToken);

        return NoContent();
    }


    [HttpPut("{patientId:guid}")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> UpdatePatient(
        Guid patientId,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId)) return Unauthorized();
        await _sender.Send(
            new UpdatePatientCommand(patientId, request.MRN, request.FirstName, request.MiddleName, request.LastName, request.SocDate, currentUserId), cancellationToken);
        return NoContent();
    }

    [HttpPut("{patientId:guid}/workflow-details")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> UpdateWorkflowDetails(Guid patientId, [FromBody] UpdateWorkflowDetailsRequest request, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId)) return Unauthorized();
        await _sender.Send(new UpdateWorkflowDetailsCommand(patientId, request.PreAuthDueDate, request.NumberOfVisits, request.CaseMixType, request.DmeMedSupplyNotes, request.SocFeedbackFromPatient, request.TifDate, request.RocDate, request.RecertDate, request.PcpPtNotified, request.DischargeDate, request.DischargeFeedback, request.TransferDestination, request.TransferDate, request.TransferReason, currentUserId), cancellationToken);
        return NoContent();
    }

    [HttpPut("{patientId:guid}/insurance")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> UpdateInsurance(
    Guid patientId,
    [FromBody] UpdateInsuranceRequest request,
    CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateInsuranceCommand(
                patientId,
                request.PrimaryInsurance,
                request.InsuranceMemberId,
                request.AuthorizationDate,
                request.ApprovedVisits,
                request.AuthorizationRequired),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{patientId:guid}/archive")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> Archive(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId))
            return Unauthorized();

        await _sender.Send(
            new ArchivePatientCommand(patientId, currentUserId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{patientId:guid}/care-management")]
    [Authorize(Policy = PermissionPolicies.PatientsView)]
    public async Task<IActionResult> GetCareManagement(
    Guid patientId,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetPatientCareManagementQuery(patientId),
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{patientId:guid}/soc/schedule")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> ScheduleSoc(
    Guid patientId,
    [FromBody] ScheduleSocRequest request,
    CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var currentUserId))
        {
            return Unauthorized();
        }


        await _sender.Send(
            new ScheduleSocCommand(
                patientId,
                request.SocDate,
                currentUserId),
            cancellationToken);


        return NoContent();
    }


    [HttpPost("{patientId:guid}/soc/complete")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> CompleteSoc(
        Guid patientId,
        [FromBody] CompleteSocRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var currentUserId))
        {
            return Unauthorized();
        }


        await _sender.Send(
            new CompleteSocCommand(
                patientId,
                currentUserId,
                request.Notes),
            cancellationToken);


        return NoContent();
    }

    

    [HttpPost("{patientId:guid}/compliance/{requirementCode}")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> CompleteCompliance(
    Guid patientId,
    string requirementCode,
    CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var currentUserId))
        {
            return Unauthorized();
        }

        await _sender.Send(
            new CompleteComplianceCommand(
                patientId,
                requirementCode,
                currentUserId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("tasks/{taskId:guid}/complete")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> CompleteTask(Guid taskId, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var currentUserId)) return Unauthorized();
        await _sender.Send(new CompleteTaskCommand(taskId, currentUserId), cancellationToken);
        return NoContent();
    }

}