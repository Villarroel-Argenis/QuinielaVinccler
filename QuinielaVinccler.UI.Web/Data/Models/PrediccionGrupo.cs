namespace QuinielaVinccler.UI.Web.Data.Models;

public class PrediccionGrupo
{
    public int Id { get; set; }

    public int PlanillaId { get; set; }
    public Planilla Planilla { get; set; } = null!;

    public int PartidoId { get; set; }
    public Partido Partido { get; set; } = null!;

    // null hasta que el usuario llene la predicción
    public ResultadoPartido? ResultadoPredicho { get; set; }

    // null hasta que el partido se juegue
    public int? PuntosObtenidos { get; set; }
}