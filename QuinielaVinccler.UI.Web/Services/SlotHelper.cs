namespace QuinielaVinccler.UI.Web.Services;

/// <summary>
/// Representa el estado de un partido de forma agnóstica al origen de datos.
/// Puede construirse desde PrediccionKnockout (usuario) o desde Partido (admin).
/// </summary>
public sealed record PartidoSlotEstado(
    int Id,
    int NumeroPartido,
    string SlotLocal,
    string SlotVisitante,
    int? EquipoLocalId,
    int? EquipoVisitanteId,
    Equipo? EquipoLocal,
    Equipo? EquipoVisitante);

/// <summary>
/// Lógica de resolución de candidatos por slot — compartida entre
/// PlanillaDetalle (predicciones del usuario) y ResultadosComponent (admin).
/// </summary>
public static class SlotHelper
{
    /// <summary>
    /// Devuelve los equipos candidatos para un slot dado.
    /// </summary>
    /// <param name="slot">Slot del partido ("2A", "3ABCDF", "G73", "P101"...)</param>
    /// <param name="equipos">Lista completa de equipos del torneo.</param>
    /// <param name="todosKo">TODOS los partidos KO (R32, R16, Cuartos, Semis, Tercero, Final).</param>
    /// <param name="semis">Solo los partidos de semifinales — para resolver slots P{n}.</param>
    /// <param name="tercero">Partido de 3° y 4° puesto.</param>
    /// <param name="finalMatch">Partido de la final.</param>
    /// <param name="excluirId">Id del equipo del otro slot del mismo partido.</param>
    /// <param name="partidoActualId">Id del partido actual — excluido del filtro de "ya usados" en R32.</param>
    public static List<Equipo> GetCandidatos(
        string slot,
        List<Equipo> equipos,
        List<PartidoSlotEstado> todosKo,
        List<PartidoSlotEstado> semis,
        PartidoSlotEstado? tercero,
        PartidoSlotEstado? finalMatch,
        int? excluirId = null,
        int? partidoActualId = null)
    {
        // R32 separado para la lógica de exclusión de slots ya usados
        var soloR32 = todosKo.Where(p =>
            p.SlotLocal.Length == 2 || p.SlotLocal.StartsWith("3") ||
            p.SlotVisitante.Length == 2 || p.SlotVisitante.StartsWith("3")).ToList();

        List<Equipo> candidatos = [];

        // ── MEJOR TERCERO (3XXXXX) ────────────────────────────────────────────
        if (slot.StartsWith("3"))
        {
            var gruposPermitidos = slot[1..].Select(c => c.ToString()).ToHashSet();

            var gruposYaUsados = soloR32
                .Where(pk => pk.Id != partidoActualId)
                .SelectMany(pk => new[]
                {
                    new { Slot = pk.SlotLocal,     Equipo = pk.EquipoLocal },
                    new { Slot = pk.SlotVisitante, Equipo = pk.EquipoVisitante }
                })
                .Where(x => x.Equipo is not null && x.Slot.StartsWith("3"))
                .Select(x => x.Equipo!.Grupo)
                .ToHashSet();

            gruposPermitidos.ExceptWith(gruposYaUsados);
            candidatos = [.. equipos.Where(e => gruposPermitidos.Contains(e.Grupo))];
        }

        // ── SLOT NORMAL DE GRUPO (1A, 2B...) ─────────────────────────────────
        else if (slot.Length == 2 && (slot[0] == '1' || slot[0] == '2'))
        {
            var grupo = slot[1].ToString();
            candidatos = [.. equipos.Where(e => e.Grupo == grupo)];
        }

        // ── GANADOR DE PARTIDO (G73, G89, G97, G101...) ───────────────────────
        // Busca en TODOS los partidos KO por NumeroPartido — cubre toda la cascada:
        // R32 → R16 → Cuartos → Semis → Final
        else if (slot.StartsWith("G") && int.TryParse(slot[1..], out var matchNum))
        {
            var src = todosKo.FirstOrDefault(p => p.NumeroPartido == matchNum);
            if (src?.EquipoLocal is not null)    candidatos.Add(src.EquipoLocal);
            if (src?.EquipoVisitante is not null) candidatos.Add(src.EquipoVisitante);

            // Excluir el perdedor ya seleccionado en 3°/4° puesto
            if (tercero is not null)
            {
                int? perdedorId = null;
                if (tercero.SlotLocal      == $"P{matchNum}") perdedorId = tercero.EquipoLocalId;
                else if (tercero.SlotVisitante == $"P{matchNum}") perdedorId = tercero.EquipoVisitanteId;
                if (perdedorId.HasValue)
                    candidatos = [.. candidatos.Where(e => e.Id != perdedorId.Value)];
            }
        }

        // ── PERDEDOR DE SEMIFINAL (P101, P102) ───────────────────────────────
        else if (slot.StartsWith("P") && int.TryParse(slot[1..], out var semiNum))
        {
            var semi = semis.FirstOrDefault(p => p.NumeroPartido == semiNum);
            if (semi?.EquipoLocal is not null)    candidatos.Add(semi.EquipoLocal);
            if (semi?.EquipoVisitante is not null) candidatos.Add(semi.EquipoVisitante);

            // Excluir el ganador ya seleccionado en la final
            if (finalMatch is not null)
            {
                int? ganadorId = null;
                if (finalMatch.SlotLocal      == $"G{semiNum}") ganadorId = finalMatch.EquipoLocalId;
                else if (finalMatch.SlotVisitante == $"G{semiNum}") ganadorId = finalMatch.EquipoVisitanteId;
                if (ganadorId.HasValue)
                    candidatos = [.. candidatos.Where(e => e.Id != ganadorId.Value)];
            }
        }

        // ── EXCLUIR EQUIPOS YA USADOS — solo aplica a R32 (1X, 2X, 3XXXXX) ──
        if (slot.Length == 2 || slot.StartsWith("3"))
        {
            var equiposYaUsados = soloR32
                .Where(pk => pk.Id != partidoActualId)
                .SelectMany(pk => new[] { pk.EquipoLocalId, pk.EquipoVisitanteId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            candidatos = [.. candidatos.Where(e => !equiposYaUsados.Contains(e.Id))];
        }

        // ── EXCLUIR EL OTRO SLOT DEL MISMO PARTIDO ───────────────────────────
        if (excluirId.HasValue)
            candidatos = [.. candidatos.Where(e => e.Id != excluirId.Value)];

        return [.. candidatos.DistinctBy(e => e.Id).OrderBy(e => e.Nombre)];
    }

    /// <summary>
    /// Construye desde PrediccionKnockout (contexto usuario).
    /// Para slots G{n}/P{n} usa el equipo PREDICHO (EquipoLocalPredichado/EquipoVisitantePredichado).
    /// Para slots G{n} de R16+ usa el ganador predicho (EquipoGanador).
    /// </summary>
    public static PartidoSlotEstado FromPrediccion(PrediccionKnockout pk)
    {
        // R16+: el equipo "local" y "visitante" en cascada son los ganadores predichos
        // que el usuario seleccionó en la fase anterior — guardados en EquipoGanador
        // Para R32 usamos EquipoLocalPredichado/EquipoVisitantePredichado
        var esR32 = pk.Partido.SlotLocal.Length == 2 || pk.Partido.SlotLocal.StartsWith("3") ||
                    pk.Partido.SlotVisitante.Length == 2 || pk.Partido.SlotVisitante.StartsWith("3");

        return new(
            Id:               pk.Id,
            NumeroPartido:    pk.Partido.NumeroPartido,
            SlotLocal:        pk.Partido.SlotLocal,
            SlotVisitante:    pk.Partido.SlotVisitante,
            EquipoLocalId:    esR32 ? pk.EquipoLocalPredichoId  : pk.EquipoGanadorId,
            EquipoVisitanteId: esR32 ? pk.EquipoVisitantePredichoId : null,
            EquipoLocal:      esR32 ? pk.EquipoLocalPredichado  : pk.EquipoGanador,
            EquipoVisitante:  esR32 ? pk.EquipoVisitantePredichado : null);
    }

    /// <summary>
    /// Construye desde Partido real (contexto admin).
    /// </summary>
    public static PartidoSlotEstado FromPartido(Partido p) => new(
        Id:                p.NumeroPartido,
        NumeroPartido:     p.NumeroPartido,
        SlotLocal:         p.SlotLocal,
        SlotVisitante:     p.SlotVisitante,
        EquipoLocalId:     p.EquipoLocalId,
        EquipoVisitanteId: p.EquipoVisitanteId,
        EquipoLocal:       p.EquipoLocal,
        EquipoVisitante:   p.EquipoVisitante);
}
