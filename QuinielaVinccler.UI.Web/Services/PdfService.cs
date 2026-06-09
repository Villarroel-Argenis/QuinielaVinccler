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

    public byte[] GenerarPlanillaPdf(Planilla planilla, Dictionary<int, string> equiposById)
    {
        var grupoMap = planilla.PrediccionesGrupo
            .GroupBy(pg => pg.Partido.EquipoLocal!.Grupo)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(pg => pg.Partido.NumeroPartido).ToList());

        var grupos = grupoMap.Keys.OrderBy(k => k).ToList();
        var gruposA = grupos.Take(6).ToList();
        var gruposB = grupos.Skip(6).ToList();

        var r32 = planilla.PrediccionesKnockout.Where(p => p.Partido.Fase == Fase.RoundOf32).OrderBy(p => p.Partido.NumeroPartido).ToList();
        var r16 = planilla.PrediccionesKnockout.Where(p => p.Partido.Fase == Fase.RoundOf16).OrderBy(p => p.Partido.NumeroPartido).ToList();
        var qf = planilla.PrediccionesKnockout.Where(p => p.Partido.Fase == Fase.Cuartos).OrderBy(p => p.Partido.NumeroPartido).ToList();
        var sf = planilla.PrediccionesKnockout.Where(p => p.Partido.Fase == Fase.Semis).OrderBy(p => p.Partido.NumeroPartido).ToList();
        var tp = planilla.PrediccionesKnockout.FirstOrDefault(p => p.Partido.Fase == Fase.TercerPuesto);
        var final = planilla.PrediccionesKnockout.FirstOrDefault(p => p.Partido.Fase == Fase.Final);
        var pf = planilla.PrediccionFinal;

        string NombreEquipo(int? id) =>
            id.HasValue && equiposById.TryGetValue(id.Value, out var n) ? n : "—";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Legal.Landscape());
                page.Margin(0.7f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(7f));

                // ── Header ──────────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("QUINIELA FIFA WORLD CUP 2026 — Vinccler C.A.")
                                .FontSize(10).Bold();
                            c.Item().Text($"Planilla: {planilla.Codigo}  |  Lote: {planilla.Lote?.Codigo ?? "—"}  |  Titular: {planilla.User?.FullName ?? "—"}")
                                .FontSize(7.5f).FontColor("#444444");
                        });
                        row.ConstantItem(120).AlignRight()
                            .Text($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(6.5f).FontColor("#888888");
                    });
                    col.Item().PaddingTop(2).LineHorizontal(1).LineColor("#2E7D32");
                    col.Item().PaddingBottom(2);
                });

                // ── Contenido en 3 columnas ──────────────────────────────────────
                page.Content().Row(root =>
                {
                    // ── Columna 1: Grupos A-F ────────────────────────────────────
                    root.RelativeItem(3).Column(col =>
                    {
                        col.Item().Text("FASE DE GRUPOS A-F (60 pts/partido)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(2);

                        foreach (var grupo in gruposA)
                        {
                            col.Item().Text($"Grupo {grupo}")
                                .FontSize(7f).Bold().FontColor("#555555");

                            col.Item().Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(3);
                                    c.ConstantColumn(13);
                                    c.ConstantColumn(13);
                                    c.ConstantColumn(13);
                                });

                                foreach (var pg in grupoMap[grupo])
                                {
                                    var pred = pg.ResultadoPredicho;

                                    t.Cell().PaddingVertical(0.5f).PaddingHorizontal(1)
                                        .Text(pg.Partido.EquipoLocal?.Nombre ?? "—").FontSize(6.5f);
                                    t.Cell().PaddingVertical(0.5f).PaddingHorizontal(1)
                                        .Text(pg.Partido.EquipoVisitante?.Nombre ?? "—").FontSize(6.5f);

                                    t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                        .Background(pred == ResultadoPartido.Uno ? "#2E7D32" : "#FFFFFF")
                                        .AlignCenter().PaddingVertical(0.5f)
                                        .Text("1").FontSize(6.5f)
                                        .FontColor(pred == ResultadoPartido.Uno ? "#FFFFFF" : "#333333");

                                    t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                        .Background(pred == ResultadoPartido.Equis ? "#2E7D32" : "#FFFFFF")
                                        .AlignCenter().PaddingVertical(0.5f)
                                        .Text("X").FontSize(6.5f)
                                        .FontColor(pred == ResultadoPartido.Equis ? "#FFFFFF" : "#333333");

                                    t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                        .Background(pred == ResultadoPartido.Dos ? "#2E7D32" : "#FFFFFF")
                                        .AlignCenter().PaddingVertical(0.5f)
                                        .Text("2").FontSize(6.5f)
                                        .FontColor(pred == ResultadoPartido.Dos ? "#FFFFFF" : "#333333");
                                }
                            });
                            col.Item().PaddingBottom(2);
                        }
                    });

                    root.ConstantItem(6);

                    // ── Columna 2: Grupos G-L ────────────────────────────────────
                    root.RelativeItem(3).Column(col =>
                    {
                        col.Item().Text("FASE DE GRUPOS G-L (60 pts/partido)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(2);

                        foreach (var grupo in gruposB)
                        {
                            col.Item().Text($"Grupo {grupo}")
                                .FontSize(7f).Bold().FontColor("#555555");

                            col.Item().Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(3);
                                    c.ConstantColumn(13);
                                    c.ConstantColumn(13);
                                    c.ConstantColumn(13);
                                });

                                foreach (var pg in grupoMap[grupo])
                                {
                                    var pred = pg.ResultadoPredicho;

                                    t.Cell().PaddingVertical(0.5f).PaddingHorizontal(1)
                                        .Text(pg.Partido.EquipoLocal?.Nombre ?? "—").FontSize(6.5f);
                                    t.Cell().PaddingVertical(0.5f).PaddingHorizontal(1)
                                        .Text(pg.Partido.EquipoVisitante?.Nombre ?? "—").FontSize(6.5f);

                                    t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                        .Background(pred == ResultadoPartido.Uno ? "#2E7D32" : "#FFFFFF")
                                        .AlignCenter().PaddingVertical(0.5f)
                                        .Text("1").FontSize(6.5f)
                                        .FontColor(pred == ResultadoPartido.Uno ? "#FFFFFF" : "#333333");

                                    t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                        .Background(pred == ResultadoPartido.Equis ? "#2E7D32" : "#FFFFFF")
                                        .AlignCenter().PaddingVertical(0.5f)
                                        .Text("X").FontSize(6.5f)
                                        .FontColor(pred == ResultadoPartido.Equis ? "#FFFFFF" : "#333333");

                                    t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                        .Background(pred == ResultadoPartido.Dos ? "#2E7D32" : "#FFFFFF")
                                        .AlignCenter().PaddingVertical(0.5f)
                                        .Text("2").FontSize(6.5f)
                                        .FontColor(pred == ResultadoPartido.Dos ? "#FFFFFF" : "#333333");
                                }
                            });
                            col.Item().PaddingBottom(2);
                        }
                    });

                    root.ConstantItem(6);

                    // ── Columna 3: Knockout + Final ──────────────────────────────
                    root.RelativeItem(2).Column(col =>
                    {
                        void FilaKo(ColumnDescriptor c, string label, string? local, string? visitante)
                        {
                            c.Item().Table(t =>
                            {
                                t.ColumnsDefinition(cd =>
                                {
                                    cd.ConstantColumn(38);
                                    cd.RelativeColumn();
                                    cd.RelativeColumn();
                                });
                                t.Cell().PaddingVertical(0.5f)
                                    .Text(label).FontSize(6f).FontColor("#888888");
                                t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                    .Background(local != null ? "#E8F5E9" : "#FFFFFF")
                                    .Padding(0.5f).Text(local ?? "").FontSize(6f);
                                t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                    .Background(visitante != null ? "#E8F5E9" : "#FFFFFF")
                                    .Padding(0.5f).Text(visitante ?? "").FontSize(6f);
                            });
                        }

                        // R32
                        col.Item().Text("DIECISEISAVOS (70 pts/casilla)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(1);
                        foreach (var pk in r32)
                            FilaKo(col, $"P#{pk.Partido.NumeroPartido}",
                                pk.EquipoLocalPredichado?.Nombre,
                                pk.EquipoVisitantePredichado?.Nombre);
                        col.Item().PaddingBottom(2);

                        // R16
                        col.Item().Text("OCTAVOS (70 pts/casilla)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(1);
                        foreach (var pk in r16)
                            FilaKo(col, $"P#{pk.Partido.NumeroPartido}",
                                pk.EquipoLocalPredichado?.Nombre,
                                pk.EquipoVisitantePredichado?.Nombre);
                        col.Item().PaddingBottom(2);

                        // Cuartos
                        col.Item().Text("CUARTOS (80 pts/casilla)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(1);
                        foreach (var pk in qf)
                            FilaKo(col, $"P#{pk.Partido.NumeroPartido}",
                                pk.EquipoLocalPredichado?.Nombre,
                                pk.EquipoVisitantePredichado?.Nombre);
                        col.Item().PaddingBottom(2);

                        // Semis
                        col.Item().Text("SEMIFINALES (100 pts/casilla)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(1);
                        foreach (var pk in sf)
                            FilaKo(col, $"P#{pk.Partido.NumeroPartido}",
                                pk.EquipoLocalPredichado?.Nombre,
                                pk.EquipoVisitantePredichado?.Nombre);
                        col.Item().PaddingBottom(2);

                        // 3°/4°
                        col.Item().Text("3° Y 4° PUESTO (100 pts/casilla)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(1);
                        if (tp is not null)
                            FilaKo(col, $"P#{tp.Partido.NumeroPartido}",
                                tp.EquipoLocalPredichado?.Nombre,
                                tp.EquipoVisitantePredichado?.Nombre);
                        col.Item().PaddingBottom(2);

                        // Gran Final
                        col.Item().Text("GRAN FINAL (100 pts/casilla)")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(1);
                        if (final is not null)
                            FilaKo(col, $"P#{final.Partido.NumeroPartido}",
                                final.EquipoLocalPredichado?.Nombre,
                                final.EquipoVisitantePredichado?.Nombre);
                        col.Item().PaddingBottom(2);

                        // Posiciones Finales
                        col.Item().Text("POSICIONES FINALES")
                            .FontSize(7.5f).Bold().FontColor("#2E7D32");
                        col.Item().PaddingBottom(1);

                        void FilaFinal(ColumnDescriptor c, string label, int? equipoId, int pts)
                        {
                            c.Item().Table(t =>
                            {
                                t.ColumnsDefinition(cd =>
                                {
                                    cd.ConstantColumn(75);
                                    cd.RelativeColumn();
                                    cd.ConstantColumn(30);
                                });
                                t.Cell().PaddingVertical(0.5f)
                                    .Text(label).FontSize(6f).FontColor("#555555");
                                t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                    .Background(equipoId != null ? "#E8F5E9" : "#FFFFFF")
                                    .Padding(0.5f).Text(NombreEquipo(equipoId)).FontSize(6f);
                                t.Cell().PaddingVertical(0.5f).AlignRight()
                                    .Text($"{pts}p").FontSize(5.5f).FontColor("#888888");
                            });
                        }

                        if (pf is not null)
                        {
                            FilaFinal(col, "Campeon", pf.CampeonEquipoId, 300);
                            FilaFinal(col, "2do Lugar", pf.SegundoLugarEquipoId, 200);
                            FilaFinal(col, "3er Lugar", pf.TercerLugarEquipoId, 100);
                            FilaFinal(col, "4to Lugar", pf.CuartoLugarEquipoId, 50);
                            FilaFinal(col, "Mas Goleador", pf.MasGoleadorEquipoId, 100);
                            FilaFinal(col, "Mas Goleado", pf.MasGoleadoEquipoId, 100);
                            FilaFinal(col, "Menos Goleado", pf.MenosGoleadoEquipoId, 100);

                            col.Item().PaddingBottom(2);
                            col.Item().Text("MARCADORES EXACTOS (100 pts c/u)")
                                .FontSize(7.5f).Bold().FontColor("#2E7D32");
                            col.Item().PaddingBottom(1);

                            void FilaMarcador(ColumnDescriptor c, string label, int? gl, int? gv)
                            {
                                var valor = (gl.HasValue && gv.HasValue) ? $"{gl} - {gv}" : "—";
                                c.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(cd =>
                                    {
                                        cd.ConstantColumn(75);
                                        cd.RelativeColumn();
                                    });
                                    t.Cell().PaddingVertical(0.5f)
                                        .Text(label).FontSize(6f).FontColor("#555555");
                                    t.Cell().Border(0.5f).BorderColor("#CCCCCC")
                                        .Background(gl.HasValue ? "#E8F5E9" : "#FFFFFF")
                                        .AlignCenter().Padding(0.5f)
                                        .Text(valor).FontSize(6f);
                                });
                            }

                            FilaMarcador(col, "Semifinal 1 (P#101)", pf.GolesLocalSemi1, pf.GolesVisitanteSemi1);
                            FilaMarcador(col, "Semifinal 2 (P#102)", pf.GolesLocalSemi2, pf.GolesVisitanteSemi2);
                            FilaMarcador(col, "Gran Final  (P#104)", pf.GolesLocalGranFinal, pf.GolesVisitanteGranFinal);
                        }
                    });
                });

                // ── Footer ──────────────────────────────────────────────────────
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text("Vinccler C.A. — Quiniela FIFA 2026")
                        .FontSize(6.5f).FontColor("#666666");
                    row.ConstantItem(180).AlignRight()
                        .Text($"Planilla {planilla.Codigo} — {planilla.User?.FullName ?? ""}")
                        .FontSize(6.5f).FontColor("#666666");
                });
            });
        }).GeneratePdf();
    }
}