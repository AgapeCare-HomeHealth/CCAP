using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Scheduling.Commands.AddSchedules;

public sealed class AddSchedulesCommandValidator : IRequestValidator<AddSchedulesCommand>
{
    public void Validate(AddSchedulesCommand request)
    {
        if (request.ScheduledByUserId == Guid.Empty)
            throw new ArgumentException("Scheduled by user ID is required.");

        if (request.Schedules is null || request.Schedules.Count == 0)
            throw new ArgumentException("At least one schedule is required.");

        if (request.Schedules.Count > 200)
            throw new ArgumentException("A maximum of 200 schedules can be added at once.");

        for (var i = 0; i < request.Schedules.Count; i++)
        {
            var row = request.Schedules[i];
            var rowNumber = i + 1;

            if (!row.Date.HasValue)
                throw new ArgumentException($"Row {rowNumber}: date is required.");
            if (row.Date.Value == default)
                throw new ArgumentException($"Row {rowNumber}: date is invalid.");
            if (string.IsNullOrWhiteSpace(row.TimeBlock))
                throw new ArgumentException($"Row {rowNumber}: time block is required.");
            if (row.TimeBlock.Length > 100)
                throw new ArgumentException($"Row {rowNumber}: time block cannot exceed 100 characters.");
            if (string.IsNullOrWhiteSpace(row.PatientName))
                throw new ArgumentException($"Row {rowNumber}: patient name is required.");
            if (row.PatientName.Length > 300)
                throw new ArgumentException($"Row {rowNumber}: patient name cannot exceed 300 characters.");
            if (string.IsNullOrWhiteSpace(row.ConfirmationStatus))
                throw new ArgumentException($"Row {rowNumber}: confirmation status is required.");
            if (row.ConfirmationStatus.Length > 100)
                throw new ArgumentException($"Row {rowNumber}: confirmation status cannot exceed 100 characters.");
            if ((row.CallNotes?.Length ?? 0) > 4000)
                throw new ArgumentException($"Row {rowNumber}: call notes cannot exceed 4,000 characters.");
            if (!row.AssignedClinicianId.HasValue || row.AssignedClinicianId.Value == Guid.Empty)
                throw new ArgumentException($"Row {rowNumber}: assigned clinician is required.");
            if (string.IsNullOrWhiteSpace(row.VisitType))
                throw new ArgumentException($"Row {rowNumber}: visit type is required.");
            if (row.VisitType.Length > 100)
                throw new ArgumentException($"Row {rowNumber}: visit type cannot exceed 100 characters.");
            if ((row.VisitStatus?.Length ?? 0) > 100)
                throw new ArgumentException($"Row {rowNumber}: visit status cannot exceed 100 characters.");
            if ((row.NotesFlag?.Length ?? 0) > 4000)
                throw new ArgumentException($"Row {rowNumber}: notes / flag cannot exceed 4,000 characters.");
        }
    }
}
