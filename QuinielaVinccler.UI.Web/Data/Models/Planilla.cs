namespace QuinielaVinccler.UI.Web.Data.Models;

public class Planilla
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";         // P-12345678

    public int? UserId { get; set; }
    public AppUser? User { get; set; }

    public int? LoteId { get; set; }
    public Lote? Lote { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }

    public EstadoPlanilla Estado { get; set; } = EstadoPlanilla.SinAsignar;
    public int PuntajeTotal { get; set; }

    // Computed — no mapeado a la DB
    public bool IsAssigned => UserId is not null;

    // Navegación
    public ICollection<PrediccionGrupo> PrediccionesGrupo { get; set; } = [];
    public ICollection<PrediccionKnockout> PrediccionesKnockout { get; set; } = [];
    public PrediccionFinal? PrediccionFinal { get; set; }
}