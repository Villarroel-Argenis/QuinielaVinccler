// Components/Shared/PlanillaModal.razor.cs
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

    private Dictionary<int, ResultadoPartido?> _resultadosGrupo = [];
    private Dictionary<int, (int? Local, int? Visitante)> _resultadosR32 = [];
    private Dictionary<int, (int? Local, int? Visitante)> _resultadosKoSlots = [];
    private Dictionary<string, (int? Local, int? Visitante)?> _marcadoresSemis = [];
    private Dictionary<int, string> _nombresEquipo = [];

    private static readonly (Fase Fase, string Label)[] _fasesKo =
    [
        (Fase.Cuartos,      "Cuartos"),
        (Fase.Semis,        "Semis"),
        (Fase.TercerPuesto, "3°/4°"),
        (Fase.Final,        "Final"),
    ];

    // ── Indicadores de progreso ───────────────────────────────────────────────
    private int TotalCampos => _planilla is null ? 0 :
        _planilla.PrediccionesGrupo.Count +
        _planilla.PrediccionesKnockout.Count * 2 +
        13; // 7 posiciones + 6 campos de marcador (3 partidos × 2)

    private int CamposPredichos
    {
        get
        {
            if (_planilla is null) return 0;
            try
            {
                int total = 0;
                total += _planilla.PrediccionesGrupo?.Count(pg => pg.ResultadoPredicho.HasValue) ?? 0;
                total += _planilla.PrediccionesKnockout?.Count(pk => pk.EquipoLocalPredichoId.HasValue) ?? 0;
                total += _planilla.PrediccionesKnockout?.Count(pk => pk.EquipoVisitantePredichoId.HasValue) ?? 0;
                if (_planilla.PrediccionFinal is { } pf)
                {
                    if (pf.CampeonEquipoId.HasValue) total++;
                    if (pf.SegundoLugarEquipoId.HasValue) total++;
                    if (pf.TercerLugarEquipoId.HasValue) total++;
                    if (pf.CuartoLugarEquipoId.HasValue) total++;
                    if (pf.MasGoleadorEquipoId.HasValue) total++;
                    if (pf.MasGoleadoEquipoId.HasValue) total++;
                    if (pf.MenosGoleadoEquipoId.HasValue) total++;
                    if (pf.GolesLocalSemi1.HasValue) total++;
                    if (pf.GolesVisitanteSemi1.HasValue) total++;
                    if (pf.GolesLocalSemi2.HasValue) total++;
                    if (pf.GolesVisitanteSemi2.HasValue) total++;
                    if (pf.GolesLocalGranFinal.HasValue) total++;
                    if (pf.GolesVisitanteGranFinal.HasValue) total++;
                }
                return total;
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> CamposPredichos ERROR: {ex.Message}");
                return -1;
            }
        }
    }

    private int TotalConResultado =>
        _resultadosGrupo.Count(r => r.Value.HasValue) +
        _resultadosR32.Count(r => r.Value.Local.HasValue) +
        _resultadosR32.Count(r => r.Value.Visitante.HasValue) +
        _resultadosKoSlots.Count(r => r.Value.Local.HasValue) +
        _resultadosKoSlots.Count(r => r.Value.Visitante.HasValue);

    private int PorcentajePrediccion => TotalCampos == 0 ? 0 :
        (int)Math.Round(CamposPredichos * 100.0 / TotalCampos);

    private int PorcentajeResultados => TotalCampos == 0 ? 0 :
        (int)Math.Round(TotalConResultado * 100.0 / TotalCampos);

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    protected override async Task OnParametersSetAsync()
    {
        _cargando = true;
        _planilla = await PredSvc.CargarPlanillaAsync(PlanillaId, UserId);

        if (_planilla is not null)
            await CargarResultadosRealesAsync();

        _cargando = false;
    }

    private async Task CargarResultadosRealesAsync()
    {
        // Lookup nombres
        var equipos = await Db.Equipos.AsNoTracking()
            .Select(e => new { e.Id, e.Nombre })
            .ToListAsync();
        _nombresEquipo = equipos.ToDictionary(e => e.Id, e => e.Nombre);

        // Resultados de grupo
        var idsGrupo = _planilla!.PrediccionesGrupo.Select(pg => pg.PartidoId).ToList();
        var pGrupo = await Db.Partidos.AsNoTracking()
            .Where(p => idsGrupo.Contains(p.Id))
            .Select(p => new { p.Id, p.ResultadoGrupo })
            .ToListAsync();
        _resultadosGrupo = pGrupo.ToDictionary(p => p.Id, p => p.ResultadoGrupo);

        // Resultados knockout
        var idsKo = _planilla.PrediccionesKnockout.Select(pk => pk.PartidoId).ToList();
        var pKo = await Db.Partidos.AsNoTracking()
            .Where(p => idsKo.Contains(p.Id))
            .Select(p => new { p.Id, p.Fase, p.EquipoLocalId, p.EquipoVisitanteId })
            .ToListAsync();

        _resultadosR32 = pKo
            .Where(p => p.Fase == Fase.RoundOf32)
            .ToDictionary(p => p.Id, p => (p.EquipoLocalId, p.EquipoVisitanteId));

        _resultadosKoSlots = pKo
            .Where(p => p.Fase != Fase.RoundOf32)
            .ToDictionary(p => p.Id, p => (p.EquipoLocalId, p.EquipoVisitanteId));

        // Marcadores exactos
        var marcadores = await Db.Partidos.AsNoTracking()
            .Where(p => p.NumeroPartido == 101 || p.NumeroPartido == 102 || p.NumeroPartido == 104)
            .Select(p => new { p.NumeroPartido, p.GolesLocal, p.GolesVisitante })
            .ToListAsync();

        foreach (var m in marcadores)
        {
            if (m.NumeroPartido == 101 && m.GolesLocal.HasValue)
                _marcadoresSemis["SF1"] = (m.GolesLocal, m.GolesVisitante);
            else if (m.NumeroPartido == 102 && m.GolesLocal.HasValue)
                _marcadoresSemis["SF2"] = (m.GolesLocal, m.GolesVisitante);
            else if (m.NumeroPartido == 104 && m.GolesLocal.HasValue)
                _marcadoresSemis["GF"] = (m.GolesLocal, m.GolesVisitante);
        }

        // ResultadoFinal singleton
        _resultadoFinal = await Db.ResultadoFinal.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == 1);
    }

    // ── Color chips grupos ────────────────────────────────────────────────────
    private MudBlazor.Color ResultadoRealColor(
        ResultadoPartido evaluar,
        ResultadoPartido? prediccion,
        ResultadoPartido? resultado)
    {
        if (evaluar != prediccion && evaluar != resultado) return MudBlazor.Color.Default;
        if (evaluar == resultado && evaluar == prediccion) return MudBlazor.Color.Success;
        if (evaluar == resultado)                          return MudBlazor.Color.Warning;
        return MudBlazor.Color.Error;
    }

    // ── RenderFragment: fila posición final ──────────────────────────────────
    private RenderFragment RenderFilaFinal(string label, int? predichoId, int? realId, int ptsMaximos) => __builder =>
    {
        var nombre = predichoId.HasValue
            ? _nombresEquipo.GetValueOrDefault(predichoId.Value, $"#{predichoId}")
            : "—";

        bool? ok = (predichoId.HasValue && realId.HasValue) ? predichoId == realId : null;
        string ptsStr = ok.HasValue ? (ok.Value ? $"+{ptsMaximos}" : "0") : "—";

        __builder.OpenElement(0, "tr");
        __builder.OpenElement(1, "td"); __builder.AddContent(2, label); __builder.CloseElement();
        __builder.OpenElement(3, "td"); __builder.AddContent(4, nombre); __builder.CloseElement();
        __builder.OpenElement(5, "td");
        __builder.AddAttribute(6, "style",
            $"font-weight:600;color:{(ok == true ? "var(--mud-palette-success)" : ok == false ? "var(--mud-palette-error)" : "inherit")}");
        __builder.AddContent(7, ptsStr);
        __builder.CloseElement();
        __builder.OpenElement(8, "td");
        if (ok.HasValue)
        {
            __builder.OpenComponent<MudIcon>(9);
            __builder.AddAttribute(10, "Icon", ok.Value
                ? Icons.Material.Filled.CheckCircle
                : Icons.Material.Filled.Cancel);
            __builder.AddAttribute(11, "Color", ok.Value ? MudBlazor.Color.Success : MudBlazor.Color.Error);
            __builder.AddAttribute(12, "Size", MudBlazor.Size.Small);
            __builder.CloseComponent();
        }
        __builder.CloseElement();
        __builder.CloseElement();
    };

    // ── RenderFragment: fila marcador exacto ─────────────────────────────────
    private RenderFragment RenderFilaMarcador(
        string label,
        int? golesLocalPred, int? golesVisitantePred,
        (int? Local, int? Visitante)? real,
        int? ptsObtenidos) => __builder =>
    {
        var pred = (golesLocalPred.HasValue && golesVisitantePred.HasValue)
            ? $"{golesLocalPred} - {golesVisitantePred}"
            : "—";

        bool? acierto = null;
        if (real.HasValue && golesLocalPred.HasValue && golesVisitantePred.HasValue)
            acierto = golesLocalPred == real.Value.Local && golesVisitantePred == real.Value.Visitante;

        string ptsStr = ptsObtenidos.HasValue ? (ptsObtenidos > 0 ? $"+{ptsObtenidos}" : "0") : "—";

        __builder.OpenElement(0, "tr");
        __builder.OpenElement(1, "td"); __builder.AddContent(2, label); __builder.CloseElement();
        __builder.OpenElement(3, "td"); __builder.AddContent(4, pred); __builder.CloseElement();
        __builder.OpenElement(5, "td");
        __builder.AddAttribute(6, "style",
            $"font-weight:600;color:{(acierto == true ? "var(--mud-palette-success)" : acierto == false ? "var(--mud-palette-error)" : "inherit")}");
        __builder.AddContent(7, ptsStr);
        __builder.CloseElement();
        __builder.OpenElement(8, "td");
        if (acierto.HasValue)
        {
            __builder.OpenComponent<MudIcon>(9);
            __builder.AddAttribute(10, "Icon", acierto.Value
                ? Icons.Material.Filled.CheckCircle
                : Icons.Material.Filled.Cancel);
            __builder.AddAttribute(11, "Color", acierto.Value ? MudBlazor.Color.Success : MudBlazor.Color.Error);
            __builder.AddAttribute(12, "Size", MudBlazor.Size.Small);
            __builder.CloseComponent();
        }
        __builder.CloseElement();
        __builder.CloseElement();
    };

    private void Cerrar() => MudDialog.Close();
}
