namespace QuinielaVinccler.UI.Web.Endpoints;

public static class LoteEndpoints
{
    public static void MapLoteEndpoints(this WebApplication app)
    {
        app.MapGet("/api/lote/{loteId}/pdf", async (
         int loteId,
         AppDbContext db,
         [FromServices] IPdfService pdfService,
         [FromServices] ILogger<Program> logger) =>
        {
            var lote = await db.Lotes
                .Include(l => l.Planillas)
                .FirstOrDefaultAsync(l => l.Id == loteId);

            if (lote is null) return Results.NotFound();

            var sw = Stopwatch.StartNew();
            var pdf = await Task.Run(() => pdfService.GenerarLotePdf(lote));
            sw.Stop();

            logger.LogInformation(
                "PDF generado: {Planillas} planillas en {Ms}ms ({Kb}KB)",
                lote.Planillas.Count,
                sw.ElapsedMilliseconds,
                pdf.Length / 1024);

            return Results.File(pdf, "application/pdf", $"Lote-{lote.Codigo}.pdf");
        })
     .RequireAuthorization("SoloAdmin");

        app.MapGet("/api/reporte/planillas", async (
    [FromQuery] string? filtro,
    AppDbContext db,
    [FromServices] IPdfService pdfService,
    [FromServices] ILoteService loteService) =>
        {
            // Cargar todas las planillas con usuario y lote
            var planillas = await db.Planillas
                .Include(p => p.User)
                .Include(p => p.Lote)
                .OrderBy(p => p.Lote!.CreatedAt)
                .ThenBy(p => p.Codigo)
                .ToListAsync();

            // Aplicar filtro
            var filtradas = filtro switch
            {
                "Asignadas" => planillas.Where(p => p.UserId != null).ToList(),
                "SinAsignar" => planillas.Where(p => p.UserId == null).ToList(),
                _ => planillas
            };

            var filtroLabel = filtro switch
            {
                "Asignadas" => "Solo asignadas",
                "SinAsignar" => "Solo sin asignar",
                _ => "Todas"
            };

            // Obtener progreso solo de las asignadas
            var idsAsignadas = filtradas
                .Where(p => p.UserId != null)
                .Select(p => p.Id)
                .ToList();

            var progreso = idsAsignadas.Count > 0
                ? await loteService.GetProgresoPlanillasAsync(idsAsignadas)
                : new Dictionary<int, int>();

            var datos = filtradas.Select(p => (
                Planilla: p,
                NombreUsuario: p.User?.FullName ?? "",
                CamposCompletos: progreso.GetValueOrDefault(p.Id, 0)
            )).ToList();

            var pdf = await Task.Run(() =>
                pdfService.GenerarReportePlanillasPdf(datos, filtroLabel));

            return Results.File(pdf, "application/pdf", "Reporte-Planillas.pdf");
        })
.RequireAuthorization("SoloAdmin");
    }
}