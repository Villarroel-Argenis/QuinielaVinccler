namespace QuinielaVinccler.UI.Web.Components.Pages;

using Color = MudBlazor.Color;

public partial class PlanillaDetalle : ComponentBase
{
    [Parameter] public int PlanillaId { get; set; }

    [Inject] private IPredictionService PredSvc { get; set; } = null!;
    [Inject] private IConfiguracionService ConfigSvc { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState> AuthState { get; set; } = null!;

    // ── Datos ─────────────────────────────────────────────────────────────────
    private Planilla? _planilla;
    private List<Equipo> _equipos = [];

    private Dictionary<string, List<PrediccionGrupo>> _grupoMap = [];
    private List<PrediccionKnockout> _r32 = [], _r16 = [], _cuartos = [], _semis = [];
    private PrediccionKnockout? _tercero, _finalMatch;
    private PrediccionFinal? _pFinal;

    // ── UI ────────────────────────────────────────────────────────────────────
    private bool _cargando = true;
    private bool _soloLectura;
    private bool _showResetTotal;
    private bool _resetando;
    private string? _errorReset;
    private int _userId;
    private HashSet<string> _checkmarks = [];

    // ── Progreso ──────────────────────────────────────────────────────────────
    // 72 grupos + 64 knockout (2 por partido x 32 partidos) + 13 final = 149
    private const int TotalCampos = 149;
    private int _camposCompletos;

    // ── Init ──────────────────────────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        var userIdStr = state.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdStr, out _userId))
        {
            Nav.NavigateTo("/mis-planillas");
            return;
        }

        _planilla = await PredSvc.CargarPlanillaAsync(PlanillaId, _userId);

        if (_planilla is null)
        {
            Nav.NavigateTo("/mis-planillas");
            return;
        }

        _equipos = await PredSvc.GetEquiposAsync();
        _soloLectura = _planilla.Estado == EstadoPlanilla.Cerrada
                    || await ConfigSvc.QuinielaCerradaAsync();

        _pFinal = _planilla.PrediccionFinal;
        OrganizarPredicciones();
        _camposCompletos = CalcularProgreso();
        _cargando = false;
    }

    private void OrganizarPredicciones()
    {
        _grupoMap = _planilla!.PrediccionesGrupo
            .GroupBy(p => p.Partido.EquipoLocal!.Grupo)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Partido.NumeroPartido).ToList());

        var ko = _planilla.PrediccionesKnockout.ToList();
        _r32 = ko.Where(p => p.Partido.Fase == Fase.RoundOf32).OrderBy(p => p.Partido.NumeroPartido).ToList();
        _r16 = ko.Where(p => p.Partido.Fase == Fase.RoundOf16).OrderBy(p => p.Partido.NumeroPartido).ToList();
        _cuartos = ko.Where(p => p.Partido.Fase == Fase.Cuartos).OrderBy(p => p.Partido.NumeroPartido).ToList();
        _semis = ko.Where(p => p.Partido.Fase == Fase.Semis).OrderBy(p => p.Partido.NumeroPartido).ToList();
        _tercero = ko.FirstOrDefault(p => p.Partido.Fase == Fase.TercerPuesto);
        _finalMatch = ko.FirstOrDefault(p => p.Partido.Fase == Fase.Final);
    }

    private List<PrediccionKnockout> GetAllKo()
    {
        return _r32
            .Concat(_r16)
            .Concat(_cuartos)
            .Concat(_semis)
            .Concat(_tercero is null ? [] : [_tercero])
            .Concat(_finalMatch is null ? [] : [_finalMatch])
            .ToList();
    }
    // ── Progreso ──────────────────────────────────────────────────────────────
    private int CalcularProgreso()
    {
        int n = 0;

        // Grupos (72)
        n += _planilla!.PrediccionesGrupo.Count(p => p.ResultadoPredicho.HasValue);

        // Knockout: local + visitante por cada partido (64)
        var todosKo = _r32.Concat(_r16).Concat(_cuartos).Concat(_semis)
                          .Concat(_tercero is null ? [] : [_tercero])
                          .Concat(_finalMatch is null ? [] : [_finalMatch]);

        n += todosKo.Count(p => p.EquipoLocalPredichoId.HasValue);
        n += todosKo.Count(p => p.EquipoVisitantePredichoId.HasValue);

        // Final (13)
        if (_pFinal is not null)
        {
            if (_pFinal.CampeonEquipoId.HasValue) n++;
            if (_pFinal.SegundoLugarEquipoId.HasValue) n++;
            if (_pFinal.TercerLugarEquipoId.HasValue) n++;
            if (_pFinal.CuartoLugarEquipoId.HasValue) n++;
            if (_pFinal.MasGoleadorEquipoId.HasValue) n++;
            if (_pFinal.MasGoleadoEquipoId.HasValue) n++;
            if (_pFinal.MenosGoleadoEquipoId.HasValue) n++;
            if (_pFinal.GolesLocalGranFinal.HasValue) n++;
            if (_pFinal.GolesVisitanteGranFinal.HasValue) n++;
            if (_pFinal.GolesLocalSemi1.HasValue) n++;
            if (_pFinal.GolesVisitanteSemi1.HasValue) n++;
            if (_pFinal.GolesLocalSemi2.HasValue) n++;
            if (_pFinal.GolesVisitanteSemi2.HasValue) n++;
        }

        return n;
    }

    private async Task ActualizarProgreso(bool anteriorTeniaValor, bool nuevoTieneValor)
    {
        if (!anteriorTeniaValor && nuevoTieneValor) _camposCompletos++;
        else if (anteriorTeniaValor && !nuevoTieneValor) _camposCompletos--;

        if (_planilla is null || _planilla.Estado == EstadoPlanilla.Cerrada) return;

        var nuevoEstado = _camposCompletos >= TotalCampos
            ? EstadoPlanilla.Completa
            : EstadoPlanilla.EnProgreso;

        if (_planilla.Estado != nuevoEstado)
            await PredSvc.ActualizarEstadoAsync(_planilla.Id, nuevoEstado);
    }

    // ── Guardar grupos ────────────────────────────────────────────────────────
    internal async Task GuardarGrupo(PrediccionGrupo pred, ResultadoPartido? resultado)
    {
        if (_soloLectura) return;
        var ant = pred.ResultadoPredicho;
        pred.ResultadoPredicho = resultado;
        await PredSvc.GuardarGrupoAsync(pred.Id, resultado);
        await ActualizarProgreso(ant.HasValue, resultado.HasValue);
        await MostrarCheckmark($"g-{pred.Id}");
    }

    // ── Guardar knockout local ────────────────────────────────────────────────
    internal async Task GuardarKoLocal(PrediccionKnockout pred, int? equipoId)
    {
        if (_soloLectura) return;
        var ant = pred.EquipoLocalPredichoId;
        pred.EquipoLocalPredichoId = equipoId;
        pred.EquipoLocalPredichado = equipoId.HasValue ? _equipos.FirstOrDefault(e => e.Id == equipoId) : null;
        await PredSvc.GuardarR32LocalAsync(pred.Id, equipoId);
        await ActualizarProgreso(ant.HasValue, equipoId.HasValue);
        await MostrarCheckmark($"kol-{pred.Id}");
    }

    // ── Guardar knockout visitante ────────────────────────────────────────────
    internal async Task GuardarKoVisitante(PrediccionKnockout pred, int? equipoId)
    {
        if (_soloLectura) return;
        var ant = pred.EquipoVisitantePredichoId;
        pred.EquipoVisitantePredichoId = equipoId;
        pred.EquipoVisitantePredichado = equipoId.HasValue ? _equipos.FirstOrDefault(e => e.Id == equipoId) : null;
        await PredSvc.GuardarR32VisitanteAsync(pred.Id, equipoId);
        await ActualizarProgreso(ant.HasValue, equipoId.HasValue);
        await MostrarCheckmark($"kov-{pred.Id}");
    }

    // ── Guardar final ─────────────────────────────────────────────────────────
    internal async Task GuardarFinal(string campo, bool anteriorTeniaValor, bool nuevoTieneValor)
    {
        if (_soloLectura || _pFinal is null) return;
        await PredSvc.GuardarFinalAsync(_pFinal);
        await ActualizarProgreso(anteriorTeniaValor, nuevoTieneValor);
        await MostrarCheckmark($"f-{campo}");
    }

    // ── Checkmark ─────────────────────────────────────────────────────────────
    private async Task MostrarCheckmark(string key)
    {
        _checkmarks.Add(key);
        await InvokeAsync(StateHasChanged);
        await Task.Delay(1500);
        _checkmarks.Remove(key);
        await InvokeAsync(StateHasChanged);
    }

    // ── Candidatos por slot ───────────────────────────────────────────────────
    // Retorna los equipos disponibles para un slot dado
    internal List<Equipo> GetCandidatosParaSlot(
      string slot,
      int? excluirId = null,
      int? prediccionActualId = null)
    {
        var allKo = GetAllKo();

        List<Equipo> candidatos = [];

        // =====================================================
        // SLOTS DE MEJORES TERCEROS
        // =====================================================
        if (slot.StartsWith("3"))
        {
            var gruposPermitidos = slot[1..]
                .Select(c => c.ToString())
                .ToHashSet();

            // Grupos ya consumidos como mejor tercero
            var gruposYaUsados = allKo
                .Where(pk => pk.Id != prediccionActualId)
                .SelectMany(pk => new[]
                {
                new
                {
                    Slot = pk.Partido.SlotLocal,
                    Equipo = pk.EquipoLocalPredichado
                },
                new
                {
                    Slot = pk.Partido.SlotVisitante,
                    Equipo = pk.EquipoVisitantePredichado
                }
                })
                .Where(x =>
                    x.Equipo is not null &&
                    x.Slot.StartsWith("3"))
                .Select(x => x.Equipo!.Grupo)
                .ToHashSet();

            gruposPermitidos.ExceptWith(gruposYaUsados);

            candidatos = [.. _equipos.Where(e => gruposPermitidos.Contains(e.Grupo))];
        }

        // =====================================================
        // SLOTS NORMALES DE GRUPO (1A, 2B...)
        // =====================================================
        else if (
            slot.Length == 2 &&
            (slot[0] == '1' || slot[0] == '2'))
        {
            var grupo = slot[1].ToString();

            candidatos = [.. _equipos.Where(e => e.Grupo == grupo)];
        }

        // =====================================================
        // GANADOR DE PARTIDO
        // =====================================================
        else if (
            slot.StartsWith("G") &&
            int.TryParse(slot[1..], out var matchNum))
        {
            var src = allKo
                .FirstOrDefault(p =>
                    p.Partido.NumeroPartido == matchNum);

            if (src?.EquipoLocalPredichado is not null)
                candidatos.Add(src.EquipoLocalPredichado);

            if (src?.EquipoVisitantePredichado is not null)
                candidatos.Add(src.EquipoVisitantePredichado);
        }

        // =====================================================
        // PERDEDOR DE SEMIFINAL
        // =====================================================
        else if (
            slot.StartsWith("P") &&
            int.TryParse(slot[1..], out var semiNum))
        {
            var semi = _semis
                .FirstOrDefault(p =>
                    p.Partido.NumeroPartido == semiNum);

            if (semi?.EquipoLocalPredichado is not null)
                candidatos.Add(semi.EquipoLocalPredichado);

            if (semi?.EquipoVisitantePredichado is not null)
                candidatos.Add(semi.EquipoVisitantePredichado);
        }

        // =====================================================
        // EXCLUIR EQUIPOS YA USADOS
        // Solo aplica a slots de R32 (1X, 2X, 3XXXXX)
        // Los slots GXX y PXX se llenan por cascada
        // =====================================================
        if (slot.Length == 2 || slot.StartsWith("3"))
        {
            var equiposYaUsados = allKo
                .Where(pk => pk.Id != prediccionActualId)
                .SelectMany(pk => new[]
                {
            pk.EquipoLocalPredichoId,
            pk.EquipoVisitantePredichoId
                })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            candidatos = [.. candidatos.Where(e => !equiposYaUsados.Contains(e.Id))];
        }
        // =====================================================
        // EXCLUIR EL OTRO SLOT DEL MISMO PARTIDO
        // =====================================================
        if (excluirId.HasValue)
        {
            candidatos = [.. candidatos.Where(e => e.Id != excluirId.Value)];
        }

        return [.. candidatos
            .DistinctBy(e => e.Id)
            .OrderBy(e => e.Nombre)];
    }

    internal bool EsSlotMejorTercero(string slot)
    {
        return slot.StartsWith("3");
    }

    // ── Reset total ───────────────────────────────────────────────────────────
    internal void AbrirResetTotal() => _showResetTotal = true;

    internal async Task ConfirmarResetTotal()
    {
        _resetando = true;
        _errorReset = null;

        try
        {
            await PredSvc.ResetTotalAsync(_planilla!.Id);
            _planilla = await PredSvc.CargarPlanillaAsync(PlanillaId, _userId);
            if (_planilla is not null)
            {
                _pFinal = _planilla.PrediccionFinal;
                OrganizarPredicciones();
                _camposCompletos = 0;
            }
            _showResetTotal = false;
        }
        catch
        {
            _errorReset = "Error al resetear. Intenta de nuevo.";
        }
        finally
        {
            _resetando = false;
        }
    }

    internal void CerrarResetTotal()
    {
        _showResetTotal = false;
        _errorReset = null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    internal static string GetLabelResultado(ResultadoPartido r) => r switch
    {
        ResultadoPartido.Uno => "1",
        ResultadoPartido.Equis => "X",
        ResultadoPartido.Dos => "2",
        _ => ""
    };

    internal bool TieneCheckmark(string key) => _checkmarks.Contains(key);
}