namespace QuinielaVinccler.UI.Web.Services;

public sealed record UserAdminDto(
    int Id,
    string Email,
    string FullName,
    string CI,
    string Telefono,
    string Role,
    bool IsBlocked,
    DateTime CreatedAt,
    int CantidadPlanillas
);

public interface IUserService
{
    Task<List<UserAdminDto>> BuscarAsync(string? termino, bool incluirAdmins);
    Task<(bool Exito, string? Error)> ToggleBloqueoAsync(int userId);
}
