// Services/IPartidoAdminService.cs
namespace QuinielaVinccler.UI.Web.Services;

public interface IPartidoAdminService
{
    Task<List<Partido>> GetPartidosGrupoAsync();
    Task<List<Partido>> GetPartidosKnockoutAsync();
    Task<(bool ok, string mensaje)> SwapLocalVisitanteAsync(int partidoId);
    Task<(bool ok, string mensaje)> SwapNumerosAsync(int partidoIdA, int partidoIdB);
    Task<(bool ok, string mensaje)> CambiarSlotsAsync(int partidoId, string slotLocal, string slotVisitante);
}