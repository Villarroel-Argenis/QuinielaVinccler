namespace QuinielaVinccler.UI.Web.Services;

public class PdfService(IWebHostEnvironment env) : IPdfService
{
    public byte[] GenerarLotePdf(Lote lote)
    {
        // Lee ambas imágenes una sola vez antes del loop
        var anversoBytes = File.ReadAllBytes(
            Path.Combine(env.WebRootPath, "images", "ANVERSO.jpg"));
        var reversoBytes = File.ReadAllBytes(
            Path.Combine(env.WebRootPath, "images", "REVERSO.jpg"));

        return Document.Create(container =>
        {
            foreach (var planilla in lote.Planillas)
            {
                BuildPagina(container, planilla, lote, anversoBytes);
                BuildPagina(container, planilla, lote, reversoBytes);
            }
        }).GeneratePdf();
    }

    public byte[] GenerarReportePlanillasPdf(
        List<(Planilla Planilla, string NombreUsuario, int CamposCompletos)> datos,
        string filtroLabel)
    {
        // Agrupar por lote manteniendo orden
        var porLote = datos
            .GroupBy(d => d.Planilla.Lote?.Codigo ?? "Sin Lote")
            .OrderBy(g => g.First().Planilla.Lote?.CreatedAt ?? DateTime.MinValue)
            .ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Listado de Planillas")
                                .FontSize(16).Bold();
                            c.Item().Text($"Filtro: {filtroLabel}")
                                .FontSize(9).FontColor("#666666");
                            c.Item().Text($"Total: {datos.Count} planillas en {porLote.Count} lotes")
                                .FontSize(9).FontColor("#666666");
                            c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8).FontColor("#999999");
                        });
                    });
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor("#2E7D32");
                    col.Item().PaddingBottom(4);
                });

                page.Content().Column(col =>
                {
                    foreach (var loteGrupo in porLote)
                    {
                        // ── Header del lote ──────────────────────────────────
                        col.Item().PaddingTop(12).PaddingBottom(4).Row(row =>
                        {
                            row.RelativeItem().Text(loteGrupo.Key)
                                .FontSize(11).Bold().FontColor("#2E7D32");
                            row.ConstantItem(100).AlignRight()
                                .Text($"{loteGrupo.Count()} planillas")
                                .FontSize(8).FontColor("#666666");
                        });

                        // ── Tabla del lote ───────────────────────────────────
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(25);   // #
                                cols.RelativeColumn(2);    // Código
                                cols.RelativeColumn(3);    // Usuario
                                cols.ConstantColumn(70);   // Estado
                                cols.ConstantColumn(55);   // Progreso
                            });

                            static IContainer HeaderCell(IContainer c) =>
                                c.DefaultTextStyle(x => x.Bold().FontSize(8))
                                 .Background("#E8F5E9")
                                 .BorderBottom(1).BorderColor("#2E7D32")
                                 .Padding(3);

                            table.Header(h =>
                            {
                                h.Cell().Element(HeaderCell).Text("#").FontColor("#2E7D32");
                                h.Cell().Element(HeaderCell).Text("Código").FontColor("#2E7D32");
                                h.Cell().Element(HeaderCell).Text("Usuario").FontColor("#2E7D32");
                                h.Cell().Element(HeaderCell).Text("Estado").FontColor("#2E7D32");
                                h.Cell().Element(HeaderCell).Text("Progreso").FontColor("#2E7D32");
                            });

                            var lista = loteGrupo.ToList();
                            for (int i = 0; i < lista.Count; i++)
                            {
                                var (planilla, nombre, campos) = lista[i];
                                var bg = i % 2 == 0 ? "#FFFFFF" : "#F9FBE7";

                                static IContainer DataCell(IContainer c, string bg) =>
                                    c.Background(bg)
                                     .Padding(3)
                                     .BorderBottom(0.5f)
                                     .BorderColor("#E0E0E0");

                                var estadoLabel = planilla.IsAssigned ? "Asignada" : "Sin asignar";
                                var estadoColor = planilla.IsAssigned ? "#2E7D32" : "#E65100";
                                var progresoLabel = planilla.IsAssigned ? $"{campos}/149" : "—";

                                table.Cell().Element(c => DataCell(c, bg))
                                    .Text((i + 1).ToString()).FontColor("#999999");
                                table.Cell().Element(c => DataCell(c, bg))
                                    .Text(planilla.Codigo).Bold();
                                table.Cell().Element(c => DataCell(c, bg))
                                    .Text(string.IsNullOrEmpty(nombre) ? "—" : nombre);
                                table.Cell().Element(c => DataCell(c, bg))
                                    .Text(estadoLabel).FontColor(estadoColor).Bold();
                                table.Cell().Element(c => DataCell(c, bg))
                                    .Text(progresoLabel);
                            }
                        });
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text("Vinccler C.A. — Quiniela FIFA 2026")
                        .FontSize(7).FontColor("#666666");
                    row.ConstantItem(100).AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(7).FontColor("#666666");
                        text.CurrentPageNumber().FontSize(7).FontColor("#666666");
                        text.Span(" de ").FontSize(7).FontColor("#666666");
                        text.TotalPages().FontSize(7).FontColor("#666666");
                    });
                });
            });
        }).GeneratePdf();
    }
    private static void BuildPagina(
        IDocumentContainer container,
        Planilla planilla,
        Lote lote,
        byte[] imagenBytes)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.Legal.Landscape());
            page.Margin(1, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(7.5f));

            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Quiniela FIFA World Cup 2026")
                            .FontSize(14).Bold();
                        c.Item().Text($"Codigo: {planilla.Codigo}")
                            .FontSize(11).Bold().FontColor("#2E7D32");
                        c.Item().Text($"Lote: {lote.Codigo}")
                            .FontSize(8).FontColor("#666666");
                    });
                });
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#2E7D32");
                col.Item().PaddingBottom(4);
            });

            page.Content().Image(imagenBytes).FitArea();

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text("Vinccler C.A. — Quiniela FIFA 2026")
                    .FontSize(7).FontColor("#666666");
                row.ConstantItem(150).Text($"Codigo: {planilla.Codigo}")
                    .FontSize(7).Bold().AlignRight();
            });
        });
    }
}