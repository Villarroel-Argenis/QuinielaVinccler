namespace QuinielaVinccler.UI.Web.Services;

public interface IPredictionService
{
    Task<Planilla?> CargarPlanillaAsync(int planillaId, int userId);
    Task<List<Equipo>> GetEquiposAsync();

    Task GuardarGrupoAsync(int prediccionId, ResultadoPartido? resultado);

    // R32: predice qué equipo ocupa cada slot
    Task GuardarR32LocalAsync(int prediccionId, int? equipoId);
    Task GuardarR32VisitanteAsync(int prediccionId, int? equipoId);

    // R16+: predice el ganador del partido
    Task GuardarGanadorAsync(int prediccionId, int? equipoId);

    Task GuardarFinalAsync(PrediccionFinal prediccion);
    Task ActualizarEstadoAsync(int planillaId, EstadoPlanilla estado);
    Task ResetTotalAsync(int planillaId);
}