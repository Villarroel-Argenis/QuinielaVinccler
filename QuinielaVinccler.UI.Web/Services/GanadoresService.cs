namespace QuinielaVinccler.UI.Web.Services;

public class GanadoresService(
    AppDbContext db,
    IConfiguracionService configSvc) : IGanadoresService
{
    private const decimal PremioBase1ro = 2000m;
    private const decimal PremioBase2do = 1000m;
    private const decimal PremioBase3ro = 500m;
    private const decimal PremioMenor = 2000m;

    public async Task<GanadoresResultado> CalcularGanadoresAsync()
    {
        // ── Monto recaudado desde Configuracion ─────────────────────────────
        decimal recaudado = 0;
        var recaudadoStr = await configSvc.GetAsync(ConfiguracionKeys.MontoRecaudado);
        if (decimal.TryParse(recaudadoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
            recaudado = r;

        var premios = new CalculoPremios(
            MontoRecaudado: recaudado,
            Premio1ro: PremioBase1ro + (recaudado * 0.50m),
            Premio2do: PremioBase2do + (recaudado * 0.30m),
            Premio3ro: PremioBase3ro + (recaudado * 0.20m),
            PremioMenor: PremioMenor
        );

        // ── Cargar planillas vinculadas ordenadas por puntaje ─────────────
        var planillas = await db.Planillas
            .AsNoTracking()
            .Where(p => p.UserId != null && p.PuntajeTotal > 0)
            .Include(p => p.User)
            .OrderByDescending(p => p.PuntajeTotal)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.PuntajeTotal,
                NombreUsuario = p.User != null ? p.User.FullName : "—"
            })
            .ToListAsync();

        var ganadores = new List<GanadorDto>();

        if (planillas.Count == 0)
            return new GanadoresResultado(premios, ganadores);

        // ── Agrupar por puntaje para detectar empates ──────────────────────
        // OPCION B: empates dividen el premio, otros se mantienen.
        // Las planillas empatadas comparten una posición; la siguiente toma la
        // posición consecutiva normal.
        //
        // Ej: puntajes 100, 100, 90, 80
        //     - 100 y 100 → 1er premio dividido entre 2
        //     - 90        → 2do premio completo
        //     - 80        → 3er premio completo

        var gruposPuntaje = planillas
            .GroupBy(p => p.PuntajeTotal)
            .OrderByDescending(g => g.Key)
            .ToList();

        var premioPorPosicion = new (TipoPremio Tipo, decimal Monto)[]
        {
            (TipoPremio.Primero, premios.Premio1ro),
            (TipoPremio.Segundo, premios.Premio2do),
            (TipoPremio.Tercero, premios.Premio3ro)
        };

        // Iterar primeros 3 grupos para premios 1/2/3
        for (int i = 0; i < Math.Min(3, gruposPuntaje.Count); i++)
        {
            var grupo = gruposPuntaje[i];
            var (tipo, monto) = premioPorPosicion[i];
            var cantidad = grupo.Count();
            var montoPorPersona = monto / cantidad;

            foreach (var planilla in grupo)
            {
                ganadores.Add(new GanadorDto(
                    Tipo: tipo,
                    Posicion: i + 1,
                    CodigoPlanilla: planilla.Codigo,
                    PlanillaId: planilla.Id,
                    NombreUsuario: planilla.NombreUsuario,
                    PuntajeTotal: planilla.PuntajeTotal,
                    MontoBase: monto,
                    MontoGanado: montoPorPersona,
                    CantidadEmpatados: cantidad
                ));
            }
        }

        // ── Menor puntaje ────────────────────────────────────────────────────
        // Solo si hay al menos un participante más allá de los primeros 3 grupos
        // (sino el menor puntaje sería alguien que ya ganó otro premio)
        var menorPuntaje = gruposPuntaje.Last().Key;
        var yaPremiados = ganadores.Select(g => g.PlanillaId).ToHashSet();
        var grupoMenor = gruposPuntaje.Last();

        // Si el grupo del menor puntaje no está entre los primeros 3 (ya premiados)
        if (gruposPuntaje.Count > 3)
        {
            var cantMenor = grupoMenor.Count();
            var montoPorPersona = premios.PremioMenor / cantMenor;
            var posicionMenor = planillas.Count - cantMenor + 1;

            foreach (var planilla in grupoMenor)
            {
                ganadores.Add(new GanadorDto(
                    Tipo: TipoPremio.MenorPuntaje,
                    Posicion: posicionMenor,
                    CodigoPlanilla: planilla.Codigo,
                    PlanillaId: planilla.Id,
                    NombreUsuario: planilla.NombreUsuario,
                    PuntajeTotal: planilla.PuntajeTotal,
                    MontoBase: premios.PremioMenor,
                    MontoGanado: montoPorPersona,
                    CantidadEmpatados: cantMenor
                ));
            }
        }

        return new GanadoresResultado(premios, ganadores);
    }
}
