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
            if (!IsValidTimeBlock(row.TimeBlock))
                throw new ArgumentException($"Row {rowNumber}: enter a valid time block, for example '9:00 AM - 10:00 AM'.");
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
            if (row.VisitsEachDay < 1 || row.VisitsEachDay > 24)
                throw new ArgumentException($"Row {rowNumber}: visits each day must be between 1 and 24.");
            if ((row.Skills?.Length ?? 0) > 300)
                throw new ArgumentException($"Row {rowNumber}: skills cannot exceed 300 characters.");
            if ((row.TreatmentAuthorizationCode?.Length ?? 0) > 100)
                throw new ArgumentException($"Row {rowNumber}: treatment authorization code cannot exceed 100 characters.");
            if ((row.AttachmentsNeeded?.Length ?? 0) > 1000)
                throw new ArgumentException($"Row {rowNumber}: attachments needed cannot exceed 1,000 characters.");
            if ((row.BillingRateCode?.Length ?? 0) > 100 || (row.BillingRevenueCode?.Length ?? 0) > 100)
                throw new ArgumentException($"Row {rowNumber}: billing codes cannot exceed 100 characters.");
            if ((row.BillingAmount.HasValue && row.BillingAmount.Value < 0) || (row.BillingOvertimeAmount.HasValue && row.BillingOvertimeAmount.Value < 0))
                throw new ArgumentException($"Row {rowNumber}: billing amounts cannot be negative.");
        }
    }

    private static bool IsValidTimeBlock(string value)
    {
        var normalized = value.Trim().Replace('–', '-');
        var parts = normalized.Split('-', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 0 || !TryParseTime(parts[0]))
            return false;
        return parts.Length == 1 || TryParseTime(parts[1]);
    }

    private static bool TryParseTime(string value) => DateTime.TryParse(
        value,
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.AllowWhiteSpaces,
        out _);
}
