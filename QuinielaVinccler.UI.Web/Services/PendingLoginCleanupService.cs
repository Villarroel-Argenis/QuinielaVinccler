namespace QuinielaVinccler.UI.Web.Services;

public class PendingLoginCleanupService(PendingLoginService pending) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
            pending.Cleanup();
        }
    }
}