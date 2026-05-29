namespace QuinielaVinccler.UI.Web.Data.Models;

public class Partido
{
    public int Id { get; set; }
    public int NumeroPartido { get; set; }       // 1-104

    public Fase Fase { get; set; }
    public DateTime FechaHoraUtc { get; set; }

    // Slots de bracket para eliminatoria ("2A", "1C", "3ABCDF", "G73")
    // Para fase de grupos queda vacío — se usan las FK directamente
    public string SlotLocal { get; set; } = "";
    public string SlotVisitante { get; set; } = "";

    // Equipos concretos — null hasta que se definan en eliminatoria
    public int? EquipoLocalId { get; set; }
    public Equipo? EquipoLocal { get; set; }

    public int? EquipoVisitanteId { get; set; }
    public Equipo? EquipoVisitante { get; set; }

    // Resultado fase de grupos (1/X/2)
    public ResultadoPartido? ResultadoGrupo { get; set; }

    // Ganador eliminatoria
    public int? EquipoGanadorId { get; set; }
    public Equipo? EquipoGanador { get; set; }

    // Marcador exacto a 90 minutos — solo relevante para #101, #102 y #104
    // (Semis y Gran Final — para cálculo de 100pts de resultado exacto)
    public int? GolesLocal { get; set; }
    public int? GolesVisitante { get; set; }
}