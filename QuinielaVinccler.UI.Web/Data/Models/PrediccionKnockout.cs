namespace QuinielaVinccler.UI.Web.Data.Models;

public class PrediccionKnockout
{
    public int Id { get; set; }

    public int PlanillaId { get; set; }
    public Planilla Planilla { get; set; } = null!;

    public int PartidoId { get; set; }
    public Partido Partido { get; set; } = null!;

    // null hasta que el usuario seleccione el equipo
    public int? EquipoPredichoId { get; set; }
    public Equipo? EquipoPredichado { get; set; }

    // null hasta que el partido se juegue
    public int? PuntosObtenidos { get; set; }
}