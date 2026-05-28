namespace QuinielaVinccler.UI.Web.Data.Models;

public class PrediccionKnockout
{
    public int Id { get; set; }

    public int PlanillaId { get; set; }
    public Planilla Planilla { get; set; } = null!;

    public int PartidoId { get; set; }
    public Partido Partido { get; set; } = null!;

    // ── R32: el usuario predice qué equipo ocupa cada slot ───────────────────
    // Solo aplica a partidos de Fase.RoundOf32
    public int? EquipoLocalPredichoId { get; set; }
    public Equipo? EquipoLocalPredichado { get; set; }

    public int? EquipoVisitantePredichoId { get; set; }
    public Equipo? EquipoVisitantePredichado { get; set; }

    // ── R16+: el usuario predice el ganador del partido ──────────────────────
    // Solo aplica a RoundOf16, Cuartos, Semis, TercerPuesto, Final
    public int? EquipoGanadorId { get; set; }
    public Equipo? EquipoGanador { get; set; }

    // null hasta que el partido se juegue
    public int? PuntosObtenidos { get; set; }
}