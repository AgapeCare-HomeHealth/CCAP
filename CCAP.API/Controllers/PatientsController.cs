using CCAP.API.Authorization;
using CCAP.API.Contracts.Patients;
using CCAP.Application.Features.Patients.Commands.AddCallNote;
using CCAP.Application.Features.Patients.Commands.AddServiceOrder;
using CCAP.Application.Features.Patients.Commands.AddCareLog;
using CCAP.Application.Features.Patients.Commands.ArchivePatient;
using CCAP.Application.Features.Patients.Commands.CompleteCare;
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
using CCAP.Application.Features.Patients.Commands.UnmarkCompliance;
using CCAP.Application.Features.Patients.Commands.SetComplianceStatus;
using CCAP.Application.Features.Patients.Commands.CompleteTask;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CCAP.Infrastructure.Persistence;
using System.Security.Claims;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize]
public sealed class PatientsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly AppDbContext _db;

    public PatientsController(ISender sender, AppDbContext db)
    {
        _sender = sender;
        _db = db;
    }

    // The JWT normally contains the database UserId. If the database was reseeded
    // and the browser is holding an old token, resolve the current user by email
    // instead of attempting to write an invalid audit-log foreign key.
    private async Task<Guid?> ResolveCurrentUserIdAsync(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idClaim, out var claimedId) &&
            await _db.ApplicationUsers.AsNoTracking()
                .AnyAsync(u => u.UserId == claimedId, cancellationToken))
            return claimedId;

        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email);

        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _db.ApplicationUsers.AsNoTracking()
            .Where(u => u.Email == email.Trim().ToLower())
            .Select(u => (Guid?)u.UserId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? clinicianId = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default) =>
        Ok(await _sender.Send(
            new GetPatientsQuery(pageNumber, pageSize, search, status, clinicianId, sortBy, sortDescending),
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
    [Authorize(Roles = "Administrator")]
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
        var resolvedUserId = await ResolveCurrentUserIdAsync(cancellationToken);
        if (resolvedUserId is null)
            return Unauthorized(new { message = "Your login is no longer linked to an active CCAP user. Please sign out and sign in again." });

        await _sender.Send(
            new CompleteComplianceCommand(
                patientId,
                requirementCode,
                resolvedUserId.Value),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{patientId:guid}/compliance/{requirementCode}/status")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> SetComplianceStatus(Guid patientId, string requirementCode, [FromBody] SetComplianceStatusRequest request, CancellationToken cancellationToken)
    {
        var resolvedUserId = await ResolveCurrentUserIdAsync(cancellationToken);
        if (resolvedUserId is null) return Unauthorized(new { message = "Your login is no longer linked to an active CCAP user. Please sign out and sign in again." });
        await _sender.Send(new SetComplianceStatusCommand(patientId, requirementCode, request.Status, resolvedUserId.Value), cancellationToken);
        return NoContent();
    }

    public sealed record SetComplianceStatusRequest(string Status);

    [HttpPost("{patientId:guid}/compliance/{requirementCode}/unmark")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> UnmarkCompliance(Guid patientId, string requirementCode, CancellationToken cancellationToken)
    {
        var resolvedUserId = await ResolveCurrentUserIdAsync(cancellationToken);
        if (resolvedUserId is null) return Unauthorized(new { message = "Your login is no longer linked to an active CCAP user. Please sign out and sign in again." });
        await _sender.Send(new UnmarkComplianceCommand(patientId, requirementCode, resolvedUserId.Value), cancellationToken);
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