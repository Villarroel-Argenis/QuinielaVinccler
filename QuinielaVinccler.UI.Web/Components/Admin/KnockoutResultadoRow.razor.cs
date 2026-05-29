namespace QuinielaVinccler.UI.Web.Components.Admin;

public partial class KnockoutResultadoRow : ComponentBase
{
    /// <summary>El partido con equipos ya resueltos (EquipoLocal y EquipoVisitante cargados).</summary>
    [Parameter, EditorRequired] public Partido Pred { get; set; } = null!;

    /// <summary>Id del equipo ganador actualmente seleccionado (puede ser null).</summary>
    [Parameter, EditorRequired] public int? GanadorId { get; set; }

    /// <summary>Callback cuando el admin selecciona o limpia el ganador.</summary>
    [Parameter, EditorRequired] public EventCallback<int?> OnGanadorChanged { get; set; }
}
