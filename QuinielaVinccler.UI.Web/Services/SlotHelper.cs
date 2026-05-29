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
    public static List<Equipo> GetCandidatos(
        string slot,
        List<Equipo> equipos,
        List<PartidoSlotEstado> todosR32,
        List<PartidoSlotEstado> semis,
        PartidoSlotEstado? tercero,
        PartidoSlotEstado? finalMatch,
        int? excluirId = null,
        int? partidoActualId = null)
    {
        var allKo = new List<PartidoSlotEstado>(todosR32);
        if (tercero    is not null) allKo.Add(tercero);
        if (finalMatch is not null) allKo.Add(finalMatch);
        allKo.AddRange(semis);

        List<Equipo> candidatos = [];

        // ── MEJOR TERCERO (3XXXXX) ────────────────────────────────────────────
        if (slot.StartsWith("3"))
        {
            var gruposPermitidos = slot[1..].Select(c => c.ToString()).ToHashSet();

            var gruposYaUsados = todosR32
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

        // ── GANADOR DE PARTIDO (G73, G101...) ────────────────────────────────
        // matchNum es el NumeroPartido del partido fuente
        else if (slot.StartsWith("G") && int.TryParse(slot[1..], out var matchNum))
        {
            var src = allKo.FirstOrDefault(p => p.NumeroPartido == matchNum);
            if (src?.EquipoLocal is not null)    candidatos.Add(src.EquipoLocal);
            if (src?.EquipoVisitante is not null) candidatos.Add(src.EquipoVisitante);

            // Excluir perdedor ya seleccionado en 3°/4° puesto
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
        // semiNum es el NumeroPartido de la semi
        else if (slot.StartsWith("P") && int.TryParse(slot[1..], out var semiNum))
        {
            var semi = semis.FirstOrDefault(p => p.NumeroPartido == semiNum);
            if (semi?.EquipoLocal is not null)    candidatos.Add(semi.EquipoLocal);
            if (semi?.EquipoVisitante is not null) candidatos.Add(semi.EquipoVisitante);

            // Excluir ganador ya seleccionado en la final
            if (finalMatch is not null)
            {
                int? ganadorId = null;
                if (finalMatch.SlotLocal      == $"G{semiNum}") ganadorId = finalMatch.EquipoLocalId;
                else if (finalMatch.SlotVisitante == $"G{semiNum}") ganadorId = finalMatch.EquipoVisitanteId;
                if (ganadorId.HasValue)
                    candidatos = [.. candidatos.Where(e => e.Id != ganadorId.Value)];
            }
        }

        // ── EXCLUIR EQUIPOS YA USADOS EN R32 (slots 1X, 2X, 3XXXXX) ─────────
        if (slot.Length == 2 || slot.StartsWith("3"))
        {
            var equiposYaUsados = todosR32
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
    /// Construye un PartidoSlotEstado desde una PrediccionKnockout (contexto usuario).
    /// Id = PrediccionKnockout.Id — para el filtro de "partido actual" en exclusiones.
    /// NumeroPartido = Partido.NumeroPartido — para búsqueda por slots G{n} y P{n}.
    /// </summary>
    public static PartidoSlotEstado FromPrediccion(PrediccionKnockout pk) => new(
        Id:                pk.Id,
        NumeroPartido:     pk.Partido.NumeroPartido,
        SlotLocal:         pk.Partido.SlotLocal,
        SlotVisitante:     pk.Partido.SlotVisitante,
        EquipoLocalId:     pk.EquipoLocalPredichoId,
        EquipoVisitanteId: pk.EquipoVisitantePredichoId,
        EquipoLocal:       pk.EquipoLocalPredichado,
        EquipoVisitante:   pk.EquipoVisitantePredichado);

    /// <summary>
    /// Construye un PartidoSlotEstado desde un Partido real (contexto admin).
    /// Id = NumeroPartido — consistente para el filtro de "partido actual".
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
