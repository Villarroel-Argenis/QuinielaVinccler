namespace QuinielaVinccler.UI.Web.Components.Admin;

public partial class ResultadosComponent : ComponentBase
{
    [Inject] private IPuntuacionService PuntuacionSvc { get; set; } = null!;
    [Inject] private AppDbContext Db { get; set; } = null!;

    // ── Datos ─────────────────────────────────────────────────────────────────
    private bool _cargando = true;
    private List<Equipo> _equipos = [];
    private Dictionary<int, Equipo> _equiposById = [];

    private Dictionary<string, List<Partido>> _grupoMap = [];
    private List<Partido> _r32 = [], _r16 = [], _cuartos = [], _semis = [];
    private Partido? _tercero, _finalPartido;
    private ResultadoFinal? _resultadoFinal;

    // ── Estado local de edición ───────────────────────────────────────────────
    private Dictionary<int, ResultadoPartido?> _resultadosGrupo = [];
    private Dictionary<int, (int? Local, int? Visitante)> _estadoR32 = [];
    private Dictionary<int, (int? Local, int? Visitante)> _equiposKo = [];

    private int? _marcadorSemi1Local, _marcadorSemi1Visitante;
    private int? _marcadorSemi2Local, _marcadorSemi2Visitante;
    private int? _marcadorFinalLocal, _marcadorFinalVisitante;

    // Extras multi-select
    private List<int> _masGoleadorIds = [];
    private List<int> _masGoleadoIds = [];
    private List<int> _menosGoleadoIds = [];

    // ── UI ────────────────────────────────────────────────────────────────────
    private bool _showConfirm;
    private bool _guardando;
    private bool _exitoGuardado;
    private string? _errorGuardado;
    private TabResultado _tabActual;
    private int _tabIndex;

    private TabResultado TabActual => _tabIndex switch
    {
        0 => TabResultado.Grupos,
        1 => TabResultado.R32,
        2 => TabResultado.R16,
        3 => TabResultado.Cuartos,
        4 => TabResultado.Semis,
        5 => TabResultado.Tercero,
        6 => TabResultado.Final,
        7 => TabResultado.PosicionesFinal,
        _ => TabResultado.Grupos
    };

    // ── Init ──────────────────────────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        _cargando = true;
        _exitoGuardado = false;
        StateHasChanged();

        _equipos = await Db.Equipos.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync();
        _equiposById = _equipos.ToDictionary(e => e.Id);

        var partidos = await Db.Partidos
            .AsNoTracking()
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Include(p => p.EquipoGanador)
            .OrderBy(p => p.NumeroPartido)
            .ToListAsync();

        _grupoMap = partidos
            .Where(p => p.Fase == Fase.Grupos)
            .GroupBy(p => p.EquipoLocal!.Grupo)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.NumeroPartido).ToList());

        foreach (var p in partidos.Where(p => p.Fase == Fase.Grupos))
            _resultadosGrupo[p.Id] = p.ResultadoGrupo;

        _r32 = partidos.Where(p => p.Fase == Fase.RoundOf32).ToList();
        _r16 = partidos.Where(p => p.Fase == Fase.RoundOf16).ToList();
        _cuartos = partidos.Where(p => p.Fase == Fase.Cuartos).ToList();
        _semis = partidos.Where(p => p.Fase == Fase.Semis).ToList();
        _tercero = partidos.FirstOrDefault(p => p.Fase == Fase.TercerPuesto);
        _finalPartido = partidos.FirstOrDefault(p => p.Fase == Fase.Final);

        foreach (var p in _r32)
            _estadoR32[p.Id] = (p.EquipoLocalId, p.EquipoVisitanteId);

        foreach (var p in _r16.Concat(_cuartos).Concat(_semis)
                               .Concat(_tercero is null ? [] : [_tercero])
                               .Concat(_finalPartido is null ? [] : [_finalPartido]))
            _equiposKo[p.Id] = (p.EquipoLocalId, p.EquipoVisitanteId);

        var semi1 = partidos.FirstOrDefault(p => p.NumeroPartido == 101);
        var semi2 = partidos.FirstOrDefault(p => p.NumeroPartido == 102);
        var final = partidos.FirstOrDefault(p => p.NumeroPartido == 104);

        _marcadorSemi1Local = semi1?.GolesLocal;
        _marcadorSemi1Visitante = semi1?.GolesVisitante;
        _marcadorSemi2Local = semi2?.GolesLocal;
        _marcadorSemi2Visitante = semi2?.GolesVisitante;
        _marcadorFinalLocal = final?.GolesLocal;
        _marcadorFinalVisitante = final?.GolesVisitante;

        _resultadoFinal = await Db.ResultadoFinal.AsNoTracking().FirstOrDefaultAsync();

        // Cargar listas multi-select
        _masGoleadorIds = _resultadoFinal?.MasGoleadorIdList ?? [];
        _masGoleadoIds = _resultadoFinal?.MasGoleadoIdList ?? [];
        _menosGoleadoIds = _resultadoFinal?.MenosGoleadoIdList ?? [];

        _cargando = false;
        StateHasChanged();
    }

    // ── Helpers de estado local ───────────────────────────────────────────────
    internal ResultadoPartido? GetResultadoGrupo(int partidoId) =>
        _resultadosGrupo.TryGetValue(partidoId, out var r) ? r : null;

    internal void SetResultadoGrupo(int partidoId, ResultadoPartido? resultado) =>
        _resultadosGrupo[partidoId] = resultado;

    internal int? GetR32Local(int partidoId) =>
        _estadoR32.TryGetValue(partidoId, out var e) ? e.Local : null;

    internal int? GetR32Visitante(int partidoId) =>
        _estadoR32.TryGetValue(partidoId, out var e) ? e.Visitante : null;

    internal void SetR32Local(int partidoId, int? equipoId)
    {
        var cur = _estadoR32.TryGetValue(partidoId, out var e) ? e : (null, null);
        _estadoR32[partidoId] = (equipoId, cur.Visitante);
        var p = _r32.FirstOrDefault(x => x.Id == partidoId);
        if (p is not null) { p.EquipoLocalId = equipoId; p.EquipoLocal = equipoId.HasValue ? _equiposById.GetValueOrDefault(equipoId.Value) : null; }
    }

    internal void SetR32Visitante(int partidoId, int? equipoId)
    {
        var cur = _estadoR32.TryGetValue(partidoId, out var e) ? e : (null, null);
        _estadoR32[partidoId] = (cur.Local, equipoId);
        var p = _r32.FirstOrDefault(x => x.Id == partidoId);
        if (p is not null) { p.EquipoVisitanteId = equipoId; p.EquipoVisitante = equipoId.HasValue ? _equiposById.GetValueOrDefault(equipoId.Value) : null; }
    }

    internal int? GetKoLocal(int partidoId) =>
        _equiposKo.TryGetValue(partidoId, out var e) ? e.Local : null;

    internal int? GetKoVisitante(int partidoId) =>
        _equiposKo.TryGetValue(partidoId, out var e) ? e.Visitante : null;

    internal void SetKoLocal(int partidoId, int? equipoId)
    {
        var cur = _equiposKo.TryGetValue(partidoId, out var e) ? e : (null, null);
        _equiposKo[partidoId] = (equipoId, cur.Visitante);
        var p = _r16.Concat(_cuartos).Concat(_semis)
                    .Concat(_tercero is null ? [] : [_tercero])
                    .Concat(_finalPartido is null ? [] : [_finalPartido])
                    .FirstOrDefault(x => x.Id == partidoId);
        if (p is not null) { p.EquipoLocalId = equipoId; p.EquipoLocal = equipoId.HasValue ? _equiposById.GetValueOrDefault(equipoId.Value) : null; }
    }

    internal void SetKoVisitante(int partidoId, int? equipoId)
    {
        var cur = _equiposKo.TryGetValue(partidoId, out var e) ? e : (null, null);
        _equiposKo[partidoId] = (cur.Local, equipoId);
        var p = _r16.Concat(_cuartos).Concat(_semis)
                    .Concat(_tercero is null ? [] : [_tercero])
                    .Concat(_finalPartido is null ? [] : [_finalPartido])
                    .FirstOrDefault(x => x.Id == partidoId);
        if (p is not null) { p.EquipoVisitanteId = equipoId; p.EquipoVisitante = equipoId.HasValue ? _equiposById.GetValueOrDefault(equipoId.Value) : null; }
    }

    // ── Candidatos R32 ────────────────────────────────────────────────────────
    internal List<Equipo> GetCandidatosR32(string slot, int? excluirId, int? partidoNumero)
    {
        var todosKo = _r32.Concat(_r16).Concat(_cuartos).Concat(_semis)
                          .Concat(_tercero is null ? [] : [_tercero])
                          .Concat(_finalPartido is null ? [] : [_finalPartido])
                          .Select(SlotHelper.FromPartido)
                          .ToList();

        var semiEstados = _semis.Select(SlotHelper.FromPartido).ToList();
        var terceroEstado = _tercero is null ? null : SlotHelper.FromPartido(_tercero);
        var finalEstado = _finalPartido is null ? null : SlotHelper.FromPartido(_finalPartido);

        return SlotHelper.GetCandidatos(
            slot: slot,
            equipos: _equipos,
            todosKo: todosKo,
            semis: semiEstados,
            tercero: terceroEstado,
            finalMatch: finalEstado,
            excluirId: excluirId,
            partidoActualId: partidoNumero);
    }

    // ── Candidatos Knockout ───────────────────────────────────────────────────
    internal List<Equipo> GetCandidatosKo(string slot, int? excluirId = null, int? partidoActualId = null)
    {
        var todosKo = _r32.Concat(_r16).Concat(_cuartos).Concat(_semis)
                          .Concat(_tercero is null ? [] : [_tercero])
                          .Concat(_finalPartido is null ? [] : [_finalPartido])
                          .ToList();

        List<Equipo> candidatos = [];

        if (slot.StartsWith("G") && int.TryParse(slot[1..], out var matchNum))
        {
            var src = todosKo.FirstOrDefault(p => p.NumeroPartido == matchNum);
            if (src?.EquipoLocal is not null) candidatos.Add(src.EquipoLocal);
            if (src?.EquipoVisitante is not null) candidatos.Add(src.EquipoVisitante);

            if (_tercero is not null)
            {
                var estadoTercero = _equiposKo.TryGetValue(_tercero.Id, out var et) ? et : (null, null);
                int? perdedorId = null;
                if (_tercero.SlotLocal == $"P{matchNum}") perdedorId = estadoTercero.Local;
                else if (_tercero.SlotVisitante == $"P{matchNum}") perdedorId = estadoTercero.Visitante;
                if (perdedorId.HasValue)
                    candidatos = [.. candidatos.Where(e => e.Id != perdedorId.Value)];
            }
        }
        else if (slot.StartsWith("P") && int.TryParse(slot[1..], out var semiNum))
        {
            var semi = _semis.FirstOrDefault(p => p.NumeroPartido == semiNum);
            if (semi is not null)
            {
                var estadoSemi = _equiposKo.TryGetValue(semi.Id, out var es) ? es : (null, null);
                if (estadoSemi.Local.HasValue && _equiposById.TryGetValue(estadoSemi.Local.Value, out var eqL)) candidatos.Add(eqL);
                if (estadoSemi.Visitante.HasValue && _equiposById.TryGetValue(estadoSemi.Visitante.Value, out var eqV)) candidatos.Add(eqV);
            }

            if (_finalPartido is not null)
            {
                var estadoFinal = _equiposKo.TryGetValue(_finalPartido.Id, out var ef) ? ef : (null, null);
                int? ganadorId = null;
                if (_finalPartido.SlotLocal == $"G{semiNum}") ganadorId = estadoFinal.Local;
                else if (_finalPartido.SlotVisitante == $"G{semiNum}") ganadorId = estadoFinal.Visitante;
                if (ganadorId.HasValue)
                    candidatos = [.. candidatos.Where(e => e.Id != ganadorId.Value)];
            }
        }

        if (excluirId.HasValue)
            candidatos = [.. candidatos.Where(e => e.Id != excluirId.Value)];

        return [.. candidatos.DistinctBy(e => e.Id)];
    }

    // ── Reset ─────────────────────────────────────────────────────────────────
    internal void AbrirResetPuntos() => _showResetPuntos = true;
    private bool _showResetPuntos;

    internal async Task EjecutarResetPuntos()
    {
        _guardando = true;
        _errorGuardado = null;
        try
        {
            await PuntuacionSvc.ResetearTodosPuntosAsync();
            _showResetPuntos = false;
            _exitoGuardado = true;
            await CargarDatosAsync();
        }
        catch (Exception ex)
        {
            _errorGuardado = $"Error al resetear: {ex.Message}";
            _showResetPuntos = false;
        }
        finally
        {
            _guardando = false;
            StateHasChanged();
        }
    }

    internal void AbrirConfirmacion(TabResultado tab)
    {
        _tabActual = tab;
        _errorGuardado = null;
        _showConfirm = true;
    }

    internal void CerrarConfirmacion()
    {
        _showConfirm = false;
        _errorGuardado = null;
    }

    // ── Guardado ──────────────────────────────────────────────────────────────
    internal async Task EjecutarGuardado()
    {
        _guardando = true;
        _errorGuardado = null;

        try
        {
            switch (_tabActual)
            {
                case TabResultado.Grupos: await GuardarGruposAsync(); break;
                case TabResultado.R32: await GuardarR32Async(); break;
                case TabResultado.R16: await GuardarKnockoutAsync(_r16); break;
                case TabResultado.Cuartos: await GuardarKnockoutAsync(_cuartos); break;
                case TabResultado.Semis: await GuardarKnockoutAsync(_semis); break;
                case TabResultado.Tercero: if (_tercero is not null) await GuardarKnockoutAsync([_tercero]); break;
                case TabResultado.Final: await GuardarFinalAsync(); break;
                case TabResultado.PosicionesFinal: await GuardarPosicionesFinalesAsync(); break;
            }

            _showConfirm = false;
            _exitoGuardado = true;
        }
        catch (Exception ex)
        {
            _errorGuardado = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            _guardando = false;
        }
    }

    private async Task GuardarGruposAsync()
    {
        foreach (var partido in _grupoMap.Values.SelectMany(p => p))
        {
            if (!_resultadosGrupo.TryGetValue(partido.Id, out var resultado)) continue;
            var entity = await Db.Partidos.FindAsync(partido.Id);
            if (entity is null) continue;
            entity.ResultadoGrupo = resultado;
            await Db.SaveChangesAsync();
            await PuntuacionSvc.RecalcularGrupoAsync(partido.Id);
        }
    }

    private async Task GuardarR32Async()
    {
        foreach (var partido in _r32)
        {
            if (!_estadoR32.TryGetValue(partido.Id, out var estado)) continue;
            var entity = await Db.Partidos.FindAsync(partido.Id);
            if (entity is null) continue;

            entity.EquipoLocalId = estado.Local;
            entity.EquipoVisitanteId = estado.Visitante;
            await Db.SaveChangesAsync();
            await PropagateSlotAsync(partido.NumeroPartido, estado.Local, estado.Visitante);
            await PuntuacionSvc.RecalcularKnockoutAsync(partido.Id);
        }

        var siguientesIds = _r16.Concat(_cuartos).Concat(_semis)
                               .Concat(_tercero is null ? [] : [_tercero])
                               .Concat(_finalPartido is null ? [] : [_finalPartido])
                               .Select(p => p.Id).ToList();

        var actualizados = await Db.Partidos
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Where(p => siguientesIds.Contains(p.Id))
            .ToListAsync();

        foreach (var act in actualizados)
        {
            var dest = _r16.Concat(_cuartos).Concat(_semis)
                           .Concat(_tercero is null ? [] : [_tercero])
                           .Concat(_finalPartido is null ? [] : [_finalPartido])
                           .FirstOrDefault(p => p.Id == act.Id);
            if (dest is null) continue;
            dest.EquipoLocalId = act.EquipoLocalId;
            dest.EquipoLocal = act.EquipoLocal;
            dest.EquipoVisitanteId = act.EquipoVisitanteId;
            dest.EquipoVisitante = act.EquipoVisitante;
        }
    }

    private async Task PropagateSlotAsync(int numeroPartido, int? equipoLocalId, int? equipoVisitanteId)
    {
        await Task.CompletedTask;
    }

    private async Task GuardarKnockoutAsync(List<Partido> partidos)
    {
        foreach (var partido in partidos)
        {
            if (!_equiposKo.TryGetValue(partido.Id, out var estado)) continue;
            var entity = await Db.Partidos.FindAsync(partido.Id);
            if (entity is null) continue;

            entity.EquipoLocalId = estado.Local;
            entity.EquipoVisitanteId = estado.Visitante;
            await Db.SaveChangesAsync();
            await Db.Entry(entity).ReloadAsync();

            if (estado.Local.HasValue)
                await PropagateGanadorAsync(partido.NumeroPartido, estado.Local.Value, isLocal: true);
            if (estado.Visitante.HasValue)
                await PropagateGanadorAsync(partido.NumeroPartido, estado.Visitante.Value, isLocal: false);

            await PuntuacionSvc.RecalcularKnockoutAsync(partido.Id);
        }

        await RecargarPartidosAsync();
    }

    private async Task PropagateGanadorAsync(int numeroPartido, int equipoId, bool isLocal)
    {
        await Task.CompletedTask;
    }

    private async Task RecargarPartidosAsync()
    {
        var ids = _r16.Concat(_cuartos).Concat(_semis)
                      .Concat(_tercero is null ? [] : [_tercero])
                      .Concat(_finalPartido is null ? [] : [_finalPartido])
                      .Select(p => p.Id).ToList();

        var actualizados = await Db.Partidos
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        foreach (var act in actualizados)
        {
            var dest = _r16.Concat(_cuartos).Concat(_semis)
                           .Concat(_tercero is null ? [] : [_tercero])
                           .Concat(_finalPartido is null ? [] : [_finalPartido])
                           .FirstOrDefault(p => p.Id == act.Id);
            if (dest is null) continue;
            dest.EquipoLocalId = act.EquipoLocalId;
            dest.EquipoLocal = act.EquipoLocal;
            dest.EquipoVisitanteId = act.EquipoVisitanteId;
            dest.EquipoVisitante = act.EquipoVisitante;
        }
    }

    private async Task GuardarFinalAsync()
    {
        if (_finalPartido is not null) await GuardarKnockoutAsync([_finalPartido]);
        await GuardarMarcadorAsync(101, _marcadorSemi1Local, _marcadorSemi1Visitante);
        await GuardarMarcadorAsync(102, _marcadorSemi2Local, _marcadorSemi2Visitante);
        await GuardarMarcadorAsync(104, _marcadorFinalLocal, _marcadorFinalVisitante);
    }

    private async Task GuardarMarcadorAsync(int numeroPartido, int? golesLocal, int? golesVisitante)
    {
        var partido = await Db.Partidos.FirstOrDefaultAsync(p => p.NumeroPartido == numeroPartido);
        if (partido is null) return;
        partido.GolesLocal = golesLocal;
        partido.GolesVisitante = golesVisitante;
        await Db.SaveChangesAsync();
        if (golesLocal.HasValue && golesVisitante.HasValue)
            await PuntuacionSvc.RecalcularMarcadorExactoAsync(partido.Id);
    }

    private async Task GuardarPosicionesFinalesAsync()
    {
        if (_resultadoFinal is null) return;
        var entity = await Db.ResultadoFinal.FirstOrDefaultAsync();
        if (entity is null) return;

        entity.CampeonEquipoId = _resultadoFinal.CampeonEquipoId;
        entity.SegundoLugarEquipoId = _resultadoFinal.SegundoLugarEquipoId;
        entity.TercerLugarEquipoId = _resultadoFinal.TercerLugarEquipoId;
        entity.CuartoLugarEquipoId = _resultadoFinal.CuartoLugarEquipoId;

        entity.MasGoleadorIds = _masGoleadorIds.Count > 0 ? string.Join(",", _masGoleadorIds) : null;
        entity.MasGoleadoIds = _masGoleadoIds.Count > 0 ? string.Join(",", _masGoleadoIds) : null;
        entity.MenosGoleadoIds = _menosGoleadoIds.Count > 0 ? string.Join(",", _menosGoleadoIds) : null;

        await Db.SaveChangesAsync();
        await PuntuacionSvc.RecalcularResultadoFinalAsync();
    }

    // ── Helpers UI ────────────────────────────────────────────────────────────
    internal static string GetLabelResultado(ResultadoPartido r) => r switch
    {
        ResultadoPartido.Uno => "1",
        ResultadoPartido.Equis => "X",
        ResultadoPartido.Dos => "2",
        _ => ""
    };

    internal static string GetNombreTab(TabResultado tab) => tab switch
    {
        TabResultado.Grupos => "Fase de Grupos",
        TabResultado.R32 => "Dieciseisavos",
        TabResultado.R16 => "Octavos",
        TabResultado.Cuartos => "Cuartos de Final",
        TabResultado.Semis => "Semifinales",
        TabResultado.Tercero => "3° y 4° Puesto",
        TabResultado.Final => "Gran Final",
        TabResultado.PosicionesFinal => "Posiciones Finales",
        _ => ""
    };
}

public enum TabResultado
{
    Grupos, R32, R16, Cuartos, Semis, Tercero, Final, PosicionesFinal
}