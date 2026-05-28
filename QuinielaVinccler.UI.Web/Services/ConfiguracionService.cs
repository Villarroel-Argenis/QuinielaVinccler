namespace QuinielaVinccler.UI.Web.Services;

public class ConfiguracionService(AppDbContext db) : IConfiguracionService
{
    public async Task<string?> GetAsync(string clave)
    {
        var config = await db.Configuraciones
            .FirstOrDefaultAsync(c => c.Clave == clave);
        return config?.Valor;
    }

    public async Task SetAsync(string clave, string valor)
    {
        var config = await db.Configuraciones
            .FirstOrDefaultAsync(c => c.Clave == clave);

        if (config is null)
        {
            db.Configuraciones.Add(new Configuracion { Clave = clave, Valor = valor });
        }
        else
        {
            config.Valor = valor;
        }

        await db.SaveChangesAsync();
    }

    public async Task<bool> QuinielaCerradaAsync()
    {
        // Cerrada si el flag global está en true O si pasó la fecha límite
        var cerradaStr = await GetAsync(ConfiguracionKeys.QuinielaCerrada);
        if (cerradaStr == "true") return true;

        var fechaStr = await GetAsync(ConfiguracionKeys.FechaCierreUtc);
        if (fechaStr is not null && DateTimeOffset.TryParse(fechaStr, out var fecha))
            return DateTimeOffset.UtcNow >= fecha;

        return false;
    }

    public async Task<bool> PuedeEditarPlanillaAsync(int planillaId)
    {
        // Verifica cierre global primero (más barato)
        if (await QuinielaCerradaAsync()) return false;

        // Verifica estado individual de la planilla
        var estado = await db.Planillas
            .Where(p => p.Id == planillaId)
            .Select(p => p.Estado)
            .FirstOrDefaultAsync();

        return estado is not EstadoPlanilla.Cerrada and not EstadoPlanilla.SinAsignar;
    }
}