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
    }
}