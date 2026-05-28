namespace QuinielaVinccler.UI.Web.Components.Pages;

public partial class MisPlanillas : ComponentBase
{
    [Inject] private IPlanillaService PlanillaSvc { get; set; } = null!;
    [Inject] private IConfiguracionService ConfigSvc { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState> AuthState { get; set; } = null!;

    private List<Planilla> _planillas = [];
    private string _codigoInput = "";
    private string? _errorVinculacion;
    private bool _exitoVinculacion;
    private bool _vinculando;
    private bool _cargando = true;
    private bool _quinielaCerrada;
    private int _userId;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        var userIdStr = state.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdStr, out var id))
            _userId = id;

        _quinielaCerrada = await ConfigSvc.QuinielaCerradaAsync();
        _planillas = await PlanillaSvc.GetPlanillasByUserAsync(_userId);
        _cargando = false;
    }

    private async Task HandleKeyDownVincular(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !_vinculando) await VincularPlanilla();
    }

    private async Task VincularPlanilla()
    {
        _errorVinculacion = null;
        _exitoVinculacion = false;

        if (string.IsNullOrWhiteSpace(_codigoInput)) return;

        _vinculando = true;

        try
        {
            var (exito, error) = await PlanillaSvc.VincularAsync(_codigoInput, _userId);

            if (exito)
            {
                _exitoVinculacion = true;
                _codigoInput = "";
                _planillas = await PlanillaSvc.GetPlanillasByUserAsync(_userId);
            }
            else
            {
                _errorVinculacion = error;
            }
        }
        catch
        {
            _errorVinculacion = "Ocurrió un error inesperado. Intenta de nuevo.";
        }
        finally
        {
            _vinculando = false;
        }
    }

    private static MudBlazor.Color GetColorEstado(EstadoPlanilla estado) => estado switch
    {
        EstadoPlanilla.Asignada => MudBlazor.Color.Info,
        EstadoPlanilla.EnProgreso => MudBlazor.Color.Warning,
        EstadoPlanilla.Completa => MudBlazor.Color.Success,
        EstadoPlanilla.Cerrada => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default,
    };

    private static string GetLabelEstado(EstadoPlanilla estado) => estado switch
    {
        EstadoPlanilla.SinAsignar => "Sin asignar",
        EstadoPlanilla.Asignada => "Asignada",
        EstadoPlanilla.EnProgreso => "En progreso",
        EstadoPlanilla.Completa => "Completa",
        EstadoPlanilla.Cerrada => "Cerrada",
        _ => estado.ToString(),
    };
}