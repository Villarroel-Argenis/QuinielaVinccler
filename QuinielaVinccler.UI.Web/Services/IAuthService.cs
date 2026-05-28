namespace QuinielaVinccler.UI.Web.Services;

public interface IAuthService
{
    Task<AppUser?> LoginAsync(string email, string password);
    Task<(AppUser? User, string? Error)> RegisterAsync(
        string email, string password, string fullName, string ci, string telefono);
    ClaimsPrincipal BuildPrincipal(AppUser user);
}
