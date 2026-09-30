namespace CCAP.Web.Features.Authentication.Services;

public sealed class SessionExpirationService
{
    public event Func<Task>? SessionExpired;

    private bool _notified;

    public async Task NotifyExpiredAsync()
    {
        if (_notified)
            return;

        _notified = true;

        if (SessionExpired is not null)
        {
            await SessionExpired.Invoke();
        }
    }

    public void Reset()
    {
        _notified = false;
    }
}