namespace QuinielaVinccler.UI.Web.Services;

public class PdfService(IWebHostEnvironment env)
{
    public byte[] GenerarLotePdf(Lote lote)
    {
        var anverso = Path.Combine(env.WebRootPath, "images", "ANVERSO.jpg");
        var reverso = Path.Combine(env.WebRootPath, "images", "REVERSO.jpg");

        return Document.Create(container =>
        {
            foreach (var planilla in lote.Planillas)
            {
                // ── ANVERSO ──
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

                    page.Content().Image(anverso).FitArea();

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Vinccler C.A. — Quiniela FIFA 2026")
                            .FontSize(7).FontColor("#666666");
                        row.ConstantItem(150).Text($"Codigo: {planilla.Codigo}")
                            .FontSize(7).Bold().AlignRight();
                    });
                });

                // ── REVERSO ──
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

                    page.Content().Image(reverso).FitArea();

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Vinccler C.A. — Quiniela FIFA 2026")
                            .FontSize(7).FontColor("#666666");
                        row.ConstantItem(150).Text($"Codigo: {planilla.Codigo}")
                            .FontSize(7).Bold().AlignRight();
                    });
                });
            }
        }).GeneratePdf();
    }
}