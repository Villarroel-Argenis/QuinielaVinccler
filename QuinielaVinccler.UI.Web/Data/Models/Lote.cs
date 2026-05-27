namespace QuinielaVinccler.UI.Web.Data.Models;

public class Lote
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";        // L-XXXXXXXX
    public int Cantidad { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Planilla> Planillas { get; set; } = [];
}
