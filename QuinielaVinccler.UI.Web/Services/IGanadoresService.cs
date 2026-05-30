namespace QuinielaVinccler.UI.Web.Services;

public sealed record CalculoPremios(
    decimal MontoRecaudado,
    decimal Premio1ro,      // 2000 + 50% recaudado
    decimal Premio2do,      // 1000 + 30% recaudado
    decimal Premio3ro,      // 500 + 20% recaudado
    decimal PremioMenor     // 2000 fijo
);

public sealed record GanadoresResultado(
    CalculoPremios Premios,
    List<GanadorDto> Ganadores
);

public interface IGanadoresService
{
    Task<GanadoresResultado> CalcularGanadoresAsync();
}
