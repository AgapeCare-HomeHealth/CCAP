namespace CCAP.Application.Features.Dashboard.DTOs;

public sealed class DashboardDto
{
    public WorkStatisticsDto Statistics { get; set; } = new();

    public List<MyTaskDto> Tasks { get; set; } = [];

    public List<ReferralSummaryDto> RecentReferrals { get; set; } = [];

    public List<UpcomingVisitDto> UpcomingVisits { get; set; } = [];

    public List<AnnouncementDto> Announcements { get; set; } = [];
}

public sealed class WorkStatisticsDto
{
    public int MyTasks { get; set; }

    public int HighPriority { get; set; }

    public int DueToday { get; set; }

    public int CompletedToday { get; set; }
}

public sealed class MyTaskDto
{
    public Guid TransactionId { get; set; }

    public Guid PatientId { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string TaskName { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public string AssignedTo { get; set; } = string.Empty;

    public string CurrentStage { get; set; } = string.Empty;
}

public sealed class ReferralSummaryDto
{
    public string PatientName { get; set; } = string.Empty;

    public string ReferralNumber { get; set; } = string.Empty;

    public DateTime ReferralDate { get; set; }
}

public sealed class UpcomingVisitDto
{
    public string PatientName { get; set; } = string.Empty;

    public string Clinician { get; set; } = string.Empty;

    public DateTime VisitDate { get; set; }
}

public sealed class AnnouncementDto
{
    public Guid AnnouncementId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}