namespace QuinielaVinccler.UI.Web.Components.Shared;

public partial class FinalEquipoRow : ComponentBase
{
    [Parameter, EditorRequired] public string Label { get; set; } = "";
    [Parameter, EditorRequired] public int? ValorActual { get; set; }
    [Parameter, EditorRequired] public List<Equipo> Equipos { get; set; } = [];
    [Parameter, EditorRequired] public Dictionary<int, Equipo> EquiposById { get; set; } = [];

    [Parameter] public bool SoloLectura { get; set; }
    [Parameter] public bool MostrarCheckmark { get; set; }

    [Parameter] public EventCallback<int?> OnValorChanged { get; set; }

    /// <summary>
    /// Convierte la lista plana de equipos a EquipoSelectItem para MudEquipoSelect.
    /// Sin headers de grupo — lista completa de 48 equipos sin restricciones.
    /// </summary>
    public IReadOnlyList<EquipoSelectItem> ItemsPlanos =>
        Equipos
            .Select(e => new EquipoSelectItem(EsHeader: false, Equipo: e))
            .ToList();
}
