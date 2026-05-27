
namespace QuinielaVinccler.UI.Web.Services;

public class PendingLoginService
{
    private readonly ConcurrentDictionary<string, (ClaimsPrincipal Principal, DateTime Expiry)> _pending = new();

    public string Store(ClaimsPrincipal principal)
    {
        var token = Guid.NewGuid().ToString("N");
        _pending[token] = (principal, DateTime.UtcNow.AddSeconds(30));
        return token;
    }

    public ClaimsPrincipal? Consume(string token)
    {
        if (_pending.TryRemove(token, out var entry) && entry.Expiry > DateTime.UtcNow)
            return entry.Principal;

        return null;
    }
}