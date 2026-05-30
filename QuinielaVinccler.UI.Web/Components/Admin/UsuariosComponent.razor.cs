namespace QuinielaVinccler.UI.Web.Components.Admin;

public partial class UsuariosComponent : ComponentBase
{
    [Inject] private IUserService UserSvc { get; set; } = default!;
    [Inject] private IAuthService AuthSvc { get; set; } = default!;

    private bool _cargando = true;
    private string _termino = "";
    private bool _incluirAdmins = false;
    private List<UserAdminDto> _usuarios = [];
    private List<UserAdminDto> _paginados = [];

    private int _pagina = 1;
    private const int _pageSize = 20;
    private int TotalPaginas => (int)Math.Ceiling(_usuarios.Count / (double)_pageSize);

    // Reset password
    private bool _showResetPassword;
    private UserAdminDto? _userSeleccionado;
    private bool _reseteando;
    private string? _passwordTemporal;
    private string? _errorReset;

    protected override async Task OnInitializedAsync()
    {
        await Buscar();
    }

    private async Task HandleKeyDownBuscar(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await Buscar();
    }

    private async Task ToggleIncluirAdmins(bool valor)
    {
        _incluirAdmins = valor;
        await Buscar();
    }

    private async Task Buscar()
    {
        _cargando = true;
        StateHasChanged();

        _usuarios = await UserSvc.BuscarAsync(_termino, _incluirAdmins);
        _pagina = 1;
        AplicarPaginacion();

        _cargando = false;
    }

    private void CambiarPagina(int nueva)
    {
        _pagina = nueva;
        AplicarPaginacion();
    }

    private void AplicarPaginacion()
    {
        _paginados = _usuarios
            .Skip((_pagina - 1) * _pageSize)
            .Take(_pageSize)
            .ToList();
    }

    private async Task ToggleBloqueo(UserAdminDto user)
    {
        var (exito, _) = await UserSvc.ToggleBloqueoAsync(user.Id);
        if (exito) await Buscar();
    }

    // ── Reset password ───────────────────────────────────────────────────────
    private void AbrirResetPassword(UserAdminDto user)
    {
        _userSeleccionado = user;
        _passwordTemporal = null;
        _errorReset = null;
        _showResetPassword = true;
    }

    private void CerrarResetPassword()
    {
        _showResetPassword = false;
        _userSeleccionado = null;
        _passwordTemporal = null;
        _errorReset = null;
    }

    private async Task EjecutarResetPassword()
    {
        if (_userSeleccionado is null) return;

        _reseteando = true;
        _errorReset = null;
        try
        {
            var (pass, error) = await AuthSvc.ResetearPasswordAdminAsync(_userSeleccionado.Id);
            if (pass is not null)
                _passwordTemporal = pass;
            else
                _errorReset = error ?? "Error desconocido al resetear.";
        }
        catch (Exception ex)
        {
            _errorReset = $"Error: {ex.Message}";
        }
        finally
        {
            _reseteando = false;
        }
    }
}
