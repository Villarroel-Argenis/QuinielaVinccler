namespace QuinielaVinccler.UI.Web.Components.Shared;

public partial class KnockoutRow : ComponentBase
{
    // ── Parámetros de datos ───────────────────────────────────────────────────

    [Parameter, EditorRequired]
    public PrediccionKnockout Pred { get; set; } = null!;

    [Parameter, EditorRequired]
    public Dictionary<int, Equipo> EquiposById { get; set; } = [];

    /// <summary>
    /// Delegado que devuelve los candidatos para un slot dado.
    /// Firma: (slot, excluirId, prediccionActualId) → List&lt;Equipo&gt;
    /// </summary>
    [Parameter, EditorRequired]
    public Func<string, int?, int?, List<Equipo>> GetCandidatos { get; set; } = null!;

    // ── Parámetros de estado UI ───────────────────────────────────────────────

    [Parameter] public bool SoloLectura { get; set; }
    [Parameter] public bool MostrarCheckmarkLocal { get; set; }
    [Parameter] public bool MostrarCheckmarkVisitante { get; set; }

    // ── Callbacks hacia el padre ──────────────────────────────────────────────

    [Parameter] public EventCallback<int?> OnLocalChanged { get; set; }
    [Parameter] public EventCallback<int?> OnVisitanteChanged { get; set; }

    // ── Candidatos calculados ─────────────────────────────────────────────────

    public IReadOnlyList<EquipoSelectItem> CandidatosLocalAgrupados =>
        BuildItems(
            slot: Pred.Partido.SlotLocal,
            excluirId: Pred.EquipoVisitantePredichoId,
            prediccionId: Pred.Id);

    public IReadOnlyList<EquipoSelectItem> CandidatosVisitanteAgrupados =>
        BuildItems(
            slot: Pred.Partido.SlotVisitante,
            excluirId: Pred.EquipoLocalPredichoId,
            prediccionId: Pred.Id);

    // ── Builder ───────────────────────────────────────────────────────────────

    private IReadOnlyList<EquipoSelectItem> BuildItems(string slot, int? excluirId, int prediccionId)
    {
        var candidatos = GetCandidatos(slot, excluirId, prediccionId);

        // Slots de mejor tercero: agrupar con headers
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

        // Resto de slots: lista plana
        return candidatos
            .Select(e => new EquipoSelectItem(EsHeader: false, Equipo: e))
            .ToList();
    }
}
