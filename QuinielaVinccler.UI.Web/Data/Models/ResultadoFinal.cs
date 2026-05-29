namespace QuinielaVinccler.UI.Web.Data.Models;

/// <summary>
/// Tabla singleton — existe una sola fila con los resultados reales del torneo.
/// El admin la completa progresivamente conforme avanza el torneo.
/// </summary>
public class ResultadoFinal
{
    public int Id { get; set; }

    // ── Posiciones finales ────────────────────────────────────────────────────
    public int? CampeonEquipoId { get; set; }
    public Equipo? Campeon { get; set; }

    public int? SegundoLugarEquipoId { get; set; }
    public Equipo? SegundoLugar { get; set; }

    public int? TercerLugarEquipoId { get; set; }
    public Equipo? TercerLugar { get; set; }

    public int? CuartoLugarEquipoId { get; set; }
    public Equipo? CuartoLugar { get; set; }

    // ── Extras fase eliminatoria ──────────────────────────────────────────────
    public int? MasGoleadorEquipoId { get; set; }
    public Equipo? MasGoleador { get; set; }

    public int? MasGoleadoEquipoId { get; set; }
    public Equipo? MasGoleado { get; set; }

    public int? MenosGoleadoEquipoId { get; set; }
    public Equipo? MenosGoleado { get; set; }
}