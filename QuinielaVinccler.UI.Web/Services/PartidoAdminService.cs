// Services/PartidoAdminService.cs
namespace QuinielaVinccler.UI.Web.Services;

public class PartidoAdminService(AppDbContext db) : IPartidoAdminService
{
    public Task<List<Partido>> GetPartidosGrupoAsync() =>
        db.Partidos
          .AsNoTracking()
          .Include(p => p.EquipoLocal)
          .Include(p => p.EquipoVisitante)
          .Where(p => p.Fase == Fase.Grupos)
          .OrderBy(p => p.NumeroPartido)
          .ToListAsync();

    public Task<List<Partido>> GetPartidosKnockoutAsync() =>
        db.Partidos
          .AsNoTracking()
          .Where(p => p.Fase != Fase.Grupos)
          .OrderBy(p => p.NumeroPartido)
          .ToListAsync();

    public async Task<(bool ok, string mensaje)> SwapLocalVisitanteAsync(int partidoId)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await db.Database.ExecuteSqlRawAsync("""
                UPDATE "Partidos" p
                SET
                    "EquipoLocalId"     = orig."EquipoVisitanteId",
                    "EquipoVisitanteId" = orig."EquipoLocalId"
                FROM (
                    SELECT "Id", "EquipoLocalId", "EquipoVisitanteId"
                    FROM "Partidos"
                    WHERE "Id" = {0}
                ) orig
                WHERE p."Id" = orig."Id"
                """, partidoId);

            await db.Database.ExecuteSqlRawAsync("""
                UPDATE "PrediccionesGrupo"
                SET "ResultadoPredicho" = CASE "ResultadoPredicho"
                    WHEN 'Uno' THEN 'Dos'
                    WHEN 'Dos' THEN 'Uno'
                    ELSE "ResultadoPredicho"
                END
                WHERE "PartidoId" = {0}
                  AND "ResultadoPredicho" IS NOT NULL
                """, partidoId);

            await tx.CommitAsync();
            return (true, "Swap aplicado correctamente.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool ok, string mensaje)> SwapNumerosAsync(int partidoIdA, int partidoIdB)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var a = await db.Partidos.FindAsync(partidoIdA);
            var b = await db.Partidos.FindAsync(partidoIdB);

            if (a is null || b is null)
                return (false, "Partido no encontrado.");

            if (a.Fase != Fase.Grupos || b.Fase != Fase.Grupos)
                return (false, "Solo se pueden renumerar partidos de fase de grupos.");

            int temp = a.NumeroPartido;
            a.NumeroPartido = 99999;
            await db.SaveChangesAsync();

            a.NumeroPartido = b.NumeroPartido;
            b.NumeroPartido = temp;
            await db.SaveChangesAsync();

            await tx.CommitAsync();
            return (true, $"Números {temp} y {a.NumeroPartido} intercambiados.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool ok, string mensaje)> CambiarSlotsAsync(int partidoId, string slotLocal, string slotVisitante)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(slotLocal) || string.IsNullOrWhiteSpace(slotVisitante))
                return (false, "Los slots no pueden estar vacíos.");

            if (slotLocal == slotVisitante)
                return (false, "SlotLocal y SlotVisitante no pueden ser iguales.");

            var partido = await db.Partidos.FindAsync(partidoId);
            if (partido is null)
                return (false, "Partido no encontrado.");

            partido.SlotLocal     = slotLocal;
            partido.SlotVisitante = slotVisitante;
            await db.SaveChangesAsync();
            return (true, "Slots actualizados correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }
}
