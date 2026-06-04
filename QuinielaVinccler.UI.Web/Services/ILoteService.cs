namespace QuinielaVinccler.UI.Web.Services;

public interface ILoteService
{
    Task<Lote> CreateAsync(int cantidad);
    Task<List<Lote>> GetAsync();
    Task<(bool Exito, string Mensaje)> EliminarAsync(int loteId);
    Task<Dictionary<int, int>> GetProgresoPlanillasAsync(List<int> planillaIds);
}