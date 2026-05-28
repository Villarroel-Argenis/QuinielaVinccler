namespace QuinielaVinccler.UI.Web.Components.Pages.Auth;

public partial class Register
{
    [Inject] private IAuthService AuthSvc { get; set; } = null!;
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
    private string? _pendingToken;
    private string? _returnUrl;
    private bool _submitForm;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

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
            var (user, error) = await AuthSvc.RegisterAsync(
          _email, _password, _fullName, _ci, _telefono);

            if (user is null)
            {
                _error = error;
                return;
            }

            var principal = AuthSvc.BuildPrincipal(user);
            _pendingToken = PendingLogin.Store(principal);
            _returnUrl = "/mis-planillas";
            _submitForm = true;
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            _loading = false;
        }
    }
}