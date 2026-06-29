using System.ComponentModel.DataAnnotations.Schema;

namespace QuinielaVinccler.UI.Web.Data.Models;

public class ResultadoFinal
{
    public int Id { get; set; }

    // ── Posiciones finales ────────────────────────────────────────────────────
    public int? CampeonEquipoId { get; set; }
    public Equipo? Campeon { get; set; }

    public int? SegundoLugarEquipoId { get; set; }
    public Equipo? SegundoLugar { get; set; }

    public int? TercerLugarEquipoId { get; set; }
    public Equipo? TercerLugar { get; set; }

    public int? CuartoLugarEquipoId { get; set; }
    public Equipo? CuartoLugar { get; set; }

    // ── Extras fase eliminatoria (múltiples equipos separados por coma) ───────
    public string? MasGoleadorIds { get; set; }   // ej. "3,17,29"
    public string? MasGoleadoIds { get; set; }
    public string? MenosGoleadoIds { get; set; }

    // ── Helpers para parsear los Ids ──────────────────────────────────────────
    [NotMapped]
    public List<int> MasGoleadorIdList =>
        ParseIds(MasGoleadorIds);

    [NotMapped]
    public List<int> MasGoleadoIdList =>
        ParseIds(MasGoleadoIds);

    [NotMapped]
    public List<int> MenosGoleadoIdList =>
        ParseIds(MenosGoleadoIds);

    private static List<int> ParseIds(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? []
            : [.. valor.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                   .Where(id => id > 0)];
}