namespace QuinielaVinccler.UI.Web.Services;

public class PredictionService(AppDbContext db) : IPredictionService
{
    public async Task<Planilla?> CargarPlanillaAsync(int planillaId, int userId)
    {
        return await db.Planillas
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.PrediccionesGrupo)
                .ThenInclude(pg => pg.Partido)
                    .ThenInclude(p => p.EquipoLocal)
            .Include(p => p.PrediccionesGrupo)
                .ThenInclude(pg => pg.Partido)
                    .ThenInclude(p => p.EquipoVisitante)
            .Include(p => p.PrediccionesKnockout)
                .ThenInclude(pk => pk.Partido)
            .Include(p => p.PrediccionesKnockout)
                .ThenInclude(pk => pk.EquipoLocalPredichado)
            .Include(p => p.PrediccionesKnockout)
                .ThenInclude(pk => pk.EquipoVisitantePredichado)
            .Include(p => p.PrediccionesKnockout)
                .ThenInclude(pk => pk.EquipoGanador)
            .Include(p => p.PrediccionFinal)
            .FirstOrDefaultAsync(p => p.Id == planillaId && p.UserId == userId);
    }

    public async Task<List<Equipo>> GetEquiposAsync()
        => await db.Equipos.AsNoTracking()
            .OrderBy(e => e.Grupo).ThenBy(e => e.Nombre)
            .ToListAsync();

    // ── Grupos ───────────────────────────────────────────────────────────────
    public async Task GuardarGrupoAsync(int prediccionId, ResultadoPartido? resultado)
        => await db.PrediccionesGrupo
            .Where(p => p.Id == prediccionId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ResultadoPredicho, resultado));

    // ── R32 ──────────────────────────────────────────────────────────────────
    public async Task GuardarR32LocalAsync(int prediccionId, int? equipoId)
        => await db.PrediccionesKnockout
            .Where(p => p.Id == prediccionId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.EquipoLocalPredichoId, equipoId));

    public async Task GuardarR32VisitanteAsync(int prediccionId, int? equipoId)
        => await db.PrediccionesKnockout
            .Where(p => p.Id == prediccionId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.EquipoVisitantePredichoId, equipoId));

    // ── R16+ ─────────────────────────────────────────────────────────────────
    public async Task GuardarGanadorAsync(int prediccionId, int? equipoId)
        => await db.PrediccionesKnockout
            .Where(p => p.Id == prediccionId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.EquipoGanadorId, equipoId));

    // ── PrediccionFinal ───────────────────────────────────────────────────────
    public async Task GuardarFinalAsync(PrediccionFinal src)
        => await db.PrediccionesFinal
            .Where(p => p.Id == src.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.CampeonEquipoId, src.CampeonEquipoId)
                .SetProperty(p => p.SegundoLugarEquipoId, src.SegundoLugarEquipoId)
                .SetProperty(p => p.TercerLugarEquipoId, src.TercerLugarEquipoId)
                .SetProperty(p => p.CuartoLugarEquipoId, src.CuartoLugarEquipoId)
                .SetProperty(p => p.MasGoleadorEquipoId, src.MasGoleadorEquipoId)
                .SetProperty(p => p.MasGoleadoEquipoId, src.MasGoleadoEquipoId)
                .SetProperty(p => p.MenosGoleadoEquipoId, src.MenosGoleadoEquipoId)
                .SetProperty(p => p.GolesLocalGranFinal, src.GolesLocalGranFinal)
                .SetProperty(p => p.GolesVisitanteGranFinal, src.GolesVisitanteGranFinal)
                .SetProperty(p => p.GolesLocalSemi1, src.GolesLocalSemi1)
                .SetProperty(p => p.GolesVisitanteSemi1, src.GolesVisitanteSemi1)
                .SetProperty(p => p.GolesLocalSemi2, src.GolesLocalSemi2)
                .SetProperty(p => p.GolesVisitanteSemi2, src.GolesVisitanteSemi2));

    public async Task ActualizarEstadoAsync(int planillaId, EstadoPlanilla estado)
        => await db.Planillas
            .Where(p => p.Id == planillaId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, estado));

    // ── Reset total ───────────────────────────────────────────────────────────
    public async Task ResetTotalAsync(int planillaId)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await db.PrediccionesGrupo
                .Where(p => p.PlanillaId == planillaId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ResultadoPredicho, (ResultadoPartido?)null));

            await db.PrediccionesKnockout
                .Where(p => p.PlanillaId == planillaId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.EquipoLocalPredichoId, (int?)null)
                    .SetProperty(p => p.EquipoVisitantePredichoId, (int?)null)
                    .SetProperty(p => p.EquipoGanadorId, (int?)null));

            await db.PrediccionesFinal
                .Where(p => p.PlanillaId == planillaId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.CampeonEquipoId, (int?)null)
                    .SetProperty(p => p.SegundoLugarEquipoId, (int?)null)
                    .SetProperty(p => p.TercerLugarEquipoId, (int?)null)
                    .SetProperty(p => p.CuartoLugarEquipoId, (int?)null)
                    .SetProperty(p => p.MasGoleadorEquipoId, (int?)null)
                    .SetProperty(p => p.MasGoleadoEquipoId, (int?)null)
                    .SetProperty(p => p.MenosGoleadoEquipoId, (int?)null)
                    .SetProperty(p => p.GolesLocalGranFinal, (int?)null)
                    .SetProperty(p => p.GolesVisitanteGranFinal, (int?)null)
                    .SetProperty(p => p.GolesLocalSemi1, (int?)null)
                    .SetProperty(p => p.GolesVisitanteSemi1, (int?)null)
                    .SetProperty(p => p.GolesLocalSemi2, (int?)null)
                    .SetProperty(p => p.GolesVisitanteSemi2, (int?)null));

            await db.Planillas
                .Where(p => p.Id == planillaId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Estado, EstadoPlanilla.Asignada)
                    .SetProperty(p => p.PuntajeTotal, 0));

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}