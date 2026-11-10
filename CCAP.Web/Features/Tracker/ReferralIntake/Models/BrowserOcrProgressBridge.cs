using Microsoft.JSInterop;

namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

/// <summary>Bridges browser OCR progress messages back to the interactive Blazor component.</summary>
public sealed class BrowserOcrProgressBridge
{
    private readonly Func<int, int, string, Task> _report;

    public BrowserOcrProgressBridge(Func<int, int, string, Task> report)
    {
        _report = report;
    }

    [JSInvokable]
    public Task ReportProgress(int currentPage, int totalPages, string stage) =>
        _report(currentPage, totalPages, stage);
}
