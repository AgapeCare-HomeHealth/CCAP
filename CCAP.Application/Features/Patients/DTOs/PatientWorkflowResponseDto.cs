namespace CCAP.Application.Features.Patients.DTOs;

public sealed class PatientWorkflowResponseDto
{
    public PatientHeaderResponseDto Header { get; set; } = new();

    public List<WorkflowStageResponseDto> WorkflowStages { get; set; } = [];

    public NextActionResponseDto NextAction { get; set; } = new();

    public KeyInformationResponseDto KeyInformation { get; set; } = new();

    public List<ActivityResponseDto> RecentActivities { get; set; } = [];

    public PatientSummaryResponseDto Summary { get; set; } = new();
}

public sealed class PatientHeaderResponseDto
{
    public Guid PatientId { get; set; }

    public Guid ReferralId { get; set; }

    public string FirstName { get; set; } = "";

    public string MiddleName { get; set; } = "";

    public string LastName { get; set; } = "";

    public int Age { get; set; }

    public string MRN { get; set; } = "";

    public string ReferralNumber { get; set; } = "";

    public string Status { get; set; } = "";

    public DateOnly? SocDate { get; set; }

    public string Coordinator { get; set; } = "";

    public string Branch { get; set; } = "";

    public int EpisodeNumber { get; set; }
}

public sealed class WorkflowStageResponseDto
{
    public int Sequence { get; set; }

    public string StageCode { get; set; } = "";

    public string StageName { get; set; } = "";

    public string Description { get; set; } = "";

    public int Status { get; set; }

    public DateTime? CompletedDate { get; set; }

    public Guid? AssignedUserId { get; set; }

    public string AssignedUserName { get; set; } = "";

    public bool IsClickable { get; set; }

    public string Route { get; set; } = "";
}

public sealed class NextActionResponseDto
{
    public Guid TaskId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public DateTime DueDate { get; set; }

    public string PageRoute { get; set; } = "";

    public string Icon { get; set; } = "";

    public bool IsOverdue { get; set; }
}

public sealed class KeyInformationResponseDto
{
    public string Coordinator { get; set; } = "";

    public string Clinician { get; set; } = "";

    public string Discipline { get; set; } = "";

    public int Episode { get; set; }

    public string Branch { get; set; } = "";

    public string Payor { get; set; } = "";

    public string Priority { get; set; } = "";
}

public sealed class ActivityResponseDto
{
    public Guid ActivityId { get; set; }

    public DateTime ActivityDate { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string PerformedBy { get; set; } = "";

    public string ActivityType { get; set; } = "";
}

public sealed class PatientSummaryResponseDto
{
    public string PrimaryDiagnosis { get; set; } = "";

    public string Insurance { get; set; } = "";

    public DateOnly? SocDate { get; set; }

    public int AuthorizedVisits { get; set; }

    public string Address { get; set; } = "";

    public string PhoneNumber { get; set; } = "";
}