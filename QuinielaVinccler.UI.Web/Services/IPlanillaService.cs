namespace QuinielaVinccler.UI.Web.Services;

public interface IPlanillaService
{
    Task<List<Planilla>> GetPlanillasByUserAsync(int userId);
    Task<(bool Exito, string? Error)> VincularAsync(string codigo, int userId);

    /// <summary>
    /// Desvinculación por el usuario — respeta restricciones de estado y quiniela cerrada.
    /// </summary>
    Task<(bool Exito, string? Error)> DesvincularAsync(int planillaId, int userId);

    /// <summary>
    /// Desvinculación por el administrador — sin restricciones de estado ni quiniela cerrada.
    /// Elimina todas las predicciones y puntos de la planilla.
    /// </summary>
    Task<(bool Exito, string? Error)> DesvincularAdminAsync(int planillaId);

    /// <summary>
    /// Busca planillas vinculadas por código o por nombre/email de usuario.
    /// Usado exclusivamente por el panel de administrador.
    /// </summary>
    Task<List<PlanillaAdminDto>> BuscarPlanillasAsync(string termino);
}