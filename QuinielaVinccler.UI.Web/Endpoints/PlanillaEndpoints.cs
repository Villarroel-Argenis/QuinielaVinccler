// Endpoints/PlanillaEndpoints.cs
namespace QuinielaVinccler.UI.Web.Endpoints;

public static class PlanillaEndpoints
{
    public static void MapPlanillaEndpoints(this WebApplication app)
    {
        app.MapGet("/api/planilla/{planillaId}/pdf", async (
            int planillaId,
            HttpContext http,
            AppDbContext db,
            [FromServices] IPdfService pdfService) =>
        {
            var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var planilla = await db.Planillas
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.User)
                .Include(p => p.Lote)
                .Include(p => p.PrediccionesGrupo)
                    .ThenInclude(pg => pg.Partido)
                        .ThenInclude(p => p.EquipoLocal)
                .Include(p => p.PrediccionesGrupo)
                    .ThenInclude(pg => pg.Partido)
                        .ThenInclude(p => p.EquipoVisitante)
                .Include(p => p.PrediccionesKnockout)
                    .ThenInclude(pk => pk.Partido)
                .Include(p => p.PrediccionesKnockout)
                    .ThenInclude(pk => pk.EquipoLocalPredichado)
                .Include(p => p.PrediccionesKnockout)
                    .ThenInclude(pk => pk.EquipoVisitantePredichado)
                .Include(p => p.PrediccionFinal)
                .FirstOrDefaultAsync(p => p.Id == planillaId && p.UserId == userId);

            if (planilla is null) return Results.NotFound();

            var equipos = await db.Equipos
                .AsNoTracking()
                .ToDictionaryAsync(e => e.Id, e => e.Nombre);

            var pdf = await Task.Run(() => pdfService.GenerarPlanillaPdf(planilla, equipos));

            return Results.File(pdf, "application/pdf", $"Planilla-{planilla.Codigo}.pdf");
        })
        .RequireAuthorization("Registrado");
    }
}