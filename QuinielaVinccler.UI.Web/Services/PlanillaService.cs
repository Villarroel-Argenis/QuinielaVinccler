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
        // Normaliza el código
        codigo = codigo.Trim().ToUpper();

        // Verifica que la quiniela esté abierta
        if (await configuracion.QuinielaCerradaAsync())
            return (false, "La quiniela está cerrada. No se pueden vincular más planillas.");

        // Busca la planilla
        var planilla = await db.Planillas
            .FirstOrDefaultAsync(p => p.Codigo == codigo);

        if (planilla is null)
            return (false, "Código no encontrado. Verifica que lo hayas ingresado correctamente.");

        if (planilla.IsAssigned)
            return (false, "Esta planilla ya fue vinculada a otra cuenta.");

        // Obtiene los IDs de todos los partidos
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
            // Vincula la planilla al usuario
            planilla.UserId = userId;
            planilla.AssignedAt = DateTime.UtcNow;
            planilla.Estado = EstadoPlanilla.Asignada;

            // Crea registros vacíos de predicciones de grupo (72 filas)
            var prediccionesGrupo = idsGrupos.Select(partidoId => new PrediccionGrupo
            {
                PlanillaId = planilla.Id,
                PartidoId = partidoId,
                ResultadoPredicho = null,
                PuntosObtenidos = null,
            }).ToList();

            // Crea registros vacíos de predicciones knockout (32 filas)
            var prediccionesKnockout = idsKnockout.Select(partidoId => new PrediccionKnockout
            {
                PlanillaId = planilla.Id,
                PartidoId = partidoId,
                EquipoPredichoId = null,
                PuntosObtenidos = null,
            }).ToList();

            // Crea el registro de predicción final (1 fila)
            var prediccionFinal = new PrediccionFinal
            {
                PlanillaId = planilla.Id,
            };

            db.PrediccionesGrupo.AddRange(prediccionesGrupo);
            db.PrediccionesKnockout.AddRange(prediccionesKnockout);
            db.PrediccionesFinal.Add(prediccionFinal);

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