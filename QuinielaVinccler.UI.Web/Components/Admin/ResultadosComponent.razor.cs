namespace QuinielaVinccler.UI.Web.Components.Admin;

using Color = MudBlazor.Color;

public partial class ResultadosComponent : ComponentBase
{
    [Inject] private IPuntuacionService PuntuacionSvc { get; set; } = null!;
    [Inject] private AppDbContext Db { get; set; } = null!;

    // ── Datos ─────────────────────────────────────────────────────────────────
    private bool _cargando = true;
    private List<Equipo> _equipos = [];
    private Dictionary<int, Equipo> _equiposById = [];

    // Fase de grupos
    private Dictionary<string, List<Partido>> _grupoMap = [];

    // Knockout — trabajamos directamente con Partido
    private List<Partido> _r32 = [], _r16 = [], _cuartos = [], _semis = [];
    private Partido? _tercero, _finalPartido;

    // ResultadoFinal singleton
    private ResultadoFinal? _resultadoFinal;

    // ── Estado local de edición ───────────────────────────────────────────────

    // Fase de grupos: partidoId → resultado
    private Dictionary<int, ResultadoPartido?> _resultadosGrupo = [];

    // R32: partidoId → (equipoLocalId, equipoVisitanteId)
    private Dictionary<int, (int? Local, int? Visitante)> _estadoR32 = [];

    // Knockout R16+: partidoId → (equipoLocalId, equipoVisitanteId)
    // El admin selecciona qué equipo clasificó en cada slot de la llave anterior
    private Dictionary<int, (int? Local, int? Visitante)> _equiposKo = [];

    // Marcadores exactos
    private int? _marcadorSemi1Local, _marcadorSemi1Visitante;
    private int? _marcadorSemi2Local, _marcadorSemi2Visitante;
    private int? _marcadorFinalLocal, _marcadorFinalVisitante;

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
        _equipos = await Db.Equipos.OrderBy(e => e.Nombre).ToListAsync();
        _equiposById = _equipos.ToDictionary(e => e.Id);

        var partidos = await Db.Partidos
            .Include(p => p.EquipoLocal)
            .Include(p => p.EquipoVisitante)
            .Include(p => p.EquipoGanador)
            .OrderBy(p => p.NumeroPartido)
            .ToListAsync();

        // Grupos
        _grupoMap = partidos
            .Where(p => p.Fase == Fase.Grupos)
            .GroupBy(p => p.EquipoLocal!.Grupo)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.NumeroPartido).ToList());

        foreach (var p in partidos.Where(p => p.Fase == Fase.Grupos))
            _resultadosGrupo[p.Id] = p.ResultadoGrupo;

        // Knockout
        _r32 = partidos.Where(p => p.Fase == Fase.RoundOf32).ToList();
        _r16 = partidos.Where(p => p.Fase == Fase.RoundOf16).ToList();
        _cuartos = partidos.Where(p => p.Fase == Fase.Cuartos).ToList();
        _semis = partidos.Where(p => p.Fase == Fase.Semis).ToList();
        _tercero = partidos.FirstOrDefault(p => p.Fase == Fase.TercerPuesto);
        _finalPartido = partidos.FirstOrDefault(p => p.Fase == Fase.Final);

        // Estado inicial R32
        foreach (var p in _r32)
            _estadoR32[p.Id] = (p.EquipoLocalId, p.EquipoVisitanteId);

        // Estado ko R16+: local y visitante
        foreach (var p in _r16.Concat(_cuartos).Concat(_semis)
                               .Concat(_tercero is null ? [] : [_tercero])
                               .Concat(_finalPartido is null ? [] : [_finalPartido]))
            _equiposKo[p.Id] = (p.EquipoLocalId, p.EquipoVisitanteId);

        // Marcadores
        var semi1 = partidos.FirstOrDefault(p => p.NumeroPartido == 101);
        var semi2 = partidos.FirstOrDefault(p => p.NumeroPartido == 102);
        var final = partidos.FirstOrDefault(p => p.NumeroPartido == 104);

        _marcadorSemi1Local = semi1?.GolesLocal;
        _marcadorSemi1Visitante = semi1?.GolesVisitante;
        _marcadorSemi2Local = semi2?.GolesLocal;
        _marcadorSemi2Visitante = semi2?.GolesVisitante;
        _marcadorFinalLocal = final?.GolesLocal;
        _marcadorFinalVisitante = final?.GolesVisitante;

        _resultadoFinal = await Db.ResultadoFinal.FirstOrDefaultAsync();

        _cargando = false;
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
        // Actualizar entidad en memoria para que SlotHelper vea el cambio
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

    // ── SlotHelper: candidatos para R32 admin ─────────────────────────────────
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

    // ── Candidatos para Octavos en adelante ──────────────────────────────────
    /// <summary>
    /// Resuelve los dos equipos candidatos para un slot G{n} buscando en los
    /// partidos ya guardados. Funciona para toda la cascada:
    /// G73 → busca partido #73 en _r32
    /// G89 → busca partido #89 en _r16
    /// G97 → busca partido #97 en _cuartos
    /// etc.
    /// </summary>
    internal List<Equipo> GetCandidatosKo(string slot, int? excluirId = null, int? partidoActualId = null)
    {
        var todosKo = _r32.Concat(_r16).Concat(_cuartos).Concat(_semis)
                          .Concat(_tercero is null ? [] : [_tercero])
                          .Concat(_finalPartido is null ? [] : [_finalPartido])
                          .ToList();

        List<Equipo> candidatos = [];

        // ── GANADOR DE PARTIDO (G73, G89, G101...) ────────────────────────────
        if (slot.StartsWith("G") && int.TryParse(slot[1..], out var matchNum))
        {
            var src = todosKo.FirstOrDefault(p => p.NumeroPartido == matchNum);
            if (src?.EquipoLocal is not null) candidatos.Add(src.EquipoLocal);
            if (src?.EquipoVisitante is not null) candidatos.Add(src.EquipoVisitante);

            // Excluir el perdedor ya seleccionado en 3°/4° puesto
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

        // ── PERDEDOR DE SEMIFINAL (P101, P102) ────────────────────────────────
        else if (slot.StartsWith("P") && int.TryParse(slot[1..], out var semiNum))
        {
            var semi = _semis.FirstOrDefault(p => p.NumeroPartido == semiNum);
            if (semi is not null)
            {
                var estadoSemi = _equiposKo.TryGetValue(semi.Id, out var es) ? es : (null, null);
                if (estadoSemi.Local.HasValue && _equiposById.TryGetValue(estadoSemi.Local.Value, out var eqL))
                    candidatos.Add(eqL);
                if (estadoSemi.Visitante.HasValue && _equiposById.TryGetValue(estadoSemi.Visitante.Value, out var eqV))
                    candidatos.Add(eqV);
            }

            // Excluir el ganador ya seleccionado en la final
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

    // ── Confirmación ──────────────────────────────────────────────────────────
    internal void AbrirResetPuntos() => _showResetPuntos = true;
    private bool _showResetPuntos;

    internal async Task EjecutarResetPuntos()
    {
        await PuntuacionSvc.ResetearTodosPuntosAsync();
        _showResetPuntos = false;
        _exitoGuardado = true;
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
                case TabResultado.Grupos:
                    await GuardarGruposAsync();
                    break;
                case TabResultado.R32:
                    await GuardarR32Async();
                    break;
                case TabResultado.R16:
                    await GuardarKnockoutAsync(_r16);
                    break;
                case TabResultado.Cuartos:
                    await GuardarKnockoutAsync(_cuartos);
                    break;
                case TabResultado.Semis:
                    await GuardarKnockoutAsync(_semis);
                    break;
                case TabResultado.Tercero:
                    if (_tercero is not null) await GuardarKnockoutAsync([_tercero]);
                    break;
                case TabResultado.Final:
                    await GuardarFinalAsync();
                    break;
                case TabResultado.PosicionesFinal:
                    await GuardarPosicionesFinalesAsync();
                    break;
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

            // Propagar equipos al partido siguiente que tenga G{numero} como slot
            await PropagateSlotAsync(partido.NumeroPartido, estado.Local, estado.Visitante);

            // Recalcular puntos de predicciones
            await PuntuacionSvc.RecalcularKnockoutAsync(partido.Id);
        }

        // Recargar partidos de fases siguientes para reflejar cascada en UI
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

    /// <summary>
    /// Al guardar el ganador de un partido knockout, propaga ese equipo
    /// al partido siguiente que tenga SlotLocal o SlotVisitante = "G{numeroPartido}".
    /// </summary>
    private async Task PropagateSlotAsync(int numeroPartido, int? equipoLocalId, int? equipoVisitanteId)
    {
        // El partido de Octavos referencia al ganador de este R32 como G{numeroPartido}
        // Llenamos ese slot con el equipo ganador — pero como aún no hay ganador en R32,
        // ponemos el equipo local en SlotLocal y visitante en SlotVisitante del partido siguiente
        // para que el admin pueda seleccionar el ganador en Octavos
        var slotKey = $"G{numeroPartido}";

        var siguientes = await Db.Partidos
            .Where(p => p.SlotLocal == slotKey || p.SlotVisitante == slotKey)
            .ToListAsync();

        foreach (var sig in siguientes)
        {
            // El slot G{n} del partido siguiente representará al ganador del partido n
            // Por ahora ponemos null — el ganador se define cuando el admin seleccione
            // en Octavos. Lo que SÍ necesitamos es que Partido.EquipoLocal y Visitante
            // del partido de Octavos tengan los dos equipos del R32 para mostrar opciones.
            // NOTA: En este modelo, el partido de Octavos tiene DOS slots (SlotLocal y SlotVisitante)
            // cada uno apuntando a un partido de R32 distinto.
            // slot G73 en Octavos → equipo que GANE el partido #73 (local o visitante del #73)
            // No podemos poner ambos en EquipoLocalId — solo uno cabe.
            // La solución: dejamos null hasta que el admin seleccione el ganador en Octavos,
            // pero el botón de Octavos debe resolver sus candidatos desde los partidos de R32.
            // Por eso no tocamos nada aquí — la propagación real ocurre en PropagateGanadorAsync
            // cuando el admin selecciona el ganador de R32 en futuros cambios de diseño.
        }

        await Db.SaveChangesAsync();
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

            // Propagar ambos equipos al partido siguiente
            if (estado.Local.HasValue)
                await PropagateGanadorAsync(partido.NumeroPartido, estado.Local.Value, isLocal: true);
            if (estado.Visitante.HasValue)
                await PropagateGanadorAsync(partido.NumeroPartido, estado.Visitante.Value, isLocal: false);

            // Recalcular siempre — incluso al borrar para resetear puntos a 0
            await PuntuacionSvc.RecalcularKnockoutAsync(partido.Id);
        }

        await RecargarPartidosAsync();
    }

    /// <summary>
    /// Propaga el ganador de un partido al slot del partido siguiente.
    /// El partido siguiente referencia al ganador como G{numeroPartido}.
    /// </summary>
    private async Task PropagateGanadorAsync(int numeroPartido, int equipoId, bool isLocal)
    {
        // No propagamos a la siguiente fase — el admin selecciona directamente
        // los equipos de cada partido en cada tab.
        // Esta función queda reservada para futuras implementaciones.
        await Task.CompletedTask;
    }

    /// <summary>
    /// Recarga todos los partidos de fases siguientes desde DB para reflejar
    /// la cascada en la UI después de guardar.
    /// </summary>
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
        entity.MasGoleadorEquipoId = _resultadoFinal.MasGoleadorEquipoId;
        entity.MasGoleadoEquipoId = _resultadoFinal.MasGoleadoEquipoId;
        entity.MenosGoleadoEquipoId = _resultadoFinal.MenosGoleadoEquipoId;

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