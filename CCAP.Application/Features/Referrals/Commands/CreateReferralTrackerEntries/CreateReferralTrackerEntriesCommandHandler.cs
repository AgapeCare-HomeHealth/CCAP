using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Referrals.DTOs;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.CreateReferralTrackerEntries;

public sealed class CreateReferralTrackerEntriesCommandHandler
    : IRequestHandler<CreateReferralTrackerEntriesCommand, CreateReferralTrackerEntriesResult>
{
    private readonly IReferralTrackerRepository _referrals;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReferralTrackerEntriesCommandHandler(
        IReferralTrackerRepository referrals,
        IUserRepository users,
        IUnitOfWork unitOfWork)
    {
        _referrals = referrals;
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateReferralTrackerEntriesResult> Handle(
        CreateReferralTrackerEntriesCommand request,
        CancellationToken cancellationToken)
    {
        var entries = request.Entries?.ToList() ?? [];
        if (entries.Count == 0)
            throw new ArgumentException("At least one tracker entry is required.");
        if (entries.Count > 100)
            throw new ArgumentException("A maximum of 100 tracker entries can be saved at once.");

        var invalidIndex = entries.FindIndex(entry =>
            string.IsNullOrWhiteSpace(entry.FirstName) ||
            string.IsNullOrWhiteSpace(entry.LastName) ||
            !entry.DateOfBirth.HasValue);
        if (invalidIndex >= 0)
            throw new ArgumentException(
                $"Referral row {invalidIndex + 1}: first name, last name, and date of birth are required.");

        var duplicateMrn = entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Mrn))
            .GroupBy(entry => entry.Mrn!.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateMrn is not null)
            throw new ArgumentException(
                $"MRN {duplicateMrn.Key} appears more than once in this batch. Verify the patient records before saving.");

        var coordinatorIds = entries
            .Where(entry => entry.AssignedCoordinatorId.HasValue)
            .Select(entry => entry.AssignedCoordinatorId!.Value)
            .Distinct()
            .ToList();
        var coordinators = await _users.GetByIdsAsync(coordinatorIds, cancellationToken);
        if (coordinators.Count != coordinatorIds.Count || coordinators.Any(user =>
                !user.IsActive ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.PasswordHash) ||
                user.Role is null ||
                !user.Role.IsActive ||
                !user.Role.RoleName.Contains("Coordinator", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                "Selected Care Coordinator accounts must be active registered coordinator accounts.");
        }

        var coordinatorNames = coordinators.ToDictionary(
            user => user.UserId,
            user => $"{user.FirstName} {user.LastName}".Trim());

        foreach (var entry in entries)
        {
            var assignedCoordinatorName = entry.AssignedCoordinatorId is Guid coordinatorId
                ? coordinatorNames[coordinatorId]
                : null;

            var referral = new ReferralTrackerEntry(
                $"TRK-{Guid.NewGuid():N}"[..20],
                entry.ReferralDate ?? DateTime.Today,
                entry.FirstName,
                entry.LastName,
                DateOnly.FromDateTime(entry.DateOfBirth!.Value),
                entry.Mrn,
                entry.PostedOn,
                entry.EstimatedDischargeDate,
                entry.Status,
                entry.Source,
                entry.Insurance,
                entry.AssignedCoordinatorId,
                assignedCoordinatorName,
                entry.FirstMarketingCall,
                entry.SecondCoordinatorCall,
                entry.ThirdCoordinatorCall,
                entry.Resolution);

            await _referrals.AddAsync(referral, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateReferralTrackerEntriesResult(entries.Count);
    }
}
