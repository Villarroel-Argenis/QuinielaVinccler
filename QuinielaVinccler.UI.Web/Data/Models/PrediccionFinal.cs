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

    // ── Extras fase eliminatoria ─────────────────────────────────────────────
    public int? MasGoleadorEquipoId { get; set; }       // 100 pts
    public Equipo? MasGoleador { get; set; }

    public int? MasGoleadoEquipoId { get; set; }        // 100 pts
    public Equipo? MasGoleado { get; set; }

    public int? MenosGoleadoEquipoId { get; set; }      // 100 pts
    public Equipo? MenosGoleado { get; set; }

    // ── Resultados exactos a 90 minutos ─────────────────────────────────────
    public int? GolesLocalGranFinal { get; set; }
    public int? GolesVisitanteGranFinal { get; set; }

    public int? GolesLocalSemi1 { get; set; }
    public int? GolesVisitanteSemi1 { get; set; }

    public int? GolesLocalSemi2 { get; set; }
    public int? GolesVisitanteSemi2 { get; set; }

    // ── Puntos desglosados (calculados por PuntuacionService) ────────────────
    // Separados para poder recalcular cada sección de forma independiente
    // sin afectar los demás cuando el admin actualiza un campo específico.

    /// <summary>Suma de pts de campeón + 2do + 3ro + 4to + 3 extras.</summary>
    public int? PuntosPosicionesFinal { get; set; }

    /// <summary>100 pts si el marcador exacto del partido #101 a 90min es correcto.</summary>
    public int? PuntosMarcadorSemi1 { get; set; }

    /// <summary>100 pts si el marcador exacto del partido #102 a 90min es correcto.</summary>
    public int? PuntosMarcadorSemi2 { get; set; }

    /// <summary>100 pts si el marcador exacto del partido #104 a 90min es correcto.</summary>
    public int? PuntosMarcadorGranFinal { get; set; }
}