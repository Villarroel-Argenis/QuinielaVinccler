namespace QuinielaVinccler.UI.Web.Services;

public interface IConfiguracionService
{
    Task<string?> GetAsync(string clave);
    Task SetAsync(string clave, string valor);
    Task<bool> PuedeEditarPlanillaAsync(int planillaId);
    Task<bool> QuinielaCerradaAsync();
}