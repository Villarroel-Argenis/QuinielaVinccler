namespace QuinielaVinccler.UI.Web.Components.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] private AuthService AuthSvc { get; set; } = null!;
    [Inject] private PendingLoginService PendingLogin { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState> AuthState { get; set; } = null!;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string? _error;
    private bool _loading;
    private bool _showPassword;
    private bool _tokenExpired;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;

        if (state.User.Identity?.IsAuthenticated == true)
        {
            var role = state.User.FindFirst(ClaimTypes.Role)?.Value;
            Nav.NavigateTo(role == AppRoles.Admin ? "/admin" : "/mis-planillas", replace: true);
            return;
        }

        var uri = new Uri(Nav.Uri);
        var qs = System.Web.HttpUtility.ParseQueryString(uri.Query);
        _tokenExpired = qs["error"] == "expired";
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await HandleLogin();
    }

    private async Task HandleLogin()
    {
        _error = null;

        if (string.IsNullOrWhiteSpace(_email) || string.IsNullOrWhiteSpace(_password))
        {
            _error = "Por favor completa todos los campos.";
            return;
        }

        _loading = true;

        try
        {
            var user = await AuthSvc.LoginAsync(_email, _password);

            if (user is null)
            {
                _error = "Correo o contraseña incorrectos.";
                return;
            }

            var principal = AuthSvc.BuildPrincipal(user);
            var token = PendingLogin.Store(principal);
            var returnUrl = user.Role == "Admin" ? "/admin" : "/mis-planillas";

            Nav.NavigateTo(
                $"/api/auth/signin?token={token}&returnUrl={Uri.EscapeDataString(returnUrl)}",
                forceLoad: true);
        }
        finally
        {
            _loading = false;
        }
    }
}