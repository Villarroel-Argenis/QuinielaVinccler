// Components/Shared/PlanillaModal.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using QuinielaVinccler.UI.Web.Data;
using QuinielaVinccler.UI.Web.Data.Models;
using QuinielaVinccler.UI.Web.Services;

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

    // Resultados reales para ✅/❌
    private Dictionary<int, ResultadoPartido?> _resultadosGrupo = [];
    private Dictionary<int, (int? Local, int? Visitante)> _resultadosR32 = [];
    private Dictionary<int, (int? Local, int? Visitante)> _resultadosKoSlots = [];
    private Dictionary<string, (int? Local, int? Visitante)?> _marcadoresSemis = [];

    // Lookup nombre por equipoId
    private Dictionary<int, string> _nombresEquipo = [];

    private static readonly (Fase Fase, string Label)[] _fasesKo =
    [
        (Fase.Cuartos,      "Cuartos"),
        (Fase.Semis,        "Semis"),
        (Fase.TercerPuesto, "3°/4°"),
        (Fase.Final,        "Final"),
    ];

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
        // ── Lookup nombres de equipos ─────────────────────────────────────────
        var equipos = await Db.Equipos.AsNoTracking()
            .Select(e => new { e.Id, e.Nombre })
            .ToListAsync();
        _nombresEquipo = equipos.ToDictionary(e => e.Id, e => e.Nombre);

        // ── Resultados de grupo ───────────────────────────────────────────────
        var idsGrupo = _planilla!.PrediccionesGrupo.Select(pg => pg.PartidoId).ToList();
        var pGrupo = await Db.Partidos.AsNoTracking()
            .Where(p => idsGrupo.Contains(p.Id))
            .Select(p => new { p.Id, p.ResultadoGrupo })
            .ToListAsync();
        _resultadosGrupo = pGrupo.ToDictionary(p => p.Id, p => p.ResultadoGrupo);

        // ── Resultados knockout ───────────────────────────────────────────────
        var idsKo = _planilla.PrediccionesKnockout.Select(pk => pk.PartidoId).ToList();
        var pKo = await Db.Partidos.AsNoTracking()
            .Where(p => idsKo.Contains(p.Id))
            .Select(p => new { p.Id, p.Fase, p.EquipoLocalId, p.EquipoVisitanteId })
            .ToListAsync();

        // R32: slots Local/Visitante para comparar predicción por slot
        _resultadosR32 = pKo
            .Where(p => p.Fase == Fase.RoundOf32)
            .ToDictionary(p => p.Id, p => (p.EquipoLocalId, p.EquipoVisitanteId));

        // R16+: misma estructura, diccionario separado
        _resultadosKoSlots = pKo
            .Where(p => p.Fase != Fase.RoundOf32)
            .ToDictionary(p => p.Id, p => (p.EquipoLocalId, p.EquipoVisitanteId));

        // ── Marcadores exactos SF1, SF2, GF ──────────────────────────────────
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

        // ── ResultadoFinal singleton ──────────────────────────────────────────
        _resultadoFinal = await Db.ResultadoFinal.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == 1);
    }

    // ── Color para botones 1/X/2 en grupos ───────────────────────────────────
    private MudBlazor.Color ResultadoRealColor(
        ResultadoPartido evaluar,
        ResultadoPartido? prediccion,
        ResultadoPartido? resultado)
    {
        if (evaluar != prediccion && evaluar != resultado) return MudBlazor.Color.Default;
        if (evaluar == resultado && evaluar == prediccion) return MudBlazor.Color.Success;
        if (evaluar == resultado)                          return MudBlazor.Color.Warning;
        return MudBlazor.Color.Error; // predijo este pero no era
    }

    // ── RenderFragment: fila de posición final ────────────────────────────────
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
        // Columna pts
        __builder.OpenElement(5, "td");
        __builder.AddAttribute(6, "style", $"font-weight:600;color:{(ok == true ? "var(--mud-palette-success)" : ok == false ? "var(--mud-palette-error)" : "inherit")}");
        __builder.AddContent(7, ptsStr);
        __builder.CloseElement();
        // Columna ícono
        __builder.OpenElement(8, "td");
        if (ok.HasValue)
        {
            __builder.OpenComponent<MudIcon>(9);
            __builder.AddAttribute(10, "Icon", ok.Value
                ? Icons.Material.Filled.CheckCircle
                : Icons.Material.Filled.Cancel);
            __builder.AddAttribute(11, "Color", ok.Value
                ? MudBlazor.Color.Success
                : MudBlazor.Color.Error);
            __builder.AddAttribute(12, "Size", MudBlazor.Size.Small);
            __builder.CloseComponent();
        }
        __builder.CloseElement();
        __builder.CloseElement();
    };

    // ── RenderFragment: fila de marcador exacto ───────────────────────────────
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
        __builder.AddAttribute(6, "style", $"font-weight:600;color:{(acierto == true ? "var(--mud-palette-success)" : acierto == false ? "var(--mud-palette-error)" : "inherit")}");
        __builder.AddContent(7, ptsStr);
        __builder.CloseElement();
        __builder.OpenElement(8, "td");
        if (acierto.HasValue)
        {
            __builder.OpenComponent<MudIcon>(9);
            __builder.AddAttribute(10, "Icon", acierto.Value
                ? Icons.Material.Filled.CheckCircle
                : Icons.Material.Filled.Cancel);
            __builder.AddAttribute(11, "Color", acierto.Value
                ? MudBlazor.Color.Success
                : MudBlazor.Color.Error);
            __builder.AddAttribute(12, "Size", MudBlazor.Size.Small);
            __builder.CloseComponent();
        }
        __builder.CloseElement();
        __builder.CloseElement();
    };

    private void Cerrar() => MudDialog.Close();
}
