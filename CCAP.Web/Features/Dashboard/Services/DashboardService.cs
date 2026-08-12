using System.Net.Http.Json;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Dashboard.Models;
using CCAP.Web.Features.MockData;

namespace CCAP.Web.Features.Dashboard.Services;

public sealed class DashboardService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public DashboardService(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<DashboardData> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return BuildMock();

        // These endpoints are the intended API contract. Keep the UI dependent on this service,
        // so switching from mock data to API data does not require changing Dashboard.razor.
        var data = await _api.GetFromJsonAsync<DashboardData>("api/dashboard", cancellationToken);
        return data ?? new DashboardData();
    }

    private DashboardData BuildMock()
    {
        return new DashboardData
        {
            Statistics = new WorkStatisticsModel { MyTasks = 14, HighPriority = 5, DueToday = 6, CompletedToday = 8 },
            Tasks =
            [
                new MyTaskDto { TransactionId = Guid.NewGuid(), PatientId = _mock.JohnId, Priority = "High", TaskName = "Verify Insurance", PatientName = "John Michael Smith", CurrentStage = "Insurance Verification", AssignedTo = "Jennifer RN", DueDate = DateTime.Today.AddHours(10) },
                new MyTaskDto { TransactionId = Guid.NewGuid(), PatientId = _mock.MariaId, Priority = "High", TaskName = "Schedule SOC", PatientName = "Maria Elena Cruz", CurrentStage = "SOC Scheduling", AssignedTo = "Jennifer RN", DueDate = DateTime.Today.AddHours(13) },
                new MyTaskDto { TransactionId = Guid.NewGuid(), PatientId = _mock.RobertId, Priority = "Medium", TaskName = "Upload Plan of Care", PatientName = "Robert James Brown", CurrentStage = "Compliance", AssignedTo = "Jennifer RN", DueDate = DateTime.Today.AddDays(1) }
            ],
            RecentReferrals =
            [
                new ReferralSummaryDto { PatientName = "John Michael Smith", ReferralNumber = "RF-2026-0018", ReferralDate = DateTime.Today },
                new ReferralSummaryDto { PatientName = "Maria Elena Cruz", ReferralNumber = "RF-2026-0019", ReferralDate = DateTime.Today.AddDays(-1) }
            ],
            UpcomingVisits =
            [
                new UpcomingVisitDto { PatientName = "John Michael Smith", Clinician = "Jennifer RN", VisitDate = DateTime.Today.AddDays(1).AddHours(9) },
                new UpcomingVisitDto { PatientName = "Maria Elena Cruz", Clinician = "Renaldi LVN", VisitDate = DateTime.Today.AddDays(2).AddHours(10) }
            ],
            Announcements =
            [
                new AnnouncementDto { Title = "System Maintenance", Message = "Scheduled maintenance tonight from 10:00 PM to 12:00 AM." },
                new AnnouncementDto { Title = "Policy Update", Message = "Updated SOC documentation guidelines are now available." }
            ]
        };
    }
}

public sealed class DashboardData
{
    public WorkStatisticsModel Statistics { get; set; } = new();
    public List<MyTaskDto> Tasks { get; set; } = [];
    public List<ReferralSummaryDto> RecentReferrals { get; set; } = [];
    public List<UpcomingVisitDto> UpcomingVisits { get; set; } = [];
    public List<AnnouncementDto> Announcements { get; set; } = [];
}
