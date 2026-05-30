// Components/Shared/PlanillaModal.razor.cs
using System.Reflection.Metadata.Ecma335;

namespace QuinielaVinccler.UI.Web.Components.Shared;

public partial class PlanillaModal : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public int PlanillaId { get; set; }
    [Parameter] public int UserId { get; set; }
    [Parameter] public string CodigoPlanilla { get; set; } = "";

    [Inject] private IPredictionService PredSvc { get; set; } = null!;
    [Inject] private AppDbContext Db { get; set; } = null!;

    private bool _cargando = true;
    private Planilla? _planilla;
    private ResultadoFinal? _resultadoFinal;

    // Resultados reales cacheados para mostrar ✅/❌
    private Dictionary<int, ResultadoPartido?> _resultadosGrupo = [];
    private Dictionary<int, int?> _resultadosKo = [];
    private Dictionary<string, (int? Local, int? Visitante)?> _marcadoresSemis = [];

    private static readonly (Fase Fase, string Label)[] _fasesKo =
    [
        (Fase.Cuartos, "Cuartos"),
        (Fase.Semis, "Semis"),
        (Fase.TercerPuesto, "3°/4°"),
        (Fase.Final, "Final"),
    ];

    protected override async Task OnParametersSetAsync()
    {
        _cargando = true;
        // userId = 0 → sin restricción de usuario (cualquier planilla)
        _planilla = await PredSvc.CargarPlanillaAsync(PlanillaId, UserId);

        if (_planilla is not null)
        {
            await CargarResultadosRealesAsync();
        }

        _cargando = false;
    }

    private async Task CargarResultadosRealesAsync()
    {
        // Resultados de grupo
        var idsGrupo = _planilla!.PrediccionesGrupo.Select(pg => pg.PartidoId).ToList();
        var partidos = await Db.Partidos
            .AsNoTracking()
            .Where(p => idsGrupo.Contains(p.Id))
            .Select(p => new { p.Id, p.ResultadoGrupo })
            .ToListAsync();

        _resultadosGrupo = partidos.ToDictionary(p => p.Id, p => p.ResultadoGrupo);

        // Resultados knockout (ganador)
        var idsKo = _planilla.PrediccionesKnockout.Select(pk => pk.PartidoId).ToList();
        var partidosKo = await Db.Partidos
            .AsNoTracking()
            .Where(p => idsKo.Contains(p.Id))
            .Select(p => new { p.Id, p.EquipoGanadorId })
            .ToListAsync();

        _resultadosKo = partidosKo.ToDictionary(p => p.Id, p => p.EquipoGanadorId);

        // Marcadores exactos (SF1, SF2, GF)
        var semis = await Db.Partidos
            .AsNoTracking()
            .Where(p => p.Fase == Fase.Semis || p.Fase == Fase.Final)
            .OrderBy(p => p.NumeroPartido)
            .Select(p => new { p.Fase, p.GolesLocal, p.GolesVisitante })
            .ToListAsync();

        var sf = semis.Where(p => p.Fase == Fase.Semis).ToList();
        var gf = semis.FirstOrDefault(p => p.Fase == Fase.Final);

        if (sf.Count >= 1 && sf[0].GolesLocal.HasValue)
            _marcadoresSemis["SF1"] = (sf[0].GolesLocal, sf[0].GolesVisitante);
        if (sf.Count >= 2 && sf[1].GolesLocal.HasValue)
            _marcadoresSemis["SF2"] = (sf[1].GolesLocal, sf[1].GolesVisitante);
        if (gf?.GolesLocal.HasValue == true)
            _marcadoresSemis["GF"] = (gf.GolesLocal, gf.GolesVisitante);

        // ResultadoFinal singleton
        _resultadoFinal = await Db.ResultadoFinal
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == 1);
    }

    private RenderFragment RenderFilaFinal(string label, int? predichoId, int? realId) => __builder =>
    {
        var nombrePredecho = predichoId.HasValue
            ? _planilla!.PrediccionesKnockout
                .SelectMany(pk => new[] { pk.EquipoLocalPredichado, pk.EquipoVisitantePredichado, pk.EquipoGanador })
                .Concat(_planilla.PrediccionesGrupo.SelectMany(pg => new[] { pg.Partido.EquipoLocal, pg.Partido.EquipoVisitante }))
                .FirstOrDefault(e => e?.Id == predichoId)?.Nombre ?? $"#{predichoId}"
            : "—";

        __builder.OpenElement(0, "tr");
        __builder.OpenElement(1, "td"); __builder.AddContent(2, label); __builder.CloseElement();
        __builder.OpenElement(3, "td"); __builder.AddContent(4, nombrePredecho); __builder.CloseElement();
        __builder.OpenElement(5, "td");
        if (predichoId.HasValue && realId.HasValue)
        {
            // Emitir ícono según acierto
            __builder.OpenComponent<MudIcon>(6);
            __builder.AddAttribute(7, "Icon", predichoId == realId
                ? Icons.Material.Filled.CheckCircle
                : Icons.Material.Filled.Cancel);
            __builder.AddAttribute(8, "Color", predichoId == realId ? MudBlazor.Color.Success : MudBlazor.Color.Error);
            __builder.AddAttribute(9, "Size", MudBlazor.Size.Small);
            __builder.CloseComponent();
        }
        __builder.CloseElement();
        __builder.CloseElement();
    };

    private RenderFragment RenderFilaMarcador(
        string label,
        int? golesLocalPred, int? golesVisitantePred,
        (int? Local, int? Visitante)? real) => __builder =>
    {
        var pred = (golesLocalPred.HasValue && golesVisitantePred.HasValue)
            ? $"{golesLocalPred} - {golesVisitantePred}"
            : "—";

        bool? acierto = null;
        if (real.HasValue && golesLocalPred.HasValue && golesVisitantePred.HasValue)
            acierto = golesLocalPred == real.Value.Local && golesVisitantePred == real.Value.Visitante;

        __builder.OpenElement(0, "tr");
        __builder.OpenElement(1, "td"); __builder.AddContent(2, label); __builder.CloseElement();
        __builder.OpenElement(3, "td"); __builder.AddContent(4, pred); __builder.CloseElement();
        __builder.OpenElement(5, "td");
        if (acierto.HasValue)
        {
            __builder.OpenComponent<MudIcon>(6);
            __builder.AddAttribute(7, "Icon", acierto.Value
                ? Icons.Material.Filled.CheckCircle
                : Icons.Material.Filled.Cancel);
            __builder.AddAttribute(8, "Color", acierto.Value ? MudBlazor.Color.Success : MudBlazor.Color.Error);
            __builder.AddAttribute(9, "Size", MudBlazor.Size.Small);
            __builder.CloseComponent();
        }
        __builder.CloseElement();
        __builder.CloseElement();
    };

    private void Cerrar() => MudDialog.Close();

    private MudBlazor.Color? ResultadoRealColor(ResultadoPartido evaluar, ResultadoPartido? prediccion, ResultadoPartido? resultado)
    {
        if (resultado == evaluar) return MudBlazor.Color.Primary;
        if (prediccion != evaluar) return MudBlazor.Color.Default;
        if (prediccion == resultado) return MudBlazor.Color.Primary;
        

        return MudBlazor.Color.Error; // Deshabilitado para no revelar resultados reales en la modal
    }
}
