namespace QuinielaVinccler.UI.Web.Services;

public interface IPlanillaService
{
    Task<(bool Exito, string? Error)> VincularAsync(string codigo, int userId);
    Task<List<Planilla>> GetPlanillasByUserAsync(int userId);
}