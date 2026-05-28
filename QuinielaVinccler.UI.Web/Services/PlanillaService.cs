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
                EquipoPredichoId = null,
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

        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            // Elimina predicciones — el cascade de FK se encarga,
            // pero lo hacemos explícito para evitar depender de la configuración del servidor
            await db.PrediccionesGrupo
                .Where(p => p.PlanillaId == planillaId)
                .ExecuteDeleteAsync();

            await db.PrediccionesKnockout
                .Where(p => p.PlanillaId == planillaId)
                .ExecuteDeleteAsync();

            await db.PrediccionesFinal
                .Where(p => p.PlanillaId == planillaId)
                .ExecuteDeleteAsync();

            // Resetea la planilla
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
}