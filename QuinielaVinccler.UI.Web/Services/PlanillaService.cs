namespace QuinielaVinccler.UI.Web.Services;

public class PlanillaService(AppDbContext db, IConfiguracionService configuracion) : IPlanillaService
{
    public async Task<List<Planilla>> GetPlanillasByUserAsync(int userId)
    {
        return await db.Planillas
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.AssignedAt)
            .ToListAsync();
    }

    public async Task<(bool Exito, string? Error)> VincularAsync(string codigo, int userId)
    {
        codigo = codigo.Trim().ToUpper();

        if (await configuracion.QuinielaCerradaAsync())
            return (false, "La quiniela está cerrada. No se pueden vincular más planillas.");

        var planilla = await db.Planillas
            .FirstOrDefaultAsync(p => p.Codigo == codigo);

        if (planilla is null)
            return (false, "Código no encontrado. Verifica que lo hayas ingresado correctamente.");

        if (planilla.IsAssigned)
            return (false, "Esta planilla ya fue vinculada a otra cuenta.");

        var idsGrupos = await db.Partidos
            .Where(p => p.Fase == Fase.Grupos)
            .Select(p => p.Id)
            .ToListAsync();

        var idsKnockout = await db.Partidos
            .Where(p => p.Fase != Fase.Grupos)
            .Select(p => p.Id)
            .ToListAsync();

        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            planilla.UserId = userId;
            planilla.AssignedAt = DateTime.UtcNow;
            planilla.Estado = EstadoPlanilla.Asignada;

            db.PrediccionesGrupo.AddRange(idsGrupos.Select(id => new PrediccionGrupo
            {
                PlanillaId = planilla.Id,
                PartidoId = id,
                ResultadoPredicho = null,
                PuntosObtenidos = null,
            }));

            db.PrediccionesKnockout.AddRange(idsKnockout.Select(id => new PrediccionKnockout
            {
                PlanillaId = planilla.Id,
                PartidoId = id,
                EquipoLocalPredichoId = null,
                EquipoVisitantePredichoId = null,
                EquipoGanadorId = null,
                PuntosObtenidos = null,
            }));

            db.PrediccionesFinal.Add(new PrediccionFinal { PlanillaId = planilla.Id });

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, null);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Desvinculación por usuario ────────────────────────────────────────────
    public async Task<(bool Exito, string? Error)> DesvincularAsync(int planillaId, int userId)
    {
        var planilla = await db.Planillas
            .FirstOrDefaultAsync(p => p.Id == planillaId && p.UserId == userId);

        if (planilla is null)
            return (false, "Planilla no encontrada.");

        if (planilla.Estado == EstadoPlanilla.Cerrada)
            return (false, "No se puede desvincular una planilla cerrada.");

        if (await configuracion.QuinielaCerradaAsync())
            return (false, "La quiniela está cerrada. No se pueden desvincular planillas.");

        return await EjecutarDesvinculacion(planilla);
    }

    // ── Desvinculación por admin (sin restricciones) ──────────────────────────
    public async Task<(bool Exito, string? Error)> DesvincularAdminAsync(int planillaId)
    {
        var planilla = await db.Planillas
            .FirstOrDefaultAsync(p => p.Id == planillaId);

        if (planilla is null)
            return (false, "Planilla no encontrada.");

        if (!planilla.IsAssigned)
            return (false, "La planilla no está vinculada a ningún usuario.");

        return await EjecutarDesvinculacion(planilla);
    }

    // ── Búsqueda para panel admin ─────────────────────────────────────────────
    public async Task<List<PlanillaAdminDto>> BuscarPlanillasAsync(string termino)
    {
        termino = termino.Trim().ToUpper();

        var query = db.Planillas
            .Where(p => p.UserId != null)
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(termino))
        {
            query = query.Where(p =>
                p.Codigo.Contains(termino) ||
                p.User!.FullName.ToUpper().Contains(termino) ||
                p.User!.Email.ToUpper().Contains(termino) ||
                p.User!.CI.Contains(termino));
        }

        return await query
            .OrderByDescending(p => p.AssignedAt)
            .Take(50)
            .Select(p => new PlanillaAdminDto(
                p.Id,
                p.Codigo,
                p.Estado,
                p.PuntajeTotal,
                p.AssignedAt,
                p.User!.FullName,
                p.User!.Email,
                p.User!.CI))
            .ToListAsync();
    }

    // ── Lógica compartida de desvinculación ───────────────────────────────────
    private async Task<(bool Exito, string? Error)> EjecutarDesvinculacion(Planilla planilla)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            await db.PrediccionesGrupo
                .Where(p => p.PlanillaId == planilla.Id)
                .ExecuteDeleteAsync();

            await db.PrediccionesKnockout
                .Where(p => p.PlanillaId == planilla.Id)
                .ExecuteDeleteAsync();

            await db.PrediccionesFinal
                .Where(p => p.PlanillaId == planilla.Id)
                .ExecuteDeleteAsync();

            planilla.UserId = null;
            planilla.AssignedAt = null;
            planilla.Estado = EstadoPlanilla.SinAsignar;
            planilla.PuntajeTotal = 0;

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, null);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Agregar este método a PlanillaService ──────────────────────────────────
    // (resto de la clase sin cambios)

    public async Task<List<RankingItemDto>> GetRankingAsync(int userId)
    {
        var permitirIncompletas = await configuracion.PermitirIncompletasEnRankingAsync();

        var query = db.Planillas
            .AsNoTracking()
            .Where(p => p.UserId != null)
            .Include(p => p.User)
            .AsQueryable();

        if (!permitirIncompletas)
        {
            query = query.Where(p =>
                db.PrediccionesGrupo.Count(pg =>
                    pg.PlanillaId == p.Id && pg.ResultadoPredicho != null) == 72 &&
                db.PrediccionesKnockout.Count(pk =>
                    pk.PlanillaId == p.Id && pk.EquipoLocalPredichoId != null) == 32 &&
                db.PrediccionesKnockout.Count(pk =>
                    pk.PlanillaId == p.Id && pk.EquipoVisitantePredichoId != null) == 32 &&
                db.PrediccionesFinal.Any(pf =>
                    pf.PlanillaId == p.Id &&
                    pf.CampeonEquipoId != null &&
                    pf.SegundoLugarEquipoId != null &&
                    pf.TercerLugarEquipoId != null &&
                    pf.CuartoLugarEquipoId != null &&
                    pf.MasGoleadorEquipoId != null &&
                    pf.MasGoleadoEquipoId != null &&
                    pf.MenosGoleadoEquipoId != null &&
                    pf.GolesLocalSemi1 != null &&
                    pf.GolesVisitanteSemi1 != null &&
                    pf.GolesLocalSemi2 != null &&
                    pf.GolesVisitanteSemi2 != null &&
                    pf.GolesLocalGranFinal != null &&
                    pf.GolesVisitanteGranFinal != null));
        }

        var raw = await query
            .OrderByDescending(p => p.PuntajeTotal)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.PuntajeTotal,
                p.UserId,
                Nombre = p.User != null ? p.User.FullName : "—"
            })
            .ToListAsync();

        var resultado = new List<RankingItemDto>(raw.Count);
        int posicion = 1;
        int puntajeAnterior = int.MinValue;
        int posicionActual = 1;

        for (int i = 0; i < raw.Count; i++)
        {
            var item = raw[i];

            if (i == 0)
            {
                posicionActual = 1;
                puntajeAnterior = item.PuntajeTotal;
            }
            else if (item.PuntajeTotal < puntajeAnterior)
            {
                posicionActual = posicion;
                puntajeAnterior = item.PuntajeTotal;
            }

            resultado.Add(new RankingItemDto(
                Posicion: posicionActual,
                Nombre: item.Nombre,
                CodigoPlanilla: item.Codigo,
                PlanillaId: item.Id,
                PuntajeTotal: item.PuntajeTotal,
                EsUsuarioActual: item.UserId == userId,
                UserId: item.UserId ?? 0
            ));

            posicion++;
        }

        return resultado;
    }
}