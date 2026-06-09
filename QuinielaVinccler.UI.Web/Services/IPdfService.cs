namespace QuinielaVinccler.UI.Web.Services;

public interface IPdfService
{
    byte[] GenerarLotePdf(Lote lote);
    byte[] GenerarReportePlanillasPdf(
    List<(Planilla Planilla, string NombreUsuario, int CamposCompletos)> datos,
    string filtroLabel);
    byte[] GenerarPlanillaPdf(Planilla planilla, Dictionary<int, string> equiposById);
}
