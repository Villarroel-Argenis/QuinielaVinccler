
namespace QuinielaVinccler.UI.Web.Components.Admin;

public partial class GanadoresComponent : ComponentBase
{
    [Inject] private IGanadoresService GanadoresSvc { get; set; } = null!;
    [Inject] private IConfiguracionService ConfigSvc { get; set; } = null!;

    private bool _cargando = true;
    private GanadoresResultado? _resultado;
    private decimal _montoInput;
    private bool _guardandoMonto;

    private string _mensaje = "";
    private Severity _mensajeSeveridad = Severity.Info;

    private readonly CultureInfo _cultura = new("en-US"); // formato $1,234

    private bool HayEmpates => _resultado?.Ganadores.Any(g => g.CantidadEmpatados > 1) ?? false;

    protected override async Task OnInitializedAsync()
    {
        await CargarMontoActualAsync();
        await Recargar();
    }

    private async Task CargarMontoActualAsync()
    {
        var str = await ConfigSvc.GetAsync(ConfiguracionKeys.MontoRecaudado);
        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
            _montoInput = v;
    }

    private async Task Recargar()
    {
        _cargando = true;
        StateHasChanged();
        try
        {
            _resultado = await GanadoresSvc.CalcularGanadoresAsync();
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _cargando = false;
        }
    }

    private async Task GuardarMonto()
    {
        if (_montoInput < 0)
        {
            MostrarMensaje("El monto no puede ser negativo.", Severity.Error);
            return;
        }

        _guardandoMonto = true;
        try
        {
            await ConfigSvc.SetAsync(
                ConfiguracionKeys.MontoRecaudado,
                _montoInput.ToString(CultureInfo.InvariantCulture));

            await Recargar();
            MostrarMensaje("Monto guardado. Premios recalculados.", Severity.Success);
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _guardandoMonto = false;
        }
    }

    private static string LabelTipo(TipoPremio tipo) => tipo switch
    {
        TipoPremio.Primero      => "🥇 1er Premio",
        TipoPremio.Segundo      => "🥈 2do Premio",
        TipoPremio.Tercero      => "🥉 3er Premio",
        TipoPremio.MenorPuntaje => "🙃 Menor puntaje",
        _                       => tipo.ToString()
    };

    private void MostrarMensaje(string texto, Severity severidad)
    {
        _mensaje = texto;
        _mensajeSeveridad = severidad;
    }
}
