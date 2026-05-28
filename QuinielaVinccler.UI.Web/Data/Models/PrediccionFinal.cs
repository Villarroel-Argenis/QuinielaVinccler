namespace QuinielaVinccler.UI.Web.Data.Models;

public class PrediccionFinal
{
    public int Id { get; set; }

    public int PlanillaId { get; set; }
    public Planilla Planilla { get; set; } = null!;

    // ── Posiciones finales (independientes del bracket) ─────────────────────
    public int? CampeonEquipoId { get; set; }           // 300 pts
    public Equipo? Campeon { get; set; }

    public int? SegundoLugarEquipoId { get; set; }      // 200 pts
    public Equipo? SegundoLugar { get; set; }

    public int? TercerLugarEquipoId { get; set; }       // 100 pts
    public Equipo? TercerLugar { get; set; }

    public int? CuartoLugarEquipoId { get; set; }       // 50 pts
    public Equipo? CuartoLugar { get; set; }

    // ── Extras fase de grupos ────────────────────────────────────────────────
    public int? MasGoleadorEquipoId { get; set; }       // 100 pts
    public Equipo? MasGoleador { get; set; }

    public int? MasGoleadoEquipoId { get; set; }        // 100 pts
    public Equipo? MasGoleado { get; set; }

    public int? MenosGoleadoEquipoId { get; set; }      // 100 pts
    public Equipo? MenosGoleado { get; set; }

    // ── Resultados exactos a 90 minutos ─────────────────────────────────────
    public int? GolesLocalGranFinal { get; set; }       // 100 pts si ambos aciertan
    public int? GolesVisitanteGranFinal { get; set; }

    public int? GolesLocalSemi1 { get; set; }           // 100 pts
    public int? GolesVisitanteSemi1 { get; set; }

    public int? GolesLocalSemi2 { get; set; }           // 100 pts
    public int? GolesVisitanteSemi2 { get; set; }
}