namespace QuinielaVinccler.UI.Web.Services;

public class LoteService(AppDbContext db, IConfiguracionService configuracion) : ILoteService
{
    public async Task<Lote> CreateAsync(int cantidad)
    {
        if (await configuracion.QuinielaCerradaAsync())
            throw new InvalidOperationException("La quiniela está cerrada. No se pueden generar más planillas.");

        var lote = new Lote
        {
            Codigo = $"L-{Random.Shared.Next(10000000, 99999999)}",
            Cantidad = cantidad
        };

        var codigos = await GenerarCodigosUnicosAsync(cantidad);
        var planillas = codigos.Select(codigo => new Planilla
        {
            Codigo = codigo,
            LoteId = lote.Id
        }).ToList();

        await using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            db.Lotes.Add(lote);
            await db.SaveChangesAsync();

            // Asigna el LoteId real después del primer SaveChanges
            planillas.ForEach(p => p.LoteId = lote.Id);

            db.Planillas.AddRange(planillas);
            await db.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        lote.Planillas = planillas;
        return lote;
    }

    public async Task<List<Lote>> GetAsync()
    {
        return await db.Lotes
            .Include(l => l.Planillas)
            .ThenInclude(p => p.User)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<(bool Exito, string Mensaje)> EliminarAsync(int loteId)
    {
        if (await configuracion.QuinielaCerradaAsync())
            return (false, "La quiniela está cerrada. No se pueden eliminar lotes.");

        var lote = await db.Lotes
            .Include(l => l.Planillas)
            .FirstOrDefaultAsync(l => l.Id == loteId);

        if (lote is null)
            return (false, "Lote no encontrado.");

        if (lote.Planillas.Any(p => p.IsAssigned))
            return (false, "No se puede eliminar el lote porque tiene planillas asignadas.");

        db.Planillas.RemoveRange(lote.Planillas);
        db.Lotes.Remove(lote);
        await db.SaveChangesAsync();

        return (true, "Lote eliminado correctamente.");
    }

    private async Task<List<string>> GenerarCodigosUnicosAsync(int cantidad)
    {
        // Genera el doble de candidatos para absorber colisiones sin loops adicionales
        var candidatos = Enumerable
            .Range(0, cantidad * 2)
            .Select(_ => GenerarCodigo())
            .Distinct()
            .ToList();

        // Una sola query para verificar cuáles ya existen en la DB
        var existentes = await db.Planillas
            .Where(p => candidatos.Contains(p.Codigo))
            .Select(p => p.Codigo)
            .ToHashSetAsync();

        var unicos = candidatos
            .Where(c => !existentes.Contains(c))
            .Take(cantidad)
            .ToList();

        // Fallback: si hubo demasiadas colisiones, completa uno por uno
        while (unicos.Count < cantidad)
        {
            var extra = GenerarCodigo();
            if (!existentes.Contains(extra) && !unicos.Contains(extra))
                unicos.Add(extra);
        }

        return unicos;
    }

    private static string GenerarCodigo() =>
        $"P-{Random.Shared.Next(10000000, 99999999)}";
}
