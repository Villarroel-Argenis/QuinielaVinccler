namespace QuinielaVinccler.UI.Web.Components.Admin;

using QuinielaVinccler.UI.Web.Components.Shared;

public partial class KnockoutResultadoRow : ComponentBase
{
    [Parameter, EditorRequired] public Partido Pred { get; set; } = null!;
    [Parameter, EditorRequired] public Dictionary<int, Equipo> EquiposById { get; set; } = [];
    [Parameter, EditorRequired] public Func<string, int?, int?, List<Equipo>> GetCandidatos { get; set; } = null!;

    [Parameter] public int? EquipoLocalId { get; set; }
    [Parameter] public int? EquipoVisitanteId { get; set; }

    [Parameter] public EventCallback<int?> OnEquipoLocalChanged { get; set; }
    [Parameter] public EventCallback<int?> OnEquipoVisitanteChanged { get; set; }

    public IReadOnlyList<EquipoSelectItem> CandidatosLocal =>
        GetCandidatos(Pred.SlotLocal, EquipoVisitanteId, Pred.NumeroPartido)
            .Select(e => new EquipoSelectItem(EsHeader: false, Equipo: e))
            .ToList();

    public IReadOnlyList<EquipoSelectItem> CandidatosVisitante =>
        GetCandidatos(Pred.SlotVisitante, EquipoLocalId, Pred.NumeroPartido)
            .Select(e => new EquipoSelectItem(EsHeader: false, Equipo: e))
            .ToList();
}
