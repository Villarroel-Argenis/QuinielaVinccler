namespace QuinielaVinccler.UI.Web.Components.Admin;

using QuinielaVinccler.UI.Web.Components.Shared;

public partial class R32ResultadoRow : ComponentBase
{
    [Parameter, EditorRequired] public Partido Partido { get; set; } = null!;
    [Parameter, EditorRequired] public Dictionary<int, Equipo> EquiposById { get; set; } = [];
    [Parameter, EditorRequired] public Func<string, int?, int?, List<Equipo>> GetCandidatos { get; set; } = null!;

    [Parameter] public int? EquipoLocalId { get; set; }
    [Parameter] public int? EquipoVisitanteId { get; set; }

    [Parameter] public EventCallback<int?> OnEquipoLocalChanged { get; set; }
    [Parameter] public EventCallback<int?> OnEquipoVisitanteChanged { get; set; }

    public IReadOnlyList<EquipoSelectItem> CandidatosLocal =>
        BuildItems(Partido.SlotLocal, EquipoVisitanteId, Partido.NumeroPartido);

    public IReadOnlyList<EquipoSelectItem> CandidatosVisitante =>
        BuildItems(Partido.SlotVisitante, EquipoLocalId, Partido.NumeroPartido);

    private IReadOnlyList<EquipoSelectItem> BuildItems(string slot, int? excluirId, int partidoNumero)
    {
        var candidatos = GetCandidatos(slot, excluirId, partidoNumero);

        if (slot.StartsWith("3"))
        {
            var items = new List<EquipoSelectItem>();
            foreach (var grp in candidatos.GroupBy(e => e.Grupo).OrderBy(g => g.Key))
            {
                var headerValue = -1000 - (int)grp.Key[0];
                items.Add(new EquipoSelectItem(EsHeader: true, Grupo: grp.Key, HeaderValue: headerValue));
                items.AddRange(grp.Select(e => new EquipoSelectItem(EsHeader: false, Equipo: e)));
            }
            return items;
        }

        return candidatos
            .Select(e => new EquipoSelectItem(EsHeader: false, Equipo: e))
            .ToList();
    }
}
