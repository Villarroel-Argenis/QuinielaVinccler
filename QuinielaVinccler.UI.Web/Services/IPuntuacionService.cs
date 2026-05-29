namespace QuinielaVinccler.UI.Web.Services;

public interface IPuntuacionService
{
    /// <summary>
    /// Recalcula puntos de todas las planillas para un partido de fase de grupos
    /// cuando el admin ingresa el resultado (1/X/2).
    /// Dispara actualización de PuntajeTotal en cada Planilla afectada.
    /// </summary>
    Task RecalcularGrupoAsync(int partidoId);

    /// <summary>
    /// Recalcula puntos de todas las planillas para un partido de fase eliminatoria
    /// cuando el admin ingresa el equipo ganador.
    /// </summary>
    Task RecalcularKnockoutAsync(int partidoId);

    /// <summary>
    /// Recalcula puntos del resultado exacto a 90 minutos para semis (#101, #102)
    /// y gran final (#104) cuando el admin ingresa el marcador.
    /// </summary>
    Task RecalcularMarcadorExactoAsync(int partidoId);

    /// <summary>
    /// Recalcula puntos de posiciones finales y extras de fase eliminatoria
    /// (campeón, 2do, 3ro, 4to, más goleador, más goleado, menos goleado)
    /// cuando el admin actualiza ResultadoFinal.
    /// </summary>
    Task RecalcularResultadoFinalAsync();

    /// <summary>
    /// Resetea todos los puntos a 0 — usar cuando el admin borra todos los resultados.
    /// </summary>
    Task ResetearTodosPuntosAsync();
}