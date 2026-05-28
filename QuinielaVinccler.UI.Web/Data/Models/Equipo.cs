namespace QuinielaVinccler.UI.Web.Data.Models;

public class Equipo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string CodigoIso { get; set; } = "";   // "mx", "br", "gb-sct"
    public string Grupo { get; set; } = "";        // "A" .. "L"

    public string FlagUrl => string.IsNullOrEmpty(CodigoIso)
        ? ""
        : $"https://flagcdn.com/24x18/{CodigoIso}.png";
}