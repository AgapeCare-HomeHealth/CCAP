using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.AddSchedules;

public sealed class AddSchedulesCommandHandler : IRequestHandler<AddSchedulesCommand, AddSchedulesResult>
{
    private readonly IVisitRepository _visits;
    private readonly IUserRepository _users;
    private readonly ILookupOptionRepository _lookups;
    private readonly IUnitOfWork _unitOfWork;

    public AddSchedulesCommandHandler(
        IVisitRepository visits,
        IUserRepository users,
        ILookupOptionRepository lookups,
        IUnitOfWork unitOfWork)
    {
        _visits = visits;
        _users = users;
        _lookups = lookups;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddSchedulesResult> Handle(
        AddSchedulesCommand request,
        CancellationToken cancellationToken)
    {
        var clinicianIds = request.Schedules
            .Where(x => x.AssignedClinicianId.HasValue)
            .Select(x => x.AssignedClinicianId!.Value)
            .Distinct()
            .ToList();

        var clinicians = await _users.GetByIdsAsync(clinicianIds, cancellationToken);
        var clinicianById = clinicians.ToDictionary(x => x.UserId);

        foreach (var row in request.Schedules)
        {
            var clinicianId = row.AssignedClinicianId!.Value;

            if (!clinicianById.TryGetValue(clinicianId, out var clinician))
                throw new KeyNotFoundException($"Assigned clinician {clinicianId} was not found.");

            if (!clinician.IsActive ||
                clinician.Role is null ||
                !string.Equals(clinician.Role.RoleName, "Clinician", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{clinician.FirstName} {clinician.LastName} is not an active clinician.");
            }
        }

        await ValidateLookupValuesAsync(
            request.Schedules,
            cancellationToken);

        var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visitIds = new List<Guid>();
        var added = 0;
        var skippedDuplicates = 0;

        foreach (var row in request.Schedules)
        {
            var scheduledDate = row.Date!.Value.Date;
            var key = BuildDuplicateKey(row.PatientName, scheduledDate, row.TimeBlock);

            if (!existingKeys.Add(key))
            {
                skippedDuplicates++;
                continue;
            }

            var existing = await _visits.FindScheduleDuplicateAsync(
                request.ScheduledByUserId,
                row.PatientName,
                scheduledDate,
                row.TimeBlock,
                cancellationToken);

            if (existing is not null)
            {
                skippedDuplicates++;
                continue;
            }

            var visit = Visit.CreateManualSchedule(
                row.PatientName,
                scheduledDate,
                row.TimeBlock,
                row.ConfirmationStatus,
                row.CallNotes,
                row.AssignedClinicianId!.Value,
                row.VisitType,
                row.VisitStatus,
                row.NotesFlag,
                request.ScheduledByUserId);

            await _visits.AddAsync(visit, cancellationToken);
            visitIds.Add(visit.VisitId);
            added++;
        }

        if (added > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddSchedulesResult
        {
            Added = added,
            SkippedDuplicates = skippedDuplicates,
            VisitIds = visitIds
        };
    }

    private async Task ValidateLookupValuesAsync(
        IReadOnlyList<AddScheduleItem> schedules,
        CancellationToken cancellationToken)
    {
        var confirmationStatuses = await _lookups.GetAsync("ConfirmationStatus", true, cancellationToken);
        var visitTypes = await _lookups.GetAsync("VisitType", true, cancellationToken);
        var visitStatuses = await _lookups.GetAsync("VisitStatus", true, cancellationToken);

        var confirmationNames = confirmationStatuses
            .Select(x => x.DisplayName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var visitTypeNames = visitTypes
            .Select(x => x.DisplayName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var visitStatusNames = visitStatuses
            .Select(x => x.DisplayName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var row in schedules)
        {
            if (!confirmationNames.Contains(row.ConfirmationStatus.Trim()))
                throw new InvalidOperationException(
                    $"Invalid confirmation status: '{row.ConfirmationStatus}'.");

            if (!visitTypeNames.Contains(row.VisitType.Trim()))
                throw new InvalidOperationException(
                    $"Invalid visit type: '{row.VisitType}'.");

            if (!string.IsNullOrWhiteSpace(row.VisitStatus) &&
                !visitStatusNames.Contains(row.VisitStatus.Trim()))
            {
                throw new InvalidOperationException(
                    $"Invalid visit status: '{row.VisitStatus}'.");
            }
        }
    }

    private static string BuildDuplicateKey(
        string patientName,
        DateTime scheduledDate,
        string timeBlock) =>
        $"{NormalizeName(patientName)}|{scheduledDate:yyyy-MM-dd}|{timeBlock.Trim().ToUpperInvariant()}";

    private static string NormalizeName(string value) =>
        string.Join(" ", value.Trim().ToUpperInvariant()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
