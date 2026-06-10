namespace QuinielaVinccler.UI.Web.Services;

/// <summary>
/// Calcula y persiste puntos según las reglas de la quiniela FIFA 2026.
///
/// Tabla de puntos:
///   Fase de grupos     → 60 pts por resultado correcto (1/X/2)
///   R32 (dieciseisavos)→ 70 pts por slot correcto (local + visitante)
///   R16 (octavos)      → 70 pts por ganador correcto
///   Cuartos            → 80 pts por ganador correcto
///   Semis              → 100 pts por ganador correcto
///   3° y 4° puesto     → 100 pts por ganador correcto
///   Gran Final         → 100 pts por ganador correcto
///   Campeón            → 300 pts
///   2° Lugar           → 200 pts
///   3er Lugar          → 100 pts
///   4° Lugar           → 50 pts
///   Más goleador (fase eliminatoria)  → 100 pts
///   Más goleado (fase eliminatoria)   → 100 pts
///   Menos goleado (fase eliminatoria) → 100 pts
///   Marcador exacto Semi 1 (90 min)   → 100 pts
///   Marcador exacto Semi 2 (90 min)   → 100 pts
///   Marcador exacto Gran Final (90min)→ 100 pts
/// </summary>
public class PuntuacionService(AppDbContext db) : IPuntuacionService
{
    // ── Tabla de puntos ───────────────────────────────────────────────────────
    private const int PtsGrupo = 60;
    private const int PtsR32 = 70;  // por cada slot (local + visitante)
    private const int PtsR16 = 70;
    private const int PtsCuartos = 80;
    private const int PtsSemis = 100;
    private const int PtsTercero = 100;
    private const int PtsFinal = 100;
    private const int PtsCampeon = 300;
    private const int PtsSegundo = 200;
    private const int PtsTercerLug = 100;
    private const int PtsCuarto = 50;
    private const int PtsExtra = 100;
    private const int PtsMarcador = 100;

    // ── Fase de grupos ────────────────────────────────────────────────────────
    public async Task RecalcularGrupoAsync(int partidoId)
    {
        var partido = await db.Partidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == partidoId);
        if (partido is null) return;

        var predicciones = await db.PrediccionesGrupo
            .Where(p => p.PartidoId == partidoId)
            .ToListAsync();

        foreach (var pred in predicciones)
        {
            // Si no hay resultado real, resetear puntos a null
            pred.PuntosObtenidos = partido.ResultadoGrupo is null
                ? null
                : pred.ResultadoPredicho == partido.ResultadoGrupo ? PtsGrupo : 0;
        }

        await db.SaveChangesAsync();
        await ActualizarTotalesAsync(predicciones.Select(p => p.PlanillaId).Distinct());
    }

    // ── Knockout (R16, Cuartos, Semis, 3°/4°, Final) ─────────────────────────
    public async Task RecalcularKnockoutAsync(int partidoId)
    {
        var partido = await db.Partidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == partidoId);
        if (partido is null) return;

        // R16+ requieren al menos un equipo definido para calcular puntos
        bool sinResultado = partido.Fase == Fase.RoundOf32
            ? partido.EquipoLocalId is null && partido.EquipoVisitanteId is null
            : partido.EquipoLocalId is null && partido.EquipoVisitanteId is null;

        if (sinResultado)
        {
            var predsReset = await db.PrediccionesKnockout
                .Where(p => p.PartidoId == partidoId)
                .ToListAsync();
            foreach (var pred in predsReset) pred.PuntosObtenidos = null;
            await db.SaveChangesAsync();
            await ActualizarTotalesAsync(predsReset.Select(p => p.PlanillaId).Distinct());
            return;
        }

        var pts = PuntosPorFase(partido.Fase);

        var predicciones = await db.PrediccionesKnockout
            .Where(p => p.PartidoId == partidoId)
            .ToListAsync();

        if (partido.Fase == Fase.RoundOf32)
        {
            // R32: 70pts por cada slot correcto (local y visitante se evalúan por separado)
            // PuntosObtenidos acumula ambos slots (0, 70 o 140)
            foreach (var pred in predicciones)
            {
                int pts32 = 0;
                if (pred.EquipoLocalPredichoId.HasValue &&
                    pred.EquipoLocalPredichoId == partido.EquipoLocalId)
                    pts32 += PtsR32;

                if (pred.EquipoVisitantePredichoId.HasValue &&
                    pred.EquipoVisitantePredichoId == partido.EquipoVisitanteId)
                    pts32 += PtsR32;

                pred.PuntosObtenidos = pts32;
            }
        }
        else
        {
            // R16+: igual que R32 — el usuario predijo local y visitante de cada partido.
            // Gana pts por cada slot correcto.
            foreach (var pred in predicciones)
            {
                int ptsKo = 0;
                if (pred.EquipoLocalPredichoId.HasValue &&
                    pred.EquipoLocalPredichoId == partido.EquipoLocalId)
                    ptsKo += pts;

                if (pred.EquipoVisitantePredichoId.HasValue &&
                    pred.EquipoVisitantePredichoId == partido.EquipoVisitanteId)
                    ptsKo += pts;

                pred.PuntosObtenidos = ptsKo;
            }
        }

        await db.SaveChangesAsync();
        await ActualizarTotalesAsync(predicciones.Select(p => p.PlanillaId).Distinct());
    }

    // ── Marcador exacto a 90 min (Semis #101, #102 y Final #104) ─────────────
    public async Task RecalcularMarcadorExactoAsync(int partidoId)
    {
        var partido = await db.Partidos.FindAsync(partidoId);

        if (partido is null) return;
        if (partido.GolesLocal is null || partido.GolesVisitante is null) return;
        if (partido.NumeroPartido is not (101 or 102 or 104)) return;

        // Los marcadores exactos están en PrediccionFinal
        // Identificamos qué campo corresponde al partido
        var predicciones = await db.PrediccionesFinal.ToListAsync();

        var planillaIds = new List<int>();

        foreach (var pred in predicciones)
        {
            bool acierta = partido.NumeroPartido switch
            {
                101 => pred.GolesLocalSemi1 == partido.GolesLocal &&
                       pred.GolesVisitanteSemi1 == partido.GolesVisitante,
                102 => pred.GolesLocalSemi2 == partido.GolesLocal &&
                       pred.GolesVisitanteSemi2 == partido.GolesVisitante,
                104 => pred.GolesLocalGranFinal == partido.GolesLocal &&
                       pred.GolesVisitanteGranFinal == partido.GolesVisitante,
                _ => false
            };

            // Guardamos puntos en una columna auxiliar de PrediccionFinal
            // usando el campo PuntosMarcadorSemi1/2/Final
            switch (partido.NumeroPartido)
            {
                case 101: pred.PuntosMarcadorSemi1 = acierta ? PtsMarcador : 0; break;
                case 102: pred.PuntosMarcadorSemi2 = acierta ? PtsMarcador : 0; break;
                case 104: pred.PuntosMarcadorGranFinal = acierta ? PtsMarcador : 0; break;
            }

            planillaIds.Add(pred.PlanillaId);
        }

        await db.SaveChangesAsync();
        await ActualizarTotalesAsync(planillaIds.Distinct());
    }

    // ── Posiciones finales + extras ───────────────────────────────────────────
    public async Task RecalcularResultadoFinalAsync()
    {
        var real = await db.ResultadoFinal.FirstOrDefaultAsync();
        if (real is null) return;

        var predicciones = await db.PrediccionesFinal.ToListAsync();
        var planillaIds = new List<int>();

        foreach (var pred in predicciones)
        {
            pred.PuntosPosicionesFinal =
                (pred.CampeonEquipoId == real.CampeonEquipoId && real.CampeonEquipoId is not null ? PtsCampeon : 0) +
                (pred.SegundoLugarEquipoId == real.SegundoLugarEquipoId && real.SegundoLugarEquipoId is not null ? PtsSegundo : 0) +
                (pred.TercerLugarEquipoId == real.TercerLugarEquipoId && real.TercerLugarEquipoId is not null ? PtsTercerLug : 0) +
                (pred.CuartoLugarEquipoId == real.CuartoLugarEquipoId && real.CuartoLugarEquipoId is not null ? PtsCuarto : 0) +
                (pred.MasGoleadorEquipoId == real.MasGoleadorEquipoId && real.MasGoleadorEquipoId is not null ? PtsExtra : 0) +
                (pred.MasGoleadoEquipoId == real.MasGoleadoEquipoId && real.MasGoleadoEquipoId is not null ? PtsExtra : 0) +
                (pred.MenosGoleadoEquipoId == real.MenosGoleadoEquipoId && real.MenosGoleadoEquipoId is not null ? PtsExtra : 0);

            planillaIds.Add(pred.PlanillaId);
        }

        await db.SaveChangesAsync();
        await ActualizarTotalesAsync(planillaIds.Distinct());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Resetea todos los puntos a null/0.
    /// Usar cuando el admin borra todos los resultados.
    /// </summary>
    public async Task ResetearTodosPuntosAsync()
    {
        // ── Puntos ────────────────────────────────────────────────────────────
        await db.PrediccionesGrupo
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.PuntosObtenidos, (int?)null));

        await db.PrediccionesKnockout
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.PuntosObtenidos, (int?)null));

        await db.PrediccionesFinal
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.PuntosPosicionesFinal, (int?)null)
                .SetProperty(x => x.PuntosMarcadorSemi1, (int?)null)
                .SetProperty(x => x.PuntosMarcadorSemi2, (int?)null)
                .SetProperty(x => x.PuntosMarcadorGranFinal, (int?)null));

        await db.Planillas
            .Where(p => p.UserId != null)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.PuntajeTotal, 0));

        // ── Resultados de partidos ─────────────────────────────────────────────
        await db.Partidos
            .Where(p => p.Fase == Fase.Grupos)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.ResultadoGrupo, (ResultadoPartido?)null));

        await db.Partidos
            .Where(p => p.Fase != Fase.Grupos)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.EquipoLocalId, (int?)null)
                .SetProperty(x => x.EquipoVisitanteId, (int?)null)
                .SetProperty(x => x.EquipoGanadorId, (int?)null)
                .SetProperty(x => x.GolesLocal, (int?)null)
                .SetProperty(x => x.GolesVisitante, (int?)null));

        // ── ResultadoFinal singleton ───────────────────────────────────────────
        await db.ResultadoFinal
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.CampeonEquipoId, (int?)null)
                .SetProperty(x => x.SegundoLugarEquipoId, (int?)null)
                .SetProperty(x => x.TercerLugarEquipoId, (int?)null)
                .SetProperty(x => x.CuartoLugarEquipoId, (int?)null)
                .SetProperty(x => x.MasGoleadorEquipoId, (int?)null)
                .SetProperty(x => x.MasGoleadoEquipoId, (int?)null)
                .SetProperty(x => x.MenosGoleadoEquipoId, (int?)null));
    }

    /// <summary>
    /// Recalcula PuntajeTotal de cada planilla afectada sumando todos sus puntos.
    /// Se ejecuta después de cada recálculo parcial para mantener coherencia.
    /// </summary>
    private async Task ActualizarTotalesAsync(IEnumerable<int> planillaIds)
    {
        var permitirIncompletas = await db.Configuraciones
            .Where(c => c.Clave == ConfiguracionKeys.PermitirIncompletasEnRanking)
            .Select(c => c.Valor)
            .FirstOrDefaultAsync() == "true";

        foreach (var planillaId in planillaIds)
        {
            var planilla = await db.Planillas.FindAsync(planillaId);
            if (planilla is null) continue;

            // Si no se permiten incompletas y la planilla no está completa → PuntajeTotal = 0
            if (!permitirIncompletas && planilla.Estado != EstadoPlanilla.Completa)
            {
                planilla.PuntajeTotal = 0;
                continue;
            }

            var ptsGrupo = await db.PrediccionesGrupo
                .Where(p => p.PlanillaId == planillaId)
                .SumAsync(p => p.PuntosObtenidos ?? 0);

            var ptsKo = await db.PrediccionesKnockout
                .Where(p => p.PlanillaId == planillaId)
                .SumAsync(p => p.PuntosObtenidos ?? 0);

            var pFinal = await db.PrediccionesFinal
                .FirstOrDefaultAsync(p => p.PlanillaId == planillaId);

            var ptsFinal = pFinal is null ? 0 :
                (pFinal.PuntosPosicionesFinal ?? 0) +
                (pFinal.PuntosMarcadorSemi1 ?? 0) +
                (pFinal.PuntosMarcadorSemi2 ?? 0) +
                (pFinal.PuntosMarcadorGranFinal ?? 0);

            planilla.PuntajeTotal = ptsGrupo + ptsKo + ptsFinal;
        }

        await db.SaveChangesAsync();
    }

    private static int PuntosPorFase(Fase fase) => fase switch
    {
        Fase.RoundOf16 => PtsR16,
        Fase.Cuartos => PtsCuartos,
        Fase.Semis => PtsSemis,
        Fase.TercerPuesto => PtsTercero,
        Fase.Final => PtsFinal,
        _ => 0
    };
}