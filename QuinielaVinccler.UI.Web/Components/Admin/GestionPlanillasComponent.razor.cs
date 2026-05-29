namespace QuinielaVinccler.UI.Web.Components.Admin;

using Color = MudBlazor.Color;

public partial class GestionPlanillasComponent : ComponentBase
{
    [Inject] private IPlanillaService PlanillaSvc { get; set; } = null!;

    // ── Estado de búsqueda ────────────────────────────────────────────────────
    private string _termino = "";
    private bool _buscando;
    private List<PlanillaAdminDto>? _resultados;

    // ── Estado de desvinculación ──────────────────────────────────────────────
    private bool _showConfirm;
    private bool _desvinculando;
    private string? _errorDesvincular;
    private PlanillaAdminDto? _seleccionada;

    // ── Búsqueda ──────────────────────────────────────────────────────────────
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !_buscando) await Buscar();
    }

    private async Task Buscar()
    {
        if (string.IsNullOrWhiteSpace(_termino)) return;

        _buscando = true;
        _resultados = null;

        try
        {
            _resultados = await PlanillaSvc.BuscarPlanillasAsync(_termino);
        }
        finally
        {
            _buscando = false;
        }
    }

    // ── Desvinculación ────────────────────────────────────────────────────────
    private void AbrirConfirmacion(PlanillaAdminDto planilla)
    {
        _seleccionada = planilla;
        _errorDesvincular = null;
        _showConfirm = true;
    }

    private void CerrarConfirmacion()
    {
        _showConfirm = false;
        _seleccionada = null;
        _errorDesvincular = null;
    }

    private async Task EjecutarDesvincular()
    {
        if (_seleccionada is null) return;

        _desvinculando = true;
        _errorDesvincular = null;

        try
        {
            var (exito, error) = await PlanillaSvc.DesvincularAdminAsync(_seleccionada.Id);

            if (exito)
            {
                _showConfirm = false;
                // Refrescar resultados removiendo la planilla desvinculada
                _resultados?.RemoveAll(p => p.Id == _seleccionada.Id);
                _seleccionada = null;
            }
            else
            {
                _errorDesvincular = error;
            }
        }
        catch
        {
            _errorDesvincular = "Ocurrió un error inesperado. Intenta de nuevo.";
        }
        finally
        {
            _desvinculando = false;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Color GetColorEstado(EstadoPlanilla estado) => estado switch
    {
        EstadoPlanilla.Asignada   => Color.Info,
        EstadoPlanilla.EnProgreso => Color.Warning,
        EstadoPlanilla.Completa   => Color.Success,
        EstadoPlanilla.Cerrada    => Color.Error,
        _                         => Color.Default,
    };

    private static string GetLabelEstado(EstadoPlanilla estado) => estado switch
    {
        EstadoPlanilla.SinAsignar => "Sin asignar",
        EstadoPlanilla.Asignada   => "Asignada",
        EstadoPlanilla.EnProgreso => "En progreso",
        EstadoPlanilla.Completa   => "Completa",
        EstadoPlanilla.Cerrada    => "Cerrada",
        _                         => estado.ToString(),
    };
}
