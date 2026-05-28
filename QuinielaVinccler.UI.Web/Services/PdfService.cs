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