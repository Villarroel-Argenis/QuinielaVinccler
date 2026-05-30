namespace QuinielaVinccler.UI.Web.Services;

public interface IAuthService
{
    Task<(AppUser? User, string? Error)> LoginAsync(string email, string password);

    Task<(AppUser? User, string? Error)> RegisterAsync(
        string email, string password, string fullName, string ci, string telefono);

    ClaimsPrincipal BuildPrincipal(AppUser user);

    /// <summary>
    /// Cambia la contraseña del usuario validando la actual.
    /// Usado por el propio usuario desde su perfil.
    /// </summary>
    Task<(bool Exito, string? Error)> CambiarPasswordAsync(
        int userId, string passwordActual, string passwordNueva);

    /// <summary>
    /// Resetea la contraseña de un usuario sin requerir la actual.
    /// Genera una contraseña temporal aleatoria y la retorna.
    /// Solo para uso administrativo.
    /// </summary>
    Task<(string? PasswordTemporal, string? Error)> ResetearPasswordAdminAsync(int userId);
}
