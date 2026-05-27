namespace QuinielaVinccler.UI.Web.Components.Pages.Auth;

public partial class Register
{
    [Inject] private AuthService AuthSvc { get; set; } = null!;
    [Inject] private PendingLoginService PendingLogin { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState> AuthState { get; set; } = null!;

    private string _fullName = string.Empty;
    private string _ci = string.Empty;
    private string _telefono = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string? _error;
    private bool _loading;
    private bool _showPassword;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;

        if (state.User.Identity?.IsAuthenticated == true)
            Nav.NavigateTo("/mis-planillas", replace: true);
    }

    private async Task HandleRegister()
    {
        _error = null;

        if (_password != _confirmPassword)
        {
            _error = "Las contraseñas no coinciden.";
            return;
        }

        _loading = true;

        try
        {
            var (success, error) = await AuthSvc.RegisterAsync(
                _email, _password, _fullName, _ci, _telefono);

            if (!success)
            {
                _error = error;
                return;
            }

            // Auto-login tras registro exitoso
            var user = await AuthSvc.LoginAsync(_email, _password);

            if (user is null)
            {
                Nav.NavigateTo("/login", forceLoad: true);
                return;
            }

            var principal = AuthSvc.BuildPrincipal(user);
            var token = PendingLogin.Store(principal);

            Nav.NavigateTo(
                $"/api/auth/signin?token={token}&returnUrl={Uri.EscapeDataString("/mis-planillas")}",
                forceLoad: true);
        }
        finally
        {
            _loading = false;
        }
    }
}