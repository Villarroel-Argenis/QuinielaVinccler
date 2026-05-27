namespace QuinielaVinccler.UI.Web.Services;

public class LoteService(AppDbContext db)
{
    public async Task<Lote> CreateAsync(int cantidad)
    {
        var lote = new Lote
        {
            Codigo = $"L-{Random.Shared.Next(10000000, 99999999)}",
            Cantidad = cantidad
        };
        db.Lotes.Add(lote);
        await db.SaveChangesAsync();

        var planillas = new List<Planilla>();
        for (int i = 0; i < cantidad; i++)
        {
            string codigo;
            do { codigo = GenerarCodigo(); }
            while (await db.Planillas.AnyAsync(p => p.Codigo == codigo));

            planillas.Add(new Planilla
            {
                Codigo = codigo,
                LoteId = lote.Id
            });
        }
        db.Planillas.AddRange(planillas);
        await db.SaveChangesAsync();

        lote.Planillas = planillas;

        return lote;
    }

    public async Task<List<Lote>> GetAsync()
    {
        return await db.Lotes
            .Include(l => l.Planillas)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<(bool Exito, string Mensaje)> EliminarAsync(int loteId)
    {
        var lote = await db.Lotes
            .Include(l => l.Planillas)
            .FirstOrDefaultAsync(l => l.Id == loteId);

        if (lote is null) return (false, "Lote no encontrado.");

        if (lote.Planillas.Any(p => p.IsAssigned))
            return (false, "No se puede eliminar el lote porque tiene planillas asignadas.");

        db.Planillas.RemoveRange(lote.Planillas);
        db.Lotes.Remove(lote);
        await db.SaveChangesAsync();

        return (true, "Lote eliminado correctamente.");
    }


    private static string GenerarCodigo()
    {
        var numero = Random.Shared.Next(10000000, 99999999);
        return $"P-{numero}";
    }

}
