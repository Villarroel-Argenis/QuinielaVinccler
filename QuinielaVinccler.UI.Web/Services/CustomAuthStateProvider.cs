namespace QuinielaVinccler.UI.Web.Services;

// Scoped: una instancia por circuito Blazor Server.
// HttpContext solo está disponible durante el request HTTP inicial
// que establece la conexión SignalR — se captura en el constructor.
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _user;

    public CustomAuthStateProvider(IHttpContextAccessor accessor)
    {
        _user = accessor.HttpContext?.User
            ?? new ClaimsPrincipal(new ClaimsIdentity());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_user));

    public void NotifyLogout()
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }
}
