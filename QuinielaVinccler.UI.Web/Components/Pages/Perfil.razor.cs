using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace QuinielaVinccler.UI.Web.Components.Pages;

public partial class Perfil : ComponentBase
{
    [Inject] private IAuthService AuthSvc { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthSp { get; set; } = null!;

    private string _passwordActual = "";
    private string _passwordNueva = "";
    private string _passwordConfirma = "";
    private bool _verActual;
    private bool _verNueva;

    private bool _guardando;
    private bool _exito;
    private string? _error;

    private int _userId;

    private bool PuedeGuardar =>
        !string.IsNullOrWhiteSpace(_passwordActual) &&
        !string.IsNullOrWhiteSpace(_passwordNueva) &&
        !string.IsNullOrWhiteSpace(_passwordConfirma);

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthSp.GetAuthenticationStateAsync();
        var claim = auth.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out var id))
            _userId = id;
    }

    private async Task CambiarPassword()
    {
        _error = null;
        _exito = false;

        if (_passwordNueva != _passwordConfirma)
        {
            _error = "Las contraseñas nuevas no coinciden.";
            return;
        }

        _guardando = true;
        try
        {
            var (exito, error) = await AuthSvc.CambiarPasswordAsync(
                _userId, _passwordActual, _passwordNueva);

            if (exito)
            {
                _exito = true;
                _passwordActual = "";
                _passwordNueva = "";
                _passwordConfirma = "";
            }
            else
            {
                _error = error;
            }
        }
        catch (Exception ex)
        {
            _error = $"Error: {ex.Message}";
        }
        finally
        {
            _guardando = false;
        }
    }
}
