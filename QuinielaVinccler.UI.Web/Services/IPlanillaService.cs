namespace QuinielaVinccler.UI.Web.Services;

public interface IPlanillaService
{
    Task<List<Planilla>> GetPlanillasByUserAsync(int userId);
    Task<(bool Exito, string? Error)> VincularAsync(string codigo, int userId);
    Task<(bool Exito, string? Error)> DesvincularAsync(int planillaId, int userId);
    Task<(bool Exito, string? Error)> DesvincularAdminAsync(int planillaId);
    Task<List<PlanillaAdminDto>> BuscarPlanillasAsync(string termino);

    /// <summary>
    /// Devuelve el ranking completo de planillas asignadas, ordenadas por PuntajeTotal DESC.
    /// Usa dense ranking: empates comparten posición y no saltan la siguiente.
    /// </summary>
    Task<List<RankingItemDto>> GetRankingAsync(int userId);
}