using CCAP.API.Authorization;
using CCAP.Application.Features.Patients.Commands.AddCallNote;
using CCAP.Application.Features.Patients.Commands.ArchivePatient;
using CCAP.Application.Features.Patients.Commands.CompleteCare;
using CCAP.Application.Features.Patients.Queries.GetPatients;
using CCAP.Application.Features.Patients.Queries.GetServiceTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize]
public sealed class PatientsController : ControllerBase
{
    private readonly ISender _sender;
    public PatientsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetPatients(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetPatientsQuery(), cancellationToken));

    [HttpGet("service-types")]
    [Authorize(Policy = PermissionPolicies.PatientsView)]
    public async Task<IActionResult> GetServiceTypes(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetServiceTypesQuery(), cancellationToken));

    [HttpPost("{patientId:guid}/call-notes")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> AddCallNote(
        Guid patientId,
        AddCallNoteCommand command,
        CancellationToken cancellationToken)
    {
        if (patientId != command.PatientId)
            return BadRequest("Route ID and command PatientId do not match.");

        var id = await _sender.Send(command, cancellationToken);
        return Ok(new { CallNoteId = id });
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

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{patientId:guid}/archive")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> Archive(
        Guid patientId,
        ArchivePatientCommand command,
        CancellationToken cancellationToken)
    {
        if (patientId != command.PatientId)
            return BadRequest("Route ID and command PatientId do not match.");

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}
